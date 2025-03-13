using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using ImageDiff;
using Xceed.Words.NET;

namespace CompareUtility
{
    public class WordComparer
    {
        private string ReportDirectoryPath = string.Empty;
        private string ReportDirectoryName = "Reports";
        private string DifferencesDirectoryName = "Differences";
        private string TemporaryDirectoryName = "Temp";
        private int Difference = 0;
        private Dictionary<int, List<string>> imgCoordinate = null;
        private ComparisonReport report;

        public WordComparer()
        {
            report = new ComparisonReport();
        }


        public ComparisonReport CompareTwoWordDocs(string SourceDoc, string TargetDoc, List<int> sourcePageRange, List<int> targetPageRange, string reportResult = "")
        {
            string FirstFile = string.Empty, SecondFile = string.Empty;
            string textSource = string.Empty;
            string textTarget = string.Empty;

            string differencesFolderPath = string.Empty,
                   tempFolder1 = string.Empty,
                   tempFolder2 = string.Empty,
                   TempFolderPath = string.Empty,
                   reportFolderpath = string.Empty;

            ComparisonReport report = new ComparisonReport(); // Assuming this is your report class
            report.ImageCompare = false;
            ReportDirectoryPath = Directory.GetParent(reportResult).FullName;

            if (File.Exists(SourceDoc) && File.Exists(TargetDoc))
            {
                // Using Xceed.Words.NET (DocX)
                using (DocX sourceDoc = DocX.Load(SourceDoc))
                {
                    var paragraphs = sourceDoc.Paragraphs;
                    // Extract text from source document
                    textSource = string.Join("\n", paragraphs.Select(p => p.Text));

                    // If specific "page" ranges are specified, treat them as paragraph indices
                    if (sourcePageRange != null && sourcePageRange.Count > 0)
                    {
                        textSource = "";
                        foreach (int paraIdx in sourcePageRange)
                        {
                            if (paraIdx >= 0 && paraIdx < paragraphs.Count)
                            {
                                textSource += paragraphs[paraIdx].Text + "\n";
                            }
                        }
                    }
                }

                using (DocX targetDoc = DocX.Load(TargetDoc))
                {
                    var paragraphs = targetDoc.Paragraphs;
                    // Extract text from target document
                    textTarget = string.Join("\n", paragraphs.Select(p => p.Text));

                    if (targetPageRange != null && targetPageRange.Count > 0)
                    {
                        textTarget = "";
                        foreach (int paraIdx in targetPageRange)
                        {
                            if (paraIdx >= 0 && paraIdx < paragraphs.Count)
                            {
                                textTarget += paragraphs[paraIdx].Text + "\n";
                            }
                        }
                    }
                }

                // For line-by-line comparison
                FirstFile = textSource;
                SecondFile = textTarget;
            }
            else
            {
                report.FilePath1 = SourceDoc;
                report.FilePath2 = TargetDoc;
                report.Indicator = true;
                report.Result = "Fail";
                report.ComparisonMessage = "Files do not exist.";
                return report;
            }

            List<string> File1diff;
            List<string> File2diff;

            IEnumerable<string> file1 = FirstFile.Trim().Split('\r', '\n');
            IEnumerable<string> file2 = SecondFile.Trim().Split('\r', '\n');

            File1diff = file1.Where(x => x.Trim() != "").ToList();
            File2diff = file2.Where(x => x.Trim() != "").ToList();

            reportFolderpath = CreateReportFolder(ReportDirectoryPath, ReportDirectoryName);
            //DeleteTempPDFImageFolders(reportFolderpath);  // Rename as needed

            differencesFolderPath = CreateFolder(reportFolderpath, DifferencesDirectoryName);
            TempFolderPath = CreateFolder(reportFolderpath, TemporaryDirectoryName);

            tempFolder1 = CreateFolder(TempFolderPath, "TempDir1");
            tempFolder2 = CreateFolder(TempFolderPath, "TempDir2");

            if (File1diff.Count() > File2diff.Count())
            {
                report.FilePath1 = SourceDoc;
                report.FilePath2 = TargetDoc;
                report.Indicator = false;
                report.Result = "Fail";
                report.IsPass = false;
                report.ComparisonMessage = "Source file has more lines than Target File.";
            }
            else if (File1diff.Count() < File2diff.Count())
            {
                report.FilePath1 = SourceDoc;
                report.FilePath2 = TargetDoc;
                report.Indicator = false;
                report.Result = "Fail";
                report.IsPass = false;
                report.ComparisonMessage = "Target File has more lines than Source File.";
            }

            TextDiff objTextDiff = new TextDiff(); // Assuming this is your custom diff class
            string res = objTextDiff.CompareText(File1diff, File2diff, SourceDoc, TargetDoc);
            report.CompareText = res;

            if (res.IndexOf("<font color='red'>") >= 0)
            {
                report.FilePath1 = SourceDoc;
                report.FilePath2 = TargetDoc;
                report.Indicator = false;
                report.Result = "Fail";
                report.IsPass = false;
                report.ComparisonMessage = "There is a difference in files";
            }
            else
            {
                report.FilePath1 = SourceDoc;
                report.FilePath2 = TargetDoc;
                report.Indicator = true;
                report.Result = "Pass";
                report.IsPass = true;
                report.ComparisonMessage = "Both files are same";
            }

            System.IO.File.Copy(SourceDoc, tempFolder1 + "\\source.docx", true);
            System.IO.File.Copy(TargetDoc, tempFolder2 + "\\target.docx", true);
            saveData(res, differencesFolderPath + "\\result.html");

            return report;
        }

        private string CreateFolder(string reportFolder, string val)
        {
            return Directory.CreateDirectory(reportFolder + "\\" + val).FullName;
        }


        private string CreateReportFolder(string reportDirectoryPath, string reportDirectoryName)
        {
            string reportFolder = Path.Combine(reportDirectoryPath, reportDirectoryName);
            if (!Directory.Exists(reportFolder))
            {
                Directory.CreateDirectory(reportFolder);
            }
            return reportFolder;
        }


        public void saveData(string data, string path)
        {
            StreamWriter stream_writer = new StreamWriter(path);
            stream_writer.Write(data);

            stream_writer.Close();  // Don't forget to close the file!

        }


        //******************* Image Comparison *******************************
        //public ComparisonReport Compare(string FileName1, string Filename2, XmlNode fileIgnorePixels, string reportResult, List<int> sourcePageRange = null, List<int> targetPageRange = null)
        //{
            
        //}



        }
}
