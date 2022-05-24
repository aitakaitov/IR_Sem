using Common.Documents;
using Model.Preprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using Model.Queries;
using Common.Documents.Basic;
using Common.Utils;

namespace Model.Indexing
{
    /// <summary>
    /// Inverted index
    /// </summary>
    public class InvertedIndex : AIndex
    {
        /// <summary>
        /// Whether or not Index method was called
        /// </summary>
        public bool Indexed { get; private set; } = false;

        /// <summary>
        /// Preprocessing analyzer
        /// </summary>
        private IAnalyzer Analyzer;

        /// <summary>
        /// Unused
        /// </summary>
        private IndexConfig Config;

        /// <summary>
        /// Documents list - index is documentId
        /// </summary>
        private List<ADocument> Documents = new();

        /// <summary>
        /// List of documentIds
        /// </summary>
        private List<int> DocumentIDs = new();

        /// <summary>
        /// DocumentId - termId mapping to improve vector-space search speed
        /// </summary>
        private List<int>[] DocumentTermIdList;

        /// <summary>
        /// DocumentId - Document-TermFrequency mapping to improve vector-space search speed
        /// </summary>
        private List<DocumentValue>[] DocumentTermList;

        /// <summary>
        /// Inverted index - term - documents mapping
        /// </summary>
        private InvertedIndexValue[] DocumentIndex;

        /// <summary>
        /// Term to termId mapping
        /// </summary>
        private Dictionary<string, int> TermIdMap = new();

        /// <summary>
        /// TermId to term mapping
        /// </summary>
        private string[] IdTermMap;

        /// <summary>
        /// Index name
        /// </summary>
        public string Name { get; private set; }


        public InvertedIndex(IAnalyzer analyzer, IndexConfig indexConfig, string name)
        {
            Analyzer = analyzer;
            Config = indexConfig;
            Name = name;
        }

        public override void Index(List<ADocument> documents)
        {
            if (Indexed)
            {
                throw new InvalidOperationException("This index does not support indexing of additional data. Create a new index and index all the data needed.");
            }

            // Create postings
            List<Posting> postings = new List<Posting>();
            for (int i = 0; i < documents.Count; i++)
            {
                ADocument document = documents[i];
                List<string> tokens = Analyzer.Preprocess(document);
                CreatePostings(postings, tokens, i);

                Documents.Add(document);
                document.Id = i;
                DocumentIDs.Add(i);
            }

            // Generate dictionary
            var uniqueTerms = GetUniqueTerms(postings);

            // Initialize
            DocumentIndex = new InvertedIndexValue[uniqueTerms.Count];
            IdTermMap = new string[uniqueTerms.Count];
            DocumentTermIdList = new List<int>[uniqueTerms.Count];
            DocumentTermList = new List<DocumentValue>[uniqueTerms.Count];
            for (int i = 0; i < uniqueTerms.Count; i++)
            {
                TermIdMap[uniqueTerms[i]] = i;
                IdTermMap[i] = uniqueTerms[i];
                DocumentTermList[i] = new();
                DocumentTermIdList[i] = new();
            }

            // Process postings
            CreateInvertedIndex(postings);

            Indexed = true;
        }

        /// <summary>
        /// Processed postings and costructs all search-related sctuctures
        /// </summary>
        /// <param name="postings"></param>
        private void CreateInvertedIndex(List<Posting> postings)
        {
            foreach (Posting posting in postings)
            {
                var termId = TermIdMap[posting.Term];

                // Document-termId mapping
                if (!DocumentTermIdList[posting.DocumentId].Contains(termId))
                {
                    DocumentTermIdList[posting.DocumentId].Add(termId);
                }

                // Inverted Index initialization
                if (DocumentIndex[termId] == null)
                {
                    DocumentIndex[termId] = new InvertedIndexValue();
                }

                // Check if the document already contains this term - if yes, increase the term frequency
                if (DocumentIndex[termId].Documents.Keys.Contains(posting.DocumentId))
                {
                    var docDict = DocumentIndex[termId].Documents;
                    docDict[posting.DocumentId].TermFrequency++;
                }
                else
                {
                    var documentValue = new DocumentValue() { DocumentId = posting.DocumentId, TermFrequency = 1 };

                    // Create document - term mapping
                    DocumentTermList[posting.DocumentId].Add(documentValue);

                    // Create term-document mapping
                    DocumentIndex[termId].Documents.Add(posting.DocumentId, documentValue);
                }
            }

            // Calculate document frequencies
            CalculateDFValues();
        }

