using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace EdgarCrawler
{
    class Prepare
    {
       public bool IsSuccessful { get; set; }
       private string myDirectory;
       private string oldDirectory;

        public Prepare()
        {

            this.IsSuccessful = true;
            this.oldDirectory = ConfigurationManager.ConnectionStrings["DownloadPrev"].ConnectionString;
            this.myDirectory = ConfigurationManager.ConnectionStrings["DownloadLocation"].ConnectionString;;
            moveFiles();            
        }

        private void moveFiles()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(myDirectory);
                foreach (FileInfo file in dir.GetFiles())
                {
                    file.MoveTo(oldDirectory + file.Name);
                }
            }
            catch (Exception x)
            {
                TextUtils.Comment("Problem in prepare class: " + x.Message);
                this.IsSuccessful = false;
            }

        }



        
    }
}
