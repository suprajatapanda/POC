
namespace TRS.IT.SI.BusinessFacadeLayer.Adapters
{
    internal class TAEUtil
    {
        public static Model.PayrollFrequency ConvertPayrollFrequency(string sPayrollFrequency)
        {
            switch (sPayrollFrequency ?? "")
            {
                case "A":
                    {
                        return Model.PayrollFrequency.Annually;
                    }
                case "B":
                    {
                        return Model.PayrollFrequency.BiWeekly;
                    }
                case "M":
                    {
                        return Model.PayrollFrequency.Monthly;
                    }
                case "Q":
                    {
                        return Model.PayrollFrequency.Quarterly;
                    }
                case "S":
                    {
                        return Model.PayrollFrequency.SemiAnnually;
                    }
                case "H":
                    {
                        return Model.PayrollFrequency.SemiMonthly;
                    }
                case "W":
                    {
                        return Model.PayrollFrequency.Weekly;
                    }
                case "N":
                    {
                        return Model.PayrollFrequency.None;
                    }

                default:
                    {
                        return Model.PayrollFrequency.Unknown;
                    }
            }
        }
    }
}