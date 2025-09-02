# Contesting

Name of the background test runner...

Short for "Constant testing"

Idea is that it will be running in the background looking for changes to files, and running appropriate tests.


Will intiially be just a console application that can do the following:

* Monitor a number of files
* Once a file is changed in said files run an executable
* The executable run will be the tests related to this file


Will need to store the following information:
* Files a test relates to
  * Maybe the testing framework can see this?
  * This can be done using the `coverlet` output
    * seems to be added by default to test projects now... so will use this for now...
* What part of files a test relates to


Create a very simple reader of the XML output from coverlet...

Just make it say what lines of code has coverage, and what lines do not have any coverage.

Watch for changes to the file, and record what lines were changed

Check to see if these lines relate to a specific test... if they do, then we re-run that test


Maybe look at https://github.com/lucaslorentz/minicover this might be what im looking for to map lines of code to unit tests, which i can then use

### minicover notes

`dotnet minicover instrument --sources "Konteiga/**/*.cs" --tests "KonteigaTests/**/*.cs" --exclude-sources "**/obj/**/*.cs" --exclude-tests "**/obj/**/*.cs"`


Example script on the readme:
```bash
dotnet restore
dotnet build

# Instrument
minicover instrument

# Reset hits
minicover reset

dotnet test --no-build

# Uninstrument
minicover uninstrument

# Create html reports inside folder coverage-html
minicover htmlreport --threshold 90

# Console report
minicover report --threshold 90
```

At the bottom of the readme it seems you can use it as a library... so I will try this way...
* seems it has to do a few steps, so this might not be ideal to keep executing...
* maybe I can rip out some of the code it uses, and not do as much if things seem too slow



## Other thoughts

Can this be built generically, so that it can be used for other tests too?

Which means I wonder if it should be done using another language other than C#?


Maybe have the engine done in Go / Rust, and have the add-ons be done in what ever language
- Although, maybe doesn't need different language? not sure...

Will try C# first, then look at how this can be modified to use Go lang for example after C#
version is working, and at that point maybe look at other languages?
