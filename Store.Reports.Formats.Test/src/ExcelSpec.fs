namespace Store.Reports.Formats.Test

open NUnit.Framework
open FsUnit

open System
open System.IO
open System.Dynamic
open FSharp.Interop.Dynamic

module ``Excel Tests`` =

  open Store.Reports.Formats

  let listA = [ 
    for i in 0 .. 9 do
      let o = new ExpandoObject()
      o?id <- i
      o?name <- "ABC"
      yield o
    ]

  [<Test>]
  let ``When List of objects``([<Values(1)>] input) =

    let excel = new Excel()
    let sheetName = "Important Stuff"
    let tmpFile = "testbuild/testout.xlsx"
    use stream = excel.listToWorkbook(listA, sheetName)
    use fs = new FileStream(tmpFile, FileMode.Create, FileAccess.Write)
    stream.CopyTo(fs)

    printfn "%A" listA
    stream.Length |> should greaterThan 0

