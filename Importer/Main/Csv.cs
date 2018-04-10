using System;
using System.Collections.Generic;
using System.IO;

namespace Importer.Main
{
    class Csv
    {
        private List<string> lines = new List<string>();

        public Csv(string file)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                while(!reader.EndOfStream)
                {
                    string line = reader.ReadLine().Replace(" ", "");
                    lines.Add(line);
                }
            }
        }

        public List<string> GetLines()
        {
            return lines;
        }

        public void BuildMapFile(System.Windows.Controls.ProgressBar progress)
        {
            progress.Minimum = 0;
            progress.Maximum = 1;
            progress.Value = 0;

            string data;
            string mapFilePath = String.Format(@"{0}Files\map.json", AppDomain.CurrentDomain.BaseDirectory);
            TextWriter writer = new StreamWriter(mapFilePath);
            writer.WriteLine("[");

            foreach (string line in lines)
            {
                string[] row = line.Split(',');
                for (int i = 0; i < row.Length; i++)
                {
                    data = $@"   {{
        ""key"": ""%{row[i]}%"",
        ""value"": ""{i}""
    }},";
                    writer.WriteLine(data);
                }
                progress.Value++;
                break;
            }

            writer.WriteLine("]");
            writer.Close();
        }
    }
}
