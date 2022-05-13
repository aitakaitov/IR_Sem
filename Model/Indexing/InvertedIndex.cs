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

        private List<List<float>> DocumentVectors = new();

        /** Inverted index */
        private Dictionary<int, InvertedIndexValue> DocumentIndex = new();

        /** Term - Term Id mapping */
        private Dictionary<string, int> TermIdDictionary = new();

        /** Term Id - Term mapping */
        private Dictionary<int, string> IdTermDictionary = new();

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
            for (int i = 0; i < uniqueTerms.Count; i++)
            {
                TermIdDictionary[uniqueTerms[i]] = i;
                IdTermDictionary[i] = uniqueTerms[i];
            }

            // Turn postings into inverted index
            CreateInvertedIndex(postings);

            // Calculate all document vectors
            CalculateDocumentVectors();

            Indexed = true;
        }

        private void CalculateDocumentVectors()
        {
            for (int i = 0; i < DocumentIDs.Count; i++)
            {
                float[] documentVector = GetDocumentVector(DocumentIDs[i]);
                DocumentVectors.Add(documentVector.ToList());
            }
        }

        private void CreateInvertedIndex(List<Posting> postings)
        {
            foreach (Posting posting in postings)
            {
                var termId = TermIdDictionary[posting.Term];
                if (!DocumentIndex.ContainsKey(termId))
                {
                    DocumentIndex[termId] = new InvertedIndexValue();
                }

                DocumentValue val = new DocumentValue() { DocumentId = posting.DocumentId, TermFrequency = 1 };
                // Check if the document already contains this term - if yes, increase the term frequency
                if (DocumentIndex[termId].Documents.ContainsValue(val))
                {
                    DocumentIndex[termId].Documents[posting.DocumentId].TermFrequency++;
                }
                else    // Otherwise just add the document to the term
                {
                    DocumentIndex[termId].Documents.Add(posting.DocumentId, val);
                }
            }

            CalculateDFValues();
        }

        private void CalculateDFValues()
        {
            foreach (var invertedIndexValue in DocumentIndex.Values)
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
                if (!TermIdDictionary.Keys.Contains(processedTerm))
                {
                    return null;
                }
                else
                {
                    List<int> ids = new();
                    foreach (var documentValue in DocumentIndex[TermIdDictionary[processedTerm]].Documents)
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
            var queryVector = GetQueryVector(query);

            if (queryVector == null)
            {
                return (new(), 0, new());
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
                var documentVector = DocumentVectors.ElementAt(docId); //GetDocumentVector(docId);
                var cosineSimilarity = CalculateCosineSimilarity(documentVector.ToArray(), queryVector, queryVectorNorm);
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

        private float CalculateCosineSimilarity(float[] documentVector, float[] queryVector, float queryVectorNorm)
        {
            return CalculateDotProduct(documentVector, queryVector) / (CalculateVectorNorm(documentVector) * queryVectorNorm);
        }

        private float CalculateDotProduct(float[] v1, float[] v2)
        {
            return v1.Zip(v2, (d1, d2) => d1 * d2).Sum();
        }

        private float CalculateVectorNorm(float[] vector)
        {
            float norm = (float)Math.Sqrt(
                CalculateDotProduct(vector, vector)
            );
            return norm;
        }

        private float[] GetDocumentVector(int docId)
        {
            var vector = new float[TermIdDictionary.Count];
            // Initialize to 0, since if TF == 0, TF-IDF == 0
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = 0;
            }

            // Replace 0 with TF-IDF values for each term present in the document
            foreach (var termId in DocumentIndex.Keys)
            {
                if (DocumentIndex[termId].Documents.Keys.Contains(docId))
                {
                    vector[termId] = DocumentIndex[termId].Documents[docId].TermFrequency;
                    vector[termId] = CalculateTFIDF((int)vector[termId], DocumentIndex[termId].DocumentFrequency);
                }
            }

            return vector;
        }

        private float[] GetQueryVector(BasicQuery query)
        {
            // Preprocess the query
            List<string> queryTokens = Analyzer.Preprocess(new Document() { Text = query.QueryText });
            List<int> tokenIds = new List<int>();
            // Turn tokens into term-ids
            foreach (var token in queryTokens)
            {
                if (TermIdDictionary.ContainsKey(token))
                {
                    tokenIds.Add(TermIdDictionary[token]);
                }
            }

            if (tokenIds.Count == 0)
            {
                return null;
            }

            // Create query vector, init to 0
            var vector = new float[TermIdDictionary.Count];
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

            return vector;
        }

        private float CalculateTFIDF(int tf, int df)
        {
            if (tf == 0) { return 0; }
            float termFreq = (float)(1 + Math.Log10(tf));
            float idf = (float)Math.Log10(Documents.Count / (float)df);

            return termFreq * idf;
        }




        /** DEBUG */
        public void PrintIndex()
        {
            foreach (var key in DocumentIndex.Keys)
            {
                var value = DocumentIndex[key];
                var term = IdTermDictionary[key];
                string output = term.ToString() + $"\tDF:{value.DocumentFrequency} -";
                foreach (var val in value.Documents)
                {
                    output += $" (DocId: {val.Value.DocumentId}, TermFreq: {val.Value.TermFrequency}),";
                }
                Console.WriteLine(output);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }



    internal class InvertedIndexValue
    {
        public SortedList<int, DocumentValue> Documents { get; set; } = new();
        public int DocumentFrequency = 0;
    }

    internal class DocumentValue : IEquatable<DocumentValue>
    {
        public int DocumentId;
        public int TermFrequency;

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
