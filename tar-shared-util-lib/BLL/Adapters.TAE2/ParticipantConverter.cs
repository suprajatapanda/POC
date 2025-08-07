using System.Collections;
using System.Data;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using SIUtil;
using TRS.IT.SI.BusinessFacadeLayer.DAL;
using TRS.IT.SI.BusinessFacadeLayer.Model;
using TRS.SqlHelper;

namespace TRS.IT.SI.BusinessFacadeLayer.Adapters
{
    internal class Converter
    {
        public bool IgnoreFundMappings;
        public bool IgnoreTransHistory;
        public bool ParseHeaderOnly;

        private string[] _StrDataArray;
        private ParticipantInfo _Participant;
        private FieldTable _FieldTable;
        private Hashtable _FundBalance;
        private Hashtable _FundSequenceMappings;
        private FundInfo _LoanFund;
        private string _BVAFundSequence;

        internal const string C_LoginMessage = "6022";
        internal const string C_ParticipantMessage = "6000";
        internal const string C_StatementRequestMessage = "6049";
        internal const string C_Suffix_UpdateElections = "009EM";
        internal const string C_Suffix_ContributionsSuffix = "009";
        internal const string C_Suffix_CatchupContributionsSuffix = "009C";
        internal const string C_Suffix_OneTimeFundTransfer = "007T";
        internal const string C_Suffix_PeriodicFundTransfer = "007D";
        internal const string C_Suffix_SDBATransfer = "007B";
        internal const string C_Suffix_CancelTransfer = "038";
        internal const string C_Suffix_OneTimeRebalancing = "007F";
        internal const string C_Suffix_PeriodicRebalancing = "007R";
        internal const string C_Suffix_UpdatePassword = "035";
        internal const string C_Suffix_LoanRequest = "005";
        internal const string C_Suffix_PinLetterRequest = "035";
        internal const string C_Suffix_OnlineDistribution = "006";
        internal const string C_Suffix_SDBAEnrollment = "097052E";

        internal const string C_TimeoutError = "Timeout";

        internal const string C_Percent = "P";
        internal const string C_Dollar = "D";
        internal const string C_Multiple = "#";
        internal const string C_AmountFormat = "0000000";
        internal const string C_PercentFormat = "000";
        internal const string C_AmountFormat2 = "0000000.00";

        internal const string C_Frequency_OneTime = "O";
        internal const string C_Frequency_Annually = "A";
        internal const string C_Frequency_SemiAnnaully = "S";
        internal const string C_Frequency_Monthly = "M";
        internal const string C_Frequency_Quarterly = "Q";

        internal const string C_WebsiteRegistration_Letter = "01";
        internal const string C_PasswordChange_Letter = "02";
        internal const string C_TempPassword_Letter = "03";
        internal const string C_UserNameChange_Letter = "04";
        internal const string C_PasswordAssociation_Letter = "05";
        internal const string C_AddressChange_Letter = "06";
        internal const string C_PinChange_Letter = "07";
        internal const string C_PinAssociation_Letter = "08";

        internal const string C_NoticeOfUndeliverableEmail_Letter = "10";

        internal const string C_ERROR_MEMBER_NOT_ON_FILE = "010EC9259";

        private const int C_Header = 0;
        private const int C_LoanInfo = 1;
        private const int C_WithdrawalInfo = 2;
        private const int C_AccountInfo = 3;
        private const int C_FundInfo = 4;
        private const int C_FieldTable = 6;
        private const int C_TransactionHistory = 7;
        private const int C_FundType_Sdba = 9;
        private const string C_TFAHYFundID1 = "02R";
        private const string C_TFAHYFundID2 = "0FX";
        private const string C_TFAHYFundID3 = "0KE";

        private int[] ExcludedFunds = [-1];
        private const string C_LoanFeePaidBy = "P";
        private const float C_PersonalReturnRate = 0f;
        private const int C_MinVstAccBalMultiplier = 2;

        private const string C_DateFormat8 = "mmddyyyy";
        private const string C_DateFormat10 = "mm/dd/yyyy";
        private const string C_DefaultDate8 = "00000000";
        private const string C_Menu_Loans = "9.2.6";
        private const string C_Menu_RebalanceFunds = "9.2.3";
        private const string C_Menu_TransferFunds = "9.2.2";
        private const string C_Menu_InvestmentElections = "9.2.5";
        private const string C_ACCOUNT_LOCKED = "011EC9997";

