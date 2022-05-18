using Common.Documents;
using Common.Documents.Basic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Indexing;
using Model.Preprocessing;
using Model.Queries;
using System;
using System.Collections.Generic;

namespace Test
{
    [TestClass]
    public class ModelTests
    {
        private const double EPS = 0.001;

        [TestMethod]
        public void TokenizerTest()
        {
            string text = "plzeň je krásné město. a je to krásné místo? ostrava, je ošklivé: místo praha je také krásné město - plzeň je hezčí";
            List<string> expectedTokensNoStopwords = new()
            {
                "plzeň",
                "je",
                "krásné",
                "město",
                "a",
                "je",
                "to",
                "krásné",
                "místo",
                "ostrava",
                "je",
                "ošklivé",
                "místo",
                "praha",
                "je",
                "také",
                "krásné",
                "město",
                "plzeň",
                "je",
                "hezčí"
            };   

            Stopwords stopwordsFilled = new();
            stopwordsFilled.SetConfig(new()
            {
                PerformStemming = false,
                Lowercase = true,
                RemoveAccents = false
            });
            stopwordsFilled.UseDefaults();

            Tokenizer tokenizer = new();

            var tokenized = tokenizer.Tokenize(text);

            Assert.AreEqual(tokenized.Count, expectedTokensNoStopwords.Count);
            for (int i = 0; i < tokenized.Count; i++)
            {
                Assert.AreEqual(expectedTokensNoStopwords[i], tokenized[i]);
            }
        }

        [TestMethod]
        public void AccentTest()
        {
            List<string> accentedTexts = new()
            {
                "crème brûlée",
                "áčďéěíňóřšťúůýž",
                "Bonjour ça va? C'est l'été! Ich möchte ä Ä á à â ê é è ë Ë É ï Ï î í ì ó ò ô ö Ö Ü ü ù ú û Û ý Ý ç Ç ñ Ñ"
            };
            List<string> expectedTexts = new()
            {
                "creme brulee",
                "acdeeinorstuuyz",
                "Bonjour ca va? C'est l'ete! Ich mochte a A a a a e e e e E E i I i i i o o o o O U u u u u U y Y c C n N"
            };

            for (int i = 0; i < accentedTexts.Count; i++)
            {
                string processedText = Accents.RemoveAccents(accentedTexts[i]);
                Assert.AreEqual(expectedTexts[i], processedText);
            }
        }

        [TestMethod]
        public void StopwordsTest()
        {
            List<string> tokens = new()
            {
                "plzeň",
                "je",
                "krásné",
                "město",
                "a",
                "je",
                "to",
                "krásné",
                "místo"
            };

            List<string> expectedTokens = new()
            {
                "plzeň",
                "krásné",
                "město",
                "krásné",
                "místo"
            };

            Stopwords stopwords = new Stopwords();
            stopwords.SetConfig(new()
            {
                Lowercase = true,
                RemoveAccents = false,
                PerformStemming = false
            });
            stopwords.UseDefaults();

            List<string> processedTokens = stopwords.RemoveStopwords(tokens);

            Assert.AreEqual(expectedTokens.Count, processedTokens.Count);
            for (int i = 0; i < processedTokens.Count; i++)
            {
                Assert.AreEqual(expectedTokens[i], processedTokens[i]);
            }
        }

        [TestMethod]
        public void BooleanSearchTest()
        {
            List<IDocument> documents = new()
            {
                new Document() { Text = "tropical fish include fish found in tropical enviroments" },
                new Document() { Text = "fish live in a sea" },
                new Document() { Text = "tropical fish are popular aquarium fish" },
                new Document() { Text = "fish also live in Czechia" },
                new Document() { Text = "Czechia is a country" }
            };

            List<int> relevantIndicesQuery1 = new() { 1, 3, 4 };
            List<int> relevantIndicesQuery2 = new() { 1, 3, 4 };
            List<int> relevantIndicesQuery3 = new() { };
            List<int> relevantIndicesQuery4 = new() { 0, 2 };
            List<int> relevantIndicesQuery5 = new() { 1, 3, 4 };
            List<int> relevantIndicesQuery6 = new() { 1, 3 };

            List<List<int>> relevantIndicesList = new()
            {
                relevantIndicesQuery1,
                relevantIndicesQuery2,
                relevantIndicesQuery3,
                relevantIndicesQuery4,
                relevantIndicesQuery5,
                relevantIndicesQuery6
            };

            BasicQuery query1 = new BasicQuery() { QueryText = "czechia sea", TopCount = 5 };
            BasicQuery query2 = new BasicQuery() { QueryText = "czechia OR sea", TopCount = 5 };
            BasicQuery query3 = new BasicQuery() { QueryText = "czechia AND sea", TopCount = 5 };
            BasicQuery query4 = new BasicQuery() { QueryText = "(sea OR fish) AND tropical", TopCount = 5 };
            BasicQuery query5 = new BasicQuery() { QueryText = "NOT tropical", TopCount = 5 };
            BasicQuery query6 = new BasicQuery() { QueryText = "(sea OR fish) AND NOT tropical", TopCount = 5 };

            List<BasicQuery> queries = new()
            {
                query1,
                query2,
                query3,
                query4,
                query5,
                query6
            };


            InvertedIndex index = Utils.CreateInvertedIndexForTestData();
            index.Index(documents);

            for (int i = 0; i < queries.Count; i++)
            {
                var query = queries[i];
                var relevantIndices = relevantIndicesList[i];

                var res = index.BooleanSearch(query);
                Assert.AreEqual(res.Item1.Count, relevantIndices.Count);

                // The results are sorted by DocumentId, which is the order in which the documents are indexed (aka passed to the Index() method)
                for (int j = 0; j < relevantIndices.Count; j++)
                {
                    Assert.AreEqual(res.Item1[j].GetRelevantText(), documents[relevantIndices[j]].GetRelevantText());
                }
            }
        }

