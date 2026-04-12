namespace ZemberekDotNet.Core.Turkish
{
    /// <summary>
    /// Represents Turkish grammatical cases as a type-safe enum.
    /// </summary>
    /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
    public enum TurkishCase
    {
        /// <summary>No case morpheme found or unrecognised.</summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
        Unknown = 0,

        /// <summary>Nominative case — bare noun, e.g. "elma".</summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek. Morpheme ID: Nom</remarks>
        Nominative,

        /// <summary>Dative case — "toward", e.g. "elmaya".</summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek. Morpheme ID: Dat</remarks>
        Dative,

        /// <summary>Accusative case — definite direct object, e.g. "elmayı".</summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek. Morpheme ID: Acc</remarks>
        Accusative,

        /// <summary>Ablative case — "from / out of", e.g. "elmadan".</summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek. Morpheme ID: Abl</remarks>
        Ablative,

        /// <summary>Locative case — "at / in", e.g. "elmada".</summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek. Morpheme ID: Loc</remarks>
        Locative,

        /// <summary>Instrumental case — "with / by means of", e.g. "elmayla".</summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek. Morpheme ID: Ins</remarks>
        Instrumental,

        /// <summary>Genitive case — possessive "of", e.g. "elmanın".</summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek. Morpheme ID: Gen</remarks>
        Genitive,

        /// <summary>Equative case — "like / as much as", e.g. "elmaca".</summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek. Morpheme ID: Equ</remarks>
        Equative
    }
}
