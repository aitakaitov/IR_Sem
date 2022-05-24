using System.Collections.Generic;

namespace Model.Preprocessing
{
    public interface IStopwords
    {
        /// <summary>
        /// Set configuration for stopwords loading
        /// </summary>
        /// <param name="config"></param>
        public void SetConfig(AnalyzerConfig config);

        /// <summary>
        /// Remove stopwords from a list of tokens
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public List<string> RemoveStopwords(List<string> tokens);
    }
}
