using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Importer.Main
{
    class XmlFormer
    {
        List<string> lines;

        public int okpoRowPosition { get; set; }
        public int soateRowPosition { get; set; }

        public string FormId { get; set; }
        public string Period { get; set; }

        public List<string> SectionIds { get; set; }
        public List<string> DsdMonikers { get; set; }

        Dictionary<string, string> xmlHeaderMap = new Dictionary<string, string>()
        {
            { "%receiverid%", "" },
            { "%senderid%", "" }
        };

        public XmlFormer(List<string> lines)
        {
            this.lines = lines;
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

        private Dictionary<string, string> FormGetSenderIdByOkpo()
        {
            Dictionary<string, string> GetSenderIdByOkpo = new Dictionary<string, string>();

            string senderIdentifierFilePath = String.Format(@"{0}Files\sender_identifiers.xml", AppDomain.CurrentDomain.BaseDirectory);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(senderIdentifierFilePath);

            XmlNodeList rows = xmlDocument.DocumentElement.SelectNodes("/Rows/Row");

            foreach (XmlNode row in rows)
                GetSenderIdByOkpo.Add(
                    row.Attributes["key"].Value,
                    row.Attributes["value"].Value
                    );

            return GetSenderIdByOkpo;
        }

        public void worker_DoWork(Object sender, DoWorkEventArgs e)
        {
            Dictionary<string, string> staticData = this.FormStaticData();

            foreach (string line in lines)
            {
                string[] row = line.Split(',');

                Dictionary<string, string> GetSenderIdByOkpo = this.FormGetSenderIdByOkpo();

                string soate = row[soateRowPosition];
                string code = soate.Substring(3, 5);
                xmlHeaderMap["%receiverid%"] = ReceiversData.GetReceiverDataByCode[code]["id"];

                string okpo = row[okpoRowPosition];
                xmlHeaderMap["%senderid%"] = GetSenderIdByOkpo[okpo];


            }
        }
    }
}
