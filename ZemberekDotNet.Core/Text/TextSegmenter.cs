using System.Collections.Generic;

namespace ZemberekDotNet.Core.Text
{
    /// <summary>
    /// TextSegmenter splits block of text(without spaces) to known tokens. This class is not
    /// thread-safe
    /// </summary>
    public abstract class TextSegmenter
    {
        public static readonly int MaxTokens = 100;
        int maxTokenCount = MaxTokens;

        protected TextSegmenter()
        {
        }

        protected TextSegmenter(int maxTokenCount)
        {
            this.maxTokenCount = maxTokenCount;
        }

        public static TextSegmenter GetWordSetSegmenter(ICollection<string> words)
        {
            return new WordSetSegmenter(words);
        }

        /**
         * Retrieves all possible segmentation as a string list.
         *
         * @param textToSegment input
         */
        public List<string> FindAll(string textToSegment)
        {
            if (textToSegment.Length == 0)
            {
                return new List<string>();
            }
            List<string> results = new List<string>(2);
            LinkedList<string> buffer = new LinkedList<string>();
            Split(textToSegment, 0, 1, buffer, results, false);
            return results;
        }

        public string FindFirst(string textToSegment)
        {
            if (textToSegment.Length == 0)
            {
                return null;
            }
            List<string> results = new List<string>(2);
            LinkedList<string> buffer = new LinkedList<string>();
            Split(textToSegment, 0, 1, buffer, results, false);
            if (results.Count > 0)
            {
                return results[0];
            }
            else
            {
                return null;
            }
        }

        public void Split(string full,
            int start,
            int end,
            LinkedList<string> buffer,
            List<string> results,
            bool findSingle
        )
        {

            while (end <= full.Length)
            {
                string sub = full.Substring(start, end);
                if (Check(sub))
                {
                    if (end == full.Length)
                    {
                        if (buffer.Count < maxTokenCount)
                        {
                            if (buffer.Count == 0)
                            {
                                results.Add(sub);
                            }
                            else
                            {
                                results.Add(string.Join(" ", buffer) + " " + sub);
                            }
                            if (findSingle)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        buffer.AddLast(sub);
                        start = end;
                        end = start + 1;
                        break;
                    }
                }
                end++;
            }
            if (end > full.Length)
            { // failed to find last word.
                if (buffer.Count == 0 || start == 0) // failed to finish
                {
                    return;
                }
                string last = buffer.Last.Value;
                buffer.RemoveLast();
                start = start - last.Length;
                end = start + last.Length + 1;
            }
            Split(full, start, end, buffer, results, findSingle);
        }

        // checks if a token is
        protected abstract bool Check(string word);

        public class WordSetSegmenter : TextSegmenter
        {
            ISet<string> words;

            public WordSetSegmenter(ICollection<string> words)
            {
                this.words = new HashSet<string>(words);
            }

            public WordSetSegmenter(params string[] words)
            {
                this.words = new HashSet<string>(words);
            }

            protected override bool Check(string word)
            {
                return words.Contains(word);
            }
        }
    }
}
