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
        string csvFilename;
        public static int sectionId = 0;

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
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

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

            int sectinoIndex = sectionsIdStackPanel.Children.Count - 1 > 0 ? sectionsIdStackPanel.Children.Count - 1 : 0;
            sectionsIdStackPanel.Children.Insert(sectinoIndex, sectionIdTxtBox);

            TextBox dsdMonikerTxtBox = new TextBox();
            dsdMonikerTxtBox.Name = $"dsdMoniker{MainWindow.sectionId}";
            dsdMonikerTxtBox.HorizontalAlignment = HorizontalAlignment.Left;
            dsdMonikerTxtBox.Height = 23;
            dsdMonikerTxtBox.Width = 219;
            dsdMonikerTxtBox.Margin = new Thickness(0, 0, 0, 5);

            int dsdIndex = dsdMonikerStackPanel.Children.Count - 1 > 0 ? dsdMonikerStackPanel.Children.Count - 1 : 0;
            dsdMonikerStackPanel.Children.Insert(dsdIndex, dsdMonikerTxtBox);
        }

        private void importBtn_Click(object sender, RoutedEventArgs e)
        {
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
            int okpoRowPosition = 0;
            int soateRowPosition = 0;

            try
            {
                okpoRowPosition = int.Parse(okpoPosition.Text);
                soateRowPosition = int.Parse(soatePosition.Text);
            } catch (FormatException)
            {
                MessageBox.Show("Положения СОАТЕ и ОКПО в строке заданы неверно");
            }

            Sender senderObj = new Sender(csvLines, okpoRowPosition);

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
        }
    }
}
