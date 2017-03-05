#r "./packages/build/FAKE/tools/FakeLib.dll"

open Fake
open Fake.FileUtils
open Fake.EnvironmentHelper
open Fake.AssemblyInfoFile
open System
open System.IO

let buildConfig = "Debug"

let deployDir = "./deploy/"


// Filesets
let appReferences  =
    !! "/**/*.csproj"
    ++ "/**/*.fsproj"

let packages = 
    !! "src/**/paket.template"

type ProjectInfo = 
    {
        ProjectName: string;
        ProjectPath: string;
        TemplatePath: string;
        Version: string;
    }

let getVersion templatePath =
    StringHelper.ReadFile templatePath
        |> Seq.where (fun line -> line.StartsWith("Version"))
        |> Seq.map (fun line -> line.Replace("Version", "").Trim())
        |> Seq.exactlyOne

let projects =
    packages
    |> Seq.map (fun templatePath -> 
        let templateDir = (directoryInfo templatePath)
        {
            ProjectName = Path.GetDirectoryName(templatePath);
            ProjectPath = templateDir.Parent.FullName;
            TemplatePath = templatePath;
            Version = getVersion(templatePath);
        }
    )


// Targets
Target "Clean" (fun _ ->
    CleanDirs [deployDir]
)

Target "Version" (fun _ ->
    for project in projects do
        CreateFSharpAssemblyInfo (project.ProjectPath @@ "AssemblyVersionInfo.fs")
            [
                Attribute.Version project.Version;
                Attribute.FileVersion project.Version
            ]
)

Target "Build" (fun _ ->
    MSBuild null "Build" ["Configuration", buildConfig] appReferences |> Log "AppBuild-Output: "
)

Target "Package" (fun _ ->
    for project in projects do
        Paket.Pack (fun p -> 
                { p with
                    BuildConfig = buildConfig;
                    Version = project.Version;
                    TemplateFile = project.TemplatePath;
                    WorkingDir = deployDir;
                    OutputPath = ".";
                    IncludeReferencedProjects = true;
                    BuildPlatform = "AnyCPU"
                }
            )
)

let executables =
    !! "./src/**/bin/Debug/*.exe"

Target "Run" (fun _ ->
    for executable in executables do
        logfn "Running: %s" executable
        System.Diagnostics.Process.Start(executable, "") |> ignore
)

// Build order
"Clean" ?=> "Build"
"Version" ?=> "Clean"
"Build" <== [ "Clean"; "Version" ]
"Run" <== [ "Version"; "Build" ]
"Package" <== [ "Clean"; "Version"; "Build"; ]

// start build
RunTargetOrDefault "Build"
