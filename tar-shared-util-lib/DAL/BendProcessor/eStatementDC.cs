using System.Data;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace TRS.IT.BendProcessor.DAL
{
    public class eStatementDC
    {
        private string _sConnectString;

        public eStatementDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public DataSet GetDIAFeed(int a_iNotificationType)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "dcP_GetDIAFeed", [a_iNotificationType]);
        }
        public DataSet GeteStatementFeedInfo(string a_sSSN, string a_sConId, string a_sSubId)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "dcP_GeteStatementFeedInfo", [a_sSSN, a_sConId, a_sSubId]);
        }

        public ResultReturn InsertDiaFeed(PartStFeedInfo a_oPptStFeedInfo, DocIndexFileInfo a_oDocIndexInfo)
        {
            ResultReturn oReturn = new();
            try
            {

                TRSSqlHelper.ExecuteNonQuery(_sConnectString, "dcP_InsertDIAFeed", [
                a_oPptStFeedInfo.inLoginId
            ,a_oDocIndexInfo.docType
            ,a_oDocIndexInfo.downloadType
            ,a_oDocIndexInfo.displayDesc
            ,a_oDocIndexInfo.promptFilename
            ,a_oDocIndexInfo.fileSize
            ,a_oDocIndexInfo.sysAssignedFilename
            ,a_oDocIndexInfo.fileType
            ,a_oDocIndexInfo.transId
            ,a_oDocIndexInfo.audienceType
            ,a_oDocIndexInfo.expireDt
            ,a_oDocIndexInfo.linkKey
            ,a_oDocIndexInfo.contractId
            ,a_oDocIndexInfo.subId
            ,a_oDocIndexInfo.partnerId
            ,a_oDocIndexInfo.connectParms
            ,a_oDocIndexInfo.fromPeriod
            ,a_oDocIndexInfo.toPeriod
            ,a_oDocIndexInfo.expireDt
            ,a_oPptStFeedInfo.email
            ,a_oPptStFeedInfo.firstName
            ,a_oPptStFeedInfo.middleName
            ,a_oPptStFeedInfo.lastName
            ,a_oPptStFeedInfo.companyUrl
            ,a_oPptStFeedInfo.companyPhone
            ,a_oPptStFeedInfo.notificationType]);
                oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }
        public int ClearDailyDiaFeed(int a_iNotificationType)
        {
            DataSet ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "dcP_ClearDiaFeedDaily", [a_iNotificationType]);
            return (int)ds.Tables[0].Rows[0]["RowCnt"];
        }

        public ResultReturn InsertDocumentIndex(DocIndexFileInfo a_oDocIndexInfo)
        {
            ResultReturn oReturn = new();
            try
            {
                int Doc_id = 0;
                TRSSqlHelper.ExecuteNonQuery(_sConnectString, "dcP_InsertDocumentIndex", [
                    a_oDocIndexInfo.docType
                    ,a_oDocIndexInfo.downloadType
                    ,a_oDocIndexInfo.displayDesc
                    ,a_oDocIndexInfo.promptFilename
                    ,a_oDocIndexInfo.fileSize
                    ,a_oDocIndexInfo.sysAssignedFilename
                    ,a_oDocIndexInfo.fileType
                    ,a_oDocIndexInfo.transId
                    ,a_oDocIndexInfo.audienceType
                    ,a_oDocIndexInfo.expireDt
                    ,a_oDocIndexInfo.linkKey
                    ,a_oDocIndexInfo.contractId
                    ,a_oDocIndexInfo.subId
                    ,a_oDocIndexInfo.partnerId
                    ,a_oDocIndexInfo.connectParms
                    ,a_oDocIndexInfo.fromPeriod
                    ,a_oDocIndexInfo.toPeriod
                    ,Doc_id       ]);
                oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }

        public string GetInvalidPptAddressReportData(DateTime dtStartDate, DateTime dtEndDate)
        {
            string sReponse = "";
            DriverSOA.WithdrawalsService DriverSOAWthd = new();
            sReponse = DriverSOAWthd.GetMissingAddressData(dtStartDate, dtEndDate);
            return sReponse;
        }

        public int InsertInvalidPptAddressReportData(string contract_id, string sub_id, string ssn_no, DateTime dtStartDate, DateTime dtEndDate, string first_name, string last_name, string MessageDesc)
        {
            int iRet = 0;

            iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "psi_InsertInvalidParticipentAddress", [contract_id, sub_id, dtStartDate, dtEndDate, ssn_no, first_name, last_name, MessageDesc]);
            return iRet;
        }
    }
}
