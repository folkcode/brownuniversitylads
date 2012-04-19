using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using LumenWorks.Framework.IO.Csv;
using Microsoft.DeepZoomTools;

namespace CSVImport
{
    public interface CSVMsgReceiver
    {
        void outputMessage(String msg);
    }

    public class asset
    {
        public string path;
        public string uniqueName; // This is not found in the CSV, but is generated after reading.
        public string name;
        public string description;
    }

    public class artwork
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
        public List<asset> validatedAssets;
    }

    public static class CSVImporter
    {
        public static StreamWriter logFileStreamWriter = null;
        public static string inputCSVPath = null;

        // Constant indices that determine where in the row a given field is.
        public const int IMAGE_PATH_INDEX = 0;
        public const int IMAGE_THUMB_INDEX = 1;
        public const int TITLE_INDEX = 2;
        public const int YEAR_INDEX = 3;
        public const int ARTIST_INDEX = 4;
        public const int MEDIUM_INDEX = 5;
        public const int KEYWORDS_INDEX = 6;

        public static CSVMsgReceiver importer_window = null;
        public static void setOutputWindow(CSVMsgReceiver win)
        {
            importer_window = win;
        }


        // Each asset takes up one cell, and all assets are at the end of the row,
        // so indices FIRST_ASSET_INDEX until the end of the array should each describe an asset.
        public const int FIRST_ASSET_INDEX = 7;

        // Given the path of the CSV file, imports it, parses it, adds all artworks and assets to the data repository,
        // and appends to the XML collection.
        // This is the only main API function! Call this!
        public static void DoBatchImport(string path)
        {
            // Initialize the logfile.
            string logpath = initLogFile(path);

            // Record the absolute path to the CSV file (since all relative paths are relative to the CSV file).
            inputCSVPath = path;
            if (!Path.IsPathRooted(path))
            {
                // If CSV path is relative, it's relative to the current directory.
                inputCSVPath = Directory.GetCurrentDirectory() + "\\" + path;
            }

            // Parse the CSV.
            csvLog("Parsing CSV");
            List<artwork> artworks = null;
            try
            {
                artworks = parseCSV(path);
            }
            catch (Exception e)
            {
                csvLog("Error parsing CSV.  Aborting.");
                csvLog("  Message: " + e.Message + Environment.NewLine);
                return;
            }
            List<artwork> validArtworks = new List<artwork>();

            csvLog("Processing " + artworks.Count + " artworks from " + path);

            // For each artwork, process it (thumbs, etc).  If an exception bubbles up this far, log and kill the artwork.
            for (int i = 0; i < artworks.Count; i++)
            {
                csvMsg("Processing artwork: " + i);
                try
                {
                    artwork aw = artworks[i];
                    validateArtwork(aw);
                    saveThumbs(aw);
                    createDeepZoomImages(aw);

                    // Process the assets.
                    List<asset> validAssets = new List<asset>();
                    for (int asset_idx = 0; asset_idx < aw.assets.Count; asset_idx++)
                    {
                        try
                        {
                            asset ass = aw.assets[asset_idx];
                            validateAsset(ass);
                            copyAsset(ass);
                            validAssets.Add(ass);
                        }
                        catch (Exception e)
                        {
                            csvLog("Error processing asset at: " + aw.assets[asset_idx].path + ". Skipping."
                                   + Environment.NewLine
                                   + "  Message:" + e.Message + Environment.NewLine);
                        }
                    }
                    // Initialize artwork's validated assets.
                    aw.validatedAssets = validAssets;

                    // If we made it, then we can add the artwork to the XML.
                    validArtworks.Add(aw);
                }
                catch (Exception e)
                {
                    csvLog("Error processing artwork at: " + artworks[i].path + ". Skipping."
                           + Environment.NewLine
                           + "  Message: " + e.Message + Environment.NewLine);
                }
            }

            csvMsg("Generating XML");


            XmlDocument doc = new XmlDocument();
            String dataDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
            doc.Load(dataDir + "NewCollection.xml");
            foreach (XmlNode collection_node in doc.ChildNodes)
            {
                if (collection_node.Name.Equals("Collection"))
                {
                    // Finally, append to the XML to reflect our successful new artwork additions.
                    for (int i = 0; i < validArtworks.Count; i++)
                    {
                        XmlElement image_element = XmlElementFromValidArtwork(doc, validArtworks[i]);
                        collection_node.AppendChild(image_element);
                    }
                }
            }
            doc.Save(dataDir + "NewCollection.xml");

            csvLog("All done!");
            csvMsg("Logfile written to " + logpath);
            logFileStreamWriter.Close();
        }

        // Given the path of the input CSV file, create a unique logfile in the same directory.
        // Returns the path of this new file.
        public static String initLogFile(string path)
        {
            // Generate the unique path of the logfile.
            string logdir = Path.GetDirectoryName(path);
            string logname = "log_" + Path.GetFileNameWithoutExtension(path) + ".txt";
            string logpath = logdir + "/" + logname;
            if (File.Exists(logpath))
            {
                // If the logfile already exists, make a new name.
                string filenameWithoutExtension = Path.GetFileNameWithoutExtension(logpath);
                string extension = Path.GetExtension(logpath);
                Random random = new Random();
                int num = 1;
                string uniqueName = filenameWithoutExtension + num + extension;
                while (File.Exists(logdir + "/" + uniqueName))
                {
                    num++;
                    uniqueName = filenameWithoutExtension + num + extension;
                }
                logpath = logdir + "/" + uniqueName;
            }
            // TODO: (TODECIDE?)
            // If this throws an exception, we just don't use the log.
            // Should we crash instead?
            try { logFileStreamWriter = File.CreateText(logpath); }
            catch (Exception e) { csvMsg("Error making log file.  We're in for a bumpy ride!"); }
            return logpath;
        }

        // Write a line to the logfile, if it isn't null.
        public static void csvLog(string line)
        {
            csvMsg(line);
            if (logFileStreamWriter != null)
            {
                logFileStreamWriter.WriteLine(line);
                logFileStreamWriter.Flush();
            }
        }

        // Shows a message in the interactive session.
        // All log mesages are written to the interactive session.
        // Some superfluous messages are written to the interactive session but NOT to the log.
        public static void csvMsg(string line)
        {
            if (importer_window != null)
            {
                importer_window.outputMessage(line + "\n");
            }
            else
            {
                Console.WriteLine(line);
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
                    fieldCount = csv.FieldCount;
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
                        throw new InvalidCSVArtworkException("Year for artwork at " + aw.path + " is not a number.");
                    }
                    aw.artist = csv[ARTIST_INDEX];
                    aw.medium = csv[MEDIUM_INDEX];
                    aw.keywords = parseKeywords(csv[KEYWORDS_INDEX]);
                    aw.assets = new List<asset>();
                    for (int i = FIRST_ASSET_INDEX; i < fieldCount; i++)
                    {
                        try
                        {
                            if (!String.IsNullOrWhiteSpace(csv[i]))
                                aw.assets.Add(parseAsset(csv[i]));
                        }
                        catch (Exception e)
                        {
                            throw new InvalidCSVArtworkException("Error parsing asset " + (i - FIRST_ASSET_INDEX + 1) + " for artwork at " + aw.path + "."
                                                                 + Environment.NewLine + "  Message: " + e.Message);
                        }
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
            string[] descSeparator = new string[] { "'''" };
            string[] tokens1 = field.Split(descSeparator, StringSplitOptions.None);
            if (tokens1.Length < 3) throw new InvalidCSVArtworkException("Unable to parse description.  Did you include all required fields?");
            // Then split the other two fields.
            string[] tokens2 = tokens1[0].Split(';');

            if (tokens2.Length < 2) throw new InvalidCSVArtworkException("Unable to parse path or title.  Did you include all required fields?");
            asset ass = new asset();
            ass.description = tokens1[1];
            ass.path = tokens2[0];
            ass.name = tokens2[1];
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
            if (aw.validatedAssets.Count > 0)
            {
                XmlElement metadata_element = doc.CreateElement("Metadata");
                XmlElement group_element = doc.CreateElement("Group");
                group_element.SetAttribute("name", "A");
                foreach (asset ass in aw.validatedAssets)
                {
                    XmlElement item_element = doc.CreateElement("Item");
                    item_element.SetAttribute("Filename", ass.uniqueName);
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
            public InvalidCSVArtworkException() : base() { }
            public InvalidCSVArtworkException(string message) : base(message) { }
            public InvalidCSVArtworkException(string message, System.Exception inner) : base(message, inner) { }
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
            // Convert the image path to an absolute path to eliminate ambiguities.
            // If the path is relative, it should be relative to the CSV directory.
            if (!Path.IsPathRooted(aw.path))
            {
                aw.path = Path.GetDirectoryName(inputCSVPath) + "\\" + aw.path;
            }

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
            // Convert the asset path to an absolute path.
            // If it is relative, it's relative to the CSV directory.
            if (!Path.IsPathRooted(ass.path))
            {
                ass.path = Path.GetDirectoryName(inputCSVPath) + "\\" + ass.path;
            }
            if (String.IsNullOrWhiteSpace(ass.name))
                throw new InvalidCSVArtworkException("Asset name missing.");
            if (String.IsNullOrWhiteSpace(ass.path) || !Helpers.staticIsImageFile(ass.path) || !File.Exists(ass.path))
                throw new InvalidCSVArtworkException("Asset path is not an existing image file.");
            ass.uniqueName = generateUniqueName(ass.path, "Data/Images/Metadata/");
        }

        // Given a file path and a new directory, returns a new file name with no conflicts in the new directory.
        // Parameter newpath should have a trailing slash.
        // Sample Input:  /myRandomImageFolder/myImage.jpg, Data/Images/Metadata/
        //        Output: myImage403945345.jpg
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
            string imagePath = aw.path;
            string imageName = aw.uniqueName;
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
            String dataDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
            string thumbPath = dataDir + "Images/Thumbnail/" + aw.uniqueName;
            string imgPath = dataDir + "Images/" + aw.uniqueName;
            if (aw.path.Equals(thumbPath))
            {
                throw new InvalidCSVArtworkException("Artwork thumbnails already exist.");
            }

            // If a thumbnail path is provided, attempt to use it.
            // If this fails, we cascade to the general case rather than throwing an exception.
            if (!String.IsNullOrWhiteSpace(aw.thumbPath))
            {
                string customThumbPath = aw.thumbPath;
                // Convert to absolute path.  If relative, it's relative to the CSV directory.
                if (!Path.IsPathRooted(customThumbPath))
                {
                    customThumbPath = Path.GetDirectoryName(inputCSVPath) + "\\" + customThumbPath;
                }
                try
                {
                    File.Delete(thumbPath);
                    File.Delete(imgPath);
                    System.Drawing.Image img = Helpers.getThumbnail(customThumbPath, 800);
                    img.Save(thumbPath);
                    img.Save(imgPath);
                    img.Dispose();
                    return;
                }
                catch (Exception e)
                {
                    csvLog("Error in loading custom thumbnail."
                           + Environment.NewLine + "  Message: " + e.Message
                           + Environment.NewLine + "Attempting to automatically generate a thumbnail from the image.");
                }
            }

            // The general case.
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
            catch (Exception e)
            {
                throw new InvalidCSVArtworkException("Error creating thumbnail for artwork. Message: " + e.Message);
            }

        }

        // Assets written to XML must be copied to the Metadata folder, and a thumbnail is produced.
        public static void copyAsset(asset ass)
        {
            if (Helpers.staticIsImageFile(ass.path))
            {
                String dataDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\";
                String newPath = dataDir + "Images/Metadata/" + ass.uniqueName;
                try
                {
                    // First, copy the asset.
                    File.Delete(newPath);
                    File.Copy(ass.path, newPath);

                    // Then create a thumbnail.
                    System.Drawing.Image thumb = System.Drawing.Image.FromFile(ass.path);
                    thumb = thumb.GetThumbnailImage(128, 128, null, new IntPtr());
                    thumb.Save(Path.GetDirectoryName(newPath) + "/" + "Thumbnail/" + ass.uniqueName);
                    thumb.Dispose();
                }
                catch (Exception e)
                {
                    throw new InvalidCSVArtworkException("Error copying image asset at " + ass.path + ". Message: " + e.Message);
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
                catch (Exception e)
                {
                    throw new InvalidCSVArtworkException("Error copying video asset at " + ass.path);
                }
            }
        }

    }
}
