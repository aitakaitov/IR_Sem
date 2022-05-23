using Common.Documents;
using Common.Documents.Basic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils
{
    public class DocumentLoaderText
    {
        /// <summary>
        /// Given a directory, loads all the text files into Documents
        /// </summary>
        /// <param name="directoryPath">directory</param>
        /// <returns>List of Documents</returns>
        public static List<ADocument> Load(string directoryPath)
        {
            List<ADocument> documents = new();
            var files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                string document = File.ReadAllText(file);
                documents.Add(new Document()
                {
                    Text = document,
                });
            }

            return documents;
        }
    }
}
