# Apps CLI Guide (.NET Port)

This page documents the command-line applications in `ZemberekDotNet.Apps`.

Run without arguments to list available apps:

```sh
dotnet run --project ZemberekDotNet.Apps/ZemberekDotNet.Apps.csproj
```

## Available Commands

- `TrainClassifier`: Train a fastText-based text classifier from labeled `__label__...` lines.
- `EvaluateClassifier`: Evaluate a classifier model on a labeled test set.
- `ClassificationConsole`: Interactive prediction console for a trained classification model.
- `PreprocessTurkishCorpus`: Sentence split and tokenize corpus files (with optional lemma preprocessing).
- `MorphologyConsole`: Interactive Turkish morphology analysis and disambiguation.
- `ValidateParityConsole`: Compare Java and .NET morphology outputs and emit a parity JSON report.
- `ValidateParityAbConsole`: Run baseline vs trained ambiguity-resolver parity A/B and emit a compact summary JSON.
- `TrainNerModel`: Train Turkish perceptron NER model and export text + compressed variants.
- `EvaluateNer`: Evaluate NER output from either a model run or a hypothesis file.
- `FindNamedEntities`: Run NER on plain text input.

## Examples

### Train a Classifier

```sh
dotnet run --project ZemberekDotNet.Apps/ZemberekDotNet.Apps.csproj -- TrainClassifier \
  --input data/news.train \
  --output model/news.bin \
  --learningRate 0.1 \
  --epochCount 50
```

### Evaluate a Classifier

```sh
dotnet run --project ZemberekDotNet.Apps/ZemberekDotNet.Apps.csproj -- EvaluateClassifier \
  --input data/news.test \
  --model model/news.bin
```

### Preprocess a Corpus

```sh
dotnet run --project ZemberekDotNet.Apps/ZemberekDotNet.Apps.csproj -- PreprocessTurkishCorpus \
  --input data/raw.txt \
  --output data/tokenized.txt \
  --operation TOKENIZED
```

Allowed preprocess operations:

- `TOKENIZED`
- `LEMMA`

### Train and Evaluate NER

```sh
dotnet run --project ZemberekDotNet.Apps/ZemberekDotNet.Apps.csproj -- TrainNerModel \
  --train data/ner-train.txt \
  --dev data/ner-dev.txt \
  --outputRoot out/ner
```

```sh
dotnet run --project ZemberekDotNet.Apps/ZemberekDotNet.Apps.csproj -- EvaluateNer \
  --reference data/ner-dev.txt \
  --modelRoot out/ner/model-compressed
```

### Interactive Morphology Console

```sh
dotnet run --project ZemberekDotNet.Apps/ZemberekDotNet.Apps.csproj -- MorphologyConsole
```

### Java vs .NET Parity (Single Run)

```sh
dotnet run --project ZemberekDotNet.Apps/ZemberekDotNet.Apps.csproj -- ValidateParityConsole \
  --input data/parity-input.txt \
  --java-output data/parity-java.tsv \
  --output out/parity-report.json
```

### Java vs .NET Parity (Baseline vs Trained A/B)

```sh
dotnet run --project ZemberekDotNet.Apps/ZemberekDotNet.Apps.csproj -- ValidateParityAbConsole \
  --input data/parity-input.txt \
  --java-output data/parity-java.tsv \
  --iterations 3 \
  --output out/parity-ab-report.json
```

Optional: provide `--java-jar` instead of `--java-output` to generate the TSV automatically.

## Notes

- Commands are discovered by class name. You can provide full class names or unique prefixes.
- Resource paths are resolved automatically in app startup, so commands can run from common working directories.
- For classification data format and workflow details, see [Classification Training Guide](classification-training-guide.md).