        private void CalculateDFValues()
        {
            foreach (var invertedIndexValue in DocumentIndex)
            {
                invertedIndexValue.DocumentFrequency = invertedIndexValue.Documents.Count;
            }
        }

        private List<string> GetUniqueTerms(List<Posting> postings)
        {
            return postings.Select(p => p.Term).Distinct().ToList();
        }

        /// <summary>
        /// Creates postings from list of tokens and document id
        /// </summary>
        /// <param name="postings">Postings list to add the new postings to</param>
        /// <param name="tokens"></param>
        /// <param name="docId"></param>
        private void CreatePostings(List<Posting> postings, List<string> tokens, int docId)
        {
            foreach (var token in tokens)
            {
                postings.Add(new Posting(token, docId));
            }
        }

        /// <summary>
        /// Performs a boolean search, returning list of topK documents and list of all relevant documents
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public override (List<ADocument>, int) BooleanSearch(BasicQuery query)
        {
            ParserNode parsedQuery = BooleanQueryParser.ParseQuery(query.QueryText);
            var documentIds = GetDocumentsForQuery(parsedQuery);
            documentIds.Sort();

            if (documentIds.Count == 0)
            {
                return (new(), 0);
            }

            int totalCount = documentIds.Count;
            List<ADocument> documents = new();
            if (query.TopCount == -1)
            {
                query.TopCount = totalCount;
            }
            for (int i = 0; i < Math.Min(totalCount, query.TopCount); i++)
            {
                documents.Add(Documents[documentIds[i]]);
            }

            return (documents, totalCount);
        }

        /// <summary>
        /// Returns a list of relevant document Ids given a query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private List<int> BooleanSearchIds(string query)
        {
            ParserNode parsedQuery = BooleanQueryParser.ParseQuery(query);
            var documentIds = GetDocumentsForQuery(parsedQuery);
            return documentIds;
        }

