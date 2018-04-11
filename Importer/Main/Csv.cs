using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml;

namespace Importer.Main
{
    class Csv
    {
        private List<string> lines = new List<string>();

        public Csv(string file)
        {
            try
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Replace(" ", "");
                        lines.Add(line);
                    }
                }
            } catch (System.ArgumentNullException)
            {
                MessageBox.Show("Укажите csv файл");
            }
        }

        public List<string> GetLines()
        {
            return lines;
        }

        public void worker_DoWork(Object sender, DoWorkEventArgs e)
        {
            int progressPercentage;

            string mapFilePath = String.Format(@"{0}Files\map.xml", AppDomain.CurrentDomain.BaseDirectory);

            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "\t";
            xmlSettings.Encoding = Encoding.UTF8;

            using (XmlWriter writer = XmlWriter.Create(mapFilePath, xmlSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Rows");

                string[] row = lines[0].Split(',');
                int rowLength = row.Length;
                for (int i = 0; i < rowLength; i++)
                {
                    writer.WriteStartElement("Row");
                    writer.WriteAttributeString("key", row[i].ToString());
                    writer.WriteAttributeString("value", i.ToString());
                    writer.WriteEndElement();

                    progressPercentage = Convert.ToInt32(((double)i / rowLength) * 100);
                    (sender as BackgroundWorker).ReportProgress(progressPercentage);
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
