using Antlr4.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Tokenization.Antlr;

namespace ZemberekDotNet.Tokenization
{
    /// <summary>
    /// A wrapper for Antlr generated lexer.
    /// </summary>
    public class TurkishTokenizer
    {
        public static readonly TurkishTokenizer All = Builder().AcceptAll().Build();
        private static readonly int MaxTokenType = ((Vocabulary)TurkishLexer.DefaultVocabulary).getMaxTokenType();
        public static readonly TurkishTokenizer Default = Builder()
      .AcceptAll()
      .IgnoreTypes(Token.Type.NewLine, Token.Type.SpaceTab)
      .Build();

        private static readonly ConsoleErrorListener<int> IgnoringErrorListener = new ConsoleErrorListener<int>();

        private long acceptedTypeBits;

        private TurkishTokenizer(long acceptedTypeBits)
        {
            this.acceptedTypeBits = acceptedTypeBits;
        }

        public static TurkishTokenizerBuilder Builder()
        {
            return new TurkishTokenizerBuilder();
        }

        private static TurkishLexer LexerInstance(ICharStream inputStream)
        {
            TurkishLexer lexer = new TurkishLexer(inputStream);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(IgnoringErrorListener);
            return lexer;
        }

        public bool IsTypeAccepted(Token.Type i)
        {
            return !TypeAccepted(i);
        }

        public bool IsTypeIgnored(Token.Type i)
        {
            return !TypeAccepted(i);
        }

        private bool TypeAccepted(Token.Type i)
        {
            return (acceptedTypeBits & (1L << (int)i)) != 0;
        }

        private bool TypeIgnored(Token.Type i)
        {
            return (acceptedTypeBits & (1L << (int)i)) == 0;
        }


        public List<Token> Tokenize(FileStream file)
        {
            return GetAllTokens(LexerInstance(CharStreams.fromStream(file)));
        }

        public List<Token> Tokenize(string input)
        {
            return GetAllTokens(LexerInstance(CharStreams.fromstring(input)));
        }

        public List<string> TokenizeToStrings(string input)
        {
            List<Token> tokens = Tokenize(input);
            List<String> tokenStrings = new List<string>(tokens.Count);
            foreach (Token token in tokens)
            {
                tokenStrings.Add(token.GetText());
            }
            return tokenStrings;
        }

        public IEnumerator<Token> GetTokenIterator(string input)
        {
            return new TokenIterator(this, LexerInstance(CharStreams.fromstring(input)));
        }

        public IEnumerator<Token> GetTokenIterator(FileStream file)
        {
            return new TokenIterator(this, LexerInstance(CharStreams.fromStream(file)));
        }

        private List<Token> GetAllTokens(Lexer lexer)
        {
            List<Token> tokens = new List<Token>();
            for (IToken token = lexer.NextToken();
                token.Type != Antlr4.Runtime.TokenConstants.EOF;
                token = lexer.NextToken())
            {
                Token.Type type = ConvertType(token);
                if (TypeIgnored(type))
                {
                    continue;
                }
                tokens.Add(Convert(token));
            }
            return tokens;
        }

        public static Token Convert(IToken token)
        {
            return new Token(token.Text, ConvertType(token), token.StartIndex, token.StopIndex);
        }

        public static Token Convert(IToken token, Token.Type type)
        {
            return new Token(token.Text, type, token.StartIndex, token.StopIndex);
        }

        public static Token.Type ConvertType(IToken token)
        {
            switch (token.Type)
            {
                case TurkishLexer.SpaceTab:
                    return Token.Type.SpaceTab;
                case TurkishLexer.Word:
                    return Token.Type.Word;
                case TurkishLexer.Number:
                    return Token.Type.Number;
                case TurkishLexer.Abbreviation:
                    return Token.Type.Abbreviation;
                case TurkishLexer.AbbreviationWithDots:
                    return Token.Type.AbbreviationWithDots;
                case TurkishLexer.Date:
                    return Token.Type.Date;
                case TurkishLexer.Email:
                    return Token.Type.Email;
                case TurkishLexer.Emoticon:
                    return Token.Type.Emoticon;
                case TurkishLexer.HashTag:
                    return Token.Type.HashTag;
                case TurkishLexer.Mention:
                    return Token.Type.Mention;
                case TurkishLexer.MetaTag:
                    return Token.Type.MetaTag;
                case TurkishLexer.NewLine:
                    return Token.Type.NewLine;
                case TurkishLexer.RomanNumeral:
                    return Token.Type.RomanNumeral;
                case TurkishLexer.PercentNumeral:
                    return Token.Type.PercentNumeral;
                case TurkishLexer.Time:
                    return Token.Type.Time;
                case TurkishLexer.Unknown:
                    return Token.Type.Unknown;
                case TurkishLexer.UnknownWord:
                    return Token.Type.UnknownWord;
                case TurkishLexer.URL:
                    return Token.Type.URL;
                case TurkishLexer.Punctuation:
                    return Token.Type.Punctuation;
                case TurkishLexer.WordAlphanumerical:
                    return Token.Type.WordAlphanumerical;
                case TurkishLexer.WordWithSymbol:
                    return Token.Type.WordWithSymbol;
                default:
                    throw new InvalidOperationException("Unidentified token type =" +
                        TurkishLexer.DefaultVocabulary.GetDisplayName(token.Type));
            }
        }

        public class TurkishTokenizerBuilder
        {
            private long acceptedTypeBits = ~0L;

            public TurkishTokenizerBuilder AcceptTypes(params Token.Type[] types)
            {
                foreach (Token.Type i in types)
                {
                    this.acceptedTypeBits |= (1L << (int)i);
                }
                return this;
            }

            public TurkishTokenizerBuilder IgnoreTypes(params Token.Type[] types)
            {
                foreach (Token.Type i in types)
                {
                    this.acceptedTypeBits &= ~(1L << (int)i);
                }
                return this;
            }

            public TurkishTokenizerBuilder IgnoreAll()
            {
                this.acceptedTypeBits = 0L;
                return this;
            }

            public TurkishTokenizerBuilder AcceptAll()
            {
                this.acceptedTypeBits = ~0L;
                return this;
            }

            public TurkishTokenizer Build()
            {
                return new TurkishTokenizer(acceptedTypeBits);
            }
        }

        private class TokenIterator : IEnumerator<Token>
        {
            internal TurkishLexer lexer;
            internal TurkishTokenizer tokenizer;
            internal Token.Type type;
            internal IToken token;

            internal TokenIterator(TurkishTokenizer tokenizer, TurkishLexer lexer)
            {
                this.tokenizer = tokenizer;
                this.lexer = lexer;
            }

            public Token Current { get; set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public bool HasNext()
            {
                IToken token = lexer.NextToken();
                if (token.Type == TokenConstants.EOF)
                {
                    return false;
                }
                type = ConvertType(token);
                while (tokenizer.TypeIgnored(type))
                {
                    token = lexer.NextToken();
                    if (token.Type == TokenConstants.EOF)
                    {
                        return false;
                    }
                    type = ConvertType(token);
                }
                this.token = token;
                return true;
            }

            public bool MoveNext()
            {
                return HasNext();
            }

            public Token Next()
            {
                return Convert(token, type);
            }

            public void Reset()
            {
                lexer.Reset();
                Current = null;
            }
        }
    }
}
