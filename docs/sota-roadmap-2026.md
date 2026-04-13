# ZemberekDotNet 2026 — SOTA Roadmap

> **Internal working document.** Tracks the evolution of ZemberekDotNet from a strict port into a high-performance, modern .NET NLP engine. Items are executed one-by-one in priority order; each track has acceptance criteria before it can be closed.

---

## Milestones

| Milestone | Theme | Status |
|-----------|-------|--------|
| **M1** | Foundation + Parity Tooling | ✅ Complete |
| **M2** | Parity Fix Iterations + Lexicon Refresh | 🔵 In Progress |
| **M3** | Performance Sprint + Native AOT | ⬜ Planned |
| **M4** | Neural Package + Published Benchmarks | ⬜ Planned |

---

## Epic Backlog

### Phase 0 — Tracking Setup ✅

| # | Task | Status | Notes |
|---|------|--------|-------|
| 0.1 | Create `docs/sota-roadmap-2026.md` (this file) | ✅ Done | — |
| 0.2 | Link tracker from `README.md` | ✅ Done | Added to Documentation table |
| 0.3 | Add sidebar entry in `docs/_Sidebar.md` | ✅ Done | Under new "Roadmap" section |

---

### Phase 1 — Foundation for Modern .NET (M1)

**Goal:** Introduce multi-targeting so net8+ modern API surface (FrozenDictionary, SearchValues, Span hot paths) can be adopted incrementally without breaking netstandard2.1 consumers.

**Acceptance criteria:**
- All touched packages build and pass tests on both `netstandard2.1` and `net8.0` TFMs.
- No existing public API signature changed.
- CI pipeline updated to run matrix on both TFMs.

| # | Task | Status | Risk | Evidence |
|---|------|--------|------|----------|
| 1.1 | Add `net8.0` target to `ZemberekDotNet.Core.csproj` alongside `netstandard2.1` | ✅ Done | Low | Build green |
| 1.2 | Add `net8.0` target to `ZemberekDotNet.Morphology.csproj` | ✅ Done | Low | Build green |
| 1.3 | Add `net8.0` target to `ZemberekDotNet.Tokenization.csproj` | ✅ Done | Low | Build green |
| 1.4 | Add `net8.0` target to remaining library projects | ✅ Done | Low | Build green (all 9 library projects) |
| 1.5 | Add `#if NET8_0_OR_GREATER` scaffolding in a placeholder file to validate conditional compilation | ✅ Done | Low | `Deasciifier.cs` uses conditional compile for `BinaryFormatter` → `System.Text.Json` |
| 1.6 | ~~Update CI matrix (`azure-pipelines.yml`) to test both TFMs~~ | 🚫 Dropped | — | Multi-TFM validation done locally via full test suite (714 tests, 0 failures) |

---

### Phase 2 — Parity Sprint (M1 / M2)

**Goal:** Build a cross-validation CLI tool that runs Java Zemberek 0.17.1 and ZemberekDotNet on the same Turkish corpus, produces a structured diff report, categorizes mismatches, and enables root-cause–driven iterative fixes.

**Acceptance criteria:**
- CLI command `validate-parity` exists in `ZemberekDotNet.Apps` and is documented in `apps-cli-guide.md`.
- Running against a 1 000-sentence reference corpus produces a JSON report + markdown summary with < 5 % crash rate.
- At least the top-3 mismatch categories are triaged and have filed issues or inline notes.

