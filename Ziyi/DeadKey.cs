using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziyi
{
    public class DeadKey
    {
        private char m_deadchar;
        private IEnumerator<KeyValuePair<ushort, char>> dk_enum;

        private Dictionary<ushort, char> m_rgdeadkeys = new Dictionary<ushort, char>();

        public DeadKey(char deadCharacter)
        {
            this.m_deadchar = deadCharacter;
        }

        public char DeadCharacter
        {
            get
            {
                return this.m_deadchar;
            }
        }

        public void AddDeadKeyRow(uint scancode, int caps, ShiftState ss,
                                  char combinedCharacter)
        {
            char value;
            ushort icaps, uindex, iss, isc;
            if (caps == 1)
                icaps = 16;
            else
                icaps = 0;
            isc = Convert.ToUInt16(scancode);
            isc *= 256;
            iss = Convert.ToUInt16((int)ss);
            uindex = (ushort)(isc + iss + icaps);

            if (this.m_rgdeadkeys.TryGetValue(uindex, out value))
            {
                this.m_rgdeadkeys[uindex] = combinedCharacter;
            }
            else
            {
                this.m_rgdeadkeys.Add(uindex, combinedCharacter);
            }
        }

        public int Count
        {
            get
            {
                return this.m_rgdeadkeys.Count;
            }
        }

        public void GetDeadKeyInfo(ref uint scss, out char value)
        {
            if (scss == 0)
            {
                // Initial call to set up enumerator
                dk_enum = this.m_rgdeadkeys.GetEnumerator();
                value = (char)0;
                return;
            }
            dk_enum.MoveNext();
            scss = dk_enum.Current.Key;
            value = dk_enum.Current.Value;
        }
    }
}
