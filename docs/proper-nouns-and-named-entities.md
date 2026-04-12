# Proper Nouns and Named Entities (.NET Port)

This page ports Java wiki notes about proper nouns and named entities to ZemberekDotNet.

## 1. Proper Noun vs Named Entity

- Proper noun handling belongs to morphology and lexicon analysis.
- Named Entity Recognition (NER) is a higher-level task that may span multiple tokens.

Examples of named entities:

- `Türkiye`
- `Cahit Arf`
- `Kemal Tahir`
- `Muğlak İşler Müdürlüğü`

## 2. What Morphology Does

Morphology can:

1. Analyze whether a token can be interpreted as proper noun.
2. Handle suffix-attached proper nouns (especially apostrophe forms).
3. Use dictionary entries and runtime unknown-token strategies.

It is not a full NER system by itself.

## 3. Dictionary and Runtime Behavior

Ported behavior summary:

1. Proper nouns in dictionary are analyzed directly.
2. Capitalization can influence proper noun candidacy.
3. Tokens with apostrophe can trigger proper-noun-like runtime handling in unknown token analysis.

Related implementation areas:

- [TurkishMorphology](../ZemberekDotNet.Morphology/TurkishMorphology.cs)
- [UnidentifiedTokenAnalyzer](../ZemberekDotNet.Morphology/Analysis/UnidentifiedTokenAnalyzer.cs)

## 4. NER in This Repository

Unlike older Java-era FAQ notes that emphasized morphology-only proper noun detection, this repository includes a dedicated NER module:

- [ZemberekDotNet.NER](../ZemberekDotNet.NER)
- [NER examples](../ZemberekDotNet.Examples.NER/NERExamples.cs)

NER model quality depends on training data and model setup.

## 5. Post-processing for Suffix-Stripping

The NER module includes post-processing logic to strip/normalize suffix effects from named entities.

See:

- [NEPostProcessor](../ZemberekDotNet.NER/NEPostProcessor.cs)

This is useful for forms like location/person names with case/possessive suffixes.

## 6. Practical Guidance

1. Use morphology when you need token-level lexical/morpheme analyses.
2. Use NER when you need multi-word entity detection/classification.
3. In pipelines, use tokenization + morphology + NER together.

## 7. Testing Signals in This Repository

Relevant test coverage exists for proper noun and noun behavior in morphology tests.
For example, proper noun interpretations and compound/possessive ambiguity cases are validated in analysis tests.

See:

- [Nouns tests](../ZemberekDotNet.Morphology.Tests/Analysis/NounsTest.cs)

## 8. Caveats

1. Proper noun ambiguity can still be high for short/common words.
2. Apostrophe and casing conventions in real text are inconsistent.
3. For production extraction, treat morphology and NER outputs as probabilistic signals, not absolute truth.
