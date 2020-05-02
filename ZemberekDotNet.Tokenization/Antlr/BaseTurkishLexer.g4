lexer grammar BaseTurkishLexer;

@header {
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
/// <summary>
/// A simple lexer grammar for Turkish texts.
/// </summary>
}

@members {

protected static ISet<string> abbreviations = new HashSet<string>();
protected Queue<IToken> queue = new Queue<IToken>();
protected static CultureInfo localeTr = CultureInfo.GetCultureInfo("tr");


public override IToken NextToken()
{
    if (!queue.IsEmpty())
    {
        return queue.Dequeue();
    }

    IToken next = base.NextToken();

    if (next.Type != Word)
    {
        return next;
    }

    IToken next2 = base.NextToken();
    if (next2.Type == Punctuation && next2.Text.Equals("."))
    {
        string abbrev = next.Text + ".";
        if (abbreviations != null && abbreviations.Contains(abbrev))
        {
            CommonToken commonToken = new CommonToken(Abbreviation, abbrev);
            commonToken.StartIndex = (next.StartIndex);
            commonToken.StopIndex = (next2.StopIndex);
            commonToken.TokenIndex = (next.TokenIndex);
            commonToken.Column = (next.Column);
            commonToken.Line = (next.Line);
            return commonToken;
        }
    }
    queue.Enqueue(next2);
    return next;
}
}

tokens {Abbreviation}

options {
  language = CSharp;
}

fragment Digit: [0-9];

// Letters
fragment TurkishLetters
    : [a-zçğıöşüâîû];

fragment TurkishLettersCapital
    : [A-ZÇĞİÖŞÜÂÎÛ];

fragment TurkishLettersAll
    : [a-zA-ZçğıöşüâîûÇĞİÖŞÜÂÎÛ];

fragment AllTurkishAlphanumerical
    : [0-9a-zA-ZçğıöşüâîûÇĞİÖŞÜÂÎÛ];

fragment AllTurkishAlphanumericalUnderscore
    : [0-9a-zA-ZçğıöşüâîûÇĞİÖŞÜÂÎÛ_];

fragment Apostrophe: ('\''|'’');

fragment DoubleQuote: ('"'|'”'|'“'|'»'|'«');

// 'lerin
fragment AposAndSuffix: Apostrophe TurkishLettersAll+;

SpaceTab
    : [ \t]+;
NewLine
    : [\n\r];

Time
    : [0-2][0-9] (':'|'.') [0-5][0-9] ((':'|'.') [0-5][0-9])? AposAndSuffix? ;

Date
    :([0-3]?[0-9] '.' [0-1]?[0-9] '.' ([1][7-9][0-9][0-9]|[2][0][0-9][0-9]|[0-9][0-9]) AposAndSuffix?)|
    ([0-3]?[0-9] '/' [0-1]?[0-9] '/' ([1][7-9][0-9][0-9]|[2][0][0-9][0-9]|[0-9][0-9]) AposAndSuffix?);

PercentNumeral
    : '%' Number;

Number
    : [+\-]? Integer [.,] Integer Exp? AposAndSuffix? // -1.35, 1.35E-9, 3,1'e
    | [+\-]? Integer Exp AposAndSuffix?     // 1e10 -3e4 1e10'dur
    | [+\-]? Integer AposAndSuffix?         // -3, 45
    | [+\-]? Integer '/' Integer AposAndSuffix?  // -1/2
    | (Integer '.')+ Integer AposAndSuffix? // 1.000.000
    | (Integer ',')+ Integer AposAndSuffix? // 2,345,531
    | Integer '.'? AposAndSuffix?           // Ordinal 2. 34.      
    ;

// Not really an integer as it can have zeroes at the start but this is ok.
fragment Integer
    : Digit+ ;

fragment Exp
    : [Ee] [+\-]? Integer ;

fragment URLFragment
    : [0-9a-zA-ZçğıöşüâîûÇĞİÖŞÜÂÎÛ\-_/?&+;=[\].]+;

URL :
    ('http://'|'https://') URLFragment AposAndSuffix? |
    ('http://'|'https://')? 'www.' URLFragment AposAndSuffix?|
    [0-9a-zA-Z_]+ ('.com'| '.org' | '.edu' | '.gov'|'.net'|'.info') ('.tr')? ('/'URLFragment)? AposAndSuffix?;

Email
    :AllTurkishAlphanumericalUnderscore+ '.'? AllTurkishAlphanumericalUnderscore+ '@'
    (AllTurkishAlphanumericalUnderscore+ '.' AllTurkishAlphanumericalUnderscore+)+ AposAndSuffix?;

HashTag: '#' AllTurkishAlphanumericalUnderscore+ AposAndSuffix?;

Mention: '@' AllTurkishAlphanumericalUnderscore+ AposAndSuffix?;

MetaTag: '<' AllTurkishAlphanumericalUnderscore+ '>';

// Only a subset.
// TODO: Add more, also consider Emoji tokens.
Emoticon
    : ':)'|':-)'|':-]'|':D'|':-D'|'8-)'|';)'|';‑)'|':('|':-('|':\'('|':\')'
    |':P'|':p'|':|'|'=|'|'=)'|'=('
    |':‑/'|':/'|':^)'|'¯\\_(ツ)_/¯'|'O_o'|'o_O'|'O_O'|'\\o/'|'<3';

// Possible Roman numbers:
RomanNumeral
    : [ILVCDMX]+ '.'? AposAndSuffix? ;

// I.B.M.
AbbreviationWithDots
    : (TurkishLettersCapital '.')+ TurkishLettersCapital? AposAndSuffix?;

// Merhaba kedi
Word
    : TurkishLettersAll+;

// f16
WordAlphanumerical
    : AllTurkishAlphanumerical+;

WordWithSymbol
    : AllTurkishAlphanumerical+ '-'? AllTurkishAlphanumerical+ AposAndSuffix?;

fragment PunctuationFragment
    : Apostrophe | DoubleQuote | '...' | '(!)' | '(?)'| [>‘…=.,!?%$&*+@:;®™©℠]
          | '\\' | '-' | '/' | '(' | ')' | '[' | ']' | '{' | '}' | '^' ;

Punctuation
    : PunctuationFragment;

UnknownWord
    : ~([ \n\r\t.,!?%$&*+@:;…®™©℠=>] | '\'' | '’' | '‘' | '"' | '”' | '“' | '»' | '«'
    |'\\' | '-' |'(' | '/' | ')' | '[' | ']' | '{' | '}' | '^')+;

// Catch all remaining as Unknown.
Unknown : .+? ;

