# Java to .NET Migration Quickstart

This page helps teams migrate from Java Zemberek usage patterns to ZemberekDotNet with minimal friction.

## 1. Dependency Setup

Java (Maven):

```xml
<dependency>
  <groupId>zemberek-nlp</groupId>
  <artifactId>zemberek-morphology</artifactId>
  <version>0.17.1</version>
</dependency>
```

.NET (NuGet):

```sh
dotnet add package ZemberekDotNet.Morphology
```

All modules:

```sh
dotnet add package ZemberekDotNet.All
```

## 2. Morphology

Java:

```java
TurkishMorphology morphology = TurkishMorphology.createWithDefaults();
SentenceAnalysis analysis = morphology.analyzeAndDisambiguate("Yarın okula gideceğim.");
```

.NET:

```csharp
TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
SentenceAnalysis analysis = morphology.AnalyzeAndDisambiguate("Yarın okula gideceğim.");
```

## 3. Tokenization

Java:

```java
List<Token> tokens = TurkishTokenizer.DEFAULT.tokenize("Merhaba dünya.");
```

.NET:

```csharp
List<Token> tokens = TurkishTokenizer.Default.Tokenize("Merhaba dünya.");
```

## 4. Sentence Extraction

Java:

```java
List<String> sentences = TurkishSentenceExtractor.DEFAULT.fromParagraph(text);
```

.NET:

```csharp
List<string> sentences = TurkishSentenceExtractor.Default.FromParagraph(text);
```

## 5. Language Identification

Java:

```java
LanguageIdentifier lid = LanguageIdentifier.fromInternalModels();
String lang = lid.identify("merhaba dünya");
```

.NET:

```csharp
LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();
string lang = lid.Identify("merhaba dünya");
```

## 6. NER

Java:

```java
PerceptronNer ner = PerceptronNer.loadModel(modelRoot, morphology);
NerSentence result = ner.findNamedEntities(sentence);
```

.NET:

```csharp
PerceptronNer ner = PerceptronNer.LoadModel(modelRoot, morphology);
NerSentence result = ner.FindNamedEntities(sentence);
```

## 7. Typical Naming Differences

| Java | .NET |
|---|---|
| `createWithDefaults()` | `CreateWithDefaults()` |
| `analyzeAndDisambiguate()` | `AnalyzeAndDisambiguate()` |
| `DEFAULT` | `Default` |
| `fromParagraph()` | `FromParagraph()` |
| `identify()` | `Identify()` |

## 8. Practical Notes

- Keep one long-lived `TurkishMorphology` instance in app lifetime.
- Prefer module-specific examples in this repository when behavior differs from old Java snippets.
- gRPC usage is deferred for future in this repository and is outside current port-completion scope.
