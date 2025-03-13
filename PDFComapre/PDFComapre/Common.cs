using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Net;
using CompareUtility;
using System.Xml.Linq;
using System.Net.Mail;
using System.Reflection;
using NPOI.Util;

namespace PDFCompare
{
    public static class Common
    {
        public static string sessionID, policyid, manuScriptID, historyid, formsmanuscriptID;

        public static string CreateFormsPDF(string username, string password, string policynumber, string url, string pdffilepath)
        {
            try
            {
                #region 1 - DCT Request 1
                string LoginrequestXML = "<server><requests><Session.loginRq userName=\"" + username + "\" password=\"" + password + "\" /><OnlineData.loadPolicyRq policyNumber=\"" + policynumber + "\" /><Session.getAllPropertiesRq /></requests></server>";
                string LoginResponseXML = DoPost(LoginrequestXML, url);
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(LoginResponseXML);
                sessionID = xdoc.SelectSingleNode("//Session.loginRs/@sessionID").Value;
                manuScriptID = xdoc.SelectSingleNode("//OnlineData.loadPolicyRs/@manuScriptID").Value;
                historyid = xdoc.SelectSingleNode("//Session.getAllPropertiesRs/property[@name='historyid']/@value").Value;
                policyid = xdoc.SelectSingleNode("//Session.getAllPropertiesRs/property[@name='policyid']/@value").Value;
                #endregion

                #region 2 - DCT Request 2
                string GetFormsManuscriptIDRequestXML = "<server><requests><Session.resumeRq sessionID=\"" + sessionID + "\" /><ManuScript.getValueRq manuscript=\"" + manuScriptID + "\" field=\"PolicyAdminOutputNonShredded.FormsManuScriptID\" /></requests></server>";
                string GetFormsManuscriptIDResponseXML = DoPost(GetFormsManuscriptIDRequestXML, url);
                xdoc.LoadXml(GetFormsManuscriptIDResponseXML);
                formsmanuscriptID = xdoc.SelectSingleNode("//ManuScript.getValueRs/@value").Value;
                #endregion

                #region 3 - DCT Request 3
                string FormsProcessRequestXML = "<server><requests><Session.resumeRq sessionID=\"" + sessionID + "\" /><FormsEngine.initPrintJobRq manuscript=\"" + formsmanuscriptID + "\" printJob=\"_TransactionPrint\" /><FormsEngine.processPrintJobRq onePDF=\"1\" outputFile=\"" + pdffilepath + "\" policyID=\"" + policyid + "\" manuscript=\"" + formsmanuscriptID + "\" printJob=\"_TransactionPrint\" ><diaryMessage /></FormsEngine.processPrintJobRq></requests></server>";
                string FormdProcessResponseXML = DoPost(FormsProcessRequestXML, url);
                xdoc.LoadXml(FormdProcessResponseXML);
                string formprocessstatus = xdoc.SelectSingleNode("//FormsEngine.processPrintJobRs/@status").Value;
                #endregion

                if (formprocessstatus.ToLower() == "success")
                {
                    return pdffilepath + ".pdf";
                }
                else
                    return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string DoPost(string message, string url)
        {
            string receivedResponse = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                byte[] requestInFormOfBytes = System.Text.Encoding.ASCII.GetBytes(message);
                request.Method = "POST";
                request.ContentType = "text/xml;charset=utf-8";
                request.ContentLength = requestInFormOfBytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(requestInFormOfBytes, 0, requestInFormOfBytes.Length);
                requestStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader respStream = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default);
                receivedResponse = respStream.ReadToEnd();
                respStream.Close();
                response.Close();
            }
            catch (Exception)
            {
                throw;
            }
            return receivedResponse;
        }

