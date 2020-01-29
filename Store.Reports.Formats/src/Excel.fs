namespace Store.Reports.Formats

open System
open System.Collections.Generic
open System.IO
open System.Data
open Newtonsoft.Json
open NPOI.XSSF.UserModel

type Excel() =

  member private this.generateHeader(dt: DataTable, sheet: NPOI.SS.UserModel.ISheet) =
    let headerRow = sheet.CreateRow(0)
    for col in dt.Columns do
      let c = headerRow.CreateCell(col.Ordinal)
      c.SetCellValue(col.ColumnName.ToUpper())

  member private this.generateSheet(dt: DataTable, sheet: NPOI.SS.UserModel.ISheet) =
    for i in 0 .. (dt.Rows.Count - 1) do
      let row = dt.Rows.[i]
      let r = sheet.CreateRow(i + 1)
      for col in dt.Columns do
        let c = r.CreateCell(col.Ordinal)
        let v = Convert.ToString(row.[col.Ordinal])
        c.SetCellValue(v)

  member private this.workbooktoFileStream(wb: XSSFWorkbook, tmpFile: string) =
    try
      use fs = new FileStream(tmpFile, FileMode.Create, FileAccess.Write)
      wb.Write(fs)
      fs.Close()
    with
      | e -> printfn "%s" e.Message

  member private this.fileToMemoryStream(tmpFile: string): MemoryStream =
    let ms = new MemoryStream()
    try
      use fs1 = new FileStream(tmpFile, FileMode.Open, FileAccess.Read)
      fs1.CopyTo(ms)
    with
      | e -> printfn "%s" e.Message

    ms

  member this.listToWorkbook<'A> (data: IEnumerable<'A>, ?maybeSheetName: string): Stream =
    let sheetName =
      match maybeSheetName with
        | a when a.IsSome -> maybeSheetName.Value
        | _ -> "Sheet1"

    let wb = new XSSFWorkbook()
    let sheet = wb.CreateSheet(sheetName)
    let headerStyle = wb.CreateCellStyle()
    let headerFont = wb.CreateFont();
    headerFont.IsBold <- true;
    headerStyle.SetFont(headerFont)

    let j = JsonConvert.SerializeObject(data)
    let dt: DataTable = JsonConvert.DeserializeObject<DataTable>(j)

    this.generateHeader(dt, sheet)
    this.generateSheet(dt, sheet)

    let gid = Guid.NewGuid().ToString()

    // POI implicitly closes stream after writing.  So we first write to disk, then read it back.
    let tmpFile = Environment.CurrentDirectory + "/" + gid

    this.workbooktoFileStream(wb, tmpFile)
    let ms = this.fileToMemoryStream(tmpFile)

    File.Delete(tmpFile)

    ms :> Stream
