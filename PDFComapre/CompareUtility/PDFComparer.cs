using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using ImageDiff;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Xml;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Syncfusion.Pdf.Parsing;

namespace CompareUtility
{

    public class PDFComparer
    {
        public GhostscriptVersionInfo _lastInstalledVersion;
        public GhostscriptRasterizer _rasterizer = null;
        CompareOptions options;

        int desired_x_dpi = 150;
        int desired_y_dpi = 150;

        private string ReportDirectoryPath = string.Empty;
        private string ReportDirectoryName = string.Empty;
        private string DifferencesDirectoryName = string.Empty;
        private string TemporaryDirectoryName = string.Empty;
        private int Difference = 0;
        Dictionary<int, List<string>> imgCoordinate = null;
        ComparisonReport report;

        public PDFComparer()
        {
            ReportDirectoryName = "Reports";
            DifferencesDirectoryName = "Differences";
            TemporaryDirectoryName = "Temp";
            report = new ComparisonReport();
        }

        public PDFComparer(string reportDirectoryPath, string reportDirectoryName, string differencesDirectoryName, string temporaryDirectoryName)
        {
            ReportDirectoryPath = reportDirectoryPath;
            ReportDirectoryName = reportDirectoryName;
            DifferencesDirectoryName = differencesDirectoryName;
            TemporaryDirectoryName = temporaryDirectoryName;
            report = new ComparisonReport();
        }

        public ComparisonReport Compare(string PolicyNumber, string FileName1, string Filename2, XmlNode fileIgnorePixels)
        {
            string differencesFolderPath = string.Empty, tempFolder1 = string.Empty, tempFolder2 = string.Empty, TempFolderPath = string.Empty, reportFolderpath = string.Empty;
            try
            {
                options = new CompareOptions
                {
                    AnalyzerType = AnalyzerTypes.CIE76,
                    JustNoticeableDifference = 8.8,
                    DetectionPadding = 2,
                    Labeler = LabelerTypes.ConnectedComponentLabeling,
                    BoundingBoxColor = Color.Red,
                    BoundingBoxPadding = 5,
                    BoundingBoxMode = BoundingBoxModes.Multiple
                };

                if (Environment.Is64BitOperatingSystem)
                    _lastInstalledVersion = new GhostscriptVersionInfo(new Version(0, 0, 0), @"gsdll64.dll", string.Empty, GhostscriptLicense.GPL);
                else
                    _lastInstalledVersion = new GhostscriptVersionInfo(new Version(0, 0, 0), @"gsdll32.dll", string.Empty, GhostscriptLicense.GPL);

                imgCoordinate = new Dictionary<int, List<string>>();

                if (fileIgnorePixels != null)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(fileIgnorePixels.OuterXml.ToString());

                    XmlNodeList IgnoreCoordinatesPage = xmlDocument.SelectNodes("//Page");
                    foreach (XmlNode page in IgnoreCoordinatesPage)
                    {
                        int pagenumber = Convert.ToInt32(page.SelectSingleNode("@pagenumber").Value);
                        List<string> cordinatespoints = new List<string>();
                        foreach (XmlNode coordinate in page.ChildNodes)
                        {
                            if (coordinate.Name == "Coordinates")
                            {
                                String sPoints;

                                sPoints = coordinate.SelectSingleNode("@x1").Value + "," + coordinate.SelectSingleNode("@y1").Value + ":" +
                                            coordinate.SelectSingleNode("@x2").Value + "," + coordinate.SelectSingleNode("@y2").Value;

                                cordinatespoints.Add(sPoints);
                            }
                        }
                        imgCoordinate.Add(pagenumber, cordinatespoints);
                    }
                    xmlDocument = null;
                }

                _rasterizer = new GhostscriptRasterizer();
                reportFolderpath = CreateReportFolder(ReportDirectoryPath, ReportDirectoryName);
                //DeleteTempPDFImageFolders(reportFolderpath);
                differencesFolderPath = CreateFolder(reportFolderpath, DifferencesDirectoryName);
                TempFolderPath = CreateFolder(reportFolderpath, TemporaryDirectoryName);
                tempFolder1 = CreateFolder(TempFolderPath, "TempDir1");
                tempFolder2 = CreateFolder(TempFolderPath, "TempDir2");

                report.FilePath1 = FileName1;
                report.FilePath2 = Filename2;
                report.PolicyNumber = PolicyNumber;
                report.Indicator = true;
                report.Result = "Success";
                report.ComparisonMessage = "Both PDF are same.";

                int pagenum = 0;
                Difference = 0;

                bool filepresent = true;
                bool numberofpagessame = CheckNumberOfPagesSame(FileName1, Filename2);
                string filename = FileName1;
                string PagesNotSame = string.Empty;

                if (filepresent && numberofpagessame)
                {
                    int numberOfPages = NumberOfPages(FileName1);
                    ConvertPDFToImages(FileName1, tempFolder1);
                    ConvertPDFToImages(Filename2, tempFolder2);
                    for (int i = 1; i <= numberOfPages; i++)
                    {
                        bool result = ComparePage(tempFolder1 + "\\Page" + i + ".jpg", tempFolder2 + "\\Page" + i + ".jpg", differencesFolderPath, i);
                        if (result)
                        {
                            report.AddPageReport(i, Difference, "Passed", "Pages are same.", "");
                        }
                        else
                        {
                            report.AddPageReport(i, Difference, "Failed", "Pages are not same", differencesFolderPath + "\\CombinedDiff" + i + ".jpg");
                            report.Indicator = false;
                            report.Result = "Fail";
                            report.PolicyNumber = PolicyNumber;
                            PagesNotSame = PagesNotSame + i.ToString() + ", ";
                        }
                    }
                    if (report.Indicator == false)
                    {
                        PagesNotSame = PagesNotSame.Substring(0, PagesNotSame.LastIndexOf(',')).Trim();
                        report.ComparisonMessage = "Following Pages are different - " + PagesNotSame;
                    }
                }
                else
                {
                    report.ComparisonMessage = "Page Numbers are Not Same";
                    report.Result = "Fail";
                    report.NumberOfPagesFile1 = NumberOfPages(FileName1);
                    report.NumberOfPagesFile2 = NumberOfPages(Filename2);
                    report.Indicator = false;
                    report.PolicyNumber = PolicyNumber;
                }
                return report;
            }
            catch (Exception ex)
            {
                report.FilePath1 = FileName1;
                report.FilePath2 = Filename2;
                report.Indicator = false;
                report.Result = "Fail";
                report.ComparisonMessage = ex.Message;
                report.PolicyNumber = PolicyNumber;
                return report;
            }
            finally
            {
                try
                {
                    if (differencesFolderPath != string.Empty && Directory.Exists(differencesFolderPath))
                    {
                        if (Directory.GetFiles(differencesFolderPath).Count() == 0)
                        {
                            Directory.Delete(differencesFolderPath, true);
                        }
                    }
                    if (tempFolder2 != string.Empty && Directory.Exists(tempFolder2))
                    {
                        Directory.Delete(tempFolder2, true);
                    }
                    if (tempFolder1 != string.Empty && Directory.Exists(tempFolder1))
                    {
                        Directory.Delete(tempFolder1, true);
                    }
                    if (TempFolderPath != string.Empty && Directory.Exists(TempFolderPath))
                    {
                        Directory.Delete(TempFolderPath, true);
                    }
                    if (reportFolderpath != string.Empty && Directory.Exists(reportFolderpath))
                    {
                        if (Directory.GetDirectories(reportFolderpath).Count() == 0)
                        {
                            Directory.Delete(reportFolderpath, true);
                        }
                    }
                }
                catch (Exception ex)
                { }
            }
        }


