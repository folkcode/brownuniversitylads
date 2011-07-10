using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Knowledge_Web
{
    public class WebXMLReader
    {
        public static List<WebGroup> LoadFromXML(String filename, String pathPrefix = "", String metaPrefix = "")
        {
            List<WebGroup> groups = new List<WebGroup>();
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            if (doc.HasChildNodes)
            {
                foreach (XmlNode docNode in doc.ChildNodes)
                {
                    if (docNode.Name == "Collection")
                    {
                        foreach (XmlNode imageNode in docNode.ChildNodes)
                        {
                            String thumb = pathPrefix + imageNode.Attributes.GetNamedItem("path").InnerText;
                            String title = imageNode.Attributes.GetNamedItem("title").InnerText;
                            WebGroup group = new WebGroup(@thumb, title);
                            foreach (XmlNode imageDetail in imageNode.ChildNodes)
                            {
                                if (imageDetail.Name == "Keywords")
                                {
                                    foreach (XmlNode keyword in imageDetail.ChildNodes)
                                    {
                                        group.addKeyword(keyword.Attributes.GetNamedItem("Value").InnerText);
                                    }
                                }
                                else if (imageDetail.Name == "Metadata")
                                {
                                    foreach (XmlNode pile in imageDetail.ChildNodes)
                                    {
                                        String groupName = pile.Attributes.GetNamedItem("name").InnerText;
                                        foreach (XmlNode groupItem in pile.ChildNodes)
                                        {
                                            if (groupItem.Name == "Item") 
                                            {
                                                String itemThumb = pathPrefix + metaPrefix + groupItem.Attributes.GetNamedItem("Filename").InnerText;
                                                group.AddToGroup(groupName, @itemThumb);
                                            }
                                        }
                                    }
                                }
                            }

                            groups.Add(group);
                        }
                    }
                }
            }

            return groups;
        }
    }
}
