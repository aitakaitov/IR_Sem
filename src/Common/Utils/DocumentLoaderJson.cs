using Common.Documents;
using Common.Documents.Idnes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Common.Utils
{
    public class DocumentLoaderJson
    {
        /// <summary>
        /// Loads all JSON files for a directory into the specified IDocument
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static List<ADocument> Load<T>(string directoryPath) where T : ADocument
        {
            List<ADocument> documents = new();
            var files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                T? document = JsonConvert.DeserializeObject<T>(File.ReadAllText(file));
                if (document == null)
                {
                    Console.WriteLine($"error deserializing file {file} to class {typeof(ADocument)}");
                    continue;
                };
                documents.Add((T) document);
            }

            return documents;
        }
    }
}