        public static List<PolicyNumberData> getPolicyNumbers(string excelpath, string ColumnKeyName, string IdentifierKeyName)
        {
            List<PolicyNumberData> lstPolicyNumber;
            try
            {
                lstPolicyNumber = new List<PolicyNumberData>();
                HSSFWorkbook hssfwb;
                using (FileStream file = new FileStream(excelpath, FileMode.Open, FileAccess.Read))
                {
                    hssfwb = new HSSFWorkbook(file);
                }
                for (int j = 0; j < hssfwb.NumberOfSheets; j++)
                {
                    ISheet sheet = hssfwb.GetSheetAt(j);
                    int useColumnKeyNumber = -1;
                    int UseIdentifierKeyname = -1;
                    for (int i = 0; i < sheet.GetRow(0).Cells.Count; i++)
                    {
                        if (sheet.GetRow(0).GetCell(i) != null && sheet.GetRow(0).GetCell(i).StringCellValue == ColumnKeyName)
                        {
                            useColumnKeyNumber = i;
                            break;
                        }
                    }
                    for (int i = 0; i < sheet.GetRow(0).Cells.Count; i++)
                    {
                        if (sheet.GetRow(0).GetCell(i) != null && sheet.GetRow(0).GetCell(i).StringCellValue == IdentifierKeyName)
                        {
                            UseIdentifierKeyname = i;
                            break;
                        }
                    }
                    if (useColumnKeyNumber != -1 && UseIdentifierKeyname != -1)
                    {
                        for (int row = 1; row <= sheet.LastRowNum; row++)
                        {
                            if (sheet.GetRow(row) != null && !string.IsNullOrEmpty(Convert.ToString(sheet.GetRow(row).GetCell(useColumnKeyNumber))) &&
                                !string.IsNullOrEmpty(Convert.ToString(sheet.GetRow(row).GetCell(UseIdentifierKeyname)))
                                && Convert.ToString(sheet.GetRow(row).GetCell(useColumnKeyNumber + 1).StringCellValue).ToLower().Trim() == "success")
                            {
                                PolicyNumberData pldata = new PolicyNumberData();
                                pldata.PolicyNumber = sheet.GetRow(row).GetCell(useColumnKeyNumber).StringCellValue;
                                pldata.test_case_No = sheet.GetRow(row).GetCell(UseIdentifierKeyname).StringCellValue;
                                pldata.SheetName = sheet.SheetName;
                                lstPolicyNumber.Add(pldata);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstPolicyNumber;
        }

        public static string getTestCaseName(string excelpath, string key, int indexofKey, int indexOFValue)
        {
            HSSFWorkbook hssfwb;
            int RowNum = 0;
            string Returnvalue = string.Empty; ;
            using (FileStream file = new FileStream(excelpath, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new HSSFWorkbook(file);
            }
            List<string> lstTemp;
            for (int j = 0; j < hssfwb.NumberOfSheets; j++)
            {
                ISheet sheet = hssfwb.GetSheetAt(j);
                lstTemp = new List<string>();
                for (int row = 1; row <= sheet.LastRowNum; row++)
                {
                    if (sheet.GetRow(row).GetCell(indexofKey).NumericCellValue == Convert.ToInt32(key))
                    {
                        RowNum = row;
                        break;
                    }
                }
                Returnvalue = sheet.GetRow(RowNum).GetCell(indexOFValue).StringCellValue;
            }
            return Returnvalue;
        }

        public static ComparisonReport ComaprePDF(string PolicyNumber, string source, string destination, string ignorecoordinatesfilepath)
        {
            // need to change the access level 
            //PDFUtility.ComparePdf
            try
            {
                PDFComparer pdfcomparer = new PDFComparer();
                XmlDocument ignorecoordinatesxml = new XmlDocument();
                ignorecoordinatesxml.Load(ignorecoordinatesfilepath);
                XmlNode fileXML = ignorecoordinatesxml.SelectSingleNode("//PDFFile[@filepath='" + destination.ToLower().Trim() + "']");
                return pdfcomparer.Compare(PolicyNumber, source, destination, fileXML);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<int> PagesToCompare(string pagenumber)
        {
            List<int> list = new List<int>();

            if (!string.IsNullOrEmpty(pagenumber.Trim()))
            {
                //XmlDocument ignorecoordinatesxml = new XmlDocument();
                //ignorecoordinatesxml.Load(ignorecoordinatesfilepath);
                //string val = ignorecoordinatesxml.SelectSingleNode("PDFFiles/PDFPageCompare/PagesToCompare").InnerText;

                String[] array = pagenumber.Split(',');

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = array[i].Trim();

                    if (array[i].Contains("-"))
                    {
                        String[] values = array[i].Split('-');

                        if (values.Length == 2)
                        {
                            values[0] = values[0].Trim();
                            values[1] = values[1].Trim();

                            int firstNumber = 0;
                            int lastNumber = 0;

                            if (!string.IsNullOrEmpty(values[0]))
                            {
                                firstNumber = Convert.ToInt32(values[0]);
                            }

                            if (!string.IsNullOrEmpty(values[1]))
                            {
                                lastNumber = Convert.ToInt32(values[1]);
                            }

                            if (firstNumber < lastNumber)
                            {
                                for (int number = firstNumber; number <= lastNumber; number++)
                                {
                                    if (number > 0)
                                    {
                                        list.Add(number);
                                    }
                                }
                            }
                            else if (firstNumber > lastNumber)
                            {
                                if (firstNumber > 0)
                                {
                                    list.Add(firstNumber);
                                }
                                if (lastNumber > 0)
                                {
                                    list.Add(lastNumber);
                                }
                            }
                            else if (firstNumber == lastNumber)
                            {
                                if (firstNumber > 0)
                                {
                                    list.Add(firstNumber);
                                }
                            }
                        }
                        else if (values.Length == 1)
                        {
                            values[0] = values[0].Trim();

                            if (values[0] != "" && Convert.ToInt32(values[0]) > 0)
                            {
                                list.Add(Convert.ToInt32(values[0]));
                            }
                        }
                        else if (values.Length > 2)
                        {
                            for (int num = 0; num < values.Length; num++)
                            {
                                values[num] = values[num].Trim();

                                if (values[num] != "" && Convert.ToInt32(values[num]) > 0)
                                {
                                    list.Add(Convert.ToInt32(values[num]));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (array[i] != "" && Convert.ToInt32(array[i]) > 0)
                        {
                            list.Add(Convert.ToInt32(array[i]));
                        }
                    }
                }
            }

            return list;
        }

        public static ComparisonReport ComaprePDF(string source, string destination, string ignorecoordinatesfilepath, string reportResult, List<int> sourcePageRangeList, List<int> targetPageRangeList)
        {
            // need to change the access level 
            //PDFUtility.ComparePdf
            try
            {
                PDFComparer pdfcomparer = new PDFComparer();
                XmlDocument ignorecoordinatesxml = new XmlDocument();
                ignorecoordinatesxml.Load(ignorecoordinatesfilepath);
                XmlNode fileXML = ignorecoordinatesxml.SelectSingleNode("//PDFFile[@filepath='" + destination.ToLower().Trim() + "']");
                return pdfcomparer.Compare(source, destination, fileXML, reportResult, sourcePageRangeList, targetPageRangeList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static ComparisonReport ComaprePDFText(string source, string destination, List<int> sourcePageRange=null, List<int> targetPageRange=null, string reportResult = "")
        {
            // need to change the access level 
            //PDFUtility.ComparePdf
            try
            {
                PDFComparer pdfcomparer = new PDFComparer();
                return pdfcomparer.CompareTwoPDF(source, destination, sourcePageRange, targetPageRange, reportResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ComparisonReport CompareWordText(string source, string destination, List<int> sourcePageRange = null, List<int> targetPageRange = null, string reportResult = "")
        {
            try
            {
                WordComparer wordComparer = new WordComparer();
                return wordComparer.CompareTwoWordDocs(source, destination, sourcePageRange, targetPageRange, reportResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void CreateReport(List<ComparisonReport> Report, string ComparisonReportFile, List<PolicyNumberData> policynumbers)
        {
            try
            {
                HSSFWorkbook hssfwb = CreateReportFile(ComparisonReportFile,"targetFileName");
                ISheet sheet;
                IRow row;
                IRow HeaderRow;
                if (hssfwb != null)
                {
                    sheet = hssfwb.GetSheet("Result");
                    HeaderRow = sheet.CreateRow(0);

                    // Setting the HeaderRow Style 
                    var boldFont = hssfwb.CreateFont();
                    boldFont.FontHeightInPoints = 11;
                    boldFont.FontName = "Calibri";
                    boldFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;

                    HeaderRow.CreateCell(0).SetCellValue("Workflow Name");
                    HeaderRow.GetCell(0).CellStyle = hssfwb.CreateCellStyle();
                    HeaderRow.GetCell(0).CellStyle.SetFont(boldFont);

                    HeaderRow.CreateCell(1).SetCellValue("Test_Case_No");
                    HeaderRow.GetCell(1).CellStyle = hssfwb.CreateCellStyle();
                    HeaderRow.GetCell(1).CellStyle.SetFont(boldFont);

                    HeaderRow.CreateCell(2).SetCellValue("PolicyNumber");
                    HeaderRow.GetCell(2).CellStyle = hssfwb.CreateCellStyle();
                    HeaderRow.GetCell(2).CellStyle.SetFont(boldFont);

                    HeaderRow.CreateCell(3).SetCellValue("Result");
                    HeaderRow.GetCell(3).CellStyle = hssfwb.CreateCellStyle();
                    HeaderRow.GetCell(3).CellStyle.SetFont(boldFont);

                    HeaderRow.CreateCell(4).SetCellValue("ComparisonMessage");
                    HeaderRow.GetCell(4).CellStyle = hssfwb.CreateCellStyle();
                    HeaderRow.GetCell(4).CellStyle.SetFont(boldFont);

                    foreach (ComparisonReport report in Report)
                    {
                        row = sheet.CreateRow(sheet.LastRowNum + 1);
                        row.CreateCell(0).SetCellValue(policynumbers.Where(p => p.PolicyNumber == report.PolicyNumber).Select(s => s.SheetName).FirstOrDefault());
                        row.CreateCell(1).SetCellValue(policynumbers.Where(p => p.PolicyNumber == report.PolicyNumber).Select(s => s.test_case_No).FirstOrDefault());
                        row.CreateCell(2).SetCellValue(report.PolicyNumber);
                        row.CreateCell(3).SetCellValue(report.Result);
                        row.CreateCell(4).SetCellValue(report.ComparisonMessage);

                        var boldFontStatus = hssfwb.CreateFont();
                        if (report.Result == "Failure")
                        {
                            boldFontStatus.Color = IndexedColors.Red.Index;
                            boldFontStatus.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
                        }
                        else
                        {
                            boldFontStatus.Color = IndexedColors.Green.Index;
                        }
                        row.GetCell(3).CellStyle = hssfwb.CreateCellStyle();
                        row.GetCell(3).CellStyle.SetFont(boldFontStatus);
                    }
                    using (FileStream file1 = new FileStream(Global.ComparisonReport, FileMode.Create, FileAccess.Write))
                    {
                        hssfwb.Write(file1);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void CreateTextFile(List<ComparisonReport> Report)
        {
            var currentDrive = Path.GetPathRoot(System.Reflection.Assembly.GetEntryAssembly().Location);
            var filePath = Path.Combine(currentDrive, @"def", @"ComparisonReport");

            bool exists = Directory.Exists(filePath);

            if (!exists)
            {
                System.IO.Directory.CreateDirectory(filePath);
            }

            using (StreamWriter s_w = new StreamWriter(filePath + "\\ComparisonReport" + DateTime.Now.ToString("MM-dd-yyyy-HHmmss") + ".txt"))
            {
                s_w.WriteLine("SOURCE FILE| TARGET FILE| TEXT COMPARE RESULT| IMAGE COMPARE| Comparison Message");

                for (int i = 0; i < Report.Count(); i++)
                {
                    s_w.WriteLine("{0}| {1}| {2}| {3}| {4}",
                                 Report[i].FilePath1.PadRight(5, ' '),
                                 Report[i].FilePath2.PadRight(5, ' '),
                                 Report[i].Result.PadRight(5, ' '),
                                 Convert.ToString(Report[i].ImageCompare).PadRight(5, ' '),
                                 Report[i].ComparisonMessage);
                }
            }
        }

        public static void CreateReport(List<ComparisonReport> Report, string ComparisonReportFile, string fileName)
        {
            try
            {
                HSSFWorkbook hssfwb = CreateReportFile(ComparisonReportFile, fileName);
                //hssfwb.SetSheetName(0,"Comparison Results");
                ISheet sheet;
                IRow row;
                IRow HeaderRow;
                if (hssfwb != null)
                {
                    sheet = hssfwb.GetSheet("Comparison Results");
                    HeaderRow = sheet.CreateRow(0);

                    // Setting the HeaderRow Style 
                    var boldFont = hssfwb.CreateFont();
                    boldFont.FontHeightInPoints = 11;
                    boldFont.FontName = "Calibri";
                    boldFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
                    boldFont.Color = (short)NPOI.SS.UserModel.IndexedColors.Red.Index;




                    HeaderRow.CreateCell(0).SetCellValue("SOURCE FILE");
                    HeaderRow.GetCell(0).CellStyle = hssfwb.CreateCellStyle();
                    HeaderRow.GetCell(0).CellStyle.SetFont(boldFont);
                    HeaderRow.GetCell(0).CellStyle.FillForegroundColor = IndexedColors.Black.Index;
                    HeaderRow.GetCell(0).CellStyle.FillPattern = FillPattern.SolidForeground;


                    HeaderRow.CreateCell(1).SetCellValue("TARGET FILE");
                    HeaderRow.GetCell(1).CellStyle = hssfwb.CreateCellStyle();
                    HeaderRow.GetCell(1).CellStyle.SetFont(boldFont);
                    HeaderRow.GetCell(1).CellStyle.FillForegroundColor = IndexedColors.Black.Index;
                    HeaderRow.GetCell(1).CellStyle.FillPattern = FillPattern.SolidForeground;

                    HeaderRow.CreateCell(2).SetCellValue("TEXT COMPARE RESULT");
                    HeaderRow.GetCell(2).CellStyle = hssfwb.CreateCellStyle();
                    HeaderRow.GetCell(2).CellStyle.SetFont(boldFont);
                    HeaderRow.GetCell(2).CellStyle.FillForegroundColor = IndexedColors.Black.Index;
                    HeaderRow.GetCell(2).CellStyle.FillPattern = FillPattern.SolidForeground;

                    HeaderRow.CreateCell(3).SetCellValue("IMAGE COMPARE RESULT");
                    HeaderRow.GetCell(3).CellStyle = hssfwb.CreateCellStyle();
                    HeaderRow.GetCell(3).CellStyle.SetFont(boldFont);
                    HeaderRow.GetCell(3).CellStyle.FillForegroundColor = IndexedColors.Black.Index;
                    HeaderRow.GetCell(3).CellStyle.FillPattern = FillPattern.SolidForeground;

                    HeaderRow.CreateCell(4).SetCellValue("ALL COMPARE RESULT");
                    HeaderRow.GetCell(4).CellStyle = hssfwb.CreateCellStyle();
                    HeaderRow.GetCell(4).CellStyle.SetFont(boldFont);
                    HeaderRow.GetCell(4).CellStyle.FillForegroundColor = IndexedColors.Black.Index;
                    HeaderRow.GetCell(4).CellStyle.FillPattern = FillPattern.SolidForeground;

                    foreach (ComparisonReport report in Report)
                    {
                        row = sheet.CreateRow(sheet.LastRowNum + 1);
                        row.CreateCell(0).SetCellValue(report.FilePath1);

                        row.CreateCell(1).SetCellValue(report.FilePath2);
                        row.CreateCell(2).SetCellValue(report.ImageCompare ? "N/A" : report.Result);
                        row.CreateCell(3).SetCellValue(report.ImageCompare ? report.Result : "N/A");
                        row.CreateCell(4).SetCellValue("N/A");

                        var boldFontStatus = hssfwb.CreateFont();
                        if (report.Result == "Fail")
                        {
                            boldFontStatus.Color = IndexedColors.Red.Index;
                            //boldFontStatus.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
                        }
                        else
                        {
                            boldFontStatus.Color = IndexedColors.Green.Index;
                        }

                        if (report.ImageCompare)
                        {
                            row.GetCell(3).CellStyle = hssfwb.CreateCellStyle();
                            row.GetCell(3).CellStyle.SetFont(boldFontStatus);
                        }
                        else {
                            row.GetCell(2).CellStyle = hssfwb.CreateCellStyle();
                            row.GetCell(2).CellStyle.SetFont(boldFontStatus);
                        }
                        
                    }
                    using (FileStream file1 = new FileStream(Global.ComparisonReport, FileMode.Create, FileAccess.Write))
                    {
                        hssfwb.Write(file1);
                        //CreateTextFile(Report, file1.Name, file1.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static HSSFWorkbook CreateReportFile(string ComparisonReportFile,string fileName)
        {
            HSSFWorkbook hssfwb;
            var appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            // string dllConfigData = appConfig.AppSettings.Settings["ComparisonReport"].Value;

            if (ComparisonReportFile.Trim() == "")
            {
                ComparisonReportFile = Path.Combine(Directory.GetCurrentDirectory(), @"ComparisonReport\ComparisonReport.xls");
            }

            string filename = fileName; //"Report"; //Path.GetFileNameWithoutExtension(ComparisonReportFile).ToString().Trim();
            string Template = @"ComparisonReport\ComparisonReport.xls";


            string tempfilename = Path.Combine(Path.GetDirectoryName(ComparisonReportFile), filename + "~" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + Path.GetExtension(ComparisonReportFile));

            for (int i = 1; File.Exists(tempfilename); i++)
            {
                tempfilename = Path.Combine(Path.GetDirectoryName(ComparisonReportFile), filename + "~" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "_" + i.ToString() + Path.GetExtension(ComparisonReportFile));
            }

            Global.ComparisonReport = tempfilename;


            if (!(File.Exists(Global.ComparisonReport)))
            {
                try
                {
                    File.Copy(Path.GetFullPath(Template), Global.ComparisonReport, true);
                }
                catch (Exception ex)
                {
                    File.Copy("Q:\\Automation & Performance\\Template\\ComparisonReport.xls", Global.ComparisonReport, true);
                }
            }

            using (FileStream file1 = new FileStream(Global.ComparisonReport, FileMode.Open, FileAccess.ReadWrite))
            {
                hssfwb = new HSSFWorkbook(file1);
            }

            hssfwb.SetSheetName(0, "Comparison Results");

            return hssfwb;
        }



        public static Dictionary<string, string> GetValueFromXML(string xmlString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            Dictionary<string, string> dtXMLVal = new Dictionary<string, string>();
            if (doc.SelectSingleNode("//server/responses/Session.loginRs") != null)
            {
                dtXMLVal.Add("seesionId", doc.SelectSingleNode("//server/responses/Session.loginRs").Attributes["sessionID"].Value);
            }
            if (doc.SelectSingleNode("//server/responses/OnlineData.loadPolicyRs") != null)
            {
                dtXMLVal.Add("manuScriptID", doc.SelectSingleNode("//server/responses/OnlineData.loadPolicyRs").Attributes["manuScriptID"].Value);
            }
            if (doc.SelectSingleNode("//server/responses/Session.getAllPropertiesRs/property[@name='historyid']") != null)
            {
                dtXMLVal.Add("historyid", doc.SelectSingleNode("//server/responses/Session.getAllPropertiesRs/property[@name='historyid']").Attributes["value"].Value);
            }
            if (doc.SelectSingleNode("//server/responses/Session.getAllPropertiesRs/property[@name='policyid']") != null)
            {
                dtXMLVal.Add("policyid", doc.SelectSingleNode("//server/responses/Session.getAllPropertiesRs/property[@name='policyid']").Attributes["value"].Value);
            }

            return dtXMLVal;
        }

        public static void Sendmail(bool IsSendMail, string sEmailList, string sEmailFrom, string sSMTPHost, string sSMTPPort, string Smtp_UserName, string Smtp_Password, string TestReportFile)
        {
            try
            {
                if (IsSendMail)
                {
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(sEmailFrom);
                    string[] sToAddreddArray;
                    NetworkCredential basicCredential;
                    sToAddreddArray = sEmailList.Split(';');
                    foreach (string to in sToAddreddArray)
                    {
                        mail.To.Add(new MailAddress(to));
                    }
                    string subject = "Comparison Report : Date : " + DateTime.Now.ToLongDateString() + " ";
                    mail.Subject = subject;
                    mail.IsBodyHtml = true;
                    mail.Body = "Compare Pdf Report";
                    if (TestReportFile != "")
                    {
                        mail.Attachments.Add(new Attachment(TestReportFile));
                    }
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = sSMTPHost;
                    smtp.Port = Convert.ToInt32(sSMTPPort);
                    if (Smtp_UserName != "" && Smtp_Password != "")
                    {
                        basicCredential = new NetworkCredential(Smtp_UserName, Smtp_Password);
                        smtp.Credentials = basicCredential;
                    }
                    else { smtp.UseDefaultCredentials = true; }
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string MapNetworkPath(string path, string networkdrive, string sourcedir)
        {
            return networkdrive + path.Substring(sourcedir.Length);
        }


    }
    public class PolicyNumberData
    {
        public string PolicyNumber;
        public string test_case_No;
        public string SheetName;
    }



}
