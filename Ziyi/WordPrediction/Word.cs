using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordPrediction
{
    class Word : IComparable<Word>
    {
        #region Protected Data

        private string term = string.Empty; 
        private ushort currentFrequency = 0;
        private ushort overallFrequency = 0;

        #endregion 

        #region Constructors

        public Word()
        {
        }
        public Word(string term, ushort cfreq, ushort ofreq)
        {
            this.term = term;
            this.currentFrequency = cfreq; 
            this.overallFrequency = ofreq;
        }

        #endregion 

        #region IComparable

        public static Comparison<Word> OverallFrequencyComparison = delegate(Word w1, Word w2)
        {
            return w1.overallFrequency.CompareTo(w2.overallFrequency);
        };

        public static Comparison<Word> CurrentFrequencyComparison = delegate(Word w1, Word w2)
        {
            return w1.currentFrequency.CompareTo(w2.currentFrequency);
        }; 

        public int CompareTo(Word other)
        {
            return term.CompareTo(other.term);
        }

        #endregion 

        #region Properties 

        public string Term
        {
            get
            {
                return this.term;
            }
            set
            {
                this.term = value;
            }
        }
        public ushort OverallFrequency
        {
            get
            {
                return this.overallFrequency;
            }
            set
            {
                this.overallFrequency = value;
            }
        }

        public ushort CurrentFrequency
        {
            get
            {
                return this.currentFrequency;
            }
            set
            {
                this.currentFrequency = value;
            }
        }

        #endregion 

        #region Overrides 

        public override string ToString()
        {
            return this.Term;
        }

        #endregion 
    }
}
