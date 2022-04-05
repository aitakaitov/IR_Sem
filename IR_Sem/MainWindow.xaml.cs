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

namespace IR_Sem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            TestSet1();
            TestSet2();
            TestMyData();
        }

        private static void TestSet1()
        {
            Debug.WriteLine("--------------------------------------\nDataset 1\n--------------------------------------");
            // Index and query dataset 1
            List<IDocument> list = new List<IDocument>();
            list.Add(new Document() { Text = "Plzeň je krásné město a je to krásné místo" });
            list.Add(new Document() { Text = "Ostrava je ošklivé místo" });
            list.Add(new Document() { Text = "Praha je také krásné město Plzeň je hezčí" });
            Stopwords sw = new Stopwords();
            Tokenizer tokenizer = new Tokenizer(sw);
            Stemmer stemmer = new Stemmer();
            Analyzer analyzer = new Analyzer(tokenizer, stemmer, sw, new AnalyzerConfig() { Lowercase = true, PerformStemming = false, RemoveAccents = false });
            InvertedIndex index = new InvertedIndex(analyzer, new IndexConfig() { BertSimilarity = false });
            index.Index(list);
            Trace.WriteLine("-- Dataset 1 inverted index");
            index.PrintIndex();
            Trace.WriteLine("- Query 1: krásné město, top count = 2");
            List<IDocument> documents = index.VectorSpaceSearch(new VectorQuery() { QueryText = "krásné město", TopCount = 2 });
            PrintDocuments(documents);
        }



        private static void TestSet2()
        {
            Debug.WriteLine("--------------------------------------\nDataset 2\n--------------------------------------");
            // Index and query dataset 1
            List<IDocument> list = new List<IDocument>();
            list.Add(new Document() { Text = "tropical fish include fish found in tropical enviroments" });
            list.Add(new Document() { Text = "fish live in a sea" });
            list.Add(new Document() { Text = "tropical fish are popular aquarium fish" });
            list.Add(new Document() { Text = "fish also live in Czechia" });
            list.Add(new Document() { Text = "Czechia is a country" });
            Stopwords sw = new Stopwords();
            Tokenizer tokenizer = new Tokenizer(sw);
            Stemmer stemmer = new Stemmer();
            Analyzer analyzer = new Analyzer(tokenizer, stemmer, sw, new AnalyzerConfig() { Lowercase = true, PerformStemming = false, RemoveAccents = false });
            InvertedIndex index = new InvertedIndex(analyzer, new IndexConfig() { BertSimilarity = false });
            index.Index(list);
            Trace.WriteLine("-- Dataset 2 inverted index");
            index.PrintIndex();
            Trace.WriteLine("- Query 1: tropical fish sea, top count = 2");
            List<IDocument> documents = index.VectorSpaceSearch(new VectorQuery() { QueryText = "tropical fish sea", TopCount = 2 });
            PrintDocuments(documents);
            Trace.WriteLine("- Query 2: tropical fish, top count = 2");
            documents = index.VectorSpaceSearch(new VectorQuery() { QueryText = "tropical fish", TopCount = 2 });
            PrintDocuments(documents);
        }

        private static void TestMyData()
        {
            Debug.WriteLine("--------------------------------------\nMy Dataset\n--------------------------------------");
            List<IDocument> list = new();
            list.AddRange(DocumentLoaderJson.Load<Article>("C:/Users/aitak/Desktop/crawled_data/json").GetRange(0, 1000));
            Stopwords sw = new Stopwords();
            Tokenizer tokenizer = new Tokenizer(sw);
            Stemmer stemmer = new Stemmer();
            Analyzer analyzer = new Analyzer(tokenizer, stemmer, sw, new AnalyzerConfig() { Lowercase = true, PerformStemming = true, RemoveAccents = true });
            InvertedIndex index = new InvertedIndex(analyzer, new IndexConfig() { BertSimilarity = false });
            index.Index(list);
            Trace.WriteLine("- Query 1: ukrajina zelensky biden , top count = 5");
            List<IDocument> documents = index.VectorSpaceSearch(new VectorQuery() { QueryText = "ukrajina zelensky biden", TopCount = 5 });
            PrintDocuments(documents);
            Trace.WriteLine("- Query 2: válek koronavirus zdravotnictví, top count = 5");
            documents = index.VectorSpaceSearch(new VectorQuery() { QueryText = "válek koronavirus zdravotnictví", TopCount = 5 });
            PrintDocuments(documents);
        }

        private static void PrintDocuments(List<IDocument> docs)
        {
            for (int i = 0; i < docs.Count; i++)
            {
                Trace.WriteLine($"Result n. {i}\n");
                Trace.WriteLine(docs[i].GetRelevantText() + "\n");
            }
        }
    }
}

