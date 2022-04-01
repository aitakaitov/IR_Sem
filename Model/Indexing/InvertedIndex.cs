using Common.Documents;
using Model.Preprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Indexing
{
    public class InvertedIndex : IIndex
    {
        public bool Indexed { get; private set; } = false;

        private Analyzer Analyzer;

        private IndexConfig Config;

        private List<IDocument> Documents;

        private Dictionary<int, InvertedIndexValue> DocumentIndex = new();

        private Dictionary<string, int> TermIdDictionary = new();


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
            }

            CreateInvertedIndex(postings);

            throw new NotImplementedException();
            Documents = documents;
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

        public void VectorSpaceSearch()
        {
            throw new NotImplementedException();
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
