using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using Importer.Main;
using System.ComponentModel;

namespace Importer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> templatesPath = new List<string>();
        List<string> csvLines;

        Dictionary<string, string> staticData;

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
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

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
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Csv files (*.csv)|*.csv";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (dialog.ShowDialog() == true)
                csvFilePath.Text = dialog.FileName;
            csvFilename = dialog.FileName;
        }

        private void addSectionBtn_Click(object sender, RoutedEventArgs e)
        {
            TextBox sectionIdTxtBox = new TextBox();
            MainWindow.sectionId++;
            sectionIdTxtBox.Name = $"sectionId{MainWindow.sectionId}";
            sectionIdTxtBox.HorizontalAlignment = HorizontalAlignment.Left;
            sectionIdTxtBox.Height = 23;
            sectionIdTxtBox.Width = 105;
            sectionIdTxtBox.Margin = new Thickness(0, 0, 0, 5);

            int sectinoIndex = sectionIdsStackPanel.Children.Count;
            sectionIdsStackPanel.Children.Insert(sectinoIndex, sectionIdTxtBox);

            TextBox dsdMonikerTxtBox = new TextBox();
            dsdMonikerTxtBox.Name = $"dsdMoniker{MainWindow.sectionId}";
            dsdMonikerTxtBox.HorizontalAlignment = HorizontalAlignment.Left;
            dsdMonikerTxtBox.Height = 23;
            dsdMonikerTxtBox.Width = 219;
            dsdMonikerTxtBox.Margin = new Thickness(0, 0, 0, 5);

            int dsdIndex = dsdMonikerStackPanel.Children.Count;
            dsdMonikerStackPanel.Children.Insert(dsdIndex, dsdMonikerTxtBox);

            Label sectionNumberLbl = new Label();
            MainWindow.sectionNumber++;
            sectionNumberLbl.Content = MainWindow.sectionNumber.ToString();
            sectionNumberLbl.Height = 23;
            sectionNumberLbl.Width = 27;
            sectionNumberLbl.Margin = new Thickness(0, 0, 0, 5);

            int sectionNumberIndex = sectionNumberStackPanel.Children.Count;
            sectionNumberStackPanel.Children.Insert(sectionNumberIndex, sectionNumberLbl);
        }

        private void importBtn_Click(object sender, RoutedEventArgs e)
        {                
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
            csvProgressBar.Value = 0;
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
            senderProgressBar.Value = 0;
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

            XmlFormer xmlFormer = new XmlFormer(csvLines);

            xmlFormer.okpoRowPosition = this.okpoRowPosition;
            xmlFormer.soateRowPosition = this.soateRowPosition;
            xmlFormer.FormId = formId.Text;
            xmlFormer.Period = period.Text;

            List<string> sectionIds = new List<string>();
            List<string> dsdMonikers = new List<string>();

            foreach (UIElement sectionId in sectionIdsStackPanel.Children)
            {
                try
                {
                    sectionIds.Add(((TextBox) sectionId).Text);
                } catch (FormatException) { continue; }
            }

            foreach (UIElement dsdMoniker in dsdMonikerStackPanel.Children)
            {
                try
                {
                    dsdMonikers.Add(((TextBox) dsdMoniker).Text);
                } catch (FormatException) { continue; }
            }

            xmlFormer.SectionIds = sectionIds;
            xmlFormer.DsdMonikers = dsdMonikers;

            this.staticData = xmlFormer.FormStaticData();

            BackgroundWorker xmlWorker = new BackgroundWorker();
            xmlDataProgressBar.Value = 0;
            xmlWorker.WorkerReportsProgress = true;
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
        }
    }
}
