using System.Collections;
using System.Collections.Generic;

namespace ZemberekDotNet.Core.Text
{
    public class TextChunk : IEnumerable<string>
    {
        public readonly string id;
        public readonly int sourceIndex;
        public readonly int index;
        private readonly List<string> data;

        public TextChunk(string id, List<string> data)
        {
            this.id = id;
            this.index = 0;
            this.sourceIndex = 0;
            this.data = data;
        }

        public TextChunk(string id, int sourceIndex, int index, List<string> data)
        {
            this.id = id;
            this.sourceIndex = sourceIndex;
            this.index = index;
            this.data = data;
        }

        public string GetId()
        {
            return id;
        }

        public List<string> GetData()
        {
            return data;
        }

        public int Size()
        {
            return data.Count;
        }

        public override string ToString()
        {
            return id + "[" + sourceIndex + "-" + index + "]";
        }

        public IEnumerator<string> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
