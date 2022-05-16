using Common.Documents.Trec;
using Common.Utils;
using Controller.Transfer;
using Model.Indexing;
using Model.Preprocessing;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
    /// <summary>
    /// Controller which serves as an interface between frontend and core functionality
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class Controller
    {
        /// <summary>
        /// Currently selected index
        /// </summary>
        public IIndex SelectedIndex { get; set; }

        /// <summary>
        /// All available indexes
        /// </summary>
        public ObservableCollection<IIndex> AvailableIndexes { get; set; } = new();

        /// <summary>
        /// Currently displayed relevant documents
        /// </summary>
        public ObservableCollection<string> RelevantDocuments { get; set; } = new();

        /// <summary>
        /// Total number of hits from last query
        /// </summary>
        public int TotalHits { get; set; } = 0;

        /// <summary>
        /// Creates a new index and adds it to AvailableIndexes
        /// </summary>
        /// <param name="request"></param>
        /// <returns>the new index</returns>
        public IIndex CreateIndex(CreateIndexRequest request)
        {
            AnalyzerConfig config = new()
            {
                PerformStemming = request.PerformStemming,
                Lowercase = request.Lowercase,
                RemoveAccents = request.RemoveAccents
            };

            IStopwords stopwords = VerifyAndLoadStopwords(request.StopwordsFilePath, config);
            IStemmer? stemmer = null;
            if (config.PerformStemming)
            {
                stemmer = new Stemmer();
            }

            ITokenizer tokenizer = new Tokenizer();
            IAnalyzer analyzer = new Analyzer(tokenizer, stemmer, stopwords, config);

            IIndex index = new InvertedIndex(analyzer, new(), request.Name);

            var documents = DocumentLoaderText.Load(request.DocumentDirectoryPath);
            index.Index(documents);

            AvailableIndexes.Add(index);
            return index;
        }

        /// <summary>
        /// Checks if the path to the stopwords file exists and if it does, it loads and returns the stopwords
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>stopwords</returns>
        private IStopwords VerifyAndLoadStopwords(string? filePath, AnalyzerConfig config)
        {
            Stopwords stopwords = new();
            stopwords.SetConfig(config);
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

        /// <summary>
        /// Performs a boolean query over the SelectedIndex
        /// Results are saved in RelevantDocuments
        /// </summary>
        /// <param name="queryText"></param>
        public void MakeBooleanQuery(string queryText)
        {
            var documents = SelectedIndex.BooleanSearch(new()
            {
                QueryText = queryText,
                TopCount = 100
            });

            RelevantDocuments.Clear();
            documents.Item1.ForEach(d => RelevantDocuments.Add(d.GetRelevantText()));
            TotalHits = documents.Item2;
        }

        /// <summary>
        /// Performs a vector-space query over the SelectedIndex
        /// Results are saved in RelevantDocuments
        /// </summary>
        /// <param name="queryText"></param>
        public void MakeVectorQuery(string queryText)
        {
            var documents = SelectedIndex.VectorSpaceSearch(new()
            {
                QueryText = queryText,
                TopCount = 100
            });

            RelevantDocuments.Clear();
            documents.Item1.ForEach(d => RelevantDocuments.Add(d.GetRelevantText()));
            TotalHits = documents.Item2;
        }

        /// <summary>
        /// Runs TREC evaluation given directory with documents and topics
        /// </summary>
        /// <param name="directory">trec dataset directory</param>
        public void RunEval(string directory)
        {
            var documents = DocumentLoaderJson.Load<TrecDocument>(directory + "/documents");
            var queries = DocumentLoaderJson.Load<Topic>(directory + "/topics");

            AnalyzerConfig config = new()
            {
                PerformStemming = true,
                Lowercase = true,
                RemoveAccents = true
            };

            Stopwords stopwords = new Stopwords();
            stopwords.SetConfig(config);
            stopwords.UseDefaults();

            IStemmer stemmer = new Stemmer();

            ITokenizer tokenizer = new Tokenizer();
            IAnalyzer analyzer = new Analyzer(tokenizer, stemmer, stopwords, config);
            IIndex index = new InvertedIndex(analyzer, new(), "TREC");

            index.Index(documents);

            List<string> lines = new();
            for (int i = 0; i < queries.Count; i++)
            {

                var query = queries[i] as Topic;
                var result = index.VectorSpaceSearch(new()
                {
                    QueryText = query.GetRelevantText(),
                    TopCount = 100
                });

                var relevantDocuments = result.Item1;
                var scores = result.Item3;

                if (relevantDocuments.Count == 0)
                {
                    lines.Add(query.Id + " Q0 " + "abc" + " " + "99" + " " + 0.0 + " runindex1");
                }
                else
                {
                    for (int j = 0; j < relevantDocuments.Count; j++)
                    {
                        var doc = relevantDocuments[j] as TrecDocument;
                        float score = scores[j];
                        string line = query.Id + " Q0 " + doc.Id + " " + j + " " + score + " runindex1";
                        lines.Add(line);
                    }
                }
            }

            File.WriteAllLines(directory + "/results.txt", lines);
        }
    }
}
