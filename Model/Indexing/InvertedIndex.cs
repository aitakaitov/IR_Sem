using Common.Documents;
using Model.Preprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Model.Queries;
using Common.Documents.Basic;
using System.IO;
using Common.Utils;
using System.Numerics;

namespace Model.Indexing
{
    public class InvertedIndex : IIndex
    {
        public bool Indexed { get; private set; } = false;

        /** Preprocessing */
        private IAnalyzer Analyzer;

        /** Not used right now */
        private IndexConfig Config;

        /** Documents - position is document ID */
        private List<IDocument> Documents = new();
        private List<int> DocumentIDs = new();

        private List<int>[] DocumentTermList;

        /** Inverted index */
        private InvertedIndexValue[] DocumentIndex;

        /** Term - Term Id mapping */
        private Dictionary<string, int> TermIdMap = new();

        /** Term Id - Term mapping */
        private string[] IdTermMap;

        public string Name { get; private set; }


        public InvertedIndex(IAnalyzer analyzer, IndexConfig indexConfig, string name)
        {
            this.Analyzer = analyzer;
            this.Config = indexConfig;
            this.Name = name;
        }

        public void Index(List<IDocument> documents)
        {
            if (Indexed)
            {
                throw new InvalidOperationException("This index does not support indexing of additional data. Create a new index and index all the data needed.");
            }

            // Create postings
            List<Posting> postings = new List<Posting>();
            for (int i = 0; i < documents.Count; i++)
            {
                IDocument document = documents[i];
                List<string> tokens = Analyzer.Preprocess(document);
                CreatePostings(postings, tokens, i);

                Documents.Add(document);
                DocumentIDs.Add(i);
            }

            // Generate dictionary
            var uniqueTerms = GetUniqueTerms(postings);
            DocumentIndex = new InvertedIndexValue[uniqueTerms.Count];
            IdTermMap = new string[uniqueTerms.Count];
            DocumentTermList = new List<int>[uniqueTerms.Count];
            for (int i = 0; i < uniqueTerms.Count; i++)
            {
                TermIdMap[uniqueTerms[i]] = i;
                IdTermMap[i] = uniqueTerms[i];
                DocumentTermList[i] = new();
            }

            // Turn postings into inverted index
            CreateInvertedIndex(postings);

            Indexed = true;
        }

