namespace Common.Documents.Trec
{
#nullable disable
    /// <summary>
    /// TREC evaluation topic
    /// For JSON parsing
    /// </summary>
    public class Topic : ADocument
    {
        public string TopicId { get; set; }
        public string Narrative { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Lang { get; set; }

        public override string GetRelevantText()
        {
            return Title;
        }
    }
}
