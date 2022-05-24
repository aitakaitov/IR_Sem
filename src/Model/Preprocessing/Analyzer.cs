using Common.Documents;
using System;
using System.Collections.Generic;

namespace Model.Preprocessing
{
    public class Analyzer : IAnalyzer
    {
        private ITokenizer Tokenizer { get; set; }
        private IStemmer? Stemmer { get; set; }
        private AnalyzerConfig Config { get; set; }
        private IStopwords Stopwords { get; set; }

        public Analyzer(ITokenizer tokenizer, IStemmer? stemmer, IStopwords stopwords, AnalyzerConfig config)
        {
            Tokenizer = tokenizer;

            if (stemmer == null && config.PerformStemming)
            {
                throw new InvalidOperationException("Configuration.PerformStemming is set to true but no stemmer was passed");
            }

            Stemmer = stemmer;
            Config = config;
            if (stopwords == null)
            {
                // Initialize empty stopwords
                Stopwords = new Stopwords();
            }
            else
            {
                // Automatically propagate config
                Stopwords = stopwords;
                Stopwords.SetConfig(config);
            }
        }

        /// <summary>
        /// Performs preprocessing on a document and returns a list of tokens
        /// </summary>
        /// <param name="doc">document</param>
        /// <returns>tokens</returns>
        public List<string> Preprocess(ADocument doc)
        {
            string text = doc.GetRelevantText();

            if (Config.Lowercase)
            {
                text = Lowercase(text);
            }
            if (Config.RemoveAccents)
            {
                // Accent removal takes some time -- optimize?
                text = Accents.RemoveAccents(text);
            }

            var tokens = Tokenizer.Tokenize(text);

            if (Config.PerformStemming && Stemmer != null)
            {
                tokens = Stemmer.StemTokens(tokens);
            }

            tokens = Stopwords.RemoveStopwords(tokens);

            return tokens;
        }

        /// <summary>
        /// Passtrough to allow tokenizing without direct access to Tokenizer
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public List<string> Tokenize(string s)
        {
            return Tokenizer.Tokenize(s);
        }

        private string Lowercase(string text)
        {
            return text.ToLower();
        }
    }
}
