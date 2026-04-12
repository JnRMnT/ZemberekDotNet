# Morphemes Reference (.NET Port)

This page ports Java wiki Morphemes content to ZemberekDotNet terminology.

It is a practical reference for:

- POS and secondary POS tags
- common morpheme IDs used in analyses
- output nuances in .NET analysis formatting

Related implementation:

- [TurkishMorphotactics](../ZemberekDotNet.Morphology/Morphotactics/TurkishMorphotactics.cs)
- [SingleAnalysis](../ZemberekDotNet.Morphology/Analysis/SingleAnalysis.cs)

## 1. Primary POS IDs

Common primary POS IDs used in morphology output:

- `Noun`
- `Adj`
- `Adv`
- `Conj`
- `Interj`
- `Verb`
- `Pron`
- `Num`
- `Det`
- `Postp`
- `Ques`
- `Dup`
- `Punc`
- `Unk` (unknown, Zemberek-specific)

## 2. Secondary POS IDs

Common secondary POS IDs:

- `Demons` (demonstrative)
- `Time`
- `Quant`
- `Ques`
- `Prop` (proper noun)
- `Pers` (personal pronoun)
- `Reflex`
- `Ord`, `Card`, `Percent`, `Ratio`, `Dist`

Additional runtime/token categories may appear in dictionary metadata in this repository (for example date/email/url-like categories).

## 3. Agreement and Possessive Morphemes

Agreement:

- `A1sg`, `A2sg`, `A3sg`
- `A1pl`, `A2pl`, `A3pl`

Possessive:

- `P1sg`, `P2sg`, `P3sg`
- `P1pl`, `P2pl`, `P3pl`
- `Pnon` (no possession)

## 4. Case Morphemes

- `Nom` (nominative)
- `Dat` (dative)
- `Acc` (accusative)
- `Abl` (ablative)
- `Loc` (locative)
- `Ins` (instrumental)
- `Gen` (genitive)
- `Equ` (equative)

## 5. Common Derivational Morphemes

A non-exhaustive set used by current morphotactics:

- `Dim`, `Ness`, `With`, `Without`, `Related`, `JustLike`
- `Agt`, `Become`, `Acquire`
- `Caus`, `Recip`, `Reflex`, `Able`, `Pass`
- `Inf1`, `Inf2`, `Inf3`, `ActOf`
- `PastPart`, `NarrPart`, `FutPart`, `PresPart`, `AorPart`
- `NotState`, `FeelLike`, `EverSince`, `Repeat`, `Almost`, `Hastily`, `Stay`, `Start`
- `AsIf`, `While`, `When`, `SinceDoingSo`, `AsLongAs`, `ByDoingSo`, `Adamantly`
- `AfterDoingSo`, `WithoutHavingDoneSo`, `WithoutBeingAbleToHaveDoneSo`
- `Zero`

## 6. Verb/Tense/Polarity Morphemes

- Polarity/modality: `Neg`, `Unable`, `Cop`
- Tense/aspect: `Pres`, `Past`, `Narr`, `Cond`, `Prog1`, `Prog2`, `Aor`, `Fut`
- Mood/person-related: `Imp`, `Opt`, `Desr`, `Neces`

## 7. Output Nuance in .NET

In `SingleAnalysis` creation, `Nom` and `Pnon` are intentionally skipped in many formatted outputs to reduce visual noise.

See logic in:

- [SingleAnalysis](../ZemberekDotNet.Morphology/Analysis/SingleAnalysis.cs)

This means internal morpheme state and displayed analysis text can differ in verbosity.

## 8. How to Inspect Morphemes in Practice

1. Analyze a word/sentence with morphology module.
2. Print lexical output (`FormatLexical`) for stable morpheme IDs.
3. Inspect `GetMorphemes()`, `GetLemmas()`, and `GetStems()` for programmatic usage.

## 9. Notes on Cross-System Comparison

Java/Oflazer IDs and Zemberek IDs overlap significantly, but outputs are not guaranteed to match 1:1.

When evaluating parity:

1. Compare accepted analyses, not just one string format.
2. Validate with corpus/context and disambiguation behavior.
3. Prefer repository tests as source of truth for current .NET behavior.
