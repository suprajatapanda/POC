using System.Data;
using SIUtil;
using TRS.SqlHelper;

namespace FWFundUpdatesToISCBatch.DAL
{
    public class General
    {
        public static int UpdateFWeDocsCases(int row_no, int process_status, string ContractID, string SubID, string change_type, string pm_name, TRS.IT.SI.BusinessFacadeLayer.Model.AddDeleteFundsInfo oAddDeleteFundsInfo)
        {
            string AddDeletefunddataXml = "";
            int iRet = -1;

            try
            {
                AddDeletefunddataXml = TRS.IT.TRSManagers.XMLManager.GetXML(oAddDeleteFundsInfo);
                iRet = TRSSqlHelper.ExecuteNonQuery(TRS.IT.SI.BusinessFacadeLayer.DAL.General.ConnectionString, "pBkP_UpdateFWeDocsCases", [row_no, process_status, ContractID, SubID, change_type, pm_name, AddDeletefunddataXml]);
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                iRet = TRSSqlHelper.ExecuteNonQuery(TRS.IT.SI.BusinessFacadeLayer.DAL.General.ConnectionString, "pBkP_UpdateFWeDocsCases", [row_no, -1, ContractID, SubID, change_type, pm_name, AddDeletefunddataXml]);
                TRS.IT.SI.BusinessFacadeLayer.Util.SendMail(TRS.IT.TrsAppSettings.AppSettings.GetValue("BendFromEmail"), TRS.IT.TrsAppSettings.AppSettings.GetValue("BendToEmail"), "UpdateFWeDocsCases row_no = ", (row_no + Convert.ToDouble(Environment.NewLine) + Convert.ToDouble(ex.Message)).ToString());

            }
            return iRet;

        }
        public static int InsertFWeDocsCases(string P3ItemXML, string start_dt, string end_dt)
        {
            var sqlparam = new Microsoft.Data.SqlClient.SqlParameter[4];
            sqlparam[0] = new Microsoft.Data.SqlClient.SqlParameter("@fund_data_P3Item", SqlDbType.Xml);
            sqlparam[1] = new Microsoft.Data.SqlClient.SqlParameter("@start_dt", SqlDbType.DateTime);
            sqlparam[2] = new Microsoft.Data.SqlClient.SqlParameter("@end_dt", SqlDbType.DateTime);
            sqlparam[3] = new Microsoft.Data.SqlClient.SqlParameter("@row_no", SqlDbType.Int);


            sqlparam[0].Value = P3ItemXML;
            sqlparam[1].Value = start_dt;
            sqlparam[2].Value = end_dt;
            sqlparam[3].Direction = ParameterDirection.Output;

            TRSSqlHelper.ExecuteNonQuery(TRS.IT.SI.BusinessFacadeLayer.DAL.General.ConnectionString, CommandType.StoredProcedure, "pBkP_InsertFWeDocsCases", sqlparam);

            return Convert.ToInt32(sqlparam[3].Value);
        }
    }
}
