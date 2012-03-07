using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SurfaceApplication3;

namespace CSVImport
{
    class Program
    {
        static void Main(string[] args)
        {
            // "C:/englishlads-yudi/CSVImport/fake_csv.csv"
            string path = args[0];
            CSVImporter.DoBatchImport(path);       
        }
    }
}