        /// <summary>
        /// For a given ParserNode, create a list of document ids that are relevant to the ParserNode
        /// </summary>
        /// <param name="queryNode"></param>
        /// <returns></returns>
        private List<int> GetDocumentsForQuery(ParserNode queryNode)
        {
            if (queryNode.Type == ParserNode.NodeType.TERM)
            {
                // If the ParsedNode is a leaf, return list of document ids
                return GetDocumentsForTerm(queryNode.Text);
            }
            else
            {
                if (queryNode.Type == ParserNode.NodeType.NOT)
                {
                    // Parser uses LeftChild for the NOT nodes
                    var documentIds = GetDocumentsForQuery(queryNode.LeftChild);
                    return DocumentIDs.Except(documentIds).ToList();
                }
                else
                {
                    var documentIdsLeft = GetDocumentsForQuery(queryNode.LeftChild);
                    var documentIdsRight = GetDocumentsForQuery(queryNode.RightChild);

                    if (documentIdsLeft.Count == 0 && documentIdsRight.Count == 0)
                    {
                        // No results
                        return new();
                    }

                    if (queryNode.Type == ParserNode.NodeType.AND)
                    {
                        return documentIdsLeft.Intersect(documentIdsRight).ToList();
                    }
                    else // OR
                    {
                        return documentIdsLeft.Union(documentIdsRight).ToList();
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of document ids that are relevant to the term
        /// Preprocessed the term
        /// </summary>
        /// <param name="unprocessedTerm"></param>
        /// <returns></returns>
        private List<int> GetDocumentsForTerm(string unprocessedTerm)
        {
            var res = Analyzer.Preprocess(new Document() { Text = unprocessedTerm });
            if (res.Count() == 0)
            {
                return new();
            }
            else
            {
                var processedTerm = res[0];
                if (!TermIdMap.Keys.Contains(processedTerm))
                {
                    return new();
                }
                else
                {
                    List<int> ids = new();
                    foreach (var documentValue in DocumentIndex[TermIdMap[processedTerm]].Documents)
                    {
                        ids.Add(documentValue.Key);
                    }
                    return ids;
                }
            }
        }

        /// <summary>
        /// Performs TF-IDF vector-space search returning top K ADocuments and number of non-zero cosine similarity documents
        /// Performs boolean prefiltering with AND operators and if no relevant documents are returned, the preprocessing is
        /// repeated with OR operators
        /// </summary>
        /// <param name="query"></param>
        /// <returns>tuple of (top K IDocs, count)</returns>
        public override (List<ADocument>, int, List<float>) VectorSpaceSearch(BasicQuery query)
        {
            // Get TF-IDF vector for query
            var res = GetQueryVector(query);
            float[] queryVector;
            if (res == null)
            {
                return (new(), 0, new());
            }
            else
            {
                queryVector = res;
            }

            // Pre-compute vector norm
            var queryVectorNorm = CalculateVectorNorm(queryVector);

            // Narrow the documents with boolean search
            var prefilteredDocuments = BooleanSearchIds(PreprocessQueryForPrefiltering(query.QueryText, "AND"));
            if (prefilteredDocuments.Count < 5)
            {
                prefilteredDocuments = BooleanSearchIds(PreprocessQueryForPrefiltering(query.QueryText, "OR"));
            }

            if (prefilteredDocuments.Count == 0)
            {
                return (new(), 0, new());
            }

            // Get TF-IDF vector for all documents and calculate cosine similarity to query vector
            Dictionary<int, double> DocIdSimilarityDictionary = new();
            foreach (var docId in prefilteredDocuments)
            {
                var documentVector = GetDocumentVector(docId);
                var cosineSimilarity = CalculateCosineSimilarity(documentVector, queryVector, queryVectorNorm);
                DocIdSimilarityDictionary.Add(docId, cosineSimilarity);
            }

            // Get top K results
            List<ADocument> results = new();
            List<float> scores = new();
            int count = 0;
            if (query.TopCount == -1)
            {
                query.TopCount = DocIdSimilarityDictionary.Count;
            }
            foreach (var doc in DocIdSimilarityDictionary.OrderByDescending(key => key.Value))
            {
                if (count == query.TopCount)
                {
                    break;
                }
                results.Add(Documents[doc.Key]);
                scores.Add((float)DocIdSimilarityDictionary[doc.Key]);
                count++;
            }

            return (results, DocIdSimilarityDictionary.Where((id, cos) => cos != 0).Count(), scores);
        }

        /// <summary>
        /// Adds default operators
        /// </summary>
        /// <param name="query">query</param>
        /// <param name="op">operator</param>
        /// <returns>preprocessed query</returns>
        private string PreprocessQueryForPrefiltering(string query, string op)
        {
            var tokens = Analyzer.Tokenize(query);

            string newQuery = "";

            if (tokens.Count < 5 && op == "AND")
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (i != tokens.Count - 1)
                    {
                        newQuery += tokens[i] + " " + op + " ";
                    }
                    else
                    {
                        newQuery += tokens[i];
                    }
                }
            }
            else if (tokens.Count > 6 && op == "OR")
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (i < tokens.Count - 1)
                    {
                        newQuery += tokens[i] + " OR ";
                    }
                    else
                    {
                        newQuery += tokens[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (i < tokens.Count - 1)
                    {
                        newQuery += tokens[i] + " " + op + " ";
                    }
                    else
                    {
                        newQuery += tokens[i];
                    }
                }
            }

            return newQuery;
        }

        private float CalculateCosineSimilarity(float[] documentVector, float[] queryVector, float queryVectorNorm)
        {
            return DotProduct(documentVector, queryVector) / (CalculateVectorNorm(documentVector) * queryVectorNorm);
        }

        private float DotProduct(float[] v1, float[] v2)
        {
            float res = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                res += v1[i] * v2[i];
            }
            return res;
        }

        private float CalculateVectorNorm(float[] vector)
        {
            float norm = (float)Math.Sqrt(
                DotProduct(vector, vector)
            );
            return norm;
        }

        /// <summary>
        /// Calculates TF-IDF vector for a document
        /// </summary>
        /// <param name="docId">document id</param>
        /// <returns>vector</returns>
        private float[] GetDocumentVector(int docId)
        {
            // Default value for float is 0.0f and value-type arrays are always initialized to the default value
            var vector = new float[TermIdMap.Count];

            // Replace 0 with TF-IDF values for each term present in the document
            //foreach (var termId in DocumentTermIdList[docId])
            for (int i = 0; i < DocumentTermIdList[docId].Count; i++)
            {
                var termId = DocumentTermIdList[docId][i];
                var documentValue = DocumentTermList[docId][i];
                vector[termId] = CalculateTFIDFDocument(documentValue.TermFrequency, DocumentIndex[termId].DocumentFrequency);
            }

            return vector;
        }

        /// <summary>
        /// Returns a TF-IDF vector for a query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private float[]? GetQueryVector(BasicQuery query)
        {
            // Preprocess the query
            List<string> queryTokens = Analyzer.Preprocess(new Document() { Text = query.QueryText });
            List<int> tokenIds = new List<int>();
            // Turn tokens into term-ids
            foreach (var token in queryTokens)
            {
                if (TermIdMap.ContainsKey(token))
                {
                    tokenIds.Add(TermIdMap[token]);
                }
            }

            if (tokenIds.Count == 0)
            {
                return null;
            }

            // Create query vector, init to 0 implicitly
            var vector = new float[TermIdMap.Count];

            // Calculate term frequency of the query
            foreach (var termId in tokenIds)
            {
                vector[termId] += 1;
            }

            // Calculate TF-IDF using the now-calculated TF values and IDF values from inverted index
            for (int i = 0; i < vector.Length; i++)
            {
                if (vector[i] != 0)
                {
                    vector[i] = CalculateTFIDFQuery((int)vector[i], DocumentIndex[i].DocumentFrequency);
                }
            }

            return vector;
        }

        private float CalculateTFIDFQuery(int tf, int df)
        {
            if (tf == 0) { return 0; }
            float termFreq = (float)(1 + Math.Log10(tf));
            float idf = (float)Math.Log10(Documents.Count / (float)df);

            return termFreq * idf;
        }
        private float CalculateTFIDFDocument(int tf, int df)
        {
            if (tf == 0) { return 0; }
            float termFreq = (float)(1 + Math.Log10(tf));
            float idf = (float)Math.Log10(Documents.Count / (float)df);

            return termFreq * idf;
        }

        public override List<ADocument> GetDocumentsByIds(List<int> documentIds)
        {
            List<ADocument> results = new();
            foreach (int documentId in documentIds)
            {
                if (documentId >= Documents.Count)
                {
                    throw new InvalidOperationException("Non existent document");
                }
                results.Add(Documents[documentId]);
            }
            return results;
        }

        public override string ToString()
        {
            return Name;
        }
    }



    internal class InvertedIndexValue
    {
        public Dictionary<int, DocumentValue> Documents { get; set; } = new();
        public int DocumentFrequency = 0;
    }

    internal class DocumentValue : IEquatable<DocumentValue>, IComparable<DocumentValue>
    {
        public int DocumentId;
        public int TermFrequency;

        public int CompareTo(DocumentValue? other)
        {
            return DocumentId - other.DocumentId;
        }

        // To make adding them to SortedList easier during the construction of the inverted index
        public bool Equals(DocumentValue? other)
        {
            if (other == null) return false;
            if (this.DocumentId == other.DocumentId) return true;
            return false;
        }

        /// <summary>
        /// To make it possible to use Contains
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.DocumentId.GetHashCode();
        }
    }

    internal record Posting(string Term, int DocumentId);
}
