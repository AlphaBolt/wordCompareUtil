using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using CompareUtility;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;

namespace PDFCompare
{
    [ComVisible(true)]
    class Program
    {
        static void Main1(string[] args)
        {
            bool isUnattended = false;
            if (args.Length > 0)
            {
                isUnattended = Convert.ToBoolean(String.Compare(args[1], "-u", StringComparison.CurrentCultureIgnoreCase));
            }
            Console.WriteLine("-----------------------PDF Compare Process Started------------------------------");
            #region Variables
            string url = string.Empty;
            string username = string.Empty;
            string password = string.Empty;
            string requestString = string.Empty;
            string responseString = string.Empty;
            string currentPolicyPDFPath = string.Empty;
            string currentPolicyPDFPathwithextension = string.Empty;
            string DestinationPDFWithExtension = string.Empty;
            string excelFilePath = string.Empty;
            string excelColumn = string.Empty;
            string ignorecoordinatesfilepath = string.Empty;
            List<ComparisonReport> Report = new List<ComparisonReport>();
            string SourcePDFDir = string.Empty;
            string DestinationPDfDir = string.Empty;
            string sEmailList = string.Empty;
            string sEmailFrom = string.Empty;
            string sSMTPHost = string.Empty;
            string sSMTPPort = string.Empty;
            bool IsSendMail = false;
            string Smtp_Password = string.Empty;
            string Smtp_UserName = string.Empty;
            string ComparisonReportFile = string.Empty;
            string NetworkDrivemap = string.Empty;
            string IdentifierKey = string.Empty;
            #endregion

            #region Variable initilization
            url = ConfigurationManager.AppSettings["url"];
            username = ConfigurationManager.AppSettings["username"];
            password = ConfigurationManager.AppSettings["password"];
            currentPolicyPDFPath = ConfigurationManager.AppSettings["currentPolicyPDFPath"];
            excelColumn = ConfigurationManager.AppSettings["excelColumn"];
            ignorecoordinatesfilepath = Path.Combine(Directory.GetCurrentDirectory(), ConfigurationManager.AppSettings["IgnoreCoordinatesFilePath"]);
            SourcePDFDir = ConfigurationManager.AppSettings["SourcePDFDir"];
            DestinationPDfDir = ConfigurationManager.AppSettings["DestinationPDfDir"];
            sEmailList = ConfigurationManager.AppSettings["MailTo"];
            sEmailFrom = ConfigurationManager.AppSettings["MailFrom"];
            sSMTPHost = ConfigurationManager.AppSettings["Host"];
            sSMTPPort = ConfigurationManager.AppSettings["Port"];
            IsSendMail = Convert.ToBoolean(ConfigurationManager.AppSettings["SendMail"]);
            Smtp_Password = ConfigurationManager.AppSettings["Smtp_Password"];
            Smtp_UserName = ConfigurationManager.AppSettings["Smtp_UserName"];
            ComparisonReportFile = ConfigurationManager.AppSettings["ComparisonReport"].ToString();
            NetworkDrivemap = ConfigurationManager.AppSettings["NetworkDriveMap"].ToString();
            IdentifierKey = ConfigurationManager.AppSettings["IdentifierKey"].ToString();
            List<string> Directories;
            List<string> ExcelReportFiles;
            List<PolicyNumberData> policynumbers;
            #endregion
            try
            {
                Directories = GetDirectoryList();
                Console.WriteLine("Get all Directories------------------------------");
                ExcelReportFiles = GetReportFiles(Directories);
                Console.WriteLine("Get all ExcelReportFiles------------------------------");

                Console.WriteLine("Policy Number Data Fetched Started------------------------------");
                #region Step 1 Policy Numbers
                policynumbers = new List<PolicyNumberData>();
                foreach (string excelpath in ExcelReportFiles)
                {
                    policynumbers.AddRange(Common.getPolicyNumbers(excelpath, excelColumn, IdentifierKey)); ////load all the policy number from excel sheet 
                }
                #endregion
                Console.WriteLine("Policy Number Data Fetched Completed------------------------------");

                Console.WriteLine("Policy Number Fetched Count - " + policynumbers.Count().ToString() + " ------------------------------");

                #region Step 2 Downloald and Compare Pdf

                foreach (PolicyNumberData PolicyNumber in policynumbers)////itreate all policy nomber
                {
                    string policyNum = PolicyNumber.PolicyNumber;
                    string testCaseName = PolicyNumber.test_case_No;
                    DestinationPDFWithExtension = DestinationPDfDir + "\\" + testCaseName + ".pdf";

                    if (File.Exists(DestinationPDFWithExtension))
                    {
                        Console.WriteLine("BaseLine PDF exists for Policy# -" + policyNum + "------------------------------");

                        currentPolicyPDFPath = SourcePDFDir + "\\" + testCaseName;

                        if (!String.IsNullOrEmpty(url))
                        {

                            Console.WriteLine("Download Started PDF for Policy# -" + policyNum + "------------------------------");
                            #region 2.1 Download PDF
                            currentPolicyPDFPathwithextension = Common.CreateFormsPDF(username, password, policyNum, url, currentPolicyPDFPath);
                            currentPolicyPDFPathwithextension = Common.MapNetworkPath(currentPolicyPDFPathwithextension, NetworkDrivemap, SourcePDFDir);
                            #endregion
                            Console.WriteLine("Download Completed PDF for Policy# -" + policyNum + "------------------------------");
                        }
                        Console.WriteLine("Compare Started Source and Destination PDF for Policy# -" + policyNum + "---------");
                        #region 2.2 Comapre Source and Destination PDF
                        //RAJ
                        currentPolicyPDFPathwithextension = "C:\\DownloadPolicyPDF\\pdf1.pdf";
                        //  DestinationPDFWithExtension = "D:\\def\\pdfdownload.pdf";
                        //RAJ
                        Report.Add(Common.ComaprePDF(policyNum, currentPolicyPDFPathwithextension, DestinationPDFWithExtension, ignorecoordinatesfilepath));
                        #endregion
                        Console.WriteLine("Compare Completed Source and Destination PDF for Policy# -" + policyNum + "---------");
                    }
                    else
                    {
                        Console.WriteLine("BaseLine PDF is missing for Policy# -" + policyNum + "------------------------------");
                    }
                }
                #endregion

                if (Report.Count > 0)
                {
                    Console.WriteLine("Creating Comparison Report Started------------------------------");
                    #region Step 3 Creating Comparison Report
                    Common.CreateReport(Report, ComparisonReportFile, policynumbers);
                    #endregion
                    Console.WriteLine("Creating Comparison Report Completed------------------------------");

                    if (IsSendMail)
                    {
                        Console.WriteLine("Sending Mail Started------------------------------");
                        #region Step 4 Sending Mail
                        Common.Sendmail(IsSendMail, sEmailList, sEmailFrom, sSMTPHost, sSMTPPort, Smtp_UserName, Smtp_Password, Global.ComparisonReport);
                        #endregion
                        Console.WriteLine("Sending Mail Completed------------------------------");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw;
            }
            finally
            {
                Console.WriteLine("Thank you");
            }
        }

        private static List<string> GetDirectoryList()
        {
            List<string> Directory = new List<string>();
            var DirectoryList = ConfigurationManager.GetSection("ReportPaths");
            for (int i = 0; i <= (((System.Collections.Specialized.NameValueCollection)(DirectoryList))).AllKeys.Count() - 1; i++)
            {
                Directory.Add((((System.Collections.Specialized.NameValueCollection)(DirectoryList))).Get(i).ToString());
            }
            return Directory;
        }

        private static List<string> GetReportFiles(List<string> Dir)
        {
            List<string> ExcelReportFile = new List<string>();
            string ATExecutionReportWithoutExt = "TestReport";
            if (ConfigurationManager.AppSettings["ATExecReportNameWithoutExt"] != null && ConfigurationManager.AppSettings["ATExecReportNameWithoutExt"] != string.Empty)
            {
                ATExecutionReportWithoutExt = ConfigurationManager.AppSettings["ATExecReportNameWithoutExt"];
            }
            foreach (string str in Dir)
            {
                if (System.IO.Directory.Exists(str))
                {
                    DirectoryInfo info = new DirectoryInfo(str);
                    FileInfo[] files = info.GetFiles(ATExecutionReportWithoutExt + "*.xls").OrderByDescending(p => p.CreationTime).ToArray();
                    if (files.Count() > 0)
                    {
                        if (File.Exists(files[0].FullName))
                        {
                            ExcelReportFile.Add(files[0].FullName);
                        }
                    }
                }
            }
            return ExcelReportFile;
        }

        [STAThread]
        static void Main(string[] args)
        {
            string SourceFilePath = "C:\\Users\\Arvind.1.Kumar\\Desktop\\PDF FILES\\FM 101.0.885 08 18.pdf"; //"Q:\\Automation & Performance\\duckqtp7~Source~2018-08-16 03-23-56src.pdf";
            string TargetFilePath = "C:\\Users\\Arvind.1.Kumar\\Desktop\\PDF FILES\\FM 101.0.885 08 18.pdf"; //"Q:\\Automation & Performance\\duckqtp7~target~2018-08-16 03-23-56targ.pdf";


            var currentDrive = Path.GetPathRoot(System.Reflection.Assembly.GetEntryAssembly().Location);

            var filePath = //"Q:\\Automation & Performance\\Pdf-Demo\\ComparisonReport\\New\\ComparisonReport.xls";
                           Path.Combine(ConfigurationManager.AppSettings["ResultPath"].ToString(), @"ComparisonReport.xls");


            compareForm frm = new compareForm();
            frm.ShowDialog();

            //fnPDFDiff_FormTemplate(SourceFilePath, TargetFilePath, filePath,"", true);
            //fnPDFDiff_FormTemplate(SourceFilePath, TargetFilePath, filePath, "1","1-2", false);
        }

        public static ComparisonReport fnPDFDiff_FormTemplate(string SourceFilePath, string TargetFilePath, string ComparisonReportFile, string sourcePageRange, string targetPageRange, bool boolImageCompare = false)
        {
            string message = string.Empty;

            List<int> sourcePageRangeList = Common.PagesToCompare(sourcePageRange);
            List<int> targetPageRangeList = Common.PagesToCompare(targetPageRange);


            List<ComparisonReport> Report = new List<ComparisonReport>();
            ComparisonReport result=new ComparisonReport();

            try
            {
                if (boolImageCompare)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "IgnoreCoordinatesXML.xml");
                    result = Common.ComaprePDF(SourceFilePath, TargetFilePath, filePath, ComparisonReportFile, sourcePageRangeList, targetPageRangeList);
                    
                        result.Message = result.Result + "|" + result.ComparisonMessage + "|" + Convert.ToString(result.IsPass);
                        Report.Add(result);            
                    
                }
                else
                {
                    //result = Common.ComaprePDFText(SourceFilePath, TargetFilePath, sourcePageRangeList, targetPageRangeList, ComparisonReportFile);
                    string extension1 = Path.GetExtension(SourceFilePath).ToLower();
                    string extension2 = Path.GetExtension(TargetFilePath).ToLower();

                    if (extension1 == ".pdf" && extension2 == ".pdf")
                    {
                        result = Common.ComaprePDFText(SourceFilePath, TargetFilePath, sourcePageRangeList, targetPageRangeList, ComparisonReportFile);
                    }
                    else if ((extension1 == ".docx" || extension1 == ".doc") && (extension2 == ".docx" || extension2 == ".doc"))
                    {
                        result = Common.CompareWordText(SourceFilePath, TargetFilePath, sourcePageRangeList, targetPageRangeList, ComparisonReportFile);
                    }

                    result.Message = result.Result + "|" + result.ComparisonMessage + "|" + Convert.ToString(result.IsPass);
                    Report.Add(result);
                }

                if (Report.Count > 0)
                {
                    if (result.ComparisonMessage != "Please provide source and target same range to compare due to different number of pages.")
                    {
                        //Console.WriteLine("Creating Comparison Report Started------------------------------");
                        #region Step 3 Creating Comparison Report

                        string fileName = Path.GetFileNameWithoutExtension(TargetFilePath);
                        Common.CreateReport(Report, ComparisonReportFile, fileName);

                        #endregion
                        // Console.WriteLine("Creating Comparison Report Completed------------------------------");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                 result.Message= ex.ToString();
                return result;
            }
        }


        public bool? getMessage(ComparisonReport obj)
        {
            return obj.IsPass;
        }

    }
}