        public ComparisonReport Compare(string FileName1, string Filename2, XmlNode fileIgnorePixels, string reportResult, List<int> sourcePageRange = null, List<int> targetPageRange = null)
        {
            string differencesFolderPath = string.Empty, tempFolder1 = string.Empty, tempFolder2 = string.Empty, TempFolderPath = string.Empty, reportFolderpath = string.Empty;

            if (!string.IsNullOrEmpty(reportResult.Trim()))
            {
                ReportDirectoryPath = Directory.GetParent(reportResult).FullName;
            }

            try
            {
                options = new CompareOptions
                {
                    AnalyzerType = AnalyzerTypes.CIE76,
                    JustNoticeableDifference = 8.8,
                    DetectionPadding = 2,
                    Labeler = LabelerTypes.ConnectedComponentLabeling,
                    BoundingBoxColor = Color.Red,
                    BoundingBoxPadding = 5,
                    BoundingBoxMode = BoundingBoxModes.Multiple

                };

                if (Environment.Is64BitOperatingSystem)
                    _lastInstalledVersion = new GhostscriptVersionInfo(new Version(0, 0, 0), @"gsdll64.dll", string.Empty, GhostscriptLicense.GPL);
                else
                    _lastInstalledVersion = new GhostscriptVersionInfo(new Version(0, 0, 0), @"gsdll32.dll", string.Empty, GhostscriptLicense.GPL);

                imgCoordinate = new Dictionary<int, List<string>>();

                if (fileIgnorePixels != null)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(fileIgnorePixels.OuterXml.ToString());

                    XmlNodeList IgnoreCoordinatesPage = xmlDocument.SelectNodes("//Page");
                    foreach (XmlNode page in IgnoreCoordinatesPage)
                    {
                        int pagenumber = Convert.ToInt32(page.SelectSingleNode("@pagenumber").Value);
                        //XmlNodeList Coordinates = page.ChildNodes("//Coordinates");
                        List<string> cordinatespoints = new List<string>();
                        foreach (XmlNode coordinate in page.ChildNodes)
                        {
                            if (coordinate.Name == "Coordinates")
                            {
                                String sPoints;

                                sPoints = coordinate.SelectSingleNode("@x1").Value + "," + coordinate.SelectSingleNode("@y1").Value + ":" +
                                            coordinate.SelectSingleNode("@x2").Value + "," + coordinate.SelectSingleNode("@y2").Value;

                                cordinatespoints.Add(sPoints);
                            }
                        }
                        imgCoordinate.Add(pagenumber, cordinatespoints);
                    }
                    xmlDocument = null;
                }

                _rasterizer = new GhostscriptRasterizer();
                reportFolderpath = CreateReportFolder(ReportDirectoryPath, ReportDirectoryName);
                //For Deleting junk image folders created in Reports Directory
                //DeleteTempPDFImageFolders(reportFolderpath);
                differencesFolderPath = CreateFolder(reportFolderpath, DifferencesDirectoryName);
                TempFolderPath = CreateFolder(reportFolderpath, TemporaryDirectoryName);
                tempFolder1 = CreateFolder(TempFolderPath, "TempDir1");
                tempFolder2 = CreateFolder(TempFolderPath, "TempDir2");

                report.DifferencesFolderPath = differencesFolderPath;
                report.FilePath1 = FileName1;
                report.FilePath2 = Filename2;
                report.ImageCompare = true;
                report.Indicator = true;
                report.Result = "Pass";
                report.ComparisonMessage = "Both PDF are same.";
                report.IsPass = true;

                int pagenum = 0;
                Difference = 0;

                bool numberofpagessame = CheckNumberOfPagesSame(FileName1, Filename2);
                string filename = FileName1;
                string PagesNotSame = string.Empty;

                if (File.Exists(FileName1) && File.Exists(Filename2) && numberofpagessame)
                {
                    int numberOfPages = NumberOfPages(FileName1);

                    if (sourcePageRange != null && sourcePageRange.Count > 0 && targetPageRange != null && targetPageRange.Count > 0)
                    {
                        if (sourcePageRange.Count == targetPageRange.Count)
                        {
                            ConvertPDFToImages(FileName1, tempFolder1, sourcePageRange);
                            ConvertPDFToImages(Filename2, tempFolder2, targetPageRange);

                            for (int index = 0; index < sourcePageRange.Count(); index++)
                            {
                                PagesNotSame = ComparePDFPages(differencesFolderPath, tempFolder1, tempFolder2, PagesNotSame, sourcePageRange[index], targetPageRange[index]);
                            }

                            if (report.Indicator == false)
                            {
                                PagesNotSame = PagesNotSame.Substring(0, PagesNotSame.LastIndexOf(',')).Trim();
                                report.ComparisonMessage = "Following Pages are different - " + PagesNotSame;
                                report.IsPass = false;
                            }
                        }
                    }
                    else
                    {
                        sourcePageRange = null;
                        targetPageRange = null;

                        ConvertPDFToImages(FileName1, tempFolder1, sourcePageRange);
                        ConvertPDFToImages(Filename2, tempFolder2, targetPageRange);

                        for (int i = 1; i <= numberOfPages; i++)
                        {
                            PagesNotSame = ComparePDFPages(differencesFolderPath, tempFolder1, tempFolder2, PagesNotSame, i);
                        }

                        if (report.Indicator == false)
                        {
                            PagesNotSame = PagesNotSame.Substring(0, PagesNotSame.LastIndexOf(',')).Trim();
                            report.ComparisonMessage = "Following Pages are different - " + PagesNotSame;
                            report.IsPass = false;
                        }
                    }
                }
                else if (File.Exists(FileName1) && File.Exists(Filename2) && !numberofpagessame)
                {
                    if (sourcePageRange != null && sourcePageRange.Count > 0 && targetPageRange != null && targetPageRange.Count > 0)
                    {
                        if (sourcePageRange.Count == targetPageRange.Count)
                        {
                            int sourceNumberOfPages = NumberOfPages(FileName1);
                            int targetNumberOfPages = NumberOfPages(Filename2);

                            ConvertPDFToImages(FileName1, tempFolder1, sourcePageRange);
                            ConvertPDFToImages(Filename2, tempFolder2, targetPageRange);

                            for (int index = 0; index < sourcePageRange.Count(); index++)
                            {
                                PagesNotSame = ComparePDFPages(differencesFolderPath, tempFolder1, tempFolder2, PagesNotSame, sourcePageRange[index], targetPageRange[index]);
                            }

                            if (report.Indicator == false)
                            {
                                PagesNotSame = PagesNotSame.Substring(0, PagesNotSame.LastIndexOf(',')).Trim();
                                report.ComparisonMessage = "Following Pages are different - " + PagesNotSame;
                                report.IsPass = false;
                            }
                        }
                        else
                        {
                            report.ComparisonMessage = "Please provide source and target same range to compare due to different number of pages.";
                            report.IsPass = false;
                        }
                    }
                    else
                    {
                        report.ComparisonMessage = "Please provide source and target same range to compare due to different number of pages.";
                        report.IsPass = false;
                    }
                }
                return report;
            }
            catch (Exception ex)
            {
                report.FilePath1 = FileName1;
                report.FilePath2 = Filename2;
                report.Indicator = false;
                report.Result = "Fail";
                report.ComparisonMessage = ex.Message;
                return report;
            }
            finally
            {
                try
                {
                    if (differencesFolderPath != string.Empty && Directory.Exists(differencesFolderPath))
                    {
                        if (Directory.GetFiles(differencesFolderPath).Count() == 0)
                        {
                            Directory.Delete(differencesFolderPath, true);
                        }
                    }
                    if (tempFolder2 != string.Empty && Directory.Exists(tempFolder2))
                    {
                        var t = Directory.GetLastAccessTime(tempFolder2);
                        Directory.Delete(tempFolder2, true);
                    }
                    if (tempFolder1 != string.Empty && Directory.Exists(tempFolder1))
                    {
                        Directory.Delete(tempFolder1, true);
                    }
                    if (TempFolderPath != string.Empty && Directory.Exists(TempFolderPath))
                    {
                        Directory.Delete(TempFolderPath, true);
                    }
                    if (reportFolderpath != string.Empty && Directory.Exists(reportFolderpath))
                    {
                        if (Directory.GetDirectories(reportFolderpath).Count() == 0)
                        {
                            Directory.Delete(reportFolderpath, true);
                        }
                    }
                }
                catch (Exception ex)
                { }
            }
        }

        private string ComparePDFPages(string differencesFolderPath, string tempFolder1, string tempFolder2, string PagesNotSame, int sourceRange, int? targetRange = null)
{
    if (targetRange != null && targetRange > 0)
    {
        bool result = ComparePage(tempFolder1 + "\\Page" + sourceRange + ".jpg", tempFolder2 + "\\Page" + targetRange + ".jpg", differencesFolderPath, sourceRange);
        if (result)
        {
            report.AddPageReport(sourceRange, Difference, "Passed", "Pages are same.", differencesFolderPath + "\\CombinedDiff" + sourceRange + ".jpg");
            report.IsPass = true;
        }
        else
        {
            report.AddPageReport(sourceRange, Difference, "Fail", "Pages are not same", differencesFolderPath + "\\CombinedDiff" + sourceRange + ".jpg");
            report.Indicator = false;
            report.Result = "Fail";
            PagesNotSame = PagesNotSame + " Source Pdf Page- " + sourceRange.ToString() + " Target Pdf Page- " + targetRange.ToString() + ", ";
            report.IsPass = false;
        }
        return PagesNotSame;
    }
    else
    {
        bool result = ComparePage(tempFolder1 + "\\Page" + sourceRange + ".jpg", tempFolder2 + "\\Page" + sourceRange + ".jpg", differencesFolderPath, sourceRange);
        if (result)
        {
            report.AddPageReport(sourceRange, Difference, "Passed", "Pages are same.", differencesFolderPath + "\\CombinedDiff" + sourceRange + ".jpg");
            report.IsPass = true;
        }
        else
        {
            report.AddPageReport(sourceRange, Difference, "Fail", "Pages are not same", differencesFolderPath + "\\CombinedDiff" + sourceRange + ".jpg");
            report.Indicator = false;
            report.Result = "Fail";
            PagesNotSame = PagesNotSame + sourceRange.ToString() + ", ";
            report.IsPass = false;
        }
        return PagesNotSame;
    }
}

        private void DeleteTempPDFImageFolders(string reportFolderpath)
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

        private void SetFileAttributes(string path)
        {
            string[] filePath = Directory.GetFiles(path);
            foreach (string file in filePath)
            {
                var filelocal = new FileInfo(file);
                filelocal.Attributes = FileAttributes.Normal;
            }
        }

        private string CreateReportFolder(string reportDirectoryPath, string reportDirectoryName)
        {
            if (String.IsNullOrEmpty(reportDirectoryName) && String.IsNullOrEmpty(ReportDirectoryPath))
            {
                //string name = DateTime.Now.Year.ToString() +  DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() +"_"+ DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString();
                string name = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() +"_"+ DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + "_" + DateTime.Now.Millisecond.ToString();
                return Directory.CreateDirectory("Reports\\" + name).FullName;
            }
            else
            {
                //string name = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString();
                string name = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() +"_"+ DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() +"_"+ DateTime.Now.Millisecond.ToString();
                return Directory.CreateDirectory(ReportDirectoryPath + "\\" + reportDirectoryName + "\\" + name).FullName;
            }
        }

        private string CreateFolder(string reportFolder, string val)
        {
            return Directory.CreateDirectory(reportFolder + "\\" + val).FullName;
        }

        public bool CheckNumberOfPagesSame(string pdfFilePath1, string pdfFilePath2)
        {
            
            _rasterizer.Open(pdfFilePath1, _lastInstalledVersion, false);
            int pagecount1 = _rasterizer.PageCount;
            _rasterizer.Close();

            _rasterizer.Open(pdfFilePath2, _lastInstalledVersion, false);
            int pagecount2 = _rasterizer.PageCount;
            _rasterizer.Close();

            if (pagecount1 == pagecount2)
                return true;
            else
                return false;
        }

        private int NumberOfPages(string FilePath)
        {
            _rasterizer.Open(FilePath, _lastInstalledVersion, false);
            int pagecount = _rasterizer.PageCount;
            _rasterizer.Close();
            return pagecount;
        }

        private void ConvertPDFToImages(string pdfFilePath, string DirPath, List<int> sourcePageRange = null)
        {
            _rasterizer.Open(pdfFilePath, _lastInstalledVersion, false);
            for (int pageNumber = 1; pageNumber <= _rasterizer.PageCount; pageNumber++)
            {
                if (sourcePageRange != null && sourcePageRange.Count > 0)
                {

                    if (sourcePageRange.Contains(pageNumber))
                    {
                        PDFToImages(DirPath, pageNumber);
                    }
                }
                else
                {
                    PDFToImages(DirPath, pageNumber);
                }
            }
            _rasterizer.Close();
        }

        private void PDFToImages(string DirPath, int pageNumber)
        {
            string pageFilePath = Path.Combine(DirPath, "Page" + pageNumber.ToString() + ".jpg");
            Image img = _rasterizer.GetPage(desired_x_dpi, desired_y_dpi, pageNumber);

            //=====Added By Reetesh
            Bitmap rowsource = new Bitmap(img);
            if (imgCoordinate.ContainsKey(pageNumber))
            {
                foreach (var dicItem in imgCoordinate)
                {
                    if (dicItem.Key == pageNumber)
                    {
                        List<string> coodPoints = dicItem.Value;
                        Bitmap cropSource = rowsource;
                        foreach (var coodItem in coodPoints)
                        {
                            if (coodItem.IndexOf(":") > -1)
                            {
                                string[] splitCoordinate = coodItem.ToString().Split(":".ToCharArray(), StringSplitOptions.None);
                                string[] x1y1Cood = splitCoordinate[0].Split(",".ToCharArray(), StringSplitOptions.None);
                                string[] x2y2Cood = splitCoordinate[1].Split(",".ToCharArray(), StringSplitOptions.None);
                                int x1 = Convert.ToInt32(x1y1Cood[0]);
                                int y1 = Convert.ToInt32(x1y1Cood[1]);
                                int x2 = Convert.ToInt32(x2y2Cood[0]);
                                int y2 = Convert.ToInt32(x2y2Cood[1]);
                                Point p = new Point(x1, y1);
                                int width = x2 - x1;
                                int height = y2 - y1;
                                cropSource = MakeImageWithoutArea(cropSource, p, width, height);
                            }
                        }
                        cropSource.Save(pageFilePath, ImageFormat.Jpeg);
                        cropSource.Dispose();
                        break;
                    }
                }
            }
            else //====== End Added By Reetesh
            {
                img.Save(pageFilePath, ImageFormat.Jpeg);
                img.Dispose();
            }
            rowsource.Dispose();
        }

        private bool ComparePage(string PagePath1, string Pagepath2, string reportDiffPath, int pageNum)
        {
            if (File.Exists(PagePath1) && File.Exists(Pagepath2))
            {
                Bitmap source = (Bitmap)Bitmap.FromFile(PagePath1);
                Bitmap destination = (Bitmap)Bitmap.FromFile(Pagepath2);

                Difference = 0;
                IsDifferentImage(source, destination);
                using (Bitmap diff = GetDifference(source, destination))
                {
                    string combinedDiffPath;
                    if (Difference > 0)
                    {
                        // Save the difference image with red boxes
                        diff.Save(reportDiffPath + "\\Page" + pageNum + ".jpg", ImageFormat.Jpeg);
                        // Combine the original source image with the difference image (with red boxes)
                        combinedDiffPath = CombineImages(PagePath1, reportDiffPath + "\\Page" + pageNum + ".jpg", reportDiffPath, pageNum);
                        source.Dispose();
                        destination.Dispose();
                        return false; // Images are different
                    }
                    else
                    {
                        // When images are the same, combine the two original images (no diff image needed)
                        combinedDiffPath = CombineImages(PagePath1, Pagepath2, reportDiffPath, pageNum);
                        source.Dispose();
                        destination.Dispose();
                        return true; // Images are the same
                    }
                }
            }

            return true; // Default case when files don't exist
        }

        private unsafe void IsDifferentImage(Bitmap image1, Bitmap image2)
        {
            if (image1 == null | image2 == null)
                Difference = 0;

            if (image1.Height != image2.Height || image1.Width != image2.Width)
                Difference = 1;

            Bitmap diffImage = image2.Clone() as Bitmap;
            int height = image1.Height< image2.Height? image1.Height: image2.Height;
            int width = image1.Width < image2.Width ? image1.Width : image2.Width;

            BitmapData data1 = image1.LockBits(new Rectangle(0, 0, width, height),
                                               ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            BitmapData data2 = image2.LockBits(new Rectangle(0, 0, width, height),
                                               ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            BitmapData diffData = diffImage.LockBits(new Rectangle(0, 0, width, height),
                                                   ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            byte* data1Ptr = (byte*)data1.Scan0;
            byte* data2Ptr = (byte*)data2.Scan0;
            byte* diffPtr = (byte*)diffData.Scan0;
            byte[] swapColor = new byte[3];
            int rowPadding = data1.Stride - (image1.Width * 3);

            // iterate over height (rows)
            for (int i = 0; i < height; i++)
            {
                // iterate over width (columns)
                for (int j = 0; j < width; j++)
                {
                    int same = 0;

                    byte[] tmp = new byte[3];

                    // compare pixels and copy new values into temporary array
                    for (int x = 0; x < 3; x++)
                    {
                        tmp[x] = data2Ptr[0];
                        if (data1Ptr[0] == data2Ptr[0])
                        {
                            same++;
                        }
                        else
                        {
                            Difference = Difference + 1;
                        }
                        data1Ptr++; // advance image1 ptr
                        data2Ptr++; // advance image2 ptr
                    }
                }
                // at the end of each column, skip extra padding
                if (rowPadding > 0)
                {
                    data1Ptr += rowPadding;
                    data2Ptr += rowPadding;
                    diffPtr += rowPadding;
                }
                if (Difference > 0)
                    break;
            }

            image1.UnlockBits(data1);
            image2.UnlockBits(data2);
            diffImage.UnlockBits(diffData);
            diffImage.Dispose();
        }
        private Bitmap GetDifference(Bitmap image1, Bitmap image2)
        {
            BitmapComparer cmp = new BitmapComparer(options);
            return cmp.Compare(image1, image2);
        }

        private string CombineImages(string FileName1, string Filename2, string DiferencespPath, int PageNum)
        {
            string[] files = new string[] { FileName1, Filename2 };

            Bitmap NewImage = Combine(files);

            string Path = DiferencespPath + "\\CombinedDiff" + PageNum + ".jpg";

            NewImage.Save(Path, ImageFormat.Jpeg);
            return Path;
        }

        private System.Drawing.Bitmap Combine(string[] files)
        {
            //read all images into memory
            List<System.Drawing.Bitmap> images = new List<System.Drawing.Bitmap>();
            System.Drawing.Bitmap finalImage = null;

            try
            {
                int width = 0;
                int height = 0;

                foreach (string image in files)
                {
                    //create a Bitmap from the file and add it to the list
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image);

                    //update the size of the final bitmap
                    width += bitmap.Width;
                    height = bitmap.Height > height ? bitmap.Height : height;

                    images.Add(bitmap);
                }

                //create a bitmap to hold the combined image
                finalImage = new System.Drawing.Bitmap(width, height);

                //get a graphics object from the image so we can draw on it
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(finalImage))
                {
                    //set background color
                    g.Clear(System.Drawing.Color.Black);

                    //go through each image and draw it on the final image
                    int offset = 0;
                    foreach (System.Drawing.Bitmap image in images)
                    {
                        g.DrawImage(image,
                          new System.Drawing.Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                }

                return finalImage;
            }
            catch (Exception ex)
            {
                if (finalImage != null)
                    finalImage.Dispose();

                throw ex;
            }
            finally
            {
                //clean up memory
                foreach (System.Drawing.Bitmap image in images)
                {
                    image.Dispose();
                }
            }
        }
        private Bitmap MakeImageWithArea(Bitmap source_bm, List<Point> points)
        {
            // Copy the image.
            Bitmap bm = new Bitmap(source_bm.Width, source_bm.Height);

            // Clear the selected area.
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.Clear(Color.Transparent);

                // Make a brush that contains the original image.
                using (Brush brush = new TextureBrush(source_bm))
                {
                    // Fill the selected area.
                    //gr.FillPolygon(brush, points.ToArray());
                    gr.FillRectangle(brush, new Rectangle(new Point(820, 660), new Size(796, 55)));
                }
            }
            return bm;
        }

        private Bitmap MakeImageWithoutArea(Bitmap source_bm, Point point, int width, int height)
        {
            //Copy the image.
            Bitmap bm = new Bitmap(source_bm);

            //Clear the selected area.
            using (Graphics gr = Graphics.FromImage(bm))
            {
                GraphicsPath path = new GraphicsPath();
                path.AddRectangle(new Rectangle(point, new Size(width, height)));
                gr.SetClip(path);
                gr.Clear(Color.Transparent);
                gr.ResetClip();
            }
            return bm;
        }

        private Dictionary<int, List<string>> LoadIgnorePixesInFile(XmlDocument ignorepixels)
        {
            Dictionary<int, List<string>> list = new Dictionary<int, List<string>>();
            return list;
        }

        public ComparisonReport CompareTwoPDF(string SourcePDF, string TargetPDF, List<int> sourcePageRange, List<int> targetPageRange, string reportResult = "")
        {

            string FirstFile = string.Empty, SecondFile = string.Empty;

            string textSource = string.Empty;
            string textTarget = string.Empty;

            string differencesFolderPath = string.Empty,
                             tempFolder1 = string.Empty,
                             tempFolder2 = string.Empty,
                          TempFolderPath = string.Empty,
                        reportFolderpath = string.Empty;

            report.ImageCompare = false;

            ReportDirectoryPath = Directory.GetParent(reportResult).FullName;

            if (File.Exists(SourcePDF) && File.Exists(TargetPDF))
            {
                //syncfusion.pdf.winforms library
                PdfLoadedDocument pdfSourceFIle = new PdfLoadedDocument(SourcePDF);

                for (int page = 0; page < pdfSourceFIle.Pages.Count; page++)
                {
                    if (sourcePageRange != null && sourcePageRange.Count > 0)
                    {
                        if (sourcePageRange.Contains(page))
                        {
                            var currentPage = pdfSourceFIle.Pages[page];
                            textSource += currentPage.ExtractText(true);
                        }
                    }
                    else
                    {
                        var currentPage = pdfSourceFIle.Pages[page];
                        textSource = textSource + currentPage.ExtractText();
                    }
                }
                pdfSourceFIle.Close(true);

                PdfLoadedDocument pdfTargetFIle = new PdfLoadedDocument(TargetPDF);

                for (int page = 0; page < pdfTargetFIle.Pages.Count; page++)
                {
                    if (targetPageRange != null && targetPageRange.Count > 0)
                    {
                        if (targetPageRange.Contains(page))
                        {
                            var currentPage = pdfTargetFIle.Pages[page];
                            textTarget += currentPage.ExtractText(true);
                        }
                    }
                    else
                    {
                        var currentPage = pdfTargetFIle.Pages[page];
                        textTarget = textTarget + currentPage.ExtractText();
                    }
                }
                pdfTargetFIle.Close(true);


                PdfReader reader = new PdfReader(SourcePDF);
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    if (sourcePageRange != null && sourcePageRange.Count > 0)
                    {
                        if (sourcePageRange.Contains(page))
                        {
                            ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();// new SimpleTextExtractionStrategy();
                            FirstFile += " \n" + PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                            //string s =" \n"+ PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                            //FirstFile += Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(s)));
                        }
                    }
                    else
                    {
                        ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();// new SimpleTextExtractionStrategy();
                        FirstFile += " \n" + PdfTextExtractor.GetTextFromPage(reader, page, strategy);

                        //string s = " \n" + PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                        //FirstFile += Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(s)));
                    }
                }

                reader.Close();

                PdfReader reader1 = new PdfReader(TargetPDF);
                for (int page = 1; page <= reader1.NumberOfPages; page++)
                {
                    if (targetPageRange != null && targetPageRange.Count > 0)
                    {
                        if (targetPageRange.Contains(page))
                        {
                            ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();// new SimpleTextExtractionStrategy();
                            SecondFile += " \n" + PdfTextExtractor.GetTextFromPage(reader1, page, strategy);
                            //byte[] bytes = Encoding.UTF8.GetBytes(SecondFile);
                            //byte[] converted = Encoding.Convert(Encoding.Default, Encoding.UTF8, bytes);
                            //SecondFile = Encoding.UTF8.GetString(converted);
                        }
                    }
                    else
                    {
                        ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();// new SimpleTextExtractionStrategy();
                        SecondFile += " \n" + PdfTextExtractor.GetTextFromPage(reader1, page, strategy);
                        //byte[] bytes = Encoding.UTF8.GetBytes(SecondFile);
                        //byte[] converted = Encoding.Convert(Encoding.Default, Encoding.UTF8, bytes);
                        //SecondFile = Encoding.UTF8.GetString(converted);
                    }
                }
                reader1.Close();
            }
            else
            {
                //Console.WriteLine("Files does not exist.");
                report.FilePath1 = SourcePDF;
                report.FilePath2 = TargetPDF;
                report.Indicator = true;
                report.Result = "Fail";
                report.ComparisonMessage = "Files does not exist.";
                return report;
            }

            List<string> File1diff;
            List<string> File2diff;

            IEnumerable<string> file1 = FirstFile.Trim().Split('\r', '\n');
            IEnumerable<string> file2 = SecondFile.Trim().Split('\r', '\n');

            IEnumerable<string> file11 = textSource.Trim().Split(new[] { '\r', '\n' });
            IEnumerable<string> file12 = textTarget.Trim().Split(new[] { '\r', '\n' });

            var File11diff = file1.Where(x => x.Trim() != "").ToList();
            var File12diff = file2.Where(x => x.Trim() != "").ToList();

            File1diff = file1.Where(x => x.Trim() != "").ToList();
            File2diff = file2.Where(x => x.Trim() != "").ToList();

            //File1diff = File1diff.Where(x => x.Count() != 2 && (x.ToLower() != "th" || x.ToLower() != "nd" || x.ToLower() != "st" || x.ToLower() != "rd")).ToList();
            //File2diff = File2diff.Where(x => x.Count() != 2 && (x.ToLower() != "th" || x.ToLower() != "nd" || x.ToLower() != "st" || x.ToLower() != "rd")).ToList();

            reportFolderpath = CreateReportFolder(ReportDirectoryPath, ReportDirectoryName);
            //For Deleting junk image folders created in Reports Directory
            DeleteTempPDFImageFolders(reportFolderpath);

            differencesFolderPath = CreateFolder(reportFolderpath, DifferencesDirectoryName);
            TempFolderPath = CreateFolder(reportFolderpath, TemporaryDirectoryName);

            tempFolder1 = CreateFolder(TempFolderPath, "TempDir1");
            tempFolder2 = CreateFolder(TempFolderPath, "TempDir2");

            if (File1diff.Count() > File2diff.Count())
            {
                //Console.WriteLine("File 1 has less number of lines than File 2.");
                report.FilePath1 = SourcePDF;
                report.FilePath2 = TargetPDF;
                report.Indicator = false;
                report.Result = "Fail";
                report.IsPass = false;
                report.ComparisonMessage = "Source file has less number of lines than Target File.";

            }
            else if (File1diff.Count() < File2diff.Count())
            {
                //Console.WriteLine("File 2 has less number of lines than File 1.");
                report.FilePath1 = SourcePDF;
                report.FilePath2 = TargetPDF;
                report.Indicator = false;
                report.Result = "Fail";
                report.IsPass = false;
                report.ComparisonMessage = "Target File has less number of lines than Source File.";

            }
            //else
            //{
            //    //Console.WriteLine("File 1 and File 2, both are having same number of lines.");
            //}
            TextDiff objTextDiff = new TextDiff();

            string res = objTextDiff.CompareText(File1diff, File2diff, SourcePDF, TargetPDF);
            report.CompareText = res;

            if (res.IndexOf("<font color='red'>") >= 0)
            {
                report.FilePath1 = SourcePDF;
                report.FilePath2 = TargetPDF;
                report.Indicator = false;
                report.Result = "Fail";
                report.IsPass = false;
                report.ComparisonMessage = "There is a difference in files";
            }
            else
            {
                report.FilePath1 = SourcePDF;
                report.FilePath2 = TargetPDF;
                report.Indicator = true;
                report.Result = "Pass";
                report.IsPass = true;
                report.ComparisonMessage = "Both File are same";
            }

            //saveData(FirstFile, tempFolder1 + "\\source.html");
            //saveData(SecondFile, tempFolder2 + "\\target.html");

            System.IO.File.Copy(SourcePDF, tempFolder1 + "\\source.pdf", true);
            System.IO.File.Copy(TargetPDF, tempFolder2 + "\\target.pdf", true);

            saveData(res, differencesFolderPath + "\\result.html");


            return report;

        }

        public string CompareTwoPDFReport(string SourcePDF, string TargetPDF, List<int> pagesToCompare)
        {
            try
            {
                string FirstFile = string.Empty, SecondFile = string.Empty;

                if (File.Exists(SourcePDF) && File.Exists(TargetPDF))
                {
                    PdfReader reader = new PdfReader(SourcePDF);
                    for (int page = 1; page <= reader.NumberOfPages; page++)
                    {
                        if (pagesToCompare != null && pagesToCompare.Count > 0)
                        {
                            if (pagesToCompare.Contains(page))
                            {
                                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                FirstFile += PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                            }
                        }
                        else
                        {
                            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                            FirstFile += PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                        }

                    }
                    PdfReader reader1 = new PdfReader(TargetPDF);
                    for (int page = 1; page <= reader1.NumberOfPages; page++)
                    {
                        if (pagesToCompare != null && pagesToCompare.Count > 0)
                        {
                            if (pagesToCompare.Contains(page))
                            {
                                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                SecondFile += PdfTextExtractor.GetTextFromPage(reader1, page, strategy);
                            }
                        }
                        else
                        {
                            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                            SecondFile += PdfTextExtractor.GetTextFromPage(reader1, page, strategy);
                        }
                    }
                }
                else
                {
                    //Console.WriteLine("Files does not exist.");
                    return "Files does not exist.";
                }

                List<string> File1diff;
                List<string> File2diff;
                IEnumerable<string> file1 = FirstFile.Trim().Split('\r', '\n');
                IEnumerable<string> file2 = SecondFile.Trim().Split('\r', '\n');
                File1diff = file1.Where(x => x.Trim() != "").ToList();
                File2diff = file2.Where(x => x.Trim() != "").ToList();

                //Console.WriteLine("File 1 and File 2, both are having same number of lines.");

                TextDiff objTextDiff = new TextDiff();
                string res = objTextDiff.CompareText(File1diff, File2diff, SourcePDF, TargetPDF);
                return res;
            }
            catch (Exception ex)
            {
                return "Fail";
            }

        }

        public void saveData(string data, string path)
        {
            StreamWriter stream_writer = new StreamWriter(path);
            stream_writer.Write(data);

            stream_writer.Close();  // Don't forget to close the file!

        }
    }
}
