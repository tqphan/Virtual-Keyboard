using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows;

namespace WordPrediction
{
    class WordPredictor
    {
        public List<char> ValidCharacters { get; protected set; }
        public List<char> ResetCharacters { get; protected set; }

        public bool IsCaseSensitive { get; set; }
        protected Trie<Word> trie = new Trie<Word>();
        protected string currentWord = "";
        protected List<Ziyi.WordCompleteKey> wordCompleteKeys = new List<Ziyi.WordCompleteKey>();
        protected Dictionary<Word, ushort> newWords = new Dictionary<Word, ushort>(new WordComparer());
        protected List<Word> predictions = new List<Word>();
        public List<Word> Predictions
        {
            get
            {
                return this.predictions;
            }
            protected set
            {
                this.predictions = value;
            }
        }
        public int MinimumPredictions { get; protected set; }

        public WordPredictor(List<Ziyi.WordCompleteKey> wcKeys)
            : this()
        {
            this.wordCompleteKeys = wcKeys; 
            this.MinimumPredictions = wcKeys.Count;
            this.predictions.Clear(); 
            for (int i = 0; i < this.MinimumPredictions; i++)
            {
                this.predictions.Add(new Word()); 
            }
        }

        public WordPredictor()
        {
            this.MinimumPredictions = 0; 
            this.ValidCharacters = new List<char>();
            this.ResetCharacters = new List<char>(); 
            this.IsCaseSensitive = false; 
        }

        private void PopulatePredictions(List<Word> lw)
        {
            lw = lw.OrderByDescending(w => w.OverallFrequency).ThenBy(w => w.Term).ToList(); 
            for (int i = 0; i < this.wordCompleteKeys.Count; i++)
            {
                Word w = new Word();
                if (i < lw.Count)
                    this.predictions[i] = lw[i];
                else
                    this.predictions[i] = w; 

                this.wordCompleteKeys[i].Content = this.predictions[i].Term;
                this.wordCompleteKeys[i].SubstringIndex = this.currentWord.Length; 
            }
        }


        public List<Word> Predict(string input)
        {
            foreach (char ch in input)
            {
                char originalChar = ch;
                char charToUse = ch; 
                if (!this.IsCaseSensitive)
                    charToUse = char.ToLower(ch, System.Globalization.CultureInfo.CurrentCulture); 

                // This is the character we're interested in, so 
                if (IsValidCharacter(charToUse))
                {
                    
                    this.currentWord = String.Concat(this.currentWord, originalChar);

                    this.trie.Matcher.NextMatch(charToUse);

                    if (string.Compare(this.trie.Matcher.GetPrefix(), this.currentWord, !this.IsCaseSensitive) == 0)
                    {
                        PopulatePredictions(this.trie.Matcher.GetPrefixMatches());
                    }
                    else
                    {
                        PopulatePredictions(new List<Word>());
                    }
                }
                else if (IsResetCharacter(charToUse))
                {
                    Word w = new Word(this.currentWord, 1, 1); 
                    ushort freq;

                    if (this.newWords.TryGetValue(w, out freq))
                    {
                        if (freq >= Ziyi.Properties.Settings.Default.NewWordAdditionTheshold)
                        {
                            if(this.IsCaseSensitive)
                                this.trie.Put(w.Term, w);
                            else
                                this.trie.Put(w.Term.ToLower(System.Globalization.CultureInfo.CurrentCulture), w);
                            this.newWords.Remove(w);
                            Console.WriteLine("Removed frm dict: {0}", w.Term); 
                        }
                        else
                        {
                            this.newWords[w] += 1;
                            Console.WriteLine("Frequ ++ {0}", w.Term); 
                        }

                    }
                    else
                    {
                        
                        this.newWords.Add(w, 1);
                        Console.WriteLine("Added to dict: {0}", w.Term); 
                    }
                    ResetAndPopulate(); 
                }
                // '\b' is backspace char
                else if (ch == '\b')
                {
                    //it's backspace so remove the last character from buffer if the buffer isn't empty 
                    if (this.currentWord != String.Empty)
                    {
                        this.currentWord = this.currentWord.Substring(0, this.currentWord.Length - 1);
                    }

                    //if the string in the buffer doesn't match any predictions ...
                    if (this.currentWord.Length > this.trie.Matcher.GetPrefix().Length)
                    {
                        //return an empty list of words
                        PopulatePredictions(new List<Word>());
                    }
                    //there are predictions for the string in the buffer, so...
                    else if (this.currentWord.Length == this.trie.Matcher.GetPrefix().Length)
                    {
                        //return the predictions list
                        PopulatePredictions(this.trie.Matcher.GetPrefixMatches());
                    }
                    //the buffer string is shorter than the trie's buffer so...
                    else
                    {
                        //
                        this.trie.Matcher.BackMatch();
                        PopulatePredictions(this.trie.Matcher.GetPrefixMatches());
                    }
                }
            }

            return this.predictions; 
        }

