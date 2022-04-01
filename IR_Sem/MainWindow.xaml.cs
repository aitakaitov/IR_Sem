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


namespace IR_Sem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Stopwords sw = new Stopwords();
            Tokenizer t = new Tokenizer(sw);
            Stemmer stemmer = new Stemmer();
            Analyzer a = new Analyzer(t, stemmer, sw, new AnalyzerConfig(true, true, true));
            var documents = DocumentLoaderJson.Load<Article>("C:/Users/aitak/Desktop/crawled_data_test/json");
            foreach (Article article in documents)
            {
                var tokens = a.Preprocess(article);
                Console.WriteLine(tokens);
            }
            InitializeComponent();
        }
    }
}
