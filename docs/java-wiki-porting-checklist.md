# Java Wiki Porting Checklist

This checklist tracks which Java wiki/documentation topics are already represented in ZemberekDotNet docs.

## Source References

- Java repo README: https://github.com/ahmetaa/zemberek-nlp
- Java wiki home: https://github.com/ahmetaa/zemberek-nlp/wiki
- Java wiki clone URL: https://github.com/ahmetaa/zemberek-nlp.wiki.git

## Checklist

| Topic | Java Source | .NET Status | Action |
|---|---|---|---|
| FAQ | Wiki: `FAQ` | Partial | Add .NET-focused FAQ page under `docs/faq.md` |
| Morphology notes | Wiki: `Morphology-Notes` + `morphology/README.md` | Partial | Add detailed morphology guide with .NET examples |
| Text dictionary rules | Wiki: `Text-Dictionary-Rules` | Missing | Port as `docs/text-dictionary-rules.md` |
| Normalization guide | `normalization/README.md` + wiki pages | Partial | Add environment/setup guide for data files |
| Classification how-to | Wiki: Turkish classification page + `classification/README.md` | Partial | Add end-to-end training/eval doc in English |
| Developer guide | Wiki: `Zemberek-For-Developers` | Missing | Add contributor/build doc for .NET repo |
| Apps usage | `apps/README.md` | Partial | Expand `ZemberekDotNet.Apps` command documentation |
| gRPC usage | `grpc` module + app docs | Deferred | Track only; not in current port-completion scope |

## Decision Log

- gRPC documentation is deferred for future because gRPC module implementation is deferred for future in this repo.
- Current documentation priority is module parity and migration support for Java users moving to .NET.
