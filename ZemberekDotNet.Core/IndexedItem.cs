using System.Collections.Generic;

namespace ZemberekDotNet.Core
{
    /// <summary>
    ///  * An item with an index. This is useful when we want to keep track of a sequence of items processed
    ///  by a system that would change the processing order.
    /// </summary>
    public class IndexedItem<T> : IComparer<IndexedItem<T>>
    {
        public readonly T Item;
        public readonly int Index;

        public IndexedItem(T item, int index)
        {
            this.Item = item;
            this.Index = index;
        }

        public int Compare(IndexedItem<T> x, IndexedItem<T> y)
        {
            if (x.Index > Index)
            {
                return 1;
            }
            else if (x.Index < Index)
            {
                return -1;
            }
            return 0;
        }

        public override string ToString()
        {
            return Index + ":" + Item.ToString();
        }
    }
}
