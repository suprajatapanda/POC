using System.Data;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;
using TRS.IT.TrsAppSettings;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;
using ErrorInfo = TRS.IT.BendProcessor.Model.ErrorInfo;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace FWFundRiderToMsgcntrBatch.BLL
{
    public class FWBend
    {

        readonly TRS.IT.BendProcessor.BLL.FWBend fWBend;
        public FWBend(TRS.IT.BendProcessor.BLL.FWBend obj)
        {
            fWBend = obj;
        }

        public TaskStatus ProcessSendFundRiderToMC()
        {
            const string TaskName = "ProcessSendFundRiderToMC";
            const string FullTaskName = ConstN.C_TAG_P_O + TaskName + ConstN.C_TAG_P_C;

            var taskStatus = new TaskStatus { retStatus = TaskRetStatus.NotRun };

            try
            {
                if (AppSettings.GetValue(TaskName) != "1")
                    return taskStatus;

                fWBend.InitTaskStatus(taskStatus, TaskName);

                foreach (DataRow row in fWBend.PendingFundChanges.Tables[0].Rows)
                {
                    int action = Utils.CheckDBNullInt(row["change_type"]);
                    string partnerId = Utils.CheckDBNullStr(row["partner_id"]).ToUpper();
                    string contractId = Utils.CheckDBNullStr(row["contract_id"]);
                    string subId = Utils.CheckDBNullStr(row["sub_id"]);
                    int caseNo = Utils.CheckDBNullInt(row["case_no"]);
                    DateTime pegasysDate = Convert.ToDateTime(row["pegasys_dt"]);

                    DateTime scheduledDate = partnerId switch
                    {
                        ConstN.C_PARTNER_TAE or ConstN.C_PARTNER_PENCO or ConstN.C_PARTNER_ISC =>
                            Convert.ToDateTime(TARSharedUtilLibBFLBLL.FWUtils.GetNextBusinessDay(pegasysDate, 2)),

                        ConstN.C_PARTNER_CPC or ConstN.C_PARTNER_SEBS or ConstN.C_PARTNER_TRS =>
                            Convert.ToDateTime(TARSharedUtilLibBFLBLL.FWUtils.GetNextBusinessDay(pegasysDate, 3)),

                        _ => new DateTime(1990, 1, 1)
                    };

                    if (scheduledDate.Date == DateTime.Today && action is not (4 or 6))
                    {
                        var result = SendFundRiderToMC(contractId, subId, caseNo);

                        if (result.returnStatus != ReturnStatusEnum.Succeeded)
                        {
                            General.CopyResultError(taskStatus, result);
                            fWBend.SendErrorEmailToUsers(contractId, subId, caseNo, partnerId, result.Errors[0].errorDesc + FullTaskName);
                            taskStatus.fatalErrCnt++;
                            taskStatus.retStatus = TaskRetStatus.ToCompletionWithErr;
                        }

                        taskStatus.rowsCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                fWBend.InitTaskError(taskStatus, ex, true);
            }

            return taskStatus;
        }

        private ResultReturn SendFundRiderToMC(string contractId, string subId, int caseNo)
        {
            const int FundRiderDocType = 185;
            var result = new ResultReturn();
            var infoPrefix = $"Error in SendFundRiderToMC() - Contract: {contractId} SubId: {subId} CaseNo: {caseNo} ";

            try
            {
                TARSharedUtilLibBFLBLL.FundWizard fundWizard =  new TARSharedUtilLibBFLBLL.FundWizard(Guid.NewGuid().ToString(), contractId, subId);
                fundWizard.GetCaseNo(caseNo);

                var taskData = fundWizard.GetTaskByTaskNo((int)BFLModel.FundWizardInfo.FwTaskTypeEnum.FundRiderSentToMC);
                if (taskData.Tables[0].Rows.Count > 0)
                {
                    result.returnStatus = ReturnStatusEnum.Succeeded;
                    return result;
                }

                DataRow riderDoc = null;
                try
                {
                    var docsToImage = fundWizard.GetDocsToImage();
                    var docTable = docsToImage.Tables[0];
                    docTable.PrimaryKey = new[] { docTable.Columns["DocTypeID"] };
                    riderDoc = docTable.Rows.Find(FundRiderDocType);
                }
                catch (Exception ex)
                {
                    Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                }

                var contract = new TARSharedUtilLibBFLBLL.Contract(fundWizard.ContractId, fundWizard.SubId);
                bool isNavAtCSC = contract.IsNAVProduct(fundWizard.ContractId, fundWizard.SubId);

                if (isNavAtCSC)
                {
                    result.returnStatus = ReturnStatusEnum.Succeeded;
                    return result;
                }

                string filePath = string.Empty;
                string promptName = string.Empty;

                if (riderDoc == null)
                {
                    var genResult = fWBend.GenerateFundRider(fundWizard);
                    if (genResult.returnStatus != ReturnStatusEnum.Succeeded)
                        throw new Exception("Error generating Fund Rider: " + genResult.Errors[0].errorDesc);

                    filePath = genResult.confirmationNo;
                    promptName = Path.GetFileName(filePath);
                }
                else
                {
                    filePath = Path.Combine(riderDoc["file_path"].ToString(), riderDoc["file_name"].ToString());
                    promptName = Path.GetFileName(filePath);
                }

                if (string.IsNullOrWhiteSpace(filePath))
                    throw new Exception("Unable to generate Fund Rider document.");

                var sendResult = new FundWizard(fundWizard).SendFundRiderToMC(promptName, filePath);
                if (sendResult.Errors[0].Number != 0)
                {
                    result.returnStatus = ReturnStatusEnum.Failed;
                    result.Errors.Add(new ErrorInfo(-1, infoPrefix + "SendFundRiderToMC: " + sendResult.Errors[0].Description, ErrorSeverityEnum.Error));
                }
                else
                {
                    result.returnStatus = ReturnStatusEnum.Succeeded;
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                result.returnStatus = ReturnStatusEnum.Failed;
                result.Errors.Add(new ErrorInfo(-1, infoPrefix + "Exception: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return result;
        }
    }
}
