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
using System.Windows.Threading;

namespace IR_Sem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Controller.Controller Controller { get; set; } = new();

        /// <summary>
        /// Loading dialog window which is shown when creating index or running eval
        /// </summary>
        private LoadingDialog LoadingDialog { get; set; }

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

            LoadingDialog = new LoadingDialog();
            LoadingDialog.Title = "Indexing";
            LoadingDialog.Show();

            SetControlsEnabled(false);

            Thread thread = new Thread(() => CreateIndexThread(result));
            thread.Start();
        }

        /// <summary>
        /// Thread method that takes care of new index creating without
        /// blocking the UI, which would prevent the loading dialog window from rendering
        /// </summary>
        /// <param name="result"></param>
        private void CreateIndexThread(IndexDialogWindow.IndexCreationDialogResult result)
        {
            try
            {
                var res = Controller.CreateIndex(new()
                {
                    DocumentDirectoryPath = result.SelectedDirectory,
                    StopwordsFilePath = result.SelectedFile,
                    RemoveAccents = result.RemoveAccents,
                    PerformStemming = result.Stem,
                    Lowercase = result.Lowercase,
                    Name = result.Name
                });

                Dispatcher.Invoke(() => Controller.AvailableIndexes.Add(res));
                Controller.SelectedIndex = res;
                Dispatcher.Invoke(() => SetControlsEnabled(true));
                Dispatcher.Invoke(() => LoadingDialog.Close());
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LoadingDialog.Close());
                MessageBox.Show(ex.Message);
                Dispatcher.Invoke(() => SetControlsEnabled(true));
                return;
            }
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

            LoadingDialog = new LoadingDialog();
            LoadingDialog.Title = "TREC Eval";
            LoadingDialog.Show();

            SetControlsEnabled(false);

            Thread t = new Thread(() => TrecEvalThread(directory));
            t.Start();
        }

        /// <summary>
        /// Thread method that handles the TREC evaluation. Allows for the 
        /// loading dialog window to be displayed.
        /// </summary>
        /// <param name="dir"></param>
        private void TrecEvalThread(string dir)
        {
            try
            {
                Controller.RunEval(dir);
                Dispatcher.Invoke(() => LoadingDialog.Close());
                Dispatcher.Invoke(() => SetControlsEnabled(true));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LoadingDialog.Close());
                MessageBox.Show(ex.Message);
                Dispatcher.Invoke(() => SetControlsEnabled(true));
            }
        }
    }
}

