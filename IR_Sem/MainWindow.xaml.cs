using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Model.Preprocessing;
using Common.Utils;
using Common.Documents.Idnes;
using Common.Documents.Basic;
using Model.Indexing;
using Common.Documents;
using System.Diagnostics;
using Model.Queries;
using System.Collections.ObjectModel;
using View.Dialogs;
using View;
using System.Threading;

namespace IR_Sem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Controller.Controller Controller { get; set; } = new();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NewIndexButton_Click(object sender, RoutedEventArgs e)
        {
            IndexDialogWindow indexDialog = new();
            if (indexDialog.ShowDialog() != true)
            {
                return;
            }

            var result = indexDialog.GetResult();

            LoadingDialog loadingDialog = new LoadingDialog();
            loadingDialog.Title = "Indexing";
            loadingDialog.Show();
            SetControlsEnabled(false);
            try
            {
                var newIndex = Controller.CreateIndex(new()
                {
                    DocumentDirectoryPath = result.SelectedDirectory,
                    StopwordsFilePath = result.SelectedFile,
                    RemoveAccents = result.RemoveAccents,
                    PerformStemming = result.Stem,
                    Lowercase = result.Lowercase,
                    Name = result.Name
                });
                Controller.SelectedIndex = newIndex;
                loadingDialog.Close();
            }
            catch (Exception ex)
            {
                loadingDialog.Close();
                MessageBox.Show(ex.Message);
            }

            SetControlsEnabled(true);
        }

        private void SetControlsEnabled(bool enabled)
        {
            SearchButton.IsEnabled = enabled;
            TrecButton.IsEnabled = enabled;
            NewIndexButton.IsEnabled = enabled;
            DeleteSelectedButton.IsEnabled = enabled;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            if (comboBox.SelectedItem == null)
            {
                return;
            }

            Controller.SelectedIndex = comboBox.SelectedItem as IIndex;
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (Controller.SelectedIndex == null)
            {
                MessageBox.Show("No index selected");
                return;
            }
            else
            {
                var res = MessageBox.Show($"Are you sure you want to delete index [{Controller.SelectedIndex}]?", "Delete index",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if (res != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            Controller.DeleteIndex(Controller.SelectedIndex);

            if (Controller.AvailableIndexes.Count != 0)
            {
                IndexComboBox.SelectedIndex = 0;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (Controller.SelectedIndex == null || Controller.AvailableIndexes.Count == 0)
            {
                MessageBox.Show("No index selected");
                return;
            }

            if (BooleanSelector.IsChecked.HasValue)
            {
                if (BooleanSelector.IsChecked.Value)
                {
                    Controller.MakeBooleanQuery(QueryBox.Text);
                    CheckResultsNotEmpty();
                    return;
                }
            }

            if (VectorSelector.IsChecked.HasValue)
            {
                if (VectorSelector.IsChecked.Value)
                {
                    Controller.MakeVectorQuery(QueryBox.Text);
                    CheckResultsNotEmpty();
                    return;
                }
            }
        }

        private void CheckResultsNotEmpty()
        {
            if (Controller.RelevantDocuments.Count == 0)
            {
                MessageBox.Show("No results found");
            }
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DocumentPreview documentPreview = new DocumentPreview(ResultsView.SelectedItem as string);
            documentPreview.Title = "Document Preview";
            documentPreview.Show();
        }

        private void TrecButton_Click(object sender, RoutedEventArgs e)
        {
            TrecDialogWindow dialog = new();
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var directory = dialog.SelectedDirectory;

            var loadingDialog = new LoadingDialog();
            loadingDialog.Title = "TREC Eval";
            loadingDialog.Show();
            SetControlsEnabled(false);
            try
            {
                Controller.RunEval(directory);
                loadingDialog.Close();
            }
            catch (Exception ex)
            {
                loadingDialog.Close();
                MessageBox.Show(ex.Message);
            }
            SetControlsEnabled(true);
        }
    }
}

