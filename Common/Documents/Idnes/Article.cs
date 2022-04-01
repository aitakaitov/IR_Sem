using System;
using System.Collections.Generic;

namespace Common.Documents.Idnes
{
    public class Article : IDocument
    {
        public Article(string headline, string opener, string text, string published, string modified,
            List<string> authors, List<string> tags, List<string> related, List<Comment> comments)
        {
            this.Opener = opener;
            this.Headline = headline;
            this.Authors = authors;
            this.Comments = comments;
            this.RelatedArticles = related;
            this.Text = text;
            this.DatePublished = published;
            this.DateModified = modified;
            this.Tags = tags;
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

        public string GetRelevantText()
        {
            string s = Headline + "\n" + Opener + "\n" + Text + "\n";

            foreach (string author in Authors)
            {
                s += author + " ";
            }
            s += "\n";


            foreach (string tag in Tags)
            {
                s += tag + " ";
            }
            s += "\n";


            foreach (string related in RelatedArticles)
            {
                s += related + "\n";
            }


            foreach (Comment comment in Comments)
            {
                s += comment.GetRelevantText() + "\n";
            }

            return s;
        }
    }
}


