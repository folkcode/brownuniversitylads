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
using System.Xml;
using System.Windows.Forms;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for EventEntry.xaml
    /// </summary>
    public partial class EventEntry : System.Windows.Controls.UserControl
    {
        public EventWindow _eventWindow;
        private String eventName;

        public EventEntry(EventWindow eventWindow)
        {
            InitializeComponent();
            _eventWindow = eventWindow;
        }

        public void setEventName(String name)
        {
            eventName = name;
        }

        //deletes event
        private void delete_Click(object sender, RoutedEventArgs e)
        {
            DialogResult result = System.Windows.Forms.MessageBox.Show("Are you sure you want to remove this event" + " " + eventName +"?", "Remove the event", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                _eventWindow.EntryListBox.Items.Remove(this);
                //Would need to remove from the xml file as well
                XmlDocument doc = new XmlDocument();
                String dataDir = "Data/";
                doc.Load(dataDir + "EventXML.xml");
                if (doc.HasChildNodes)
                {
                    foreach (XmlNode docNode in doc.ChildNodes)
                    {
                        if (docNode.Name == "events")
                        {

                            foreach (XmlNode node in docNode.ChildNodes)
                            {
                                if (node.Name == "event")
                                {
                                    String name = node.Attributes.GetNamedItem("name").InnerText;
                                    if (eventName == name)
                                    {
                                        docNode.RemoveChild(node);
                                        doc.Save(dataDir + "EventXML.xml");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }
            
        private void edit_Click(object sender, TouchEventArgs e)
        {
            this.editClicked();
        }

        private void edit_Click(object sender, MouseButtonEventArgs e)
        {
            this.editClicked();
        }

        /// <summary>
        /// edit existing events
        /// </summary>
        public void editClicked()
        {
            AddEventWindow newBigWindow = new AddEventWindow(_eventWindow);
            String dataDir = "Data/";
            String dataUri = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
            XmlDocument doc = new XmlDocument();
            doc.Load(dataDir + "EventXML.xml");
            if (doc.HasChildNodes)
            {
                foreach (XmlNode docNode in doc.ChildNodes)
                {
                    if (docNode.Name == "events")
                    {
                        foreach (XmlNode node in docNode.ChildNodes)
                        {
                            if (node.Name == "event")
                            {
                                String name = node.Attributes.GetNamedItem("name").InnerText;
                                if (eventName == name)
                                {
                                    String start = node.Attributes.GetNamedItem("start").InnerText;
                                    String end = node.Attributes.GetNamedItem("end").InnerText;
                                    String location = node.Attributes.GetNamedItem("location").InnerText;
                                    String description = node.Attributes.GetNamedItem("description").InnerText;

                                    newBigWindow.name_tag.Text = name;
                                    newBigWindow.start_tag.Text = start;
                                    newBigWindow.end_tag.Text = end;
                                    newBigWindow.location_tag.Text = location;
                                    newBigWindow.description_tag.Text = description;
                                    newBigWindow.isEditingEvent(true);
                                    newBigWindow.setEventName(name);
                                }
                            }

                        }

                    }
                }
            }
            newBigWindow.Show();
        }
    }
}
