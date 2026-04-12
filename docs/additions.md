# Additions and Release Notes (.NET Port)

This page tracks ZemberekDotNet-specific additions that do not exist in Java Zemberek, plus release-level API notes.

## Version 0.19.2

### Morphology: Extended API additions

Namespace:

- `ZemberekDotNet.Morphology.Extended`

Added public API:

- `TurkishCase` enum (`ZemberekDotNet.Core.Turkish`)
- `SingleAnalysis` extensions:
  - `ExtdGetCase()`
  - `ExtdGetPossibleCases()`
- `TurkishMorphology` extensions:
  - `ExtdAnalyzeNumeralWithSuffix(string input)`
  - `ExtdAnalyzeWithRanking(string input, WordFrequencyModel frequencyModel = null)`
  - `ExtdSynthesize(string lemma, TurkishCase targetCase, bool plural = false)`
- `WordAnalysis` extension:
  - `ExtdGetRankedAnalyses(WordFrequencyModel frequencyModel = null)`
- New helper types:
  - `RankedAnalysis`
  - `WordFrequencyModel`
  - `ExtendedMorphologyContext`

### Embedded corpus

`ZemberekDotNet.Morphology` now embeds a Turkish frequency resource:

- `Extended/Resources/tr_50k.txt`

This corpus is used by ranking/fuzzy workflows and is packaged inside the Morphology NuGet artifact.

### Correctness note

`ExtdGetCase()` and `ExtdGetPossibleCases()` now infer nominative from the last morpheme group's POS, not from any nominal morpheme anywhere in the chain.

Why this matters:

- It prevents adjective-derivation false positives such as `elma + With(lı) -> Adj` from being incorrectly marked as nominative.

### Test coverage

Extended morphology tests include:

- numeral apostrophe analysis
- typed case extraction
- ranked analysis confidence behavior
- BK-tree fuzzy analysis behavior
- morphology synthesis helper behavior
- edge-case POS correctness for derivations (adjective, adverb, postposition, verb, gerund)

## Compatibility and migration

- These APIs are additive and backward-compatible.
- Existing Morphology APIs remain unchanged.
- To use new methods, import `ZemberekDotNet.Morphology.Extended`.
