using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public int FormId { get; set; }
        public int PeriodType { get; set; }

        public List<int> SectionIds { get; set; }
        public List<string> DsdMonikers { get; set; }

        public XmlFormer(List<string> lines)
        {
            this.lines = lines;
        }

        public void worker_DoWork(Object sender, DoWorkEventArgs e)
        {

        }
    }
}
