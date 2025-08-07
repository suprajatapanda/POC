using System.Text;
using TRS.IT.BendProcessor.Model;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
namespace TRS.IT.BendProcessor.Util
{
    public class General
    {
        //marked private
        public static void CopyResultError(TaskStatus a_oTaskStatus, ResultReturn a_oResultReturn)
        {
            foreach (ErrorInfo er in a_oResultReturn.Errors)
            {
                a_oTaskStatus.errors.Add(new ErrorInfo(er.errorNum, er.errorDesc, er.severity));
                switch (er.severity)
                {
                    case ErrorSeverityEnum.Warning:
                        a_oTaskStatus.warningCnt += 1;
                        break;
                    case ErrorSeverityEnum.Error:
                    case ErrorSeverityEnum.Failed:
                    case ErrorSeverityEnum.ExceptionRaised:
                        a_oTaskStatus.fatalErrCnt += 1;
                        break;
                }
            }
        }
        public static void CopyResultError(ResultReturn a_oResultReturn, ResultReturn a_oFrom)
        {
            foreach (ErrorInfo er in a_oFrom.Errors)
            {
                a_oResultReturn.Errors.Add(new ErrorInfo(er.errorNum, er.errorDesc, er.severity));
            }
        }
        public static string ParseTaskInfo(TaskStatus a_oTaskStatus)
        {
            StringBuilder strB = new();
            strB.Append(ConstN.C_TAG_TABLE_O);
            WriteTaskRow(strB, "Task Name: ", a_oTaskStatus.taskName);
            WriteTaskRow(strB, "Task Status: ", a_oTaskStatus.retStatus.ToString());
            WriteTaskRow(strB, "Row Count: ", a_oTaskStatus.rowsCount.ToString());
            WriteTaskRow(strB, "Start Time: ", a_oTaskStatus.startTime.ToString());
            WriteTaskRow(strB, "End Time: ", a_oTaskStatus.endTime.ToString());

            //if (a_oTaskStatus.retStatus != TaskRetStatus.Succeeded)
            //{
            WriteTaskRow(strB, "Fatal Error(s): ", a_oTaskStatus.fatalErrCnt.ToString());
            WriteTaskRow(strB, "Warning(s): ", a_oTaskStatus.warningCnt.ToString());
            WriteTaskRow(strB, "Error(s): ", ParseErrorText(a_oTaskStatus.errors, ConstN.C_TAG_BR));
            //}
            strB.Append(ConstN.C_TAG_TR_O);
            strB.Append(ConstN.C_TAG_TD_O + ConstN.C_TAG_TD_C);
            strB.Append(ConstN.C_TAG_TR_C);

            strB.Append(ConstN.C_TAG_TABLE_C);
            strB.Append(ConstN.C_TAG_BR);

            return strB.ToString();
        }
        public static string ParseErrorText(List<ErrorInfo> a_oErrors, string a_sDelimiter)
        {
            StringBuilder strB = new();
            foreach (ErrorInfo er in a_oErrors)
            {
                strB.Append(er.errorDesc + "(" + er.errorNum.ToString() + "-" + er.severity.ToString() + ")" + a_sDelimiter);
            }
            return strB.ToString();
        }
        private static void WriteTaskRow(StringBuilder a_strB, string a_sColText, string a_sColVal)
        {
            a_strB.Append(ConstN.C_TAG_TR_O + ConstN.C_TAG_TD_O + a_sColText + ConstN.C_TAG_TD_C);
            a_strB.Append(ConstN.C_TAG_TD_O + a_sColVal + ConstN.C_TAG_TD_C + ConstN.C_TAG_TR_C);
        }
    }
}
