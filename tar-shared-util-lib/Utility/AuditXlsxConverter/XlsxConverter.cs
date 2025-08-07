using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace AuditXlsxConverter
{
    public class XlsxConverter
    {
        private sealed class ReportInfo
        {
            public string Birthday;
            public string ContractID;
            public string EndDate;
            public double[] Fields;
            public string FirstName;
            public string FundNum;
            public string HireDate;
            public string Investment;
            public string LastName;
            public string Location;
            public string RehireDate;
            public string SSN;
            public string SourceName;
            public string SourceNum;
            public string StartDate;
            public string SubID;
            public string TermDate;
        }

        public static void ConvertCsvToXlsx(string inputFileCSV, string outputFileXlsx)
        {
            string[] fileData = File.ReadAllLines(inputFileCSV);
            List<ReportInfo> ris = new();
            string header = LoadData(fileData, ris);
            IEnumerable<ReportInfo> ris2 = SummarizeRecordsBySSN(ris);
            IEnumerable<ReportInfo> ris3 = SummarizeRecordsBySource(ris);
            IEnumerable<ReportInfo> enumerable = SummarizeRecordsByFund(ris);
            IEnumerable<ReportInfo> enumerable2 = SummarizeRecordsByLoan(ris);
            IEnumerable<ReportInfo> enumerable3 = SummarizeRecordsByForf(ris);
            IEnumerable<ReportInfo> enumerable4 = SummarizeRecordsByForfbyPPT(ris);

            using (var workbook = new XLWorkbook())
            {
                // Set default font for the workbook
                workbook.Style.Font.FontName = "Calibri";
                workbook.Style.Font.FontSize = 11;

                // Create "Data" worksheet
                var ws1 = workbook.Worksheets.Add("Data");
                CreateOutputFile(ws1, ris, header);
                RemoveColumns(ws1, new[] { 35, 29 }); // Remove columns 34 and 28 (0-based to 1-based conversion)

                // Create "Plan by Source" worksheet
                var ws2 = workbook.Worksheets.Add("Plan by Source");
                CreateOutputFile(ws2, ris3, header);
                RemoveColumns(ws2, new[] { 36, 35, 29, 27, 16, 15, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 });

                // Create "Plan by Fund" worksheet if we have fund data
                if (enumerable.Any())
                {
                    var ws3 = workbook.Worksheets.Add("Plan by Fund");
                    CreateOutputFile(ws3, enumerable, header);
                    RemoveColumns(ws3, new[] { 36, 35, 29, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 });
                }

                // Create "PPT Loan" worksheet if we have loan data
                if (enumerable2.Any())
                {
                    var ws4 = workbook.Worksheets.Add("PPT Loan");
                    CreateOutputFile(ws4, enumerable2, header);
                    RemoveColumns(ws4, new[] { 35, 29, 27, 16, 15, 12, 11, 10, 9, 5, 4, 3 });
                }

                // Create "PPT by Source" worksheet
                var ws5 = workbook.Worksheets.Add("PPT by Source");
                CreateOutputFile(ws5, ris2, header);
                RemoveColumns(ws5, new[] { 35, 29, 27, 16, 15, 12, 11, 10, 9, 5, 4, 3 });

                // Save the workbook
                workbook.SaveAs(outputFileXlsx);
            }
        }

        private static void RemoveColumns(IXLWorksheet ws, int[] columnIndices)
        {
            // Sort in descending order to remove from right to left
            var sortedIndices = columnIndices.OrderByDescending(x => x).ToArray();

            foreach (int columnIndex in sortedIndices)
            {
                if (columnIndex <= ws.LastColumnUsed()?.ColumnNumber())
                {
                    ws.Column(columnIndex).Delete();
                }
            }
        }

        private static string LoadData(IEnumerable<string> fileData, ICollection<ReportInfo> ris)
        {
            Regex regex = new("\\0");
            int num = 0;
            string result = string.Empty;

            foreach (string fileDatum in fileData)
            {
                string text = regex.Replace(fileDatum, string.Empty);
                if (num++ == 0)
                {
                    result = text;
                    continue;
                }

                Regex regex2 = new("\\\".*?\\\"");
                foreach (Match item2 in regex2.Matches(text))
                {
                    text = text.Replace(item2.Value, item2.Value.Replace(",", "<comma>"));
                }

                string[] array = text.Split(',');
                if (!array[5].Trim().Contains("F"))
                {
                    int num2 = 16;
                    ReportInfo reportInfo = new();
                    reportInfo.ContractID = array[0].Trim().Replace("=", "").Replace("\"", "");
                    reportInfo.SubID = array[1].Trim().Replace("=", "").Replace("\"", "");
                    reportInfo.StartDate = array[2].Trim().Replace("\"", "");
                    reportInfo.EndDate = array[3].Trim().Replace("=", "").Replace("\"", "");
                    reportInfo.Location = array[4].Trim().Replace("<comma>", ",").Replace("\"", "").Replace("=", "");
                    reportInfo.SSN = array[5].Trim().Replace("=", "").Replace("\"", "");
                    reportInfo.FirstName = array[6].Trim().Replace("<comma>", ",").Replace("\"", "");
                    reportInfo.LastName = array[7].Trim().Replace("<comma>", ",").Replace("\"", "");
                    reportInfo.Birthday = array[8].Trim().Replace("\"", "");
                    reportInfo.HireDate = array[9].Trim().Replace("\"", "");
                    reportInfo.TermDate = array[10].Trim().Replace("\"", "");
                    reportInfo.RehireDate = array[11].Trim().Replace("\"", "");
                    reportInfo.SourceNum = array[12].Trim().Replace("=", "").Replace("\"", "");
                    reportInfo.SourceName = array[13].Trim().Replace("\"", "");
                    reportInfo.FundNum = array[14].Trim().Replace("\"", "");
                    reportInfo.Investment = array[15].Trim().Replace("\"", "");
                    reportInfo.Fields = new double[]
                    {
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2++].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "")),
                        ConvertToDouble(array[num2].Replace("($", "-").Replace("$", "").Replace(")", "").Replace("\\", "").Replace("\"", "").Replace("%", ""))
                    };
                    ris.Add(reportInfo);
                }
            }
            return result;
        }

        private static double ConvertToDouble(string s)
        {
            if (string.IsNullOrEmpty(s.Trim()))
            {
                return 0.0;
            }
            s = s.Replace("<comma>", "");
            return Convert.ToDouble(s);
        }

        private static IEnumerable<ReportInfo> SummarizeRecordsBySSN(IEnumerable<ReportInfo> ris)
        {
            return from r in ris
                   orderby r.SubID, r.SSN, r.SourceNum
                   group r by new { r.ContractID, r.SubID, r.SSN, r.LastName, r.FirstName, r.SourceNum, r.SourceName } into grp
                   select new ReportInfo
                   {
                       ContractID = grp.Key.ContractID,
                       SubID = grp.Key.SubID,
                       SSN = grp.Key.SSN,
                       LastName = grp.Key.LastName,
                       FirstName = grp.Key.FirstName,
                       SourceNum = grp.Key.SourceNum,
                       SourceName = grp.Key.SourceName,
                       Fields = new double[]
                       {
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
                           grp.Min(o => o.Fields[19])
                       }
                   };
        }

        private static IEnumerable<ReportInfo> SummarizeRecordsBySource(IEnumerable<ReportInfo> ris)
        {
            return from r in ris
                   orderby r.SourceNum
                   group r by new { r.ContractID, r.SourceNum, r.SourceName } into grp
                   select new ReportInfo
                   {
                       ContractID = grp.Key.ContractID,
                       SourceNum = grp.Key.SourceNum,
                       SourceName = grp.Key.SourceName,
                       Fields = new double[]
                       {
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
                           grp.Min(o => o.Fields[19])
                       }
                   };
        }

        private static IEnumerable<ReportInfo> SummarizeRecordsByFund(IEnumerable<ReportInfo> ris)
        {
            return from r in ris
                   orderby r.FundNum
                   group r by new { r.ContractID, r.FundNum, r.Investment } into grp
                   select new ReportInfo
                   {
                       ContractID = grp.Key.ContractID,
                       FundNum = grp.Key.FundNum,
                       Investment = grp.Key.Investment,
                       Fields = new double[]
                       {
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
                           grp.Min(o => o.Fields[19])
                       }
                   };
        }

        private static IEnumerable<ReportInfo> SummarizeRecordsByLoan(IEnumerable<ReportInfo> ris)
        {
            return from r in ris
                   where r.Investment.ToUpper() == "LOAN FUND"
                   orderby r.SubID, r.SSN
                   group r by new { r.ContractID, r.SubID, r.SSN, r.LastName, r.FirstName, r.SourceNum, r.SourceName, r.FundNum, r.Investment } into grp
                   select new ReportInfo
                   {
                       ContractID = grp.Key.ContractID,
                       SubID = grp.Key.SubID,
                       SSN = grp.Key.SSN,
                       LastName = grp.Key.LastName,
                       FirstName = grp.Key.FirstName,
                       SourceNum = grp.Key.SourceNum,
                       SourceName = grp.Key.SourceName,
                       FundNum = grp.Key.FundNum,
                       Investment = grp.Key.Investment,
                       Fields = new double[]
                       {
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
                           grp.Min(o => o.Fields[19])
                       }
                   };
        }

        private static IEnumerable<ReportInfo> SummarizeRecordsByForf(IEnumerable<ReportInfo> ris)
        {
            return from r in ris
                   where r.SSN.Contains("F")
                   orderby r.SourceNum
                   group r by new { r.SourceNum, r.SourceName } into grp
                   select new ReportInfo
                   {
                       SourceNum = grp.Key.SourceNum,
                       SourceName = grp.Key.SourceName,
                       Fields = new double[]
                       {
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
                           grp.Min(o => o.Fields[19])
                       }
                   };
        }

        private static IEnumerable<ReportInfo> SummarizeRecordsByForfbyPPT(IEnumerable<ReportInfo> ris)
        {
            return from r in ris
                   where r.Fields[8] != 0.0
                   orderby r.SourceNum
                   group r by new { r.SourceNum, r.SourceName } into grp
                   select new ReportInfo
                   {
                       SourceNum = grp.Key.SourceNum,
                       SourceName = grp.Key.SourceName,
                       Fields = new double[]
                       {
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
                           grp.Min(o => o.Fields[19])
                       }
                   };
        }

        private static void CreateOutputFile(IXLWorksheet ws, IEnumerable<ReportInfo> ris, string header)
        {
            string[] array = header.Split(',');
            int num = 0;

            // Create headers
            foreach (string value in array)
            {
                var headerCell = ws.Cell(1, num + 1); // ClosedXML uses 1-based indexing
                headerCell.Value = value.ToString();
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 204);
                headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerCell.Style.Border.OutsideBorderColor = XLColor.FromArgb(211, 211, 211);
                num++;
            }

            // Freeze first row
            ws.SheetView.FreezeRows(1);

            // Set header row properties
            ws.Row(1).Height = 30; // Convert from 600 to points (600/20)
            ws.Row(1).Style.Alignment.WrapText = true;
            ws.Row(1).AdjustToContents();

            int rowNum = 2; // Start from row 2 (1-based indexing, row 1 is header)
            foreach (ReportInfo ri in ris)
            {
                ws.Cell(rowNum, 1).Value = ri.ContractID?.ToString() ?? "";
                ws.Cell(rowNum, 2).Value = ri.SubID?.ToString() ?? "";

                // Handle date conversions with null checks
                if (!string.IsNullOrEmpty(ri.StartDate))
                {
                    try
                    {
                        ws.Cell(rowNum, 3).Value = Convert.ToDateTime(ri.StartDate).ToString("MM/dd/yyyy");
                    }
                    catch
                    {
                        ws.Cell(rowNum, 3).Value = ri.StartDate;
                    }
                }

                if (!string.IsNullOrEmpty(ri.EndDate))
                {
                    try
                    {
                        ws.Cell(rowNum, 4).Value = Convert.ToDateTime(ri.EndDate).ToString("MM/dd/yyyy");
                    }
                    catch
                    {
                        ws.Cell(rowNum, 4).Value = ri.EndDate;
                    }
                }

                ws.Cell(rowNum, 5).Value = ri.Location?.ToString() ?? "";

                // Handle SSN
                if (IsNumeric(ri.SSN))
                {
                    ws.Cell(rowNum, 6).Value = Convert.ToInt32(ri.SSN);
                }
                else
                {
                    ws.Cell(rowNum, 6).Value = ri.SSN?.ToString() ?? "";
                }
                ws.Cell(rowNum, 6).Style.NumberFormat.Format = "000-00-0000";

                ws.Cell(rowNum, 7).Value = ri.FirstName?.ToString() ?? "";
                ws.Cell(rowNum, 8).Value = ri.LastName?.ToString() ?? "";

                // Handle Birthday
                if (!string.IsNullOrEmpty(ri.Birthday?.Trim()))
                {
                    try
                    {
                        ws.Cell(rowNum, 9).Value = Convert.ToDateTime(ri.Birthday).ToString("MM/dd/yyyy");
                    }
                    catch
                    {
                        ws.Cell(rowNum, 9).Value = ri.Birthday;
                    }
                }

                // Handle HireDate
                if (!string.IsNullOrEmpty(ri.HireDate?.Trim()))
                {
                    try
                    {
                        ws.Cell(rowNum, 10).Value = Convert.ToDateTime(ri.HireDate).ToString("MM/dd/yyyy");
                    }
                    catch
                    {
                        ws.Cell(rowNum, 10).Value = ri.HireDate;
                    }
                }

                // Handle TermDate
                if (!string.IsNullOrEmpty(ri.TermDate?.Trim()))
                {
                    try
                    {
                        ws.Cell(rowNum, 11).Value = Convert.ToDateTime(ri.TermDate).ToString("MM/dd/yyyy");
                    }
                    catch
                    {
                        ws.Cell(rowNum, 11).Value = ri.TermDate;
                    }
                }

                // Handle RehireDate
                if (!string.IsNullOrEmpty(ri.RehireDate?.Trim()))
                {
                    try
                    {
                        ws.Cell(rowNum, 12).Value = Convert.ToDateTime(ri.RehireDate).ToString("MM/dd/yyyy");
                    }
                    catch
                    {
                        ws.Cell(rowNum, 12).Value = ri.RehireDate;
                    }
                }

                ws.Cell(rowNum, 13).Value = ri.SourceNum?.ToString() ?? "";
                ws.Cell(rowNum, 14).Value = ri.SourceName?.ToString() ?? "";
                ws.Cell(rowNum, 15).Value = ri.FundNum?.ToString() ?? "";
                ws.Cell(rowNum, 16).Value = ri.Investment?.ToString() ?? "";

                // Fill the Fields data (columns 17-35 in 1-based indexing)
                for (int j = 17; j <= 36; j++)
                {
                    if (j != 29) // Skip column 28 in 0-based (29 in 1-based)
                    {
                        SetCol(ws, rowNum, j, ri.Fields[j - 17]);
                    }
                }

                // Set the last field (Fields[19]) in column 36
                ws.Cell(rowNum, 36).Value = ri.Fields[19];

                rowNum++;
            }

            // Add Total Row
            ws.Row(rowNum++).Hide(); // Leave a row space for sorting

            if (ws.Name == "Plan by Source")
            {
                ws.Cell(rowNum, 2).Value = "Total:";
            }
            else
            {
                ws.Cell(rowNum, 1).Value = "Total:";
            }

            ws.Row(rowNum).Style.Font.Bold = true;
            ws.Row(rowNum).Style.Border.TopBorder = XLBorderStyleValues.Thick;
            ws.Row(rowNum).Style.Border.TopBorderColor = XLColor.Black;

            // Calculate totals for each field column
            for (int j = 17; j <= 36; j++)
            {
                int col1 = j;
                if (j != 29) // Skip column 28 in 0-based (29 in 1-based)
                {
                    SetCol(ws, rowNum, j, ris.Sum(r => r.Fields[col1 - 17]));
                }
            }

            // Auto fit all the columns
            for (int num3 = 1; num3 <= 37; num3++)
            {
                ws.Column(num3).AdjustToContents();
                if (num3 > 16)
                {
                    ws.Column(num3).Width = 20; // Convert from 5120 to approximate width
                }
            }
        }

        private static bool IsNumeric(string value)
        {
            return int.TryParse(value, out _);
        }

        private static void SetCol(IXLWorksheet worksheet, int row, int col, double field)
        {
            worksheet.Cell(row, col).Value = Convert.ToDecimal(field);
            worksheet.Cell(row, col).Style.NumberFormat.Format = "_($* #,##0.00_);[Red]($* #,##0.00)";
        }
    }
}