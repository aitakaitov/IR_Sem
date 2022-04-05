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

namespace Model.Indexing
{
    public class InvertedIndex : IIndex
    {
        public bool Indexed { get; private set; } = false;

        /** Preprocessing */
        private Analyzer Analyzer;

        /** Not used right now */
        private IndexConfig Config;

        /** Documents - position is document ID */
        private List<IDocument> Documents = new();

        /** Inverted index */
        private Dictionary<int, InvertedIndexValue> DocumentIndex = new();

        /** Term - Term Id mapping */
        private Dictionary<string, int> TermIdDictionary = new();

        /** Term Id - Term mapping */
        private Dictionary<int, string> IdTermDictionary = new();


        public InvertedIndex(Analyzer analyzer, IndexConfig indexConfig)
        {
            this.Analyzer = analyzer;
            this.Config = indexConfig;
        }

        public void Index(List<IDocument> documents)
        {
            if (Indexed)
            {
                throw new InvalidOperationException("This index does not support indexing of additional data. Create a new index and index all the data needed.");
            }

            List<Posting> postings = new List<Posting>();
            for (int i = 0; i < documents.Count; i++)
            {
                IDocument document = documents[i];
                List<string> tokens = Analyzer.Preprocess(document);
                CreatePostings(postings, tokens, i);

                Documents.Add(document);
            }

            var uniqueTerms = GetUniqueTerms(postings);
            for (int i = 0; i < uniqueTerms.Count; i++)
            {
                TermIdDictionary[uniqueTerms[i]] = i;
                IdTermDictionary[i] = uniqueTerms[i];
            }

            CreateInvertedIndex(postings);
            Indexed = true;
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

        public void BooleanSearch()
        {
            throw new NotImplementedException();
        }

        public List<IDocument> VectorSpaceSearch(VectorQuery query)
        {
            var queryVector = GetQueryVector(query);
            double queryVectorNorm = CalculateVectorNorm(queryVector);
            Dictionary<int, double> DocIdSimilarityDictionary = new();
            for (int i = 0; i < Documents.Count; i++)
            {
                var documentVector = GetDocumentVector(i);
                DocIdSimilarityDictionary.Add(i, CalculateCosineSimilarity(documentVector, queryVector, queryVectorNorm));
            }

            List<IDocument> results = new();
            int count = 0;
            foreach (var doc in DocIdSimilarityDictionary.OrderByDescending(key => key.Value))
            {
                if (count == query.TopCount)
                {
                    break;
                }
                results.Add(Documents[doc.Key]);
                Debug.Print($"Result {count + 1} cosine: {doc.Value}");
                count++;
            }

            return results;
        }

        private double CalculateCosineSimilarity(double[] documentVector, double[] queryVector, double queryVectorNorm)
        {
            return CalculateDotProduct(documentVector, queryVector) / (CalculateVectorNorm(documentVector) * queryVectorNorm);
        }

        private double CalculateDotProduct(double[] v1, double[] v2)
        {
            return v1.Zip(v2, (d1, d2) => d1 * d2).Sum();
        }

        private double CalculateVectorNorm(double[] vector)
        {
            double norm = Math.Sqrt(
                CalculateDotProduct(vector, vector)
            );
            return norm;
        }

        private double[] GetDocumentVector(int docId)
        {
            var vector = new double[TermIdDictionary.Count];
            for (int i = 0; i < vector.Length; i++) { vector[i] = 0; }

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

        private double[] GetQueryVector(VectorQuery query)
        {
            List<string> queryTokens = Analyzer.Preprocess(new Document() { Text = query.QueryText });
            List<int> tokenIds = new List<int>();
            foreach (var token in queryTokens)
            {
                if (TermIdDictionary.ContainsKey(token))
                {
                    tokenIds.Add(TermIdDictionary[token]);
                }
            }

            var vector = new double[TermIdDictionary.Count];
            for (int i = 0; i < vector.Length; i++) { vector[i] = 0; }

            foreach (var termId in tokenIds)
            {
                vector[termId] += 1;
            }

            for (int i = 0; i < vector.Length; i++)
            {
                if (vector[i] != 0)
                {
                    vector[i] = CalculateTFIDF((int)vector[i], DocumentIndex[i].DocumentFrequency);
                }
            }

            return vector;
        }

        private double CalculateTFIDF(int tf, int df)
        {
            if (tf == 0) { return 0; }
            double termFreq = 1 + Math.Log10(tf);
            double idf = Math.Log10(Documents.Count / (double)df);

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
                Trace.WriteLine(output);
            }
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
