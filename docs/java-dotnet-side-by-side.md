# Java vs .NET Side-by-Side

This page maps core Java Zemberek documentation and APIs to ZemberekDotNet equivalents.

## Project-Level Mapping

| Java (zemberek-nlp) | .NET (ZemberekDotNet) | Status |
|---|---|---|
| `core` | `ZemberekDotNet.Core` | Available |
| `morphology` | `ZemberekDotNet.Morphology` | Available |
| `tokenization` | `ZemberekDotNet.Tokenization` | Available |
| `normalization` | `ZemberekDotNet.Normalization` | Available |
| `ner` | `ZemberekDotNet.NER` | Available |
| `classification` | `ZemberekDotNet.Classification` | Available |
| `lang-id` | `ZemberekDotNet.LangID` | Available |
| `lm` | `ZemberekDotNet.LM` | Available |
| `apps` | `ZemberekDotNet.Apps` | In progress |
| `grpc` | `ZemberekDotNet.GRPC` | Deferred for future |
| `examples` | `ZemberekDotNet.Examples.*` | Available |

## Common API Mapping

| Java API | .NET API |
|---|---|
| `TurkishMorphology.createWithDefaults()` | `TurkishMorphology.CreateWithDefaults()` |
| `analyzeAndDisambiguate(sentence)` | `AnalyzeAndDisambiguate(sentence)` |
| `analyzeSentence(sentence)` | `AnalyzeSentence(sentence)` |
| `disambiguate(sentence, analyses)` | `Disambiguate(sentence, analyses)` |
| `SentenceAnalysis.bestAnalysis()` | Iterate `SentenceAnalysis` and call `GetBestAnalysis()` |
| `TurkishTokenizer.DEFAULT` | `TurkishTokenizer.Default` |
| `TurkishSentenceExtractor.DEFAULT` | `TurkishSentenceExtractor.Default` |
| `LanguageIdentifier.fromInternalModels()` | `LanguageIdentifier.FromInternalModels()` |

## Documentation Mapping

| Java Docs Source | .NET Counterpart |
|---|---|
| Root `README.md` modules and usage | Root `README.md` quick start, modules, examples |
| `morphology/README.md` | `README.md` + `ZemberekDotNet.Examples.Morphology` |
| `normalization/README.md` | `README.md` + `ZemberekDotNet.Examples.Normalization` |
| `apps/README.md` | `ZemberekDotNet.Apps` + root module table |
| Wiki pages (`FAQ`, morphology notes, dictionary rules, etc.) | `docs/` pages in this repo (ongoing port) |

## Notes

- Java wiki pages exist but are mixed in freshness and some pages are sparse or not actively maintained.
- For this repository, docs under `docs/` are intended to become the canonical source for .NET-specific behavior.
