using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Preprocessing
{
    public class Stopwords : IStopwords
    {
        private List<string> StopwordsList = new List<string>();
        private AnalyzerConfig Config { get; set; } = new AnalyzerConfig();


        /// <summary>
        /// Removes stopwords from list of tokens
        /// </summary>
        /// <param name="tokens">tokens</param>
        /// <returns>cleaned tokens</returns>
        public List<string> RemoveStopwords(List<string> tokens)
        {
            List<string> validTokens = new List<string>();
            foreach (string token in tokens)
            {
                if (!StopwordsList.Contains(token))
                {
                    validTokens.Add(token);
                }
            }
            return validTokens;
        }

        /// <summary>
        /// Loads a stopwords file
        /// The stopwords file should have one word on each line
        /// </summary>
        /// <param name="file">file path</param>
        /// <param name="lowercase"></param>
        /// <param name="removeAccents"></param>
        public void LoadStopwords(string file)
        {
            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Count(); i++)
            {
                lines[i] = lines[i].Trim();

                if (Config.Lowercase)
                {
                    lines[i] = lines[i].ToLower();
                }
                if (Config.RemoveAccents)
                {
                    lines[i] = Accents.RemoveAccents(lines[i]);
                }
            }

            StopwordsList = lines.ToList();
        }

        public void SetConfig(AnalyzerConfig config)
        {
            this.Config = config;
        }
    }
}
