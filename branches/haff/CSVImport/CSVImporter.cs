using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using SurfaceApplication3;
using LumenWorks.Framework.IO.Csv;
using Microsoft.DeepZoomTools;


namespace CSVImport
{
    public struct asset
    {
        public string path;
        public string uniqueName; // This is not found in the CSV, but is generated after reading.
        public string name;
        public string description;
    }

    public struct artwork
    {
        public string path;
        public string thumbPath;
        public string uniqueName; // This is not found in the CSV, but is generated after reading.
        public string title;
        public int year;
        public string artist;
        public string medium;
        public List<string> keywords;
        public List<asset> assets;
    }

    static class CSVImporter
    {
        public static StreamWriter logFileStreamWriter = null;

        // Constant indices that determine where in the row a given field is.
        public const int IMAGE_PATH_INDEX = 0;
        public const int IMAGE_THUMB_INDEX = 1;
        public const int TITLE_INDEX = 2;
        public const int YEAR_INDEX = 3;
        public const int ARTIST_INDEX = 4;
        public const int MEDIUM_INDEX = 5;
        public const int KEYWORDS_INDEX = 6;

        // Each asset takes up one cell, and all assets are at the end of the row,
        // so indices FIRST_ASSET_INDEX until the end of the array should each describe an asset.
        public const int FIRST_ASSET_INDEX = 7;

        // Given the path of the CSV file, imports it, parses it, adds all artworks and assets to the data repository,
        // and appends to the XML collection.
        // This is the only main API function! Call this!
        public static void DoBatchImport(string path)
        {
            // Parse the CSV.
            // For each artwork, process it (thumbs, etc).  If an exception bubbles up this far, log and kill the artwork.
            // Finally, append to the XML to reflect our successful new artwork additions.
        }

        // Given the path of the input CSV file, create a unique logfile in the same directory.
        public static void initLogFile(string path)
        {
            // Generate the unique path of the logfile.
            string logdir = Path.GetDirectoryName(path);
            string logname = "log_" + Path.GetFileNameWithoutExtension(path) + ".txt";
            string logpath = logdir + "/" + logname;
            if (File.Exists(logpath))
            {
                // If the logfile already exists, make a new randomized name.
                logname = generateUniqueName(logpath, logdir);
                logpath = logdir + "/" + logname;
            }
            // TODO: (TODECIDE?)
            // If this throws an exception, we just don't use the log.
            // Should we crash instead?
            try { logFileStreamWriter = File.CreateText(logpath); }
            catch (Exception e) { }
        }

        // Write a line to the logfile, if it isn't null.
        public static void csvLog(string line) {
            if (logFileStreamWriter != null)
            {
                logFileStreamWriter.WriteLine(line);
            }
        }

        // Parses a CSV file into a list of artwork structs.
        public static List<artwork> parseCSV(string path)
        {
            List<artwork> artworks = new List<artwork>();

            using (CsvReader csv =
                   new CsvReader(new StreamReader(path), true))
            {
                // The CSV exported from excel will generally have rows of identical lengths, even though our rows in the excel
                // document will be of variable length.  Thus there will be a large number of empty cells.
                int fieldCount = csv.FieldCount;
                while (csv.ReadNextRecord())
                {
                    artwork aw = new artwork();

                    // Parse the artwork.
                    aw.path = csv[IMAGE_PATH_INDEX];
                    aw.thumbPath = csv[IMAGE_THUMB_INDEX];
                    aw.title = csv[TITLE_INDEX];
                    try
                    {
                        aw.year = Convert.ToInt32(csv[YEAR_INDEX]);
                    }
                    catch (Exception e)
                    {
                        // If the year isn't a valid number, assign a valid but out of range number.
                        // This will cause an exception when the artwork is validated.
                        aw.year = -200000;
                    }                    
                    aw.artist = csv[ARTIST_INDEX];
                    aw.medium = csv[MEDIUM_INDEX];
                    aw.keywords = parseKeywords(csv[KEYWORDS_INDEX]);
                    aw.assets = new List<asset>();
                    for (int i = FIRST_ASSET_INDEX; i < fieldCount; i++)
                    {
                        if(!String.IsNullOrWhiteSpace(csv[i]))
                            aw.assets.Add(parseAsset(csv[i]));
                    }

                    artworks.Add(aw);
                }
            }
            return artworks;
        }

        // Splits keywords.
        public static List<string> parseKeywords(String field)
        {
            return field.Split(';').ToList();
        }

