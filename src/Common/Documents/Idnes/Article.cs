using System;
using System.Collections.Generic;

namespace Common.Documents.Idnes
{
    /// <summary>
    /// IDnes article class for JSON parsing purposes
    /// </summary>
    public class Article : ADocument
    {
        public Article(string headline, string opener, string text, string published, string modified,
            List<string> authors, List<string> tags, List<string> related, List<Comment> comments)
        {
            Opener = opener;
            Headline = headline;
            Authors = authors;
            Comments = comments;
            RelatedArticles = related;
            Text = text;
            DatePublished = published;
            DateModified = modified;
            Tags = tags;
        }

        public string Headline { get; set; }
        public string Opener { get; set; }
        public string Text { get; set; }
        public string DatePublished { get; set; }
        public string DateModified { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Tags { get; set; }
        public List<string> RelatedArticles { get; set; }
        public List<Comment> Comments { get; set; }

        public override string GetRelevantText()
        {
            // Ignore authors, related articles and comments
            string s = Headline + "\n" + Opener + "\n" + Text + "\n";

            foreach (string tag in Tags)
            {
                s += tag + " ";
            }
            s += "\n";

            return s;
        }
    }
}


