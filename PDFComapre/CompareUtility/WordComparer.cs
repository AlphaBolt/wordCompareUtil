using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using ImageDiff;
using Spire.Doc.Documents;
using Spire.Doc;
using Xceed.Words.NET;
using System.Drawing.Drawing2D;



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
        CompareOptions options;

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
            DeleteTempWordImageFolders(reportFolderpath);

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

        public void saveData(string data, string path)
        {
            StreamWriter stream_writer = new StreamWriter(path);
            stream_writer.Write(data);

            stream_writer.Close();

        }


        //******************* Image Comparison *******************************
        public ComparisonReport Compare(string fileName1, string fileName2, XmlNode fileIgnorePixels, string reportResult, List<int> sourcePageRange = null, List<int> targetPageRange = null)
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

                reportFolderpath = CreateReportFolder(ReportDirectoryPath, ReportDirectoryName);
                //DeleteTempWordImageFolders(reportFolderpath);
                differencesFolderPath = CreateFolder(reportFolderpath, DifferencesDirectoryName);
                TempFolderPath = CreateFolder(reportFolderpath, TemporaryDirectoryName);
                tempFolder1 = CreateFolder(TempFolderPath, "TempDir1");
                tempFolder2 = CreateFolder(TempFolderPath, "TempDir2");

                report.FilePath1 = fileName1;
                report.FilePath2 = fileName2;
                report.ImageCompare = true;
                report.Indicator = true;
                report.Result = "Pass";
                report.ComparisonMessage = "Both PDF are same.";
                report.IsPass = true;

                int pagenum = 0;
                Difference = 0;

                bool numberOfPagesSame = (NumberOfPages(fileName1) == NumberOfPages(fileName2));
                string filename = fileName1;
                string pagesNotSame = string.Empty;

                if (File.Exists(fileName1) && File.Exists(fileName2) && numberOfPagesSame)
                {
                    int numberOfPages = NumberOfPages(fileName1);

                    if (sourcePageRange != null && sourcePageRange.Count > 0 && targetPageRange != null && targetPageRange.Count > 0)
                    {
                        if (sourcePageRange.Count == targetPageRange.Count)
                        {
                            ConvertWordToImages(fileName1, tempFolder1, sourcePageRange);
                            ConvertWordToImages(fileName2, tempFolder2, targetPageRange);

                            for (int index = 0; index < sourcePageRange.Count(); index++)
                            {
                                pagesNotSame = CompareWordPages(differencesFolderPath, tempFolder1, tempFolder2, pagesNotSame, sourcePageRange[index], targetPageRange[index]);
                            }

                            if (!report.Indicator)
                            {
                                pagesNotSame = pagesNotSame.Substring(0, pagesNotSame.LastIndexOf(',')).Trim();
                                report.ComparisonMessage = $"Following Pages are different - {pagesNotSame}";
                                report.IsPass = false;
                            }
                        }
                    }
                    else
                    {
                        sourcePageRange = null;
                        targetPageRange = null;

                        ConvertWordToImages(fileName1, tempFolder1, sourcePageRange);
                        ConvertWordToImages(fileName2, tempFolder2, targetPageRange);

                        for (int i = 1; i <= numberOfPages; i++)
                        {
                            pagesNotSame = CompareWordPages(differencesFolderPath, tempFolder1, tempFolder2, pagesNotSame, i);
                        }

                        if (!report.Indicator)
                        {
                            pagesNotSame = pagesNotSame.Substring(0, pagesNotSame.LastIndexOf(',')).Trim();
                            report.ComparisonMessage = $"Following Pages are different - {pagesNotSame}";
                            report.IsPass = false;
                        }
                    }
                }
                else if (File.Exists(fileName1) && File.Exists(fileName2) && !numberOfPagesSame)
                {
                    if (sourcePageRange != null && sourcePageRange.Count > 0 && targetPageRange != null && targetPageRange.Count > 0)
                    {
                        if (sourcePageRange.Count == targetPageRange.Count)
                        {
                            ConvertWordToImages(fileName1, tempFolder1, sourcePageRange);
                            ConvertWordToImages(fileName2, tempFolder2, targetPageRange);

                            for (int index = 0; index < sourcePageRange.Count; index++)
                            {
                                pagesNotSame = CompareWordPages(differencesFolderPath, tempFolder1, tempFolder2, pagesNotSame, sourcePageRange[index], targetPageRange[index]);
                            }

                            if (!report.Indicator)
                            {
                                pagesNotSame = pagesNotSame.Substring(0, pagesNotSame.LastIndexOf(',')).Trim();
                                report.ComparisonMessage = $"Following Pages are different - {pagesNotSame}";
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
                report.FilePath1 = fileName1;
                report.FilePath2 = fileName2;
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

        private int NumberOfPages(string FilePath)
        {
            Document doc = new Document();
            doc.LoadFromFile(FilePath);
            return doc.PageCount;
        }


        private string CompareWordPages(string differencesFolderPath, string tempFolder1, string tempFolder2, string PagesNotSame, int sourceRange, int? targetRange = null)
        {
            if (targetRange != null && targetRange > 0)
            {
                bool result = ComparePage(tempFolder1 + "\\Page" + sourceRange + ".jpg", tempFolder2 + "\\Page" + targetRange + ".jpg", differencesFolderPath, sourceRange);
                if (result)
                {
                    report.AddPageReport(sourceRange, Difference, "Passed", "Pages are same.", "");
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
                    report.AddPageReport(sourceRange, Difference, "Passed", "Pages are same.", "");
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

        private void ConvertWordToImages(string wordFilePath, string dirPath, List<int> sourcePageRange = null)
        {
            Document doc = new Document();
            doc.LoadFromFile(wordFilePath);

            //Convert the whole document into individual images 
            Image[] images = doc.SaveToImages(ImageType.Bitmap);

            for (int i = 0; i < images.Length; i++)
            {
                WordToImages(images[i], dirPath, i + 1);
            }

        }

        //private void ConvertWordToImages(string wordFilePath, string dirPath, List<int> sourcePageRange = null)
        //{
        //    Document doc = _wordApp.Documents.Open(wordFilePath);
        //    Window window = doc.ActiveWindow;

        //    for (int pageNumber = 1; pageNumber <= doc.ComputeStatistics(WdStatistic.wdStatisticPages); pageNumber++)
        //    {
        //        window.Selection.GoTo(WdGoToItem.wdGoToPage, WdGoToDirection.wdGoToAbsolute, pageNumber);
        //        doc.ActiveWindow.ActivePane.View.Zoom.Percentage = 100; // Adjust zoom as needed
        //        doc.ActiveWindow.Selection.CopyAsPicture();

        //        using (Image img = )
        //        {
        //            if (img != null)
        //            {
        //                WordToImages(img, dirPath, pageNumber);
        //            }
        //        }
        //    }

        //}

        private void WordToImages(Image img, string dirPath, int pageNumber)
        {
            string pageFilePath = Path.Combine(dirPath, $"Page{pageNumber}.jpg");

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
                                System.Drawing.Point p = new System.Drawing.Point(x1, y1);
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


        public Bitmap MakeImageWithoutArea(Bitmap source_bm, System.Drawing.Point point, int width, int height)
        {
            //Copy the image.
            Bitmap bm = new Bitmap(source_bm);

            //Clear the selected area.
            using (Graphics gr = Graphics.FromImage(bm))
            {
                GraphicsPath path = new GraphicsPath();
                path.AddRectangle(new System.Drawing.Rectangle(point, new Size(width, height)));
                gr.SetClip(path);
                gr.Clear(Color.Transparent);
                gr.ResetClip();
            }
            return bm;
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
                    if (Difference > 0)
                    {
                        diff.Save(reportDiffPath + "\\Page" + pageNum + ".jpg", ImageFormat.Jpeg);
                        string CombinedDiff = CombineImages(PagePath1, reportDiffPath + "\\Page" + pageNum + ".jpg", reportDiffPath, pageNum);
                        return false;
                    }
                    else
                        return true;
                }

                source.Dispose();
                destination.Dispose();
            }

            return true;
        }

        private unsafe void IsDifferentImage(Bitmap image1, Bitmap image2)
        {
            if (image1 == null | image2 == null)
                Difference = 0;

            if (image1.Height != image2.Height || image1.Width != image2.Width)
                Difference = 1;

            Bitmap diffImage = image2.Clone() as Bitmap;
            int height = image1.Height < image2.Height ? image1.Height : image2.Height;
            int width = image1.Width < image2.Width ? image1.Width : image2.Width;

            BitmapData data1 = image1.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                                               ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            BitmapData data2 = image2.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                                               ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            BitmapData diffData = diffImage.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
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

        private string CombineImages(string fileName1, string fileName2, string DiferencespPath, int PageNum)
        {
            string[] files = new string[] { fileName1, fileName2 };

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


    }
}