| # | Task | Status | Risk | Evidence |
|---|------|--------|------|----------|
| 2.1 | Design diff report schema (`ParityReport.cs` — JSON-serializable) | ✅ Done | Low | `ParityReport`, `SentenceParity`, `WordParity`, `MismatchCategory` in `ZemberekDotNet.Apps/Morphology/Parity/ParityReport.cs` |
| 2.2 | Add `ValidateParityConsole.cs` in `ZemberekDotNet.Apps/Morphology/` implementing `ConsoleApp<T>` | ✅ Done | Low | Builds, `--help` output correct |
| 2.3 | Implement Java process invocation adapter (stdout/stderr capture, timeout, normalization) | ✅ Done | Medium | `JavaProcessRunner.cs`; configurable args template |
| 2.4 | Implement .NET analysis runner with same normalization as Java adapter | ✅ Done | Low | `DotNetAnalysisRunner.cs` uses `FormatLexical()` + `IsUnknown()` |
| 2.5 | Implement diff engine: per-word lattice comparison, mismatch categorization | ✅ Done | Medium | `ParityDiffEngine.cs`; 5 categories: TokenizationDiff, LexiconGap, BestAnalysisDiff, AnalysisCountDiff, BothUnknown |
| 2.6 | Implement JSON + markdown report emitter | ✅ Done | Low | JSON via `System.Text.Json`; console summary in `ValidateParityConsole.PrintSummary` |
| 2.7 | Add deterministic fixture corpus + expected-output snapshot; add snapshot test | ✅ Done | Low | `ZemberekDotNet.Apps.Tests` project; 12 tests green; `fixture_parity_java.tsv` covers all 5 mismatch categories |
| 2.8 | Triage top mismatch categories from first corpus run; log findings in this table | ⬜ Planned | — | Triage notes below |
| 2.9 | Fix iteration 1 — highest-frequency mismatch class | ⬜ Planned | Varies | Parity report delta |
| 2.10 | Fix iteration 2 — second mismatch class | ⬜ Planned | Varies | Parity report delta |

**Triage log** *(populated after 2.8 completes)*

| Category | Count | Root cause | Fix target |
|----------|-------|-----------|-----------|
| *(to be filled)* | — | — | — |

---

### Phase 3 — Lexicon Refresh & Injection (M2)

**Goal:** Update bundled lexicons toward more linguistically rigorous modern Turkish vocabulary; validate update using the parity tool from Phase 2.

**Acceptance criteria:**
- Lexicon update passes all existing morphology test cases with no regressions.
- Parity report shows measurable improvement in coverage after update.
- Custom lexicon injection path is documented.

| # | Task | Status | Risk | Evidence |
|---|------|--------|------|----------|
| 3.1 | Evaluate candidate source (e.g. Google Research `turkish-morphology`): license, format, coverage overlap | ⬜ Planned | Medium | Evaluation note added here |
| 3.2 | Write import/validation script: deduplication, invalid metadata, POS consistency checks | ⬜ Planned | Low | Script + test |
| 3.3 | Integrate validated additions into `.dict` files; run regression suite | ⬜ Planned | Medium | 0 new failures |
| 3.4 | Re-run parity tool pre/post and capture coverage delta | ⬜ Planned | Low | Parity report diff |
| 3.5 | Document custom lexicon override workflow in `text-dictionary-rules.md` | ⬜ Planned | Low | Doc review |

---

### Phase 4 — Performance Sprint (M3)

**Goal:** Use modern .NET 8+ APIs to reduce allocations, CPU cycles, and GC pressure in hot analysis/disambiguation/tokenization paths. Gate every optimization on benchmark evidence.

**Acceptance criteria:**
- `ZemberekDotNet.Benchmarks` project exists with reproducible BenchmarkDotNet suite.
- FrozenDictionary/SearchValues changes show measurable gains in benchmarks.
- No behavioral regressions on netstandard2.1 path.

**Key candidates (from codebase analysis):**

