using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace View.Dialogs
{
    /// <summary>
    /// Interaction logic for IndexDialogWindow.xaml
    /// </summary>
    public partial class IndexDialogWindow : Window
    {
        public string SelectedDirectory
        {
            get
            {
                return DirectoryNameTextBox.Text;
            }
        }

        public string? SelectedFile
        {
            get
            {
                return StopwordsTextBox.Text;
            }
        }

        public string? IndexName
        {
            get
            {
                return IndexNameTextBox.Text;
            }
        }

        public IndexDialogWindow()
        {
            InitializeComponent();
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
            public string SelectedDirectory { get; set; }
            public string? SelectedFile { get; set; }
            public bool Stem { get; set; }
            public bool Lowercase { get; set; }
            public bool RemoveAccents { get; set; }
            public string Name { get; set; }
        }
    }
}
