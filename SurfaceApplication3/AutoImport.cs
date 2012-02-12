using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Microsoft.DeepZoomTools;
using System.Threading;
using DexterLib;

namespace SurfaceApplication3
{
    class AutoImport
    {
        private Helpers _helpers;

        public AutoImport(String importFolderPath)
        {
            _helpers = new Helpers();
            this.importContent(importFolderPath);
        }

        public void importContent(String importFolderPath)
        {
            // TODO: ERROR HANDLING - try/catch blocks & e-mail curator?

            XmlDocument importDoc = new XmlDocument();
            importDoc.Load(importFolderPath + "\\import.xml"); // TODO: what if file doesn't load?

            XmlDocument catalogDoc = new XmlDocument();
            catalogDoc.Load(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\NewCollection.xml");

            XmlNode importDocNode = null; // LOAD
            for (int i = 0; i < importDoc.ChildNodes.Count; i++)
            {
                XmlNode childNode = importDoc.ChildNodes.Item(i);
                if (childNode.Name == "imagesToImport")
                {
                    importDocNode = childNode;
                    break;
                }
            }
            if (importDocNode == null)
            {
                Console.WriteLine("ERROR: import.xml is not a valid import file."); // TODO - e-mail curator?
                return;
            }

            XmlNode catalogDocNode = null; // SAVE
            for (int i = 0; i < catalogDoc.ChildNodes.Count; i++)
            {
                XmlNode childNode = catalogDoc.ChildNodes.Item(i);
                if (childNode.Name == "Collection")
                {
                    catalogDocNode = childNode;
                    break;
                }
            }
            if (catalogDocNode == null)
            {
                Console.WriteLine("ERROR: NewCollection.xml is not a valid catalog file."); // TODO - e-mail curator?
                return;
            }

            foreach (XmlNode imageNode in importDocNode.ChildNodes)
            {
                if (imageNode.Name == "image")
                {
                    String imageFile = imageNode.Attributes.GetNamedItem("file").InnerText;
                    String imagePath = importFolderPath + "\\" + imageFile + "\\" + imageFile;

                    String imageTitle = imageNode.Attributes.GetNamedItem("title").InnerText;
                    String imageYear = imageNode.Attributes.GetNamedItem("year").InnerText;
                    String imageArtist = imageNode.Attributes.GetNamedItem("artist").InnerText;
                    String imageMedium = imageNode.Attributes.GetNamedItem("medium").InnerText;
                    String imageDescription = imageNode.Attributes.GetNamedItem("description").InnerText;

                    int yearInput = 0;
                    try
                    {
                        yearInput = Convert.ToInt32(imageYear);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("ERROR: The year must be a valid number."); // TODO - e-mail curator?
                        return;
                    }
                    if (yearInput < -9999 || yearInput > 9999)
                    {
                        Console.WriteLine("ERROR: Year must be between -9999 and 9999."); // TODO - e-mail curator?
                        return;
                    }
                    XmlElement newEntry = catalogDoc.CreateElement("Image");
                    catalogDocNode.AppendChild(newEntry);

                    Random random = new Random();
                    int randomNumber = random.Next(0, 100000000);
                    string imageFileWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(imagePath);
                    string imageExtension = System.IO.Path.GetExtension(imagePath);
                    string destImageFile = imageFileWithoutExtension + "_" + randomNumber + imageExtension;
                    while (File.Exists(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\" + destImageFile))
                    {
                        randomNumber = random.Next(0, 100000000);
                        destImageFile = imageFileWithoutExtension + "_" + randomNumber + imageExtension;
                    }

                    newEntry.SetAttribute("path", "" + destImageFile);
                    newEntry.SetAttribute("title", "" + imageTitle);
                    newEntry.SetAttribute("year", "" + imageYear);
                    newEntry.SetAttribute("artist", "" + imageArtist);
                    newEntry.SetAttribute("medium", "" + imageMedium);
                    newEntry.SetAttribute("description", "" + imageDescription);

                    foreach (XmlNode imageChildNode in imageNode.ChildNodes)
                    {
                        if (imageChildNode.Name == "keywords")
                        {
                            XmlElement newKeywords = catalogDoc.CreateElement("Keywords");

                            foreach (XmlNode keywordNode in imageChildNode.ChildNodes)
                            {
                                if (keywordNode.Name == "keyword") {
                                    String keyword = keywordNode.Attributes.GetNamedItem("value").InnerText;

                                    XmlElement value = catalogDoc.CreateElement("Keyword");
                                    value.SetAttribute("Value", "" + keyword);
                                    newKeywords.AppendChild(value);
                                }
                            }

                            newEntry.AppendChild(newKeywords);
                        }

                        else if (imageChildNode.Name == "media")
                        {

                            XmlElement newMeta = catalogDoc.CreateElement("Metadata");
                            XmlElement newGroup = catalogDoc.CreateElement("Group");
                            newGroup.SetAttribute("name", "A"); //arbitrarily set the group as A

                            foreach (XmlNode itemNode in imageChildNode.ChildNodes)
                            {
                                XmlElement newFile = catalogDoc.CreateElement("Item");

                                String mediaFile = itemNode.Attributes.GetNamedItem("file").InnerText;
                                String mediaPath = importFolderPath + "\\" + imageFile + "\\media\\" + mediaFile;

                                String mediaName = itemNode.Attributes.GetNamedItem("name").InnerText;
                                String mediaDescription = itemNode.Attributes.GetNamedItem("description").InnerText;
                                String mediaType = itemNode.Attributes.GetNamedItem("type").InnerText;

                                newFile.SetAttribute("Name", mediaName);
                                newFile.SetAttribute("Description", mediaDescription);

                                if (itemNode.Attributes.GetNamedItem("type").InnerText == "image")
                                {
                                    newFile.SetAttribute("Type", "Image");

                                    randomNumber = random.Next(0, 100000000);
                                    string mediaFileWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(mediaPath);
                                    string mediaExtension = System.IO.Path.GetExtension(mediaPath);
                                    string destMediaFile = mediaFileWithoutExtension + "_" + randomNumber + mediaExtension;
                                    while (File.Exists(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\Metadata\\" + destMediaFile))
                                    {
                                        randomNumber = random.Next(0, 100000000);
                                        destMediaFile = mediaFileWithoutExtension + "_" + randomNumber + mediaExtension;
                                    }

                                    newFile.SetAttribute("Filename", destMediaFile);

                                    newGroup.AppendChild(newFile);

                                    String destMediaPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\Metadata\\" + destMediaFile;
                                    String destMediaThumbPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\Metadata\\Thumbnail\\" + destMediaFile;

                                    try
                                    {
                                        // Ensure that the target does not exist.
                                        File.Delete(destMediaPath);
                                        File.Delete(destMediaThumbPath);

                                        // save thumbnail
                                        System.Drawing.Image img = System.Drawing.Image.FromFile(mediaPath);
                                        img = img.GetThumbnailImage(128, 128, null, new IntPtr());
                                        img.Save(destMediaThumbPath);
                                        img.Dispose();

                                        // copy media file
                                        File.Copy(mediaPath, destMediaPath);
                                    }
                                    catch (Exception exc) {
                                        Console.WriteLine("Failed to create thumbnails and/or copy media file for " + destMediaFile); // TODO - e-mail curator?
                                        return;
                                    }
                                }

                                else if (itemNode.Attributes.GetNamedItem("type").InnerText == "video")
                                {
                                    newFile.SetAttribute("Type", "Video");

                                    randomNumber = random.Next(0, 100000000);
                                    string mediaFileWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(mediaPath);
                                    string mediaExtension = System.IO.Path.GetExtension(mediaPath);
                                    string destMediaFile = mediaFileWithoutExtension + "_" + randomNumber + mediaExtension;
                                    while (File.Exists(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Videos\\Metadata\\" + destMediaFile))
                                    {
                                        randomNumber = random.Next(0, 100000000);
                                        destMediaFile = mediaFileWithoutExtension + "_" + randomNumber + mediaExtension;
                                    }

                                    newFile.SetAttribute("Filename", destMediaFile);

                                    newGroup.AppendChild(newFile);

                                    String destMediaPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Videos\\Metadata\\" + destMediaFile;
                                    String destMediaThumbPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Videos\\Metadata\\" + destMediaFile; // TODO: change this to Thumbnail subdirectory LATER

                                    if (_helpers.IsDirShowFile(mediaPath)) {
                                        
                                        try
                                        {
                                            File.Delete(destMediaThumbPath);

                                            // save thumbnail
                                            DexterLib.MediaDet md = new MediaDet();
                                            md.Filename = mediaPath;
                                            md.CurrentStream = 0;
                                            string fBitmapName = destMediaThumbPath;
                                            fBitmapName = fBitmapName.Remove(fBitmapName.Length - 4, 4);
                                            fBitmapName += ".bmp";
                                            File.Delete(fBitmapName); // Ensure that the target does not exist.
                                            md.WriteBitmapBits(md.StreamLength / 2, 400, 240, fBitmapName);

                                            File.Delete(destMediaPath); // Ensure that the target does not exist.
                                            // copy media file
                                            File.Copy(mediaPath, destMediaPath);
                                        }
                                        catch (Exception exc)
                                        {
                                            Console.WriteLine("Failed to create thumbnails and/or copy media file for " + destMediaFile); // TODO - e-mail curator?
                                            return;
                                        }

                                    }
                                    else {
                                        try
                                        {
                                            // save thumbnail
                                            String sImageText = Path.GetFileNameWithoutExtension(mediaPath);
                                            System.Drawing.Bitmap objBmpImage = new System.Drawing.Bitmap(1, 1);

                                            int intWidth = 0;
                                            int intHeight = 0;

                                            // Create the Font object for the image text drawing.
                                            System.Drawing.Font objFont = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);

                                            // Create a graphics object to measure the text's width and height.
                                            System.Drawing.Graphics objGraphics = System.Drawing.Graphics.FromImage(objBmpImage);

                                            // This is where the bitmap size is determined.
                                            intWidth = (int)objGraphics.MeasureString(sImageText, objFont).Width;
                                            intHeight = (int)objGraphics.MeasureString(sImageText, objFont).Height;
                                            System.Drawing.Size newsize = new System.Drawing.Size(intWidth, intHeight);

                                            // Create the bmpImage again with the correct size for the text and font.
                                            objBmpImage = new System.Drawing.Bitmap(objBmpImage, newsize);

                                            // Add the colors to the new bitmap.
                                            objGraphics = System.Drawing.Graphics.FromImage(objBmpImage);

                                            // Set Background color
                                            objGraphics.Clear(System.Drawing.Color.White);
                                            objGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//SmoothingMode.AntiAlias;
                                            objGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;//TextRenderingHint.AntiAlias;
                                            objGraphics.DrawString(sImageText, objFont, new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(102, 102, 102)), 0, 0);
                                            objGraphics.Flush();

                                            string fBitmapName = destMediaThumbPath;
                                            fBitmapName = fBitmapName.Remove(fBitmapName.Length - 4, 4);
                                            fBitmapName += ".bmp";
                                            File.Delete(fBitmapName); // Ensure that the target does not exist.
                                            objBmpImage.Save(fBitmapName);

                                            File.Delete(destMediaPath); // Ensure that the target does not exist.
                                            // copy media file
                                            File.Copy(mediaPath, destMediaPath);
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Failed to create thumbnails and/or copy media file for " + destMediaFile); // TODO - e-mail curator?
                                            return;
                                        }
                                    }
                                }
                            }

                            newMeta.AppendChild(newGroup);
                            newEntry.AppendChild(newMeta);
                        }

                        else if (imageChildNode.Name == "hotspots")
                        {
                            XmlDocument hotspotDoc = new XmlDocument();
                            XmlNode hotspotDocNode = hotspotDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                            hotspotDoc.AppendChild(hotspotDocNode);
                            XmlElement hotspots = hotspotDoc.CreateElement("hotspots");
                            hotspotDoc.AppendChild(hotspots);

                            foreach (XmlNode hotspotNode in imageChildNode.ChildNodes)
                            {
                                XmlElement hotspot = hotspotDoc.CreateElement("hotspot");

                                XmlElement hotspotName = hotspotDoc.CreateElement("name");
                                XmlElement hotspotType = hotspotDoc.CreateElement("type");
                                XmlElement hotspotDescription = hotspotDoc.CreateElement("description");
                                XmlElement hotspotPositionX = hotspotDoc.CreateElement("positionX");
                                XmlElement hotspotPositionY = hotspotDoc.CreateElement("positionY");

                                hotspotName.InnerText = hotspotNode.Attributes.GetNamedItem("name").InnerText;
                                hotspotPositionX.InnerText = hotspotNode.Attributes.GetNamedItem("positionX").InnerText;
                                hotspotPositionY.InnerText = hotspotNode.Attributes.GetNamedItem("positionY").InnerText;

                                if (hotspotNode.Attributes.GetNamedItem("type").InnerText == "text")
                                {
                                    hotspotType.InnerText = "text";

                                    hotspotDescription.InnerText = hotspotNode.Attributes.GetNamedItem("description").InnerText;
                                }
                                else if (hotspotNode.Attributes.GetNamedItem("type").InnerText == "image")
                                {
                                    hotspotType.InnerText = "image";

                                    String hotspotMediaFile = hotspotNode.Attributes.GetNamedItem("description").InnerText;
                                    String hotspotMediaPath = importFolderPath + "\\" + imageFile + "\\hotspots\\" + hotspotMediaFile;

                                    randomNumber = random.Next(0, 100000000);
                                    string hotspotMediaFileWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(hotspotMediaPath);
                                    string hotspotMediaExtension = System.IO.Path.GetExtension(hotspotMediaPath);
                                    string destHotspotMediaFile = hotspotMediaFileWithoutExtension + "_" + randomNumber + hotspotMediaExtension;
                                    while (File.Exists(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\" + destHotspotMediaFile))
                                    {
                                        randomNumber = random.Next(0, 100000000);
                                        destHotspotMediaFile = hotspotMediaFileWithoutExtension + "_" + randomNumber + hotspotMediaExtension;
                                    }

                                    hotspotDescription.InnerText = destHotspotMediaFile;

                                    String destHotspotMediaPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\" + destHotspotMediaFile;
                                    String destHotspotMediaThumbPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Images\\Thumbnail\\" + destHotspotMediaFile;

                                    try
                                    {
                                        // Ensure that the target does not exist.
                                        File.Delete(destHotspotMediaPath);
                                        File.Delete(destHotspotMediaThumbPath);

                                        // save thumbnail
                                        System.Drawing.Image img = System.Drawing.Image.FromFile(hotspotMediaPath);
                                        img = img.GetThumbnailImage(128, 128, null, new IntPtr());
                                        img.Save(destHotspotMediaThumbPath);
                                        img.Dispose();

                                        // copy media file
                                        File.Copy(hotspotMediaPath, destHotspotMediaPath);
                                    }
                                    catch (Exception exc)
                                    {
                                        Console.WriteLine("Failed to create thumbnails and/or copy hotspot media file for " + destHotspotMediaFile); // TODO - e-mail curator?
                                        return;
                                    }
                                }

                                else if (hotspotNode.Attributes.GetNamedItem("type").InnerText == "audio")
                                {
                                    hotspotType.InnerText = "audio";

                                    String hotspotMediaFile = hotspotNode.Attributes.GetNamedItem("description").InnerText;
                                    String hotspotMediaPath = importFolderPath + "\\" + imageFile + "\\hotspots\\" + hotspotMediaFile;

                                    randomNumber = random.Next(0, 100000000);
                                    string hotspotMediaFileWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(hotspotMediaPath);
                                    string hotspotMediaExtension = System.IO.Path.GetExtension(hotspotMediaPath);
                                    string destHotspotMediaFile = hotspotMediaFileWithoutExtension + "_" + randomNumber + hotspotMediaExtension;
                                    while (File.Exists(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Audios\\" + destHotspotMediaFile))
                                    {
                                        randomNumber = random.Next(0, 100000000);
                                        destHotspotMediaFile = hotspotMediaFileWithoutExtension + "_" + randomNumber + hotspotMediaExtension;
                                    }

                                    hotspotDescription.InnerText = destHotspotMediaFile;

                                    String destHotspotMediaPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Audios\\" + destHotspotMediaFile;

                                    try
                                    {
                                        // Ensure that the target does not exist.
                                        File.Delete(destHotspotMediaPath);

                                        // copy media file
                                        File.Copy(hotspotMediaPath, destHotspotMediaPath);
                                    }
                                    catch (Exception exc)
                                    {
                                        Console.WriteLine("Failed to copy hotspot media file for " + destHotspotMediaFile); // TODO - e-mail curator?
                                        return;
                                    }
                                }

                                else if (hotspotNode.Attributes.GetNamedItem("type").InnerText == "video")
                                {
                                    hotspotType.InnerText = "video";

                                    String hotspotMediaFile = hotspotNode.Attributes.GetNamedItem("description").InnerText;
                                    String hotspotMediaPath = importFolderPath + "\\" + imageFile + "\\hotspots\\" + hotspotMediaFile;

                                    randomNumber = random.Next(0, 100000000);
                                    string hotspotMediaFileWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(hotspotMediaPath);
                                    string hotspotMediaExtension = System.IO.Path.GetExtension(hotspotMediaPath);
                                    string destHotspotMediaFile = hotspotMediaFileWithoutExtension + "_" + randomNumber + hotspotMediaExtension;
                                    while (File.Exists(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Videos\\" + destHotspotMediaFile))
                                    {
                                        randomNumber = random.Next(0, 100000000);
                                        destHotspotMediaFile = hotspotMediaFileWithoutExtension + "_" + randomNumber + hotspotMediaExtension;
                                    }

                                    hotspotDescription.InnerText = destHotspotMediaFile;

                                    String destHotspotMediaPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Hotspots\\Videos\\" + destHotspotMediaFile;

                                    try
                                    {
                                        // Ensure that the target does not exist.
                                        File.Delete(destHotspotMediaPath);

                                        // copy media file
                                        File.Copy(hotspotMediaPath, destHotspotMediaPath);
                                    }
                                    catch (Exception exc)
                                    {
                                        Console.WriteLine("Failed to copy hotspot media file for " + destHotspotMediaFile); // TODO - e-mail curator?
                                        return;
                                    }
                                }

                                hotspot.AppendChild(hotspotName);
                                hotspot.AppendChild(hotspotPositionX);
                                hotspot.AppendChild(hotspotPositionY);
                                hotspot.AppendChild(hotspotType);
                                hotspot.AppendChild(hotspotDescription);

                                hotspots.AppendChild(hotspot);
                            }

                            hotspotDoc.Save(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\XMLFiles\\" + destImageFile + "." + "xml");
                        }
                    }
                    // LOAD_END

                    // SAVE
                    Boolean imageNameExist = false;
                    if (imageFile != "" && imageYear != "" && imageTitle != "")
                    {
                        if (catalogDocNode.HasChildNodes)
                        {
                            foreach (XmlNode catalogImageNode in catalogDocNode.ChildNodes)
                            {
                                if (catalogImageNode.Name == "Image")
                                {
                                    String oldImageName = catalogImageNode.Attributes.GetNamedItem("path").InnerText;

                                    if (oldImageName == imageFile)
                                    {
                                        imageNameExist = true;
                                    }
                                }
                            }
                        }

                        if (!imageNameExist)
                        {
                            String destThumbPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\Thumbnail\\" + destImageFile;
                            String destImagePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\" + destImageFile;

                            try
                            {
                                // Ensure that the target does not exist.
                                File.Delete(destThumbPath);
                                File.Delete(destImagePath);

                                // Copy the file.
                                System.Drawing.Image img = Helpers.getThumbnail(imagePath, 800);
                                img.Save(destThumbPath);
                                img.Save(destImagePath);
                                img.Dispose();

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed to create thumbnails for " + imageFile); // TODO - e-mail curator?
                                return;
                            }

                            Console.WriteLine("Saved " +imageFile+ " to catalog. Converting to Deep Zoom format...");

                            // Create Deep Zoom Image:
                            String destFolderPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\Images\\DeepZoom";

                            ImageCreator ic = new ImageCreator();

                            ic.TileFormat = ImageFormat.Jpg;
                            ic.TileOverlap = 1;
                            ic.TileSize = 256;
                            ic.ImageQuality = 0.92;
                            ic.UseOptimizations = true;
                            Directory.CreateDirectory(destFolderPath + "\\" + destImageFile);

                            string target = destFolderPath + "\\" + destImageFile + "\\dz.xml";

                            ic.Create(imagePath, target);
                            ic = null;
                            System.GC.Collect();
                            System.GC.Collect();
                            Thread.Sleep(0);

                            catalogDoc.Save(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Data\\NewCollection.xml");
                        }
                        else
                        {
                            Console.WriteLine("The image " + imageFile + " already exists in the catalog"); // TODO - e-mail curator?
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Some items are still missing:"); // TODO - e-mail curator?
                        if (imageFile == "")
                        {
                            Console.WriteLine("\t-Image"); // TODO - e-mail curator?
                        }

                        if (imageTitle == "")
                        {
                            Console.WriteLine("\t-Title"); // TODO - e-mail curator?
                        }

                        if (imageYear == "")
                        {
                            Console.WriteLine("\t-Year"); // TODO - e-mail curator?
                        }

                    }
                    // SAVE_END
                }
            }
        }
    }
}
