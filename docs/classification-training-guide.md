# Classification Training Guide (.NET Port)

This page ports the Java wiki page `Zemberek-NLP-ile-Metin-Sınıflandırma.md` to ZemberekDotNet.

It explains how to train and evaluate a Turkish news-title classifier using the .NET examples and apps in this repository.

## Dataset

Use the same dataset referenced by the original Java wiki:

- `news-title-category-set`
- `news-title-category-set.tokenized`
- `news-title-category-set.lemmas`

Source link (Java wiki reference):

- https://drive.google.com/drive/folders/1JBPExAeRctAXL2oGW2U6CbqfwIJ84BG7

Expected line format:

```text
__label__magazin Jackie Chan'a yapmadıklarını bırakmadılar!
__label__spor Fenerbahçe Akhisar'da çok rahat kazandı
```

## End-to-End Example in This Repo

The class `NewsTitleCategoryFinder` in `ZemberekDotNet.Examples.Classification` runs an end-to-end workflow:

- reads raw labeled lines
- prints category distribution
- creates train/test split (`testSize = 1000`)
- trains model(s) with `TrainClassifier`
- evaluates with `EvaluateClassifier`
- compares raw vs tokenized vs lemma-based inputs

Main file:

- `ZemberekDotNet.Examples.Classification/NewsTitleCategoryFinder.cs`

Set your local dataset path in that file before running.

Run:

```bash
dotnet run --project ZemberekDotNet.Examples.Classification/ZemberekDotNet.Examples.Classification.csproj
```

The example is configured with startup object:

- `ZemberekDotNet.Examples.Classification.NewsTitleCategoryFinder`

## Preprocessing Stages

The Java wiki highlights that preprocessing significantly improves classification quality. The same applies here.

Recommended progression:

1. Raw text
2. Tokenized + lowercase text
3. Lemma-based text
4. Optional model compression (quantization)

In this repo, the example already evaluates raw, tokenized, and lemma/split variants.

## Training Parameters

`NewsTitleCategoryFinder` currently uses:

- `--learningRate 0.1`
- `--epochCount 70`
- `--dimension 100`
- `--wordNGrams 2`

These are passed through `ZemberekDotNet.Apps.FastText.TrainClassifier`.

For reproducible comparisons, keep train/test split and parameters fixed while changing one preprocessing factor at a time.

## Evaluate Models

Evaluation is done with:

- `ZemberekDotNet.Apps.FastText.EvaluateClassifier`

The example writes prediction outputs and prints top-1 metrics (`k=1`) for the generated test split.

## API-Level Prediction

For runtime classification, see:

- `ZemberekDotNet.Examples.Classification/SimpleClassification.cs`

Core flow:

1. Load model with `FastTextClassifier.Load(...)`
2. Preprocess input exactly like training data
3. Call `Predict(processed, k)`

Important: preprocessing mismatch between training and inference will hurt accuracy.

## Quantization / Smaller Models

The training call in `NewsTitleCategoryFinder` includes commented options for quantization:

- `--applyQuantization`
- `--cutOff 25000`

Enable them when you need much smaller model files and can tolerate a small quality drop.

## Notes for Java Users

- Java CLI examples using `zemberek-full.jar` map to .NET app classes under `ZemberekDotNet.Apps.FastText`.
- Java example classes map to `.NET` example projects under `ZemberekDotNet.Examples.Classification`.
- Keep dataset format (`__label__...`) unchanged.

## Related Docs

- [Developer Guide](developer-guide.md)
- [Java to .NET Migration Quickstart](java-to-dotnet-migration-quickstart.md)
- [Java vs .NET Side-by-Side](java-dotnet-side-by-side.md)