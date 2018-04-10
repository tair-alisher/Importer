using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;

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

        public void BuildSenderIdsFile(System.Windows.Controls.ProgressBar progress)
        {
            progress.Minimum = 0;
            progress.Maximum = okpos.Count;
            progress.Value = 0;

            SqlCommand command;
            string data;

            string senderIdsFilePath = String.Format(@"{0}Files\sender_identifiers.json", AppDomain.CurrentDomain.BaseDirectory);
            TextWriter writer = new StreamWriter(senderIdsFilePath);
            writer.WriteLine("[");

            string connectionString = ConfigurationManager.ConnectionStrings["ServerConnection"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            foreach (string okpo in okpos)
            {
                command = new SqlCommand("SELECT Id as id FROM AspNetUsers WHERE OKPO = @okpo", connection);
                command.Parameters.AddWithValue("@okpo", okpo);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        data = $@"   {{
        ""okpo"": ""{okpo}"",
        ""sender_id"": ""{reader["id"].ToString()}""
    }},";
                        writer.WriteLine(data);
                    }
                }
                progress.Value++;
            }
                    
            connection.Close();

            writer.WriteLine("]");
            writer.Close();
        }
    }
}
