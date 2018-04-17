using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace Importer.Main
{
    class Import
    {
        private static readonly string okpoXmlSectionsSelectQuery = "SELECT XmlContent as xml FROM OkpoXmlSections WHERE Okpo = @okpo";

        private static readonly string usersInfoInsertQuery = "INSERT INTO UsersInfo(OKPO,User_ID) VALUES(@okpo,@userId);SELECT SCOPE_IDENTITY()";
        private static readonly string messagesStatusesInsertQuery = "INSERT INTO MessagesStatuses(Form_ID,UsersInfo_ID,PeriodType,DepartmentType,DateTime) VALUES(@formId,@usersInfoId,@periodType,@departmentType,@dateTime);SELECT SCOPE_IDENTITY()";
        private static readonly string respondentDatasInsertQuery = "INSERT INTO RespondentDatas(MessageStatusID,SoateCode) VALUES(@messageStatusId,@soateCode);SELECT SCOPE_IDENTITY()";
        private static readonly string messagesInsertQuery = "INSERT INTO Messages(Message_XML,Section_ID,MessagesStatuses_ID,DSDMoniker) VALUES(@messageXml,@sectionId,@messagesStatusesId,@dsdMoniker)";

        public void ImportWorker(object sender, DoWorkEventArgs e)
        {
            SqlCommand command;
            SqlCommand selectCommand;
            SqlCommand insertCommand;

            SqlDataReader reader;
            SqlDataReader selectReader;
            int progressPercentage;

            Dictionary<string, string> formInfo = this.GetFormInfoFromStaticDataXml();
            List<Dictionary<string, string>> sectionsInfo = this.GetSectionsInfoFromStaticDataXml();

            List<Dictionary<string, string>> usersData = this.GetUsersDataFromDataXml();
            int usersDataCount = usersData.Count;

            string localConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string uploadConnectionString = ConfigurationManager.ConnectionStrings["UploadConnection"].ConnectionString;

            SqlConnection localConnection = new SqlConnection(localConnectionString);
            localConnection.Open();

            SqlConnection uploadConnection = new SqlConnection(uploadConnectionString);

            string lastMessagesId = "";
            string lastMessagesIdSelectQuery = "SELECT TOP(1) ID as id FROM Messages ORDER BY 1 DESC";
            command = new SqlCommand(lastMessagesIdSelectQuery, uploadConnection);

            string okpo;
            int lastUsersInfoId = 0;
            int lastMessagesStatusesId = 0;
            int sectionCounter = 0;
            for (int i = 0; i < usersDataCount; i++)
            {
                okpo = usersData[i]["okpo"];

                using (insertCommand = new SqlCommand(Import.usersInfoInsertQuery))
                {
                    insertCommand.Connection = uploadConnection;
                    insertCommand.CommandType = CommandType.Text;
                    insertCommand.Parameters.AddWithValue("@okpo", okpo);
                    insertCommand.Parameters.AddWithValue("@userId", usersData[i]["userId"]);

                    try
                    {
                        uploadConnection.Open();
                        insertCommand.ExecuteNonQuery();
                        lastUsersInfoId = Convert.ToInt32(insertCommand.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine();
                    }
                    finally
                    {
                        insertCommand.Dispose();
                        uploadConnection.Close();
                    }
                }

                using (insertCommand = new SqlCommand(Import.messagesStatusesInsertQuery))
                {
                    insertCommand.Connection = uploadConnection;
                    insertCommand.CommandType = CommandType.Text;
                    DateTime dateTime = DateTime.ParseExact(formInfo["datetime"], "yyyy-MM-dd HH:mm:ss.fff",
                                       System.Globalization.CultureInfo.InvariantCulture);

                    insertCommand.Parameters.AddWithValue("@formId", formInfo["formId"]);
                    insertCommand.Parameters.AddWithValue("@usersInfoId", lastUsersInfoId);
                    insertCommand.Parameters.AddWithValue("@periodType", formInfo["period"]);
                    insertCommand.Parameters.AddWithValue("@departmentType", usersData[i]["departmentType"]);
                    insertCommand.Parameters.AddWithValue("@dateTime", dateTime);

                    try
                    {
                        uploadConnection.Open();
                        insertCommand.ExecuteNonQuery();
                        lastMessagesStatusesId = Convert.ToInt32(insertCommand.ExecuteScalar());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine();
                    }
                    finally
                    {
                        insertCommand.Dispose();
                        uploadConnection.Close();
                    }
                }

                using (insertCommand = new SqlCommand(Import.respondentDatasInsertQuery))
                {
                    insertCommand.Connection = uploadConnection;
                    insertCommand.CommandType = CommandType.Text;

                    insertCommand.Parameters.AddWithValue("@messageStatusId", lastMessagesStatusesId);
                    insertCommand.Parameters.AddWithValue("@soateCode", usersData[i]["soate"]);

                    try
                    {
                        uploadConnection.Open();
                        insertCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine();
                    }
                    finally
                    {
                        insertCommand.Dispose();
                        uploadConnection.Close();
                    }
                }

                sectionCounter = 0;
                selectCommand = new SqlCommand(Import.okpoXmlSectionsSelectQuery, localConnection);
                selectCommand.Parameters.AddWithValue("@okpo", okpo);
                using (selectReader = selectCommand.ExecuteReader())
                {
                    while (selectReader.Read())
                    {
                        using (insertCommand = new SqlCommand(Import.messagesInsertQuery))
                        {
                            uploadConnection.Open();
                            using (reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                    lastMessagesId = (Convert.ToInt32(reader["id"]) + 1).ToString();
                                else
                                    lastMessagesId = "0";
                            }
                            uploadConnection.Close();

                            insertCommand.Connection = uploadConnection;
                            insertCommand.CommandType = CommandType.Text;

                            string xml = selectReader["xml"].ToString().Replace("%messageIdentifier%", lastMessagesId);

                            insertCommand.Parameters.AddWithValue("@messageXml", xml);
                            insertCommand.Parameters.AddWithValue("@sectionId", sectionsInfo[sectionCounter]["id"]);
                            insertCommand.Parameters.AddWithValue("@messagesStatusesId", lastMessagesStatusesId);
                            insertCommand.Parameters.AddWithValue("@dsdMoniker", sectionsInfo[sectionCounter]["dsdMoniker"]);

                            try
                            {
                                uploadConnection.Open();
                                insertCommand.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine();
                                Console.WriteLine(ex.ToString());
                                Console.WriteLine();
                            }
                            finally
                            {
                                insertCommand.Dispose();
                                uploadConnection.Close();
                            }
                        }
                        sectionCounter++;
                    }
                }

                progressPercentage = Convert.ToInt32(((double)i + 1 / usersDataCount) * 100);
                (sender as BackgroundWorker).ReportProgress(progressPercentage);
            }

            localConnection.Close();
        }

        private List<Dictionary<string, string>> GetSectionsInfoFromStaticDataXml()
        {
            List<Dictionary<string, string>> sectionsInfo = new List<Dictionary<string, string>>();

            string staticDataXmlFile = String.Format(@"{0}Files\staticData.xml", AppDomain.CurrentDomain.BaseDirectory);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(staticDataXmlFile);

            XmlNodeList rows = xmlDoc.DocumentElement.SelectNodes("/Rows/Sections/Section");

            foreach (XmlNode row in rows)
                sectionsInfo.Add(
                    new Dictionary<string,string>()
                    {
                        { "id", row.Attributes["id"].Value },
                        { "dsdMoniker", row.Attributes["dsdMoniker"].Value }
                    }
                    );

            return sectionsInfo;

        }

        private Dictionary<string, string> GetFormInfoFromStaticDataXml()
        {
            string staticDataXmlFile = String.Format(@"{0}Files\staticData.xml", AppDomain.CurrentDomain.BaseDirectory);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(staticDataXmlFile);

            XmlNode row = xmlDoc.DocumentElement.SelectSingleNode("/Rows/FormInfo");

            Dictionary<string, string> formInfo = new Dictionary<string, string>()
            {
                { "formId", row.Attributes["formId"].Value },
                { "period", row.Attributes["period"].Value },
                { "datetime", row.Attributes["datetime"].Value }
            };

            return formInfo;
        }

        private List<Dictionary<string, string>> GetUsersDataFromDataXml()
        {
            List<Dictionary<string, string>> usersData = new List<Dictionary<string, string>>();

            string dataXmlFilePath = String.Format(@"{0}Files\data.xml", AppDomain.CurrentDomain.BaseDirectory);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(dataXmlFilePath);

            XmlNodeList rows = xmlDocument.DocumentElement.SelectNodes("/Rows/Row");

            foreach (XmlNode row in rows)
                usersData.Add(
                    new Dictionary<string, string>()
                    {
                        { "okpo", row.Attributes["okpo"].Value },
                        { "soate", row.Attributes["soate"].Value },
                        { "userId", row.Attributes["user_id"].Value },
                        { "departmentType", row.Attributes["departmentType"].Value }
                    });

            return usersData;
        }
    }
}