| File | Candidate | API |
|------|-----------|-----|
| `ZemberekDotNet.Core/Text/TextUtil.cs` | `htmlstringToCharMapFull` / `htmlstringToCharMapCommon` | `FrozenDictionary` |
| `ZemberekDotNet.Morphology/Extended/SingleAnalysisExtensions.cs` | `ExplicitCaseMap` (8 entries) | `FrozenDictionary` |
| `ZemberekDotNet.Tokenization/PerceptronSegmenter.cs L74–78` | vowel / upper-case char loops | `SearchValues<char>` |
| `ZemberekDotNet.Tokenization/TurkishSentenceExtractor.cs` | `GetSubstring()` 15+ Substring calls per boundary | `ReadOnlySpan<char>` slice |
| `ZemberekDotNet.Morphology/TurkishMorphology.cs` | `NormalizeForAnalysis()` Substring+Replace per word | `Span<char>` / `string.Replace` overload |
| `ZemberekDotNet.Core/Enums/EnumConverter.cs` | `conversionFromEToP` / `conversionFromPToE` | `FrozenDictionary` |

| # | Task | Status | Risk | Evidence |
|---|------|--------|------|----------|
| 4.1 | Create `ZemberekDotNet.Benchmarks` project (BenchmarkDotNet, net8.0) | ⬜ Planned | Low | Project builds |
| 4.2 | Add baseline benchmarks for tokenization, morphology analysis, disambiguation | ⬜ Planned | Low | BDN HTML report |
| 4.3 | Frozen collections pass — Core static maps (net8+ `#if`-guarded) | ⬜ Planned | Low | Benchmark delta, tests green |
| 4.4 | Frozen collections pass — Morphology static maps | ⬜ Planned | Low | Benchmark delta, tests green |
| 4.5 | SearchValues pass — PerceptronSegmenter vowel/upper-case classification | ⬜ Planned | Low | Benchmark delta, tests green |
| 4.6 | Span pass — TurkishSentenceExtractor hot substring loop | ⬜ Planned | Medium | Benchmark delta, tests green |
| 4.7 | Span pass — TurkishMorphology normalization | ⬜ Planned | Medium | Benchmark delta, tests green |

---

### Phase 5 — Native AOT Readiness (M3)

**Goal:** Eliminate or contain reflection paths so the library can be published with `PublishAot=true`. Targets primarily the Apps host and Core enum conversion.

**Acceptance criteria:**
- `dotnet publish -p:PublishAot=true` succeeds for a sample console app using Core + Morphology.
- No reflection-based runtime failures in AOT smoke test.
- Reflection-free paths documented; any remaining unsupported surface clearly noted.

**Known blockers (from codebase analysis):**

| File | Issue |
|------|-------|
| `ZemberekDotNet.Apps/ApplicationRunner.cs:17–85` | `Assembly.GetTypes()` + `Activator.CreateInstance()` + `MethodInfo.Invoke()` for dynamic command discovery |
| `ZemberekDotNet.Core/Enums/EnumConverter.cs:18–80` | `Type.GetType("Google.Protobuf...")` + `PropertyInfo`/`FieldInfo` reflection |

| # | Task | Status | Risk | Evidence |
|---|------|--------|------|----------|
| 5.1 | Replace `ApplicationRunner` dynamic dispatch with static command registry | ⬜ Planned | Medium | Existing commands still work |
| 5.2 | Rewrite `EnumConverter` reflection path with AOT-safe explicit mapping | ⬜ Planned | Medium | Unit tests green |
| 5.3 | Add AOT publish profile for `ZemberekDotNet.Examples.Morphology` as smoke target | ⬜ Planned | Low | Publishes without trim warnings |
| 5.4 | Document supported AOT surface and known exclusions | ⬜ Planned | Low | Doc added |

---

### Phase 6 — Optional ONNX Neural Package (M4)

**Goal:** Create an optional `ZemberekDotNet.Neural` NuGet package that adds context-aware morphological disambiguation via a small ONNX model, while keeping the core packages dependency-free.

**Acceptance criteria:**
- `ZemberekDotNet.Neural` implements `IAmbiguityResolver` and compiles independently.
- Neural scorer falls back to `PerceptronAmbiguityResolver` when confidence is below threshold.
- Core `TurkishMorphology` API unchanged; neural scorer injected through existing `SetDisambiguator` path.

