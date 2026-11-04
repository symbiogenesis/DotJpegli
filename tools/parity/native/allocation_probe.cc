// Linux/GNU linker allocation observation. The upstream source is unmodified.
// Only successful requests during an explicitly enabled encode are counted.
// Real allocator calls are resolved directly, so nested libc implementation
// details do not count the same request twice.
#include "allocation_probe.h"

#include <cstdlib>
#include <limits>
#include <new>
#include <stdexcept>

namespace {
thread_local bool enabled = false;
thread_local bool overflow = false;
thread_local allocation_probe::Sample sample{};

void Count(void* result, size_t bytes) {
    if (!enabled || result == nullptr) return;
    if (bytes > std::numeric_limits<uint64_t>::max() - sample.bytes ||
            sample.calls == std::numeric_limits<uint64_t>::max()) {
        overflow = true;
        return;
    }
    sample.bytes += bytes;
    ++sample.calls;
}
}  // namespace

extern "C" {
void* __real_malloc(size_t);
void* __real_calloc(size_t, size_t);
void* __real_realloc(void*, size_t);
void* __real_aligned_alloc(size_t, size_t);
int __real_posix_memalign(void**, size_t, size_t);
void* __real_memalign(size_t, size_t);

void* __wrap_malloc(size_t bytes) {
    void* result = __real_malloc(bytes);
    Count(result, bytes);
    return result;
}
void* __wrap_calloc(size_t count, size_t bytes) {
    void* result = __real_calloc(count, bytes);
    Count(result, count * bytes);
    return result;
}
void* __wrap_realloc(void* previous, size_t bytes) {
    void* result = __real_realloc(previous, bytes);
    Count(result, bytes);
    return result;
}
void* __wrap_aligned_alloc(size_t alignment, size_t bytes) {
    void* result = __real_aligned_alloc(alignment, bytes);
    Count(result, bytes);
    return result;
}
int __wrap_posix_memalign(void** destination, size_t alignment, size_t bytes) {
    int error = __real_posix_memalign(destination, alignment, bytes);
    if (error == 0) Count(*destination, bytes);
    return error;
}
void* __wrap_memalign(size_t alignment, size_t bytes) {
    void* result = __real_memalign(alignment, bytes);
    Count(result, bytes);
    return result;
}
}  // extern "C"

// libstdc++ is dynamic. Replace its allocation entry points so C++ containers
// in the static jpegli/Highway libraries reach the same wrapped allocator.
void* operator new(size_t bytes) {
    if (void* result = __wrap_malloc(bytes == 0 ? 1 : bytes)) return result;
    throw std::bad_alloc();
}
void* operator new[](size_t bytes) { return ::operator new(bytes); }
void* operator new(size_t bytes, const std::nothrow_t&) noexcept {
    try { return ::operator new(bytes); } catch (...) { return nullptr; }
}
void* operator new[](size_t bytes, const std::nothrow_t& tag) noexcept { return ::operator new(bytes, tag); }
void* operator new(size_t bytes, std::align_val_t alignment) {
    void* result = nullptr;
    if (__wrap_posix_memalign(&result, static_cast<size_t>(alignment), bytes == 0 ? 1 : bytes) == 0) return result;
    throw std::bad_alloc();
}
void* operator new[](size_t bytes, std::align_val_t alignment) { return ::operator new(bytes, alignment); }
void* operator new(size_t bytes, std::align_val_t alignment, const std::nothrow_t&) noexcept {
    try { return ::operator new(bytes, alignment); } catch (...) { return nullptr; }
}
void* operator new[](size_t bytes, std::align_val_t alignment, const std::nothrow_t& tag) noexcept {
    return ::operator new(bytes, alignment, tag);
}
void operator delete(void* pointer) noexcept { std::free(pointer); }
void operator delete[](void* pointer) noexcept { std::free(pointer); }
void operator delete(void* pointer, size_t) noexcept { std::free(pointer); }
void operator delete[](void* pointer, size_t) noexcept { std::free(pointer); }
void operator delete(void* pointer, const std::nothrow_t&) noexcept { std::free(pointer); }
void operator delete[](void* pointer, const std::nothrow_t&) noexcept { std::free(pointer); }
void operator delete(void* pointer, std::align_val_t) noexcept { std::free(pointer); }
void operator delete[](void* pointer, std::align_val_t) noexcept { std::free(pointer); }
void operator delete(void* pointer, size_t, std::align_val_t) noexcept { std::free(pointer); }
void operator delete[](void* pointer, size_t, std::align_val_t) noexcept { std::free(pointer); }
void operator delete(void* pointer, std::align_val_t, const std::nothrow_t&) noexcept { std::free(pointer); }
void operator delete[](void* pointer, std::align_val_t, const std::nothrow_t&) noexcept { std::free(pointer); }

namespace allocation_probe {
void Begin() {
    sample = {};
    overflow = false;
    enabled = true;
}
Sample End() {
    enabled = false;
    if (overflow) throw std::runtime_error("Native allocation counter overflow");
    return sample;
}
}  // namespace allocation_probe
