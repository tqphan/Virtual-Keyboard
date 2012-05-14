using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordPrediction
{
    class WordComparer : IEqualityComparer<Word>
    {
        public bool CaseSensitive { get; set; }
        public WordComparer()
        {
            CaseSensitive = false;
        }
        public WordComparer(bool CaseSensitive)
        {
            this.CaseSensitive = CaseSensitive;
        }
        public bool Equals(Word w1, Word w2)
        {
            if (CaseSensitive)
            {
                return w1.Term == w2.Term;
            }
            else 
            {
                if (string.Compare(w1.Term, w2.Term, true) == 0)
                    return true;
                else
                    return false; 
            }
        }
        public int GetHashCode(Word w)
        {
            if (CaseSensitive)
                return w.Term.GetHashCode();
            else
                return w.Term.ToLower(System.Globalization.CultureInfo.CurrentCulture).GetHashCode();
        }
    }
}
