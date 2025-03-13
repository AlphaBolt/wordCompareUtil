using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompareUtility;
using PDFCompare;
using System.Runtime.InteropServices;

namespace LibraryTest
{
    [ComVisible(true)]
    public class Class1
    {
        public static string fnPDFDiff_FormTemplate(string SourceFilePath, string TargetFilePath, string ignorecoordinatesfilepath, string ComparisonReportFile, string pagesNumber, bool boolImageCompare = false)
        {
            string message = string.Empty;

            List<int> list = Common.PagesToCompare(pagesNumber);

            //bool res = Licencing.IsTokenValid("ZGRJT0FWOEFxbFhnRnFsVjB1SEQ3K0s3SDI0NXdVMlJySzBhZjdGbjlEYz06TmlpdF91c2VyOjEwMDAwMDAwMDAwMA==", "1.1.20.17");

            List<ComparisonReport> Report = new List<ComparisonReport>();

            try
            {
                if (boolImageCompare)
                {
                    var result = Common.ComaprePDF(SourceFilePath, TargetFilePath, ignorecoordinatesfilepath, ComparisonReportFile, list,new List<int>());
                    message = result.Result;
                    Report.Add(result);
                }
                else
                {
                    var result = Common.ComaprePDFText(SourceFilePath, TargetFilePath, list, new List<int>(), ComparisonReportFile);
                    message = result.Result;
                    Report.Add(result);
                }

                if (Report.Count > 0)
                {
                    //Console.WriteLine("Creating Comparison Report Started------------------------------");
                    #region Step 3 Creating Comparison Report
                    Common.CreateReport(Report, ComparisonReportFile,"targetFileName");
                    //Common.CreateTextFile(Report);
                    #endregion
                    // Console.WriteLine("Creating Comparison Report Completed------------------------------");
                }

                return message;
            }
            catch (Exception ex)
            {
                return message = "Failure";
            }
        }

        public string getMessage()
        {
            return "newly built";
        }
    }
}
