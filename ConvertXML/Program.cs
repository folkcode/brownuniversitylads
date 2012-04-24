using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.IO;

namespace ConvertXML
{
    class Program
    {
        private XmlTextWriter _xmlWriter;
        private HashSet<string> _desiredFields;

        public Program(string sourcePath, string destPath)
        {
            this.InitDesiredFields();
            XmlNodeList dbRows = LoadXML(sourcePath);
            this.InitWriter(destPath);
            this.GenerateXML(dbRows);
            this.CloseWriter();
            Utils.CleanUpTempFiles();
            Debug.WriteLine("Success!");
        }

        private void InitDesiredFields()
        {
            _desiredFields = new HashSet<string>();
            _desiredFields.Add("Block");
            _desiredFields.Add("City");
            _desiredFields.Add("Country");
            _desiredFields.Add("DateReceived");
            _desiredFields.Add("dob");
            _desiredFields.Add("dod");
            _desiredFields.Add("MakerFirst");
            _desiredFields.Add("MakerLast");
            _desiredFields.Add("OtherCities");
            _desiredFields.Add("PanelFirst");
            _desiredFields.Add("PanelLast");
            _desiredFields.Add("PanelNumber");
            _desiredFields.Add("State");
        }

        /// <summary>
        /// Load the database generated xml file
        /// </summary>
        /// <param name="dbXmlPath">The xml file path</param>
        /// <returns>Rows in database</returns>
        private XmlNodeList LoadXML(string dbXmlPath)
        {
            //load xml document, may not be necessary
            XmlDocument doc = new XmlDocument();
            doc.Load(Utils.SanitizeXmlFile(dbXmlPath));
            return doc.SelectNodes("/pma_xml_export/database/table");
        }

        /// <summary>
        /// Initialize XmlTextWriter and set up writing properties
        /// </summary>
        /// <param name="dest">The destination xml file</param>
        private void InitWriter(string dest)
        {
            // Create a new file
            _xmlWriter = new XmlTextWriter(dest, null);
            _xmlWriter.Formatting = Formatting.Indented;
            _xmlWriter.Indentation = 4;
            // Opens the document
            _xmlWriter.WriteStartDocument();
        }

        /// <summary>
        /// Finalize the xml document.
        /// </summary>
        private void CloseWriter()
        {
            // Ends the document.
            _xmlWriter.WriteEndDocument();
            // close writer
            _xmlWriter.Close();
        }

        /// <summary>
        /// Writes the database rows to xml file using the given xml text writer 
        /// </summary>
        /// <param name="rows">Rows in database</param>
        private void GenerateXML(XmlNodeList rows)
        {
            _xmlWriter.WriteStartElement("Collection");
            foreach (XmlNode row in rows)
            {
                _xmlWriter.WriteStartElement("Image");
                XmlNodeList columns = row.SelectNodes("column");
                foreach (XmlNode column in columns)
                {
                    String name = column.Attributes.GetNamedItem("name").InnerText;
                    if (_desiredFields.Contains(name))
                    {
                        //Debug.WriteLine(name + ": " + column.InnerText);
                        _xmlWriter.WriteStartAttribute(name);
                        if (name.Equals("Country") && column.InnerText.Equals("NULL"))
                        {
                            _xmlWriter.WriteString("USA");
                        }
                        else
                        {
                            _xmlWriter.WriteString(column.InnerText);
                        }
                        _xmlWriter.WriteEndAttribute();
                    }
                    
                }
                _xmlWriter.WriteEndElement();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">input xml path and output xml path</param>
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                new Program(args[0], args[1]);
            }
            else
            {
                Debug.WriteLine("Two arguments needed.");
            }
        }
    }
}
