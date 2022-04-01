﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Documents;

namespace Model.Preprocessing
{
    public class Analyzer : IAnalyzer
    {
        private ITokenizer Tokenizer { get; set; }
        private IStemmer Stemmer { get; set; }
        private AnalyzerConfig Config { get; set; }
        private IStopwords Stopwords { get; set; }

        public Analyzer(ITokenizer tokenizer, IStemmer stemmer, IStopwords stopwords, AnalyzerConfig config)
        {
            Tokenizer = tokenizer;
            Stemmer = stemmer;
            Config = config;
            Stopwords = stopwords;
            if (stopwords == null)
            {
                // Initialize empty stopwords
                stopwords = new Stopwords();
            }
        }

        /// <summary>
        /// Performs preprocessing on a document and returns a list of tokens
        /// </summary>
        /// <param name="doc">document</param>
        /// <returns>tokens</returns>
        public List<string> Preprocess(IDocument doc)
        {
            string text = doc.GetRelevantText();

            if (Config.Lowercase)
            {
                text = Lowercase(text);
            }
            if (Config.RemoveAccents)
            {
                text = Accents.RemoveAccents(text);
            }

            var tokens = Tokenizer.Tokenize(text);
            tokens = Stopwords.RemoveStopwords(tokens);

            if (Config.PerformStemming)
            {
                tokens = Stemmer.StemTokens(tokens);
            }

            return tokens;
        }

        private string Lowercase(string text)
        {
            return text.ToLower();
        }
    }
}
