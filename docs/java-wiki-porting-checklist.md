# Java Wiki Porting Checklist

This checklist tracks which Java wiki/documentation topics are already represented in ZemberekDotNet docs.

## Source References

- Java repo README: https://github.com/ahmetaa/zemberek-nlp
- Java wiki home: https://github.com/ahmetaa/zemberek-nlp/wiki
- Java wiki clone URL: https://github.com/ahmetaa/zemberek-nlp.wiki.git

## Full Wiki Inventory (Java)

Source wiki pages discovered from `zemberek-nlp.wiki.git`:

1. `Home.md`
2. `FAQ.md`
3. `Metin-Normalizasyonu.md`
4. `Morphemes.md`
5. `Morphology-Notes.md`
6. `Proper-nouns-and-named-entities..md`
7. `Text-Dictionary-Rules.md`
8. `Zemberek-For-Developers.md`
9. `Zemberek-NLP-ile-Metin-Sınıflandırma.md`

## Checklist

| Java Wiki Page | Proposed .NET Doc | Status | Action |
|---|---|---|---|
| `Home.md` | `docs/wiki-home-port.md` | Ported | Keep as docs landing page and ensure links stay current |
| `FAQ.md` | `docs/faq.md` | Ported | Keep answers aligned with module status and scope notes |
| `Metin-Normalizasyonu.md` | `docs/normalization-guide.md` | Ported | Keep aligned with sentence normalizer resource/data requirements |
| `Morphemes.md` | `docs/morphemes-reference.md` | Ported | Keep terminology aligned with morphotactics/output formatting |
| `Morphology-Notes.md` | `docs/morphology-notes.md` | Ported | Keep synchronized with ambiguity decisions and test updates |
| `Proper-nouns-and-named-entities..md` | `docs/proper-nouns-and-named-entities.md` | Ported | Keep aligned with morphology + NER module behavior |
| `Text-Dictionary-Rules.md` | `docs/text-dictionary-rules.md` | Ported | Keep updated with loader/test behavior changes |
| `Zemberek-For-Developers.md` | `docs/developer-guide.md` | Ported | Keep commands synchronized with CI and target frameworks |
| `Zemberek-NLP-ile-Metin-Sınıflandırma.md` | `docs/classification-training-guide.md` | Ported | Keep dataset and workflow examples aligned with classification examples |
| Java `grpc` docs context | `docs/grpc-notes.md` (future) | Deferred | Track as future work only; outside current port-completion scope |

## Decision Log

- gRPC documentation is deferred for future because gRPC module implementation is deferred for future in this repo.
- Current documentation priority is module parity and migration support for Java users moving to .NET.
