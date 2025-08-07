using System.Data;
using System.ServiceModel;
using System.Xml.Linq;

namespace FWSignedDocsToMsgcntrBatch.SOA
{
    public class FundInfoSoa : IDisposable
    {
        private TRS.IT.SI.Services.wsFmrs.FMRSSoapClient _wsFmrs;
        private bool _disposed = false;
        public FundInfoSoa()
        {
            InitializeFmrsClient();
        }
        private void InitializeFmrsClient()
        {
            string soapEndpoint = TRS.IT.TrsAppSettings.AppSettings.GetValue("FMRSURL");
            var endpointAddress = new EndpointAddress(soapEndpoint);
            var basicHttpBinding = new BasicHttpBinding(
                endpointAddress.Uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
                    ? BasicHttpSecurityMode.Transport
                    : BasicHttpSecurityMode.None);
            if (endpointAddress.Uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                basicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            }
            basicHttpBinding.MaxReceivedMessageSize = 10 * 1024 * 1024;
            basicHttpBinding.MaxBufferSize = 10 * 1024 * 1024;
            basicHttpBinding.MaxBufferPoolSize = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxDepth = 128;
            basicHttpBinding.ReaderQuotas.MaxStringContentLength = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxArrayLength = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxBytesPerRead = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxNameTableCharCount = 10 * 1024 * 1024;
            _wsFmrs = new TRS.IT.SI.Services.wsFmrs.FMRSSoapClient(basicHttpBinding, endpointAddress);
            _wsFmrs.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        public string GetFMRSFundCategory()
        {
            return _wsFmrs.GetFMRSFundCategory(Environment.UserName, DateTime.Now);
        }
        public DataSet GetFMRSFundsDataset(string inputXml, int fundType, string[] fundIds)
        {
            var fmrsXml = _wsFmrs.GetFMRSFundsXml(inputXml, fundType);
            return getFundData(fmrsXml, fundIds);
        }
        private static DataSet getFundData(string fmrsXml, string[] fundIds)
        {
            var noteCode = 65;
            var dtFundNotes = new DataTable("FundNotes");
            dtFundNotes.Columns.Add(new DataColumn("NoteId", typeof(string)));
            dtFundNotes.Columns.Add(new DataColumn("NoteDesc", typeof(string)));
            dtFundNotes.Columns.Add(new DataColumn("NoteCode", typeof(string)));

            var dtAddFunds = new DataTable("AddFunds");
            dtAddFunds.TableName = "AddFunds";
            dtAddFunds.Columns.Add(new DataColumn("FundId", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("FundDesc", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("InceptionDt", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("BmFundDesc", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("AssetClass", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("SubAssetClass", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("OperatingExpanseAs", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("OperatingExpansePer", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("AsOfDate", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("AnnualReturn1", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("AnnualReturn5", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("AnnualReturn10", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("BmAnnualReturn1", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("BmAnnualReturn5", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("BmAnnualReturn10", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("RedemptionFee", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("RedemptionFeePeriod", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("Notes", typeof(string)));
            dtAddFunds.Columns.Add(new DataColumn("PSFValue", typeof(string)));
            DataRow oRow;
            var filterList = from fundId in fundIds
                             select new
                             {
                                 fundId = fundId,
                                 orderId = Array.IndexOf(fundIds, fundId)
                             };

            var xDoc = XDocument.Parse(fmrsXml);
            var addedFunds = from fund in xDoc.Descendants("Fund")
                             from filteredFund in filterList
                             where fund.Attribute("FundID").Value == filteredFund.fundId
                             orderby filteredFund.orderId
                             select fund;

            foreach (var fund in addedFunds)
            {
                oRow = dtAddFunds.NewRow();
                if (fund.Attribute("PSFValue") != null)
                {
                    oRow["PSFValue"] = Math.Round(Convert.ToDecimal(fund.Attribute("PSFValue").Value), 2) + "%";
                }
                else
                {
                    oRow["PSFValue"] = "";
                }

                oRow["FundId"] = fund.Attribute("FundID").Value;
                oRow["FundDesc"] = fund.Element("Name").Value;
                oRow["BmFundDesc"] = fund.Attribute("IndexName") == null ? string.Empty : fund.Attribute("IndexName").Value;
                oRow["AssetClass"] = (from fundGroup in fund.Ancestors("FundGroup")
                                      where fundGroup.Attribute("FundGroupID").Value == SplitConcat(fund.Attribute("FundGroupIDConcat").Value, 1)
                                      select fundGroup.Attribute("Name").Value).FirstOrDefault();
                oRow["SubAssetClass"] = (from fundGroup in fund.Ancestors("FundGroup")
                                         where fundGroup.Attribute("FundGroupID").Value == SplitConcat(fund.Attribute("FundGroupIDConcat").Value, 2)
                                         select fundGroup.Attribute("Name").Value).FirstOrDefault();
                var expRatio = 0.00;
                oRow["OperatingExpanseAs"] = expRatio;
                oRow["OperatingExpansePer"] = expRatio;
                if (double.TryParse(fund.Attribute("ExpenseRatio").Value, out expRatio))
                {
                    oRow["OperatingExpanseAs"] = expRatio.ToString("N2") + "%";
                    oRow["OperatingExpansePer"] = "$" + ((expRatio / 100) * 1000).ToString("N2");
                }
                var asofdate = (from perf in fund.Elements("Performance")
                                where perf.Attribute("TimePeriod").Value == "Month_Last"
                                select perf.Attribute("AsOfDate").Value).FirstOrDefault();
                if (asofdate != null)
                {
                    oRow["AsOfDate"] = DateTime.Parse(asofdate.Substring(0, 10)).ToString("MM/dd/yyyy");
                }

                DateTime inceptionDate;
                if (fund.Attribute("PerfInceptionDate") != null && DateTime.TryParse(fund.Attribute("PerfInceptionDate").Value.Substring(0, 10), out inceptionDate))
                {
                    oRow["InceptionDt"] = "(" + inceptionDate.ToString("MM/yyyy") + ")";
                }

                var idx = from index in xDoc.Descendants("Index")
                          where index.Attribute("IndexID").Value == fund.Attribute("IndexID").Value
                          select index.Elements("Morningstar");

                oRow["AnnualReturn1"] = GetAnnualReturn(fund.Elements("Performance"), "M12");
                oRow["BmAnnualReturn1"] = "N/A";
                if (oRow["AnnualReturn1"].ToString() != "N/A" && fund.Attribute("IndexID") != null)
                {
                    oRow["BmAnnualReturn1"] = GetAnnualReturn(idx.FirstOrDefault(), "M12");
                }

                oRow["AnnualReturn5"] = GetAnnualReturn(fund.Elements("Performance"), "M60");
                oRow["BmAnnualReturn5"] = "N/A";
                if (oRow["AnnualReturn5"].ToString() != "N/A" && fund.Attribute("IndexID") != null)
                {
                    oRow["BmAnnualReturn5"] = GetAnnualReturn(idx.FirstOrDefault(), "M60");
                }

                oRow["BmAnnualReturn10"] = "N/A";
                var m120Return = GetAnnualReturn(fund.Elements("Performance"), "M120");
                if (m120Return == "N/A")
                {
                    oRow["AnnualReturn10"] = GetSIReturn(fund.Elements("Performance"));
                    oRow["BmAnnualReturn10"] = fund.Attribute("IndexPerformanceSI") == null ? "N/A" : double.Parse(fund.Attribute("IndexPerformanceSI").Value).ToString("N2") + " %";
                }
                else
                {
                    oRow["AnnualReturn10"] = m120Return;
                    oRow["BmAnnualReturn10"] = GetAnnualReturn(idx.FirstOrDefault(), "M120");
                }
                if (fund.Elements("RedemptionFee").Any())
                {
                    oRow["RedemptionFee"] = "Redemption Fee: " + double.Parse(fund.Element("RedemptionFee").Attribute("Fee").Value).ToString("P2");
                    oRow["RedemptionFeePeriod"] = "Redemption Period: " + fund.Element("RedemptionFee").Attribute("Days").Value + " day(s)";
                }
                else
                {
                    oRow["Notes"] = "N/A";
                }
                if (fund.Elements("TradingRestrictions").Any())
                {
                    var noteId = fund.Element("TradingRestrictions").Element("Notes").Element("Note").Attribute("NoteID").Value;

                    var filter = "NoteId = '" + noteId + "'";
                    var rows = dtFundNotes.Select(filter);
                    if (dtFundNotes.Rows.Count > 0 && rows.Count() > 0)
                    {
                        oRow["Notes"] = "See note " + rows[0]["NoteCode"].ToString();
                    }
                    else
                    {
                        var footNotes = (from note in xDoc.Element("FMRS").Element("Notes").Descendants("Note")
                                         where note.Attribute("NoteID").Value == noteId
                                         select note.Element("Value").Value).FirstOrDefault();
                        var dr = dtFundNotes.NewRow();
                        dr["NoteId"] = noteId;
                        dr["NoteCode"] = (char)noteCode;
                        dr["NoteDesc"] = footNotes.ToString();
                        oRow["Notes"] = "See note " + (char)noteCode;
                        dtFundNotes.Rows.Add(dr);
                        noteCode += 1;
                    }
                }
                dtAddFunds.Rows.Add(oRow);
            }
            var ds = new DataSet();
            ds.Tables.Add(dtAddFunds);
            ds.Tables.Add(dtFundNotes);

            return ds;
        }
        private static string GetAnnualReturn(IEnumerable<XElement> xElements, string period)
        {
            var annualReturn = "N/A";
            if (xElements == null || !xElements.Any())
            {
                return annualReturn;
            }

            var ret = from ele in xElements
                      where ele.Attribute("TimePeriod").Value == "Month_Last"
                      from returnDetail in ele.Element("TrailingReturn").Element("Return").Elements("ReturnDetail")
                      where returnDetail.Attribute("TimePeriod").Value == period && returnDetail.Attributes("Symbol").Any() == false
                      select returnDetail.Attribute("Value").Value;

            if (ret.Any())
            {
                annualReturn = ret.FirstOrDefault().ToString();
            }

            if (annualReturn != "N/A")
            {
                annualReturn = double.Parse(annualReturn).ToString("N2") + "%";
            }
            return annualReturn;
        }
        private static string GetSIReturn(IEnumerable<XElement> xElements)
        {
            var annualReturn = "N/A";
            if (xElements == null || !xElements.Any())
            {
                return annualReturn;
            }

            var ret = from ele in xElements
                      where ele.Attribute("TimePeriod").Value == "Month_Last"
                      from returnDetail in ele.Element("TrailingReturn").Element("Return").Elements("ReturnDetail")
                      .Where(a => a.Attributes("Symbol").Any())
                      select returnDetail.Attribute("Value").Value;

            if (ret.Any())
            {
                annualReturn = ret.FirstOrDefault().ToString();
            }

            if (annualReturn != "N/A")
            {
                annualReturn = double.Parse(annualReturn).ToString("N2") + "%";
            }
            return annualReturn;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    if (_wsFmrs?.State == CommunicationState.Opened)
                    {
                        _wsFmrs.Close();
                    }
                }
                catch
                {
                    _wsFmrs?.Abort();
                }
                _disposed = true;
            }
        }
        private static string SplitConcat(string fundGroupIDConcat, Int16 position)
        {
            var concats = fundGroupIDConcat.Split(['|'], StringSplitOptions.RemoveEmptyEntries);
            if (concats.Length > position)
            {
                return concats[position];
            }

            return string.Empty;
        }
    }
}
