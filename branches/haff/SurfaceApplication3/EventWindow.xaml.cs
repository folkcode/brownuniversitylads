using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using System.Xml;

namespace SurfaceApplication3
{
    /// <summary>
    /// Interaction logic for SurfaceWindow2.xaml
    /// </summary>
    public partial class EventWindow : SurfaceWindow
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public EventWindow()
        {
            InitializeComponent();
            this.setWindowSize();
            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
            this.load();
        }

        public void setWindowSize()
        {
            Double width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            Double height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            Double ratio = height / width;
            ScaleTransform tran = new ScaleTransform();

            if (width < 1024 || height < 800)
            {
                if (width / 1024 > height / 800)
                {
                    this.Height = height - 100;
                    this.Width = this.Height / 800 * 1024;
                    tran.ScaleY = this.Height / 800;
                    tran.ScaleX = this.Width / 1024;
                }
                else
                {
                    this.Width = width - 100;
                    this.Height = this.Width / 1024 * 800;
                    tran.ScaleX = this.Width / 1024;
                    tran.ScaleY = this.Height / 800;
                }
                //scale according to 1600* 900 resolution
                mainCanvas.RenderTransform = tran;
            }
        }
        
        /// <summary>
        /// load event info
        /// </summary>
        public void load()
        {
            String filepath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath + "EventXML.xml");
            if (doc.HasChildNodes)
            {
                foreach (XmlNode node in doc.ChildNodes)
                {
                    if (node.Name == "events")
                    {
                        foreach (XmlNode inNode in node.ChildNodes)
                        {
                            if (inNode.Name == "event")
                            {
                                EventEntry newEntry = new EventEntry(this);
                                String name = inNode.Attributes.GetNamedItem("name").InnerText;
                                String start = inNode.Attributes.GetNamedItem("start").InnerText;
                                String end = inNode.Attributes.GetNamedItem("end").InnerText;
                                String location = inNode.Attributes.GetNamedItem("location").InnerText;
                                String description = inNode.Attributes.GetNamedItem("description").InnerText;

                                //set image source
                                newEntry.name_tag.Text = name;
                                newEntry.start_tag.Text = start;
                                newEntry.end_tag.Text = end;
                                newEntry.location_tag.Text = location;
                                newEntry.description.Text = description;

                                newEntry.setEventName(name);

                                EntryListBox.Items.Add(newEntry);
                            }
                        }
                    }
                }
            }
        }

        //reloads event info
        public void reload()
        {
            EventWindow newEventWindow = new EventWindow();
            newEventWindow.ShowActivated = true;
            newEventWindow.Show();
            this.Close();
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

        private void addEvent_Click(object sender, RoutedEventArgs e)
        {
            AddEventWindow newWindow = new AddEventWindow(this);
            newWindow.Show();
        }
       
    }
}