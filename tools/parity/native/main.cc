// Test-only reference wrapper. All JPEG coding is performed by unmodified
// google/jpegli at the revision checked by CMakeLists.txt.
#include <cmath>
#include <cstdio>
#include <fstream>
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

void Encode(char** argv) {
  const Image image = ReadPpm(argv[2]);
  size_t distance_used = 0;
  const std::string distance_text(argv[4]);
  const float distance = std::stof(distance_text, &distance_used);
  if (distance_used != distance_text.size() || !std::isfinite(distance) || distance <= 0) {
    throw std::runtime_error("Distance must be finite and positive");
  }
  const std::string sampling(argv[5]);
  if (sampling != "444" && sampling != "420") throw std::runtime_error("Sampling must be 444 or 420");
  const int progressive = ParseInt(argv[6]);
  if (progressive < 0) throw std::runtime_error("Progressive level must be nonnegative");
  const bool optimize = ParseBool(argv[7]);
  const bool adaptive = ParseBool(argv[8]);

  File output = OpenFile(argv[3], "wb");
  jpeg_compress_struct cinfo{};
  jpeg_error_mgr error{};
  cinfo.err = jpegli_std_error(&error);
  jpegli_create_compress(&cinfo);
  jpegli_stdio_dest(&cinfo, output.get());
  cinfo.image_width = image.width;
  cinfo.image_height = image.height;
  cinfo.input_components = 3;
  cinfo.in_color_space = JCS_RGB;
  jpegli_set_defaults(&cinfo);
  jpegli_set_colorspace(&cinfo, JCS_YCbCr);
  jpegli_set_input_format(&cinfo, JPEGLI_TYPE_UINT8, JPEGLI_NATIVE_ENDIAN);
  cinfo.comp_info[0].h_samp_factor = sampling == "420" ? 2 : 1;
  cinfo.comp_info[0].v_samp_factor = sampling == "420" ? 2 : 1;
  for (int component = 1; component < 3; ++component) {
    cinfo.comp_info[component].h_samp_factor = 1;
    cinfo.comp_info[component].v_samp_factor = 1;
  }
  jpegli_set_distance(&cinfo, distance, TRUE);
  jpegli_set_progressive_level(&cinfo, progressive);
  jpegli_enable_adaptive_quantization(&cinfo, adaptive ? TRUE : FALSE);
  cinfo.optimize_coding = optimize ? TRUE : FALSE;
  cinfo.smoothing_factor = 0;
  cinfo.restart_interval = 0;
  cinfo.restart_in_rows = 0;
  // Match the managed encoder's mandatory YCbCr JFIF marker, including size.
  cinfo.write_JFIF_header = TRUE;
  cinfo.JFIF_major_version = 1;
  cinfo.JFIF_minor_version = 1;
  cinfo.density_unit = 0;
  cinfo.X_density = 1;
  cinfo.Y_density = 1;
  jpegli_start_compress(&cinfo, TRUE);
  while (cinfo.next_scanline < cinfo.image_height) {
    JSAMPROW row = const_cast<unsigned char*>(image.pixels.data() +
        static_cast<size_t>(cinfo.next_scanline) * image.width * 3);
    if (jpegli_write_scanlines(&cinfo, &row, 1) != 1) throw std::runtime_error("Scanline write suspended");
  }
  jpegli_finish_compress(&cinfo);
  jpegli_destroy_compress(&cinfo);
  if (error.num_warnings != 0) throw std::runtime_error("JPEG encoder reported warnings");
  if (std::fclose(output.release()) != 0) throw std::runtime_error("Failed closing JPEG output");
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
    } else {
      std::cerr << "Usage: jpegli-parity-native encode INPUT.ppm OUTPUT.jpg DISTANCE 444|420 LEVEL OPTIMIZE_0|1 AQ_0|1\n"
                   "       jpegli-parity-native decode INPUT.jpg OUTPUT.ppm\n"
                   "       jpegli-parity-native version\n";
      return 2;
    }
    return 0;
  } catch (const std::exception& error) {
    std::cerr << error.what() << '\n';
    return 1;
  }
}
