# ZemberekDotNet

> **No Java. No JVM. No sidecar.**
> Pure .NET Standard 2.1 Turkish NLP — morphology, tokenization, NER, classification, normalization, language ID and more.

ZemberekDotNet is a C#/.NET port of [Zemberek-NLP](https://github.com/ahmetaa/zemberek-nlp) that has evolved into an actively maintained library.
It is no longer a strict port — correctness fixes and .NET-specific improvements are introduced where needed.

See the [repository README](https://github.com/JnRMnT/ZemberekDotNet#readme) for NuGet quick-start, module table, and code samples.

---

## Get started

| I want to… | Go to |
|---|---|
| Install a package and write first code | [README — Quick Start](https://github.com/JnRMnT/ZemberekDotNet#quick-start) |
| Migrate from Java Zemberek | [Java to .NET Migration Quickstart](java-to-dotnet-migration-quickstart) |
| Compare Java and .NET API names | [Java vs .NET Side-by-Side](java-dotnet-side-by-side) |
| Build and test from source | [Developer Guide](developer-guide) |
| Run CLI tools (train, evaluate, preprocess) | [Apps CLI Guide](apps-cli-guide) |

---

## Module reference

| Module | Wiki page |
|---|---|
| Morphological analysis and disambiguation | [Morphology Notes](morphology-notes) |
| Morpheme inventory | [Morphemes Reference](morphemes-reference) |
| Text classification (fastText) | [Classification Training Guide](classification-training-guide) |
| Noisy text normalization / spell check | [Normalization Guide](normalization-guide) |
| Proper nouns and named entity recognition | [Proper Nouns and Named Entities](proper-nouns-and-named-entities) |
| Dictionary and lexicon rules | [Text Dictionary Rules](text-dictionary-rules) |

---

## Reference

- [FAQ](faq) — common questions, licensing, performance, .NET Framework limits
- [Java Wiki Porting Checklist](java-wiki-porting-checklist) — tracks parity between original Java wiki and this repo

---

## Scope notes

- gRPC module documentation is deferred — the gRPC project itself is not yet in scope.
- Apps CLI covers classification, morphology, corpus preprocessing, and NER. Additional commands may be added over time.