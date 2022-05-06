
namespace Common.Documents.Idnes
{
    /** IDnes article comment - for JSON parsing */
    public class Comment : IDocument
    {
        public Comment(string text, string timestamp, string author, int positive, int negative)
        {
            this.Text = text;
            this.TimeStamp = timestamp;
            this.Author = author;
            this.PositiveReactions = positive;
            this.NegativeReactions = negative;
        }

        public string Text { get; set; }
        public string TimeStamp { get; set; }
        public string Author { get; set; }
        public int PositiveReactions { get; set; }
        public int NegativeReactions { get; set; }

        public string GetRelevantText()
        {
            return Author + "\n" + Text;
        }
    }


}
