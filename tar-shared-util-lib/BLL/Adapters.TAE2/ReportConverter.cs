using TRS.IT.SI.BusinessFacadeLayer.Model;

namespace TRS.IT.SI.BusinessFacadeLayer.Adapters
{

    internal class ReportConverter
    {
        private const string C_ReportMessage = "6049";
        private const string C_ValidReportResponse = "000EC60490";
        private const string C_ReportSuffix = "097055";

        public static string FormatReportRequest(ReportInfo oReportInfo, ref ReportResponse oResponse)
        {

            var sbRequest = new System.Text.StringBuilder();
            string ext;

            if (oResponse is null)
            {
                oResponse = new ReportResponse();
            }

            ext = "PDF";

            oResponse.FileName = GetReportFileName(oReportInfo);

            if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.AccountStatement || oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.AccountStatement_SuppressVesting || oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.ParticipantStatement)
            {
                oReportInfo.PartnerUserID = "M" + oReportInfo.MemberID.Substring(0, Math.Min(6, oReportInfo.MemberID.Length));
            }

            // build message
            sbRequest.Append(C_ReportMessage);
            sbRequest.Append(DefaultSettings.BANKCODE(oReportInfo.SubID, oReportInfo.LocationCode));
            sbRequest.Append(DefaultSettings.CID());
            sbRequest.Append(oReportInfo.PlanID);
            sbRequest.Append((oReportInfo.MemberID + new string(' ', 10)).Substring(0, 10));
            sbRequest.Append(DefaultSettings.FPID(oReportInfo.PlanID));
            sbRequest.Append(oReportInfo.PartnerUserID.PadRight(8));
            sbRequest.Append("/");
            sbRequest.Append(C_ReportSuffix);
            sbRequest.Append(oResponse.FileName.Substring(0, Math.Min(10, oResponse.FileName.Length)));  // first 10 bytes of report file name

            switch (oReportInfo.ReportType)
            {
                case (int)ReportInfo.ReportTypeEnum.ParticipantStatement:
                case (int)ReportInfo.ReportTypeEnum.AccountStatement:
                case (int)ReportInfo.ReportTypeEnum.AccountStatement_SuppressVesting:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append("000000000PRT13");
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.ContributionLimit:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DAL.General.FormatNumberFlat(oReportInfo.DollarAmount, 6, 2));

                        if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                        {
                            ext = "CSV";
                            // add 3 to sortby
                            sbRequest.Append(((int)oReportInfo.SortBy + 3).ToString());
                        }
                        else
                        {
                            sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                        }

                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.ContributionDetails:
                case (int)ReportInfo.ReportTypeEnum.ContributionRateChange:
                case (int)ReportInfo.ReportTypeEnum.ContributionRateChange_2:
                case (int)ReportInfo.ReportTypeEnum.ContributionRateChangeText:
                case (int)ReportInfo.ReportTypeEnum.LoansIssued:
                case (int)ReportInfo.ReportTypeEnum.LoansPaymentHistory:
                case (int)ReportInfo.ReportTypeEnum.LoansPaidOff:
                    {

                        if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.ContributionRateChangeText)
                        {
                            ext = "TXT";
                            if (oReportInfo.FullFile == false)
                            {
                                sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                                sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                            }
                            else
                            {
                                sbRequest.Append("00/00/0000");
                                sbRequest.Append("00/00/0000");
                            }
                            sbRequest.Append(oReportInfo.FullFile ? "Y" : "N");
                        }
                        else
                        {
                            sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                            sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));

