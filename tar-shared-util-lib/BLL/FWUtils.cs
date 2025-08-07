using System.Data;
using System.Xml.Linq;
using SIUtil;
using TRS.IT.SI.BusinessFacadeLayer.Model;
using BFL = TRS.IT.SI.BusinessFacadeLayer;

namespace TRS.IT.SI.BusinessFacadeLayer
{
    public class FWUtils
    {
        private FWUtils()
        {
        }
        public const string C_ConId = "998775"; // "995978"
        public const string C_SubId = "000";
        public const string C_SessionId = "0FCB60D9-5969-4E6B-A2A1-0000544FBFC9";
        public const string C_FW = "FundWizard";
        public const string C_FWFrom = "FWStartFrom";
        public const string C_CancelReturnUrl = "/SIP/Employer/PlanInformation/ps_planInformation.aspx";
        public const string C_CancelAddMAReturnUrl = "ps_planInformation.aspx";
        public const string C_CancelReturnUrlPMUI = "ps_FWPmui.aspx";
        public const string C_fwOutputPath = "/sp_tmp/"; // "/pr_tmp/fw_tmp/"
        public const string C_fwLocalPath = "/local_tmp/"; // "/pr_tmp/fw_tmp/"
        public const string C_fwTemplatePath = "/SIP/Employer/PlanFunds/Templates/";
        public const string C_TradingRestriction = "FWTradingRestriction";
        public string C_eSignPath = TrsAppSettings.AppSettings.GetValue("SignixDownloadDocumentPath");
        public const string C_hdr_default_fund_qdia_answer = "default_fund_qdia_answer";
        public const string C_hdr_default_fund_qdia_answer_no = "default_fund_qdia_answer_no";
        public const string C_hdr_qdia_select = "qdia_select";
        public const string C_hdr_default_fund_tmf_select = "default_fund_tmf_select";
        public const string C_hdr_default_fund_new = "default_fund_new";
        public const string C_hdr_default_fund_new_partner_id = "default_fund_new_partner_id";
        public const string C_hdr_default_fund = "default_fund";
        public const string C_hdr_underwriting_company_code = "underwriting_company_code";
        public const string C_hdr_underwriting_company = "underwriting_company";
        public const string C_hdr_heading_city = "heading_city";
        public const string C_hdr_is_mep = "is_mep";
        public const string C_hdr_company_name = "company_name";
        public const string C_hdr_company_addr1 = "company_addr1";
        public const string C_hdr_company_city_n_state = "";
        public const string C_hdr_ = "company_city_n_state";
        public const string C_hdr_plan_name = "plan_name";
        public const string C_hdr_contract_id = "contract_id";
        public const string C_hdr_contact_type = "contact_type";
        public const string C_hdr_request_name = "request_name";
        public const string C_hdr_request_email = "request_email";
        public const string C_hdr_request_date = "request_date";
        public const string C_hdr_company_fax = "company_fax";
        public const string C_hdr_company_phone = "company_phone";
        public const string C_hdr_primary_first_name = "primary_first_name";
        public const string C_hdr_primary_last_name = "primary_last_name";
        public const string C_hdr_primary_addr1 = "primary_addr1";
        public const string C_hdr_primary_city_n_state = "primary_city_n_state";
        public const string C_hdr_plan_admin = "plan_admin";
        public const string C_hdr_plan_admin_addr1 = "plan_admin_addr1";
        public const string C_hdr_plan_admin_city_n_state = "plan_admin_city_n_state";
        public const string C_hdr_forfeiture_fund = "forfeiture_fund";
        public const string C_hdr_forfeiture_fund_new = "forfeiture_fund_new";
        public const string C_hdr_forfeiture_fund_new_partner_id = "forfeiture_fund_new_partner_id";
        public const string C_hdr_forfeiture_fund_held_in_cash = "forfeiture_fund_new_held_in_cash";
        public const string C_hdr_partner_plan_id = "partner_plan_id";
        public const string C_hdr_fiduciary_services_ProviderID = "FiduciaryServicesProviderID";
        public const string C_hdr_portXpress_selected = "portXpress_selected";
        public const string C_hdr_PortXpress_is_material = "PortXpressIsMaterial";
        public const string C_hdr_PortXpress_is_material_qdia = "PortXpressIsMaterial_QDIA";
        public const string C_hdr_PortXpress_is_material_custom = "PortXpressIsMaterial_custom";
        public const string C_hdr_PortXpress_custom = "PortXpressCustom";
        public const string C_hdr_PortXpress_glidepath = "PortXpressGlidePath";
        public const string C_hdr_PortXpress_removal = "PortXpressRequestRemoval";
        public const string C_hdr_PortXpress_rule_3100_LC = "PortXpressRule3100LargeCap";
        public const string C_hdr_PortXpress_rule_3101_ST = "PortXpressRule3101ShortTerm";
        public const string C_hdr_PortXpress_fiduciary_type = "PortXpressFiduciaryType";
        public const string C_hdr_PortXpress_fiduciary_type_select = "PortXpressFiduciaryTypeSelect";
        public const string C_hdr_PortXpress_changeauthorization_type = "PortXpressChangeAuthorizationType";
        public const string C_hdr_PortXpress_Sponsor_Letter_type = "PortXpressSponsorLettertype";
        public const string C_hdr_PortXpress_RiskPreference = "PortXpressRiskPreference";
        public const string C_hdr_PortXpress_SponsorPPTPLetterFile = "PortXpressSponsorPPTPLetterFile";
        public const string C_hdr_PortXpress_AgreementCode = "PortXpressAgreementCode";
        public const string C_hdr_ManagedAdvice_Addition = "ManagedAdviceAddition";
        public const string C_hdr_fiduciary_Name = "FiduciaryName";
        public const string C_hdr_Mesirow_AutoExecute = "MesirowAutoExecute";
        public const string C_PayStartAgreementTemplatePath = "/SIP/Employer/PlanAdministration/Templates/";
        public static void AddPdfRow(DataTable a_tb, string a_sRowId, string a_sRowDesc, string a_sRowVal)
        {
            var drw = a_tb.Rows.Find(a_sRowId);
            if (drw == null)
            {
                drw = a_tb.NewRow();
                drw["row_id"] = a_sRowId;
                drw["row_desc"] = a_sRowDesc;
                drw["row_val"] = a_sRowVal;
                a_tb.Rows.Add(drw);
            }
            else
            {
                drw["row_desc"] = a_sRowDesc;
                drw["row_val"] = a_sRowVal;
            }
        }
        public static string[] GetHdrData(string a_sRowId, DataTable a_tb)
        {
            var rw = a_tb.Rows.Find(a_sRowId);
            var str = new string[2];
            if (!(rw == null))
            {
                str[0] = CheckDBNull(rw["row_val"]);
                str[1] = Convert.ToString(rw["row_desc"]);
                return str;
            }
            else
            {
                str[0] = string.Empty;
                str[1] = string.Empty;
                return str;
            }
        }
        public static string CheckDBNull(object a_oData)
        {
            if (a_oData is DBNull | a_oData == null)
            {
                return "";
            }
            else
            {
                return a_oData.ToString().Trim();
            }
        }
        public static string CheckDBNullInt(object a_oData)
        {
            if (a_oData is DBNull | a_oData == null)
            {
                return 0.ToString();
            }
            else
            {
                return Convert.ToInt32(a_oData).ToString();
            }
        }
        public static string CheckDBNullDt(object a_oData)
        {
            if (a_oData is DBNull || a_oData == null)
            {
                return "";
            }
            else
            {
                return DateTime.Parse(a_oData.ToString()).ToShortDateString();
            }
        }
        public static string CheckDBNullDateTime(object a_oData)
        {
            if (a_oData is DBNull || a_oData == null)
            {
                return "";
            }
            else
            {
                return DateTime.Parse(a_oData.ToString()).ToString();
            }
        }
        public static string GetNextBusinessDay(DateTime a_dtDate, int a_iDays)
        {
            DataTable tbl;
            tbl = DAL.General.AddBusinessDays(a_dtDate, a_iDays);
            if (a_iDays < 0)
            {
                a_iDays = -1 * a_iDays;
            }

            if (tbl.Rows.Count > 0)
            {
                if (a_iDays < 2)
                {
                    return DateTime.Parse(tbl.Rows[0]["Bdate"].ToString()).ToString("MM/dd/yyyy");
                }
                else
                {
                    return DateTime.Parse(tbl.Rows[a_iDays - 1]["Bdate"].ToString()).ToString("MM/dd/yyyy");
                }
            }
            else
            {
                return string.Empty;
            }

        }
    }
}