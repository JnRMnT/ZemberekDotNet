using System;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Native.Collections;
using static ZemberekDotNet.NER.NerDataSet;

namespace ZemberekDotNet.NER
{
    public class NerSentence
    {
        internal string content;
        internal List<NerToken> tokens;

        public NerSentence(String content, List<NerToken> tokens)
        {
            this.content = content;
            this.tokens = tokens;
        }

        public List<NamedEntity> GetNamedEntities()
        {
            return GetAllEntities().Where(s => !s.type.Equals(NerDataSet.OutTokenType)).ToList();
        }

        public List<NamedEntity> GetAllEntities()
        {
            List<NamedEntity> namedEntities = new List<NamedEntity>();
            List<NerToken> neTokens = new List<NerToken>();
            foreach (NerToken token in tokens)
            {
                if (token.position == NePosition.UNIT
                    || token.position == NePosition.LAST ||
                    token.position == NePosition.OUTSIDE)
                {
                    neTokens.Add(token);
                    namedEntities.Add(new NamedEntity(token.type, neTokens));
                    neTokens = new List<NerToken>();
                    continue;
                }
                neTokens.Add(token);
            }
            return namedEntities;
        }

        public string GetAsTrainingSentence(AnnotationStyle style)
        {
            List<NamedEntity> all = GetAllEntities();
            List<String> tokens = new List<string>(); ;
            foreach (NamedEntity namedEntity in all)
            {
                if (namedEntity.type.Equals(OutTokenType))
                {
                    tokens.Add(namedEntity.tokens[0].word);
                }
                else
                {
                    switch (style)
                    {
                        case AnnotationStyle.ENAMEX:
                            tokens.Add("<b_enamex TYPE=\"" + namedEntity.type + "\">");
                            break;
                        case AnnotationStyle.OPEN_NLP:
                            tokens.Add("<START:" + namedEntity.type + ">");
                            break;
                        case AnnotationStyle.BRACKET:
                            tokens.Add("[" + namedEntity.type);
                            break;
                    }
                    foreach (NerToken token in namedEntity.tokens)
                    {
                        tokens.Add(token.word);
                    }
                    switch (style)
                    {
                        case AnnotationStyle.ENAMEX:
                            tokens.Add("<e_enamex>");
                            break;
                        case AnnotationStyle.OPEN_NLP:
                            tokens.Add("<END>");
                            break;
                        case AnnotationStyle.BRACKET:
                            tokens.Add("]");
                            break;
                    }
                }
            }
            return string.Join(" ", tokens);
        }

        public List<NamedEntity> MatchingNEs(List<NamedEntity> nes)
        {
            LinkedHashSet<NamedEntity> set = new LinkedHashSet<NamedEntity>(GetNamedEntities());
            List<NamedEntity> result = new List<NamedEntity>();
            foreach (NamedEntity ne in nes)
            {
                if (set.Contains(ne))
                {
                    result.Add(ne);
                }
            }
            return result;
        }

        public override string ToString()
        {
            return "NerSentence{" +
                "content='" + content + '\'' +
                ", tokens=" + tokens +
                '}';
        }


        public override bool Equals(Object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || !GetType().Equals(o.GetType()))
            {
                return false;
            }

            NerSentence that = (NerSentence)o;

            if (!content.Equals(that.content))
            {
                return false;
            }
            return tokens.Equals(that.tokens);
        }

        public override int GetHashCode()
        {
            int result = content.GetHashCode();
            result = 31 * result + tokens.GetHashCode();
            return result;
        }
    }
}
