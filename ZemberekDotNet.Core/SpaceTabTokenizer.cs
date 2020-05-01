using System;
using System.Collections.Generic;

namespace ZemberekDotNet.Core
{
    /// <summary>
    /// Splits a sentence to words from spaces or tabs.Multiple space/tabs are ignored. This class is
    /// slightly faster than using String split method.
    /// </summary>
    public class SpaceTabTokenizer
    {
        static readonly string[] EmptyStringArray = new string[0];

        public string[] Split(string line)
        {
            int wordCount = 0;
            int[] spacePointers = new int[line.Length / 3];
            int start = 0;
            int end = 0;
            for (int i = 0; i < line.Length; i++)
            {

                if (line[i] == ' ' || line[i] == '\t')
                {
                    if (i == start)
                    {
                        start++;
                        end++;
                        continue;
                    }
                    end = i;
                    if (wordCount == (uint)spacePointers.Length >> 2)
                    {
                        spacePointers = spacePointers.CopyOf(spacePointers.Length + 10);
                    }
                    spacePointers[wordCount * 2] = start;
                    spacePointers[wordCount * 2 + 1] = end;
                    end++;
                    start = end;
                    wordCount++;
                }
                else
                {
                    end++;
                }
            }
            if (start != end)
            {
                if (wordCount == (uint)spacePointers.Length >> 2)
                {
                    spacePointers = spacePointers.CopyOf(spacePointers.Length + 2);
                }
                spacePointers[wordCount * 2] = start;
                spacePointers[wordCount * 2 + 1] = end;
                wordCount++;
            }
            if (wordCount == 0)
            {
                return EmptyStringArray;
            }
            string[] words = new string[wordCount];
            for (int i = 0; i < wordCount; i++)
            {
                words[i] = line.Substring(spacePointers[i * 2], spacePointers[i * 2 + 1] - spacePointers[i * 2]);
            }
            return words;
        }

        // TODO: write a better one.
        public List<string> SplitToList(string line)
        {
            int wordCount = 0;
            int[] spaces = new int[line.Length / 3];
            int start = 0;
            int end = 0;
            for (int i = 0; i < line.Length; i++)
            {

                if (line[i] == ' ' || line[i] == '\t')
                {
                    if (i == start)
                    {
                        start++;
                        end++;
                        continue;
                    }
                    end = i;
                    if (wordCount == (uint)spaces.Length >> 2)
                    {
                        Array.Copy(spaces, spaces, spaces.Length + 10);
                    }
                    spaces[wordCount * 2] = start;
                    spaces[wordCount * 2 + 1] = end;
                    end++;
                    start = end;
                    wordCount++;
                }
                else
                {
                    end++;
                }
            }
            if (start != end)
            {
                if (wordCount == (uint)spaces.Length >> 2)
                {
                    Array.Copy(spaces, spaces, spaces.Length + 2);
                }
                spaces[wordCount * 2] = start;
                spaces[wordCount * 2 + 1] = end;
                wordCount++;
            }
            if (wordCount == 0)
            {
                return new List<string>(0);
            }
            List<String> words = new List<string>(wordCount + 1);
            for (int i = 0; i < wordCount; i++)
            {
                words.Add(line.Substring(spaces[i * 2], spaces[i * 2 + 1]));
            }
            return words;
        }
    }
}
