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
        public static List<IDocument> Load(string directoryPath)
        {
            List<IDocument> documents = new();
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
