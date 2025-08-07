using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace ProcessRequiredNoticesProcBatch.DAL
{
    public class eStatementDC
    {
        private string _sConnectString;

        public eStatementDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public ResultReturn InsertDocumentIndexAndDIAFeed(DocIndexFileInfo a_oDocIndexInfo, int a_iNotificationType, int sub_notification_type, string feed)
        {
            ResultReturn oReturn = new();
            try
            {
                int Doc_id = 0;
                bool bRollupMEP = false;
                if ((a_iNotificationType == 4 || a_iNotificationType == 5) && a_oDocIndexInfo.docType == 696)
                {
                    bRollupMEP = true;
                }
                TRSSqlHelper.ExecuteNonQuery(_sConnectString, "dcP_InsertDocumentIndexAndDIAFeed", [
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
                ,a_iNotificationType
                ,bRollupMEP
                ,sub_notification_type
                ,feed
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
    }
}
