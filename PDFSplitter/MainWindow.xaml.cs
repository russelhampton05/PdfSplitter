using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace PDFSplitter
{ 
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _filePath;
        private string _destinationDirectory;
        private string _pageCounts;
        public string filePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }
        public string pageCounts
        {
            get => _pageCounts;
            set
            {
                _pageCounts = value;
                OnPropertyChanged();
            }
        }
        public string destinationDirectory
        {
            get => _destinationDirectory;
            set
            {
                _destinationDirectory = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            pageCounts = "";
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SelectFileClick(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePath = dialog.FileName;
            }
        }


        private void SelectDirectoryClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                destinationDirectory = dialog.SelectedPath;
            }
        }


        private void CreateClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(destinationDirectory))
            {
                return;
            }

            // Open the file
            PdfDocument inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            var pages = pageCounts.Split(',').Where(page => !string.IsNullOrEmpty(page)).ToList();
            var fileCount = 0;
            var idx = 0;
            while (idx < inputDocument.PageCount)
            {
                // Create new document
                PdfDocument outputDocument = new PdfDocument();
                outputDocument.Version = inputDocument.Version;
                outputDocument.Info.Creator = inputDocument.Info.Creator;

                // Add the page and save it
                if (pages.Count > fileCount)
                {
                    var pageCount = int.Parse(pages[fileCount]);
                    for (int x = 0; x < pageCount; x++)
                    {
                        outputDocument.AddPage(inputDocument.Pages[idx]);
                        idx++;
                    }
                }
                else
                {
                    outputDocument.AddPage(inputDocument.Pages[idx]);
                    idx++;
                }

                outputDocument.Save(Path.Combine(destinationDirectory, $"{idx}_{Path.GetFileName(filePath)}"));
                fileCount++;
            }
        }

        private void SelectDirectoryCombineClick(object sender, RoutedEventArgs e)
        {
            SelectDirectoryClick(sender, e);
        }

        private void CreateCombinedClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(destinationDirectory))
            {
                return;
            }
            var directory = new DirectoryInfo(destinationDirectory);
            var files = directory.GetFiles();
            var pdfPages = new List<PdfPage>();
          //  var openPdfStreams = List<PdfDocument>();

            foreach (var file in files)
            {
                var pdf = PdfReader.Open(file.FullName, PdfDocumentOpenMode.Import);
                for (var x = 0; x < pdf.Pages.Count; x++)
                {
                    pdfPages.Add(pdf.Pages[x]);
                }
            }

            PdfDocument document = new PdfDocument();
            foreach (var page in pdfPages)
            {
                document.AddPage(page);
            }
            document.Save(directory + "/combined.pdf");
        }
    }
}
