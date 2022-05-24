using Ookii.Dialogs.Wpf;
using System.IO;
using System.Linq;
using System.Windows;

namespace View.Dialogs
{
    /// <summary>
    /// Interaction logic for TrecDialogWindow.xaml
    /// </summary>
    public partial class TrecDialogWindow : Window
    {
        /// <summary>
        /// Selected directory
        /// </summary>
        public string SelectedDirectory
        {
            get
            {
                return DirectoryNameTextBox.Text;
            }
        }

        public TrecDialogWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Effectively OKButton clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EvalButton_Click(object sender, RoutedEventArgs e)
        {
            if (DirectoryNameTextBox.Text == "" || DirectoryNameTextBox.Text == null)
            {
                MessageBox.Show("No directory chosen");
                return;
            }


            if (!DirectoryPathValid())
            {
                return;
            }

            DialogResult = true;
        }

        /// <summary>
        /// Check if the directory exists and contains directories "topics" and "documents"
        /// </summary>
        /// <returns></returns>
        private bool DirectoryPathValid()
        {
            if (!Directory.Exists(SelectedDirectory))
            {
                MessageBox.Show("Directory does not exist");
                return false;
            }

            var files = Directory.GetDirectories(SelectedDirectory);
            if (files.Length == 0)
            {
                MessageBox.Show("Directory contains no files");
                return false;
            }

            if (!files.Any(f => f.Contains("topics")))
            {
                MessageBox.Show("topics directory not found");
                return false;
            }

            if (!files.Any(f => f.Contains("documents")))
            {
                MessageBox.Show("documents directory not found");
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
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
    }
}
