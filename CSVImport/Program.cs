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

            //List<artwork> artworks = CSVImporter.parseCSV(path);
            /*Debug.WriteLine("Printing artworks:");
            foreach (artwork aw in artworks)
            {
                Debug.WriteLine("ARTWORK");
                Debug.WriteLine(aw.title);
                Debug.WriteLine(aw.medium);
                Debug.WriteLine(aw.path);
                Debug.WriteLine(aw.thumbPath);
                Debug.WriteLine(aw.year);
                Debug.Write("KEYWORDS: ");
                foreach (string k in aw.keywords) Debug.Write(k + " ");
                foreach (asset ass in aw.assets)
                {
                    Debug.WriteLine("ASSET");
                    Debug.WriteLine(ass.name);
                    Debug.WriteLine(ass.path);
                    Debug.WriteLine(ass.description);
                }
            }*/
            

        }
    }
}
