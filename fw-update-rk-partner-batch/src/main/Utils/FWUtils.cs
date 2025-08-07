using System.Data;
using TRS.IT.SI.BusinessFacadeLayer.Model;

namespace FWUpdateRKPartner.Utils
{
    class FWUtils
    {
        public static string TAE2ByteFundId(string a_sFundID)
        {
            if (a_sFundID.Length < 2)
            {
                return "0" + a_sFundID;
            }
            else
            {
                return a_sFundID;
            }
        }
        public static void GetPartnerEmail(string sPartner, string sPlan, string sContract, string sPM, DateTime dtEffective, ref string sSubject, ref string sBody)
        {
            DataTable dt;
            sSubject = sPlan + " - % Fund Change Effective (" + dtEffective.ToShortDateString() + ")";
            sBody = "Contract #: " + sContract + "<br /> Assigned PM:" + sPM + "<br /><br />Please process the attached fund change request. <br /><br /> ";
            switch (sPartner ?? "")
            {
                case GeneralConstants.C_PARTNER_ID_TRS:
                    {
                        sSubject = sSubject.Replace("%", "HOME OFFICE");
                        sBody = sBody.Replace("Please process the attached fund change request.", "");
                        sBody += "Please process the attached request for fund change an effective date of " + dtEffective.ToShortDateString() + ". <br /><br />";
                        break;
                    }
                case GeneralConstants.C_PARTNER_ID_PENCO:
                    {
                        sSubject = sSubject.Replace("%", "CSC");
                        dt = TRS.IT.SI.BusinessFacadeLayer.DAL.General.AddBusinessDays(dtEffective, -2);
                        sBody += "<b>Business Day 1 </b> (" + Convert.ToDateTime(dt.Rows[1][0]).ToShortDateString() + ") - PM sends e-mail request for fund closure to  CSC by 12 Noon.<br /><br />";
                        sBody += "<b>Business Day 2 </b> (" + Convert.ToDateTime(dt.Rows[0][0]).ToShortDateString() + ") - (Pegasys Update Day); CSC sends transfer instructions to TRS via the evening link.<br /><br />";
                        sBody += "<b>Business Day 3 </b> (" + dtEffective.ToShortDateString() + ") - TRS sends CSC a confirmation file detailing the transactions.  CSC reconciles the record keeping system.  VRS/IAS files are updated.  New funds are available on the web.<br /><br />";
                        break;
                    }
                case GeneralConstants.C_PARTNER_ID_SEBS:
                    {
                        sSubject = sSubject.Replace("%", GeneralConstants.C_PARTNER_ID_SEBS);
                        dt = TRS.IT.SI.BusinessFacadeLayer.DAL.General.AddBusinessDays(dtEffective, -3);
                        sBody += "<b>Business Day 1 </b> (" + Convert.ToDateTime(dt.Rows[2][0]).ToShortDateString() + ") - Send e-mail with fund change by 12:00pm noon and SEBS shuts down fund at 4:00pm EST.<br /><br />";
                        sBody += "<b>Business Day 2 </b> (" + Convert.ToDateTime(dt.Rows[1][0]).ToShortDateString() + ") - Idle.<br /><br />";
                        sBody += "<b>Business Day 3 </b> (" + Convert.ToDateTime(dt.Rows[0][0]).ToShortDateString() + ") - TRS processes settlements/transfers and inactivates deleted funds (Pegasys Update Day).<br /><br />";
                        sBody += "<b>Business Day 4 </b> (" + dtEffective.ToShortDateString() + ") - SEBS receives, reconciles and processes settlements/transfers from TRS.<br /><br />";
                        dt = TRS.IT.SI.BusinessFacadeLayer.DAL.General.AddBusinessDays(dtEffective, 1);
                        sBody += "<b>Business Day 5 </b> (" + Convert.ToDateTime(dt.Rows[0][0]).ToShortDateString() + ") - SEBS activates change.<br /><br />";
                        break;
                    }
                case GeneralConstants.C_PARTNER_ID_CPC:
                    {
                        sSubject = sSubject.Replace("%", GeneralConstants.C_PARTNER_ID_CPC);
                        dt = TRS.IT.SI.BusinessFacadeLayer.DAL.General.AddBusinessDays(dtEffective, -3);
                        sBody += "<b>Business Day 1 </b> (" + Convert.ToDateTime(dt.Rows[2][0]).ToShortDateString() + ") - Send e-mail with fund change by 12:00pm noon and CPC shuts down fund at 4:00pm EST.<br /><br />";
                        sBody += "<b>Business Day 2 </b> (" + Convert.ToDateTime(dt.Rows[1][0]).ToShortDateString() + ") - Idle.<br /><br />";
                        sBody += "<b>Business Day 3 </b> (" + Convert.ToDateTime(dt.Rows[0][0]).ToShortDateString() + ") - TRS processes settlements/transfers and inactivates deleted funds (Pegasys Update Day).<br /><br />";
                        sBody += "<b>Business Day 4 </b> (" + dtEffective.ToShortDateString() + ") - CPC receives, reconciles and processes settlements/transfers from TRS.<br /><br />";
                        dt = TRS.IT.SI.BusinessFacadeLayer.DAL.General.AddBusinessDays(dtEffective, 1);
                        sBody += "<b>Business Day 5 </b> (" + Convert.ToDateTime(dt.Rows[0][0]).ToShortDateString() + ") - CPC activates change.<br /><br />";
                        break;
                    }

                default:
                    {
                        sSubject = "<b>Partner code error!! </b> ";
                        sBody = "";
                        break;
                    }
            }
            sBody += "Please let us know if you have any questions.";
        }
    }
}
