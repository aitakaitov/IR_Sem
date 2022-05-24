using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveonik.Stemmers;


namespace Model.Preprocessing
{
    public class Stemmer : IStemmer
    {
        CzechStemmer CzechStemmer = new();

        public List<string> StemTokens(List<string> tokens)
        {
            List<string> result = new List<string>();
            foreach (string token in tokens)
            {
                result.Add(CzechStemmer.Stem(token));
            }

            return result;
        }
    }
}
