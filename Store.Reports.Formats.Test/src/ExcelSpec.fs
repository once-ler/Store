namespace Store.Reports.Formats.Test

open NUnit.Framework
open FsUnit

open System.IO
open System.Dynamic
open FSharp.Interop.Dynamic
open Store.Storage.SqlServer


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

  [<Test>]
  let ``When Result from SQL``([<Values(1)>] input) =
    let dbcontext = new Store.Models.DBContext ( server = "127.0.0.1", database = "test", userId = "admin", password = "12345678" )
    let sqlClient = new Client(dbcontext)
    let sql = "select epic_id, crms2_id, epic_description, crms_description from epic.race"
    let list = sqlClient.runSqlDynamic(sql)
              
    let excel = new Excel()
    let sheetName = "More Important Stuff"
    let tmpFile = "testbuild/testsqlout.xlsx"
    use stream = excel.listToWorkbook(list, sheetName)
    use fs = new FileStream(tmpFile, FileMode.Create, FileAccess.Write)
    stream.CopyTo(fs)

    printfn "%A" list
    stream.Length |> should greaterThan 0