        private void CreateInvertedIndex(List<Posting> postings)
        {
            foreach (Posting posting in postings)
            {
                var termId = TermIdMap[posting.Term];

                // Document-term mapping
                if (!DocumentTermList[posting.DocumentId].Contains(termId))
                {
                    DocumentTermList[posting.DocumentId].Add(termId);
                }

                // Inverted Index
                if (DocumentIndex[termId] == null)
                {
                    DocumentIndex[termId] = new InvertedIndexValue();
                }

                //DocumentValue val = new DocumentValue() { DocumentId = posting.DocumentId, TermFrequency = 1 };
                // Check if the document already contains this term - if yes, increase the term frequency
                if (DocumentIndex[termId].Documents.Keys.Contains(posting.DocumentId))
                {
                    var docDict = DocumentIndex[termId].Documents;
                    docDict[posting.DocumentId].TermFrequency++;
                }
                else    // Otherwise just add the document to the term
                {
                    DocumentIndex[termId].Documents.Add(posting.DocumentId, new() { DocumentId = posting.DocumentId, TermFrequency = 1 });
                }
            }

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

        private void CreatePostings(List<Posting> postings, List<string> tokens, int docId)
        {
            foreach (var token in tokens)
            {
                postings.Add(new Posting(token, docId));
            }
        }

        public (List<IDocument>, int) BooleanSearch(BasicQuery query)
        {
            ParserNode parsedQuery = BooleanQueryParser.ParseQuery(query.QueryText);
            var documentIds = GetDocumentsForQuery(parsedQuery);

            if (documentIds == null)
            {
                return (new(), 0);
            }

            int totalCount = documentIds.Count;
            List<IDocument> documents = new();
            for (int i = 0; i < Math.Min(totalCount, query.TopCount); i++)
            {
                documents.Add(Documents[documentIds[i]]);
            }

            return (documents, totalCount);
        }

        private List<int> BooleanSearchIds(string query)
        {
            ParserNode parsedQuery = BooleanQueryParser.ParseQuery(query);
            var documentIds = GetDocumentsForQuery(parsedQuery);
            return documentIds;
        }

        private List<int> GetDocumentsForQuery(ParserNode queryNode)
        {
            if (queryNode.Type == ParserNode.NodeType.TERM)
            {
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

                    if (documentIdsLeft == null && documentIdsRight == null)
                    {
                        // No results
                        return null;
                    }
                    else if (documentIdsLeft == null)
                    {
                        // Pass right
                        return documentIdsRight;
                    }
                    else if (documentIdsRight == null)
                    {
                        // Pass left
                        return documentIdsLeft;
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

        private List<int> GetDocumentsForTerm(string unprocessedTerm)
        {
            var res = Analyzer.Preprocess(new Document() { Text = unprocessedTerm });
            if (res.Count() == 0)
            {
                return DocumentIDs;
            }
            else
            {
                var processedTerm = res[0];
                if (!TermIdMap.Keys.Contains(processedTerm))
                {
                    return null;
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
        /// Performs TF-IDF vector-space search returning top K IDocuments and number of non-zero cosine similarity documents
        /// </summary>
        /// <param name="query"></param>
        /// <returns>tuple of (top K IDocs, count)</returns>
        public (List<IDocument>, int, List<float>) VectorSpaceSearch(BasicQuery query)
        {
            // Get TF-IDF vector for query
            var res = GetQueryVector(query);
            var queryVector = new Vector<float>();
            if (res == null)
            {
                return (new(), 0, new());
            }
            else
            {
                queryVector = res.Value;
            }

            // Pre-compute vector norm
            var queryVectorNorm = CalculateVectorNorm(queryVector);
            // Narrow the documents with boolean search
            var prefilteredDocuments = BooleanSearchIds(query.QueryText);

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
            List<IDocument> results = new();
            List<float> scores = new();
            int count = 0;
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

        private float CalculateCosineSimilarity(Vector<float> documentVector, Vector<float> queryVector, float queryVectorNorm)
        {
            return Vector.Dot(documentVector, queryVector) / (CalculateVectorNorm(documentVector) * queryVectorNorm);
        }

        private float CalculateVectorNorm(Vector<float> vector)
        {
            float norm = (float)Math.Sqrt(
                Vector.Dot(vector, vector)
            );
            return norm;
        }

        private Vector<float> GetDocumentVector(int docId)
        {
            // Default value for float is 0.0f and value-type arrays are always initialized to the default value
            var vector = new float[TermIdMap.Count];

            // Replace 0 with TF-IDF values for each term present in the document
            foreach (var termId in DocumentTermList[docId])
            {
                //if (DocumentIndex[termId].Documents.ContainsKey(docId))
                //{
                vector[termId] = DocumentIndex[termId].Documents[docId].TermFrequency;
                vector[termId] = CalculateTFIDF((int)vector[termId], DocumentIndex[termId].DocumentFrequency);
                //}
            }

            return new Vector<float>(vector);
        }

        private Vector<float>? GetQueryVector(BasicQuery query)
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

            // Create query vector, init to 0
            var vector = new float[TermIdMap.Count];
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = 0;
            }

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
                    vector[i] = CalculateTFIDF((int)vector[i], DocumentIndex[i].DocumentFrequency);
                }
            }

            return new Vector<float>(vector);
        }

        private float CalculateTFIDF(int tf, int df)
        {
            if (tf == 0) { return 0; }
            float termFreq = (float)(1 + Math.Log10(tf));
            float idf = (float)Math.Log10(Documents.Count / (float)df);

            return termFreq * idf;
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

        public override int GetHashCode()
        {
            return this.DocumentId.GetHashCode();
        }
    }

    internal record Posting(string Term, int DocumentId);
}
