using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace View.Dialogs
{
    /// <summary>
    /// Interaction logic for IndexDialogWindow.xaml
    /// </summary>
    public partial class IndexDialogWindow : Window
    {
        /// <summary>
        /// Selected directory with documents to index
        /// </summary>
        public string SelectedDirectory
        {
            get
            {
                return DirectoryNameTextBox.Text;
            }
        }

        /// <summary>
        /// Optional selected file with stopwords
        /// </summary>
        public string? SelectedFile
        {
            get
            {
                return StopwordsTextBox.Text;
            }
        }

        /// <summary>
        /// Name of the index
        /// </summary>
        public string? IndexName
        {
            get
            {
                return IndexNameTextBox.Text;
            }
        }

        private List<string> TakenIndexNames = new();

        public IndexDialogWindow(List<string> indexNames)
        {
            InitializeComponent();
            TakenIndexNames = indexNames;
        }

        private void BrowseDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog openFileDialog = new();
            openFileDialog.Multiselect = false;
            var result = openFileDialog.ShowDialog();
            if (result == null)
            {
                return;
            }

            if (result.Value)
            {
                DirectoryNameTextBox.Text = openFileDialog.SelectedPath;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (DirectoryNameTextBox.Text == "" || DirectoryNameTextBox.Text == null)
            {
                MessageBox.Show("No directory chosen");
                return;
            }
            else if (IndexName == null || IndexName == "")
            {
                MessageBox.Show("Index name empty");
                return;
            }
            else if (TakenIndexNames.Contains(IndexName))
            {
                MessageBox.Show("Index name already used");
                return;
            }
            else
            {
                if (!DirectoryPathValid())
                {
                    return;
                }

                if (!FilePathValid())
                {
                    return;
                }

                DialogResult = true;
            }
        }

        /// <summary>
        /// Checks of the directory exists and contains any files
        /// </summary>
        /// <returns></returns>
        private bool DirectoryPathValid()
        {
            if (!Directory.Exists(SelectedDirectory))
            {
                MessageBox.Show("Directory does not exist");
                return false;
            }

            var files = Directory.GetFiles(SelectedDirectory);
            if (files.Length == 0)
            {
                MessageBox.Show("Directory contains no files");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the stopwords file exists
        /// </summary>
        /// <returns></returns>
        private bool FilePathValid()
        {
            if (SelectedFile == null || SelectedFile == "")
            {
                return true;
            }

            if (!File.Exists(SelectedFile))
            {
                MessageBox.Show("File does not exist");
                return false;
            }

            return true;
        }

        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Multiselect = false;
            var result = openFileDialog.ShowDialog();
            if (result == null)
            {
                return;
            }

            if (result.Value)
            {
                StopwordsTextBox.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Returns result of the dialog
        /// </summary>
        /// <returns></returns>
        public IndexCreationDialogResult GetResult()
        {
            // We know that the checkboxes have a value, because they are initialized to IsChecked=False in XAML
            return new()
            {
                SelectedDirectory = SelectedDirectory,
                SelectedFile = SelectedFile == "" ? null : SelectedFile,
                Stem = StemCheckBox.IsChecked.Value,
                Lowercase = LowercaseCheckBox.IsChecked.Value,
                RemoveAccents = AccentCheckBox.IsChecked.Value,
                Name = IndexName
            };
        }


        public class IndexCreationDialogResult
        {
            public string SelectedDirectory { get; set; } = "";
            public string? SelectedFile { get; set; }
            public bool Stem { get; set; }
            public bool Lowercase { get; set; }
            public bool RemoveAccents { get; set; }
            public string Name { get; set; } = "";
        }
    }
}
