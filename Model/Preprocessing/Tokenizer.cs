using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Model.Preprocessing
{

    public class Tokenizer : ITokenizer
    {
        private IStopwords Stopwords;

        public Tokenizer(IStopwords stopwords)
        {
            Stopwords = stopwords;
        }

        // https://gist.github.com/dperini/729294
        private const string UrlRegexString =
            // protocol identifier (optional)
            // short syntax // still required
            "(?:(?:(?:https?|ftp):)?\\/\\/)" +
            // user:pass BasicAuth (optional)
            "(?:\\S+(?::\\S*)?@)?" +
            "(?:" +
            // IP address exclusion
            // private & local networks
            "(?!(?:10|127)(?:\\.\\d{1,3}){3})" +
            "(?!(?:169\\.254|192\\.168)(?:\\.\\d{1,3}){2})" +
            "(?!172\\.(?:1[6-9]|2\\d|3[0-1])(?:\\.\\d{1,3}){2})" +
            // IP address dotted notation octets
            // excludes loopback network 0.0.0.0
            // excludes reserved space >= 224.0.0.0
            // excludes network & broadcast addresses
            // (first & last IP address of each class)
            "(?:[1-9]\\d?|1\\d\\d|2[01]\\d|22[0-3])" +
            "(?:\\.(?:1?\\d{1,2}|2[0-4]\\d|25[0-5])){2}" +
            "(?:\\.(?:[1-9]\\d?|1\\d\\d|2[0-4]\\d|25[0-4]))" +
            "|" +
            // host & domain names, may end with dot
            // can be replaced by a shortest alternative
            // (?![-_])(?:[-\\w\\u00a1-\\uffff]{0,63}[^-_]\\.)+
            "(?:" +
            "(?:" +
            "[a-z0-9\\u00a1-\\uffff]" +
            "[a-z0-9\\u00a1-\\uffff_-]{0,62}" +
            ")?" +
            "[a-z0-9\\u00a1-\\uffff]\\." +
            ")+" +
            // TLD identifier name, may end with dot
            "(?:[a-z\\u00a1-\\uffff]{2,}\\.?)" +
            ")" +
            // port number (optional)
            "(?::\\d{2,5})?" +
            // resource path (optional)
            "(?:[/?#]\\S*)?";


        // Takes care of words, dates in some format etc.
        private const string DefaultRegexString =
            "([0-3]?[0-9]\\.[0-1]?[0-9]\\.[0-9]?[0-9]?[0-9]{2})|([0-3]?[0-9]\\.[0-1]?[0-9]\\.)" +  // date in dd.mm.yyyy or dd.mm.
            "|(" + UrlRegexString + ")" +
            "|(\\d+[.,](\\d+)?)" +
            "|([\\p{L}\\*\\d^[:\\r\\n]]+)" +
            "|(<.*?>)" +
            "|([\\p{P}])" + 
            "|\\s+";


        public List<string> Tokenize(string text)
        {
            text = Regex.Replace(text, "\n", " ");
            text = Regex.Replace(text, "\\s+", " ");
            var tokens = Regex.Split(text, DefaultRegexString, RegexOptions.Compiled);
            var tokenList = FilterTokens(tokens);

            return tokenList;
        }

        private List<string> FilterTokens(string[] tokens)
        {
            List<string> filtered = new();
            foreach (var token in tokens)
            {   
                if (token.Trim().Length == 0)
                {
                    continue;
                }
                else if (token.Length == 1)
                {
                    if (Char.IsPunctuation(token[0]))
                    {
                        continue;
                    }
                }
                filtered.Add(token.Trim());
            }

            return filtered;
        }
    }
}
