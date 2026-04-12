# FAQ (.NET Port)

This page ports and adapts the Java Zemberek FAQ for ZemberekDotNet users.

## What is ZemberekDotNet?

ZemberekDotNet is a .NET implementation of Turkish NLP tooling inspired by Java Zemberek.
It provides modules for:

- morphology
- tokenization
- normalization
- NER
- classification
- language identification
- language modeling

See module matrix in the root README.

## What is the current project state?

Active .NET development with practical parity goals for core modules and examples.
The repository is not a strict line-by-line Java mirror; .NET-specific improvements are allowed.

## Should I use it?

Yes, especially for:

- Turkish preprocessing pipelines
- tokenization and sentence splitting
- lemmatization/stemming workflows
- baseline NLP systems in .NET

For state-of-the-art tasks, modern transformer-based pipelines may outperform classical approaches, but ZemberekDotNet remains very useful for deterministic preprocessing and hybrid systems.

## What is morphological analysis?

Morphological analysis decomposes words into stem/lemma and morphemes.
For example, a word may include plural, possessive, and case morphemes that are surfaced in analysis output.

Use:

```csharp
TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
WordAnalysis analyses = morphology.Analyze("kalemlerimden");
```

## What is morphological disambiguation?

Many Turkish words are ambiguous out of context. Disambiguation selects the most likely parse using sentence context.

Use:

```csharp
SentenceAnalysis sentence = morphology.AnalyzeAndDisambiguate("yarın okula gideceğim");
```

## Why does disambiguation fail sometimes?

Disambiguation is statistical/contextual and can fail on sparse, noisy, or domain-specific text.
Common causes:

- insufficient domain adaptation
- informal/noisy language
- out-of-vocabulary forms

## Can I use it as stemmer/lemmatizer?

Yes. You can extract stems/lemmas from best analyses.
For sentence-level quality, prefer disambiguated results.

## Can I add dictionary items programmatically?

Yes. You can build custom lexicons and load dictionary lines.
See:

- [Text Dictionary Rules](text-dictionary-rules.md)
- [Migration Quickstart](java-to-dotnet-migration-quickstart.md)

## Can I generate words?

Yes. Word generation is available in morphology module.
See morphology examples in this repository.

## Where is spell checking / suggestion functionality?

Use the normalization module.
It contains spell-checking and suggestion functionality.
For noisy sentence normalization, additional data/resources may be needed depending on scenario.

## Can I detect languages?

Yes. Use the LangID module:

```csharp
LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();
string lang = lid.Identify("merhaba dünya");
```

For very short strings (especially single words), language detection is less reliable.

## Is Python integration available?

There is no first-class Python package in this repository.
Typical integration patterns are:

- host .NET services and call from Python
- inter-process communication in your own architecture

gRPC support in this repository is deferred for future and not part of current port-completion scope.

## Is Android supported?

No official Android support is provided in this repository.
Primary target is modern .NET runtime usage (server/desktop/tooling scenarios).

## Can I use it in commercial products?

Check this repository license terms and dependency licenses.
As a general rule, commercial usage is possible when license obligations are met.

## What are alternatives?

Alternatives include other Turkish NLP toolchains and modern deep-learning frameworks.
In many real systems, ZemberekDotNet works well as a preprocessing component even when a neural model is used later in the pipeline.

## Why are docs/code in English?

English documentation/code conventions improve accessibility for broader developer audiences and cross-team collaboration.
