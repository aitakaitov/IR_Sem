namespace Model.Preprocessing
{
    public class AnalyzerConfig
    {
        public AnalyzerConfig() { }

        public AnalyzerConfig(bool performStemming, bool removeAccents, bool lowercase)
        {
            PerformStemming = performStemming;
            RemoveAccents = removeAccents;
            Lowercase = lowercase;
        }

        /// <summary>
        /// Whether to remove accents or not
        /// </summary>
        public bool RemoveAccents { get; set; } = true;

        /// <summary>
        /// Whether or lowercase or not
        /// </summary>
        public bool Lowercase { get; set; } = true;

        /// <summary>
        /// Whether to perform stemming or not. Stemming works only for Czech
        /// </summary>
        public bool PerformStemming { get; set; } = true;
    }
}
