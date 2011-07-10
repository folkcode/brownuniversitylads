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

namespace LADSArtworkMode
{
    /// <summary>
    /// Interaction logic for metaDataEntry.xaml
    /// </summary>
    /// 

    

    public partial class metaDataEntry : UserControl
    {
        public ArtworkModeWindow _artModeWin;
        public String _filePath;
        public String _fileName;
        private Helpers _helpers;

        public metaDataEntry(ArtworkModeWindow artModeWin, String fileName, String filepath)
        {
            _artModeWin = artModeWin;
            String dataDir1 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
            String dataDir = dataDir1 + "Images\\Metadata\\";
            _helpers = new Helpers();
            _fileName = fileName;
            //imageName.Text = _fileName;
            //_fileName = fileName.Substring(0, fileName.Length - 3) + "jpg";
            //_fileName = fileName;
            //_filePath = dataDir + _fileName;
            Console.WriteLine("fileName is : " + fileName + " and filepath is: " + filepath);
            if (_helpers.IsImageFile(filepath))
            {
                Console.WriteLine("IMAGE");
                dataDir = dataDir1 + "Images\\Metadata\\";
            }
            else if (_helpers.IsVideoFile(filepath))
            {
                Console.WriteLine("VIDEO");
                dataDir = dataDir1 + "Videos\\Metadata\\";

                int decrement = System.IO.Path.GetExtension(filepath).Length;
                filepath = filepath.Remove(filepath.Length - decrement, decrement);

                filepath += ".bmp";
            }
            _filePath = dataDir + filepath;
            Console.WriteLine("_filePath is : " + _filePath);
            InitializeComponent();
            this.Focusable = true;
            
            //this.PreviewTouchDown += new EventHandler<TouchEventArgs>(chooseMetaData);
            
        }
        public void loadPictures()
        {

            FileStream stream = new FileStream(_filePath, FileMode.Open);
            System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
            System.Windows.Controls.Image wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
            image.Source = wpfImage.Source;
            stream.Close();
            /*BitmapImage newImage = new BitmapImage();

            newImage.BeginInit();
            newImage.UriSource = new Uri(@filePath);
            newImage.EndInit();
            image.Source = newImage;*/
        }

        private void image_PreviewTouchUp(object sender, EventArgs e)
        {
            _artModeWin.newMediaTimeLine(_filePath, _fileName);
            _artModeWin.hideMetaList();
        }

    }
}
