using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;

namespace Importer.Main
{
    class XmlFormer
    {
        List<string> lines;
        List<string> xmlFiles;

        public int okpoRowPosition { get; set; }
        public int soateRowPosition { get; set; }

        public string FormId { get; set; }
        public string Period { get; set; }

        public List<string> SectionIds { get; set; }
        public List<string> DsdMonikers { get; set; }

        public Dictionary<string, string> xmlHeaderMap = new Dictionary<string, string>()
        {
            { "%receiverIdentifier%", "" },
            { "%senderIdentifier%", "" },
            { "%TIME%", "" }
        };

        public XmlFormer(List<string> lines, List<string> xmlFiles)
        {
            this.lines = lines;
            this.xmlFiles = xmlFiles;
        }

        private void FormStaticDataXmlFile()
        {

            string datetime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

            Dictionary<string, string> staticData = new Dictionary<string, string>()
            {
                { "Form_ID", this.FormId },
                { "Period", this.Period },
                { "Datetime", datetime }
            };

            string staticDataXmlFilePath = String.Format(@"{0}Files\staticData.xml", AppDomain.CurrentDomain.BaseDirectory);
            XmlWriterSettings xmlSettings = Main.CustomizedXmlWriterSettingsInstance();

            using (XmlWriter writer = XmlWriter.Create(staticDataXmlFilePath, xmlSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Rows");

                writer.WriteStartElement("FormInfo");
                writer.WriteAttributeString("formId", staticData["Form_ID"]);
                writer.WriteAttributeString("period", staticData["Period"]);
                writer.WriteAttributeString("datetime", staticData["Datetime"]);
                writer.WriteEndElement();

                writer.WriteStartElement("Sections");

                for (int i = 0; i < SectionIds.Count; i++)
                {
                    writer.WriteStartElement("Section");
                    writer.WriteAttributeString("id", SectionIds[i]);
                    writer.WriteAttributeString("dsdMoniker", DsdMonikers[i]);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private Dictionary<string, string> FormMap(string filePath, string type = "map")
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

            string mapFilePath = String.Format(filePath);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(mapFilePath);

            XmlNodeList rows = xmlDocument.DocumentElement.SelectNodes("/Rows/Row");

            if (type == "map")
            {
                foreach (XmlNode row in rows)
                    map.Add(
                        row.Attributes["key"].Value,
                        row.Attributes["value"].Value
                        );
            }
            else
            {
                foreach (XmlNode row in rows)
                    map.Add(
                        row.Attributes["okpo"].Value,
                        row.Attributes["id"].Value
                        );
            }

            return map;
        }

        private string ReplaceXmlHeaderKeysWithValues(string template, Dictionary<string, string> map)
        {
            foreach (KeyValuePair<string, string> item in map)
                template = template.Replace(item.Key, item.Value);
            return template;
        }

        private string ReplaceXmlBodyKeysWithValues(string template, Dictionary<string, string> map, List<string> row)
        {
            string value;
            int intItemValue;
            foreach (KeyValuePair<string, string> item in map)
            {
                if (template.Contains(item.Key))
                {
                    intItemValue = int.Parse(item.Value);
                    value = (row[intItemValue] == "None" || row[intItemValue] == "0") ? "0.0" : row[intItemValue];
                    template = template.Replace(item.Key, value);
                }
            }
            return template;
        }

        public void worker_DoWork(Object sender, DoWorkEventArgs e)
        {
            this.FormStaticDataXmlFile();

            SqlCommand command;
            int progressPercentage;
            int linesCount = lines.Count;

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);

            string soate;
            string code;
            string okpo;
            string xmlTemplate;
            string okpoTemplate;
            int sectionCounter;
            List<string> row;

            string senderIdentifiersFilePath = String.Format(@"{0}Files\sender_identifiers.xml", AppDomain.CurrentDomain.BaseDirectory);
            Dictionary<string, string>  GetSenderIdByOkpo = this.FormMap(senderIdentifiersFilePath, "identifiers");

            string mapFilePath = String.Format(@"{0}files\map.xml", AppDomain.CurrentDomain.BaseDirectory);
            Dictionary<string, string>  xmlBodyMap = this.FormMap(mapFilePath);

            string dataFilePath = String.Format(@"{0}Files\data.xml", AppDomain.CurrentDomain.BaseDirectory);
            XmlWriterSettings xmlSettings = Main.CustomizedXmlWriterSettingsInstance();
            using (XmlWriter writer = XmlWriter.Create(dataFilePath, xmlSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Rows");

                for (int i = 1; i < linesCount; i++)
                {
                    row = lines[i].Split(',').ToList();

                    soate = row[soateRowPosition];
                    code = soate.Substring(3, 5);
                    xmlHeaderMap["%receiverIdentifier%"] = Main.GetReceiverDataByCode[code]["id"];

                    okpo = row[okpoRowPosition];
                    try
                    {
                        xmlHeaderMap["%senderIdentifier%"] = GetSenderIdByOkpo[okpo];
                    } catch (Exception ex)
                    {
                        Main.AppendTextToFile(ex.ToString());
                        continue;
                    }

                    xmlHeaderMap["%TIME%"] = this.Period;

                    writer.WriteStartElement("Row");
                    writer.WriteAttributeString("okpo", okpo);
                    writer.WriteAttributeString("soate", soate);
                    writer.WriteAttributeString("user_id", xmlHeaderMap["%senderIdentifier%"]);
                    writer.WriteAttributeString("departmentType", Main.GetReceiverDataByCode[code]["type"]);
                    writer.WriteEndElement(); // </Row>

                    sectionCounter = 0;
                    foreach (string xmlFile in xmlFiles)
                    {
                        using (StreamReader reader = new StreamReader(xmlFile, Encoding.UTF8))
                            xmlTemplate = reader.ReadToEnd();

                        okpoTemplate = ReplaceXmlHeaderKeysWithValues(xmlTemplate, xmlHeaderMap);
                        okpoTemplate = ReplaceXmlBodyKeysWithValues(okpoTemplate, xmlBodyMap, row);

                        sectionCounter++;
                        using (command = new SqlCommand("OkpoXmlSectionsInsert", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            SqlParameter section = new SqlParameter("@section", SqlDbType.NVarChar, 150);
                            SqlParameter xmlContent = new SqlParameter("@xmlContent", SqlDbType.Xml);
                            SqlParameter dbOkpo = new SqlParameter("@okpo", SqlDbType.NVarChar, 50);

                            section.Value = $"section_{sectionCounter}";
                            xmlContent.Value = okpoTemplate;
                            dbOkpo.Value = okpo;

                            command.Parameters.Add(section);
                            command.Parameters.Add(xmlContent);
                            command.Parameters.Add(dbOkpo);

                            try
                            {
                                connection.Open();
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Main.AppendTextToFile(ex.ToString());
                            }
                            finally
                            {
                                command.Dispose();
                                connection.Close();
                            }
                        }

                    }

                    progressPercentage = Convert.ToInt32(((double)i / linesCount) * 100);
                    (sender as BackgroundWorker).ReportProgress(progressPercentage);
                }

                (sender as BackgroundWorker).ReportProgress(100);

                writer.WriteEndElement(); // </Rows>
                writer.WriteEndDocument();
            }

            connection.Close();
        }
    }
}
