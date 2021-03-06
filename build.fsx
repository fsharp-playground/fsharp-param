
#I "packages/FAKE/tools/"
#I "packages/ICSharpCode.Decompiler/lib/net40/"
#I "packages/ICSharpCode.NRefactory/lib/net40/"
#I "packages/Mono.Cecil/lib/net40"

#r "ICSharpCode.Decompiler"
#r "Mono.Cecil.dll"
#r "packages/FAKE/tools/FakeLib.dll"

open Fake.FscHelper
open Fake
open Fake.FileHelper
open Fake.CscHelper
open ICSharpCode.Decompiler.Ast
open ICSharpCode.Decompiler
open System
open System.Text
open System.IO

let prepare() =
    CreateDir "release"
    "packages/FAKE/tools/FSharp.Core.dll" |> CopyFile "release/FSharp.Core.dll"

Target "decompile" (fun _ ->
        try
            let pathToAssembly = "release/lib.dll"
            let def = Mono.Cecil.AssemblyDefinition.ReadAssembly(pathToAssembly)
            let dc = AstBuilder(DecompilerContext(def.MainModule))
            dc.AddAssembly(def);

            let output = new StringWriter()
            dc.GenerateCode(PlainTextOutput(output))
            let str = output.ToString()
            File.WriteAllText ("release/source.cs", str)
        with err -> printfn "%A" err
)

Target "csharp" (fun _ ->
        ["src/cs/Program.cs"]
        |> Csc(fun p ->
            { p with
                Output = "release/program.exe"
                OtherParams = ["/reference:release/lib.dll"]
                Target = CscTarget.Exe })
)

Target "fsharp" (fun _ ->
        ["src/Param.fsx"]
        |> Fsc( fun p ->
            { p with
                Output = "release/lib.dll"
                FscTarget = FscTarget.Library} )
    )

"fsharp"
    ==> "csharp"
    ==> "decompile"

prepare()
//RunTargetOrDefault fsi.CommandLineArgs.[1]
RunTargetOrDefault "decompile"

(*
[<EntryPoint>]
let go args =
    RunTargetOrDefault args.[1]
    0
*)