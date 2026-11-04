# Follow-up optimization batch

This batch follows the source audit at `51126a7`. Earlier experiments have
already been completed or retired; this document tracks the newly identified
work. No operation-count reduction is a measured encoding speedup.

| Lead | Intended integration | Status |
| --- | --- | --- |
| Progressive AC collection across scans | Exact per-component traversal with independent scan states and bounded token storage | In progress |
| Joint sample/coefficient cache planning | Reserve useful coefficient capacity before admitting converted samples under constrained budgets | In progress |
| Parallel histogram/token production | Complete-cache baseline batches with private counters and exact predecessor DC values | In progress |
| Sampled ICC inverse index | Bounded immutable exact predecessor index, including plateau ties | In progress |
| Smoothing row reuse | Fixed-size three-row halo with original floating-point operation order | Pending |
| Byte-input XYB transfer values | Lazy exact 256-value table for untransformed byte RGB/grayscale | In progress |
| Quantized parallel row pipeline | Assess stage ownership, bounded storage and ordered failure behavior before deployment | Pending |
| Huffman symbol ordering | Offline complete-JPEG search within equal-length code groups; retain control on ties | In progress |
| Image-specific RDOQ costs | Bounded feedback between coefficient search and actual image Huffman lengths | Pending |
| Cross-block DC trellis | Bounded opt-in search with restart-aware predictors and explicit quality validation | Pending |
| Improved chroma preparation/advice | Opt-in reconstruction-aware candidates and sampling assessment | Pending |
| Clipping-aware deringing | Opt-in saturated-edge preprocessing with decoded quality floors | Pending |

Ordinary encoding must preserve its existing output unless an explicitly
selected compression experiment changes coefficients or sampling. Experiments
retain controls and rank complete encoded output; approximate rate models never
establish final byte savings. Internal diagnostic choices belong to immutable
encode options and never change another concurrent encode.

Local validation consists of Release builds and focused correctness tests.
Full suites and every benchmark run on CI. The settled batch is pushed once to
`main`, reusing that push's automatic run for correctness, full native comparison
and bounded same-host diagnostics. Logical changes are committed as they become
ready. Raw invalid timing cases remain retained and no thresholds are relaxed.
