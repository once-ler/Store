// include Fake libs
// https://stackoverflow.com/questions/39726728/f-unit-test-projects-in-linux-with-mono-fake-nunit-3
// https://stackoverflow.com/questions/41815649/f-how-to-setup-fake-project-that-can-use-fsunit
// Running FAKE:
// chmod a+x ../packages/FAKE.4.64.17/tools/FAKE.exe
// ./packages/FAKE.4.64.17/tools/FAKE.exe build.fsx
#r "./packages/FAKE.4.64.17/tools/FakeLib.dll"

open System
open Fake
open Fake.Testing // NUnit3 is in here

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

// RestorePackages()

// Directories
let buildDir  = "./build/"
let testDir = "./testbuild/"

// version info
let version = "0.1"  // or retrieve from CI server

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir]
)

Target "Build" (fun _ ->
    //MSBuildDebug buildDir "Build" appReferences
    !! "Store.*/**/*.fsproj"
    ++ "Store.*/**/*.csproj"
    -- "Store.*.Test/**/*.fsproj"
    -- "Store.*.Test/**/*.csproj"
    |> MSBuildRelease buildDir "Build"
    |> Log "AppBuild-Output: "
)

Target "BuildTest" (fun _ ->
    !! "Store.Reports.Formats.Test/*.fsproj"
    |> MSBuildDebug testDir "Build"
    |> Log "TestBuild-Output: "
)

// Build
// /usr/bin/msbuild  /apps/Store/Store.Reports.Formats.Test/Store.Reports.Formats.Test.fsproj /t:Build    /p:RestorePackages="False" /p:OutputPath="/apps/Store/testbuild" /p:Configuration="Debug" /logger:Fake.MsBuildLogger+ErrorLogger,"/apps/Store/packages/FAKE.4.64.17/tools/FakeLib.dll"

// Test
// ./packages/NUnit.ConsoleRunner.3.10.0/tools/nunit3-console.exe "--noheader" "--result=./testbuild/TestResults.xml" "/apps/Store/testbuild/Store.Reports.Formats.Test.dll"

Target "Test" (fun _ ->
     !! (testDir + "/Store.Reports.Formats.Test.dll")
     |> NUnit3 (fun p ->
         { p with
             ToolPath = "./packages/NUnit.ConsoleRunner.3.10.0/tools/nunit3-console.exe"
             ShadowCopy = false;
             ResultSpecs = [testDir + "TestResults.xml"]
             })
)

let files includes = 
  { BaseDirectory = __SOURCE_DIRECTORY__
    Includes = includes
    Excludes = [] }

Target "Test2" (fun _ ->
    !! "/tests*.dll" |> NUnit (fun p ->
        {p with
           DisableShadowCopy = true;
           OutputFile = testDir + "TestResults.xml" })
)

Target "Default" (fun _ -> trace "Completed FAKE.")

"Clean" ==> "Build" ==> "BuildTest" ==> "Test" ==> "Default"

RunTargetOrDefault "Default"
