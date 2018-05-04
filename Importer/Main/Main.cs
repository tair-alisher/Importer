using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Importer.Main
{
    public static class Main
    {
        public static void AppendTextToFile(string text)
        {
            File.AppendAllText(String.Format(@"Files\logs\logs.txt"), Environment.NewLine + (text + DateTime.Now.ToString()) + Environment.NewLine);
        }

        public static XmlWriterSettings CustomizedXmlWriterSettingsInstance()
        {
            XmlWriterSettings xws = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t",
                Encoding = Encoding.UTF8
            };

            return xws;
        }

        public static readonly Dictionary<string, Dictionary<string, string>> GetReceiverDataByCode = new Dictionary<string, Dictionary<string, string>>()
        {
            {
                "",
                new Dictionary<string, string>() { { "id", "1" }, { "type", "0" } }
            },
            {
                "02000",
                new Dictionary<string, string>() { { "id", "6" }, { "type", "1" } }
            },
            {
                "03000",
                new Dictionary<string, string>() { { "id", "6" }, { "type", "1" } }
            },
            {
                "03204",
                new Dictionary<string, string>() { { "id", "7" }, { "type", "2" } }
            },
            {
                "03207",
                new Dictionary<string, string>() { { "id", "8" }, { "type", "2" } }
            },
            {
                "05000",
                new Dictionary<string, string>() { { "id", "10" }, { "type", "1" } }
            },
            {
                "05258",
                new Dictionary<string, string>() { { "id", "11" }, { "type", "2" } }
            },
            {
                "04000",
                new Dictionary<string, string>() { { "id", "12" }, { "type", "1" } }
            },
            {
                "04400",
                new Dictionary<string, string>() { { "id", "13" }, { "type", "2" } }
            },
            {
                "04230",
                new Dictionary<string, string>() { { "id", "14" }, { "type", "2" } }
            },
            {
                "21000",
                new Dictionary<string, string>() { { "id", "15" }, { "type", "1" } }
            },
            {
                "11000",
                new Dictionary<string, string>() { { "id", "16" }, { "type", "1" } }
            },
            {
                "11203",
                new Dictionary<string, string>() { { "id", "16" }, { "type", "1" } }
            },
            {
                "02205",
                new Dictionary<string, string>() { { "id", "17" }, { "type", "2" } }
            },
            {
                "02210",
                new Dictionary<string, string>() { { "id", "18" }, { "type", "2" } }
            },
            {
                "02215",
                new Dictionary<string, string>() { { "id", "19" }, { "type", "2" } }
            },
            {
                "02220",
                new Dictionary<string, string>() { { "id", "20" }, { "type", "2" } }
            },
            {
                "02225",
                new Dictionary<string, string>() { { "id", "21" }, { "type", "2" } }
            },
            {
                "02420",
                new Dictionary<string, string>() { { "id", "22" }, { "type", "2" } }
            },
            {
                "03211",
                new Dictionary<string, string>() { { "id", "23" }, { "type", "2" } }
            },
            {
                "03215",
                new Dictionary<string, string>() { { "id", "24" }, { "type", "2" } }
            },
            {
                "03220",
                new Dictionary<string, string>() { { "id", "25" }, { "type", "2" } }
            },
            {
                "03223",
                new Dictionary<string, string>() { { "id", "26" }, { "type", "2" } }
            },
            {
                "03225",
                new Dictionary<string, string>() { { "id", "27" }, { "type", "2" } }
            },
            {
                "03230",
                new Dictionary<string, string>() { { "id", "28" }, { "type", "2" } }
            },
            {
                "03410",
                new Dictionary<string, string>() { { "id", "29" }, { "type", "2" } }
            },
            {
                "03420",
                new Dictionary<string, string>() { { "id", "30" }, { "type", "2" } }
            },
            {
                "03430",
                new Dictionary<string, string>() { { "id", "31" }, { "type", "2" } }
            },
            {
                "03440",
                new Dictionary<string, string>() { { "id", "32" }, { "type", "2" } }
            },
            {
                "04210",
                new Dictionary<string, string>() { { "id", "33" }, { "type", "2" } }
            },
            {
                "04220",
                new Dictionary<string, string>() { { "id", "34" }, { "type", "2" } }
            },
            {
                "04235",
                new Dictionary<string, string>() { { "id", "35" }, { "type", "2" } }
            },
            {
                "04245",
                new Dictionary<string, string>() { { "id", "36" }, { "type", "2" } }
            },
            {
                "05214",
                new Dictionary<string, string>() { { "id", "37" }, { "type", "2" } }
            },
            {
                "05236",
                new Dictionary<string, string>() { { "id", "38" }, { "type", "2" } }
            },
            {
                "05420",
                new Dictionary<string, string>() { { "id", "39" }, { "type", "2" } }
            },
            {
                "06000",
                new Dictionary<string, string>() { { "id", "40" }, { "type", "1" } }
            },
            {
                "06207",
                new Dictionary<string, string>() { { "id", "41" }, { "type", "2" } }
            },
            {
                "06211",
                new Dictionary<string, string>() { { "id", "42" }, { "type", "2" } }
            },
            {
                "06226",
                new Dictionary<string, string>() { { "id", "43" }, { "type", "2" } }
            },
            {
                "06242",
                new Dictionary<string, string>() { { "id", "44" }, { "type", "2" } }
            },
            {
                "06246",
                new Dictionary<string, string>() { { "id", "45" }, { "type", "2" } }
            },
            {
                "06255",
                new Dictionary<string, string>() { { "id", "46" }, { "type", "2" } }
            },
            {
                "06259",
                new Dictionary<string, string>() { { "id", "47" }, { "type", "2" } }
            },
            {
                "07000",
                new Dictionary<string, string>() { { "id", "48" }, { "type", "1" } }
            },
            {
                "07215",
                new Dictionary<string, string>() { { "id", "49" }, { "type", "1" } }
            },
            {
                "07220",
                new Dictionary<string, string>() { { "id", "50" }, { "type", "2" } }
            },
            {
                "07225",
                new Dictionary<string, string>() { { "id", "51" }, { "type", "2" } }
            },
            {
                "07232",
                new Dictionary<string, string>() { { "id", "52" }, { "type", "2" } }
            },
            {
                "08000",
                new Dictionary<string, string>() { { "id", "53" }, { "type", "1" } }
            },
            {
                "08203",
                new Dictionary<string, string>() { { "id", "54" }, { "type", "2" } }
            },
            {
                "08206",
                new Dictionary<string, string>() { { "id", "55" }, { "type", "2" } }
            },
            {
                "08209",
                new Dictionary<string, string>() { { "id", "56" }, { "type", "2" } }
            },
            {
                "08213",
                new Dictionary<string, string>() { { "id", "57" }, { "type", "2" } }
            },
            {
                "08217",
                new Dictionary<string, string>() { { "id", "58" }, { "type", "2" } }
            },
            {
                "08219",
                new Dictionary<string, string>() { { "id", "59" }, { "type", "2" } }
            },
            {
                "08222",
                new Dictionary<string, string>() { { "id", "60" }, { "type", "2" } }
            },
            {
                "08223",
                new Dictionary<string, string>() { { "id", "61" }, { "type", "2" } }
            },
            {
                "02410",
                new Dictionary<string, string>() { { "id", "62" }, { "type", "2" } }
            },
            {
                "05410",
                new Dictionary<string, string>() { { "id", "63" }, { "type", "2" } }
            },
            {
                "05430",
                new Dictionary<string, string>() { { "id", "64" }, { "type", "2" } }
            },
            {
                "07400",
                new Dictionary<string, string>() { { "id", "65" }, { "type", "2" } }
            },
            {
                "08400",
                new Dictionary<string, string>() { { "id", "66" }, { "type", "2" } }
            },
            {
                "11201",
                new Dictionary<string, string>() { { "id", "16" }, { "type", "1" } }
            },
            {
                "11202",
                new Dictionary<string, string>() { { "id", "16" }, { "type", "1" } }
            },
            {
                "11204",
                new Dictionary<string, string>() { { "id", "16" }, { "type", "1" } }
            },
            {
                "09999",
                new Dictionary<string, string>() { { "id", "1" }, { "type", "0" } }
            },
        };
    }
}
