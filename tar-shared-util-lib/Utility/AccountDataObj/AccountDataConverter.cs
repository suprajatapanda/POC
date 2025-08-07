using System.Data;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace AccountDataObj
{
    public class AccountDataConverter
    {
        private static void removeReceivable(IXLWorksheet ws, string removeRECEIV)
        {
            if (removeRECEIV.Contains("1"))
            {
                // Remove last two columns
                int lastColumn = ws.LastColumnUsed()?.ColumnNumber() ?? 1;
                if (lastColumn > 1)
                {
                    ws.Column(lastColumn).Delete();
                    lastColumn = ws.LastColumnUsed()?.ColumnNumber() ?? 1;
                    if (lastColumn > 1)
                    {
                        ws.Column(lastColumn).Delete();
                    }
                }
            }
        }

        public static void ConvertCsvToXlsx(DataView dv, string inputFileCSV, string outputFileXlsx, string removeRECEIV)
        {
            string[] fileData = File.ReadAllLines(inputFileCSV);
            List<ReportInfo> ris = new();

            // *** Load Data
            string header = LoadData(fileData, ris);

            // *** Summarize records ***
            IEnumerable<ReportInfo> ris2 = SummarizeRecordsBySSN(ris);
            IEnumerable<ReportInfo> ris3 = SummarizeRecordsBySource(ris);
            IEnumerable<ReportInfo> ris4 = SummarizeRecordsByFund(ris);

            using (var workbook = new XLWorkbook())
            {
                // Create "By SSN" worksheet
                var ws1 = workbook.Worksheets.Add("By SSN");
                CreateOutputFile(ws1, ris2, header, dv);
                RemoveColumns(ws1, [(int)TheColumns.FundName, (int)TheColumns.FundNumber, (int)TheColumns.DivCode]);
                removeReceivable(ws1, removeRECEIV);

                // Create "By Source" worksheet
                var ws2 = workbook.Worksheets.Add("By Source");
                CreateOutputFile(ws2, ris3, header, dv);
                // Remove specified columns for the 2nd worksheet (by source)
                RemoveColumns(ws2, [
                    (int)TheColumns.FundName,
                    (int)TheColumns.FundNumber,
                    (int)TheColumns.DivCode,
                    (int)TheColumns.MiddleName,
                    (int)TheColumns.FirstName,
                    (int)TheColumns.LastName,
                    (int)TheColumns.SSN
                ]);
                removeReceivable(ws2, removeRECEIV);

                // Create "Loan Info." worksheet if we have loan information
                if (ris4.Any())
                {
                    var ws3 = workbook.Worksheets.Add("Loan Info.");
                    CreateOutputFile(ws3, ris4, header, dv);
                    RemoveColumns(ws3, [(int)TheColumns.FundName, (int)TheColumns.FundNumber, (int)TheColumns.DivCode]);
                    removeReceivable(ws3, removeRECEIV);
                }

                // Create "Account Data" worksheet
                var ws4 = workbook.Worksheets.Add("Account Data");
                CreateOutputFile(ws4, ris, header, dv);
                removeReceivable(ws4, removeRECEIV);

                // Handle file format and size limitations
                if (Path.GetExtension(outputFileXlsx).ToLower() == ".xls")
                {
                    if (fileData.Length > 65533)
                    {
                        outputFileXlsx = Path.ChangeExtension(outputFileXlsx, "xlsx");
                        workbook.SaveAs(outputFileXlsx);
                        throw new Exception("Maximum number of rows in XLS file is 65536. Given file has " + fileData.Length + " rows and is converted to .XLSX.");
                    }
                }

                workbook.SaveAs(outputFileXlsx);
            }
        }

        private static void RemoveColumns(IXLWorksheet ws, int[] columnIndices)
        {
            // Sort in descending order to remove from right to left
            var sortedIndices = columnIndices.OrderByDescending(x => x).ToArray();

            foreach (int columnIndex in sortedIndices)
            {
                // Add 1 because ClosedXML uses 1-based indexing
                if (columnIndex + 1 <= ws.LastColumnUsed()?.ColumnNumber())
                {
                    ws.Column(columnIndex + 1).Delete();
                }
            }
        }

        private static string LoadData(IEnumerable<string> fileData, ICollection<ReportInfo> ris)
        {
            Regex reg = new("\\0");

            int counter = 0;
            string header = string.Empty;
            var iErrCnt = 0;
            var sbErrs = new System.Text.StringBuilder();

            foreach (string s in fileData)
            {
                try
                {
                    string results = reg.Replace(s, string.Empty);
                    if (counter++ == 0)
                    {
                        header = results;
                        continue;
                    }

                    results = results.Replace("=", string.Empty);//for isc. remove this after this is handled in service layer
                    // Replace "," in between double quotes with <comma>
                    Regex regComma = new("\\\".*?\\\"");

                    foreach (Match m in regComma.Matches(results))
                    {
                        results = results.Replace(m.Value, m.Value.Replace(",", "<comma>"));
                    }
                    string[] sa = results.Replace("\"", string.Empty).Split(',');

                    int col = 10;

                    //'*** Load Report Data ****
                    ReportInfo ri = new()
                    {
                        SSN = sa[(int)TheColumns.SSN].Trim(),
                        FirstName = sa[(int)TheColumns.FirstName].Trim().Replace("<comma>", ",").Trim(),
                        MiddleName = sa[(int)TheColumns.MiddleName].Trim(),
                        LastName = sa[(int)TheColumns.LastName].Trim().Replace("<comma>", ",").Trim(),
                        StartDate = sa[(int)TheColumns.StartDate].Trim(),
                        EndDate = sa[(int)TheColumns.EndDate].Trim(),
                        DivCode = sa[(int)TheColumns.DivCode].Trim(),
                        FundNumber = Convert.ToInt32(sa[(int)TheColumns.FundNumber].Trim()),
                        Source = sa[(int)TheColumns.Source].Trim(),
                        FundName = sa[(int)TheColumns.FundName].Trim(),
                        Fields =
                        [
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col++]),
                            Convert.ToDouble(sa[col])
                        ]
                    };

                    ris.Add(ri);
                }
                catch (Exception ex)
                {
                    var lineNo = ris.Count + iErrCnt + 1;
                    sbErrs.AppendLine("Row " + lineNo.ToString() + " conversion failed with this exception: " + ex.Message);
                    iErrCnt++;
                }
            }

            if (sbErrs.Length > 0)
            {
                sbErrs.Insert(0, iErrCnt + " error(s) in conversion.\r\n");
                throw new Exception(sbErrs.ToString());
            }

            return header;
        }

        private static IEnumerable<ReportInfo> SummarizeRecordsBySSN(IEnumerable<ReportInfo> ris)
        {
            //Find match
            IEnumerable<ReportInfo> ris2 =
                from r in ris
                group r by new { r.SSN, r.FirstName, r.MiddleName, r.LastName, r.StartDate, r.EndDate, r.Source }
                    into grp
                select new ReportInfo
                {
                    SSN = grp.Key.SSN,
                    FirstName = grp.Key.FirstName,
                    MiddleName = grp.Key.MiddleName,
                    LastName = grp.Key.LastName,
                    StartDate = grp.Key.StartDate,
                    EndDate = grp.Key.EndDate,
                    Source = grp.Key.Source,
                    Fields =
                    [
                        grp.Sum(o => o.Fields[0]),
                        grp.Sum(o => o.Fields[1]),
                        grp.Sum(o => o.Fields[2]),
                        grp.Sum(o => o.Fields[3]),
                        grp.Sum(o => o.Fields[4]),
                        grp.Sum(o => o.Fields[5]),
                        grp.Sum(o => o.Fields[6]),
                        grp.Sum(o => o.Fields[7]),
                        grp.Sum(o => o.Fields[8]),
                        grp.Sum(o => o.Fields[9]),
                        grp.Sum(o => o.Fields[10]),
                        grp.Sum(o => o.Fields[11]),
                        grp.Sum(o => o.Fields[12]),
                        grp.Sum(o => o.Fields[13]),
                        grp.Sum(o => o.Fields[14]),
                        grp.Sum(o => o.Fields[15]),
                        grp.Sum(o => o.Fields[16]),
                        grp.Sum(o => o.Fields[17]),
                        grp.Sum(o => o.Fields[18]),
                        grp.Sum(o => o.Fields[19]),
                        grp.Sum(o => o.Fields[20])
                    ]
                };

            return ris2;
        }

        private static IEnumerable<ReportInfo> SummarizeRecordsBySource(IEnumerable<ReportInfo> ris)
        {
            IEnumerable<ReportInfo> ris2 =
                from r in ris
                group r by new { r.StartDate, r.EndDate, r.Source }
                    into grp
                select new ReportInfo
                {
                    StartDate = grp.Key.StartDate,
                    EndDate = grp.Key.EndDate,
                    Source = grp.Key.Source,
                    Fields =
                    [
                        grp.Sum(o => o.Fields[0]),
                        grp.Sum(o => o.Fields[1]),
                        grp.Sum(o => o.Fields[2]),
                        grp.Sum(o => o.Fields[3]),
                        grp.Sum(o => o.Fields[4]),
                        grp.Sum(o => o.Fields[5]),
                        grp.Sum(o => o.Fields[6]),
                        grp.Sum(o => o.Fields[7]),
                        grp.Sum(o => o.Fields[8]),
                        grp.Sum(o => o.Fields[9]),
                        grp.Sum(o => o.Fields[10]),
                        grp.Sum(o => o.Fields[11]),
                        grp.Sum(o => o.Fields[12]),
                        grp.Sum(o => o.Fields[13]),
                        grp.Sum(o => o.Fields[14]),
                        grp.Sum(o => o.Fields[15]),
                        grp.Sum(o => o.Fields[16]),
                        grp.Sum(o => o.Fields[17]),
                        grp.Sum(o => o.Fields[18]),
                        grp.Sum(o => o.Fields[19]),
                        grp.Sum(o => o.Fields[20])
                    ]
                };

            return ris2;
        }

        private static IEnumerable<ReportInfo> SummarizeRecordsByFund(IEnumerable<ReportInfo> ris)
        {
            IEnumerable<ReportInfo> ris2 =
                from r in ris
                where r.FundNumber == 669 | r.FundName == "LOAN"
                group r by new { r.SSN, r.FirstName, r.MiddleName, r.LastName, r.StartDate, r.EndDate, r.Source }
                    into grp
                select new ReportInfo
                {
                    SSN = grp.Key.SSN,
                    FirstName = grp.Key.FirstName,
                    MiddleName = grp.Key.MiddleName,
                    LastName = grp.Key.LastName,
                    StartDate = grp.Key.StartDate,
                    EndDate = grp.Key.EndDate,
                    Source = grp.Key.Source,
                    Fields =
                    [
                        grp.Sum(o => o.Fields[0]),
                        grp.Sum(o => o.Fields[1]),
                        grp.Sum(o => o.Fields[2]),
                        grp.Sum(o => o.Fields[3]),
                        grp.Sum(o => o.Fields[4]),
                        grp.Sum(o => o.Fields[5]),
                        grp.Sum(o => o.Fields[6]),
                        grp.Sum(o => o.Fields[7]),
                        grp.Sum(o => o.Fields[8]),
                        grp.Sum(o => o.Fields[9]),
                        grp.Sum(o => o.Fields[10]),
                        grp.Sum(o => o.Fields[11]),
                        grp.Sum(o => o.Fields[12]),
                        grp.Sum(o => o.Fields[13]),
                        grp.Sum(o => o.Fields[14]),
                        grp.Sum(o => o.Fields[15]),
                        grp.Sum(o => o.Fields[16]),
                        grp.Sum(o => o.Fields[17]),
                        grp.Sum(o => o.Fields[18]),
                        grp.Sum(o => o.Fields[19]),
                        grp.Sum(o => o.Fields[20])
                    ]
                };

            return ris2;
        }

        private static void CreateOutputFile(IXLWorksheet ws, IEnumerable<ReportInfo> ris, string header, DataView dv)
        {
            // *** Add table headers going cell by cell ***
            string[] sa = header.Split(',');
            int counter = 0;

            foreach (string s in sa)
            {
                var headerCell = ws.Cell(1, counter + 1); // ClosedXML uses 1-based indexing
                headerCell.Value = s;
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 204);
                headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerCell.Style.Border.OutsideBorderColor = XLColor.FromArgb(211, 211, 211);
                counter++;
            }

            // Freeze panes (equivalent to the original Panes setting)
            ws.SheetView.FreezeRows(1);

            //region *** Populate the data **
            int row = 2; // Start from row 2 (1-based indexing, row 1 is header)
            foreach (ReportInfo gri in ris)
            {
                // SSN column
                if (IsNumeric(gri.SSN))
                {
                    ws.Cell(row, (int)TheColumns.SSN + 1).Value = Convert.ToInt32(gri.SSN);
                }
                else
                {
                    ws.Cell(row, (int)TheColumns.SSN + 1).Value = gri.SSN;
                }
                ws.Cell(row, (int)TheColumns.SSN + 1).Style.NumberFormat.Format = "000-00-0000";

                ws.Cell(row, (int)TheColumns.FirstName + 1).Value = gri.FirstName;
                ws.Cell(row, (int)TheColumns.MiddleName + 1).Value = gri.MiddleName;
                ws.Cell(row, (int)TheColumns.LastName + 1).Value = gri.LastName;

                ws.Cell(row, (int)TheColumns.StartDate + 1).Value = gri.StartDate;
                ws.Cell(row, (int)TheColumns.EndDate + 1).Value = gri.EndDate;

                if (!string.IsNullOrWhiteSpace(gri.DivCode))
                {
                    ws.Cell(row, (int)TheColumns.DivCode + 1).Value = gri.DivCode;
                }

                if (gri.FundNumber != 0)
                {
                    ws.Cell(row, (int)TheColumns.FundNumber + 1).Value = gri.FundNumber;
                }

                if (!string.IsNullOrEmpty(gri.Source))
                {
                    dv.RowFilter = string.Format("acc_id = {0}", gri.Source);
                    if (dv.Count > 0)
                    {
                        ws.Cell(row, (int)TheColumns.Source + 1).Value = dv[0]["acc_name"]?.ToString() ?? "";
                    }
                }

                ws.Cell(row, (int)TheColumns.FundName + 1).Value = gri.FundName;

                // Fill the Fields data (columns 11-31 in 1-based indexing)
                for (int col = 11; col <= 31; col++)
                {
                    SetCol(ws, row, col, gri.Fields[col - 11]);
                }

                row++;
            }

            // Add Total Row
            ws.Row(row++).Hide(); // Leave a row space for sorting

            if (ws.Name == "By Source")
            {
                ws.Cell(row, 2).Value = "Total:"; // Column B
            }
            else
            {
                ws.Cell(row, 1).Value = "Total:"; // Column A
            }

            ws.Row(row).Style.Font.Bold = true;
            ws.Row(row).Style.Border.TopBorder = XLBorderStyleValues.Thick;
            ws.Row(row).Style.Border.TopBorderColor = XLColor.Black;

            // Calculate totals for each field column
            for (int col = 11; col <= 31; col++)
            {
                int col1 = col;
                SetCol(ws, row, col, ris.Sum(r => r.Fields[col1 - 11]));
            }

            // Auto fit all the columns
            for (int x = 1; x <= 31; x++)
            {
                if (x >= 11)
                {
                    ws.Column(x).AdjustToContents(1.0, 15.0); // Min 1.0, Max 15.0 width
                }
                else
                {
                    ws.Column(x).AdjustToContents();
                }
            }
        }

        private static bool IsNumeric(string value)
        {
            return int.TryParse(value, out _);
        }

        private static void SetCol(IXLWorksheet worksheet, int row, int col, double field)
        {
            if (field != 0)
            {
                worksheet.Cell(row, col).Value = Convert.ToDecimal(field);
                worksheet.Cell(row, col).Style.NumberFormat.Format = "_($* #,##0.00_);[Red]($* #,##0.00)";
            }
        }

        private sealed class ReportInfo
        {
            public string DivCode;
            public string EndDate;
            public double[] Fields;
            public string SSN;
            public string FirstName;
            public string MiddleName;
            public string LastName;
            public string Source;
            public int FundNumber;
            public string FundName;
            public string StartDate;
        }

        public enum TheColumns
        {
            SSN = 0,
            LastName,
            FirstName,
            MiddleName,
            StartDate,
            EndDate,
            DivCode,
            FundNumber,
            Source,
            FundName
        }
    }
}