using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace EdgarCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            TextUtils.Comment("I grab code from edgar's site and find Form Ds and form D/As");
            TextUtils.Comment("I then pull xmls and load them into a file for further execution");

            var prep = new Prepare();            
            TextUtils.Comment("preperation was successful");
            var p = new EdgarParser();               
            var ex = new ExcelInterop();
            var ac = new AccessInterop();
            var prep2 = new Prepare();
        }

    }

    
}
