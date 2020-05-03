using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Analysis
{
    public class SurfaceTransition
    {
        public readonly string surface;
        // TODO: this can be removed if SearchPath contains StemTransition.
        public readonly MorphemeTransition lexicalTransition;

        public SurfaceTransition(string surface, MorphemeTransition transition)
        {
            this.surface = surface;
            this.lexicalTransition = transition;
        }

        public bool IsDerivative()
        {
            return lexicalTransition.to.derivative;
        }

        public MorphemeState GetState()
        {
            return lexicalTransition.to;
        }

        public Morpheme GetMorpheme()
        {
            return lexicalTransition.to.morpheme;
        }

        public bool isDerivationalOrRoot()
        {
            return GetState().derivative || GetState().posRoot;
        }

        public override string ToString()
        {
            return SurfaceString() + GetState().id;
        }

        public string ToMorphemeString()
        {
            return SurfaceString() + GetState().morpheme.Id;
        }

        private string SurfaceString()
        {
            return surface.IsEmpty() ? "" : surface + ":";
        }

        static TurkishAlphabet alphabet = TurkishAlphabet.Instance;

        public static string GenerateSurface(
            SuffixTransition transition,
            AttributeSet<PhoneticAttribute> phoneticAttributes)
        {
            string cached = transition.GetFromSurfaceCache(phoneticAttributes);
            if (cached != null)
            {
                return cached;
            }

            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (SuffixTemplateToken token in transition.GetTokenList())
            {
                AttributeSet<PhoneticAttribute> attrs =
                    AttributesHelper.GetMorphemicAttributes(sb.ToString().ToCharArray(), phoneticAttributes);
                switch (token?.type)
                {
                    case TemplateTokenType.LETTER:
                        sb.Append(token.letter);
                        break;

                    case TemplateTokenType.A_WOVEL:
                        // TODO: document line below.
                        if (index == 0 && phoneticAttributes.Contains(PhoneticAttribute.LastLetterVowel))
                        {
                            break;
                        }
                        if (attrs.Contains(PhoneticAttribute.LastVowelBack))
                        {
                            sb.Append('a');
                        }
                        else if (attrs.Contains(PhoneticAttribute.LastVowelFrontal))
                        {
                            sb.Append('e');
                        }
                        else
                        {
                            throw new ArgumentException("Cannot generate A form! ");
                        }
                        break;

                    case TemplateTokenType.I_WOVEL:
                        // TODO: document line below. With templates like +Im this would not be necessary
                        if (index == 0 && phoneticAttributes.Contains(PhoneticAttribute.LastLetterVowel))
                        {
                            break;
                        }
                        if (attrs.Contains(PhoneticAttribute.LastVowelFrontal) && attrs.Contains(PhoneticAttribute.LastVowelUnrounded))
                        {
                            sb.Append('i');
                        }
                        else if (attrs.Contains(PhoneticAttribute.LastVowelBack) && attrs.Contains(PhoneticAttribute.LastVowelUnrounded))
                        {
                            sb.Append('ı');
                        }
                        else if (attrs.Contains(PhoneticAttribute.LastVowelBack) && attrs.Contains(PhoneticAttribute.LastVowelRounded))
                        {
                            sb.Append('u');
                        }
                        else if (attrs.Contains(PhoneticAttribute.LastVowelFrontal) && attrs.Contains(PhoneticAttribute.LastVowelRounded))
                        {
                            sb.Append('ü');
                        }
                        else
                        {
                            throw new ArgumentException("Cannot generate I form!");
                        }
                        break;

                    case TemplateTokenType.APPEND:
                        if (attrs.Contains(PhoneticAttribute.LastLetterVowel))
                        {
                            sb.Append(token.letter);
                        }
                        break;

                    case TemplateTokenType.DEVOICE_FIRST:
                        char ld = token.letter;
                        if (attrs.Contains(PhoneticAttribute.LastLetterVoiceless))
                        {
                            ld = alphabet.devoice(ld);
                        }
                        sb.Append(ld);
                        break;

                    case TemplateTokenType.LAST_VOICED:
                    case TemplateTokenType.LAST_NOT_VOICED:
                        ld = token.letter;
                        sb.Append(ld);
                        break;
                }
                index++;
            }
            String s = sb.ToString();
            transition.AddToSurfaceCache(phoneticAttributes, s);
            return s;
        }

        public enum TemplateTokenType
        {
            I_WOVEL,
            A_WOVEL,
            DEVOICE_FIRST,
            //VOICE_LAST,
            LAST_VOICED,
            LAST_NOT_VOICED,
            APPEND,
            LETTER
        }

        public class SuffixTemplateToken
        {
            internal TemplateTokenType type;
            internal char letter;
            internal bool append = false;

            internal SuffixTemplateToken(TemplateTokenType type, char letter)
            {
                this.type = type;
                this.letter = letter;
            }

            internal SuffixTemplateToken(TemplateTokenType type, char letter, bool append)
            {
                this.type = type;
                this.letter = letter;
                this.append = append;
            }

            public TemplateTokenType GetTokenType()
            {
                return type;
            }

            public char GetLetter()
            {
                return letter;
            }
        }

        // TODO: consider making templates like "+Im" possible. Also change + syntax to ()
        public class SuffixTemplateTokenizer : IEnumerator<SuffixTemplateToken>
        {
            private readonly string generationWord;
            private int pointer;

            public SuffixTemplateTokenizer(string generationWord)
            {
                this.generationWord = generationWord;
            }

            public SuffixTemplateToken Current { get; set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {

            }

            public bool HasNext()
            {
                return generationWord != null && pointer < generationWord.Length;
            }

            public bool MoveNext()
            {
                if (HasNext())
                {
                    Current = Next();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public SuffixTemplateToken Next()
            {
                if (!HasNext())
                {
                    throw new IndexOutOfRangeException("no elements left!");
                }
                char c = generationWord[pointer++];
                char cNext = (char)0;
                if (pointer < generationWord.Length)
                {
                    cNext = generationWord[pointer];
                }

                char undefined = (char)0;
                switch (c)
                {
                    case '+':
                        pointer++;
                        if (cNext == 'I')
                        {
                            return new SuffixTemplateToken(TemplateTokenType.I_WOVEL, undefined, true);
                        }
                        else if (cNext == 'A')
                        {
                            return new SuffixTemplateToken(TemplateTokenType.A_WOVEL, undefined, true);
                        }
                        else
                        {
                            return new SuffixTemplateToken(TemplateTokenType.APPEND, cNext);
                        }
                    case '>':
                        pointer++;
                        return new SuffixTemplateToken(TemplateTokenType.DEVOICE_FIRST, cNext);
                    case '~':
                        pointer++;
                        return new SuffixTemplateToken(TemplateTokenType.LAST_VOICED, cNext);
                    case '!':
                        pointer++;
                        return new SuffixTemplateToken(TemplateTokenType.LAST_NOT_VOICED, cNext);
                    case 'I':
                        return new SuffixTemplateToken(TemplateTokenType.I_WOVEL, undefined);
                    case 'A':
                        return new SuffixTemplateToken(TemplateTokenType.A_WOVEL, undefined);
                    default:
                        return new SuffixTemplateToken(TemplateTokenType.LETTER, c);

                }
            }
            public void Remove()
            {
            }

            public void Reset()
            {
                pointer = 0;
            }
        }
    }
}
