// A separate translation unit exercises actual allocator symbol references,
// rather than calling the wrappers directly. Volatile function pointers keep
// the compiler from folding or eliding these synthetic allocation requests.
#include "allocation_probe.h"

#include <cstdlib>
#include <malloc.h>
#include <new>
#include <stdexcept>

namespace allocation_probe {
void SelfTest() {
    void* (*volatile allocate)(size_t) = std::malloc;
    void* (*volatile zero_allocate)(size_t, size_t) = std::calloc;
    void* (*volatile resize)(void*, size_t) = std::realloc;
    void* (*volatile aligned)(size_t, size_t) = std::aligned_alloc;
    int (*volatile posix_aligned)(void**, size_t, size_t) = ::posix_memalign;
    void* (*volatile legacy_aligned)(size_t, size_t) = ::memalign;
    void* (*volatile allocate_new)(size_t) = ::operator new;
    void* (*volatile allocate_array)(size_t) = ::operator new[];
    void* (*volatile aligned_new)(size_t, std::align_val_t) = ::operator new;
    Begin();
    void* a = allocate(20);
    void* b = zero_allocate(6, 7);
    a = resize(a, 79);
    void* c = aligned(64, 128);
    void* d = nullptr;
    const int error = posix_aligned(&d, 64, 256);
    void* e = legacy_aligned(64, 512);
    void* f = allocate_new(25);
    void* g = allocate_array(37);
    void* h = aligned_new(64, std::align_val_t{64});
    const Sample observed = End();
    if (!a || !b || !c || error != 0 || !e || !f || !g || !h ||
            observed.bytes != 1163 || observed.calls != 9) {
        throw std::runtime_error("Native allocation linker routing self-test failed");
    }
    std::free(a); std::free(b); std::free(c); std::free(d); std::free(e);
    ::operator delete(f); ::operator delete[](g); ::operator delete(h, std::align_val_t{64});
    a = allocate(123);
    std::free(a);
    const Sample unchanged = End();
    if (unchanged.bytes != observed.bytes || unchanged.calls != observed.calls) {
        throw std::runtime_error("Native allocation scope self-test failed");
    }
}
}  // namespace allocation_probe
