using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Windows.Controls;
using System.Xml;
using System.Text;

namespace Importer.Main
{
    class Sender
    {
        List<string> okpos = new List<string>();
        Dictionary<string, string> senderIdentifiers = new Dictionary<string, string>();

        public Sender(List<string> lines, int okpoRowPosition)
        {
            for (int i = 1; i < lines.Count; i++)
            {
                string[] row = lines[i].Split(',');
                this.okpos.Add(row[okpoRowPosition]);
            }
        }

        public void worker_DoWork(Object sender, DoWorkEventArgs e)
        {
            SqlCommand command;
            int progressPercentage;
            int okposCount = okpos.Count;

            string senderIdsFilePath = String.Format(@"{0}Files\sender_identifiers.xml", AppDomain.CurrentDomain.BaseDirectory);

            string connectionString = ConfigurationManager.ConnectionStrings["ServerConnection"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "\t";
            xmlSettings.Encoding = Encoding.UTF8;

            using (XmlWriter writer = XmlWriter.Create(senderIdsFilePath, xmlSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Rows");

                for (int i = 0; i < okposCount; i++)
                {
                    command = new SqlCommand("SELECT Id as id FROM AspNetUsers WHERE OKPO = @okpo", connection);
                    command.Parameters.AddWithValue("@okpo", okpos[i]);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            writer.WriteStartElement("Row");
                            writer.WriteAttributeString("okpo", okpos[i].ToString());
                            writer.WriteAttributeString("id", reader["id"].ToString());
                            writer.WriteEndElement();

                            progressPercentage = Convert.ToInt32(((double)i / okposCount) * 100);
                            (sender as BackgroundWorker).ReportProgress(progressPercentage);
                        }
                    }
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            connection.Close();
        }
    }
}
