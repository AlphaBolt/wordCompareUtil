using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageDiff;
using Microsoft.Office.Interop.Word;
using System.Runtime.InteropServices;


namespace CompareUtility
{
    public class WordComparer : PDFComparer
    {
        private string ReportDirectoryPath = string.Empty;
        private string ReportDirectoryName = "Reports";
        private string DifferencesDirectoryName = "Differences";
        private string TemporaryDirectoryName = "Temp";
        private int Difference = 0;
        private Dictionary<int, List<string>> imgCoordinate = null;
        private ComparisonReport report;
        CompareOptions options;

        public WordComparer()
        {
            report = new ComparisonReport();
        }

        public ComparisonReport CompareTwoWordDocs(string SourceDocFilePath, string TargetDocFilePath, List<int> sourcePageRange, List<int> targetPageRange, string reportResult = "")
        {
            string FirstFile = string.Empty, SecondFile = string.Empty;
            string textSource = string.Empty;
            string textTarget = string.Empty;

            string differencesFolderPath = string.Empty,
                   tempFolder1 = string.Empty,
                   tempFolder2 = string.Empty,
                   TempFolderPath = string.Empty,
                   reportFolderpath = string.Empty;

            ComparisonReport report = new ComparisonReport();
            report.ImageCompare = false;
            ReportDirectoryPath = Directory.GetParent(reportResult).FullName;

            // Check if files exist and extract text
            if (File.Exists(SourceDocFilePath) && File.Exists(TargetDocFilePath))
            {
                Application wordApp = new Application { Visible = false };
                Document sourceDoc = null;
                Document targetDoc = null;
                try
                {
                    sourceDoc = wordApp.Documents.Open(SourceDocFilePath);
                    textSource = ExtractText(sourceDoc, sourcePageRange);

                    targetDoc = wordApp.Documents.Open(TargetDocFilePath);
                    textTarget = ExtractText(targetDoc, targetPageRange);
                }
                finally
                {
                    if (sourceDoc != null)
                    {
                        sourceDoc.Close();
                        Marshal.ReleaseComObject(sourceDoc);
                    }
                    if (targetDoc != null)
                    {
                        targetDoc.Close();
                        Marshal.ReleaseComObject(targetDoc);
                    }
                    if (wordApp != null)
                    {
                        wordApp.Quit();
                        Marshal.ReleaseComObject(wordApp);
                    }
                }
            }
            else
            {
                report.FilePath1 = SourceDocFilePath;
                report.FilePath2 = TargetDocFilePath;
                report.Indicator = true;
                report.Result = "Fail";
                report.ComparisonMessage = "Files do not exist.";
                return report;
            }

            // Split text into lines (paragraphs in Word are separated by \r)
            List<string> File1diff = textSource.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> File2diff = textTarget.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Create report directories
            reportFolderpath = CreateReportFolder(ReportDirectoryPath, ReportDirectoryName);
            DeleteTempWordImageFolders(reportFolderpath);

            differencesFolderPath = CreateFolder(reportFolderpath, DifferencesDirectoryName);
            TempFolderPath = CreateFolder(reportFolderpath, TemporaryDirectoryName);

            tempFolder1 = CreateFolder(TempFolderPath, "TempDir1");
            tempFolder2 = CreateFolder(TempFolderPath, "TempDir2");

            // Compare line counts
            if (File1diff.Count > File2diff.Count)
            {
                report.FilePath1 = SourceDocFilePath;
                report.FilePath2 = TargetDocFilePath;
                report.Indicator = false;
                report.Result = "Fail";
                report.IsPass = false;
                report.ComparisonMessage = "Source file has more lines than Target File.";
            }
            else if (File1diff.Count < File2diff.Count)
            {
                report.FilePath1 = SourceDocFilePath;
                report.FilePath2 = TargetDocFilePath;
                report.Indicator = false;
                report.Result = "Fail";
                report.IsPass = false;
                report.ComparisonMessage = "Target File has more lines than Source File.";
            }

            // Compare text and generate report
            TextDiff objTextDiff = new TextDiff();
            string res = objTextDiff.CompareText(File1diff, File2diff, SourceDocFilePath, TargetDocFilePath);
            report.CompareText = res;

            if (res.Contains("<font color='red'>"))
            {
                report.FilePath1 = SourceDocFilePath;
                report.FilePath2 = TargetDocFilePath;
                report.Indicator = false;
                report.Result = "Fail";
                report.IsPass = false;
                report.ComparisonMessage = "There is a difference in files";
            }
            else
            {
                report.FilePath1 = SourceDocFilePath;
                report.FilePath2 = TargetDocFilePath;
                report.Indicator = true;
                report.Result = "Pass";
                report.IsPass = true;
                report.ComparisonMessage = "Both files are same";
            }

            // Copy original files to temp folders with original extensions
            string sourceExt = Path.GetExtension(SourceDocFilePath);
            string targetExt = Path.GetExtension(TargetDocFilePath);
            File.Copy(SourceDocFilePath, Path.Combine(tempFolder1, "source" + sourceExt), true);
            File.Copy(TargetDocFilePath, Path.Combine(tempFolder2, "target" + targetExt), true);

            // Save comparison result
            saveData(res, Path.Combine(differencesFolderPath, "result.html"));

            return report;
        }

        private string ExtractText(Document doc, List<int> paragraphRange)
        {
            string text = "";
            if (paragraphRange != null && paragraphRange.Count > 0)
            {
                var paragraphs = doc.Paragraphs;
                foreach (int paraIdx in paragraphRange)
                {
                    if (paraIdx >= 0 && paraIdx < paragraphs.Count)
                    {
                        // Adjust for 1-based indexing in Interop
                        text += paragraphs[paraIdx + 1].Range.Text.Replace('\v', '\n') + "\n";
                    }
                }
            }
            else
            {
                text = doc.Content.Text.Replace('\v', '\n');
            }
            return text;
        }

