ZemberekDotNet
============
[![Test Status](https://img.shields.io/azure-devops/tests/jnrmnt/ZemberekDotNet/13)](https://img.shields.io/azure-devops/tests/jnrmnt/ZemberekDotNet/13)
[![Code Coverage](https://img.shields.io/azure-devops/coverage/jnrmnt/ZemberekDotNet/13)](https://img.shields.io/azure-devops/coverage/jnrmnt/ZemberekDotNet/13)

[![Build Status](https://dev.azure.com/jnrmnt/ZemberekDotNet/_apis/build/status/ZemberekDotNet?branchName=master)](https://dev.azure.com/jnrmnt/ZemberekDotNet/_build/latest?definitionId=13&branchName=master)
[![Release Status](https://vsrm.dev.azure.com/jnrmnt/_apis/public/Release/badge/dbf777b3-aa03-4952-92dc-55f20eba6724/1/1)](https://vsrm.dev.azure.com/jnrmnt/_apis/public/Release/badge/dbf777b3-aa03-4952-92dc-55f20eba6724/1/1)




ZemberekDotNet started as a C#/.NET port of [Zemberek-NLP](https://github.com/ahmetaa/zemberek-nlp) (Natural Language Processing tools for Turkish) and has since evolved into an actively improved library. While it maintains compatibility with the original Java library's module structure and core algorithms, it is no longer a strict port — new features, correctness fixes, and .NET-specific improvements are introduced where needed for production use.

The goal is to provide a high-quality, production-ready Turkish NLP library for the .NET ecosystem, not merely to replicate the Java implementation.

### Improvements over the original Java port

- **Reciprocal verb morphology**: Re-enabled the reciprocal suffix (`Iş`) transition in the morphotactics engine, which was disabled in the port. Verbs such as `kaçış`, `dövüşmek` are now correctly analyzed.
- **Smart apostrophe tokenization**: Added dual-path merge logic in the tokenizer to correctly handle foreign brand names containing apostrophes (e.g., `L'Oréal` is emitted as a single token), while preserving Turkish morphological suffix boundaries (e.g., `Ankara'ya` remains split for downstream analysis).
- **ANTLR runtime upgrade**: Upgraded from ANTLR 4.9.3 to 4.13.1, replacing a fragile 460-line custom ATN deserializer with the standard runtime implementation.

This library will maintain the same module structure as Zemberek-NLP using NuGet packages under separate projects, and will continue to track the original library where relevant.

## Modules

|  Module    | Package Name |  Description       |    Status      |
|------------|----------|---------|---------|
| [All](ZemberekDotNet.All)                    | ZemberekDotNet.All           | Wrapper Package that includes all the modules. | [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.All)](https://www.nuget.org/packages/ZemberekDotNet.All/)[![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.All)](https://www.nuget.org/packages/ZemberekDotNet.All/)  |
| [Core](ZemberekDotNet.Core)                    | ZemberekDotNet.Core           | Special Collections, Hash functions and helpers. | [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.Core)](https://www.nuget.org/packages/ZemberekDotNet.Core/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.Core)](https://www.nuget.org/packages/ZemberekDotNet.Core/) |
| [Morphology](ZemberekDotNet.Morphology)        | ZemberekDotNet.Morphology     | Turkish morphological analysis, disambiguation and word generation. | [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.Morphology)](https://www.nuget.org/packages/ZemberekDotNet.Morphology/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.Morphology)](https://www.nuget.org/packages/ZemberekDotNet.Morphology/)  |
| [Tokenization](ZemberekDotNet.Tokenization)    | ZemberekDotNet.Tokenization         | Turkish Tokenization and sentence boundary detection. | [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.Tokenization)](https://www.nuget.org/packages/ZemberekDotNet.Tokenization/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.Tokenization)](https://www.nuget.org/packages/ZemberekDotNet.Tokenization/) |
| [Normalization](ZemberekDotNet.Normalization)  | ZemberekDotNet.Normalization        | Basic spell checker, word suggestion. Noisy text normalization. |  [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.Normalization)](https://www.nuget.org/packages/ZemberekDotNet.Normalization/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.Normalization)](https://www.nuget.org/packages/ZemberekDotNet.Normalization/) |
| [NER](NER)                      | ZemberekDotNet.NER                  | Turkish Named Entity Recognition. |  [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.NER)](https://www.nuget.org/packages/ZemberekDotNet.NER/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.NER)](https://www.nuget.org/packages/ZemberekDotNet.NER/) |
| [Classification](ZemberekDotNet.Classification)| ZemberekDotNet.Classification       | Text classification based on Java port of fastText project. |  [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.Classification)](https://www.nuget.org/packages/ZemberekDotNet.Classification/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.Classification)](https://www.nuget.org/packages/ZemberekDotNet.Classification/) |
| [Language Identification](ZemberekDotNet.LangID)| ZemberekDotNet.LangID            | Fast identification of text language. |  [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.LangID)](https://www.nuget.org/packages/ZemberekDotNet.LangID/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.LangID)](https://www.nuget.org/packages/ZemberekDotNet.LangID/) |
| [Language Modeling](ZemberekDotNet.LM)         | ZemberekDotNet.LM                   | Provides a language model compression algorithm. |  [![NuGet](https://img.shields.io/nuget/v/ZemberekDotNet.LM)](https://www.nuget.org/packages/ZemberekDotNet.LM/) [![NuGet](https://img.shields.io/nuget/dt/ZemberekDotNet.LM)](https://www.nuget.org/packages/ZemberekDotNet.LM/) |
| [Applications](ZemberekDotNet.Apps)            | ZemberekDotNet.Apps                 | Console applications | Pending |
| [gRPC Server](ZemberekDotNet.GRPC)             | ZemberekDotNet.GRPC                 | gRPC server for access from other languages. | Pending |
| [Examples](ZemberekDotNet.Examples)            | ZemberekDotNet.Examples             | Usage examples. |  Pending |

## Target Platforms
Current targets are:

- Library packages target `netstandard2.1` (cross-platform for modern .NET runtimes).
- Test projects target `net8.0`.
- Apps and examples target `net8.0`.

Compatibility notes:

- `netstandard2.1` libraries can be consumed by .NET Core 3.0+ and .NET 5+.
- .NET Framework is not supported by `netstandard2.1`.

## CI/CD
Repository is configured to continuously trigger a build, test and release cycle using Azure DevOps. At the end of a successful release, it automatically publishes the artifacts to NuGet.org.
