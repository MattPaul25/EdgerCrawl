using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Configuration;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Collections;

namespace EdgarCrawler
{
    class EdgarParser
    {
        
        public string DownloadLocation { get; set; }
        public string DownloadPrev { get; set; }
        public bool IsSuccessful { get; set; }
        private bool containsForms;
        private string formDString;
        private string formDAString;
        private string edgarHtml;
        private string pattern;
        List<string> duplicateCheck = new List<string>();

        public EdgarParser()
        {
            this.containsForms = true;
            this.IsSuccessful = true;
            this.DownloadLocation = ConfigurationManager.ConnectionStrings["DownloadLocation"].ConnectionString;
            this.DownloadPrev = ConfigurationManager.ConnectionStrings["DownloadPrev"].ConnectionString;
            this.formDString = ConfigurationManager.ConnectionStrings["formDHtml"].ConnectionString;
            this.formDAString = ConfigurationManager.ConnectionStrings["formDaHtml"].ConnectionString;
            this.pattern = @"\d{6,7}\/\d{18}\/";
            DuplicateCheck();
            EdgerCycle();
        }

        private void DuplicateCheck()
        {
            DirectoryInfo d = new DirectoryInfo(DownloadPrev);
            foreach (var fil in d.GetFiles())
	        {
                string FilingIdentifier = fil.Name.Substring(fil.Name.Search("_", 2));
                duplicateCheck.Add(FilingIdentifier);
	        }
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

                    int x = myMatch.Value.Length;
                    string fileName = constructFileName(myMatch.Value);
                    
                    download(xmlUrl, fileName);
                }
                else
                {
                    break;
                }            
            }
        }

        private string constructFileName(string match)
        {
            string fileName = match.Replace("/", "_");
            int startParse =  TextUtils.Search(fileName, "_", 1);
            int endParse = TextUtils.Search(fileName, "_", 2)-1;
            string secondString = fileName.Substring(startParse, endParse - startParse);
            string secondPortion = secondString.Substring(0, 10)
                + "-" + secondString.Substring(10, 2) + "-" + secondString.Substring(12, secondString.Length - 12);

            fileName = fileName + secondPortion + ".xml";
            return fileName;
            
        }

        private void download(string xmlUrl, string fileName)
        {
            if (fileName.Search("_", 2) > -1)
            {
                string FileIdentifier = fileName.Substring(fileName.Search("_", 2));
                if (!duplicateCheck.Contains(FileIdentifier))
                {
                    duplicateCheck.Add(FileIdentifier);
                    try
                    {
                        Console.WriteLine("Downloading: " + fileName);
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(xmlUrl);
                        string newXml = InsertString(xmlDoc, @"<schemaVersion>", @"</schemaVersion>");
                        xmlDoc.InnerXml = newXml;
                        xmlDoc.Save(DownloadLocation + fileName);
                    }
                    catch (Exception x)
                    {
                        TextUtils.Comment(x.Message);
                        IsSuccessful = false;
                    }
                }
                else
                {
                    Console.WriteLine("File already exists: " + fileName);
                }
            }
            else
            {
                Console.WriteLine("Invalid File: " + fileName);
            }
            
        }

        private static string InsertString(XmlDocument xmlDoc, string startString, string endString)
        {
            string someText = xmlDoc.InnerXml;
            int startPlace = TextUtils.Search(someText, startString) - 1;
            int endPlace = TextUtils.Search(someText, endString)-1;
            string schema = someText.Substring(startPlace, endPlace - (startPlace - endString.Length));
            someText = someText.Insert(startPlace, schema);
            return someText;
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
