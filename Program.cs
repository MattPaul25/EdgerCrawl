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
            Console.WriteLine("I grab code from edgar's site and find Form Ds and form D/As");
            Console.WriteLine("I then pull xmls and load them into a file for further execution");
            var p = new EdgarParser();
        }
    }

    
}
