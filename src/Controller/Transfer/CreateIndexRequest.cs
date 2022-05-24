namespace Controller.Transfer
{
    /// <summary>
    /// Request for new index creation
    /// </summary>
    public class CreateIndexRequest
    {
        /// <summary>
        /// Index name
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// Whether to perform stemming or not
        /// Stemming works only for Czech
        /// </summary>
        public bool PerformStemming { get; set; }
        /// <summary>
        /// Whether to lowercase or not
        /// </summary>
        public bool Lowercase { get; set; }
        /// <summary>
        /// Whether to remove accents or not
        /// </summary>
        public bool RemoveAccents { get; set; }
        /// <summary>
        /// Optional stopwords file path. If null, no stopwords are used
        /// </summary>
        public string? StopwordsFilePath { get; set; } = "";
        /// <summary>
        /// Path to directory with documents to index
        /// </summary>
        public string DocumentDirectoryPath { get; set; } = "";
    }
}
