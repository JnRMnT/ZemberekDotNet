# Developer Guide (.NET Port)

This page ports Java wiki “Zemberek For Developers” into ZemberekDotNet workflows.

## 1. Requirements

- .NET SDK 8.0 (recommended)
- Git
- Optional IDE: Visual Studio 2022 or VS Code

## 2. Clone the Repository

```sh
git clone https://github.com/JnRMnT/ZemberekDotNet.git
cd ZemberekDotNet
```

If you plan to contribute, fork first and clone your fork.

## 3. Restore, Build, Test

```sh
dotnet restore ZemberekDotNet.sln
dotnet build ZemberekDotNet.sln -c Release
dotnet test ZemberekDotNet.sln -c Release
```

Notes:

- Most library projects target netstandard2.1.
- Test projects target net8.0.

## 4. Package Generation

Library projects use GeneratePackageOnBuild, so build will produce nupkg outputs for package-enabled projects.

You can also explicitly pack:

```sh
dotnet pack ZemberekDotNet.sln -c Release
```

## 5. CI Parity

CI pipeline reference:

- [azure-pipelines.yml](../azure-pipelines.yml)

Pipeline steps are effectively:

1. Restore
2. Build
3. Test with code coverage
4. Collect and publish nupkg artifacts

To mirror CI locally:

```sh
dotnet restore ZemberekDotNet.sln
dotnet build ZemberekDotNet.sln -c Release
dotnet test ZemberekDotNet.sln -c Release --collect "Code coverage" --logger "trx"
```

## 6. Solution Structure

Core modules are organized as separate projects (Core, Morphology, Tokenization, Normalization, NER, Classification, LangID, LM) with corresponding test projects.

Entry references:

- [ZemberekDotNet.sln](../ZemberekDotNet.sln)
- [README.md](../README.md)

## 7. Working with Dictionaries and Morphology Data

Dictionary and lexicon behavior is implemented in morphology module.
If you change dictionary-line rules or lexicon parsing behavior, update:

1. implementation code
2. related tests
3. docs pages (especially text dictionary rules)

Related pages:

- [Text Dictionary Rules (.NET Port)](text-dictionary-rules.md)
- [Morphology Notes (.NET Port)](morphology-notes.md)

## 8. Documentation Maintenance Workflow

For Java wiki parity maintenance work:

1. Identify source Java page changes.
2. Update the equivalent docs page under docs.
3. Adapt examples to current .NET API names.
4. Keep checklist status and notes in sync.

Tracking page:

- [Java Wiki Porting Checklist](java-wiki-porting-checklist.md)

## 9. Current Scope Decision

- gRPC module/docs are deferred for future.
- Current port-completion scope prioritizes core libraries, tests, examples, and documentation parity for these modules.
