using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using System.Xml;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for SurfaceWindow2.xaml
    /// </summary>
    public partial class AddEventWindow : SurfaceWindow
    {
        private EventWindow _eventWindow;
        private bool editingExistingEvent;
        private String eventName;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AddEventWindow(EventWindow eventWindow)
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            _eventWindow = eventWindow;
            eventName = "";
        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        public void isEditingEvent(bool isEditing)
        {
            editingExistingEvent = isEditing;
        }

        public void setEventName(String name)
        {
            eventName = name;
        }

        /// <summary>
        /// reloads the event window to reflect the new changes
        /// </summary>
        private void close_Click(object sender, RoutedEventArgs e)
        {
            _eventWindow.reload();
            this.Close();
        }

        /// <summary>
        /// Saves events and writes the information to XML
        /// </summary>
        private void save_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = Colors.Black; //CHANGE TO ACTUAL ORIGINAL COLOR
            name_tag.BorderBrush = brush;
            start_tag.BorderBrush = brush;
            end_tag.BorderBrush = brush;
            location_tag.BorderBrush = brush;
            description_tag.BorderBrush = brush;
            String dataDir = "Data/";
            XmlDocument doc = new XmlDocument();
            doc.Load(dataDir + "EventXML.xml");
            //if editing an existing event, find its location in the xml file in order to change the info.
            if (editingExistingEvent)
            {
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
                                    if (name == eventName)
                                    {
                                        //this is the information that is currently saved (old info)
                                        String start = node.Attributes.GetNamedItem("start").InnerText;
                                        String end = node.Attributes.GetNamedItem("end").InnerText;
                                        String location = node.Attributes.GetNamedItem("location").InnerText;
                                        String description = node.Attributes.GetNamedItem("description").InnerText;

                                        if (name_tag.Text != name)
                                        {
                                            node.Attributes.GetNamedItem("name").InnerText = name_tag.Text;
                                        }
                                        if (start_tag.Text != start)
                                        {
                                            node.Attributes.GetNamedItem("start").InnerText = start_tag.Text;
                                        }
                                        if (end_tag.Text != end)
                                        {
                                            node.Attributes.GetNamedItem("end").InnerText = end_tag.Text;
                                        }
                                        if (location_tag.Text != location)
                                        {
                                            node.Attributes.GetNamedItem("location").InnerText = location_tag.Text;
                                        }
                                        if (description_tag.Text != description)
                                        {
                                            node.Attributes.GetNamedItem("description").InnerText = description_tag.Text;
                                        }

                                    }


                                }

                            }
                        }
                    }

                }
            }
            //If it's a new event (not editing existing one)
            else
            {
                //figure out if it's duplicating an existing event (based on name)
                bool eventNameExists = false;
                if (doc.HasChildNodes)
                {
                    foreach (XmlNode docNode in doc.ChildNodes)
                    {
                        if (docNode.Name == "events")
                        {
                            if (docNode.HasChildNodes)
                            {

                                foreach (XmlNode eventNode in docNode.ChildNodes)
                                {
                                    if (eventNode.Name == "event")
                                    {
                                        String existingEventName = eventNode.Attributes.GetNamedItem("name").InnerText;

                                        if (existingEventName == name_tag.Text)
                                        {
                                            eventNameExists = true;
                                        }
                                    }
                                }
                            }
                            if (!eventNameExists)
                            {
                                XmlElement newEntry = doc.CreateElement("event");
                                docNode.AppendChild(newEntry);
                                newEntry.SetAttribute("name", "" + name_tag.Text);
                                newEntry.SetAttribute("start", "" + start_tag.Text);
                                newEntry.SetAttribute("end", "" + end_tag.Text);
                                newEntry.SetAttribute("location", "" + location_tag.Text);
                                newEntry.SetAttribute("description", "" + description_tag.Text);
                            }
                            else
                            {
                                MessageBox.Show("An event with this name already exists.");
                                return;
                            }
                        }
                    }


                }

            }
            if (name_tag.Text == "" || start_tag.Text == "" || end_tag.Text == "")
            {
                incomplete_information.Content = "Some items are not complete! Event must have a name, start year, and end year.";

                if (name_tag.Text == "")
                {
                    name_tag.BorderBrush = Brushes.DarkRed;
                }
                else
                {
                    name_tag.BorderBrush = Brushes.DarkGreen;
                }
                if (start_tag.Text == "")
                {
                    start_tag.BorderBrush = Brushes.DarkRed;
                }
                else
                {
                    start_tag.BorderBrush = Brushes.DarkGreen;
                }
                if (end_tag.Text == "")
                {
                    end_tag.BorderBrush = Brushes.DarkRed;
                }
                else
                {
                    end_tag.BorderBrush = Brushes.DarkGreen;
                }
                return;
            }
            int startYear = 0;
            int endYear = 0;
            try{
                startYear = Convert.ToInt32(start_tag.Text);
                endYear = Convert.ToInt32(end_tag.Text);
            }
            catch(Exception exc){
                MessageBox.Show("Start and end years must be valid numbers.");
                return;
            }
            if (endYear - startYear <= 0)
            {
                MessageBox.Show("End year must be later than start year.");
                return;
            }
            if (startYear < -9999 || startYear > 9999 || endYear < -9999 || endYear > 9999)
            {
                MessageBox.Show("Years must be between -9999 and 9999.");
                return;
            }
            doc.Save(dataDir + "EventXML.xml");
            _eventWindow.reload();
            this.Close();
        }

        private void summaryMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int cursorPosition = description_tag.SelectionStart;
            int nextSpace = description_tag.Text.IndexOf(' ', cursorPosition);
            int selectionStart = 0;
            string trimmedString = string.Empty;
            if (nextSpace != -1)
            {
                trimmedString = description_tag.Text.Substring(0, nextSpace);
            }
            else
            {
                trimmedString = description_tag.Text;
            }


            if (trimmedString.LastIndexOf(' ') != -1)
            {
                selectionStart = 1 + trimmedString.LastIndexOf(' ');
                trimmedString = trimmedString.Substring(1 + trimmedString.LastIndexOf(' '));
            }

            description_tag.SelectionStart = selectionStart;
            description_tag.SelectionLength = trimmedString.Length;
        }
    }
}