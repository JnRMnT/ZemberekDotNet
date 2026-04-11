# ZemberekDotNet Project Status

Current test suite: **607 passing / 29 skipped / 0 failing** (as of commit `fd95fe9`)

---

## 1. Missing from C# Port (Java has it, C# doesn't)

### 1.1 FastText retraining after quantize cutoff
- **File:** `ZemberekDotNet.Core/Embeddings/FastText.cs`, `FastTextTrainer.cs`, `Args.cs`
- **Status:** `[x]` DONE
- **Detail:** Extracted `TrainModel()` private method from `Train()`. Added `Retrain(input, dict_, model_)` internal method that bypasses dict/matrix initialization and runs only the parallel training loop. Added `input` field to `Args` for corpus path. In `Quantize()`, if `qargs.retrain && !string.IsNullOrEmpty(qargs.input)`, calls `new FastTextTrainer(args_).Retrain(qargs.input, dict_, model_)` to update matrices in-place before quantization.
- **Impact:** Quantized `.ftz` model correctly retrains after pruning when `retrain=true` and `input` is set.

### 1.2 AnalysisCache size limit not enforced
- **File:** `ZemberekDotNet.Morphology/Analysis/AnalysisCache.cs` line ~44
- **Status:** `[x]` DONE
- **Detail:** `SizeLimit = builder.DynamicCacheMaxSize` is commented out. Java uses Guava cache with max size. C# `MemoryCache` grows unbounded. Fix requires also setting `Size = 1` on each `Set()` call (MS requirement for SizeLimit to work).
- **Impact:** Memory leak in long-running analysis server scenarios.

### 1.3 InformalTurkishMorphotactics — yapıyim/okuyim/bakıyim
- **File:** `ZemberekDotNet.Morphology/Morphotactics/InformalTurkishMorphotactics.cs` line ~155
- **Status:** `[x]` ASSESSED — same gap in Java source, not a port regression
- **Detail:** The comment says `// Handling of yapıyim, okuyim, bakıyim. TODO: not yet finished`. The rules for these informal present tense forms are incomplete. Morphological analysis of informal Turkish text will miss these forms.
- **Impact:** Informal morphology accuracy.

---

## 2. Missing Even in Java (same gaps in both, not port regressions)

