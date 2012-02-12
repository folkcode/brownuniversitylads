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
using Microsoft.Surface.Presentation.Controls;
using System.Net.Mail;
using System.IO;
using System.ComponentModel;
using Microsoft.Surface.Presentation;
using System.Windows.Threading;
using System.Net.Mime;
using System.Net;

namespace LADSArtworkMode
{
    /// <summary>
    /// Interaction logic for EmailWindow.xaml
    /// </summary>
    public partial class EmailWindow : UserControl
    {
        private ScatterView _parent;
        private ScatterViewItem _container;
        private System.Drawing.Bitmap _bmp;
        private DockableItem _dockItem;
        private DispatcherTimer _resetTimer;

        public EmailWindow(ScatterView parent, DockableItem dockItem, DispatcherTimer tmr)
        {
            InitializeComponent();

            _parent = parent;
            _container = new ScatterViewItem();
            _container.CanRotate = false;
            _container.Content = this;
            _container.Width = 374;
            _container.Height = 200;
            _container.IsContainerActive = true;
            _container.ContainerStaysActive = true;

            _container.Background = Brushes.Transparent;
            _parent.Items.Add(_container);
            _container.Center = new Point(parent.Width / 2 - 374 / 2, parent.Height / 2 - 250 / 2);
            _dockItem = dockItem;
            _resetTimer = tmr;

            //Keyboard.AddKeyDownHandler(this, KeyDown); // jcchin - not needed for now
            //Keyboard.AddKeyUpHandler(this, KeyUp); // jcchin - not needed for now
            AddressBox.KeyboardPositioning += AddressBox_KeyboardPositioning;
            Body.KeyboardPositioning += AddressBox_KeyboardPositioning;
        }

        /* jcchin - not needed for now */
        /*private void KeyDown(object sender, KeyEventArgs e)
        {
            _resetTimer.Stop();
        }

        private void KeyUp(object sender, KeyEventArgs e)
        {
            _resetTimer.Start();
        }*/

        private void AddressBox_KeyboardPositioning(object sender, KeyboardPositioningEventArgs e)
        {
            e.CenterY = (float)(_parent.Height - e.Height / 2 + 25);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _parent.Items.Remove(_container);
            _parent = null;
            _container = null;
        }

        public string parsetext(string text, bool allow)
        {
            //Create a StringBuilder object from the string intput
            //parameter
            StringBuilder sb = new StringBuilder(text);
            //Replace all double white spaces with a single white space
            //and &nbsp;
            sb.Replace(" ", " &nbsp;");
            //Check if HTML tags are not allowed
            if (!allow)
            {
                //Convert the brackets into HTML equivalents
                sb.Replace("<", "&lt;");
                sb.Replace(">", "&gt;");
                //Convert the double quote
                sb.Replace("\"", "&quot;");
            }
            //Create a StringReader from the processed string of
            //the StringBuilder
            StringReader sr = new StringReader(sb.ToString());
            StringWriter sw = new StringWriter();
            //Loop while next character exists
            while (sr.Peek() > -1)
            {
                //Read a line from the string and store it to a temp
                //variable
                string temp = sr.ReadLine();
                //write the string with the HTML break tag
                //Note here write method writes to a Internal StringBuilder
                //object created automatically
                sw.Write(temp + "<br>");
            }
            //Return the final processed text
            return sw.GetStringBuilder().ToString();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MailMessage mail = new MailMessage();
                if (AddressBox.Text == null)
                    return;
                mail.To.Add(AddressBox.Text);

                //set the content
                mail.Subject = "Wolbach UX Lab - Screenshot";

                //first we create the Plain Text part
                AlternateView plainView = AlternateView.CreateAlternateViewFromString("Wolbach", null, "text/plain");

                /*LinkedResource defaultText = null;
                defaultText = new LinkedResource("C:/Garibaldi/EmailBody.txt");
                defaultText.ContentId = "LibraryText";*/

                // jcchin - need schpeel.txt?
                /*StreamReader streamReader = new StreamReader("C:/Garibaldi/Schpeel.txt");
                string text = streamReader.ReadToEnd();
                streamReader.Close();*/
                string text = ""; // jcchin

                //text = parsetext(text, true); // jcchin - need schpeel.txt

                //String body = text + "<br>" + _infoFrame.Source + "<br>" + _infoFrame.Date + "<br>" + _infoFrame.Author + "<br>" + _infoFrame.Title; // jcchin
                String body = text;

                Console.Out.WriteLine(text);
                if ((bool)BodyCheckBox.IsChecked)
                {
                    body = body + "<br> --Notes-- <br>" + Body.Text;
                }

                //create the LinkedResource (embedded image)
                LinkedResource pic = null;
                AlternateView htmlView = null;
                if (_dockItem.GetImageURIPath == "")
                {
                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)_dockItem.image.Source.Width,
                                                               (int)_dockItem.image.Source.Height,
                                                               100, 100, PixelFormats.Default);
                    renderTargetBitmap.Render(_dockItem.image);
                    JpegBitmapEncoder jpegBitmapEncoder = new JpegBitmapEncoder();
                    jpegBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                    String filePath = "C:/Temp/WolbachUXLab_Screenshot.jpg"; // jcchin - PICK A BETTER LOCATION!!!!!!!!

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create)) 
                    {
                        jpegBitmapEncoder.Save(fileStream);
                        fileStream.Flush();
                        fileStream.Close();
                    }

                    // jcchin - old Garibaldi way of saving image
                    /*System.Drawing.Imaging.EncoderParameters encoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                    encoderParameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                    _bmp.Save("C:/Garibaldi/Temp.jpg", Utils.GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg), encoderParameters);*/

                    /*pic = new LinkedResource("C:/Temp/WolbachGCNav_Email_Temp.jpg");
                    pic.ContentId = "Wolbach UX Lab - Screenshot";
                    htmlView = AlternateView.CreateAlternateViewFromString(body + "<br><img src='cid:Wolbach'>", null, "text/html");
                    htmlView.LinkedResources.Add(pic);*/

                    Attachment data = new Attachment(filePath);
                    mail.Attachments.Add(data);
                    ContentDisposition disposition = data.ContentDisposition;
                    disposition.CreationDate = System.IO.File.GetCreationTime(filePath);
                    disposition.ModificationDate = System.IO.File.GetLastWriteTime(filePath);
                    disposition.ReadDate = System.IO.File.GetLastAccessTime(filePath);
                    disposition.FileName = System.IO.Path.GetFileName(filePath);

                    htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                }
                else
                {
                    //pic = new LinkedResource(_infoFrame.FileName);
                    Attachment data = new Attachment(_dockItem.GetImageURIPath);
                    mail.Attachments.Add(data);
                    htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                }
                //htmlView.LinkedResources.Add(defaultText);

                //add the views
                mail.AlternateViews.Add(plainView);
                mail.AlternateViews.Add(htmlView);

                //send the message
                SmtpClient smtp = new SmtpClient();
                NetworkCredential creds = new System.Net.NetworkCredential("ladsgaribaldi@gmail.com", "browngfx1");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = creds;
                
                smtp.Send(mail);
                Utils.Soon(delegate
                {
                    this.CancelButton_Click(this, null);
                });
            }
            catch (Exception ex)
            {
                //place the box that says email address is not valid here, dunno how to to do it with existing infrastructure
                Warning.Text = "Email address is not valid";
            }
        }

        private void SurfaceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            BodyCanvas.Visibility = Visibility.Visible;
        }

        private void SurfaceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            BodyCanvas.Visibility = Visibility.Hidden;
        }
    }
}
