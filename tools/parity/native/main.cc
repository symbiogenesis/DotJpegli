// Test-only reference wrapper. All JPEG coding is performed by unmodified
// google/jpegli at the revision checked by CMakeLists.txt.
#include <chrono>
#include <cmath>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <limits>
#include <memory>
#include <stdexcept>
#include <string>
#include <vector>

#include "lib/jpegli/decode.h"
#include "lib/jpegli/encode.h"

namespace {
using File = std::unique_ptr<FILE, int (*)(FILE*)>;

#define STRINGIFY_DETAIL(value) #value
#define STRINGIFY(value) STRINGIFY_DETAIL(value)

const char* CompilerDescription() {
#if defined(__clang__)
  return "Clang " STRINGIFY(__clang_major__) "." STRINGIFY(__clang_minor__) "." STRINGIFY(__clang_patchlevel__);
#elif defined(_MSC_VER)
  return "MSVC " STRINGIFY(_MSC_FULL_VER);
#elif defined(__GNUC__)
  return "GCC " STRINGIFY(__GNUC__) "." STRINGIFY(__GNUC_MINOR__) "." STRINGIFY(__GNUC_PATCHLEVEL__);
#else
  return "unknown";
#endif
}

const char* OperatingSystem() {
#if defined(_WIN32)
  return "Windows";
#elif defined(__APPLE__)
  return "macOS";
#elif defined(__linux__)
  return "Linux";
#else
  return "unknown";
#endif
}

const char* Architecture() {
#if defined(_M_X64) || defined(__x86_64__)
  return "x64";
#elif defined(_M_ARM64) || defined(__aarch64__)
  return "arm64";
#elif defined(_M_IX86) || defined(__i386__)
  return "x86";
#else
  return "unknown";
#endif
}

File OpenFile(const char* path, const char* mode) {
  File file(std::fopen(path, mode), std::fclose);
  if (!file) throw std::runtime_error(std::string("Cannot open ") + path);
  return file;
}

int ParseInt(const std::string& value) {
  size_t used = 0;
  int result = std::stoi(value, &used);
  if (used != value.size()) throw std::runtime_error("Invalid integer: " + value);
  return result;
}

bool ParseBool(const char* value) {
  std::string text(value);
  if (text == "1") return true;
  if (text == "0") return false;
  throw std::runtime_error("Boolean arguments must be 0 or 1");
}

bool IsSpace(int c) {
  return c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\f' || c == '\v';
}

// Consume only the token's one terminating whitespace (or CRLF), so a first
// raster byte that happens to be whitespace is never skipped.
std::string PpmToken(std::istream& input) {
  int c;
  while (true) {
    c = input.get();
    if (c == '#') {
      input.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
    } else if (!IsSpace(c)) {
      break;
    }
  }
  if (c == EOF) throw std::runtime_error("Truncated PPM header");
  std::string token;
  do {
    token.push_back(static_cast<char>(c));
    c = input.get();
  } while (c != EOF && !IsSpace(c));
  if (c == EOF) throw std::runtime_error("Missing PPM header delimiter");
  if (c == '\r' && input.peek() == '\n') input.get();
  return token;
}

struct Image {
  int width;
  int height;
  std::vector<unsigned char> pixels;
};

Image ReadPpm(const char* path) {
  std::ifstream input(path, std::ios::binary);
  if (!input) throw std::runtime_error(std::string("Cannot open ") + path);
  if (PpmToken(input) != "P6") throw std::runtime_error("Expected binary RGB P6 PPM");
  Image image;
  image.width = ParseInt(PpmToken(input));
  image.height = ParseInt(PpmToken(input));
  if (image.width <= 0 || image.height <= 0 || image.width > 65535 || image.height > 65535) {
    throw std::runtime_error("PPM dimensions outside JPEG range");
  }
  if (ParseInt(PpmToken(input)) != 255) throw std::runtime_error("Expected 8-bit PPM maxval 255");
  const size_t count = static_cast<size_t>(image.width) * image.height * 3;
  if (count > static_cast<size_t>(std::numeric_limits<std::streamsize>::max())) {
    throw std::runtime_error("PPM too large");
  }
  image.pixels.resize(count);
  input.read(reinterpret_cast<char*>(image.pixels.data()), static_cast<std::streamsize>(count));
  if (static_cast<size_t>(input.gcount()) != count) throw std::runtime_error("Truncated PPM raster");
  if (input.peek() != EOF) throw std::runtime_error("Unexpected bytes after PPM raster");
  return image;
}

struct EncodeSettings {
  float distance;
  std::string sampling;
  int progressive;
  bool optimize = true;
  bool adaptive = true;
};

EncodeSettings ParseSettings(const char* distance_arg, const char* sampling_arg,
                             const char* progressive_arg) {
  size_t distance_used = 0;
  const std::string distance_text(distance_arg);
  const float distance = std::stof(distance_text, &distance_used);
  if (distance_used != distance_text.size() || !std::isfinite(distance) || distance <= 0) {
    throw std::runtime_error("Distance must be finite and positive");
  }
  const std::string sampling(sampling_arg);
  if (sampling != "444" && sampling != "420") throw std::runtime_error("Sampling must be 444 or 420");
  const int progressive = ParseInt(progressive_arg);
  if (progressive < 0) throw std::runtime_error("Progressive level must be nonnegative");
  return {distance, sampling, progressive};
}

// Shared by file encoding and memory timing so their JPEG settings cannot drift.
void ConfigureEncoder(jpeg_compress_struct* cinfo, const Image& image,
                      const EncodeSettings& settings) {
  cinfo->image_width = image.width;
  cinfo->image_height = image.height;
  cinfo->input_components = 3;
  cinfo->in_color_space = JCS_RGB;
  jpegli_set_defaults(cinfo);
  jpegli_set_colorspace(cinfo, JCS_YCbCr);
  jpegli_set_input_format(cinfo, JPEGLI_TYPE_UINT8, JPEGLI_NATIVE_ENDIAN);
  cinfo->comp_info[0].h_samp_factor = settings.sampling == "420" ? 2 : 1;
  cinfo->comp_info[0].v_samp_factor = settings.sampling == "420" ? 2 : 1;
  for (int component = 1; component < 3; ++component) {
    cinfo->comp_info[component].h_samp_factor = 1;
    cinfo->comp_info[component].v_samp_factor = 1;
  }
  jpegli_set_distance(cinfo, settings.distance, TRUE);
  jpegli_set_progressive_level(cinfo, settings.progressive);
  jpegli_enable_adaptive_quantization(cinfo, settings.adaptive ? TRUE : FALSE);
  cinfo->optimize_coding = settings.optimize ? TRUE : FALSE;
  cinfo->smoothing_factor = 0;
  cinfo->restart_interval = 0;
  cinfo->restart_in_rows = 0;
  // Match the managed encoder's mandatory YCbCr JFIF marker, including size.
  cinfo->write_JFIF_header = TRUE;
  cinfo->JFIF_major_version = 1;
  cinfo->JFIF_minor_version = 1;
  cinfo->density_unit = 0;
  cinfo->X_density = 1;
  cinfo->Y_density = 1;
}

void WriteImage(jpeg_compress_struct* cinfo, const Image& image) {
  jpegli_start_compress(cinfo, TRUE);
  while (cinfo->next_scanline < cinfo->image_height) {
    JSAMPROW row = const_cast<unsigned char*>(image.pixels.data() +
        static_cast<size_t>(cinfo->next_scanline) * image.width * 3);
    if (jpegli_write_scanlines(cinfo, &row, 1) != 1) throw std::runtime_error("Scanline write suspended");
  }
  jpegli_finish_compress(cinfo);
}

void Encode(char** argv) {
  const Image image = ReadPpm(argv[2]);
  EncodeSettings settings = ParseSettings(argv[4], argv[5], argv[6]);
  settings.optimize = ParseBool(argv[7]);
  settings.adaptive = ParseBool(argv[8]);
  File output = OpenFile(argv[3], "wb");
  jpeg_compress_struct cinfo{};
  jpeg_error_mgr error{};
  cinfo.err = jpegli_std_error(&error);
  jpegli_create_compress(&cinfo);
  jpegli_stdio_dest(&cinfo, output.get());
  ConfigureEncoder(&cinfo, image, settings);
  WriteImage(&cinfo, image);
  jpegli_destroy_compress(&cinfo);
  if (error.num_warnings != 0) throw std::runtime_error("JPEG encoder reported warnings");
  if (std::fclose(output.release()) != 0) throw std::runtime_error("Failed closing JPEG output");
}

struct FreeBytes {
  void operator()(unsigned char* bytes) const { std::free(bytes); }
};

struct MemoryJpeg {
  std::unique_ptr<unsigned char, FreeBytes> bytes;
  unsigned long size;
};

// Entire encoder lifecycle, including creation, configuration, destination
// allocation/growth, input, finish and destruction. The returned JPEG allocation
// belongs to the caller; consuming and freeing it happens outside the timer.
MemoryJpeg EncodeMemory(const Image& image, const EncodeSettings& settings) {
  jpeg_compress_struct cinfo{};
  jpeg_error_mgr error{};
  cinfo.err = jpegli_std_error(&error);
  unsigned char* bytes = nullptr;
  unsigned long size = 0;
  jpegli_create_compress(&cinfo);
  try {
    jpegli_mem_dest(&cinfo, &bytes, &size);
    ConfigureEncoder(&cinfo, image, settings);
    WriteImage(&cinfo, image);
    jpegli_destroy_compress(&cinfo);
  } catch (...) {
    jpegli_destroy_compress(&cinfo);
    std::free(bytes);
    throw;
  }
  MemoryJpeg result{std::unique_ptr<unsigned char, FreeBytes>(bytes), size};
  if (error.num_warnings != 0) throw std::runtime_error("JPEG encoder reported warnings");
  return result;
}

void VerifyJpeg(const MemoryJpeg& output, const std::vector<unsigned char>& expected) {
  if (output.size != expected.size() ||
      std::memcmp(output.bytes.get(), expected.data(), expected.size()) != 0) {
    throw std::runtime_error("Encoder output changed between benchmark iterations");
  }
}

void Benchmark(int argc, char** argv) {
  const Image image = ReadPpm(argv[2]);
  const EncodeSettings settings = ParseSettings(argv[3], argv[4], argv[5]);
  const int warmups = ParseInt(argv[6]);
  const int iterations = ParseInt(argv[7]);
  const int repeats = ParseInt(argv[8]);
  if (warmups < 5 || iterations < 1 || repeats < 1) {
    throw std::runtime_error("Benchmark requires at least 5 warmups and positive iterations/repeats");
  }

  // Retain a complete reference JPEG and compare every later result outside its
  // timed region. This also makes each output observable without console I/O.
  std::vector<unsigned char> expected;
  for (int warmup = 0; warmup < warmups; ++warmup) {
    MemoryJpeg output = EncodeMemory(image, settings);
    if (warmup == 0) {
      if (output.size == 0) throw std::runtime_error("Encoder returned an empty JPEG");
      expected.assign(output.bytes.get(), output.bytes.get() + output.size);
    } else {
      VerifyJpeg(output, expected);
    }
  }

  using Clock = std::chrono::steady_clock;
  std::vector<double> samples;
  samples.reserve(repeats);
  for (int repeat = 0; repeat < repeats; ++repeat) {
    std::chrono::nanoseconds elapsed{0};
    for (int iteration = 0; iteration < iterations; ++iteration) {
      const auto start = Clock::now();
      MemoryJpeg output = EncodeMemory(image, settings);
      const auto stop = Clock::now();
      elapsed += std::chrono::duration_cast<std::chrono::nanoseconds>(stop - start);
      VerifyJpeg(output, expected);
    }
    samples.push_back(static_cast<double>(elapsed.count()) / iterations);
  }

  if (argc == 10) {
    File output = OpenFile(argv[9], "wb");
    if (std::fwrite(expected.data(), 1, expected.size(), output.get()) != expected.size()) {
      throw std::runtime_error("Failed writing benchmark JPEG");
    }
    if (std::fclose(output.release()) != 0) throw std::runtime_error("Failed closing benchmark JPEG");
  }

  std::cout << std::setprecision(17)
            << "{\"encoder\":\"native-jpegli\",\"upstreamPin\":\"" << JPEGLI_PIN
            << "\",\"width\":" << image.width << ",\"height\":" << image.height
            << ",\"distance\":" << settings.distance
            << ",\"sampling\":\"" << settings.sampling
            << "\",\"progressiveLevel\":" << settings.progressive
            << ",\"optimizeHuffman\":true,\"adaptiveQuantization\":true,\"threads\":1"
            << ",\"warmups\":" << warmups << ",\"iterations\":" << iterations
            << ",\"repeats\":" << repeats << ",\"bytes\":" << expected.size()
            << ",\"deterministic\":true"
            << ",\"timingScope\":\"create/configure/mem_dest/write/finish/destroy; returned-buffer free and output verification excluded; per-encode steady_clock intervals summed\""
            << ",\"environment\":{\"os\":\"" << OperatingSystem()
            << "\",\"architecture\":\"" << Architecture()
            << "\",\"compiler\":\"" << CompilerDescription()
            << "\",\"runtime\":\"native\",\"clock\":\"steady_clock\"}"
            << ",\"samplesNsPerEncode\":[";
  for (size_t sample = 0; sample < samples.size(); ++sample) {
    if (sample != 0) std::cout << ',';
    std::cout << samples[sample];
  }
  std::cout << "]}\n";
}

void Decode(char** argv) {
  File input = OpenFile(argv[2], "rb");
  File output = OpenFile(argv[3], "wb");
  jpeg_decompress_struct cinfo{};
  jpeg_error_mgr error{};
  cinfo.err = jpegli_std_error(&error);
  jpegli_create_decompress(&cinfo);
  jpegli_stdio_src(&cinfo, input.get());
  if (jpegli_read_header(&cinfo, TRUE) != JPEG_HEADER_OK) throw std::runtime_error("Invalid JPEG header");
  cinfo.out_color_space = JCS_RGB;
  cinfo.scale_num = 1;
  cinfo.scale_denom = 1;
  cinfo.do_fancy_upsampling = TRUE;
  cinfo.dct_method = JDCT_ISLOW;
  jpegli_set_output_format(&cinfo, JPEGLI_TYPE_UINT8, JPEGLI_NATIVE_ENDIAN);
  if (!jpegli_start_decompress(&cinfo) || cinfo.output_components != 3) {
    throw std::runtime_error("Cannot decode to RGB");
  }
  if (std::fprintf(output.get(), "P6\n%u %u\n255\n", cinfo.output_width, cinfo.output_height) < 0) {
    throw std::runtime_error("Failed writing PPM header");
  }
  std::vector<unsigned char> row(static_cast<size_t>(cinfo.output_width) * 3);
  while (cinfo.output_scanline < cinfo.output_height) {
    JSAMPROW rows[] = {row.data()};
    if (jpegli_read_scanlines(&cinfo, rows, 1) != 1) throw std::runtime_error("Scanline read suspended");
    if (std::fwrite(row.data(), 1, row.size(), output.get()) != row.size()) {
      throw std::runtime_error("Failed writing PPM raster");
    }
  }
  if (!jpegli_finish_decompress(&cinfo)) throw std::runtime_error("Incomplete JPEG decode");
  jpegli_destroy_decompress(&cinfo);
  if (error.num_warnings != 0) throw std::runtime_error("JPEG decoder reported warnings");
  if (std::fclose(output.release()) != 0) throw std::runtime_error("Failed closing PPM output");
}
}  // namespace

int main(int argc, char** argv) {
  try {
    if (argc == 2 && std::string(argv[1]) == "version") {
      std::cout << JPEGLI_PIN << '\n';
    } else if (argc == 9 && std::string(argv[1]) == "encode") {
      Encode(argv);
    } else if (argc == 4 && std::string(argv[1]) == "decode") {
      Decode(argv);
    } else if ((argc == 9 || argc == 10) && std::string(argv[1]) == "benchmark") {
      Benchmark(argc, argv);
    } else {
      std::cerr << "Usage: jpegli-parity-native encode INPUT.ppm OUTPUT.jpg DISTANCE 444|420 LEVEL OPTIMIZE_0|1 AQ_0|1\n"
                   "       jpegli-parity-native decode INPUT.jpg OUTPUT.ppm\n"
                   "       jpegli-parity-native benchmark INPUT.ppm DISTANCE 444|420 LEVEL WARMUPS ITERATIONS REPEATS [OUTPUT.jpg]\n"
                   "       jpegli-parity-native version\n";
      return 2;
    }
    return 0;
  } catch (const std::exception& error) {
    std::cerr << error.what() << '\n';
    return 1;
  }
}
