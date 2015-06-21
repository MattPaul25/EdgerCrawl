using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

namespace EdgarCrawler
{
    class EdgarParser
    {
        
        public string DownloadLocation { get; set; }
        public bool IsSuccessful { get; set; }
        private bool containsForms;
        private string formDString;
        private string formDAString;
        private string edgarHtml;
        private string pattern;

        public EdgarParser()
        {
            containsForms = true;
            IsSuccessful = true;
            DownloadLocation = ConfigurationManager.ConnectionStrings["DownloadLocation"].ConnectionString;
            formDString = ConfigurationManager.ConnectionStrings["formDHtml"].ConnectionString;
            formDAString = ConfigurationManager.ConnectionStrings["formDaHtml"].ConnectionString;
            pattern = @"\d{6,7}\/\d{18}\/";
            EdgerCycle();
        }

        private void EdgerCycle()
        {
            int interval = 100;
            int webPageStartNumber = 0;
            while (containsForms)
            {
                string edgarUrl = ConfigurationManager.ConnectionStrings["EdgarSite"].ConnectionString +
                                                                  webPageStartNumber.ToString() + "&count=" + interval.ToString();

                bool isDownloaded = getHtml(edgarUrl);
                if (isDownloaded)
                {

                    Regex myRegex = new Regex(pattern);
                    Match myMatch = myRegex.Match(edgarHtml);
                    if (myMatch.Success)
                    {
                        cycleThroughString(formDString);
                        cycleThroughString(formDAString);
                    }
                    else
                    {
                        containsForms = false;
                    }
                }
                webPageStartNumber += interval;
            }
        }

        private void cycleThroughString(string formString)
        {
            string myUrl = edgarHtml;            
            int startPosition = 0;
            while (true)
            {               
                startPosition = TextUtils.Search(myUrl, formString);
                if (startPosition > 0)
                {
                    myUrl = myUrl.Substring(startPosition + formString.Length);                  
                    Regex myRegex = new Regex(pattern);
                    Match myMatch = myRegex.Match(myUrl);
                    string xmlUrl = "https://www.sec.gov/Archives/edgar/data/" + myMatch.Value + "primary_doc.xml";
                    string fileName = myMatch.Value.Replace("/", "") + ".xml";
                    download(xmlUrl, fileName);
                }
                else
                {
                    break;
                }            
            }
        }

        private void download(string xmlUrl, string fileName)
        {
            if (!File.Exists(DownloadLocation + fileName))
            {
                try
                {
                    Console.WriteLine("Downloading: " + fileName);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlUrl);
                    xmlDoc.Save(DownloadLocation + fileName);
                }
                catch(Exception x)
                {
                    Console.WriteLine(x.Message);
                    IsSuccessful = false;
                }                    
            }
            else
            {
                Console.WriteLine("File already exists: " + fileName);
            }
        }       

        private bool getHtml(string url)
        {
            bool isDownloaded = false;
            using (WebClient client = new WebClient())
            {
                try
                {
                    Console.WriteLine("downloading html from " + url);
                    edgarHtml = client.DownloadString(url);                  
                    isDownloaded = true;
                }
                catch (Exception x)
                {
                    Console.WriteLine("Problem downloading: "  + x.Message);
                    isDownloaded = false;
                    IsSuccessful = false;
                }                
            }
            return isDownloaded;          
        }

    }
}
