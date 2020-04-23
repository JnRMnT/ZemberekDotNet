ZemberekDotNet
============

ZemberekDotNet is/will be the .NET Port of [Zemberek-NLP](https://github.com/ahmetaa/zemberek-nlp) (Natural Language Processing tools for Turkish).

This library will be kept in sync with Zemberek-NLP and same module structure will be maintained in .NET platform using NuGet packages under seperate projects.

## Modules

|  Module    | Package Name |  Description       |    Status      |
|------------|----------|---------|---------|
| [Core](ZemberekDotNet.Core)                    | ZemberekDotNet.Core           | Special Collections, Hash functions and helpers. | Active Development  |
| [Morphology](ZemberekDotNet.Morphology)        | ZemberekDotNet.Morphology     | Turkish morphological analysis, disambiguation and word generation. | Pending |
| [Tokenization](ZemberekDotNet.Tokenization)    | ZemberekDotNet.Tokenization         | Turkish Tokenization and sentence boundary detection. | Pending |
| [Normalization](ZemberekDotNet.Normalization)  | ZemberekDotNet.Normalization        | Basic spell checker, word suggestion. Noisy text normalization. |  Pending |
| [NER](NER)                      | ZemberekDotNet.NER                  | Turkish Named Entity Recognition. | Pending |
| [Classification](ZemberekDotNet.Classification)| ZemberekDotNet.Classification       | Text classification based on Java port of fastText project. |  Pending |
| [Language Identification](ZemberekDotNet.LangID)| ZemberekDotNet.LangID            | Fast identification of text language. |  Pending |
| [Language Modeling](ZemberekDotNet.LM)         | ZemberekDotNet.LM                   | Provides a language model compression algorithm. | Pending |
| [Applications](ZemberekDotNet.Apps)            | ZemberekDotNet.Apps                 | Console applications | Pending |
| [gRPC Server](ZemberekDotNet.GRPC)             | ZemberekDotNet.GRPC                 | gRPC server for access from other languages. | Pending |
| [Examples](ZemberekDotNet.Examples)            | ZemberekDotNet.Examples             | Usage examples. |  Pending |

## Target Platforms
Packages are targeting .NET Standart 2.1 Framework so that it can be used within .Net Core and .Net Framework projects. Examples/console applications will also be prepared with .Net Core aiming that the whole library can be used cross platform.