        private struct FundStruct
        {
            public int FundID;
            public string FundName;
            public double FundBalance;
            public double Units;
            public double VstVal;
        }
        public Converter()
        {
            _Participant = new ParticipantInfo();
            IgnoreTransHistory = true;
        }
        public ParticipantInfo ConvertToParticipantInfo(int InLoginID, string ContractID, string SubID, string strParticipantData)
        {
            try
            {
                string argerrorCode = null;
                _StrDataArray = SplitMQResponse(strParticipantData, ref _Participant.Errors[0], ref argerrorCode);

                if (_Participant.Errors[0].Number != 0)
                {
                    return _Participant;
                }
                else
                {
                    _Participant = new ParticipantInfo();
                }

                _FieldTable = new FieldTable(_StrDataArray[C_FieldTable]);
                _FundBalance = new Hashtable();
                _Participant.SDBAFundInfo = new SDBAFundInfo();
                _LoanFund = null;
                ParseHeaderInfo(InLoginID, ContractID, SubID);

                if (ParseHeaderOnly == false)
                {
                    ParseLoans();

                    ParseAccountBalance();

                    ParseAssets();

                    ParseWithdrawals();

                    if (IgnoreTransHistory == false)
                    {
                        ParseTransactionHistory();
                    }

                    ParseSecurityInfo();
                }
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                _Participant.Errors[0].Number = (int)ErrorCodes.MappingError;
                _Participant.Errors[0].Description = _Participant.Errors[0].Description + "  " + General.FormatErrorMsg(ex.Message, "Mapping Error", "ParticipantConverter::ConvertToParticipantInfo") + "       " + _Participant.Errors[0].Description;
            }
            finally
            {
                if (!(_FieldTable == null))
                {
                    _FieldTable.Clear();
                }
                if (!(_FundBalance == null))
                {
                    _FundBalance.Clear();
                }
                if (!(_StrDataArray == null))
                {
                    Array.Clear(_StrDataArray, 0, _StrDataArray.Length);
                }
                if (!(_FundSequenceMappings == null))
                {
                    _FundSequenceMappings.Clear();
                }
            }
            return _Participant;
        }
        private List<AccountInfo> ParseWthAvailByMoneyType(string a_sData)
        {
            var oAccts = new List<AccountInfo>();
            AccountInfo oAcct;
            int iI;
            string sWorkingStr = a_sData.Substring(2);

            var loopTo = sWorkingStr.Length - 1;
            for (iI = 0; iI <= loopTo; iI += 12)
            {
                if (sWorkingStr.Substring(iI + 3, 9) != "000000000")
                {
                    oAcct = new AccountInfo();
                    oAcct.AccID = Convert.ToInt32(sWorkingStr.Substring(iI, 3)).ToString();
                    switch (a_sData.Substring(0, 2) ?? "")
                    {
                        case "01":
                        case "02":
                            {
                                oAcct.VestingHardshipAmt = Convert.ToDouble(FormatNumber(sWorkingStr.Substring(iI + 3, 9), 7));
                                break;
                            }
                        case "03":
                        case "05":
                            {
                                oAcct.VestingInservice59Amt = Convert.ToDouble(FormatNumber(sWorkingStr.Substring(iI + 3, 9), 7));
                                oAcct.VestingInservice62Amt = oAcct.VestingInservice59Amt;
                                break;
                            }
                        case "04":
                        case "06":
                            {
                                oAcct.VestingInserviceAmt = Convert.ToDouble(FormatNumber(sWorkingStr.Substring(iI + 3, 9), 7));
                                break;
                            }
                    }
                    oAccts.Add(oAcct);
                }
            }
            return oAccts;
        }
        private void AssignWthAvailByMoneyType(AccountInfo[] a_oPartAccts, List<AccountInfo> a_oAccts)
        {
            int iI, iI2;

            var loopTo = a_oAccts.Count - 1;
            for (iI = 0; iI <= loopTo; iI++)
            {
                var loopTo1 = a_oPartAccts.Length - 1;
                for (iI2 = 0; iI2 <= loopTo1; iI2++)
                {
                    if ((a_oAccts[iI].AccID ?? "") == (a_oPartAccts[iI2].AccID ?? "") & a_oPartAccts[iI2].FundID == "0")
                    {
                        if (a_oAccts[iI].VestingHardshipAmt > 0.01d & a_oPartAccts[iI2].VestingHardshipAmt < 0.01d)
                        {
                            a_oPartAccts[iI2].VestingHardshipAmt = a_oAccts[iI].VestingHardshipAmt;
                        }
                        if (a_oAccts[iI].VestingInserviceAmt > 0.01d & a_oPartAccts[iI2].VestingInserviceAmt < 0.01d)
                        {
                            a_oPartAccts[iI2].VestingInserviceAmt = a_oAccts[iI].VestingInserviceAmt;
                        }
                        if (a_oAccts[iI].VestingInservice59Amt > 0.01d & a_oPartAccts[iI2].VestingInservice59Amt < 0.01d)
                        {
                            a_oPartAccts[iI2].VestingInservice59Amt = a_oAccts[iI].VestingInservice59Amt;
                        }
                        if (a_oAccts[iI].VestingInservice62Amt > 0.01d & a_oPartAccts[iI2].VestingInservice62Amt < 0.01d)
                        {
                            a_oPartAccts[iI2].VestingInservice62Amt = a_oAccts[iI].VestingInservice62Amt;
                        }
                        break;
                    }
                }
            }
        }
        public void WthAvailableAmtByMoneyType(ParticipantInfo a_oPartInfo, string a_sData)
        {
            const int C_Rec_Len = 182;
            string sWorkingData = a_sData.Substring(13);
            List<AccountInfo> oAccts;

            if (a_oPartInfo.AccountInfo == null)
            {
                return;
            }

            int iI, iCnt;

            iCnt = Convert.ToInt32(a_sData.Substring(12, 1));

            var loopTo = sWorkingData.Length - 1;
            for (iI = 0; iI <= loopTo; iI += C_Rec_Len)
            {
                oAccts = ParseWthAvailByMoneyType(sWorkingData.Substring(iI, C_Rec_Len));
                AssignWthAvailByMoneyType(a_oPartInfo.AccountInfo, oAccts);
            }


        }
        private void ParseHeaderInfo(int InLoginID, string ContractID, string SubID)
        {
            string strData = _StrDataArray[0];
            string strTemp;

            {
                ref var withBlock = ref _Participant;
                withBlock.PlanInfo = new PlanInfo();
                withBlock.EnrollmentInfo = new EnrollmentInfo();

                withBlock.PayrollFrequency = TAEUtil.ConvertPayrollFrequency(strData.Substring(311, 1));
                ParseProfile(InLoginID, ContractID, SubID);

                withBlock.AccBal = Convert.ToDouble(FormatNumber(strData.Substring(130, 10).Trim(), 8, 0, 1));

                withBlock.CatchupInfo = new CatchupInfo();
                if (Convert.ToBoolean(Convert.ToInt32(_FieldTable["35"].Substring(5, 5))) && Convert.ToInt32(_FieldTable["35"].Substring(5, 5)) > 0)
                {
                    withBlock.CatchupInfo.CatchupType = CatchUpPayType.PayPeriodDollar;
                    withBlock.CatchupInfo.CatchupContrPerPeriod = Convert.ToInt32(_FieldTable["35"].Substring(5, 5));
                }
                else
                {
                    strTemp = _FieldTable["66"].Substring(8, 2);
                    if (int.TryParse(strTemp, out int catchupVal) && catchupVal > 0)
                    {
                        withBlock.CatchupInfo.CatchupType = CatchUpPayType.PayPeriodPercent;
                        withBlock.CatchupInfo.CatchupContrPerPeriod = catchupVal;
                    }
                }
                withBlock.CatchupInfo.CatchupContrPerYear = Convert.ToInt32(_FieldTable["35"].Substring(0, 5));
                withBlock.CatchupInfo.CatchupContrMax = Convert.ToInt32(_FieldTable["36"].Substring(0, 5));
                strTemp = _FieldTable["69"].Substring(0, 5);
                if (int.TryParse(strTemp, out int catchupOneTime))
                {
                    withBlock.CatchupInfo.CatchupContrOneTime = catchupOneTime;
                }
                withBlock.CatchupInfo.OneTimeCatchupDate = _FieldTable["70"];
                strTemp = "";

                withBlock.VstBal = Convert.ToDouble(FormatNumber(strData.Substring(345, 10), 8, 0, 1));
                withBlock.VstPct = Convert.ToDouble(FormatNumber(strData.Substring(395, 3), 3));
                withBlock.HardshipAmt = Convert.ToDouble(FormatNumber(_FieldTable["05"], 8, 0, 1));

                withBlock.LastContrDt = FormatDate(_FieldTable["06"], C_DateFormat10);
                withBlock.LastContrAmt = Convert.ToDouble(FormatNumber(_FieldTable["47"], 8));

                withBlock.LastInvChangeDt = FormatDate(strData.Substring(454, 8), C_DateFormat8);
                withBlock.LastProductionDt = FormatDate(_FieldTable["08"], C_DateFormat10);
                if (withBlock.LastProductionDt is null)
                {
                    withBlock.LastProductionDt = FormatDate(strData.Substring(335, 10), C_DateFormat10);
                }

                withBlock.LastTfrDt = FormatDate(strData.Substring(430, 8), C_DateFormat8);

                withBlock.InvNewCounter = Convert.ToInt32(strData.Substring(179, 2));
                withBlock.InvNewIncrement = Convert.ToInt32(strData.Substring(189, 3));
                withBlock.InvNewNumber = Convert.ToInt32(strData.Substring(192, 2));
                withBlock.InvNewPeriod = strData.Substring(194, 1).Trim();


                withBlock.LoanOutBal = Convert.ToDouble(FormatNumber(strData.Substring(280, 10), 8));

                withBlock.PersonalReturnRate = C_PersonalReturnRate;
                withBlock.PlanEntryDt = FormatDate(_FieldTable["03"], C_DateFormat10);
                if (!DateTime.TryParse(withBlock.PlanEntryDt, out _))
                {
                    withBlock.PlanEntryDt = null;
                }

                withBlock.TransPending = strData.Substring(312, 1);
                switch (withBlock.TransPending ?? "")
                {
                    case "T":
                        {
                            withBlock.TransPendingName = "Investment Choice Transfers";
                            break;
                        }
                    case "R":
                        {
                            withBlock.TransPendingName = "Rebalance Investment Choices";
                            break;
                        }
                    case "L":
                        {
                            withBlock.TransPendingName = "Loan Request";
                            break;
                        }
                    case "W":
                        {
                            withBlock.TransPendingName = "Withdrawal Request";
                            break;
                        }
                }
                withBlock.PeriodicIndicator = Convert.ToInt32(strData.Substring(274, 1));
                withBlock.Post86NonTax = Convert.ToDouble(FormatNumber(_FieldTable["18"], 8));
                withBlock.Pre87NonTax = Convert.ToDouble(FormatNumber(_FieldTable["19"], 8));

                ParsePlanLoanInfo();

                withBlock.MinVstAccBal = withBlock.PlanLoanInfo.MinLoanAmt * C_MinVstAccBalMultiplier;

                strTemp = strData.Substring(331, 4).Trim();
                if (strTemp == "EL1")
                {
                    withBlock.EnrollmentInfo.EnrollmentStatus = EnrollmentStatus.Eligible;
                }
                else
                {
                    withBlock.EnrollmentInfo.EnrollmentStatus = EnrollmentStatus.Complete;
                }

                strTemp = strData.Substring(471, 1);
                if (strTemp == "1")
                {
                    withBlock.EnrollmentInfo.ContrChangeReq = false;
                }
                else
                {
                    withBlock.EnrollmentInfo.ContrChangeReq = true;
                }

                strTemp = strData.Substring(472, 1);
                if (strTemp == "1")
                {
                    withBlock.EnrollmentInfo.InvElectChangeReq = false;
                }
                else
                {
                    withBlock.EnrollmentInfo.InvElectChangeReq = true;
                }

                strTemp = strData.Substring(473, 1);
                if (strTemp == "1")
                {
                    withBlock.EnrollmentInfo.PinChangeReq = false;
                }
                else
                {
                    withBlock.EnrollmentInfo.PinChangeReq = true;
                }

                withBlock.ShowTransHistory = strData.Substring(385, 1) == "1" ? true : false;
                withBlock.PlanInfo.SuppressVesting = strData.Substring(387, 1) == "Y" ? true : false;

                _BVAFundSequence = strData.Substring(420, 2);
                withBlock.PlanInfo.BVAMaxAmt = Convert.ToDouble(strData.Substring(422, 8).Trim());

                withBlock.OldestBuySellDt = FormatDate(_FieldTable["50"], C_DateFormat10);
                if (!DateTime.TryParse(withBlock.OldestBuySellDt, out _) || withBlock.OldestBuySellDt == "00/00/0000")
                {
                    withBlock.OldestBuySellDt = DateTime.Now.AddMonths(-18).ToString("MM/dd/yyyy");
                }
                withBlock.ExecutionDt = FormatDate(_FieldTable["55"], C_DateFormat10);
                if (DateTime.TryParse(FormatDate(_FieldTable["60"], C_DateFormat10), out DateTime afterTaxDate))
                {
                    withBlock.AfterTaxYrOfFirstContrib = afterTaxDate.Year.ToString();
                }
                withBlock.AfterTaxCurrentBal = Convert.ToDouble(FormatNumber(_FieldTable["61"], 8));
                withBlock.AfterTaxBasis = Convert.ToDouble(FormatNumber(_FieldTable["62"], 8));
            }

            ParseSDBAInfo();
            ParseDeferrals();

            ParsePlanInfo();

            GetContractData();

        }
        private void ParsePlanLoanInfo()
		{
		    string LoanAllowed;
		    var withBlock = _Participant;
    
		    withBlock.PlanLoanInfo = new PlanLoanInfo();
		    withBlock.PlanLoanInfo.CurrentLoans = Convert.ToInt32(_StrDataArray[0].Substring(290, 2));
		    LoanAllowed = _StrDataArray[0].Substring(217, 1);
		    if (LoanAllowed == "1" || LoanAllowed == "2")
		    {
		        withBlock.PlanLoanInfo.IsLoanAllowed = true;
		    }
		    else
		    {
		        withBlock.PlanLoanInfo.IsLoanAllowed = false;
		    }
		    withBlock.PlanLoanInfo.LoanAmtAvail = Convert.ToDouble(FormatNumber(_StrDataArray[0].Substring(218, 8), 6));
		    withBlock.PlanLoanInfo.LoanFee = Convert.ToDouble(FormatNumber(_StrDataArray[0].Substring(211, 6), 4));
		    withBlock.PlanLoanInfo.LoanFeePaidBy = C_LoanFeePaidBy;
		    withBlock.PlanLoanInfo.LoanSetupFee = Convert.ToDouble(FormatNumber(_StrDataArray[0].Substring(294, 6), 4));
		    withBlock.PlanLoanInfo.LoanMaintFee = Convert.ToDouble(FormatNumber(_StrDataArray[0].Substring(211, 6), 4));
		    withBlock.PlanLoanInfo.LoanIntRate = Convert.ToDouble(FormatNumber(_StrDataArray[0].Substring(300, 6), 2));
		    withBlock.PlanLoanInfo.MaxGeneralLoanTerm = Convert.ToInt32(_StrDataArray[0].Substring(248, 3).Trim());
		    withBlock.PlanLoanInfo.MaxLoanAmt = Convert.ToDouble(FormatNumber(_StrDataArray[0].Substring(238, 8), 6));
		    withBlock.PlanLoanInfo.MaxLoans = Convert.ToInt32(_StrDataArray[0].Substring(246, 2).Trim());
		    withBlock.PlanLoanInfo.MaxResLoanTerm = Convert.ToInt32(_StrDataArray[0].Substring(307, 3).Trim());
		    withBlock.PlanLoanInfo.MinLoanAmt = Convert.ToDouble(FormatNumber(_StrDataArray[0].Substring(251, 8), 6));
		    withBlock.PlanLoanInfo.MaxLoansPerPeriod = Convert.ToInt32(_StrDataArray[0].Substring(267, 2).Trim());
		    withBlock.PlanLoanInfo.LoanPeriod = _StrDataArray[0].Substring(292, 1).Trim();
		    withBlock.PlanLoanInfo.LoanCounter = Convert.ToInt32(_StrDataArray[0].Substring(234, 2).Trim());
		}
        private void GetContractData()
		{
		    var spDc = new SponsorDC(_Participant.PlanInfo.ContractID, _Participant.PlanInfo.SubID);
		    DataSet ds;
		    _Participant.PersonalInfo.EmployerName = spDc.GetSponsorName();
		    ds = spDc.GetPlanLevelData();
    
		    if (ds.Tables[0].Rows.Count > 0)
		    {
		        var row = ds.Tables[0].Rows[0];
		        _Participant.PlanLoanInfo.PaperlessLoans = row["paperless_loan"].ToString() == "yes";
		        _Participant.PlanInfo.ServiceType = row["service_type"].ToString();
		        _Participant.PlanInfo.OnlineEnrollment = row["Online_Enrollment"].ToString() == "yes";
		        _Participant.PlanInfo.AutoEnrollment = row["auto_enrollment"].ToString() == "yes";
		        _Participant.PlanInfo.CatchupContributions = row["catchup_allowed"].ToString() == "yes";
		    }
		}
        public static string CleanMe(string s)
        {
            string allText;

            int iLoop;
            string strDataArray = string.Empty;
            allText = s.Substring(11);

            var loopTo = allText.Length;
            for (iLoop = 1; iLoop <= loopTo; iLoop += 80)
            {
                strDataArray = strDataArray + allText.Substring(iLoop - 1, Math.Min(79, allText.Length - (iLoop - 1)));
            }

            return strDataArray;

        }
        private void ParseSDBAInfo()
        {
            string strSDBAData;

            strSDBAData = _FieldTable["32"];
            {
                ref var withBlock = ref _Participant.SDBAFundInfo;
                withBlock.SDBAFlag = Convert.ToInt32(strSDBAData.Substring(0, 1));
                withBlock.SDBABal = Convert.ToDouble(FormatNumber(strSDBAData.Substring(1, 9), 7));
                strSDBAData = _FieldTable["45"];
                withBlock.SDBAInitTransferAmt = Convert.ToDouble(strSDBAData.Substring(0, 5));
                withBlock.SDBASubTransferAmt = Convert.ToDouble(strSDBAData.Substring(5, 5));

                strSDBAData = _FieldTable["46"];
                withBlock.SDBAMinBal = Convert.ToDouble(strSDBAData.Substring(0, 7));
                withBlock.SDBAMaxPct = Convert.ToDouble(strSDBAData.Substring(7, 3));

            }
        }
        private void ParseProfile(int InLoginid, string ContractID, string SubID)
        {
            string strData;
            string memberID = string.Empty;
            string planID = string.Empty;

            ParticipantDC.GetPartner(InLoginid, ContractID, SubID, ref planID, ref memberID);

            strData = _StrDataArray[C_Header];

            _Participant.PersonalInfo = new PersonalProfile();
            {
                ref var withBlock = ref _Participant.PersonalInfo;
                withBlock.MemberID = memberID;
                withBlock.PayrollFrequency = _Participant.PayrollFrequency;
                withBlock.SSN = strData.Substring(317, 9).Trim();
                withBlock.LastName = strData.Substring(0, 20).Trim();
                withBlock.FirstName = strData.Substring(20, 14).Trim();
                withBlock.MiddleInitial = _FieldTable["69"].Substring(5, 1).Trim();
                withBlock.AddressLine1 = strData.Substring(34, 33).Trim();
                withBlock.AddressLine2 = strData.Substring(67, 23).Trim();
                withBlock.City = strData.Substring(90, 33).Trim();
                withBlock.State = strData.Substring(123, 2).Trim();
                withBlock.ZipCode1 = strData.Substring(125, 5).Trim();
                withBlock.ZipCode2 = _FieldTable["40"].Substring(6, 4).Trim();
                withBlock.VestingDt = _FieldTable["09"].Trim();

                if (string.IsNullOrEmpty(withBlock.State?.ToUpper()) || (withBlock.State?.ToUpper() ?? "") == GeneralConstants.C_NON_US_STATE)
				{
				    withBlock.USAddress = false;
				    withBlock.State = GeneralConstants.C_NON_US_STATE;    
				    int iPos = withBlock.City.IndexOf(",");
				    if (iPos > 0)
				    {
				        withBlock.ZipCode1 = withBlock.City.Substring(0, iPos);
				        withBlock.Country = withBlock.City.Substring(iPos + 1).Trim();
				    }
				    else
				    {
				        withBlock.ZipCode1 = withBlock.City;
				        withBlock.Country = "";
				    }
    
				    withBlock.City = withBlock.AddressLine2;
				    withBlock.AddressLine2 = string.Empty;
				}
                else
                {
                    withBlock.USAddress = true;
                }

                withBlock.Email = _FieldTable["37"].Trim() + _FieldTable["38"].Trim() + _FieldTable["39"].Trim() + _FieldTable["40"].Substring(0, 3).Trim();
                withBlock.Telephone = _FieldTable["41"].Trim();
                withBlock.WorkTelephone = _FieldTable["54"].Trim();
                withBlock.Status = strData.Substring(331, 4).Trim();
                Regex rg;

                rg = new Regex("[^0]", RegexOptions.IgnorePatternWhitespace);

                if (!rg.IsMatch(withBlock.Telephone))
                {
                    withBlock.Telephone = "";
                }

                if (!rg.IsMatch(withBlock.WorkTelephone))
                {
                    withBlock.WorkTelephone = "";
                }

                withBlock.BirthDt = FormatDate(_FieldTable["02"], C_DateFormat10);
                withBlock.EmploymentDt = FormatDate(_FieldTable["04"], C_DateFormat10);
                withBlock.TerminationDt = FormatDate(_FieldTable["10"], C_DateFormat10);

                withBlock.LocationCode = strData.Substring(269, 5).Trim();

                withBlock.RehireDt = FormatDate(_FieldTable["51"], C_DateFormat10);
                withBlock.GroupCode = TRSManagers.RegularExpression.RegExpReplace(_FieldTable["52"].Trim());
                withBlock.Suffix = _FieldTable["53"].Trim();
                withBlock.YearsofService = int.TryParse(_FieldTable["12"].Trim(), out int yearsOfService) ? yearsOfService : 0;
                string sVal;
                sVal = _FieldTable["12"].Trim();
                if (sVal.Length >= 3)
                {
                    sVal = sVal.Substring(sVal.Length - 3);
                    if (int.TryParse(sVal, out int vestingCounter))
                    {
                        withBlock.VestingCounter = vestingCounter;
                    }
                }

                if (int.TryParse(_FieldTable["66"].Substring(4, 4).Trim(), out int hoursWorked))
                {
                    withBlock.HoursWorkedYTD = hoursWorked;
                }

                switch (_FieldTable["43"].Substring(9, 1).ToUpper() ?? "")
                {
                    case "P":
                        {
                            withBlock.eStmtPreference = "N";
                            break;
                        }
                    case "E":
                        {
                            withBlock.eStmtPreference = "Y";
                            break;
                        }

                    default:
                        {
                            withBlock.eStmtPreference = string.Empty;
                            break;
                        }
                }
                switch (_FieldTable["57"].Substring(9, 1).ToUpper() ?? "")
                {
                    case "P":
                        {
                            withBlock.eConfirmPreference = "N";
                            break;
                        }
                    case "E":
                        {
                            withBlock.eConfirmPreference = "Y";
                            break;
                        }

                    default:
                        {
                            withBlock.eConfirmPreference = string.Empty;
                            break;
                        }
                }
                switch (_FieldTable["78"].Substring(0, 1).ToUpper() ?? "")
                {
                    case "P":
                        {
                            withBlock.ReqdNoticesPreference = "N";
                            break;
                        }
                    case "E":
                        {
                            withBlock.ReqdNoticesPreference = "Y";
                            break;
                        }

                    default:
                        {
                            withBlock.ReqdNoticesPreference = string.Empty;
                            break;
                        }
                }
            }
            _Participant.PlanInfo.PlanID = planID;
        }
        private void ParsePlanInfo()
        {
            DataTable dtPlan;
            string PartnerFundID;
            var FundName = default(string);
            dtPlan = ParticipantDC.GetPlanInfo(_Participant.PersonalInfo.MemberID, _Participant.PlanInfo.PlanID).Tables[0];
            {
                ref var withBlock = ref _Participant.PlanInfo;
                withBlock.PlanName = _FieldTable["14"].Trim() + _FieldTable["15"].Trim() + _FieldTable["16"].Trim() + _FieldTable["17"].Trim();
                withBlock.ContractID = Convert.ToString(dtPlan.Rows[0]["ContractID"]);
                withBlock.SubID = Convert.ToString(dtPlan.Rows[0]["SubID"]);
                withBlock.EligAge = Convert.ToInt32(_FieldTable["43"].Substring(0, 2)).ToString();
                withBlock.EligMonths = Convert.ToInt32(_FieldTable["43"].Substring(6, 2)).ToString();
                withBlock.EligEntryDt = "";
                _Participant.EnrollmentInfo.EnrollmentDt = ParticipantDC.GetEnrollmentDate(_Participant.PersonalInfo.MemberID, withBlock.ContractID, withBlock.SubID);
                if (int.TryParse(_FieldTable["53"].Substring(_FieldTable["53"].Length - 4).Trim(), out int termDistFee))
                {
                    withBlock.Term_Dist_Fee = termDistFee;
                }
                else
                {
                    withBlock.Term_Dist_Fee = 0d;
                }
                withBlock.PlanStatus = _FieldTable["40"].Substring(3, 1);
                withBlock.DistributionRestriction = _FieldTable["40"].Substring(4, 1);
                withBlock.DefaultFund = new FundInfo();
                PartnerFundID = _FieldTable["44"].Substring(_FieldTable["44"].Length - 3);
                withBlock.DefaultFund.FundID = AudienceDC.GetTRSFundID(PartnerFundID, (int)PartnerFlag.TAE, ref FundName).ToString();
                withBlock.DefaultFund.FundName = FundName;
            }
        }
        private void ParseDeferrals()
        {
            string strData;
            string sVal;

            strData = _StrDataArray[C_Header];

            _Participant.DeferralInfo = new DeferralInfo();
            {
                ref var withBlock = ref _Participant.DeferralInfo;
                withBlock.DefType = strData.Substring(175, 1);
                if (withBlock.DefType == "A")
                {
                    withBlock.CurDefPct = Convert.ToDouble(FormatNumber(strData.Substring(146, 3), 3));
                    withBlock.MaxDefPct = Convert.ToDouble(FormatNumber(strData.Substring(149, 3), 3));
                    withBlock.MinDefPct = Convert.ToDouble(FormatNumber(strData.Substring(152, 3), 3));
                }
                else if (withBlock.DefType == "K")
                {
                    withBlock.CurDefPct = Convert.ToDouble(FormatNumber(strData.Substring(160, 3), 3));
                    withBlock.MaxDefPct = Convert.ToDouble(FormatNumber(strData.Substring(163, 3), 3));
                    withBlock.MinDefPct = Convert.ToDouble(FormatNumber(strData.Substring(166, 3), 3));
                }
                else
                {
                    withBlock.CurDefPct = Convert.ToDouble(FormatNumber(strData.Substring(160, 3), 3));
                    withBlock.MaxDefPct = Convert.ToDouble(FormatNumber(strData.Substring(169, 3), 3));
                    withBlock.MinDefPct = Convert.ToDouble(FormatNumber(strData.Substring(172, 3), 3));
                }
                withBlock.MaxDefChanges = Convert.ToInt32(FormatNumber(strData.Substring(155, 2), 2));
                withBlock.ActualDefChanges = Convert.ToInt32(FormatNumber(strData.Substring(158, 2), 2));
                withBlock.DefChangePeriod = strData.Substring(157, 1);


                switch (_FieldTable["43"].Substring(8, 1) ?? "")
                {
                    case "P":
                        {
                            withBlock.Method = TransferMethod.Percent;
                            break;
                        }
                    case "D":
                        {
                            withBlock.Method = TransferMethod.Dollar;
                            withBlock.CurDefPct = Convert.ToDouble(FormatNumber(_FieldTable["56"], 8));
                            break;
                        }
                }
                switch (_FieldTable["59"].Substring(0, 1) ?? "")
                {
                    case "P":
                        {
                            withBlock.DefValAT = Convert.ToDouble(FormatNumber(_FieldTable["59"].Substring(5), 3));
                            break;
                        }
                    case "D":
                        {
                            withBlock.DefValAT = Convert.ToDouble(FormatNumber(_FieldTable["59"].Substring(1), 7));
                            break;
                        }
                }
                sVal = FormatDate(_FieldTable["68"], C_DateFormat10);
                withBlock.ContributionRateLastChangeDate = sVal;

                sVal = Convert.ToString(FormatNumber(_FieldTable["66"].Substring(3, 1), 1));
                if (sVal == "1" || sVal == "Y")
                {
                    withBlock.AutoOptout = true;
                }
                else
                {
                    withBlock.AutoOptout = false;
                }

                if (withBlock.AutoOptout)
                {
                    return;
                }

                int iVal = 0;
                withBlock.AutomaticDeferralInfo = new AutomaticDeferralInfo();
                AutoIncreasedInfo oAutoIncrease;
                var lAutoIncrease = new List<AutoIncreasedInfo>();

                oAutoIncrease = new AutoIncreasedInfo();
                oAutoIncrease.Type = E_ContribType.Traditional401k;
                iVal = Convert.ToInt32(FormatNumber(_FieldTable["65"].Substring(0, 2), 2));
                oAutoIncrease.maxValue = iVal;
                iVal = Convert.ToInt32(FormatNumber(_FieldTable["65"].Substring(2, 2), 2));
                oAutoIncrease.minValue = iVal;
                lAutoIncrease.Add(oAutoIncrease);

                iVal = Convert.ToInt32(FormatNumber(_FieldTable["65"].Substring(4, 2), 2));
                withBlock.AutomaticDeferralInfo.AutomaticIncreaseEffectiveMonth = iVal;

                iVal = Convert.ToInt32(FormatNumber(_FieldTable["65"].Substring(6, 2), 2));
                oAutoIncrease = new AutoIncreasedInfo();
                oAutoIncrease.Type = E_ContribType.Roth401k;
                iVal = Convert.ToInt32(FormatNumber(_FieldTable["65"].Substring(8, 2), 2));
                oAutoIncrease.maxValue = iVal;
                iVal = Convert.ToInt32(FormatNumber(_FieldTable["66"].Substring(0, 2), 2));
                oAutoIncrease.minValue = iVal;
                lAutoIncrease.Add(oAutoIncrease);

                withBlock.AutomaticDeferralInfo.AutoIncreasedInfo = lAutoIncrease.ToArray();

                sVal = Convert.ToString(FormatNumber(_FieldTable["67"].Substring(3), 4));
                if (!int.TryParse(sVal, out _))
                {
                    sVal = "0";
                }
                withBlock.AutomaticDeferralInfo.AutomaticIncreaseEffectiveYear = Convert.ToInt32(sVal);

            }

        }
        private void ParseAssets()
        {
            string strData;
            var sbFundIDList = new System.Text.StringBuilder();
            int i;
            FundInfo oFundInfo;
            Hashtable oFundList;
            string sTemp;
            string PartnerFundID;
            FundStruct oFundStruct;
            int fundType;
            string strFundTable;

            strFundTable = _StrDataArray[C_FundInfo];

            if (string.IsNullOrEmpty(strFundTable))
                return;

            oFundList = new Hashtable();

            sbFundIDList.Append("<Funds>" + Environment.NewLine);

            var loopTo = strFundTable.Length;
            for (i = 1; i <= loopTo; i += 61)
            {
                strData = strFundTable.Substring(i - 1, Math.Min(61, strFundTable.Length - (i - 1)));
                fundType = Convert.ToInt32(strData.Substring(6, 1));
                PartnerFundID = strData.Substring(0, 3);

                if (Array.IndexOf(ExcludedFunds, PartnerFundID) == -1)
                {
                    if (fundType == C_FundType_Sdba)
                    {
                        _Participant.SDBAFundInfo.SDBAFundSequence = strData.Substring(3, 2);
                    }
                    if (!oFundList.Contains(PartnerFundID))
                    {
                        oFundInfo = new FundInfo();
                        oFundInfo.PartnerFundID = PartnerFundID;
                        oFundInfo.FundSequenceNumber = strData.Substring(3, 2);
                        oFundInfo.MinContribution = Convert.ToDouble(strData.Substring(11, 3));
                        oFundInfo.MaxContribution = Convert.ToDouble(strData.Substring(14, 3));

                        oFundInfo.ContrDirection = Convert.ToDouble(strData.Substring(17, 3));
                        oFundInfo.ExistingPct = Convert.ToDouble(strData.Substring(30, 3));
                        oFundInfo.TransPending = strData.Substring(54, 1);
                        sTemp = strData.Substring(8, 1);
                        if (sTemp == "1")
                        {
                            oFundInfo.TransfersInAllowed = false;
                        }
                        else
                        {
                            oFundInfo.TransfersInAllowed = true;
                        }

                        sTemp = strData.Substring(9, 1);
                        if (sTemp == "1")
                        {
                            oFundInfo.TransfersOutAllowed = false;
                        }
                        else
                        {
                            oFundInfo.TransfersOutAllowed = true;
                        }

                        if ((PartnerFundID ?? "") == C_TFAHYFundID1 || (PartnerFundID ?? "") == C_TFAHYFundID2 || (PartnerFundID ?? "") == C_TFAHYFundID3)
                        {
                            oFundInfo.DisplayOnly = true;
                        }
                        else
                        {
                            oFundInfo.DisplayOnly = false;
                        }

                        if (oFundInfo.TransPending == "X")
                        {
                            oFundInfo.TransfersInAllowed = false;
                            oFundInfo.TransfersOutAllowed = false;
                            oFundInfo.MaxContribution = 0d;
                            oFundInfo.MinContribution = 0d;
                        }

                        oFundInfo.FundType = (FundType)fundType;
                        sbFundIDList.Append("<Fund>" + oFundInfo.PartnerFundID + "</Fund>" + Environment.NewLine);
                        oFundStruct = (FundStruct)_FundBalance[oFundInfo.PartnerFundID];
                        oFundInfo.FundBalance = Convert.ToDouble(FormatNumber(strData.Substring(20, 10), 8));
                        oFundInfo.VstVal = oFundStruct.VstVal;
                        oFundInfo.Units = oFundStruct.Units;

                        oFundInfo.UnitPrice = Convert.ToDouble(FormatNumber(strData.Substring(43, 11), 3));
                        oFundList.Add(oFundInfo.PartnerFundID, oFundInfo);
                        if (oFundInfo.FundType == FundType.LoanFund)
                        {
                            _LoanFund = oFundInfo;
                        }
                    }
                }
            }

            if (_LoanFund == null)
            {
                _LoanFund = GetLoanFund();
                if (!(_LoanFund == null))
                {
                    sbFundIDList.Append("<Fund>" + _LoanFund.PartnerFundID + "</Fund>" + Environment.NewLine);
                    oFundList.Add(_LoanFund.PartnerFundID, _LoanFund);
                }
            }

            sbFundIDList.Append("</Funds>");

            if (oFundList.Count > 0)
            {
                _Participant.FundInfo = new FundInfo[oFundList.Count];
                i = 0;
                foreach (FundInfo currentOFundInfo in oFundList.Values)
                {
                    oFundInfo = currentOFundInfo;
                    _Participant.FundInfo[i] = oFundInfo;
                    i += 1;
                }
            }

            if (IgnoreFundMappings == false)
            {
                UpdateAssetFundMappigs(sbFundIDList.ToString());
            }

        }
        private void ParseWithdrawals()
		{
		    int i;
			
		    string strWdwlTable;

		    strWdwlTable = _StrDataArray[C_WithdrawalInfo];

		    if (string.IsNullOrEmpty(strWdwlTable)) 
		        return;

		    for (i = 1; i <= strWdwlTable.Length; i += 31)
		    {
		        if (i + 30 > strWdwlTable.Length)
		            break;

		        string recordType = strWdwlTable.Substring(i - 1, 3);
        
		        switch (recordType)
		        {
		            case "HT1":
		                _Participant.HardshipAmt = Convert.ToDouble(FormatNumber(strWdwlTable.Substring(i + 18, 10), 8));
		                break;
                
		            case "HT2":
		                _Participant.AfterTaxHardshipAmt = Convert.ToDouble(FormatNumber(strWdwlTable.Substring(i + 18, 10), 8));
		                break;
                
		            case "HT3":
		                _Participant.AfterTaxInservice59Amt = Convert.ToDouble(FormatNumber(strWdwlTable.Substring(i + 18, 10), 8));
		                _Participant.AfterTaxInservice62Amt = _Participant.AfterTaxInservice59Amt;
		                break;
                
		            case "HT4":
		                _Participant.AfterTaxInserviceAmt = Convert.ToDouble(FormatNumber(strWdwlTable.Substring(i + 18, 10), 8));
		                break;
                
		            case "HT5":
		                _Participant.MaxAmt59Withdrawal = Convert.ToDouble(FormatNumber(strWdwlTable.Substring(i + 18, 10), 8));
		                _Participant.MaxAmt62Withdrawal = _Participant.MaxAmt59Withdrawal;
		                break;
                
		            case "HT6":
		                _Participant.InserviceAmt = Convert.ToDouble(FormatNumber(strWdwlTable.Substring(i + 18, 10), 8));
		                break;
		        }
		    }
		}
        private void UpdateAssetFundMappigs(string strFundIDList)
        {
            int AssetID, FundID;
            string AssetName = string.Empty;
            string FundName = string.Empty;
            DataView dvFundMapping;
            dvFundMapping = GetFundMappingsDataView(strFundIDList);

            dvFundMapping.Sort = "partner_fund_id";
            int FundIndex = 0;

            _FundSequenceMappings = new Hashtable();

            var oFundStruct = default(FundStruct);
            foreach (var oFundInfo in _Participant.FundInfo)
            {

                AssetID = 0;
                FundID = 0;
                GetAssetFundMappings(dvFundMapping, oFundInfo.PartnerFundID, ref AssetID, ref AssetName, ref FundID, ref FundName);
                if (AssetID != 0 & FundID != 0)
                {
                    oFundStruct.FundID = FundID;
                    oFundStruct.FundName = FundName;
                    _FundSequenceMappings.Add(oFundInfo.FundSequenceNumber, oFundStruct);
                    oFundInfo.AssetID = AssetID.ToString();
                    oFundInfo.AssetName = AssetName;
                    oFundInfo.FundID = FundID.ToString();
                    oFundInfo.FundName = FundName;
                    if ((_BVAFundSequence ?? "") == (oFundInfo.FundSequenceNumber ?? ""))
                    {
                        _Participant.PlanInfo.BVAFundID = FundID;
                    }
                    FundIndex += 1;
                }
            }
            dvFundMapping.Dispose();
        }
        private void GetAssetFundMappings(DataView dvFundMapping, string partnerFundID, ref int assetID, ref string assetName, ref int fundID, ref string fundName)
        {
            int index = dvFundMapping.Find(partnerFundID);

            if (index == -1)
            {
                throw new Exception("Mapping not found for TAE Fund ID:" + partnerFundID.ToString());
            }

            else
            {
                var dr = dvFundMapping[index].Row;
                assetID = Convert.ToInt32(dr["asset_id"]);
                assetName = Convert.ToString(dr["asset_name"]).Trim();
                fundID = Convert.ToInt32(dr["fund_id"]);
                fundName = Convert.ToString(dr["fund_name"]).Trim();
            }
        }
        private DataView GetFundMappingsDataView(string fundIDList)
        {
            DataSet ds;
            ds = TRSSqlHelper.ExecuteDataset(General.ConnectionString, "pSI_FundMappings", [PartnerFlag.TAE, fundIDList]);
            return ds.Tables[0].DefaultView;
        }
        private void ParseAccountBalance()
        {
            AccountInfo oAccountInfo;
            int i;
            string strData;
            ArrayList oAccountList;
            int fundType;
            string partnerFundId;

            string strAccountIDList, strFundIDList;

            if (string.IsNullOrEmpty(_StrDataArray[C_AccountInfo]))
                return;

            oAccountList = new ArrayList();

            strFundIDList = "<Funds>" + Environment.NewLine;
            strAccountIDList = "<Accounts>" + Environment.NewLine;

            var loopTo = _StrDataArray[C_AccountInfo].Length;
            for (i = 1; i <= loopTo; i += 51)
            {
                strData = _StrDataArray[C_AccountInfo].Substring(i - 1, Math.Min(51, _StrDataArray[C_AccountInfo].Length - (i - 1)));
                fundType = (int)(FundType)Convert.ToInt32(strData.Substring(3, 1));
                partnerFundId = strData.Substring(0, 3);
                if (Array.IndexOf(ExcludedFunds, partnerFundId) == -1)
                {
                    oAccountInfo = new AccountInfo();
                    oAccountInfo.FundType = (FundType)fundType;
                    oAccountInfo.PartnerFundID = partnerFundId;
                    oAccountInfo.PartnerAccountID = strData.Substring(4, 3);
                    oAccountInfo.Units = Convert.ToString(FormatNumber(strData.Substring(17, 10), 7));
                    oAccountInfo.VstVal = Convert.ToDouble(FormatNumber(strData.Substring(37, 10), 8));
                    oAccountInfo.VstPct = Convert.ToDouble(FormatNumber(strData.Substring(47, 4), 1, 1));
                    oAccountInfo.MarketVal = Convert.ToDouble(FormatNumber(strData.Substring(7, 10), 8));
                    if (Convert.ToDouble(oAccountInfo.PartnerAccountID) == 0d)
                    {
                        FundStruct oFundStruct = default;
                        oFundStruct.FundBalance = oAccountInfo.MarketVal;
                        oFundStruct.Units = Convert.ToDouble(oAccountInfo.Units);
                        oFundStruct.VstVal = oAccountInfo.VstVal;
                        if (!_FundBalance.ContainsKey(oAccountInfo.PartnerFundID))
                        {
                            _FundBalance.Add(oAccountInfo.PartnerFundID, oFundStruct);
                        }
                    }

                    if (Convert.ToDouble(oAccountInfo.PartnerAccountID) == 0d & oAccountInfo.FundType == FundType.SDBAFund)
                    {
                        _Participant.SDBAFundInfo.SDBACash = _Participant.SDBAFundInfo.SDBACash + Convert.ToDouble(FormatNumber(strData.Substring(27, 10), 8));
                    }

                    oAccountList.Add(oAccountInfo);
                    strFundIDList = strFundIDList + "<Fund>" + oAccountInfo.PartnerFundID + "</Fund>" + Environment.NewLine;
                    strAccountIDList = strAccountIDList + "<Account>" + oAccountInfo.PartnerAccountID + "</Account>" + Environment.NewLine;
                }
            }
            strFundIDList = strFundIDList + "</Funds>" + Environment.NewLine;
            strAccountIDList = strAccountIDList + "</Accounts>" + Environment.NewLine;
            _Participant.AccountInfo = (AccountInfo[])oAccountList.ToArray(typeof(AccountInfo));
            if (IgnoreFundMappings == false)
            {
                UpdateAccountInfoMappings(strFundIDList, strAccountIDList);
            }
        }
        private FundInfo GetLoanFund()
        {
            foreach (var oAccountInfo in _Participant.AccountInfo)
            {
                if (oAccountInfo.FundType == FundType.LoanFund & Convert.ToDouble(oAccountInfo.AccID) == 0d)
                {
                    _LoanFund = new FundInfo();
                    {
                        ref var withBlock = ref _LoanFund;
                        withBlock.PartnerFundID = oAccountInfo.PartnerFundID;
                        withBlock.Units = Convert.ToDouble(oAccountInfo.Units);
                        if (Convert.ToDouble(oAccountInfo.Units) != 0d & oAccountInfo.MarketVal != 0d)
                        {
                            withBlock.UnitPrice = oAccountInfo.MarketVal / Convert.ToDouble(oAccountInfo.Units);
                        }
                        else
                        {
                            withBlock.UnitPrice = 1d;
                        }
                        withBlock.VstVal = oAccountInfo.VstVal;
                        withBlock.FundBalance = oAccountInfo.MarketVal;
                        withBlock.ContrDirection = 0d;
                        withBlock.ExistingPct = 0d;
                        withBlock.TransfersInAllowed = false;
                        withBlock.TransfersOutAllowed = false;
                        withBlock.FundID = oAccountInfo.FundID;
                        withBlock.FundType = oAccountInfo.FundType;
                        withBlock.FundSequenceNumber = _StrDataArray[C_Header].Substring(278, 2);
                    }
                    break;
                }
            }
            return _LoanFund;
        }
        private void UpdateAccountInfoMappings(string strFundIDList, string strAccountIDList)
        {
            var dvFunds = GetFundMappingsDataView(strFundIDList);
            var dvAccounts = GetAccountMappingDataView(strAccountIDList);

            dvAccounts.Sort = "partner_acc_id";

            dvFunds.Sort = "partner_fund_id";
            foreach (var oAccountInfo in _Participant.AccountInfo)
            {
                if (Convert.ToDouble(oAccountInfo.PartnerAccountID) == 0d)
                {
                    oAccountInfo.AccName = "Total Account Balance";
                    oAccountInfo.AccID = 0.ToString();
                }
                else
                {
                    int AccountIndex;
                    DataRow drAccount;
                    AccountIndex = dvAccounts.Find(oAccountInfo.PartnerAccountID);
                    if (AccountIndex != -1)
                    {
                        drAccount = dvAccounts[AccountIndex].Row;
                        oAccountInfo.AccName = Convert.ToString(drAccount["acc_name"]).Trim();
                        oAccountInfo.AccID = Convert.ToString(drAccount["acc_id"]);
                    }
                }

                if (oAccountInfo.PartnerFundID == "000")
                {
                    oAccountInfo.FundName = "All Funds";
                    oAccountInfo.FundID = "0";
                }
                else
                {
                    int FundIndex;
                    DataRow drFund;
                    FundIndex = dvFunds.Find(oAccountInfo.PartnerFundID);
                    if (FundIndex == -1)
                    {
                        throw new Exception("Mapping not found for partner fund:" + oAccountInfo.PartnerFundID);
                    }
                    else
                    {
                        drFund = dvFunds[FundIndex].Row;
                        oAccountInfo.FundName = Convert.ToString(drFund["fund_name"]).Trim();
                        oAccountInfo.FundID = Convert.ToString(drFund["fund_id"]);
                    }
                }
            }

        }
        private DataView GetAccountMappingDataView(string partnerAccountList)
        {
            DataView dv;

            dv = TRSSqlHelper.ExecuteDataset(General.ConnectionString, "pSI_AccountMappings", [PartnerFlag.TAE, partnerAccountList]).Tables[0].DefaultView;
            return dv;

        }
        private void ParseLoans()
        {
            if (_StrDataArray[C_LoanInfo] is null || _StrDataArray[C_LoanInfo].Length == 0)
            {
                return;
            }
            int iLoop;
            ArrayList oLoanList;
            string strLoanData;
            LoanInfo oLoanInfo;

            oLoanList = new ArrayList();

            var loopTo = _StrDataArray[C_LoanInfo].Length;
            for (iLoop = 1; iLoop <= loopTo; iLoop += 110)
            {
                strLoanData = _StrDataArray[C_LoanInfo].Substring(iLoop - 1, Math.Min(110, _StrDataArray[C_LoanInfo].Length - (iLoop - 1)));
                oLoanInfo = new LoanInfo();
                oLoanInfo.LoanNumber = strLoanData.Substring(0, 2);
                oLoanInfo.LoanDt = FormatDate(strLoanData.Substring(2, 10), C_DateFormat10);
                oLoanInfo.CurBal = Convert.ToDouble(FormatNumber(strLoanData.Substring(12, 8), 6, 0));
                oLoanInfo.PmtAmt = Convert.ToDouble(FormatNumber(strLoanData.Substring(20, 8), 6, 0));
                oLoanInfo.LoanIntRate = Convert.ToDouble(FormatNumber(strLoanData.Substring(46, 5), 2));
                oLoanInfo.MaturityDt = FormatDate(strLoanData.Substring(51, 10), C_DateFormat10);
                oLoanInfo.FirstPmtDt = FormatDate(strLoanData.Substring(96, 10), C_DateFormat10);
                oLoanInfo.LoanAmt = Convert.ToDouble(FormatNumber(strLoanData.Substring(61, 8), 6));
                oLoanInfo.PayOffAmt = Convert.ToDouble(FormatNumber(strLoanData.Substring(28, 8), 6));
                oLoanInfo.RemPayments = Convert.ToInt32(FormatNumber(strLoanData.Substring(93, 3), 3));
                oLoanInfo.YTDIntPaid = Convert.ToDouble(FormatNumber(strLoanData.Substring(77, 8), 6));
                oLoanInfo.TotalIntPaid = Convert.ToDouble(FormatNumber(strLoanData.Substring(85, 8), 6));
                oLoanInfo.NumberOfPayments = Convert.ToInt32(FormatNumber(strLoanData.Substring(106, 3), 3));
                switch (strLoanData.Substring(109, 1) ?? "")
                {
                    case "B":
                        {
                            oLoanInfo.PaymentFrequency = LoanPaymentFrequency.Every2Weeks;
                            break;
                        }
                    case "M":
                        {
                            oLoanInfo.PaymentFrequency = LoanPaymentFrequency.Monthly;
                            break;
                        }
                    case "Q":
                        {
                            oLoanInfo.PaymentFrequency = LoanPaymentFrequency.Quarterly;
                            break;
                        }
                    case "H":
                        {
                            oLoanInfo.PaymentFrequency = LoanPaymentFrequency.TwiceAMonth;
                            break;
                        }
                    case "W":
                        {
                            oLoanInfo.PaymentFrequency = LoanPaymentFrequency.Weekly;
                            break;
                        }

                    default:
                        {
                            oLoanInfo.PaymentFrequency = LoanPaymentFrequency.Unknown;
                            break;
                        }

                }
                oLoanList.Add(oLoanInfo);
            }

            _Participant.LoanInfo = (LoanInfo[])oLoanList.ToArray(typeof(LoanInfo));
            oLoanList.Clear();
        }
        private void ParseTransactionHistory()
        {
            var oArrayList = new ArrayList();
            TransactionHistoryInfo oTransactionHistoryInfo;
            FundStruct oFundStruct;
            string HostFID;
            string PrevDate = string.Empty;
            string lastContrDate = string.Empty;
            var PrevActivity = default(int);
            string ActivityCode;

            var msgReader = new MessageReader(_StrDataArray[C_TransactionHistory]);

            while (!msgReader.EOF())
            {
                oTransactionHistoryInfo = new TransactionHistoryInfo();


                oTransactionHistoryInfo.TxnDate = msgReader.ReadString(10);
                if (!DateTime.TryParse(oTransactionHistoryInfo.TxnDate, out _))
                {
                    oTransactionHistoryInfo.TxnDate = PrevDate;
                }
                else
                {
                    PrevDate = oTransactionHistoryInfo.TxnDate;
                }
                HostFID = msgReader.ReadString(2);
                oTransactionHistoryInfo.Amount = Convert.ToDouble(msgReader.ReadString(14));
                oTransactionHistoryInfo.Shares = Convert.ToDouble(msgReader.ReadString(14));
                oFundStruct = (FundStruct)_FundSequenceMappings[HostFID];
                ActivityCode = msgReader.ReadString(1);
                if (!int.TryParse(ActivityCode, out _))
                {
                    ActivityCode = (Convert.ToInt32(ActivityCode.ToUpper()[0]) - 54).ToString();
                }
                oTransactionHistoryInfo.ActivityCode = (TransactionActivityCode)Convert.ToInt32(ActivityCode);
                if ((int)oTransactionHistoryInfo.ActivityCode == 6)
                {
                    oTransactionHistoryInfo.ActivityCode = (TransactionActivityCode)PrevActivity;
                }
                else
                {
                    PrevActivity = (int)oTransactionHistoryInfo.ActivityCode;
                }

                oTransactionHistoryInfo.FundID = oFundStruct.FundID;
                oTransactionHistoryInfo.FundName = oFundStruct.FundName;

                oTransactionHistoryInfo.MoneyTypeID = GetMoneyTypeBasedOnLookup(Convert.ToInt32(msgReader.ReadString(2)));
                oTransactionHistoryInfo.UnitPrice = 0d;

                if (oTransactionHistoryInfo.ActivityCode == TransactionActivityCode.Contribution)
                {
                    if (string.IsNullOrEmpty(lastContrDate))
                    {
                        lastContrDate = oTransactionHistoryInfo.TxnDate;
                    }
                    if ((lastContrDate ?? "") == (oTransactionHistoryInfo.TxnDate ?? ""))
                    {
                        switch (oTransactionHistoryInfo.MoneyTypeID)
                        {
                            case 120:
                            case 433:
                            case 644:
                            case 664:
                            case 752:
                                {
                                    _Participant.PersonalInfo.ContributionAmount_Elective += oTransactionHistoryInfo.Amount;
                                    break;
                                }
                            case 1:
                            case 140:
                            case 307:
                            case 327:
                            case 347:
                            case 352:
                            case 353:
                            case 367:
                            case 372:
                            case 373:
                            case 387:
                            case 407:
                            case 427:
                            case 619:
                            case 623:
                            case 652:
                                {
                                    _Participant.PersonalInfo.ContributionAmount_Matching += oTransactionHistoryInfo.Amount;
                                    break;
                                }

                            default:
                                {
                                    _Participant.PersonalInfo.ContributionAmount_Other += oTransactionHistoryInfo.Amount;
                                    break;
                                }
                        }
                    }

                }
                if (oTransactionHistoryInfo.FundID != 0)
                {
                    oArrayList.Add(oTransactionHistoryInfo);
                }

            }

            msgReader.Close();

            _Participant.TransactionHistoryInfo = (TransactionHistoryInfo[])oArrayList.ToArray(typeof(TransactionHistoryInfo));
            oArrayList.Clear();
        }
        public int GetMoneyTypeBasedOnLookup(int lookup)
        {
            var oFieldTable = new FieldTable(_StrDataArray[C_FieldTable]);
            int fieldNum;
            int position;
            string retVal;

            if (lookup < 0)
            {
                return default;
            }

            fieldNum = (int)Math.Round(Math.Ceiling(lookup / 3d - 1d) + 20d);

            position = lookup - (fieldNum - 20) * 3 - 1;

            retVal = oFieldTable[fieldNum.ToString("00")].Substring(position * 3, 3);

            return Convert.ToInt32(retVal);
        }
        public void ParseSecurityInfo()
        {

            var arrPageList = new ArrayList();
            var arrMenuList = new ArrayList();

            if (_StrDataArray[0].Substring(210, 1) != "0")
            {
                arrMenuList.Add(C_Menu_InvestmentElections);
            }

            if (_Participant.TransPending != "0")
            {
                arrMenuList.Add(C_Menu_TransferFunds);
                arrMenuList.Add(C_Menu_Loans);
                arrMenuList.Add(C_Menu_RebalanceFunds);
            }
            else if (_Participant.PlanLoanInfo.IsLoanAllowed == false)
            {
                arrMenuList.Add(C_Menu_Loans);
            }
            _Participant.ExcludedMenuList = string.Join(",", arrMenuList.ToArray());
            arrMenuList.Clear();

        }
        public static object FormatNumber(string strNumber, int iFirstCount, short iPercent = 0, short iFormatCurrency = 0)
        {
            int i, iLen;
            string strIntPart, strFloatPart;
            double dFormatCurrency;
            try
            {
                if (!string.IsNullOrEmpty(strNumber))
                {
                    strIntPart = strNumber.Substring(0, Math.Min(iFirstCount, strNumber.Length));
                    iLen = strIntPart.Length;
                    var loopTo = iLen;
                    for (i = 1; i <= loopTo; i++)
                    {
                        if (strIntPart.Substring(0, 1) == "0")
                        {
                            strIntPart = strIntPart.Substring(1);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(strNumber.Substring(Math.Min(iFirstCount, strNumber.Length))))
                    {
                        if (string.IsNullOrEmpty(strIntPart))
                        {
                            return "0";
                        }
                        else
                        {
                            return strIntPart;
                        }
                    }
                    else
                    {
                        strFloatPart = strIntPart + "." + strNumber.Substring(Math.Min(iFirstCount, strNumber.Length));
                        if (strFloatPart.Trim() == ".")
                        {
                            strFloatPart = string.Empty;
                        }
                        else
                        {
                            if (iPercent == 1)
                            {
                                strFloatPart = (Convert.ToDouble(strFloatPart) * 100d).ToString();
                            }
                            if (iFormatCurrency == 1)
                            {
                                dFormatCurrency = Convert.ToDouble(strFloatPart);
                                strFloatPart = dFormatCurrency.ToString("C");
                            }
                        }
                        return strFloatPart;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                throw;
            }
            return null;
        }
        private static string FormatDate(string strDate, string sDateFormat)
        {
            try
            {
                if ((sDateFormat ?? "") == C_DateFormat8)
                {
                    if ((strDate ?? "") != C_DefaultDate8)
                    {
                        strDate = strDate.Substring(0, 2) + "/" + strDate.Substring(2, 2) + "/" + strDate.Substring(4);
                    }
                }
                if (!DateTime.TryParse(strDate, out _))
                {
                    strDate = null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                throw;
            }
            return strDate;
        }
        private class FieldTable : Hashtable
        {
            public FieldTable(string strFieldData)
            {
                int i;
                var loopTo = strFieldData.Length - 1;
                for (i = 0; i <= loopTo; i += 12)
                    base.Add(strFieldData.Substring(i, 2), strFieldData.Substring(i + 2, 10));
            }
            public new string this[object key]
            {
                get
                {
                    if (base.ContainsKey(key))
                    {
                        return Convert.ToString(base[key]);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }
        public static string Format6000Message(string sessionID, bool UseDefaultUserID = true)
        {
            string memberID, planID;
            string userID;
            SessionInfo oSessionInfo;

            oSessionInfo = AudienceDC.GetSessionInfo(sessionID);

            memberID = oSessionInfo.PartnerUserID;
            planID = oSessionInfo.PlanID;

            if (UseDefaultUserID == true)
            {
                userID = DefaultSettings.USERID();
            }
            else
            {
                userID = DefaultSettings.USERID();
            }

            if (oSessionInfo.SubID != "000" && ParticipantDC.IsMEPContract(oSessionInfo.ContractID, oSessionInfo.SubID))
            {
                oSessionInfo.SubID = "000";
            }

            return C_ParticipantMessage + DefaultSettings.BANKCODE(oSessionInfo.SubID) + DefaultSettings.CID() + planID + memberID.PadRight(10, ' ') + DefaultSettings.FPID(planID) + userID.PadRight(8, ' ') + "/";

        }
        public static string Format6000Message(string planID, string memberID, string UserID = "", string SubID = "000")
        {
            if (string.IsNullOrEmpty(UserID))
            {
                return C_ParticipantMessage + DefaultSettings.BANKCODE(SubID) + DefaultSettings.CID() + planID + memberID.PadRight(10, ' ') + DefaultSettings.FPID(planID) + DefaultSettings.USERID().PadRight(8, ' ') + "/";
            }
            else
            {
                return C_ParticipantMessage + DefaultSettings.BANKCODE(SubID) + DefaultSettings.CID() + planID + memberID.PadRight(10, ' ') + DefaultSettings.FPID(planID) + UserID.PadRight(8, ' ') + "/";
            }
        }
        public static string Format6007Message(string sessionID, string sMQCommand)
        {
            string memberID = string.Empty;
            string planID = string.Empty;
            SessionInfo oSessionInfo;

            oSessionInfo = AudienceDC.GetSessionInfo(sessionID);

            ParticipantDC.GetPartner(sessionID, ref planID, ref memberID);
            return "6007" + DefaultSettings.BANKCODE(oSessionInfo.SubID) + DefaultSettings.CID() + planID + memberID.PadRight(10, ' ') + DefaultSettings.FPID(planID) + DefaultSettings.USERID().PadRight(8, ' ') + "/" + sMQCommand;
        }
        public static string Format6007Message(string memberID, string planID, string sMQCommand, string SubID = "000")
        {
            return "6007" + DefaultSettings.BANKCODE(SubID) + DefaultSettings.CID() + planID + memberID.PadRight(10, ' ') + DefaultSettings.FPID(planID) + DefaultSettings.USERID().PadRight(8, ' ') + "/" + sMQCommand;
        }
        public static SIResponse ParseMQResponse(string customError, string classFunctionName, string mqResponse, [Optional] ref string ErrorCode, bool Validate6000 = true)
        {
            var oSIResponse = new SIResponse();
            oSIResponse.Errors[0] = GetTAEErrorInfo(mqResponse, true, ref ErrorCode, Validate6000);
            if (oSIResponse.Errors[0].Number != 0)
            {
                oSIResponse.Errors[0].Description = General.FormatErrorMsg(oSIResponse.Errors[0].Description, customError, "ParticipantAdapter::" + classFunctionName);
            }
            else if (Validate6000)
            {
                var oFieldTable = new FieldTable("");
                string[] _strDataArray;

                _strDataArray = SplitMQResponse(mqResponse, ref oSIResponse.Errors[0], ref ErrorCode);

                if (oSIResponse.Errors[0].Number == 0)
                {
                    oFieldTable = new FieldTable(_strDataArray[C_FieldTable]);
                    oSIResponse.ConfirmationNumber = oFieldTable["48"] + oFieldTable["49"].Substring(0, Math.Min(5, oFieldTable["49"].Length)).Trim();
                }

            }

            return oSIResponse;
        }
        public static string FormatConfirmationLetterMessage(ConfirmationLetterInfo oConfirmationInfo)
        {

            string sLetterType;
            int iCount, iCount2, iCount3;
            var sbRequest = new System.Text.StringBuilder();

            var loopTo = oConfirmationInfo.Letters.Length - 1;
            for (iCount = 0; iCount <= loopTo; iCount++)
            {
                sLetterType = oConfirmationInfo.Letters[iCount].LetterType.ToString("00");
                sbRequest.Append(sLetterType);
                switch (sLetterType ?? "")
                {
                    case C_WebsiteRegistration_Letter:
                        {
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].Username.PadRight(35, ' '));
                            var loopTo1 = oConfirmationInfo.Letters[iCount].Contract.Length - 1;
                            for (iCount2 = 0; iCount2 <= loopTo1; iCount2++)
                                sbRequest.Append(oConfirmationInfo.Letters[iCount].Contract[iCount2].PlanName.PadRight(255, ' '));
                            break;
                        }
                    case C_PasswordChange_Letter:
                        {
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].ConfirmationNumber.PadRight(15, ' '));
                            break;
                        }
                    case C_TempPassword_Letter:
                        {
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].TemporaryPwd.PadRight(35, ' '));
                            break;
                        }
                    case C_UserNameChange_Letter:
                        {
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].ConfirmationNumber.PadRight(15, ' '));
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].Username.PadRight(35, ' '));
                            break;
                        }
                    case C_PasswordAssociation_Letter:
                        {
                            var loopTo2 = oConfirmationInfo.Letters[iCount].Contract.Length - 1;
                            for (iCount2 = 0; iCount2 <= loopTo2; iCount2++)
                                sbRequest.Append(oConfirmationInfo.Letters[iCount].Contract[iCount2].PlanName.PadRight(255, ' '));
                            break;
                        }
                    case C_AddressChange_Letter:
                        {
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].ConfirmationNumber.PadRight(15, ' '));
                            if (oConfirmationInfo.Letters[iCount].OldAddressLine1 is null)
                            {
                                oConfirmationInfo.Letters[iCount].OldAddressLine1 = "";
                            }
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].OldAddressLine1.PadRight(30, ' '));

                            if (oConfirmationInfo.Letters[iCount].OldAddressLine2 is null)
                            {
                                oConfirmationInfo.Letters[iCount].OldAddressLine2 = "";
                            }
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].OldAddressLine2.PadRight(30, ' '));

                            if (oConfirmationInfo.Letters[iCount].OldAddressLine3 is null)
                            {
                                oConfirmationInfo.Letters[iCount].OldAddressLine3 = "";
                            }
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].OldAddressLine3.PadRight(30, ' '));

                            if (oConfirmationInfo.Letters[iCount].OldAddressLine4 is null)
                            {
                                oConfirmationInfo.Letters[iCount].OldAddressLine4 = "";
                            }
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].OldAddressLine4.PadRight(30, ' '));
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].DayPhoneNumber.PadRight(14, ' '));
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].EveningPhoneNumber.PadRight(14, ' '));
                            break;
                        }
                    case C_PinChange_Letter:
                        {
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].ConfirmationNumber.PadRight(15, ' '));
                            break;
                        }
                    case C_PinAssociation_Letter:
                        {
                            sbRequest.Append(oConfirmationInfo.Letters[iCount].ConfirmationNumber.PadRight(15, ' '));
                            var loopTo3 = oConfirmationInfo.Letters[iCount].Contract.Length - 1;
                            for (iCount2 = 0; iCount2 <= loopTo3; iCount2++)
                                sbRequest.Append(oConfirmationInfo.Letters[iCount].Contract[iCount2].PlanName.PadRight(255, ' '));
                            break;
                        }
                    case C_NoticeOfUndeliverableEmail_Letter:
                        {
                            var loopTo4 = oConfirmationInfo.Letters[iCount].UndeliverableDocuments.Length - 1;
                            for (iCount3 = 0; iCount3 <= loopTo4; iCount3++)
                            {
                                sbRequest.Append(oConfirmationInfo.Letters[iCount].UndeliverableDocuments[iCount3]);
                                sbRequest.Append(@"\r");
                            }

                            break;
                        }
                }
                sbRequest.Append(C_Multiple);
            }

            return sbRequest.ToString();

        }
        public static ErrorInfo GetTAEErrorInfo(string mqMessage, bool CheckUpdateResult, ref string ErrorCode, bool Validate6000 = true)
		{
		    var oErrorInfo = new ErrorInfo();
		    string strData = mqMessage;
    
		    if (string.IsNullOrEmpty(strData) || strData == "-1")
		    {
		        oErrorInfo.Number = -1;
		        oErrorInfo.Description = "Unknown Error! Missing Response Generated";
		        General.LogErrors(@"TAE_Adapter\Employee\ParticipantConverter.vb", "N/A", "BusinessFacadeLayer", oErrorInfo.Description, mqMessage);
		        return oErrorInfo;
		    }

		    strData = strData.Replace("\0", " ");
		    string strFlag = strData.Length >= 10 ? strData.Substring(0, 10) : strData;

		    if (!strFlag.StartsWith("000"))
		    {
		        string strFlag2 = strFlag.Length >= 9 ? strFlag.Substring(3, 6) : strFlag.Substring(3);
        
		        switch (strFlag2)
		        {
		            case "EC9999":
		            case "EC9997":
		            case "EC9996":
		            case "EC9995":
		                oErrorInfo = ErrorHandler.GetPartnerErrorInfo(PartnerFlag.TAE, strFlag2);
		                ErrorCode = strFlag;
		                break;
                
		            case "EC2098":
		                if (strData.IndexOf("UNABLE TO ALLOCATE FILES") > 0)
		                {
		                    oErrorInfo = new ErrorInfo
		                    {
		                        Number = (int)ErrorCodes.PartnerUnavailable,
		                        Description = strData.Length > 10 ? strData.Substring(10) : strData
		                    };
		                }
		                else
		                {
		                    oErrorInfo = ErrorHandler.GetPartnerErrorInfo(PartnerFlag.TAE, strFlag2);
		                    if (oErrorInfo.Number != (int)ErrorCodes.Unknown && strData.Length > 10)
		                    {
		                        oErrorInfo.Description = strData.Substring(10);
		                    }
		                }
		                ErrorCode = strFlag;
		                break;
                
		            default:
		                if (strData.Length >= 15 && strData.Substring(10, 5) == "CURE0")
		                {
		                    string strFlagCure = strData.Substring(10, 8);
		                    oErrorInfo = ErrorHandler.GetPartnerErrorInfo(PartnerFlag.TAE, strFlagCure);
		                    ErrorCode = strFlagCure;
		                }
		                else
		                {
		                    oErrorInfo = ErrorHandler.GetPartnerErrorInfo(PartnerFlag.TAE, strFlag);
		                    ErrorCode = strFlag;
		                }
		                break;
		        }
		    }
		    else if (CheckUpdateResult && Validate6000)
		    {
		        string[] strDataArray = SplitMQResponse(strData, ref oErrorInfo, ref ErrorCode);
		        if (oErrorInfo.Number == 0 && strDataArray.Length > C_FieldTable)
		        {
		            strFlag = strDataArray[C_FieldTable].Length >= 84 
		                ? strDataArray[C_FieldTable].Substring(74, 10)
		                : "";
                
		            if (strFlag.Length >= 6 && !strFlag.EndsWith("000000"))
		            {
		                oErrorInfo = ErrorHandler.GetPartnerErrorInfo(PartnerFlag.TAE, strFlag);
		                ErrorCode = strFlag;
		            }
		        }
		    }

            return oErrorInfo;
        }
        public static string FormatUpdateProfileMessage(string sessionID, string msg6007Response, PersonalProfile profile)
        {
            char[] mqMessage = new string(' ', 485).ToCharArray();

            Format6000Message(sessionID).CopyTo(0, mqMessage, 0, Math.Min(Format6000Message(sessionID).Length, 41));
            "097004P".CopyTo(0, mqMessage, 41, 7);
            msg6007Response.CopyTo(0, mqMessage, 48, Math.Min(msg6007Response.Length, mqMessage.Length - 48));

            if (profile.Title != null)
            {
                var title = (profile.Title + new string(' ', 10)).Substring(0, 10);
                title.CopyTo(0, mqMessage, 48, 10);
            }
            if (profile.FirstName != null)
            {
                var firstName = (profile.FirstName + new string(' ', 14)).Substring(0, 14);
                firstName.CopyTo(0, mqMessage, 59, 14);
            }

            if (profile.LastName != null)
            {
                var lastName = (profile.LastName + new string(' ', 20)).Substring(0, 20);
                lastName.CopyTo(0, mqMessage, 74, 20);
            }

            if (profile.AddressLine1 != null)
            {
                var address1 = (profile.AddressLine1 + new string(' ', 33)).Substring(0, 33);
                address1.CopyTo(0, mqMessage, 142, 33);
            }

            var spaces33 = new string(' ', 33);
            spaces33.CopyTo(0, mqMessage, 241, 33);

            if (profile.USAddress)
            {
                var address2 = ((profile.AddressLine2 ?? "") + new string(' ', 33)).Substring(0, 33);
                address2.CopyTo(0, mqMessage, 175, 33);

                var city = ((profile.City ?? "") + new string(' ', 33)).Substring(0, 33);
                city.CopyTo(0, mqMessage, 208, 33);

                var state = ((profile.State ?? "") + new string(' ', 2)).Substring(0, 2);
                state.CopyTo(0, mqMessage, 274, 2);

                var zip = ((profile.ZipCode1 ?? "") + new string(' ', 5)).Substring(0, 5) + "-" +
                          ((profile.ZipCode2 ?? "") + new string(' ', 4)).Substring(0, 4);
                zip.CopyTo(0, mqMessage, 276, 10);
            }
            else
            {
                var city = ((profile.City ?? "") + new string(' ', 33)).Substring(0, 33);
                city.CopyTo(0, mqMessage, 175, 33);

                string sCountryZip = "";
                sCountryZip = (profile.ZipCode1 ?? "").Substring(0, Math.Min((profile.ZipCode1 ?? "").Length, 15)).Trim() + ",";
                sCountryZip += (profile.Country ?? "").Substring(0, Math.Min((profile.Country ?? "").Length, 33 - sCountryZip.Length));

                var countryZip = (sCountryZip + new string(' ', 33)).Substring(0, 33);
                countryZip.CopyTo(0, mqMessage, 208, 33);

                GeneralConstants.C_NON_US_STATE.CopyTo(0, mqMessage, 274, 2);

                var spaces10 = new string(' ', 10);
                spaces10.CopyTo(0, mqMessage, 276, 10);
            }

            profile.Telephone = (profile.Telephone ?? "").Replace("-", "");
            if (profile.Telephone.Length <= 10)
            {
                var phone = (profile.Telephone + new string('0', 10)).Substring(0, 10);
                phone.CopyTo(0, mqMessage, 286, 10);
            }

            profile.WorkTelephone = (profile.WorkTelephone ?? "").Replace("-", "");
            if (profile.WorkTelephone.Length <= 10)
            {
                var workPhone = (profile.WorkTelephone + new string('0', 10)).Substring(0, 10);
                workPhone.CopyTo(0, mqMessage, 296, 10);
            }

            "**".CopyTo(0, mqMessage, 461, 2);

            if (profile.eStmtPreference != null)
            {
                switch (profile.eStmtPreference.ToUpper())
                {
                    case "Y":
                        "E".CopyTo(0, mqMessage, 483, 1);
                        break;
                    case "N":
                        "P".CopyTo(0, mqMessage, 483, 1);
                        break;
                    default:
                        var eStmt = (profile.eStmtPreference + " ").Substring(0, 1);
                        eStmt.CopyTo(0, mqMessage, 483, 1);
                        break;
                }
            }
            else
            {
                " ".CopyTo(0, mqMessage, 483, 1);
            }

            if (profile.eConfirmPreference != null)
            {
                switch (profile.eConfirmPreference.ToUpper())
                {
                    case "Y":
                        "E".CopyTo(0, mqMessage, 484, 1);
                        break;
                    case "N":
                        "P".CopyTo(0, mqMessage, 484, 1);
                        break;
                    default:
                        var eConf = (profile.eConfirmPreference + " ").Substring(0, 1);
                        eConf.CopyTo(0, mqMessage, 484, 1);
                        break;
                }
            }
            else
            {
                " ".CopyTo(0, mqMessage, 484, 1);
            }

            if (profile.ReqdNoticesPreference != null)
            {
                switch (profile.ReqdNoticesPreference.ToUpper())
                {
                    case "Y":
                        "E".CopyTo(0, mqMessage, 485, 1);
                        break;
                    case "N":
                        "P".CopyTo(0, mqMessage, 485, 1);
                        break;
                    default:
                        var reqNotice = (profile.ReqdNoticesPreference + " ").Substring(0, 1);
                        reqNotice.CopyTo(0, mqMessage, 485, 1);
                        break;
                }
            }
            else
            {
                " ".CopyTo(0, mqMessage, 485, 1);
            }

            return new string(mqMessage);
        }
        public static bool IsAccountLocked(string mqResponse)
        {
            return !string.IsNullOrEmpty(mqResponse) &&
                   mqResponse.Length >= 9 &&
                   mqResponse.StartsWith(C_ACCOUNT_LOCKED);
        }


        private static string[] SplitMQResponse(string strParticipantData, ref ErrorInfo errorInfo, ref string errorCode)
        {
            if (errorInfo is null)
            {
                errorInfo = new ErrorInfo();
            }

            strParticipantData = CleanMe(strParticipantData);
            string[] strDataArray;
            string fixedData;
            if (strParticipantData.Length < 475)
            {
                errorInfo.Number = (int)ErrorCodes.IncompleteResponse;
                errorInfo.Description = ErrorMessages.IncompleteResponse;
                errorCode = "RES_ERROR";
                return new string[] { };
            }

            fixedData = strParticipantData.Substring(0, 474);
            strParticipantData = strParticipantData.Insert(474, ":");
            strParticipantData = strParticipantData.Substring(474);
            strDataArray = strParticipantData.Split(':');
            strDataArray[0] = fixedData;

            if (strDataArray.Length < 8)
            {
                errorInfo.Number = (int)ErrorCodes.IncompleteResponse;
                errorInfo.Description = ErrorMessages.IncompleteResponse;
                errorCode = "RES_ERROR";
                return new string[] { };
            }
            return strDataArray;
        }
    }

    internal class MessageReader : System.IO.MemoryStream
    {        
        private bool _EOF;
        public MessageReader(string response) : base(System.Text.Encoding.ASCII.GetBytes(response))
        {
            base.Position = 0L;
            if (base.Length == 0L)
            {
                _EOF = true;
            }
        }
        public bool EOF()
        {
            return _EOF;
        }
        public string ReadText(string len)
        {
            int bytesRead;
            var buf = new byte[Convert.ToInt32(len)];
            bytesRead = base.Read(buf, 0, Convert.ToInt32(len));
            if (base.Position == base.Length)
            {
                _EOF = true;
            }
            return System.Text.Encoding.ASCII.GetString(buf);
        }
        public string ReadString(int len)
        {
            string str1;
            str1 = ReadText(len.ToString()).Trim();
            if (string.IsNullOrEmpty(str1))
            {
                str1 = null;
            }
            return str1;
        }
    }
}
