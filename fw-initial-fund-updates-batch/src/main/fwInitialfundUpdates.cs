using SIUtil;
using TARSharedUtilLibModel = TRS.IT.BendProcessor.Model;
using FWInitialFundUpdatesBatch.BLL;
using TRS.IT.BendProcessor.Util;

namespace FWInitialFundUpdatesBatch;
public class fwInitialfundUpdates
{
    public void Run(string jobName)
    {
        TARSharedUtilLibModel.TaskStatus oTaskStatus = null;
        TRS.IT.BendProcessor.BLL.FWBend fWBend = new TRS.IT.BendProcessor.BLL.FWBend();
        try
        {
            Logger.LogMessage(jobName + " " + DateTime.Now.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            FWBend oFW = new FWBend(fWBend);
            oTaskStatus = oFW.ProcessUpdatePegasysInitial();
            if (oTaskStatus.retStatus != TARSharedUtilLibModel.TaskRetStatus.Succeeded && oTaskStatus.retStatus != TARSharedUtilLibModel.TaskRetStatus.NotRun)
                Utils.AddErrorEventLog(jobName + ": " + Utils.ParseError(oTaskStatus.errors));
            if (oTaskStatus.rowsCount > 0)
            {
                Logger.LogMessage(jobName + " - Signed: " + oTaskStatus.rowsCount.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
        }
        catch (Exception ex)
        {
            Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            string sStatusError = string.Empty;
            if (oTaskStatus != null) Utils.ParseError(oTaskStatus.errors);
            fWBend.SendErrorEmail(ex);
        }
    }
}