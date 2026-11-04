#ifndef DOTJPEGLI_ALLOCATION_PROBE_H_
#define DOTJPEGLI_ALLOCATION_PROBE_H_

#include <cstdint>

namespace allocation_probe {
struct Sample {
    uint64_t bytes;
    uint64_t calls;
};
void Begin();
Sample End();
void SelfTest();
}  // namespace allocation_probe

#endif
