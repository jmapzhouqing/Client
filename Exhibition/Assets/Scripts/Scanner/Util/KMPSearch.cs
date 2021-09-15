using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner.Util
{
    class KMPSearch<T>
    {
        private T[] pattern;
        private int[] next;

        public KMPSearch(T[] pattern){
            this.pattern = pattern;
            next = new int[pattern.Length];

            this.GetNext();
        }

        private void GetNext() {
            int j = 0, k = -1;
            next[0] = -1;
            while (j < pattern.Length - 1) {
                if (k == -1 || EqualityComparer<T>.Default.Equals(pattern[j], pattern[k]))
                {
                    next[++j] = ++k;
                }
                else {
                    k = next[k];
                }
            }
        }

        public int Search(T[] data,int offset,int length){
            int i = offset, j = 0;
            length += offset;
            length = length < data.Length ? length : data.Length;
            while (i < length && j < pattern.Length){
                if (j == -1 || EqualityComparer<T>.Default.Equals(data[i], pattern[j])){
                    i++;
                    j++;
                }else j = next[j];
            }

            if (j >= pattern.Length)
                return (i - pattern.Length); 
            else
                return -1;        
        }

        public List<int> GlobalSearch(T[] data,int offset,int length){
            List<int> value = new List<int>();
            int i = offset, j = 0;
            length += offset;
            while (i < length)
            {
                if (j != pattern.Length)
                {
                    if (j == -1 || EqualityComparer<T>.Default.Equals(data[i], pattern[j]))
                    {
                        i++;
                        j++;
                    }
                    else j = next[j];
                }
                else {
                    value.Add(i - pattern.Length);
                    j = 0;
                }  
            }

            return value;
        }
    }
}
