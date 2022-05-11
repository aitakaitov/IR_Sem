using Common.Utils;
using Controller.Transfer;
using Model.Indexing;
using Model.Preprocessing;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
    [AddINotifyPropertyChangedInterface]
    public class Controller
    {
        public IIndex SelectedIndex { get; set; }
        public ObservableCollection<IIndex> AvailableIndexes { get; set; } = new();
        public ObservableCollection<string> RelevantDocuments { get; set; } = new();

        public IIndex CreateIndex(CreateIndexRequest request)
        {
            AnalyzerConfig config = new()
            {
                PerformStemming = request.PerformStemming,
                Lowercase = request.Lowercase,
                RemoveAccents = request.RemoveAccents
            };

            IStopwords stopwords = VerifyAndLoadStopwords(request.StopwordsFilePath);
            IStemmer stemmer = null;
            if (config.PerformStemming)
            {
                stemmer = new Stemmer();
            }

            ITokenizer tokenizer = new Tokenizer(stopwords);
            IAnalyzer analyzer = new Analyzer(tokenizer, stemmer, stopwords, config);

            IIndex index = new InvertedIndex(analyzer, new(), request.Name);

            var documents = DocumentLoaderText.Load(request.DocumentDirectoryPath);
            index.Index(documents);

            AvailableIndexes.Add(index);
            return index;
        }

        private IStopwords VerifyAndLoadStopwords(string? filePath)
        {
            Stopwords stopwords = new();
            if (filePath == null)
            {
                // Return empty
                return stopwords;
            }

            stopwords.LoadStopwords(filePath);
            return stopwords;
        }

        public void DeleteIndex(IIndex index)
        {
            AvailableIndexes.Remove(index);
        }

        public void MakeBooleanQuery(string queryText)
        {
            var documents = SelectedIndex.BooleanSearch(new()
            {
                QueryText = queryText,
                TopCount = 10
            });

            RelevantDocuments.Clear();
            documents.Item1.ForEach(d => RelevantDocuments.Add(d.GetRelevantText()));
        }

        public void MakeVectorQuery(string queryText)
        {
            var documents = SelectedIndex.VectorSpaceSearch(new()
            {
                QueryText = queryText,
                TopCount = 10
            });

            RelevantDocuments.Clear();
            documents.Item1.ForEach(d => RelevantDocuments.Add(d.GetRelevantText()));
        }
    }
}
