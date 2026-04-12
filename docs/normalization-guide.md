# Normalization Guide (.NET Port)

This page ports Java wiki/content around text normalization to ZemberekDotNet usage.

Related code:

- [Normalization examples](../ZemberekDotNet.Examples.Normalization/NormalizationExamples.cs)
- [TurkishSentenceNormalizer](../ZemberekDotNet.Normalization/TurkishSentenceNormalizer.cs)

## 1. What Normalization Covers

In this repository, normalization-related workflows include:

1. Word spell-checking (`TurkishSpellChecker.Check`)
2. Word-level correction suggestions (`TurkishSpellChecker.SuggestForWord`)
3. Noisy sentence normalization (`TurkishSentenceNormalizer.Normalize`)

## 2. Spell Checker (Word Validity)

```csharp
TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
TurkishSpellChecker spellChecker = new TurkishSpellChecker(morphology);

bool ok = spellChecker.Check("okuyabileceğimden");
```

## 3. Suggestion API

```csharp
List<string> suggestions = spellChecker.SuggestForWord("okuyablirim");
```

Context-aware ranking variant is also available and can improve ranking quality when an LM/context is supplied.

## 4. Noisy Text Normalization

`TurkishSentenceNormalizer` combines multiple candidate-generation strategies (lookup tables, informal/ascii-tolerant morphology, spell checker candidates) and applies LM-based sequence decoding.

Constructor:

```csharp
TurkishSentenceNormalizer normalizer = new TurkishSentenceNormalizer(
    morphology,
    dataRoot: @"C:\zemberek-data\normalization",
    languageModelPath: @"C:\zemberek-data\lm\lm.2gram.slm");
```

Usage:

```csharp
string normalized = normalizer.Normalize("Yrn okua gidicem");
```

## 5. Data Requirements

Sentence-level normalization needs external resources:

1. Normalization lookup files (`dataRoot` folder)
2. Compressed language model file (`languageModelPath`)

Java docs reference a downloadable baseline package with these assets. The same concept applies here: sentence normalizer quality depends heavily on domain-appropriate data.

## 6. Practical Setup Notes for This Repository

1. Word-level spell-checking and suggestions are demonstrated directly in:
   - [NormalizationExamples.cs](../ZemberekDotNet.Examples.Normalization/NormalizationExamples.cs)
2. Some normalization tests/resources in this repository are lightweight or test-focused and may not replace full production normalization data.
3. For best noisy-text results, build or curate domain-specific lookup/LM resources.

## 7. Method Summary (Ported Concept)

The underlying normalization pipeline follows the same high-level approach documented on Java side:

1. Build vocabularies from clean/noisy corpora.
2. Generate candidate corrections from lookup tables and analysis heuristics.
3. Add contextual scoring with language model.
4. Select best token sequence with dynamic search/decoding.

## 8. Limitations and Expectations

1. Automatic normalization may still produce incorrect edits.
2. Casing/punctuation may change depending on processing path.
3. Domain mismatch can reduce correction quality.
4. Results depend on quality of lookup and LM resources.

## 9. Recommendation

Use normalization as a pipeline component with evaluation:

1. Start with a baseline dataset and measure precision/recall of corrections.
2. Add domain-specific corpora and regenerate resources.
3. Keep user-facing safeguards for critical workflows (for example, review mode rather than silent rewrite).
