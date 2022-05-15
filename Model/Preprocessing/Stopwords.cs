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

        public IStemmer? Stemmer { get; set; } = null;


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
                if (Config.PerformStemming)
                {
                    if (Stemmer != null)
                    {
                        lines[i] = Stemmer.StemTokens(new() { lines[i] }).ElementAt(0);
                    }
                }
            }

            StopwordsList = lines.ToList();
        }

        public void UseDefaults()
        {
            StopwordsList = new();
            foreach (string stopWord in DefaultStopWords)
            {
                string processed = stopWord;
                if (Config.Lowercase)
                {
                    processed = processed.ToLower();
                }
                if (Config.RemoveAccents)
                {
                    processed = Accents.RemoveAccents(processed);
                }
                if (Config.PerformStemming)
                {
                    if (Stemmer != null)
                    {
                        processed = Stemmer.StemTokens(new() { processed }).ElementAt(0);
                    }
                }

                StopwordsList.Add(processed);
            }
        }

        public void SetConfig(AnalyzerConfig config)
        {
            this.Config = config;
        }

        // I know this is ugly and it won't be here in the release version
        // I generated this list using a Python script which read the original stopwords file
        // https://raw.githubusercontent.com/stopwords-iso/stopwords-cs/master/raw/stop-words-czech2.txt
        private List<string> DefaultStopWords = new()
        {
            "﻿a",
            "aby",
            "aj",
            "ale",
            "ani",
            "aniž",
            "ano",
            "asi",
            "až",
            "bez",
            "bude",
            "budem",
            "budeš",
            "by",
            "byl",
            "byla",
            "byli",
            "bylo",
            "být",
            "co",
            "což",
            "cz",
            "či",
            "článek",
            "článku",
            "články",
            "další",
            "dnes",
            "do",
            "ho",
            "i",
            "já",
            "jak",
            "jako",
            "je",
            "jeho",
            "jej",
            "její",
            "jejich",
            "jen",
            "jenž",
            "ještě",
            "ji",
            "jiné",
            "již",
            "jsem",
            "jseš",
            "jsme",
            "jsou",
            "jšte",
            "k",
            "kam",
            "každý",
            "kde",
            "kdo",
            "když",
            "ke",
            "která",
            "které",
            "kterou",
            "který",
            "kteři",
            "ku",
            "ma",
            "máte",
            "me",
            "mě",
            "mezi",
            "mi",
            "mít",
            "mně",
            "mnou",
            "můj",
            "může",
            "my",
            "na",
            "ná",
            "nad",
            "nám",
            "napište",
            "náš",
            "naši",
            "ne",
            "nebo",
            "nechť",
            "nejsou",
            "není",
            "než",
            "ní",
            "nic",
            "nové",
            "nový",
            "o",
            "od",
            "ode",
            "on",
            "pak",
            "po",
            "pod",
            "podle",
            "pokud",
            "pouze",
            "práve",
            "pro",
            "proč",
            "proto",
            "protože",
            "první",
            "před",
            "přede",
            "přes",
            "při",
            "pta",
            "re",
            "s",
            "se",
            "si",
            "sice",
            "strana",
            "své",
            "svůj",
            "svých",
            "svým",
            "svými",
            "ta",
            "tak",
            "také",
            "takže",
            "tato",
            "te",
            "tě",
            "tedy",
            "těma",
            "ten",
            "tento",
            "této",
            "tím",
            "tímto",
            "tipy",
            "to",
            "to",
            "tohle",
            "toho",
            "tohoto",
            "tom",
            "tomto",
            "tomuto",
            "toto",
            "tu",
            "tuto",
            "tvůj",
            "ty",
            "tyto",
            "u",
            "už",
            "v",
            "vám",
            "váš",
            "vaše",
            "ve",
            "více",
            "však",
            "všechen",
            "vy",
            "z",
            "za",
            "zda",
            "zde",
            "ze",
            "zpět",
            "zprávy",
            "že"
        };
    }
}
