using System;
using System.Collections.Generic;

namespace ZemberekDotNet.Core.Text
{
    public class TextConsumer
    {
        List<string> content;
        int cursor = 0;

        public TextConsumer(List<string> content)
        {
            this.content = content;
        }

        public List<string> MoveUntil(Predicate<string> predicate)
        {
            List<string> consumed = new List<string>();

            while (!Finished())
            {
                String line = content[cursor];
                if (predicate.Invoke(line))
                {
                    return consumed;
                }
                consumed.Add(line);
                cursor++;
            }

            return consumed;
        }

        public bool Finished()
        {
            return cursor >= content.Count;
        }

        public string Current()
        {
            return content[cursor];
        }

        public void Advance()
        {
            if (!Finished())
            {
                cursor++;
            }
        }

        public string GetAndAdvance()
        {
            String r = Current();
            Advance();
            return r;
        }

        public List<string> GetUntilEnd()
        {
            List<string> result = new List<string>();
            while (!Finished())
            {
                result.Add(GetAndAdvance());
            }
            return result;
        }
    }
}
