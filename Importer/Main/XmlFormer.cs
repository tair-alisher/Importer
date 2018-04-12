using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public XmlFormer(List<string> lines)
        {
            this.lines = lines;
        }

        public Dictionary<string, string> FormStaticData()
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

        public void worker_DoWork(Object sender, DoWorkEventArgs e)
        {
            
        }

        
    }
}