                            if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.ContributionRateChange || oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.ContributionRateChange_2)
                            {
                                ext = "CSV";
                                sbRequest.Append(oReportInfo.ParticipantType.ToString());
                                sbRequest.Append(((int)oReportInfo.Company).ToString());
                                if (!string.IsNullOrEmpty(oReportInfo.FTPLocation))
                                {
                                    sbRequest.Append(oReportInfo.FTPLocation);
                                }
                            }
                            // loans issued is handled seperately later
                            else if (!(oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.LoansIssued) && !(oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.ContributionDetails))
                            {
                                sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                            }

                            if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.LoansPaidOff)
                            {
                                if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                                {
                                    ext = "CSV";
                                    sbRequest.Append("0");
                                }
                                else
                                {
                                    sbRequest.Append("1");
                                }
                            }

                            if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.LoansIssued || oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.ContributionDetails)
                            {
                                if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                                {
                                    ext = "CSV";
                                    sbRequest.Append(((int)oReportInfo.SortBy + 2).ToString());
                                }
                                else
                                {
                                    sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                                }
                            }
                        }

                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.ContributionByMoneyType:
                case (int)ReportInfo.ReportTypeEnum.ContributionSummaryByFund:
                case (int)ReportInfo.ReportTypeEnum.PlanLevelInvestmentSummary:
                case (int)ReportInfo.ReportTypeEnum.InvestmentSummaryTPA:
                case (int)ReportInfo.ReportTypeEnum.PlanAdminstration:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.ContributionByMoneyType)
                        {
                            if (oReportInfo.PlanReportOption == ReportInfo.PlanReportOptionEnum.ByTradeDate)
                            {
                                if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                                {
                                    ext = "CSV";
                                    sbRequest.Append("3");   // By Trade Date
                                }
                                else
                                {
                                    sbRequest.Append("1");
                                }   // By Trade Date
                            }
                            else if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                            {
                                ext = "CSV";
                                sbRequest.Append("2");   // By Effective date 
                            }
                            else
                            {
                                sbRequest.Append("0");

                            }   // By Effective date 
                        }

                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.DemographicEligibility:
                case (int)ReportInfo.ReportTypeEnum.ParticipantEligibility:
                case (int)ReportInfo.ReportTypeEnum.ParticipantEligibilityCsv:
                    {
                        if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.ParticipantEligibilityCsv)
                        {
                            ext = "CSV";
                        }
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.ContributionByMoneyType)
                        {
                            if (oReportInfo.PlanReportOption == ReportInfo.PlanReportOptionEnum.ByTradeDate)
                            {
                                sbRequest.Append("1");   // By Trade Date
                            }
                            else
                            {
                                sbRequest.Append("0");
                            }   // By Effective date 
                        }

                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.DemographicByVestedPercent:
                case (int)ReportInfo.ReportTypeEnum.DemographicActiveInactive:
                case (int)ReportInfo.ReportTypeEnum.DemographicInactiveParticipant:
                case (int)ReportInfo.ReportTypeEnum.DemographicIncompleteDataForActiveParticipants:
                case (int)ReportInfo.ReportTypeEnum.LoansBalance:
                    {

                        sbRequest.Append("00/00/0000");
                        sbRequest.Append("00/00/0000");
                        // rlou - 04/05/2010 - BSS-BUG788 - The Vested Percentage report displays a list
                        if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.LoansBalance)
                        {
                            if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                            {
                                ext = "CSV";
                                sbRequest.Append("0");
                            }
                            else
                            {
                                sbRequest.Append("1");
                            }
                        }
                        else if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.DemographicByVestedPercent)
                        {
                            if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                            {
                                ext = "CSV";
                                if (oReportInfo.SortBy == ReportInfo.SortOptionEnum.SSN)
                                {
                                    sbRequest.Append("2");
                                }
                                else if (oReportInfo.SortBy == ReportInfo.SortOptionEnum.LastName)
                                {
                                    sbRequest.Append("3");
                                }
                            }
                            else
                            {
                                // Report display type = PDF
                                sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                            }
                        }
                        else
                        {
                            // Report display type = PDF
                            // Report type = DemographicActiveInactive, DemographicInactiveParticipant, DemographicIncompleteDataForActiveParticipants
                            sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                        }

                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.DistributionEmployeeDisbursement:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                        {
                            short iSortBy = 0;
                            iSortBy = (short)(Convert.ToInt16(((int)oReportInfo.SortBy).ToString()) + 2);
                            sbRequest.Append(iSortBy.ToString());
                            ext = "CSV";
                        }
                        else
                        {
                            sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                        }

                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.DemographicDesignatedAge:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        if (oReportInfo.AgeLimit.ToString().IndexOf(".5") != -1)
                        {
                            sbRequest.Append("00/00/9999");
                            sbRequest.Append(Convert.ToInt32(oReportInfo.AgeLimit.ToString().Substring(0, oReportInfo.AgeLimit.ToString().IndexOf("."))).ToString("00"));
                        }
                        else
                        {
                            sbRequest.Append("00/00/0000");
                            sbRequest.Append(((int)Math.Round(oReportInfo.AgeLimit)).ToString("00"));
                        }

                        if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                        {
                            ext = "CSV";
                            if (oReportInfo.SortBy == ReportInfo.SortOptionEnum.Age)
                            {
                                sbRequest.Append("5");
                            }
                            else if (oReportInfo.SortBy == ReportInfo.SortOptionEnum.SSN)
                            {
                                sbRequest.Append("3");
                            }
                            else if (oReportInfo.SortBy == ReportInfo.SortOptionEnum.LastName)
                            {
                                sbRequest.Append("4");
                            }
                        }
                        else if (oReportInfo.SortBy == ReportInfo.SortOptionEnum.Age)
                        {
                            sbRequest.Append("2");
                        }
                        else
                        {
                            sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                        }

                        break;
                    }

                // 'sort option
                // If .SortBy = ReportInfo.SortOptionEnum.Age Then
                // sbRequest.Append("2")
                // Else
                // sbRequest.Append(CStr(.SortBy))
                // End If

                // ext = "CSV"
                case (int)ReportInfo.ReportTypeEnum.DemographicEmployeeAddress:
                    {
                        sbRequest.Append("00/00/0000");
                        sbRequest.Append("00/00/0000");
                        // sbRequest.Append(CStr(.SortBy))
                        sbRequest.Append(oReportInfo.ParticipantType.ToString());

                        if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                        {
                            ext = "CSV";
                            // add 2 to sortby
                            sbRequest.Append(((int)oReportInfo.SortBy + 2).ToString());
                        }
                        else
                        {
                            sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                        }

                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.DistributionDeminimusBalance:
                    {

                        sbRequest.Append("00/00/0000");
                        sbRequest.Append("00/00/0000");

                        // sort order
                        if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                        {
                            ext = "CSV";
                            // add 2 to sortby
                            sbRequest.Append(((int)oReportInfo.SortBy + 2).ToString());
                        }
                        else
                        {
                            sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                        }
                        // If .ParticipantType = ReportInfo.ParticipantTypeEnum.TerminatedParticipants Then
                        // sbRequest.Append("1")
                        // Else
                        // sbRequest.Append("0")   'all participants
                        // End If

                        sbRequest.Append(DAL.General.FormatNumberFlat(oReportInfo.DollarAmount, 6, 2));
                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.PlanDataCsvFile:
                case (int)ReportInfo.ReportTypeEnum.PlanDataXlsFile:
                    {
                        ext = "CSV";
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        if (oReportInfo.PlanReportOption == ReportInfo.PlanReportOptionEnum.ByPayrollEndingDate)
                        {
                            sbRequest.Append("2");
                        }
                        else
                        {
                            sbRequest.Append("1");
                        }
                        if (oReportInfo.FullFile == true)
                        {
                            sbRequest.Append("1");
                        }
                        else
                        {
                            sbRequest.Append("0");
                        }

                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.CensusFile:
                    {
                        ext = "CSV";
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        if (oReportInfo.PlanReportOption == ReportInfo.PlanReportOptionEnum.ByProcessEndDate)
                        {
                            sbRequest.Append("0");
                        }
                        else
                        {
                            sbRequest.Append("1");
                        } // -- for ReportInfo.PlanReportOptionEnum.ByCurrentPlanYear

                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.DiscriminationDataDownload:
                    {
                        ext = "CSV";
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        if (oReportInfo.PlanReportOption == ReportInfo.PlanReportOptionEnum.ByProcessEndDate)
                        {
                            sbRequest.Append("0");
                        }
                        else
                        {
                            sbRequest.Append("1");
                        } // -- for ReportInfo.PlanReportOptionEnum.ByCurrentPlanYear

                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.ParticipantCensusData:
                    {
                        ext = "CSV";
                        sbRequest.Append("00/00/0000");
                        sbRequest.Append("00/00/0000");
                        sbRequest.Append("8Z");
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.AccountBalanceAsOf:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        // set sort oprtion
                        if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                        {
                            ext = "CSV";
                            int iSortBy = Convert.ToInt32(GetAccountBalanceSortOption(oReportInfo.SortBy)) + 5;
                            sbRequest.Append(iSortBy.ToString());
                        }
                        else
                        {
                            sbRequest.Append(GetAccountBalanceSortOption(oReportInfo.SortBy));
                        }

                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.PlanLevelMultiLocationParticipants:
                    {
                        sbRequest.Append("00/00/0000");
                        sbRequest.Append("00/00/0000");
                        if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                        {
                            ext = "CSV";
                            sbRequest.Append("1");
                        }
                        else
                        {
                            sbRequest.Append("0");
                        }

                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.DemographicParticpantDisplay:
                    {
                        sbRequest.Append(oReportInfo.MemberID.PadRight(10));
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.DemographicParticipantBalanceByFund:
                    {
                        ext = "CSV";
                        break;
                    }
                // #1698 - Start
                case (int)ReportInfo.ReportTypeEnum.ContributionDetailsCSV:
                    {
                        ext = "CSV";
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.ParticipantInvestmentElections:
                    {
                        ext = "CSV";
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.ParticipantIndicativeData:
                    {
                        ext = "CSV";
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.LoanDetail:
                    {
                        ext = "CSV";
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.BasicPlanInformation:
                    {
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.ConversionStatement:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        break;
                    }
                // #1698 - End
                case (int)ReportInfo.ReportTypeEnum.PayrollTemplate:
                    {
                        ext = "CSV";
                        sbRequest.Append("00/00/000000/00/0000");
                        // Indicator to indicate if iSeries contract or not
                        if (!string.IsNullOrEmpty(oReportInfo.FTPLocation))
                        {
                            sbRequest.Append(oReportInfo.FTPLocation);
                        }

                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.ContributionRate:
                    {
                        // No parameters
                        ext = "CSV";
                        sbRequest.Append("00/00/000000/00/0000");
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.MinRequiredDistribution:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append("00/00/0000");
                        // sbRequest.Append(CStr(.SortBy))
                        if (oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.CSV)
                        {
                            ext = "CSV";
                            sbRequest.Append(((int)oReportInfo.SortBy + 3).ToString());
                        }
                        else
                        {
                            sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                        }

                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.ParticipantLoanBalance:
                case (int)ReportInfo.ReportTypeEnum.ParticipantLoanIssued:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.IndicativeDataDownload_NoVesting:
                case (int)ReportInfo.ReportTypeEnum.IndicativeDataDownload_Vesting:
                    {
                        // No parameters
                        ext = "CSV";
                        sbRequest.Append("00/00/000000/00/0000");
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.HardshipSuspension:
                    {
                        ext = "CSV";
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.PASSAnnualNotice:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append("00/00/0000");
                        sbRequest.Append("1");
                        ext = "CSV";
                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.PASSSummaryAnnualReport:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append("00/00/0000");
                        sbRequest.Append("0");
                        ext = "CSV";
                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.PASSSummaryPlanDescription:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append("00/00/0000");
                        sbRequest.Append("0");
                        ext = "CSV";
                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.PASSSummaryOfMaterialModifications:
                    {
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append("00/00/0000");
                        sbRequest.Append("0");
                        ext = "CSV";
                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.PASSForceOutDistribution:
                    {
                        ext = "CSV";
                        sbRequest.Append("99/99/9999");
                        sbRequest.Append("99/99/9999");
                        sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                        sbRequest.Append(DAL.General.FormatNumberFlat(oReportInfo.DollarAmount, 6, 2));
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.PASSForceOutTermination:
                    {
                        ext = "CSV";
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(((int)oReportInfo.SortBy).ToString());
                        sbRequest.Append("99999999");
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.PASSEnrollment:
                    {
                        ext = "CSV";
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.RequestATest:
                    {
                        sbRequest.Append("01/01/" + DateTime.Now.Year.ToString());
                        sbRequest.Append("12/31/" + DateTime.Now.Year.ToString());
                        ext = "PDF";
                        break;
                    }

                case (int)ReportInfo.ReportTypeEnum.BeneficiaryDetails:
                    {
                        ext = "CSV";
                        sbRequest.Append("00/00/000000/00/00000"); // 1 is sort by name
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.ChangestoSafeHarborNonElectiveRate:
                    {
                        ext = "CSV";
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.ForfeitureReport:
                    {
                        ext = "PDF";
                        sbRequest.Append(DateTime.Parse(oReportInfo.StartDate).ToString("MM/dd/yyyy"));
                        sbRequest.Append(DateTime.Parse(oReportInfo.EndDate).ToString("MM/dd/yyyy"));
                        break;
                    }
                case (int)ReportInfo.ReportTypeEnum.AUTOEnrollment:
                    {
                        sbRequest.Append("00/00/0000");
                        sbRequest.Append("00/00/0000");
                        ext = "CSV";
                        break;
                    }

            }
            if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.ParticipantStatement || oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.AccountStatement || oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.AccountStatement_SuppressVesting)
            {
                oResponse.FileName = TrsAppSettings.AppSettings.GetValue("ReportPrefix") + "P" + oReportInfo.PlanID + "." + oReportInfo.PartnerUserID + ".R" + oResponse.FileName.Substring(0, 2) + "." + oResponse.FileName.Substring(2, 8) + "." + ext;
            }
            else
            {
                oResponse.FileName = TrsAppSettings.AppSettings.GetValue("ReportPrefix") + "P" + oReportInfo.PlanID + "." + oReportInfo.PartnerUserID + ".R" + oResponse.FileName.Substring(0, 2) + "." + oResponse.FileName.Substring(2, 7) + "." + ext;
            }
            oResponse.Request = sbRequest.ToString();
            return sbRequest.ToString();

        }

        private static string GetAccountBalanceSortOption(ReportInfo.SortOptionEnum sort)
        {
            switch (sort)
            {
                case ReportInfo.SortOptionEnum.NameWithinLocation:
                    {
                        return "1";
                    }
                case ReportInfo.SortOptionEnum.BalanceWithLocation:
                    {
                        return "2";
                    }
                case ReportInfo.SortOptionEnum.LastName:
                    {
                        return "3";
                    }
                case ReportInfo.SortOptionEnum.DollarAmount:
                    {
                        return "4";
                    }

                default:
                    {
                        throw new Exception("Invaild Sort Option Selected.");
                    }
            }
        }

        private static string GetReportFileName(ReportInfo oReportInfo)
        {
            string[] ConvertValue = ["", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"];
            string str;
            var dt = DateTime.Now;
            string whichReport = GetReportNumber((ReportInfo.ReportTypeEnum)oReportInfo.ReportType);
            int currentHour = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(dt) + 1;
            int currentMinute = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(dt);
            int currentMonth = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMonth(dt) + 1;
            int currentDay = dt.Day;
            if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.AccountStatement || oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.AccountStatement_SuppressVesting || oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.ParticipantStatement)
            {
                // special handling required for participant statements
                int R_hh = 10 + (23 - System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(dt));
                int R_mm = 10 + (59 - System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(dt));
                int R_s = (int)Math.Round(6d - Convert.ToDouble(System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetSecond(dt).ToString().Substring(0, 1))); // higher order digit of current second
                string HHMMS, HMMS, HMM;
                HHMMS = R_hh.ToString() + R_mm.ToString() + R_s.ToString();
                HMMS = HHMMS.Substring(HHMMS.Length - 4);
                HMM = HMMS.Substring(1, 3);
                if (oReportInfo.MemberID.Length == 10)
                {
                    str = whichReport + "M" + oReportInfo.MemberID.Trim().Substring(oReportInfo.MemberID.Trim().Length - 4) + HMM;
                }
                else
                {
                    str = whichReport + "M" + oReportInfo.MemberID.Trim().Substring(oReportInfo.MemberID.Trim().Length - 3) + HMMS;
                }
            }
            else
            {
                if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.PlanLevelForefeitureBalance || oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.ParticipantCensusData)
                {
                    str = whichReport.ToString() + "D";
                }
                else if (whichReport.Trim().Length == 1)
                {
                    str = "0" + whichReport + "T";
                }
                else
                {
                    str = whichReport + "T";
                }
                str = str + DAL.ReportsDC.GetNextReportNumber().ToString("000000").Substring(DAL.ReportsDC.GetNextReportNumber().ToString("000000").Length - 6) + "X";
            }
            return str;
        }

        private static string GetReportNumber(ReportInfo.ReportTypeEnum rptType)
        {
            switch (rptType)
            {
                case ReportInfo.ReportTypeEnum.ContributionByMoneyType:
                    {
                        return "5";
                    }
                case ReportInfo.ReportTypeEnum.ContributionDetails:
                    {
                        return "27";
                    }
                case ReportInfo.ReportTypeEnum.ContributionLimit:
                    {
                        return "4";
                    }
                case ReportInfo.ReportTypeEnum.ContributionRateChange:
                case ReportInfo.ReportTypeEnum.ContributionRateChange_2:
                    {
                        return "55";
                    }
                case ReportInfo.ReportTypeEnum.ContributionRateChangeText:
                    {
                        return "25";
                    }
                case ReportInfo.ReportTypeEnum.ContributionSummaryByFund:
                    {
                        return "6";
                    }
                case ReportInfo.ReportTypeEnum.DemographicActiveInactive:
                    {
                        return "32";
                    }
                case ReportInfo.ReportTypeEnum.DemographicByVestedPercent:
                    {
                        return "20";
                    }
                case ReportInfo.ReportTypeEnum.DemographicDesignatedAge:
                    {
                        return "2";
                    }
                case ReportInfo.ReportTypeEnum.DemographicEligibility:
                    {
                        return "21";
                    }
                case ReportInfo.ReportTypeEnum.DemographicEmployeeAddress:
                    {
                        return "3";
                    }
                case ReportInfo.ReportTypeEnum.DemographicInactiveParticipant:
                    {
                        return "12";
                    }
                case ReportInfo.ReportTypeEnum.DemographicIncompleteDataForActiveParticipants:
                    {
                        return "11";
                    }
                case ReportInfo.ReportTypeEnum.DemographicParticpantDisplay:
                    {
                        return "19";
                    }
                case ReportInfo.ReportTypeEnum.DistributionDeminimusBalance:
                    {
                        return "7";
                    }
                case ReportInfo.ReportTypeEnum.DistributionEmployeeDisbursement:
                    {
                        return "9";
                    }
                case ReportInfo.ReportTypeEnum.LoansBalance:
                    {
                        return "13";
                    }
                // Case ReportInfo.ReportTypeEnum.LoansIssued
                // Return "15"
                case ReportInfo.ReportTypeEnum.LoansPaidOff:
                    {
                        return "16";
                    }
                case ReportInfo.ReportTypeEnum.LoansPaymentHistory:
                    {
                        return "14";
                    }
                case ReportInfo.ReportTypeEnum.PlanLevelForefeitureBalance:
                    {
                        return "10";
                    }
                case ReportInfo.ReportTypeEnum.PlanLevelInvestmentSummary:
                case ReportInfo.ReportTypeEnum.InvestmentSummaryTPA:
                    {
                        return "29";
                    }
                case ReportInfo.ReportTypeEnum.PlanLevelHeadCountByFund:
                    {
                        return "34";
                    }
                case ReportInfo.ReportTypeEnum.ParticipantStatement:
                case ReportInfo.ReportTypeEnum.AccountStatement:
                    {
                        return "65";
                    }
                case ReportInfo.ReportTypeEnum.AccountStatement_SuppressVesting:
                    {
                        return "65"; // 'Return "63" ' BSS Bug - 2358
                    }
                case ReportInfo.ReportTypeEnum.PlanDataCsvFile:
                case ReportInfo.ReportTypeEnum.PlanDataXlsFile:
                    {
                        return "33";
                    }
                case ReportInfo.ReportTypeEnum.CensusFile:
                    {
                        return "36";
                    }
                case ReportInfo.ReportTypeEnum.ParticipantCensusData:
                    {
                        return "23";
                    }
                case ReportInfo.ReportTypeEnum.AccountBalanceAsOf:
                    {
                        return "37";
                    }
                case ReportInfo.ReportTypeEnum.PlanLevelMultiLocationParticipants:
                    {
                        return "26";
                    }
                case ReportInfo.ReportTypeEnum.DemographicParticipantBalanceByFund:
                    {
                        return "45";
                    }

                // #1698 - Start
                case ReportInfo.ReportTypeEnum.ContributionDetailsCSV:
                    {
                        return "40";
                    }
                case ReportInfo.ReportTypeEnum.ParticipantInvestmentElections:
                    {
                        return "42";
                    }
                case ReportInfo.ReportTypeEnum.ParticipantIndicativeData:
                    {
                        return "39";
                    }
                case ReportInfo.ReportTypeEnum.LoanDetail:
                    {
                        return "41";
                    }
                case ReportInfo.ReportTypeEnum.BasicPlanInformation:
                    {
                        return "43";
                    }
                case ReportInfo.ReportTypeEnum.ConversionStatement:
                    {
                        return "44";
                    }
                // #1698 - End
                case ReportInfo.ReportTypeEnum.PayrollTemplate:
                    {
                        // Return "52"
                        return "B1";
                    }
                case ReportInfo.ReportTypeEnum.ContributionRate:
                    {
                        return "A1";
                    }
                case ReportInfo.ReportTypeEnum.MinRequiredDistribution:
                    {
                        return "A2";
                    }
                case ReportInfo.ReportTypeEnum.ParticipantLoanBalance:
                    {
                        return "A3";
                    }
                case ReportInfo.ReportTypeEnum.ParticipantLoanIssued:
                case ReportInfo.ReportTypeEnum.LoansIssued:
                    {
                        return "A4";
                    }
                case ReportInfo.ReportTypeEnum.ParticipantEligibility:
                    {
                        return "A5";
                    }
                case ReportInfo.ReportTypeEnum.ParticipantEligibilityCsv: // for PASS
                    {
                        return "AB";
                    }
                case ReportInfo.ReportTypeEnum.IndicativeDataDownload_NoVesting:
                case ReportInfo.ReportTypeEnum.IndicativeDataDownload_Vesting:
                    {
                        return "A6";
                    }
                case ReportInfo.ReportTypeEnum.DiscriminationDataDownload:
                    {
                        return "A7";
                    }
                case ReportInfo.ReportTypeEnum.PlanAdminstration:
                    {
                        return "A9";
                    }
                case ReportInfo.ReportTypeEnum.HardshipSuspension:
                    {
                        return "AG";
                    }

                case ReportInfo.ReportTypeEnum.PASSAnnualNotice:
                    {
                        return "AI";
                    }
                case ReportInfo.ReportTypeEnum.PASSEnrollment:
                    {
                        return "AJ";
                    }
                case ReportInfo.ReportTypeEnum.PASSSummaryAnnualReport:
                    {
                        return "AK";
                    }
                case ReportInfo.ReportTypeEnum.PASSSummaryPlanDescription:
                    {
                        return "AL";
                    }
                case ReportInfo.ReportTypeEnum.PASSSummaryOfMaterialModifications:
                    {
                        return "AM";
                    }
                case ReportInfo.ReportTypeEnum.PASSForceOutDistribution:
                    {
                        return "AH";
                    }
                case ReportInfo.ReportTypeEnum.PASSForceOutTermination:
                    {
                        return "AN";
                    }
                case ReportInfo.ReportTypeEnum.RequestATest:
                    {
                        return "AO";
                    }
                case ReportInfo.ReportTypeEnum.BeneficiaryDetails:
                    {
                        return "AR";
                    }
                case ReportInfo.ReportTypeEnum.ChangestoSafeHarborNonElectiveRate:
                    {
                        return "AU";
                    }
                case ReportInfo.ReportTypeEnum.ForfeitureReport:
                    {
                        return "B2";
                    }
                case ReportInfo.ReportTypeEnum.AUTOEnrollment:
                    {
                        return "AT";
                    }

                default:
                    {
                        throw new Exception("Invalid Report Request.");
                    }
            }
        }

        public static SIResponse IsValidReportResponse(string mqResponse)
        {
            var oResponse = new SIResponse();
            if ((mqResponse ?? "") != C_ValidReportResponse)
            {
                oResponse.Errors[0].Number = (int)ErrorCodes.StatementError;
                oResponse.Errors[0].Description = ErrorMessages.StatementError;
            }
            return oResponse;
        }

    }
}