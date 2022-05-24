
namespace Common.Documents.Idnes
{
    /// <summary>
    /// IDnes article comment for JSON parsing purposes
    /// </summary>
    public class Comment : ADocument
    {
        public Comment(string text, string timestamp, string author, int positive, int negative)
        {
            Text = text;
            TimeStamp = timestamp;
            Author = author;
            PositiveReactions = positive;
            NegativeReactions = negative;
        }

        public string Text { get; set; }
        public string TimeStamp { get; set; }
        public string Author { get; set; }
        public int PositiveReactions { get; set; }
        public int NegativeReactions { get; set; }

        public override string GetRelevantText()
        {
            return Author + "\n" + Text;
        }
    }


}
