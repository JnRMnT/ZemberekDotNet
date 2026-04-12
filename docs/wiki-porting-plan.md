# Wiki Porting Plan

This plan defines how to port all Java wiki pages into the ZemberekDotNet documentation set.

## Objective

Port all Java wiki pages to .NET-focused documentation with:

- API-correct .NET examples.
- Current repository structure and build/test commands.
- Links to corresponding examples and tests in this codebase.

## Source Scope

Java wiki pages to port:

1. Home
2. FAQ
3. Metin Normalizasyonu
4. Morphemes
5. Morphology Notes
6. Proper nouns and named entities
7. Text Dictionary Rules
8. Zemberek For Developers
9. Zemberek NLP ile Metin Sınıflandırma

## Phases

## Phase 1: Foundation

Deliverables:

- `docs/wiki-home-port.md`
- `docs/faq.md`
- `docs/developer-guide.md`

Definition of done:

- All links resolve in this repository.
- Commands are executable on this repo layout.
- Terminology is consistent with existing README.

## Phase 2: Morphology and Lexicon

Deliverables:

- `docs/morphemes-reference.md`
- `docs/morphology-notes.md`
- `docs/text-dictionary-rules.md`
- `docs/proper-nouns-and-named-entities.md`

Definition of done:

- Every conceptual section has at least one .NET code example.
- Examples reference actual classes/methods in current codebase.
- Known differences from Java behavior are documented.

## Phase 3: Normalization and Classification

Deliverables:

- `docs/normalization-guide.md`
- `docs/classification-training-guide.md`

Definition of done:

- Data/model prerequisites are clearly listed.
- Resource paths are documented for .NET test and example workflows.
- End-to-end sample commands are included.

## Phase 4: Quality and Parity Review

Deliverables:

- Parity review table update in `docs/java-wiki-porting-checklist.md`
- Link audit and consistency pass across `README.md` and `docs/`

Definition of done:

- All checklist items marked complete except explicitly deferred items.
- No broken internal links.
- Side-by-side mapping and migration quickstart reference all new docs.

## Prioritization

Suggested order:

1. Text Dictionary Rules
2. FAQ
3. Morphology Notes
4. Morphemes
5. Proper nouns and named entities
6. Normalization guide
7. Classification training guide
8. Developer guide
9. Wiki home port page

## Deferred Scope

- gRPC documentation is deferred for future, matching repository scope decisions.
- If gRPC module implementation starts later, create `docs/grpc-notes.md` and update this plan.

## Tracking

Use `docs/java-wiki-porting-checklist.md` as the canonical tracking board.
