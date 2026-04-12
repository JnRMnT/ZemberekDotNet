# Morphology Notes (.NET Port)

This document ports and curates Java wiki Morphology Notes for ZemberekDotNet.

The original Java page is a long engineering backlog of ambiguity edge cases and design questions.
This .NET version keeps the same spirit, but groups items into actionable categories.

Related references:

- [Morphology Examples](../ZemberekDotNet.Examples.Morphology/MorphologyExamples.cs)
- [Morphology Tests](../ZemberekDotNet.Morphology.Tests)
- [Porting Checklist](java-wiki-porting-checklist.md)

## 1. Purpose

Use this page as:

- a backlog of morphology decisions,
- a quality radar for high-ambiguity surfaces,
- a bridge between linguistic discussion and executable tests.

## 2. High-Value Ambiguity Areas

These are recurring ambiguity clusters that should be reviewed with corpus-driven evidence.

1. Noun to Verb zero-derivation overgeneration
- Example family: forms like noun + Zero->Verb + agreement/copula chains.
- Risk: linguistically odd but formally valid analyses increase ambiguity.

2. Pronoun and quantifier behavior
- `kimse`, `herkes`, `öbür`, demonstratives and reflexives produce many edge cases.
- Risk: permissive rules improve recall but can add false positives.

3. Compound and possessive interactions
- CompoundP3sg words and plural/possessive combinations can produce multiple parses.
- Risk: parser may allow additional analyses not desired in downstream tasks.

4. Proper noun suffix policies
- Apostrophe behavior, Equ/derivational suffix constraints, capitalization handling.
- Risk: mismatch between orthographic conventions and parser acceptance.

5. Verb derivation chain complexity
- Aorist/Narr/Past/Cond combinations, Able/Neg stacking, Inf->Noun->Verb chains.
- Risk: large ambiguity spaces and occasional unnatural candidates.

## 3. Practical Triage Rules

When deciding whether to keep or restrict a parse path:

1. Keep if:
- It appears in real corpus data,
- It improves disambiguation outcomes,
- It is consistent with existing accepted forms.

2. Restrict if:
- It mostly creates false positives,
- It appears only as typographical or tokenization artifacts,
- It causes severe ambiguity explosion with minimal real gain.

3. Defer if:
- It requires broad linguistic redesign,
- Java behavior is also unresolved,
- no stable evaluation benchmark exists yet.

## 4. Test-Driven Workflow for Morphology Notes

For each candidate rule change:

1. Add one positive test in morphology tests.
2. Add one negative test for likely false-positive shape.
3. Re-run affected test groups.
4. Record decision and rationale here.

Suggested test location patterns:

- analysis behavior: `ZemberekDotNet.Morphology.Tests/Analysis`
- ambiguity robustness: `ZemberekDotNet.Morphology.Tests/Ambiguity`

## 5. Known Open Notes in Current .NET Codebase

Examples of tracked open items (non-exhaustive):

1. Informal morphology forms (for some colloquial variants) are still incomplete.
2. Certain proper noun and derivation edge cases require stricter constraints.
3. Some derivational naming and morpheme grouping semantics remain discussion points.

See project status tracking for current priority order.

## 6. Recommended Next Deep-Dive Topics

1. Pronoun + quantifier morphology consistency matrix.
2. Compound P3sg plural/possessive ambiguity policy.
3. Zero-derivation restrictions for low-frequency false positives.
4. Informal morphology completion with dedicated corpus examples.

## 7. Scope Note

This page does not attempt to replicate each raw Java note line-by-line.
It provides a maintainable .NET engineering version of the same backlog intent.