        public bool IsResetCharacter(char input)
        {
            return (this.ResetCharacters.Exists(element => element == input));
        }
        public bool IsResetCharacters(string input)
        {
            foreach (char ch in input)
            {
                if (!IsResetCharacter(ch))
                    return false;
            }
            return true;
        }
        public bool IsValidCharacter(char input)
        {
            return (this.ValidCharacters.Exists(element => element == input));
        }
        public bool IsValidCharacters(string input)
        {
            foreach (char ch in input)
            {
                if (!IsValidCharacter(ch))
                    return false;
            }
            return true;
        }
        public void WordSimulated(string term)
        {
            Reset(); 
            foreach (char ch in term)
            {
                this.trie.Matcher.NextMatch(ch);
            }
            if (this.trie.Matcher.IsExactMatch())
            {
                Word w = this.trie.Matcher.GetExactMatch();
                if (w.OverallFrequency < ushort.MaxValue)
                    w.OverallFrequency++;
                if (w.CurrentFrequency < ushort.MaxValue)
                    w.CurrentFrequency++;
            }
            ResetAndPopulate();
        }
        private void Reset()
        {
            this.currentWord = "";  
            this.trie.Matcher.ResetMatch();
        }
        public void ResetAndPopulate()
        {
            Reset(); 
            PopulatePredictions(new List<Word>());
        }

        public bool SaveDictionary(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement dictionary = (XmlElement)doc.AppendChild(doc.CreateElement("dictionary"));
            //el.SetAttribute("Bar", "some & value");
            XmlElement settings = doc.CreateElement("settings");
            dictionary.AppendChild(settings);
            XmlElement case_sensitive = doc.CreateElement("case_sensitive");
            case_sensitive.InnerText = this.IsCaseSensitive.ToString().ToLower();
            settings.AppendChild(case_sensitive);
            XmlElement valid_characters = doc.CreateElement("valid_characters");
            string str = "";
            foreach (char ch in this.ValidCharacters)
            {
                str = string.Concat(str, ch);
            }
            valid_characters.InnerText = str;
            settings.AppendChild(valid_characters);
            XmlElement reset_characters = doc.CreateElement("reset_characters");
            str = "";
            foreach (char ch in this.ResetCharacters)
            {
                str = string.Concat(str, ch);
            }
            reset_characters.InnerText = str;
            settings.AppendChild(reset_characters);

            List<Word> lw = trie.Matcher.GetAllMatches();
            foreach (Word w in lw)
            {
                XmlElement word = doc.CreateElement("word");
                XmlElement term = doc.CreateElement("term");
                term.InnerText = w.Term;
                XmlElement frequency = doc.CreateElement("frequency");
                frequency.InnerText = w.OverallFrequency.ToString();
                word.AppendChild(term);
                word.AppendChild(frequency);
                dictionary.AppendChild(word);
            }

            XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            doc.Save(writer);
            return true; 
        }

        public bool LoadDictionary (string flieName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (!File.Exists(flieName))
                return false;
            xmlDoc.Load(flieName);
            if (!xmlDoc.DocumentElement.HasChildNodes)
                return false;
             
            XmlNodeList dictionaryNode = xmlDoc.DocumentElement.ChildNodes;
            Trie<Word> tempTrie = new Trie<Word>(); 
            for (int i = 0; i < dictionaryNode.Count; i++)
            {
                if (dictionaryNode[i].Name == "word")
                {
                    if (dictionaryNode[i].HasChildNodes)
                    {
                        XmlNodeList wordNodes = dictionaryNode[i].ChildNodes;
                        Word word = new Word(); 
                        for (int j = 0; j < wordNodes.Count; j++)
                        {
                            if (wordNodes[j].Name == "term")
                            {
                                word.Term = wordNodes[j].InnerText; 
                            }
                            else if (wordNodes[j].Name == "frequency")
                            {
                                ushort f = 0; 
                                ushort.TryParse(wordNodes[j].InnerText, out f); 
                                word.OverallFrequency = f; 
                            }
                        }
                        if (this.IsCaseSensitive)
                            tempTrie.Put(word.Term, word); 
                        else
                            tempTrie.Put(word.Term.ToLower(System.Globalization.CultureInfo.CurrentCulture), word); 
                    }
                }
                else if (dictionaryNode[i].Name == "settings")
                {
                    if (dictionaryNode[i].HasChildNodes)
                    {
                        XmlNodeList settingsNodes = dictionaryNode[i].ChildNodes;
                        for (int j = 0; j < settingsNodes.Count; j++)
                        {
                            if (settingsNodes[j].Name == "case_sensitive")
                            {
                                if (settingsNodes[j].InnerText == "true")
                                    this.IsCaseSensitive = true;
                                else if (settingsNodes[j].InnerText == "false")
                                    this.IsCaseSensitive = false; 
                            }
                            else if (settingsNodes[j].Name == "valid_characters")
                            {
                                this.ValidCharacters.Clear();
                                string s = settingsNodes[j].InnerText;
                                foreach (char ch in s)
                                   this.ValidCharacters.Add(ch); 
                            }
                            else if (settingsNodes[j].Name == "reset_characters")
                            {
                                this.ResetCharacters.Clear();
                                foreach (char ch in settingsNodes[j].InnerText)
                                    this.ResetCharacters.Add(ch); 
                            }
                        }
                    }
                }
            }
            this.ValidCharacters.Sort();
            this.ResetCharacters.Sort(); 
            this.trie = tempTrie; 
            return true; 
        }    
    }
}
