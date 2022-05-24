using Common.Documents;
using Common.Documents.Trec;
using Common.Utils;
using Controller.Database;
using Controller.Database.Entities;
using Controller.Enums;
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
        public AIndex? SelectedIndex { get; set; }

        /// <summary>
        /// All available indexes
        /// </summary>
        public ObservableCollection<AIndex> AvailableIndexes { get; set; } = new();

        /// <summary>
        /// Currently displayed relevant documents
        /// </summary>
        public ObservableCollection<string> RelevantDocuments { get; set; } = new();

        /// <summary>
        /// Total number of hits from last query
        /// </summary>
        public int TotalHits { get; set; } = 0;

        /// <summary>
        /// History of queries made over the selected index
        /// </summary>
        public ObservableCollection<QueryResult> QueryHistory { get; set; } = new();

        /// <summary>
        /// Database
        /// </summary>
        private DatabaseContext databaseContext;

        /// <summary>
        /// Callback to View to set the query type
        /// </summary>
        private Action<EQueryType>? SetQueryTypeCallback = null;

        /// <summary>
        /// Callback to View to set the query string
        /// </summary>
        private Action<string>? SetQueryStringCallback = null;

        public Controller()
        {
            databaseContext = new DatabaseContext();
            databaseContext.Database.EnsureDeleted();
            databaseContext.Database.EnsureCreated();
        }

        /// <summary>
        /// Sets the required callbacks
        /// If not called, history does not work
        /// </summary>
        /// <param name="setQueryTypeCallback"></param>
        /// <param name="setQueryStringCallback"></param>
        public void SetCallbacks(Action<EQueryType> setQueryTypeCallback, Action<string> setQueryStringCallback)
        {
            SetQueryTypeCallback = setQueryTypeCallback;
            SetQueryStringCallback = setQueryStringCallback;
        }


        /// <summary>
        /// Creates a new index and adds it to AvailableIndexes
        /// </summary>
        /// <param name="request"></param>
        /// <returns>the new index</returns>
        public AIndex CreateIndex(CreateIndexRequest request)
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

            AIndex index = new InvertedIndex(analyzer, new(), request.Name);

            var documents = DocumentLoaderText.Load(request.DocumentDirectoryPath);
            index.Index(documents);

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

        public void DeleteIndex(AIndex index)
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
            if (SelectedIndex == null)
            {
                throw new InvalidOperationException("No index selected");
            }

            var documents = SelectedIndex.BooleanSearch(new()
            {
                QueryText = queryText,
                TopCount = 100
            });

            RelevantDocuments.Clear();
            documents.Item1.ForEach(d => RelevantDocuments.Add(d.GetRelevantText()));
            TotalHits = documents.Item2;

            StoreQueryResults(documents.Item1, queryText, EQueryType.BOOLEAN);
        }

        private void StoreQueryResults(List<ADocument> documents, string query, EQueryType type)
        {
            List<DocumentInfo> documentInfos = new List<DocumentInfo>();
            for (int i = 0; i < documents.Count; i++)
            {
                documentInfos.Add(new()
                {
                    DocumentId = documents[i].Id,
                    OrderInQuery = i
                });
            }

            QueryResult qr = new()
            {
                Query = query,
                Documents = documentInfos,
                DateQueried = DateTime.Now,
                QueryType = type,
                IndexName = SelectedIndex.ToString()        // At this point we know an index is selected, because we had to make a query on it first
            };

            databaseContext.Queries.Add(qr);
            databaseContext.SaveChanges();

            QueryHistory.Insert(0, qr);
        }

        /// <summary>
        /// Performs a vector-space query over the SelectedIndex
        /// Results are saved in RelevantDocuments
        /// </summary>
        /// <param name="queryText"></param>
        public void MakeVectorQuery(string queryText)
        {
            if (SelectedIndex == null)
            {
                throw new InvalidOperationException("No index selected");
            }

            var documents = SelectedIndex.VectorSpaceSearch(new()
            {
                QueryText = queryText,
                TopCount = 100
            });

            RelevantDocuments.Clear();
            documents.Item1.ForEach(d => RelevantDocuments.Add(d.GetRelevantText()));
            TotalHits = documents.Item2;

            StoreQueryResults(documents.Item1, queryText, EQueryType.VECTOR);
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
            AIndex index = new InvertedIndex(analyzer, new(), "TREC");

            index.Index(documents);

            List<string> lines = new();
            for (int i = 0; i < queries.Count; i++)
            {
                // We know it is a topic, so null is not an issue here
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
                    lines.Add(query.TopicId + " Q0 " + "abc" + " " + "99" + " " + 0.0 + " runindex1");
                }
                else
                {
                    for (int j = 0; j < relevantDocuments.Count; j++)
                    {
                        // Again, we know it is a TrecDocument
                        var doc = relevantDocuments[j] as TrecDocument;
                        float score = scores[j];
                        string line = query.TopicId + " Q0 " + doc.DocumentId + " " + j + " " + score + " runindex1";
                        lines.Add(line);
                    }
                }
            }

            File.WriteAllLines(directory + "/results.txt", lines);
        }

        /// <summary>
        /// Updates the history of queries based on the current selected index
        /// The queries are sorted based on their time of execution
        /// </summary>
        public void UpdateHistory()
        {
            if (SelectedIndex == null)
            {
                return;
            }

            QueryHistory.Clear();
            databaseContext.Queries
                .Where(q => q.IndexName == SelectedIndex.ToString())
                .OrderByDescending(q => q.DateQueried)
                .ToList()
                .ForEach(q => QueryHistory.Add(q));
        }

        /// <summary>
        /// Sets the results view to the results of the passed QueryResult
        /// Uses callbacks to view in order to set the query type and query string
        /// </summary>
        /// <param name="result">QueryResult</param>
        public void SetToHistory(object result)
        {
            if (result is not QueryResult || SetQueryStringCallback == null || SetQueryTypeCallback == null || SelectedIndex == null)
            {
                return;
            }

            var queryResult = (QueryResult)result;
            SetQueryStringCallback(queryResult.Query);
            SetQueryTypeCallback(queryResult.QueryType);

            var documentIds = queryResult.Documents.OrderBy(di => di.OrderInQuery).Select(di => di.DocumentId).ToList();
            var documents = SelectedIndex.GetDocumentsByIds(documentIds);
            RelevantDocuments.Clear();
            documents.ForEach(d => RelevantDocuments.Add(d.GetRelevantText()));
            TotalHits = documents.Count;
        }

        /// <summary>
        /// Returns the names of already created indexes
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvailableIndexesNames()
        {
            return AvailableIndexes.Select(i => i.ToString()).ToList();
        }
    }
}
