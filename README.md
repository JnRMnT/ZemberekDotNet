ZemberekDotNet
============
[![Test Status](https://img.shields.io/azure-devops/tests/jnrmnt/ZemberekDotNet/13)](https://img.shields.io/azure-devops/tests/jnrmnt/ZemberekDotNet/13)
[![Code Coverage](https://img.shields.io/azure-devops/coverage/jnrmnt/ZemberekDotNet/13)](https://img.shields.io/azure-devops/coverage/jnrmnt/ZemberekDotNet/13)

[![Build Status](https://dev.azure.com/jnrmnt/ZemberekDotNet/_apis/build/status/ZemberekDotNet?branchName=master)](https://dev.azure.com/jnrmnt/ZemberekDotNet/_build/latest?definitionId=13&branchName=master)
[![Release Status](https://vsrm.dev.azure.com/jnrmnt/_apis/public/Release/badge/dbf777b3-aa03-4952-92dc-55f20eba6724/1/1)](https://vsrm.dev.azure.com/jnrmnt/_apis/public/Release/badge/dbf777b3-aa03-4952-92dc-55f20eba6724/1/1)

> **No Java. No JVM. No sidecar.** Pure .NET Standard 2.1 — drop it into any .NET Core 3.0+ or .NET 5/6/7/8 project from NuGet and start processing Turkish text immediately.


ZemberekDotNet started as a C#/.NET port of [Zemberek-NLP](https://github.com/ahmetaa/zemberek-nlp) (Natural Language Processing tools for Turkish) and has since evolved into an actively improved library. While it maintains compatibility with the original Java library's module structure and core algorithms, it is no longer a strict port — new features, correctness fixes, and .NET-specific improvements are introduced where needed for production use.

The goal is to provide a high-quality, production-ready Turkish NLP library for the .NET ecosystem, not merely to replicate the Java implementation.

### Improvements over the original Java port

- **Reciprocal verb morphology**: Re-enabled the reciprocal suffix (`Iş`) transition in the morphotactics engine, which was disabled in the port. Verbs such as `kaçış`, `dövüşmek` are now correctly analyzed.
- **Smart apostrophe tokenization**: Added dual-path merge logic in the tokenizer to correctly handle foreign brand names containing apostrophes (e.g., `L'Oréal` is emitted as a single token), while preserving Turkish morphological suffix boundaries (e.g., `Ankara'ya` remains split for downstream analysis).
- **ANTLR runtime upgrade**: Upgraded from ANTLR 4.9.3 to 4.13.1, replacing a fragile 460-line custom ATN deserializer with the standard runtime implementation.

This library will maintain the same module structure as Zemberek-NLP using NuGet packages under separate projects, and will continue to track the original library where relevant.

## Quick Start

Install the morphology module:

```sh
dotnet add package ZemberekDotNet.Morphology
```

Analyze and disambiguate a sentence:

```csharp
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;

// Loads the built-in lexicon and disambiguation model (one-time initialization)
TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();

SentenceAnalysis result = morphology.AnalyzeAndDisambiguate("Kitaplara gidiyorum.");
foreach (SentenceWordAnalysis swa in result)
{
    SingleAnalysis best = swa.GetBestAnalysis();
    Console.WriteLine($"{swa.GetWordAnalysis().GetInput()} → {best.GetLemmas()[0]}  [{best.FormatLexical()}]");
}
// Kitaplara → kitap  [kitap:Noun+A3pl+Dat]
// gidiyorum → git    [git:Verb+Pres+A1sg]
```

Install all modules at once:

```sh
dotnet add package ZemberekDotNet.All
```

## Modules

|  Module    | Package Name |  Description       |    Status      |
|------------|----------|---------|---------|
| [All](ZemberekDotNet.All)                    | ZemberekDotNet.All           | Wrapper Package that includes all the modules. | [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.All)](https://www.nuget.org/packages/ZemberekDotNet.All/)[![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.All)](https://www.nuget.org/packages/ZemberekDotNet.All/)  |
| [Core](ZemberekDotNet.Core)                    | ZemberekDotNet.Core           | Special Collections, Hash functions and helpers. | [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.Core)](https://www.nuget.org/packages/ZemberekDotNet.Core/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.Core)](https://www.nuget.org/packages/ZemberekDotNet.Core/) |
| [Morphology](ZemberekDotNet.Morphology)        | ZemberekDotNet.Morphology     | Turkish morphological analysis, disambiguation and word generation. | [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.Morphology)](https://www.nuget.org/packages/ZemberekDotNet.Morphology/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.Morphology)](https://www.nuget.org/packages/ZemberekDotNet.Morphology/)  |
| [Tokenization](ZemberekDotNet.Tokenization)    | ZemberekDotNet.Tokenization         | Turkish Tokenization and sentence boundary detection. | [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.Tokenization)](https://www.nuget.org/packages/ZemberekDotNet.Tokenization/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.Tokenization)](https://www.nuget.org/packages/ZemberekDotNet.Tokenization/) |
| [Normalization](ZemberekDotNet.Normalization)  | ZemberekDotNet.Normalization        | Basic spell checker, word suggestion. Noisy text normalization. |  [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.Normalization)](https://www.nuget.org/packages/ZemberekDotNet.Normalization/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.Normalization)](https://www.nuget.org/packages/ZemberekDotNet.Normalization/) |
| [NER](ZemberekDotNet.NER)                      | ZemberekDotNet.NER                  | Turkish Named Entity Recognition. |  [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.NER)](https://www.nuget.org/packages/ZemberekDotNet.NER/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.NER)](https://www.nuget.org/packages/ZemberekDotNet.NER/) |
| [Classification](ZemberekDotNet.Classification)| ZemberekDotNet.Classification       | High-performance fastText-based text classification. |  [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.Classification)](https://www.nuget.org/packages/ZemberekDotNet.Classification/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.Classification)](https://www.nuget.org/packages/ZemberekDotNet.Classification/) |
| [Language Identification](ZemberekDotNet.LangID)| ZemberekDotNet.LangID            | Fast identification of text language. |  [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.LangID)](https://www.nuget.org/packages/ZemberekDotNet.LangID/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.LangID)](https://www.nuget.org/packages/ZemberekDotNet.LangID/) |
| [Language Modeling](ZemberekDotNet.LM)         | ZemberekDotNet.LM                   | Provides a language model compression algorithm. |  [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.LM)](https://www.nuget.org/packages/ZemberekDotNet.LM/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.LM)](https://www.nuget.org/packages/ZemberekDotNet.LM/) |
| [Applications](ZemberekDotNet.Apps)            | ZemberekDotNet.Apps                 | CLI tools for classification, morphology, corpus preprocessing, and NER workflows. | ✅ Available |
| gRPC Server (planned)             | ZemberekDotNet.GRPC                 | gRPC server for access from other languages. | Deferred for future (not required for current port scope) |
| [Examples: Classification](ZemberekDotNet.Examples.Classification) | — | News title category classification using fastText. | ✅ Available |
| [Examples: Morphology](ZemberekDotNet.Examples.Morphology)         | — | Analysis, disambiguation, LINQ lemma extraction, word generation (conjugation + noun cases). | ✅ Available |
| [Examples: Tokenization](ZemberekDotNet.Examples.Tokenization)     | — | Sentence splitting, token-type inspection, document processing. | ✅ Available |
| [Examples: Language ID](ZemberekDotNet.Examples.LangID)            | — | Language ID, confidence scores, `ContainsLanguage`, sentence-level mixed-language scanner. | ✅ Available |
| [Examples: NER](ZemberekDotNet.Examples.NER)                       | — | In-memory NER training and named-entity inference with PERSON/LOCATION/ORGANIZATION labels. | ✅ Available |
| [Examples: Normalization](ZemberekDotNet.Examples.Normalization)   | — | Turkish spell check, word suggestions, sentence-level typo highlighting. | ✅ Available |
| [Examples: Pipeline](ZemberekDotNet.Examples.Pipeline)             | — | End-to-end Tokenization → Morphology → LangID pipeline with POS fingerprinting. | ✅ Available |

## Documentation

The full developer documentation is published on the **[GitHub Wiki](https://github.com/JnRMnT/ZemberekDotNet/wiki)**.

| Page | Description |
|---|---|
| [Apps CLI Guide](https://github.com/JnRMnT/ZemberekDotNet/wiki/apps-cli-guide) | CLI tools for classification, morphology, corpus preprocessing, and NER |
| [Java to .NET Migration Quickstart](https://github.com/JnRMnT/ZemberekDotNet/wiki/java-to-dotnet-migration-quickstart) | Step-by-step guide for teams moving from Java Zemberek |
| [Developer Guide](https://github.com/JnRMnT/ZemberekDotNet/wiki/developer-guide) | Build, test, and contribute from source |
| [Morphology Notes](https://github.com/JnRMnT/ZemberekDotNet/wiki/morphology-notes) | Morphological analysis and disambiguation internals |
| [Morphemes Reference](https://github.com/JnRMnT/ZemberekDotNet/wiki/morphemes-reference) | Full morpheme inventory with examples |
| [Classification Training Guide](https://github.com/JnRMnT/ZemberekDotNet/wiki/classification-training-guide) | Training and evaluating fastText text classifiers |
| [Normalization Guide](https://github.com/JnRMnT/ZemberekDotNet/wiki/normalization-guide) | Noisy text normalization and spell checking |
| [Proper Nouns and Named Entities](https://github.com/JnRMnT/ZemberekDotNet/wiki/proper-nouns-and-named-entities) | NER, proper noun handling, and entity types |
| [Text Dictionary Rules](https://github.com/JnRMnT/ZemberekDotNet/wiki/text-dictionary-rules) | Lexicon format, rule syntax, and custom dictionaries |
| [Java vs .NET Side-by-Side](https://github.com/JnRMnT/ZemberekDotNet/wiki/java-dotnet-side-by-side) | API name mapping between the Java and .NET libraries |
| [FAQ](https://github.com/JnRMnT/ZemberekDotNet/wiki/faq) | Common questions: licensing, performance, .NET Framework limits |

Wiki source lives in the [`docs/`](docs/) folder and is auto-synced to the wiki on each local build.

Notes:

- Current port-completion scope prioritizes core library and example parity.
- gRPC documentation is deferred for future because the gRPC module is deferred for future in this repository.

### Known Missing / Deferred Items

- `ZemberekDotNet.GRPC` project is not in the repository yet. The module name is reserved in the table for future implementation.
- Apps CLI parity is focused on high-use workflows (classification, morphology, corpus preprocessing, NER). Additional app surface from the original Java ecosystem may be expanded over time.

## Examples

Each example project is a self-contained runnable console app - clone the repo, `dotnet run`, and see real output.
Examples are validated primarily through runnable sample projects and module-level test suites.

| Project | What it shows | Entry point |
|---|---|---|
| [Examples.Morphology](ZemberekDotNet.Examples.Morphology) | Single-word analysis, sentence disambiguation, LINQ lemma extraction, word generation | [MorphologyExamples.cs](ZemberekDotNet.Examples.Morphology/MorphologyExamples.cs) |
| [Examples.Tokenization](ZemberekDotNet.Examples.Tokenization) | Sentence splitting, token-type inspection, document processing | [TokenizationExamples.cs](ZemberekDotNet.Examples.Tokenization/TokenizationExamples.cs) |
| [Examples.LangID](ZemberekDotNet.Examples.LangID) | Language detection, confidence scores, `ContainsLanguage`, mixed-language sentence scanner | [LangIDExamples.cs](ZemberekDotNet.Examples.LangID/LangIDExamples.cs) |
| [Examples.NER](ZemberekDotNet.Examples.NER) | Train a small NER model and run PERSON/LOCATION/ORGANIZATION extraction | [NERExamples.cs](ZemberekDotNet.Examples.NER/NERExamples.cs) |
| [Examples.Normalization](ZemberekDotNet.Examples.Normalization) | Spell checking, ranked word suggestions, sentence typo highlighting | [NormalizationExamples.cs](ZemberekDotNet.Examples.Normalization/NormalizationExamples.cs) |
| [Examples.Pipeline](ZemberekDotNet.Examples.Pipeline) | End-to-end Tokenization + Morphology + LangID workflow | [PipelineExamples.cs](ZemberekDotNet.Examples.Pipeline/PipelineExamples.cs) |
| [Examples.Classification](ZemberekDotNet.Examples.Classification) | News title category classification (fastText, no model file needed to browse code) | [SimpleClassification.cs](ZemberekDotNet.Examples.Classification/SimpleClassification.cs) |

## API at a Glance

ZemberekDotNet uses idiomatic C# — no Java-style builder chains, no Guava dependencies. The table below shows the most common use cases side-by-side.

### Morphological Analysis

| Java (zemberek-nlp) | C# (ZemberekDotNet) |
|---|---|
| `TurkishMorphology m = TurkishMorphology.createWithDefaults();` | `TurkishMorphology m = TurkishMorphology.CreateWithDefaults();` |
| `SentenceAnalysis a = m.analyzeAndDisambiguate(s);` | `SentenceAnalysis a = m.AnalyzeAndDisambiguate(s);` |
| `a.forEach(e -> e.getBestAnalysis().getLemmas())` | `a.Select(e => e.GetBestAnalysis().GetLemmas())` |

```csharp
// LINQ-style: extract all root lemmas from a sentence
TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();

List<string> lemmas = morphology
    .AnalyzeAndDisambiguate("Güzel bir gün bugün.")
    .Where(swa => !swa.GetBestAnalysis().IsUnknown())
    .Select(swa => swa.GetBestAnalysis().GetLemmas()[0])
    .ToList();
// ["güzel", "bir", "gün", "bu"]
```

### Tokenization & Sentence Splitting

```csharp
using ZemberekDotNet.Tokenization;

// Split a paragraph into sentences
List<string> sentences = TurkishSentenceExtractor.Default
    .FromParagraph("Merhaba dünya. Bugün iyi bir gün.");
// ["Merhaba dünya.", "Bugün iyi bir gün."]

// Tokenize a sentence
List<string> tokens = TurkishTokenizer.Default
    .TokenizeToStrings(sentences[0]);
// ["Merhaba", "dünya", "."]
```

### Language Identification

```csharp
using ZemberekDotNet.LangID;

LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();

Console.WriteLine(lid.Identify("merhaba dünya ve tüm gezegenler")); // tr
Console.WriteLine(lid.Identify("hello world and all the planets")); // en
Console.WriteLine(lid.Identify("Hola mundo y todos los planetas")); // es

// With confidence scores
List<LanguageIdentifier.IdResult> scores =
    lid.GetScores("merhaba dünya", maxSampleCount: -1);
scores.ForEach(r => Console.WriteLine($"{r.id}: {r.score:F4}"));
```


Current targets are:

- Library packages target `netstandard2.1` (cross-platform for modern .NET runtimes).
- Test projects target `net8.0`.
- Apps and examples target `net8.0`.

Compatibility notes:

- `netstandard2.1` libraries can be consumed by .NET Core 3.0+ and .NET 5+.
- .NET Framework is not supported by `netstandard2.1`.

## CI/CD
Repository is configured to continuously trigger a build, test and release cycle using Azure DevOps. At the end of a successful release, it automatically publishes the artifacts to NuGet.org.
