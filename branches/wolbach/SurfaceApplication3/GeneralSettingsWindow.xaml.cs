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
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using System.Xml;

namespace SurfaceApplication3
{
    
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class GeneralSettingsWindow: SurfaceWindow
    {
        private XmlDocument doc;


        public GeneralSettingsWindow()
        {
            InitializeComponent();
            doc = new XmlDocument();
            this.loadCurrentSettings();

        }


        
      
        public void loadCurrentSettings() {
            
            doc.Load("data/NewCollection.xml");
            if (doc.HasChildNodes)
            {
                foreach (XmlNode docNode in doc.ChildNodes)
                {
                    if (docNode.Name == "Collection")
                    {
                        foreach (XmlNode node in docNode.ChildNodes)
                        {
                            if (node.Name == "Email")
                            {
                                if (node.Attributes.GetNamedItem("address") != null)
                                    address_tag.Text = node.Attributes.GetNamedItem("address").InnerText;

                                if (node.Attributes.GetNamedItem("password") != null)
                                    password_tag.Text = node.Attributes.GetNamedItem("password").InnerText;

                                if (node.Attributes.GetNamedItem("host") != null)
                                    host_tag.Text = node.Attributes.GetNamedItem("host").InnerText;

                                if (node.Attributes.GetNamedItem("port") != null)
                                    port_tag.Text = node.Attributes.GetNamedItem("port").InnerText;

                                if (node.Attributes.GetNamedItem("copyright") != null)
                                    copyright_tag.Text = node.Attributes.GetNamedItem("copyright").InnerText;

                                if (node.Attributes.GetNamedItem("logaddress") != null)
                                    logaddress_tag.Text = node.Attributes.GetNamedItem("logaddress").InnerText;

                                if (node.Attributes.GetNamedItem("returnaddress") != null)
                                    returnaddress_tag.Text = node.Attributes.GetNamedItem("returnaddress").InnerText;

                                if (node.Attributes.GetNamedItem("location") != null)
                                    location_tag.Text = node.Attributes.GetNamedItem("location").InnerText;
                            }
                        }
                    }
                }
            }

        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            bool hasEmail = false;
            if (doc.HasChildNodes)
            {
                foreach (XmlNode docNode in doc.ChildNodes)
                {
                    if (docNode.Name == "Collection")
                    {

                        foreach (XmlNode node in docNode.ChildNodes)
                        {
                            if (node.Name == "Email")
                            {
                                docNode.RemoveChild(node);
                            }
                        }
                        //hasEmail = true;

                        //if (node.Attributes.GetNamedItem("address") != null)
                        //    node.Attributes.GetNamedItem("address").InnerText = address_tag.Text;

                        //if (node.Attributes.GetNamedItem("password") != null)
                        //    node.Attributes.GetNamedItem("password").InnerText = password_tag.Text;

                        //if (node.Attributes.GetNamedItem("host") != null)
                        //     node.Attributes.GetNamedItem("host").InnerText = host_tag.Text;

                        //if (node.Attributes.GetNamedItem("port") != null)
                        //    node.Attributes.GetNamedItem("port").InnerText = port_tag.Text;

                        //if (node.Attributes.GetNamedItem("copyright") != null)
                        //    node.Attributes.GetNamedItem("copyright").InnerText = copyright_tag.Text;

                        //if (node.Attributes.GetNamedItem("logaddress") != null) 
                        //node.Attributes.GetNamedItem("logaddress").InnerText = logaddress_tag.Text;


                        //}
                        //if (hasEmail == false)
                        //{
                        XmlElement emailElement = doc.CreateElement("Email");
                        docNode.AppendChild(emailElement);
                        emailElement.SetAttribute("address", "" + address_tag.Text);
                        emailElement.SetAttribute("password", "" + password_tag.Text);
                        emailElement.SetAttribute("host", "" + host_tag.Text);
                        emailElement.SetAttribute("port", "" + port_tag.Text);
                        emailElement.SetAttribute("copyright", "" + copyright_tag.Text);
                        emailElement.SetAttribute("logaddress", "" + logaddress_tag.Text);
                        emailElement.SetAttribute("returnaddress", "" + returnaddress_tag.Text);
                        emailElement.SetAttribute("location", "" + location_tag.Text);
                        //}
                    }
                    //}
                }
                String dataDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
                doc.Save(dataDir + "NewCollection.xml");
            }
        }



        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    

}
