using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Importer.Main;
using System.ComponentModel;
using System.Xml;

namespace Importer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly int MaxSectionsCount = 16;
        List<string> templatesPath = new List<string>();
        List<string> csvLines;

        int okpoRowPosition;
        int soateRowPosition;

        string csvFilename;
        private static int sectionId = 0;
        private static int sectionNumber = 1;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void templatesBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "Xml files (*.xml)|*.xml";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (dialog.ShowDialog() == true)
            {
                foreach (string filename in dialog.FileNames)
                {
                    this.templatesPath.Add(filename);
                    xmlTemplates.Items.Add(filename);
                }
            }
        }

        private void csvFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "Csv files (*.csv)|*.csv",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog() == true)
                csvFilePath.Text = dialog.FileName;
            csvFilename = dialog.FileName;
        }

        private void addSectionBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateSectionsUI();

            if (MainWindow.sectionNumber == MainWindow.MaxSectionsCount)
                addSectionBtn.IsEnabled = false;
        }

        private void loadSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "Xml files (*.xml)|*.xml",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            string settingsFile = "";
            if (dialog.ShowDialog() == true)
                settingsFile = dialog.FileName;

            if (!String.IsNullOrEmpty(settingsFile))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(settingsFile);

                XmlNode config = xmlDoc.DocumentElement.SelectSingleNode("/Configuration/Config");

                formId.Text = config.Attributes["formId"].Value;
                period.Text = config.Attributes["period"].Value;
                okpoPosition.Text = config.Attributes["okpoPosition"].Value;
                soatePosition.Text = config.Attributes["soatePosition"].Value;

                XmlNodeList sections = xmlDoc.DocumentElement.SelectNodes("/Configuration/Sections/Section");

                sectionId1.Text = sections[0].Attributes["id"].Value;
                dsdMoniker1.Text = sections[0].Attributes["dsdMoniker"].Value;

                for (int i = 1; i < sections.Count; i++)
                    CreateSectionsUI(sections[i].Attributes["id"].Value, sections[i].Attributes["dsdMoniker"].Value);
            }
        }

        private void CreateSectionsUI(string sectionIdText = "", string dsdMonikerText = "")
        {
            MainWindow.sectionId++;
            string sectionIdTextBoxName = $"sectionId{MainWindow.sectionId}";
            double sectionIdTextBoxWidth = 105;
            TextBox sectionIdTextBox = this.CreateTextBox(sectionIdTextBoxName, sectionIdTextBoxWidth, sectionIdText);

            int sectionIndex = sectionIdsStackPanel.Children.Count;
            sectionIdsStackPanel.Children.Insert(sectionIndex, sectionIdTextBox);

            string dsdMonikerTextBoxName = $"dsdMoniker{MainWindow.sectionId}";
            double dsdMonikerTextBoxWidth = 219;
            TextBox dsdMonikerTextBox = this.CreateTextBox(dsdMonikerTextBoxName, dsdMonikerTextBoxWidth, dsdMonikerText);

            int dsdIndex = dsdMonikerStackPanel.Children.Count;
            dsdMonikerStackPanel.Children.Insert(dsdIndex, dsdMonikerTextBox);

            MainWindow.sectionNumber++;
            Label sectionNumberLbl = new Label()
            {
                Content = MainWindow.sectionNumber.ToString(),
                Height = 23,
                Width = 27,
                Margin = new Thickness(0, 0, 0, 5)
            };

            int sectionNumberIndex = sectionNumberStackPanel.Children.Count;
            sectionNumberStackPanel.Children.Insert(sectionNumberIndex, sectionNumberLbl);
        }

        private void saveSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog()
            {
                FileName = "Config",
                DefaultExt = ".xml",
                Filter = "Xml documents (.xml)|*.xml"
            };

            string filename = "";
            if ((bool)saveDialog.ShowDialog())
                filename = saveDialog.FileName;

            if (!String.IsNullOrEmpty(filename))
            {
                string formIdValue = ((TextBox)formId).Text;
                string periodValue = ((TextBox)period).Text;
                string okpoPositionValue = ((TextBox)okpoPosition).Text;
                string soatePositionValue = ((TextBox)soatePosition).Text;
                List<string> sectionIdsList = this.FormListFromUICollection(sectionIdsStackPanel.Children);
                List<string> dsdMonikersList = this.FormListFromUICollection(dsdMonikerStackPanel.Children);

                XmlWriterSettings xmlSettings = Main.Main.CustomizedXmlWriterSettingsInstance();

                using (XmlWriter writer = XmlWriter.Create(filename, xmlSettings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Configuration");

                    writer.WriteStartElement("Config");
                    writer.WriteAttributeString("formId", formIdValue);
                    writer.WriteAttributeString("period", periodValue);
                    writer.WriteAttributeString("okpoPosition", okpoPositionValue);
                    writer.WriteAttributeString("soatePosition", soatePositionValue);
                    writer.WriteEndElement();

                    writer.WriteStartElement("Sections");

                    if (sectionIdsList.Count == dsdMonikersList.Count)
                    {
                        for (int i = 0; i < sectionIdsList.Count; i++)
                        {
                            writer.WriteStartElement("Section");
                            writer.WriteAttributeString("id", sectionIdsList[i]);
                            writer.WriteAttributeString("dsdMoniker", dsdMonikersList[i]);
                            writer.WriteEndElement();
                        }
                    }
                    else
                    {
                        writer.WriteStartElement("Error");
                        writer.WriteString("Ошибка. Количество идентификаторов разделов и показателей DsdMoniker должно быть одинаковым.");
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
        }

        private void importBtn_Click(object sender, RoutedEventArgs e)
        {
            csvProgressBar.Value = 0;
            senderProgressBar.Value = 0;
            xmlDataProgressBar.Value = 0;
            importProgressBar.Value = 0;

            try
            {
                this.okpoRowPosition = int.Parse(okpoPosition.Text);
                this.soateRowPosition = int.Parse(soatePosition.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Положения СОАТЕ и ОКПО в строке заданы неверно");
            }

            Csv csv = new Csv(csvFilename);
            csvLines = csv.GetLines();
            BackgroundWorker csvWorker = new BackgroundWorker();
            csvWorker.WorkerReportsProgress = true;
            csvWorker.DoWork += csv.worker_DoWork;
            csvWorker.ProgressChanged += (senderObject, arguments) =>
            {
                csvProgressBar.Value = arguments.ProgressPercentage;
            };
            csvWorker.RunWorkerCompleted += worker_RunSenderWorker;
            csvWorker.RunWorkerAsync();
        }

        void worker_RunSenderWorker(object sender, RunWorkerCompletedEventArgs e)
        {
            mapStatusLbl.Content = "ok";

            Sender senderObj = new Sender(csvLines, this.okpoRowPosition);

            BackgroundWorker senderWorker = new BackgroundWorker();
            senderWorker.WorkerReportsProgress = true;
            senderWorker.DoWork += senderObj.worker_DoWork;
            senderWorker.ProgressChanged += (senderObject, arguments) =>
            {
                senderProgressBar.Value = arguments.ProgressPercentage;
            };
            senderWorker.RunWorkerCompleted += worker_RunBuildXmlWorker;
            senderWorker.RunWorkerAsync();
        }

        void worker_RunBuildXmlWorker(object sender, RunWorkerCompletedEventArgs e)
        {
            senderStatusLbl.Content = "ok";

            List<string> xmlFiles = new List<string>();

            foreach (object item in xmlTemplates.Items)
                xmlFiles.Add(item.ToString());

            XmlFormer xmlFormer = new XmlFormer(csvLines, xmlFiles)
            {
                okpoRowPosition = this.okpoRowPosition,
                soateRowPosition = this.soateRowPosition,
                FormId = formId.Text,
                Period = period.Text
            };

            List<string> sectionIdsList = this.FormListFromUICollection(sectionIdsStackPanel.Children);
            List<string> dsdMonikersList = this.FormListFromUICollection(dsdMonikerStackPanel.Children);

            xmlFormer.SectionIds = sectionIdsList;
            xmlFormer.DsdMonikers = dsdMonikersList;

            BackgroundWorker xmlWorker = new BackgroundWorker() { WorkerReportsProgress = true };
            xmlWorker.DoWork += xmlFormer.worker_DoWork;
            xmlWorker.ProgressChanged += (senderObject, arguments) =>
            {
                xmlDataProgressBar.Value = arguments.ProgressPercentage;
            };
            xmlWorker.RunWorkerCompleted += worker_RunImportWorker;
            xmlWorker.RunWorkerAsync();
        }

        private void worker_RunImportWorker(object sender, RunWorkerCompletedEventArgs e)
        {
            xmldataStatusLbl.Content = "ok";

            Import import = new Import();

            BackgroundWorker importWorker = new BackgroundWorker() { WorkerReportsProgress = true };
            importWorker.DoWork += import.ImportWorker;
            importWorker.ProgressChanged += (senderObject, arguments) =>
            {
                importProgressBar.Value = arguments.ProgressPercentage;
            };
            importWorker.RunWorkerCompleted += OnWorkerFinish;
            importWorker.RunWorkerAsync();
        }

        private void OnWorkerFinish(Object sender, RunWorkerCompletedEventArgs e)
        {
            importStatusLbl.Content = "ok";
            MessageBox.Show("Заврешено");
        }

        private List<string> FormListFromUICollection(UIElementCollection elementCollection)
        {
            List<string> list = new List<string>();
            foreach (UIElement element in elementCollection)
            {
                try
                {
                    list.Add(((TextBox)element).Text);
                }
                catch (FormatException) { continue; }
            }
            return list;
        }

        private TextBox CreateTextBox(string name, double width, string text = "")
        {
            TextBox textBox = new TextBox()
            {
                Name = name,
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = 23,
                Width = width,
                Margin = new Thickness(0, 0, 0, 5),
                Text = text
            };

            return textBox;
        }
    }
}
