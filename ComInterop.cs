using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Access = Microsoft.Office.Interop.Access;
using System.Configuration;
using System.Runtime.InteropServices;

namespace EdgarCrawler
{
    class AccessInterop
    {
        private string mMcrName;
        private string mFilePath;
        private int attemptNum;
        public bool isSuccessful { get; set; }

        public AccessInterop()
        {
            isSuccessful = true;
            this.mFilePath = ConfigurationManager.ConnectionStrings["AccessLcation"].ConnectionString; ; ;
            this.mMcrName = ConfigurationManager.ConnectionStrings["AccessMacro"].ConnectionString; ; ;
            attemptNum = 1;
            RunMacro();
        }
        private void RunMacro()
        {
            Console.WriteLine("opening access db, running macro " + mMcrName);
            Access.Application oAccess = new Access.Application();
            oAccess.Visible = false;
            oAccess.OpenCurrentDatabase(mFilePath, false);
            try
            {
                oAccess.Run(mMcrName);
            }
            catch (Exception x)
            {
                System.Threading.Thread.Sleep(10000);
                oAccess.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oAccess);
                oAccess = null;
                if (attemptNum < 3)
                {
                    attemptNum++;
                    TextUtils.Comment("attempting time " + attemptNum);
                    RunMacro();
                }
                TextUtils.Comment("something went wrong with the access macro: \n" + x.Message);
                isSuccessful = false;
            }
            finally
            {
                oAccess.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oAccess);
                oAccess = null;
            }
        }
    }
    class ExcelInterop
    {
        private string mMcrName;
        private string mFilePath;
        public bool isSuccessful { get; set; }

        public ExcelInterop()
        {
            this.isSuccessful = true;
            this.mFilePath = ConfigurationManager.ConnectionStrings["excelLocation"].ConnectionString; ;
            this.mMcrName = ConfigurationManager.ConnectionStrings["excelMacroName"].ConnectionString; ; ;
            RunMacro();
        }
        private void RunMacro()
        {
            TextUtils.Comment("opening excel file, running macro " + mMcrName);
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlwb;
            try
            {
                xlwb = xlApp.Workbooks.Open(mFilePath);
                xlApp.Visible = false;
                xlApp.Run(mMcrName);
                xlApp.Quit();
                TextUtils.Comment("removing excel objects from memory");
                removeObject(xlApp);
                removeObject(xlwb);
            }
            catch (COMException cx)
            {
                TextUtils.Comment("there is an issue with the excel macro " + cx.Message);
                isSuccessful = false;
            }
            catch (Exception x)
            {
                TextUtils.Comment("something went wrong with the excel macro: \n" + x.Message);
                isSuccessful = false;
            }

        }

        private void removeObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception x)
            {
                TextUtils.Comment(x.Message);
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}

