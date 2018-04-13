using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

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

        Dictionary<string, string> xmlHeaderMap = new Dictionary<string, string>()
        {
            { "%REC%", "" },
            { "%SENDER%", "" }
        };

        public XmlFormer(List<string> lines, List<string> xmlFiles)
        {
            this.lines = lines;
            this.xmlFiles = xmlFiles;
        }

        private Dictionary<string, string> FormStaticData()
        {
            string datetime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

            Dictionary<string, string> staticData = new Dictionary<string, string>()
            {
                { "Form_ID", this.FormId },
                { "Period", this.Period },
                { "Datetime", datetime }
            };

            string sectionIdsRow = "";
            foreach (string sectionId in SectionIds)
                sectionIdsRow += sectionId;
            staticData.Add("SectionIds", sectionIdsRow);

            string dsdMonikersRow = "";
            foreach (string dsdMoniker in DsdMonikers)
                dsdMonikersRow += dsdMoniker;
            staticData.Add("DsdMonikers", dsdMonikersRow);

            return staticData;
        }

        private Dictionary<string, string> FormMap(string filePath)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

            string mapFilePath = String.Format(filePath);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(mapFilePath);

            XmlNodeList rows = xmlDocument.DocumentElement.SelectNodes("/Rows/Row");

            foreach (XmlNode row in rows)
                map.Add(
                    row.Attributes["key"].Value,
                    row.Attributes["value"].Value
                    );

            return map;
        }

        private string ReplaceXmlHeaderKeysWithValues(Dictionary<string, string> map, string template)
        {
            foreach (KeyValuePair<string, string> item in map)
                template = template.Replace(item.Key, item.Value);
            return template;
        }

        private string ReplaceXmlBodyKeysWithValues(Dictionary<string, string> map, string template)
        {
            string value;
            foreach (KeyValuePair<string, string> item in map)
                if (template.Contains(item.Key))
                {
                    value = (item.Value == "0" || item.Value == "0.0") ? "" : item.Value;
                    template = template.Replace(item.Key, value);
                }
            return template;
        }

        public void worker_DoWork(Object sender, DoWorkEventArgs e)
        {
            Dictionary<string, string> staticData = this.FormStaticData();

            string soate;
            string code;
            string okpo;
            string xmlTemplate;
            string okpoTemplate;
            List<string> row;
            foreach (string line in lines)
            {
                row = line.Split(',').ToList();

                string senderIdentifiersFilePath = String.Format(@"{0}Files\sender_identifiers.xml", AppDomain.CurrentDomain.BaseDirectory);
                Dictionary<string, string> GetSenderIdByOkpo = this.FormMap(senderIdentifiersFilePath);

                string mapFilePath = String.Format(@"{0}files\map.xml", AppDomain.CurrentDomain.BaseDirectory);
                Dictionary<string, string> xmlBodyMap = this.FormMap(mapFilePath);

                soate = row[soateRowPosition];
                code = soate.Substring(3, 5);
                xmlHeaderMap["%REC%"] = ReceiversData.GetReceiverDataByCode[code]["id"];

                okpo = row[okpoRowPosition];
                xmlHeaderMap["%SENDER%"] = GetSenderIdByOkpo[okpo];

                foreach (string xmlFile in xmlFiles)
                {
                    using (StreamReader reader = new StreamReader(xmlFile, Encoding.UTF8))
                        xmlTemplate = reader.ReadToEnd();

                    okpoTemplate = ReplaceXmlHeaderKeysWithValues(xmlHeaderMap, xmlTemplate);
                    okpoTemplate = ReplaceXmlBodyKeysWithValues(xmlBodyMap, okpoTemplate);
                }
            }
        }
    }
}
