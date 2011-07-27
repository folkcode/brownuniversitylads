using System;
using System.Windows.Controls;
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
            if (_helpers.IsImageFile(filepath))
            {
                dataDir = dataDir1 + "Images\\Metadata\\";
            }
            else if (_helpers.IsVideoFile(filepath))
            {
                dataDir = dataDir1 + "Videos\\Metadata\\";
                int decrement = System.IO.Path.GetExtension(filepath).Length;
                filepath = filepath.Remove(filepath.Length - decrement, decrement);
                filepath += ".bmp";
            }
            _filePath = dataDir + filepath;
            InitializeComponent();
            this.Focusable = true;
            
        }
        public void loadPictures()
        {

            FileStream stream = new FileStream(_filePath, FileMode.Open);
            System.Drawing.Image dImage = System.Drawing.Image.FromStream(stream);
            System.Windows.Controls.Image wpfImage = _helpers.ConvertDrawingImageToWPFImage(dImage);
            image.Source = wpfImage.Source;
            stream.Close();
        }

        private void image_PreviewTouchUp(object sender, EventArgs e)
        {
            _artModeWin.newMediaTimeLine(_filePath, _fileName);
            _artModeWin.hideMetaList();
        }

    }
}
