using System.Data;
using HardshipLiftReport.SOA;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace HardshipLiftReport.DAL
{
    public class PASSDC
    {
        private string _sConnectString;

        public PASSDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }

        public DataSet GetHardshipLiftData(DateTime StartDate, DateTime EndDate)
        {
            return new eDocsSOA().ListForHardshipLift(StartDate.ToShortDateString(), EndDate.ToShortDateString(), "Pass 150");
        }

        public DataSet GetHDParticipantInfo(DataTable dt)
        {
            DataSet ds = new();
            string strDetails = "";
            dt.TableName = "HardShipDistribution";
            foreach (DataColumn dc in dt.Columns)
            {
                dc.ColumnMapping = MappingType.Attribute; // reduce the size of xml
            }

            // Datatable as XML string 
            using (StringWriter swStringWriter = new())
            {

                dt.WriteXml(swStringWriter);
                strDetails = swStringWriter.ToString();

            }
            // strDetails = strDetails.Replace("'", "");
            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pPAS_GetHDParticipantInfo", [strDetails]);
            if (ds != null)
            {
                ds.Tables.Add(dt.Copy());
                ds.Tables[0].TableName = "Address";
                ds.Tables[1].TableName = "HardshipInfo";
                ds.Tables[1].Columns.Add("address1");
                ds.Tables[1].Columns.Add("address2");
                ds.Tables[1].Columns.Add("City");
                ds.Tables[1].Columns.Add("State");
                ds.Tables[1].Columns.Add("Zip");
                ds.Tables[1].Columns.Add("termination");

                // ds.Relations.Add("SSN_Relation", ds.Tables[1].Columns["participant_ssn"], ds.Tables[0].Columns["SSN"]);
                foreach (DataRow drHardship in ds.Tables["HardshipInfo"].Rows)
                {
                    foreach (DataRow drAddress in ds.Tables["Address"].Rows)

                    {
                        if ((drHardship["contract_id"].ToString().Trim() == drAddress["contract_id"].ToString().Trim()) &
                              (drHardship["participant_ssn"].ToString().Trim() == drAddress["SSN"].ToString().Trim()))
                        {
                            drHardship["address1"] = drAddress["address1"];
                            drHardship["address2"] = drAddress["address2"];
                            drHardship["City"] = drAddress["City"];
                            drHardship["State"] = drAddress["State"];
                            drHardship["Zip"] = drAddress["Zip"];
                            drHardship["termination"] = drAddress["termination"];
                            drHardship.AcceptChanges();
                        }
                    }
                }
                ds.Tables[1].Columns["sub_id"].ColumnName = "Location";
                ds.Tables[1].Columns["participant_ssn"].ColumnName = "SSN";
            }
            return ds;

        }

    }
}