        // Parses a single asset.
        public static asset parseAsset(String field)
        {
            // First take out the description, because it has the special ''' delimiter.
            // Break the input into:
            // [path and name] [description] [extraneous semicolon]
            string[] descSeparator = new string[] {"'''"};
            string[] tokens1 = field.Split(descSeparator, StringSplitOptions.None);
            // Then split the other two fields.
            string[] tokens2 = tokens1[0].Split(';');
            asset ass = new asset();
            ass.description = tokens1[1];
            ass.path = tokens2[0];
            ass.name = tokens2[1];
            // TODO: Currently assumes that all paths are valid paths in the filesystem.
            // Loading from a url still needs to be implemented.
            return ass;
        }

        // Given an artwork, construct a ready-to-append XmlElement (including assets, etc).
        // This method performs no validation on the artwork, so be sure to validate before calling.
        public static XmlElement XmlElementFromValidArtwork(XmlDocument doc, artwork aw)
        {
            XmlElement el = doc.CreateElement("Image");
            el.SetAttribute("path", "" + aw.uniqueName);
            el.SetAttribute("title", "" + aw.title);
            el.SetAttribute("year", "" + aw.year);
            el.SetAttribute("artist", "" + aw.artist);
            el.SetAttribute("medium", "" + aw.medium);
            // TODO: Do we want this?
            // newEntry.SetAttribute("description", "" + aw.);

            // If there are keywords, add them.
            if (aw.keywords.Count > 0)
            {
                XmlElement keywords = doc.CreateElement("Keywords");
                foreach (string keyword in aw.keywords)
                {
                    XmlElement keyword_val = doc.CreateElement("Keyword");
                    keyword_val.SetAttribute("Value", "" + keyword);
                    keywords.AppendChild(keyword_val);
                }
                el.AppendChild(keywords);
            }

            // If there are metadata assets, add them.
            if (aw.assets.Count > 0)
            {
                XmlElement metadata_element = doc.CreateElement("Metadata");
                XmlElement group_element = doc.CreateElement("Group");
                group_element.SetAttribute("name", "A");
                foreach (asset ass in aw.assets)
                {
                    XmlElement item_element = doc.CreateElement("Item");
                    item_element.SetAttribute("Filename", ass.path);
                    item_element.SetAttribute("Name", ass.name);
                    item_element.SetAttribute("Description", ass.description);
                    // TODO: Implement Type = "Web"
                    item_element.SetAttribute("Type", "Image");
                    group_element.AppendChild(item_element);
                }
                metadata_element.AppendChild(group_element);
                el.AppendChild(metadata_element);
            }
            return el;
        }

        // The exception that is thrown when an artwork read from a CSV file is broken in some way.
        public class InvalidCSVArtworkException : System.ApplicationException
        {
            public InvalidCSVArtworkException() { }
            public InvalidCSVArtworkException(string message) { }
            public InvalidCSVArtworkException(string message, System.Exception inner) { }
            // Constructor needed for serialization 
            // when exception propagates from a remoting server to the client.
            protected InvalidCSVArtworkException(System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context) { }
        }

        // Attempts to validate the artwork, throwing an exception at the first invalid occurence.
        // This doesn't attempt to actually load images for artworks or assets: that check is performed
        // later implicitly, when the DeepZoom images/thumbnails are created.
        // An artwork is invalid if:
        // - Its path is not an existing image file.
        // - Its title is missing.
        // - Its year is not a valid number between -9999 and 9999
        public static void validateArtwork(artwork aw)
        {
            if (!Helpers.staticIsImageFile(aw.path) || !File.Exists(aw.path))
                throw new InvalidCSVArtworkException("Artwork path is not an existing image file.");            
            if (String.IsNullOrWhiteSpace(aw.title))
                throw new InvalidCSVArtworkException("Artwork title missing.");
            if (aw.year < -9999 || aw.year > 9999)
                throw new InvalidCSVArtworkException("Artwork year must be a valid number from -9999 to 9999.");

            for (int i = 0; i < aw.assets.Count; i++)
            {
                asset ass = aw.assets[i];
                try { validateAsset(ass); }
                catch (Exception e)
                {
                    csvLog("Error validating asset at: " + ass.path + ".\nMessage: " + e.Message);
                    // TODO: Remove the asset so it doesn't get added to the XML or have thumbnails made.
                }
            }

            // Also do things that are handled in AddNewImageControl.Browse_Click(), namely:
            // - Generate a unique name for this instance of this file
            aw.uniqueName = generateUniqueName(aw.path, "Data/Images/");
        }