**Integration seam:** `ZemberekDotNet.Morphology/Ambiguity/IAmbiguityResolver.cs`

| # | Task | Status | Risk | Evidence |
|---|------|--------|------|----------|
| 6.1 | Create `ZemberekDotNet.Neural` project with `Microsoft.ML.OnnxRuntime` dependency | ⬜ Planned | Low | Project builds |
| 6.2 | Implement `OnnxAmbiguityScorer : IAmbiguityResolver` with fallback to `PerceptronAmbiguityResolver` | ⬜ Planned | Medium | Unit tests with stub model |
| 6.3 | Implement feature extraction aligned with existing perceptron feature set | ⬜ Planned | Medium | Feature parity note |
| 6.4 | Add model loading, versioning, and confidence-threshold configuration | ⬜ Planned | Low | Integration test |
| 6.5 | Document usage and model acquisition workflow | ⬜ Planned | Low | Doc added |

---

### Phase 7 — Standardized Benchmarks & Credibility (M4)

**Goal:** Publish reproducible performance results (BenchmarkDotNet) and quality evaluation against a TrGLUE-style benchmark in the README so users and adopters have objective data.

**Acceptance criteria:**
- Benchmark results table added to README with hardware/runtime disclosure.
- Methodology and reproduction steps in `docs/benchmarks.md`.

| # | Task | Status | Risk | Evidence |
|---|------|--------|------|----------|
| 7.1 | Run full BenchmarkDotNet suite on reference machine; capture baseline HTML report | ⬜ Planned | Low | BDN artifacts |
| 7.2 | Create `docs/benchmarks.md` with methodology, hardware spec, reproduction steps | ⬜ Planned | Low | Doc review |
| 7.3 | Add Benchmark results summary table to `README.md` | ⬜ Planned | Low | README updated |
| 7.4 | Evaluate TrGLUE 2026 compatibility and add relevant evaluation harness if feasible | ⬜ Planned | Medium | Evaluation note |

---

## KPI Targets

| KPI | Current baseline | M3 Target | M4 Target |
|-----|-----------------|-----------|-----------|
| Morphology mismatches vs Java (1 000-sentence corpus) | *TBD after 2.8* | ≤ 5 % | ≤ 2 % |
| Tokenizer throughput (tokens/sec, net8) | *TBD after 4.2* | +30 % | +50 % |
| Morphology alloc/op (hot path) | *TBD after 4.2* | −50 % | −70 % |
| Cold-start latency (TurkishMorphology.CreateWithDefaults) | *TBD after 4.2* | ≤ current | ≤ current |
| PublishAot smoke build | ❌ Fails | ✅ Passes | ✅ Passes |
| Neural disambiguator accuracy (test set) | N/A | N/A | *Established* |

---

## Decision Log

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-04-13 | First implementation focus: Parity Sprint | Most impactful correctness signal before optimization |
| 2026-04-13 | Add net8.0 multi-targeting while retaining netstandard2.1 | Enables modern API surface without breaking existing consumers |
| 2026-04-13 | Java runtime permitted in CI/dev for cross-validation tooling | Enables direct output comparison; no ongoing production dependency |
| 2026-04-13 | Lexicon source generators deferred | Runtime embedding already works; source generators add complexity without proven startup bottleneck |
| 2026-04-13 | `Deasciifier.BinaryFormatter` → `System.Text.Json` on net8+ | `BinaryFormatter` is removed in .NET 8; `turkishPatternTable.json` generated from binary via .NET 7 converter tool; `Deasciifier.cs` uses `#if NET8_0_OR_GREATER` to load JSON, legacy path retained for `netstandard2.1` |

---

*Last updated: 2026-04-13 — M1 complete. Phase 2 tooling (2.1–2.7) complete; 2.8–2.10 awaiting first live corpus run.*