| # | File | Line | Description |
|---|---|---|---|
| 2.1 | `Core/Embeddings/ProductQuantizer.cs` | 9 | `nbits_=7` workaround (should be 8 — `byte` is unsigned in C#, signed in Java). Both kept for binary compat with each other. |
| 2.2 | `Core/Embeddings/ProductQuantizer.cs` | 61 | `GetCentroids` const overload not ported (minor, unused). |
| 2.3 | `Core/Embeddings/Model.cs` | 89 | Args mutation during `Load()` — design smell, not a bug. |
| 2.4 | `Core/Embeddings/Model.cs` | 335 | `Dfs()` — `//todo: check here.` — condition unverified, same in Java. |
| 2.5 | `Normalization/NormalizationVocabularyGenerator.cs` | 225 | `// TODO: fix below.` — token type filtering heuristic, same in Java. |
| 2.6 | `Morphology/Morphotactics/InformalTurkishMorphotactics.cs` | 34 | `// TODO: not used yet.` — unused rule state. |
| 2.7 | Various | — | 20+ linguistic design TODOs in Morphotactics, RootAttribute, etc. — domain decisions, not bugs. |

---

## 3. Skipped Tests (29 total)

### 3A — Require external data on Linux paths (unfixable without data)
| Test | Project | Reason |
|---|---|---|
| `TestActualData` | LM.Tests | Hardcoded `/home/ahmetaa/...` Linux paths |
| `TestBigFakeLm` | LM.Tests | Hardcoded `/media/ahmetaa/...` + 10.5M n-gram generation |
| `LoadLargeLmAndPrintInfo` | LM.Tests | Hardcoded `/media/depo/...` Linux path |
| `Performance` | Tokenization.Tests | `/media/aaa/Data/aaa/corpora/dunya.100k` |

### 3B — Require trained model binaries (not shipped)
| Test | Project | Reason |
|---|---|---|
| `FindNamedEntitiesInSentenceRequiresModel` | NER.Tests | Empty body, needs trained NER model |
| `LoadModelFromDirectoryRequiresFiles` | NER.Tests | Empty body, needs trained NER model |
| `LoadFromFileProducesClassifier` | Classification.Tests | Needs `model.bin` on disk |

### 3C — Slow tests needing large resource files
| Test | Project | Resource needed |
|---|---|---|
| `suggestWordPerformanceStemEnding` | Normalization.Tests | `lm-unigram.slm` |
| `SuggestWord1` | Normalization.Tests | `lm-unigram.slm` |
| `SuggestWordPerformanceWord` | Normalization.Tests | large word list + `lm-unigram.slm` |
| `RunSentence` | Normalization.Tests | `lm-bigram.slm` + test sentences |
| `CorrectWordFindingTest` | Normalization.Tests | `Resources/spell-checker-test.txt` |
| `PerformanceTest` (CharacterGraphDecoder) | Normalization.Tests | `zemberek-parsed-words-min30.txt` |
| `PerformanceTest` (SingleWordSpellChecker) | Normalization.Tests | `Resources/10000_frequent_turkish_word` — **possibly present, investigate** |
| `TestNewsCorpus` | Morphology.Tests | `Resources/corpora/cnn-turk-10k` |

### 3D — Intentionally not unit tests (dev utilities, not fixable)
| Test | Project | Reason |
|---|---|---|
| `PrepareWordListFromHistogram` | Morphology.Tests | Data conversion utility |
| `ShouldPrintItemsInDevlDictionary` | Morphology.Tests | Dev dictionary printer, needs `Resources/dev-lexicon.txt` |
| `SaveFullAttributes` | Morphology.Tests | Dumps dictionary to file |

### 3E — Performance benchmarks (intentionally skipped, not fixable as unit tests)
| Test | Project |
|---|---|
| `PerfStrings` | Core.Tests |
| `TestPerformance` (×2) | Core.Tests |
| `PerformanceAgainstMap` | Core.Tests |
| `PerformanceTest` | Core.Tests |
| `SpeedAgainstHashMap` (×2) | Core.Tests |
| `NgramFileMPHFTest` | Core.Tests |
| `LogSumPerf` | Core.Tests |
| `Dump` | Core.Tests |
| `TestMergePerformance` | Core.Tests |

---

## 4. Completed Work (this session series)

### Port fixes and implementations
- [x] ANTLR 4.9.3 → 4.13.1 upgrade
- [x] Reciprocal verb fix
- [x] Apostrophe tokenization fix
- [x] Matrix OOM fix
- [x] `LongBitVector.Serialize()` — implemented (was commented-out Java code)
- [x] `NoisyWordsLexiconGenerator.Build()` — inverted if/else bug fixed
- [x] `LongestCommonSubstring()` — jagged array NRE (C# vs Java 2D array init difference)
- [x] `FastTextTrainer.LoadVectors()` — implemented (pretrained vectors seeding), ported from C++ fasttext
- [x] `Dictionary.Threshold()` — extracted as reusable method (was inline + TODO)
- [x] `NormalizationVocabulary` in-memory constructor added
- [x] `ContextualSimilarityGraph.ContextHashToWordCounts` exposed as internal property

### Tests enabled
- [x] `WriteStringKeepOpenTest` — added `Flush()` to `SimpleTextWriter`, fixed seek-after-flush
- [x] `KeyIteratorStressTest` — `SetEquals` replacing `SequenceEqual`, removed dead loop
- [x] `EvaluateRequiresTestFile` — creates temp labeled corpus inline
- [x] `VocabularyGenerationTest` — fully ported from Java pseudocode to working C#

### Cleanup
- [x] Stale `//TODO:Check` removed from `LongBitVector.cs` (Serialize implemented)
- [x] Stale `// TODO: Check` removed from `TextUtil.RemoveAmpresandStrings` (verified correct)
- [x] Dead commented-out Java `CountComparator` removed from `Histogram.cs`

---

## 5. Work Order (Next Steps)

1. **[ ] Fix 1.2** — `AnalysisCache` size limit (`SizeLimit` + `Size=1` on entries)
2. **[ ] Fix 1.3** — `InformalTurkishMorphotactics` yapıyim/okuyim/bakıyim rules
3. **[ ] Investigate 3C** — Check if `SingleWordSpellCheckerTest.PerformanceTest` resource exists and can be enabled
4. **[ ] Fix 1.1** — `FastText` retraining (complex, last)