        private string CreateFolder(string reportFolder, string val)
        {
            return Directory.CreateDirectory(reportFolder + "\\" + val).FullName;
        }

        private string CreateReportFolder(string reportDirectoryPath, string reportDirectoryName)
        {
            if (String.IsNullOrEmpty(reportDirectoryName) && String.IsNullOrEmpty(ReportDirectoryPath))
            {
                string name = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + "_" + DateTime.Now.Millisecond.ToString();
                return Directory.CreateDirectory("Reports\\" + name).FullName;
            }
            else
            {
                string name = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + "_" + DateTime.Now.Millisecond.ToString();
                return Directory.CreateDirectory(ReportDirectoryPath + "\\" + reportDirectoryName + "\\" + name).FullName;
            }
        }

        private void DeleteTempWordImageFolders(string reportFolderpath)
        {
            try
            {
                reportFolderpath = reportFolderpath.Substring(0, reportFolderpath.LastIndexOf("\\"));
                if (reportFolderpath != String.Empty)
                {
                    string[] DirList = Directory.GetDirectories(reportFolderpath);
                    foreach (string dir in DirList)
                    {
                        string[] dirList_Inner = Directory.GetDirectories(dir);
                        foreach (string dir1 in dirList_Inner)
                        {
                            DirectoryInfo drInfo = new DirectoryInfo(dir1);
                            if (drInfo.Name == "Differences")
                            {
                                if (Directory.GetFiles(dir1).Count() == 0)
                                {
                                    Directory.Delete(dir1, true);
                                }
                            }
                            else
                            {
                                Directory.Delete(dir1, true);
                            }
                        }
                        if (Directory.GetDirectories(dir).Count() == 0)
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void saveData(string data, string path)
        {
            StreamWriter stream_writer = new StreamWriter(path);
            stream_writer.Write(data);

            stream_writer.Close();

        }




        //******************* Image Comparison - converting Word to PDF *******************************

        public void WordToPDF(string SourceDocFilePath, string TargetDocFilePath, string reportResult, out string SourcePdfFilepath, out string TargetPdfFilepath)
        {
            if (!string.IsNullOrEmpty(reportResult.Trim()))
            {
                ReportDirectoryPath = Directory.GetParent(reportResult).FullName;
            }

            string tempPdfFolderPath = Path.Combine(ReportDirectoryPath, "tempPDFs");

            // Create temp PDF folder
            CreateTempPdfFolder(tempPdfFolderPath);

            // Create Unique folder
            string uniqueName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + "_" + DateTime.Now.Millisecond.ToString();
            tempPdfFolderPath = Path.Combine(tempPdfFolderPath, uniqueName);
            CreateTempPdfFolder(tempPdfFolderPath);

            string tempDir1 = Path.Combine(tempPdfFolderPath, "tempDir1");
            CreateTempPdfFolder(tempDir1);
            string tempDir2 = Path.Combine(tempPdfFolderPath, "tempDir2");
            CreateTempPdfFolder(tempDir2);

            Application wordApp = new Application { Visible = false };
            Document sourceDoc = null;
            Document targetDoc = null;

            string sourceFilename = string.Empty;
            string targetFilename = string.Empty;

            if (File.Exists(SourceDocFilePath))
            {
                sourceDoc = wordApp.Documents.Open(SourceDocFilePath);
                sourceFilename = Path.GetFileNameWithoutExtension(SourceDocFilePath);
            }
            if (File.Exists(TargetDocFilePath))
            {
                targetDoc = wordApp.Documents.Open(TargetDocFilePath);
                targetFilename = Path.GetFileNameWithoutExtension(TargetDocFilePath);
            }

            SourcePdfFilepath = ConvertWordToPDF(sourceDoc, sourceFilename, tempDir1);
            TargetPdfFilepath = ConvertWordToPDF(targetDoc, targetFilename, tempDir2);

            //Close and release objects
            if (sourceDoc != null)
            {
                sourceDoc.Close();
                Marshal.ReleaseComObject(sourceDoc);
            }
            if (targetDoc != null)
            {
                targetDoc.Close();
                Marshal.ReleaseComObject(targetDoc);
            }
            if (wordApp != null)
            {
                wordApp.Quit();
                Marshal.ReleaseComObject(wordApp);
            }
        }

        private string ConvertWordToPDF(Document wordDoc, string filename, string directory)
        {
            try
            {
                string pdfFilePath = Path.Combine(directory, filename) + ".pdf";
                if (!File.Exists(pdfFilePath))
                {
                    wordDoc.ExportAsFixedFormat(pdfFilePath, WdExportFormat.wdExportFormatPDF);
                    return pdfFilePath;
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message.ToString();
            }
            
            return string.Empty;
        }

        // tempPDFs -> with UniqueName -> tempDir1, tempDir2 having source and destination PDFs
        private void CreateTempPdfFolder(string folderpath)
        {
            if (!Directory.Exists(folderpath))
            {
                Directory.CreateDirectory(folderpath);
            }
        }

        // Delete tempPDFs folder
        public void DeleteTempPdfFolder(string reportResult)
        {
            if (!string.IsNullOrEmpty(reportResult.Trim()))
            {
                ReportDirectoryPath = Directory.GetParent(reportResult).FullName;
            }

            string tempPdfFolderPath = Path.Combine(ReportDirectoryPath, "tempPDFs");
            Directory.Delete(tempPdfFolderPath, true);
        }



    }
}
