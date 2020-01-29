namespace Store.Reports.Formats

open NPOI.XSSF.UserModel
open System
open System.Collections.Generic
open Newtonsoft.Json
open System.IO
open System.Data

type Excel() =

  member private this.generateHeader(dt: DataTable, sheet: NPOI.SS.UserModel.ISheet) =
    let headerRow = sheet.CreateRow(0)
    for col in dt.Columns do
      let c = headerRow.CreateCell(col.Ordinal)
      c.SetCellValue(col.ColumnName.ToUpper())

  member this.export<'A> (data: IEnumerable<'A>, fileName: string, ?maybeSheetName: string): MemoryStream =
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

    let sheet = wb.CreateSheet(sheetName)
    
    let j = JsonConvert.SerializeObject(data)
    let dt: DataTable = JsonConvert.DeserializeObject<DataTable>(j)

    this.generateHeader(dt, sheet)

    for i in 0 .. dt.Rows.Count do
      let row = dt.Rows.[i]
      let r = sheet.CreateRow(i + 1)
      for col in dt.Columns do
        let c = r.CreateCell(col.Ordinal)
        let v = Convert.ToString(row.[col.Ordinal])
        c.SetCellValue(v)

    use stream = new MemoryStream()
    wb.Write(stream)

    stream
