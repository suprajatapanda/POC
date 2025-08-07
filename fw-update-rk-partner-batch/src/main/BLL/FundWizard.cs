using System.Xml.Linq;
using MO = TRS.IT.SI.BusinessFacadeLayer.Model;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;

namespace FWUpdateRKPartner.BLL
{
    public class FundWizard
    {
        TARSharedUtilLibBFLBLL.FundWizard oFW;
        public FundWizard(TARSharedUtilLibBFLBLL.FundWizard obj)
        {
            oFW = obj;
        }

        public int InsertTaskSendNoticeToPartner(string a_sToEmail, string a_sSubject, string a_sBody)
        {
            var xEmail = new XElement("EmailContent", new XAttribute("ToEmail", a_sToEmail), new XAttribute("Subject", a_sSubject), a_sBody);
            return oFW.InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.SendNoticeToPartner, 100, [xEmail]);
        }
        public int InsertTaskUpdatePartnerSystem(string a_sResult, string a_sRequest, string a_sResponse)
        {
            var xEl = new XElement("UpdatePartnerSystem", new XAttribute("MQRequest", a_sRequest), new XAttribute("MQResponse", a_sResponse), a_sResult);
            int iStatus = 100;
            if (a_sResult != "Succeeded")
                iStatus = -1;

            return oFW.InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.UpdatePartnerSystem, iStatus, new XElement[] { xEl });
        }

    }
}
