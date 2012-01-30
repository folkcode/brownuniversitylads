using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Knowledge_Web
{
    public class WebGroup
    {
        private const int THUMBNAIL_HEIGHT = 50;
        private String[] ALLOWED_GROUPS = { "A", "B", "C", "D" };

        private Dictionary<String, List<Image>> _groups = new Dictionary<String, List<Image>>();
        private Dictionary<String, List<BitmapImage>> _groupsBitmap = new Dictionary<string, List<BitmapImage>>();

        private HashSet<string> _keywords = new HashSet<string>();
        private Image _thumbnail;
        private String _title;
        private String _filename;

        public String Title { get { return _title; } }
        public Image Thumbnail { get { return _thumbnail; } }
        public String Filename { get { return _filename; } }

        public WebGroup(String thumbPath, String title)
        {
            _filename = thumbPath;
            _title = title;
            foreach (String g in ALLOWED_GROUPS)
            {
                _groups.Add(g, new List<Image>());
                _groupsBitmap.Add(g, new List<BitmapImage>());
            }

            BitmapImage thumb = new BitmapImage();
            thumb.BeginInit();
            thumb.DecodePixelHeight = THUMBNAIL_HEIGHT;
            thumb.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/" + @thumbPath, UriKind.Absolute);
            thumb.EndInit();

            _thumbnail = new Image();
            _thumbnail.Source = thumb;
            _thumbnail.Stretch = Stretch.Uniform;
        }

        private void AddToGroup(String group, Image image)
        {
            List<Image> g = null;
            if (_groups.TryGetValue(group, out g))
            {
                g.Add(image);
            }
        }

        public void AddToGroup(String group, String filename)
        {
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            //img.DecodePixelHeight = THUMBNAIL_HEIGHT;
            img.UriSource = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/" + @filename, UriKind.Absolute);
            img.EndInit();
            List<BitmapImage> g = null;
            if(_groupsBitmap.TryGetValue(group, out g)) {
                g.Add(img);
            }

            Image image = new Image();
            image.Source = img;
            image.Stretch = Stretch.Uniform;
            AddToGroup(group, image);
        }

        public List<Image> getGroup(String group)
        {
            List<Image> g = null;
            _groups.TryGetValue(group, out g);
            return g;
        }

        public List<BitmapImage> getGroupBitmap(String group)
        {
            List<BitmapImage> g = null;
            _groupsBitmap.TryGetValue(group, out g);
            return g;
        }

        public void addKeyword(String keyword)
        {
            if (!_keywords.Contains(keyword))
                _keywords.Add(keyword);
        }

        public bool hasAnyKeywordOf(IEnumerable<String> keywords)
        {
            foreach (String k in keywords)
                if (hasKeyword(k))
                    return true;

            return false;
        }
        public bool hasKeyword(String keyword)
        {
            return _keywords.Contains(keyword);
        }

        public IEnumerable<String> Keywords()
        {
            foreach (String s in _keywords)
                yield return s;
        }
    }
}
