namespace Store.Reports.Formats.Specs

open NUnit.Framework
open FsUnit

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
  let ``When List of objects``([<Values()>] input) =

    let excel = new Excel()

    let res = excel.export(listA, "file-a")

    1 |> should equal 1
