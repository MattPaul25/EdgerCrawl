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
        public string edgarHtml { get; set; }
        public string DownloadLocation { get; set; }
        public EdgarParser(string webUrl)
        {
            GetHTML(webUrl);
            DownloadLocation = ConfigurationManager.ConnectionStrings["DownloadLocation"].ConnectionString;
            string formDString = ConfigurationManager.ConnectionStrings["formDHtml"].ConnectionString;
            cycleThroughString(formDString);
            string formDAString = ConfigurationManager.ConnectionStrings["formDaHtml"].ConnectionString;
            cycleThroughString(formDAString);
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
                    string pattern = @"\d{6,7}\/\d{18}\/";
                    Regex myRegex = new Regex(pattern);
                    Match myMatch = myRegex.Match(myUrl);
                    string xmlUrl = "https://www.sec.gov/Archives/edgar/data/" + myMatch.Value + "primary_doc.xml";
                    string fileName = myMatch.Value.Replace("/", "") + ".xml";
                    Download(xmlUrl, fileName);
                }
                else
                {
                    break;
                }
            
            }
        }
        private void Download(string xmlUrl, string fileName)
        {
            if (!File.Exists(DownloadLocation + fileName))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlUrl);
                xmlDoc.Save(DownloadLocation + fileName);
            }
        }       
        private void GetHTML(string url)
        {
            using (WebClient client = new WebClient())
            {
                edgarHtml = client.DownloadString(url);
                
            }            
          
        }   
    }
}
