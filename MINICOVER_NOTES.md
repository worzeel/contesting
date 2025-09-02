# MiniCover Steps

Steps to get stuff to work for minicover:

1. dotnet restore & dotnet build
2. minicover instrument
3. minicover reset
4. dotnet test --no-build
5. minicover uninstrument
6. minicover report / minicover htmlreport


Would be good to do these above steps, in code
* Maybe only copy relevant code for the console output maybe

# Notes about MiniCover code
Just some quick notes about what happens in the `MiniCover` code

## Trace modes

Output from running `-v Trace` during `minicover instrument`

```
Changing working directory to "/Users/worzeel/sandbox/MyTestApp/"
Hit services assembly location: "/Users/worzeel/.nuget/packages/minicover/3.8.1/tools/net9.0/any/MiniCover.HitServices.dll"
Checking assembly files [
  "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0/MyTestAppTests.dll"
]
  Assembly resolver search directories: [
    "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0"
  ]
  Assembly instrumented
  Temporary assembly file: "/var/folders/v3/p2yqjk7d01xdn5tl8z9r55k40000gn/T/minicover/de17417b-a42c-415c-a9c6-756921dcb0e8.dll"
  Temporary PDB file: "/var/folders/v3/p2yqjk7d01xdn5tl8z9r55k40000gn/T/minicover/de17417b-a42c-415c-a9c6-756921dcb0e8.pdb"
  Instrumenting assembly file "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0/MyTestAppTests.dll"
  PDB file: "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0/MyTestAppTests.pdb"
  Assembly backup file: "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0/MyTestAppTests.uninstrumented.dll"
  PDB backup file: "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0/MyTestAppTests.uninstrumented.pdb"
  Assembly directory: "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0"
Checking assembly files [
  "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0/MyTestApp.dll",
  "/Users/worzeel/sandbox/MyTestApp/MyTestApp/bin/Debug/net9.0/MyTestApp.dll"
]
  Assembly resolver search directories: [
    "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0"
  ]
  Assembly instrumented
  Temporary assembly file: "/var/folders/v3/p2yqjk7d01xdn5tl8z9r55k40000gn/T/minicover/dcae863b-2928-4cb8-b977-568e52b98128.dll"
  Temporary PDB file: "/var/folders/v3/p2yqjk7d01xdn5tl8z9r55k40000gn/T/minicover/dcae863b-2928-4cb8-b977-568e52b98128.pdb"
  Instrumenting assembly file "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0/MyTestApp.dll"
  PDB file: "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0/MyTestApp.pdb"
  Assembly backup file: "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0/MyTestApp.uninstrumented.dll"
  PDB backup file: "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0/MyTestApp.uninstrumented.pdb"
  Assembly directory: "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0"
  Instrumenting assembly file "/Users/worzeel/sandbox/MyTestApp/MyTestApp/bin/Debug/net9.0/MyTestApp.dll"
  PDB file: "/Users/worzeel/sandbox/MyTestApp/MyTestApp/bin/Debug/net9.0/MyTestApp.pdb"
  Assembly backup file: "/Users/worzeel/sandbox/MyTestApp/MyTestApp/bin/Debug/net9.0/MyTestApp.uninstrumented.dll"
  PDB backup file: "/Users/worzeel/sandbox/MyTestApp/MyTestApp/bin/Debug/net9.0/MyTestApp.uninstrumented.pdb"
  Assembly directory: "/Users/worzeel/sandbox/MyTestApp/MyTestApp/bin/Debug/net9.0"
Checking assembly files [
  "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0/NUnit3.TestAdapter.dll"
]
  Assembly resolver search directories: [
    "/Users/worzeel/sandbox/MyTestApp/MyTestAppTests/bin/Debug/net9.0"
  ]
  Source files has changed: [
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\AdapterSettings.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\CategoryList.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\Execution.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NavigationData.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NavigationDataProvider.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnit3TestDiscoverer.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnit3TestExecutor.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEventListener.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitTestAdapter.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitTestFilterBuilder.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\Seed.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\TestConverter.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\TestConverterForXml.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\TestLogger.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\TraitsFeature.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\TryParse.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\VsTestFilter.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\XmlHelper.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\TestFilterConverter\\TestFilterParser.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\TestFilterConverter\\TestFilterParserException.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\TestFilterConverter\\Tokenizer.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\DiscoveryConverter.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\DiscoveryException.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\Extensions.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\NUnitDiscoveryTestClasses.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\NUnitEngineAdapter.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\NUnitEventTestCase.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\NUnitResults.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\NUnitTestEvent.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\NUnitTestEventHeader.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\NUnitTestEventStartTest.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\NUnitTestEventSuiteFinished.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\NUnitTestEventTestCase.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\NUnitTestEventTestOutput.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\NUnitTestNode.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\NUnitEngine\\UnicodeEscapeHelper.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\Metadata\\DirectReflectionMetadataProvider.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\Metadata\\TypeInfo.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\Internal\\Extensions.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\Internal\\TimingLogger.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\Dump\\DumpXml.cs",
    "C:\\repos\\nunit\\nunit3-vs-adapter\\src\\NUnitTestAdapter\\Dump\\XmlNodeExtension.cs"
  ]
```


## Command line params

Using command line params like the following:
```bash
dotnet minicover instrument --sources "Konteiga/**/*.cs" --tests "KonteigaTests/**/*.cs" --exclude-sources "**/obj/**/*.cs" --exclude-tests "**/obj/**/*.cs"
```

### sources and tests

The `--sources` and `--tests` options, (along with `--exclude-sources` and `--exclude-tests` just seems to return back `IFileInfo[]` of all the files specified.

Uses `Microsoft.Extensions.FileSystemGlobbing.Matcher` to get the files specified
  * -> https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.filesystemglobbing.matcher?view=net-9.0-pp
  
  
## Hits

Within the code, there is a project `MiniCover.HitServices` which is the thing that seems to record the information about where code is hit on a test.

This information is stored within a binary file...
  * This could be instead put into a database of some sort (SQL Lite?)
