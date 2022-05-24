namespace Common.Documents
{
    public abstract class ADocument
    {
        /// <summary>
        /// ID of the document, determined by the indexer
        /// </summary>
        public int Id { get; set; }

        public abstract string GetRelevantText();
    }
}