        // Validates an asset and generates a unique name for it.
        // An asset is invalid if: 
        // - Its title is missing.
        // - Its path is not an existing image file.
        public static void validateAsset(asset ass)
        {
            if (String.IsNullOrWhiteSpace(ass.name))
                throw new InvalidCSVArtworkException("Asset name missing.");
            if (String.IsNullOrWhiteSpace(ass.path) || !Helpers.staticIsImageFile(ass.path) || !File.Exists(ass.path))
                throw new InvalidCSVArtworkException("Asset path is not an existing image file.");
            ass.uniqueName = generateUniqueName(ass.path, "Data/Images/Metadata/");
        }

        // Given a file path and a new directory, returns a new file name with no conflicts in the new directory.
        // Parameter newpath should have a trailing slash.
        // Sample Input:  /myRandomImageFolder/myImage.jpg, Data/Images/Metadata/
        //        oOtput: myImage403945345.jpg
        public static string generateUniqueName(string oldpath, string newpath)
        {
            string filenameWithoutExtension = Path.GetFileNameWithoutExtension(oldpath);
            string extension = Path.GetExtension(oldpath);
            Random random = new Random();
            int num = random.Next(0, 100000000);
            string uniqueName = filenameWithoutExtension + num + extension;
            while (File.Exists(newpath + uniqueName))
            {
                num = random.Next(0, 100000000);
                uniqueName = filenameWithoutExtension + num + extension;
            }
            return uniqueName;
        }

        // Creates the DeepZoom images.
        // Should throw an exception if the DeepZoom creation fails (for instance if the image is broken)
        public static void createDeepZoomImages(artwork aw)
        {
            string imagePath = aw.uniqueName;
            string imageName = Path.GetFileName(imagePath);
            string destFolderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\DeepZoom";
            
            ImageCreator ic = new ImageCreator();
            ic.TileFormat = ImageFormat.Jpg;
            ic.TileOverlap = 1;
            ic.TileSize = 256;
            ic.ImageQuality = 0.92;
            ic.UseOptimizations = true;
            Directory.CreateDirectory(destFolderPath + "\\" + imageName);
            string target = destFolderPath + "\\" + imageName + "\\dz.xml";
            ic.Create(imagePath, target);
            ic = null;
            System.GC.Collect();
        }

        // Saves both thumbnails for an artwork.
        public static void saveThumbs(artwork aw)
        {
            string thumbPath = "data/Images/Thumbnail/" + aw.uniqueName;
            string imgPath = "data/Images/" + aw.uniqueName;
            if (aw.path.Equals(thumbPath))
            {
                throw new InvalidCSVArtworkException("Artwork thumbnails already exist.");
            }
            else
            {
                try
                {
                    File.Delete(thumbPath);
                    File.Delete(imgPath);
                    // TODO: Get thumbnail from thumbnail image instead, if it exists!
                    System.Drawing.Image img = Helpers.getThumbnail(aw.path, 800);
                    img.Save(thumbPath);
                    img.Save(imgPath);
                    img.Dispose();
                }
                catch (Exception e) {}
            }
        }

        // Assets written to XML must be copied to the Metadata folder, and a thumbnail is produced.
        public static void copyAsset(asset ass)
        {
            if (Helpers.staticIsImageFile(ass.path))
            {
                String newPath = "Data/Images/Metadata/" + ass.uniqueName;
                try
                {
                    // First, copy the asset.
                    File.Delete(newPath);
                    File.Copy(ass.path, newPath);

                    // Then create a thumbnail.
                    System.Drawing.Image thumb = System.Drawing.Image.FromFile(ass.path);
                    thumb = thumb.GetThumbnailImage(128, 128, null, new IntPtr());
                    thumb.Save(newPath + "Thumbnail/" + ass.uniqueName);
                    thumb.Dispose();
                }
                catch (Exception e) {
                    throw new InvalidCSVArtworkException("Error copying image asset at " + ass.path);
                }
            }
            else if (Helpers.staticIsVideoFile(ass.path))
            {
                String newPath = "Data/Videos/Metadata/" + Path.GetFileNameWithoutExtension(ass.uniqueName) + ".bmp";
                try
                {
                    // First, copy the asset.
                    File.Delete(newPath);
                    File.Copy(ass.path, newPath);

                    // Then create a thumbnail, using the thumbnail image of the video.
                    string bmpName = Path.GetFileNameWithoutExtension(ass.path) + ".bmp";
                    System.Drawing.Image thumb = System.Drawing.Image.FromFile(bmpName);
                    thumb.Save(newPath);
                    thumb.Dispose();
                }
                catch (Exception e) {
                    throw new InvalidCSVArtworkException("Error copying video asset at " + ass.path);
                }
            }
        }

    }
}