        /// <summary>
        /// Test for IR TF-IDF dataset 1 with verified results
        /// </summary>
        [TestMethod]
        public void VectorSearchTest_Set1()
        {
            List<double> expectedCosines = new() { 0.3699, 0.2827 };

            InvertedIndex index = Utils.CreateInvertedIndexForTestData();

            List<IDocument> documents = new()
            {
                new Document() { Text = "Plzeň je krásné město a je to krásné místo" },
                new Document() { Text = "Ostrava je ošklivé místo" },
                new Document() { Text = "Praha je také krásné město Plzeň je hezčí" }
            };

            BasicQuery q = new() { QueryText = "krásné město", TopCount = 3 };

            index.Index(documents);

            var res = index.VectorSpaceSearch(q);

            // Check the top document
            Assert.AreEqual("Plzeň je krásné město a je to krásné místo", res.Item1[0].GetRelevantText());

            // Check if one docuent has 0.0 similarity and is not returned
            Assert.AreEqual(2, res.Item1.Count);

            // Check cosine distances - only two since the 
            for (int i = 0; i < expectedCosines.Count; i++)
            {
                Assert.AreEqual(expectedCosines[i], res.Item3[i], EPS);
            }
        }

        /// <summary>
        /// Test for IR TF-IDF dataset 2 with verified results
        /// </summary>
        [TestMethod]
        public void VectorSearchTest_Set2()
        {
            List<double> expectedCosinesQuery1 = new() { 0.6613, 0.2009, 0.1644, 0.0125 };
            List<double> expectedCosinesQuery2 = new() { 0.3973, 0.3252, 0.0247, 0.0247 };

            InvertedIndex index = Utils.CreateInvertedIndexForTestData();

            List<IDocument> documents = new()
            {
                new Document() { Text = "tropical fish include fish found in tropical enviroments" },
                new Document() { Text = "fish live in a sea" },
                new Document() { Text = "tropical fish are popular aquarium fish" },
                new Document() { Text = "fish also live in Czechia" },
                new Document() { Text = "Czechia is a country" }
            };

            index.Index(documents);


            BasicQuery query1 = new() { QueryText = "tropical fish sea", TopCount = 5 };
            var res = index.VectorSpaceSearch(query1);

            Assert.AreEqual("fish live in a sea", res.Item1[0].GetRelevantText());

            // Check if one docuent has 0.0 similarity and is not returned
            Assert.AreEqual(4, res.Item1.Count);

            // Check cosine distances - only two since the 
            for (int i = 0; i < expectedCosinesQuery1.Count; i++)
            {
                Assert.AreEqual(expectedCosinesQuery1[i], res.Item3[i], EPS);
            }


            BasicQuery query2 = new() { QueryText = "tropical fish", TopCount = 5 };
            res = index.VectorSpaceSearch(query2);

            Assert.AreEqual("tropical fish include fish found in tropical enviroments", res.Item1[0].GetRelevantText());

            // Check if one docuent has 0.0 similarity and is not returned
            Assert.AreEqual(4, res.Item1.Count);

            // Check cosine distances - only two since the 
            for (int i = 0; i < expectedCosinesQuery2.Count; i++)
            {
                Assert.AreEqual(expectedCosinesQuery2[i], res.Item3[i], EPS);
            }
        }
    }

    class Utils
    {
        public static InvertedIndex CreateInvertedIndexForTestData()
        {
            AnalyzerConfig config = new()
            {
                PerformStemming = false,
                RemoveAccents = false,
                Lowercase = true
            };

            Stopwords stopwords = new();
            stopwords.SetConfig(config);
            Tokenizer tokenizer = new();
            Analyzer analyzer = new(tokenizer, null, stopwords, config);
            InvertedIndex index = new(analyzer, new(), "Index");

            return index;
        }
    }
}