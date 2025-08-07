using System.ComponentModel;
using System.Data;

namespace FWBamlDocsToWMSBatch.Model
{
    /// <summary>
    /// BAML information for WMS doc generation
    /// </summary>
    public class BamlFundInfo
    {
        /// <summary>
        /// ContractID
        /// </summary>
        public string ContractID;
        /// <summary>
        /// Sub ID
        /// </summary>
        public string SubID;
        /// <summary>
        /// CaseNo
        /// </summary>
        public string CaseNo;
        /// <summary>
        /// Pegasys product_id 
        /// </summary>
        public string ProductID;
        /// <summary>
        /// MDP_productId
        /// </summary>
        public string MDPProductID;
        /// <summary>
        /// Effective date
        /// </summary>
        public DateTime EffectiveDate;
        /// <summary>
        /// Newly added Fund IDs action 1
        /// </summary>
        public List<int> NewFundList;
        /// <summary>
        /// Action 2
        /// </summary>
        public List<int> DeletedFundList;
        /// <summary>
        /// List of BamlFund
        /// </summary>
        public List<BamlFund> FundList;
        /// <summary>
        /// The file that uploaded to WMS
        /// </summary>
        public string OutputFile;
        /// <summary>
        /// These plans are restricted to credit psf 
        /// </summary>
        public string PSFPlans = string.Empty;


        /// <summary>
        /// Contructor
        /// </summary>
        public BamlFundInfo()
        {
            NewFundList = new List<int>();
            DeletedFundList = new List<int>();
            FundList = new List<BamlFund>();
        }

        public BamlFundInfo(string psfplans)
            : this()
        {
            PSFPlans = psfplans;
        }

        /// <summary>
        /// Is BAML or NAV
        /// </summary>
        public string ReportName
        {
            get
            {
                return MDPProductID == "116" ? "BAML" : "NAV";
            }
        }

        /// <summary>
        /// WMS Source Name 
        /// </summary>
        public string SourceName
        {
            get
            {
                return MDPProductID == "116" ? "Notification BAMLFundWizardDocGen" : "Notification NAVFundWizardDocGen";
            }
        }
        /// <summary>
        /// contructor
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="sid"></param>
        /// <param name="caseNo"></param>
        /// <param name="effectaiveDate"></param>
        public BamlFundInfo(string cid, string sid, string caseNo, DateTime effectaiveDate)
            : this()
        {
            ContractID = cid;
            SubID = sid;
            CaseNo = caseNo;
            EffectiveDate = effectaiveDate;
        }

        /// <summary>
        /// Return the newly added funds in DataTable format
        /// </summary>
        /// <returns></returns>
        public DataSet GetFundListToDataSet()
        {
            DataSet ds = new DataSet();
            List<BamlFund> newBAMLFundList = new List<BamlFund>();
            foreach (int f in NewFundList)
            {
                foreach (BamlFund fd in FundList)
                {
                    if (fd.FundId == f)
                    {
                        newBAMLFundList.Add(fd);
                        break;
                    }
                }
            }
            ds.Tables.Add(FundListToDataTable(newBAMLFundList, "FundListNew"));

            List<BamlFund> deleteBAMLFundList = new List<BamlFund>();
            foreach (int f in DeletedFundList)
            {
                foreach (BamlFund fd in FundList)
                {
                    if (fd.FundId == f)
                    {
                        deleteBAMLFundList.Add(fd);
                        break;
                    }
                }
            }
            ds.Tables.Add(FundListToDataTable(deleteBAMLFundList, "FundListDelete"));

            return ds;
        }

        /// <summary>
        /// Return Fundlist in Datatable format
        /// </summary>
        /// <returns></returns>
        private DataTable FundListToDataTable(List<BamlFund> fl, string tableName)
        {
            int specialPlan = PSFPlans.IndexOf(ContractID);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(BamlFund));
            DataTable table = new DataTable(tableName);
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (BamlFund item in fl)
            {
                if (specialPlan < 0 || (specialPlan >= 0 && item.PSF < 0))  //PSF + debit - credit
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    table.Rows.Add(row);
                }
            }
            return table;
        }
    }

    /// <summary>
    /// Fund info
    /// </summary>
    public class BamlFund
    {
        /// <summary>
        /// Fund ID
        /// </summary>
        public int FundId { get; set; }
        /// <summary>
        /// Fund Name
        /// </summary>
        public string FundName { get; set; }
        /// <summary>
        /// Fund class name
        /// </summary>
        public string FundClass { get; set; }
        /// <summary>
        /// P3 fund descriptor
        /// </summary>
        public string Descriptor { get; set; }
        /// <summary>
        /// Revenue bps
        /// </summary>
        public double RevenueBps { get; set; }
        /// <summary>
        /// Plan Service Fee
        /// </summary>
        public double PSF { get; set; }
    }

}
