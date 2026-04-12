# Text Dictionary Rules (.NET Port)

This page is the ZemberekDotNet port of Java wiki page Text Dictionary Rules.

It explains dictionary-line syntax used by the Turkish lexicon loader in this repository.

Related implementation:

- [TurkishDictionaryLoader](../ZemberekDotNet.Morphology/Lexicon/TR/TurkishDictionaryLoader.cs)
- [TurkishDictionaryLoader tests](../ZemberekDotNet.Morphology.Tests/Lexicon/TR/TurkishDictionaryLoaderTest.cs)

## 1. Dictionary Line Format

General format:

```text
word [P:PRIMARY_POS,SECONDARY_POS ; A:ATTRIBUTE1,ATTRIBUTE2 ; Pr:PRONUNCIATION ; Roots:r1-r2 ; Ref:itemId ; Index:n]
```

Minimal examples:

```text
kalem
okumak
Ankara
```

## 2. POS Rules

Metadata key for POS is P.

Examples:

```text
ekşi [P:Adj]
ve [P:Conj]
bu [P:Pron,Demons]
Tdk [P:Abbrv]
… [P:Punc]
```

Inference rules (same intent as Java docs):

- If a lemma ends with mak or mek, loader infers Verb.
- Otherwise loader usually infers Noun.
- Capitalized lemma is treated as proper noun unless overridden.

Examples:

```text
elma           // inferred noun
okumak         // inferred verb
çomak [P:Noun] // noun ending with -mak requires override
```

## 3. Attribute Rules

Metadata key for attributes is A.

Common examples:

```text
bulut [A:NoVoicing]
turp [A:Voicing]
saat [A:InverseHarmony,NoVoicing]
hat [A:Doubling]
ağız [A:LastVowelDrop]
kavurmak [A:LastVowelDrop]
```

Frequently used attributes in dictionary lines:

- Voicing / NoVoicing
- InverseHarmony
- Doubling
- LastVowelDrop
- CompoundP3sg
- Aorist_A / Aorist_I (verbs)
- NoQuote

## 4. Compound Entries

For compound forms with embedded possessive behavior, use CompoundP3sg and Roots.

```text
aşevi [A:CompoundP3sg; Roots:aş-ev]
atkuyruğu [A:CompoundP3sg; Roots:at-kuyruk]
```

## 5. Pronunciation Rules

Metadata key for pronunciation is Pr.

Use this especially for abbreviations and foreign names where orthography does not provide correct Turkish phonetics.

```text
Google [Pr:gugıl]
A101 [P:Abbrv; Pr:ayüzbir]
```

Multiple pronunciation workaround (using Ref and Index):

```text
VST [P:Noun,Abbrv; Pr:viesti]
VST [P:Noun,Abbrv; Pr:vesete; Ref:VST_Noun_Abbrv; Index:2]
```

## 6. Metadata Keys Supported by Loader

Based on current loader implementation:

- P: POS
- A: Attributes
- Pr: Pronunciation
- Roots: Compound roots
- Ref: Reference item
- Index: Index for alternative entries
- S: Suffix metadata (reserved/special use)

## 7. C# Usage

### Load from inline lines

```csharp
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Lexicon.TR;

RootLexicon lexicon = TurkishDictionaryLoader.Load(
    "elma",
    "okumak",
    "çomak [P:Noun]",
    "Google [Pr:gugıl]",
    "atkuyruğu [A:CompoundP3sg; Roots:at-kuyruk]"
);
```

### Load one item

```csharp
DictionaryItem item = TurkishDictionaryLoader.LoadFromString("turp [A:Voicing]");
```

### Load from file

```csharp
RootLexicon lexicon = TurkishDictionaryLoader.Load("Resources/my-dictionary.txt");
```

## 8. Verification

Use existing tests as canonical behavior references:

- POS inference and noun/verb behavior
- voicing and attribute handling
- pronunciation + reference/index behavior
- compound root handling

See [TurkishDictionaryLoader tests](../ZemberekDotNet.Morphology.Tests/Lexicon/TR/TurkishDictionaryLoaderTest.cs).

## 9. Differences and Notes

- This page is a .NET-targeted port, so examples and method names follow C# APIs.
- Behavior is aligned with current loader implementation and tests in this repo.
- If behavior differs from Java wiki wording, implementation and tests in this repository are authoritative.
