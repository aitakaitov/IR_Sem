using Controller.Enums;
using Model.Indexing;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using View;
using View.Dialogs;

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
        private LoadingDialog LoadingDialog { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            Controller.SetCallbacks(SetQueryType, SetQueryString);
        }

        /// <summary>
        /// Callback for the Controller to be able to set the displayed query string
        /// </summary>
        /// <param name="s"></param>
        private void SetQueryString(string s)
        {
            QueryBox.Text = s;
        }

        /// <summary>
        /// Callback for the Controller to be able to set the displayed query type
        /// </summary>
        /// <param name="type"></param>
        private void SetQueryType(EQueryType type)
        {
            switch (type)
            {
                case EQueryType.BOOLEAN:
                    BooleanSelector.IsChecked = true;
                    break;
                case EQueryType.VECTOR:
                    VectorSelector.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        private void NewIndexButton_Click(object sender, RoutedEventArgs e)
        {
            IndexDialogWindow indexDialog = new(Controller.GetAvailableIndexesNames());
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

        /// <summary>
        /// Enables or disables all buttons
        /// </summary>
        /// <param name="enabled"></param>
        private void SetControlsEnabled(bool enabled)
        {
            SearchButton.IsEnabled = enabled;
            TrecButton.IsEnabled = enabled;
            NewIndexButton.IsEnabled = enabled;
            DeleteSelectedButton.IsEnabled = enabled;
        }

        /// <summary>
        /// When the index combobox selection changes, propagate the change to Controller
        /// Update the query history
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox? comboBox = sender as ComboBox;
            if (comboBox == null)
            {
                return;
            }

            if (comboBox.SelectedItem == null)
            {
                return;
            }

            Controller.SelectedIndex = comboBox.SelectedItem as AIndex;
            Controller.UpdateHistory();
        }

        /// <summary>
        /// When a query is selected in query history, notify controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void History_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox? comboBox = sender as ComboBox;
            if (comboBox == null)
            {
                return;
            }

            if (comboBox.SelectedItem == null)
            {
                return;
            }

            Controller.SetToHistory(comboBox.SelectedItem);
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

            if (QueryBox.Text == null || QueryBox.Text.Length == 0)
            {
                MessageBox.Show("Empty query");
                return;
            }

            if (BooleanSelector.IsChecked.HasValue)
            {
                if (BooleanSelector.IsChecked.Value)
                {
                    Controller.MakeBooleanQuery(QueryBox.Text);
                    HistoryComboBox.SelectedIndex = 0;
                    CheckResultsNotEmpty();
                    return;
                }
            }

            if (VectorSelector.IsChecked.HasValue)
            {
                if (VectorSelector.IsChecked.Value)
                {
                    Controller.MakeVectorQuery(QueryBox.Text);
                    HistoryComboBox.SelectedIndex = 0;
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

        /// <summary>
        /// On doubleclick on a result in results view, pop up a full view of the document
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsView.SelectedItem == null)
            {
                return;
            }

            var documentContent = (string)ResultsView.SelectedItem;

            DocumentPreview documentPreview = new DocumentPreview(documentContent);
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

