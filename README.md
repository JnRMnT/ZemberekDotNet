ZemberekDotNet
============
[![Test Status](https://img.shields.io/azure-devops/tests/jnrmnt/ZemberekDotNet/13)](https://img.shields.io/azure-devops/tests/jnrmnt/ZemberekDotNet/13)
[![Code Coverage](https://img.shields.io/azure-devops/coverage/jnrmnt/ZemberekDotNet/13)](https://img.shields.io/azure-devops/coverage/jnrmnt/ZemberekDotNet/13)

[![Build Status](https://dev.azure.com/jnrmnt/ZemberekDotNet/_apis/build/status/ZemberekDotNet?branchName=master)](https://dev.azure.com/jnrmnt/ZemberekDotNet/_build/latest?definitionId=13&branchName=master)
[![Release Status](https://vsrm.dev.azure.com/jnrmnt/_apis/public/Release/badge/dbf777b3-aa03-4952-92dc-55f20eba6724/1/1)](https://vsrm.dev.azure.com/jnrmnt/_apis/public/Release/badge/dbf777b3-aa03-4952-92dc-55f20eba6724/1/1)




ZemberekDotNet is/will be the .NET Port of [Zemberek-NLP](https://github.com/ahmetaa/zemberek-nlp) (Natural Language Processing tools for Turkish).

This library will be kept in sync with Zemberek-NLP and same module structure will be maintained in .NET platform using NuGet packages under seperate projects.

## Modules

|  Module    | Package Name |  Description       |    Status      |
|------------|----------|---------|---------|
| [Core](ZemberekDotNet.Core)                    | ZemberekDotNet.Core           | Special Collections, Hash functions and helpers. | Completed  |
| [Morphology](ZemberekDotNet.Morphology)        | ZemberekDotNet.Morphology     | Turkish morphological analysis, disambiguation and word generation. | Pending |
| [Tokenization](ZemberekDotNet.Tokenization)    | ZemberekDotNet.Tokenization         | Turkish Tokenization and sentence boundary detection. | Pending |
| [Normalization](ZemberekDotNet.Normalization)  | ZemberekDotNet.Normalization        | Basic spell checker, word suggestion. Noisy text normalization. |  Pending |
| [NER](NER)                      | ZemberekDotNet.NER                  | Turkish Named Entity Recognition. | Pending |
| [Classification](ZemberekDotNet.Classification)| ZemberekDotNet.Classification       | Text classification based on Java port of fastText project. |  Completed |
| [Language Identification](ZemberekDotNet.LangID)| ZemberekDotNet.LangID            | Fast identification of text language. |  Active Development |
| [Language Modeling](ZemberekDotNet.LM)         | ZemberekDotNet.LM                   | Provides a language model compression algorithm. | Pending |
| [Applications](ZemberekDotNet.Apps)            | ZemberekDotNet.Apps                 | Console applications | Pending |
| [gRPC Server](ZemberekDotNet.GRPC)             | ZemberekDotNet.GRPC                 | gRPC server for access from other languages. | Pending |
| [Examples](ZemberekDotNet.Examples)            | ZemberekDotNet.Examples             | Usage examples. |  Pending |

## Target Platforms
Packages are targeting .NET Standart 2.1 Framework so that it can be used within .Net Core and .Net Framework projects. Examples/console applications will also be prepared with .Net Core aiming that the whole library can be used cross platform.

## CI/CD
Repository is configured to continuously trigger a build, test and release cycle using Azure DevOps. At the end of a successful release, it automatically publishes the artifacts to NuGet.org.
