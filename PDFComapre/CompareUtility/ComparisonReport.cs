using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareUtility
{
    public class ComparisonReport
    {
        public string DifferencesFolderPath;
        public string ComparisonMessage;
        public string FilePath1;
        public string FilePath2;
        public int NumberOfPagesFile1;
        public int NumberOfPagesFile2;
        public string Result;
        public bool Indicator;
        public List<Page> Pages;
        public string PolicyNumber;
        public bool ImageCompare;
        public bool? IsPass;
        public string CompareText;
        public string Message;

        public ComparisonReport()
        {
            Pages = new List<CompareUtility.Page>();
        }

        public void AddPageReport(int pageNumber, int differences, string result, string comparisonMessage, string combinedDiff)
        {
            Pages.Add(new Page(pageNumber, differences, result, comparisonMessage, combinedDiff));
        }
    }
    public class Page
    {
        public string ComparisonMessage;
        public int Differences;
        public string Result;
        public string CombinedDiff;
        public Page()
        {

        }
        public Page(int pageNumber, int differences, string result, string comparisonMessage, string combinedDiff)
        {
            ComparisonMessage = comparisonMessage;
            Differences = differences;
            Result = result;
            CombinedDiff = combinedDiff;

        }
    }
}
