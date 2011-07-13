using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using Microsoft.Surface.Presentation.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace LADSArtworkMode
{
    /// <summary>
    /// Interaction logic for metadata_lists.xaml
    /// </summary>
    public partial class metadata_lists : UserControl
    {

        ArtworkModeWindow _artModeWin;
        //public TourSystem tourSystem { get; set;}

        //not edited for videos
        public metadata_lists(ArtworkModeWindow artModeWin, string filename)
        {
            _artModeWin = artModeWin;
            InitializeComponent();
            Console.WriteLine("Filename = " + filename);
            this.loadAssets(filename);
        }
        public void loadAssets(string filename)
        {
            Console.Out.WriteLine("called loading");
            String dataDir1 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
            String dataDir = dataDir1 + "Images\\Metadata\\Thumbnail\\";

            XmlDocument doc = new XmlDocument();
            doc.Load("data/NewCollection.xml");
            if (doc.HasChildNodes)
            {
                foreach (XmlNode docNode in doc.ChildNodes)
                {
                    if (docNode.Name == "Collection")
                    {
                        foreach (XmlNode node in docNode.ChildNodes)
                        {
                            if (node.Name == "Image")
                            {
                                if (filename != node.Attributes.GetNamedItem("path").InnerText)
                                    continue;
                                Console.WriteLine("Here!");
                                foreach (XmlNode imgnode in node.ChildNodes)
                                {
                                    if (imgnode.Name == "Metadata")
                                    {
                                        foreach (XmlNode group in imgnode.ChildNodes)
                                        {
                                            foreach (XmlNode file in group.ChildNodes)
                                            {
                                                if (file.Attributes.GetNamedItem("Type").InnerText != "Video")
                                                {
                                                    string metadatafilename = file.Attributes.GetNamedItem("Filename").InnerText;
                                                    string name;
                                                    try
                                                    {
                                                        name = file.Attributes.GetNamedItem("Name").InnerText;
                                                    }
                                                    catch (Exception exc)
                                                    {
                                                        name = "Untitled";
                                                    }
                                                    Console.WriteLine("Metadata called " + name);
                                                    metaDataEntry newEntry = new metaDataEntry(_artModeWin, name, metadatafilename);

                                                    newEntry.imageName.Text = name;
                                                    //Console.Out.WriteLine(fileName.Length);
                                                    //Console.Out.WriteLine(file);

                                                    newEntry.loadPictures();


                                                    assetsList.Items.Add(newEntry);
                                                }
                                            }

                                        }
                                    }

                                }

                            }
                        }
                    }
                }
            }






            /*String[] files = Directory.GetFiles(dataDir);
            foreach (String file in files) {

                String[] fileName = Regex.Split(file, "Thumbnail");
                String subString = fileName[fileName.Length - 1].Substring(1, fileName[fileName.Length - 1].Length - 1);
                metaDataEntry newEntry = new metaDataEntry(_artModeWin, subString);
                
                newEntry.imageName.Text = subString;
                Console.Out.WriteLine(fileName.Length);
                Console.Out.WriteLine(file);
                
                newEntry.loadPictures(file);

                
                assetsList.Items.Add(newEntry);
            }*/

        }

        public void closeClick(object sender, EventArgs e)
        {
            _artModeWin.hideMetaList();
        }
    }
}
