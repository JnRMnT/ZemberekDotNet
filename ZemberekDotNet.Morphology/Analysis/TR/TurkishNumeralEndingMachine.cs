using ZemberekDotNet.Core.Native;

namespace ZemberekDotNet.Morphology.Analysis.TR
{
    /// <summary>
    /// This class is used for finding the last word of a number.
    /// such as 123 returns "üç"(3) whereas it returns "yüz"(100) for 12300
    /// It uses a simple state machine that processes the input backwards.
    /// </summary>
    public class TurkishNumeralEndingMachine
    {
        State ROOT = new State(StateId.ROOT);
        private State[] states1 = {
            new State(StateId.SIFIR),
            new State(StateId.BIR),
            new State(StateId.IKI),
            new State(StateId.UC),
            new State(StateId.DORT),
            new State(StateId.BES),
            new State(StateId.ALTI),
            new State(StateId.YEDI),
            new State(StateId.SEKIZ),
            new State(StateId.DOKUZ)
        };

        private State[] states10 = {
            null,
            new State(StateId.ON),
            new State(StateId.YIRMI),
            new State(StateId.OTUZ),
            new State(StateId.KIRK),
            new State(StateId.ELLI),
            new State(StateId.ALTMIS),
            new State(StateId.YETMIS),
            new State(StateId.SEKSEN),
            new State(StateId.DOKSAN)
        };

        private static readonly State SIFIR = new State(StateId.SIFIR);
        private static readonly State YUZ = new State(StateId.YUZ);
        private static readonly State BIN_1 = new State(StateId.BIN);
        private static readonly State BIN_2 = new State(StateId.BIN);
        private static readonly State BIN_3 = new State(StateId.BIN);
        private static readonly State MILYON_1 = new State(StateId.MILYON);
        private static readonly State MILYON_2 = new State(StateId.MILYON);
        private static readonly State MILYON_3 = new State(StateId.MILYON);
        private static readonly State MILYAR_1 = new State(StateId.MILYAR);
        private static readonly State MILYAR_2 = new State(StateId.MILYAR);
        private static readonly State MILYAR_3 = new State(StateId.MILYAR);
        static readonly State[] zeroStates = { SIFIR, YUZ, BIN_1, BIN_2, BIN_3, MILYON_1, MILYON_2, MILYON_3, MILYAR_1, MILYAR_2, MILYAR_3 };

        public TurkishNumeralEndingMachine()
        {
            Build();
        }

        private void Build()
        {
            SIFIR.ZeroState = false;
            foreach (State largeState in zeroStates)
            {
                largeState.ZeroState = true;
            }
            for (int i = 1; i < states1.Length; i++)
            {
                State oneState = states1[i];
                ROOT.Add(i, oneState);
            }
            for (int i = 1; i < states10.Length; i++)
            {
                State tenState = states10[i];
                SIFIR.Add(i, tenState);
            }
            ROOT.Add(0, SIFIR);
            SIFIR.Add(0, YUZ);
            YUZ.Add(0, BIN_1);
            BIN_1.Add(0, BIN_2);
            BIN_2.Add(0, BIN_3);
            BIN_3.Add(0, MILYON_1);
            MILYON_1.Add(0, MILYON_2);
            MILYON_2.Add(0, MILYON_3);
            MILYON_3.Add(0, MILYAR_1);
            MILYAR_1.Add(0, MILYAR_2);
            MILYAR_2.Add(0, MILYAR_3);
        }

        /// <summary>
        /// Finds the last Turkish number word of an alphanumeric String's pronunciation.
        /// Examples:
        /// 123 -> "üç"(3)
        /// 12300 -> "yüz"(100)
        /// a20 -> "yirmi"(20)
        /// 00 -> "sıfır"(0)
        /// abc -> ""
        /// 1abc -> ""
        /// </summary>
        /// <param name="numStr">input suppose to have digits in it. It may contain alphanumeric values.</param>
        /// <returns>last Turkish word pronunciation of the imput number.</returns>
        public string Find(string numStr)
        {
            State current = ROOT;
            for (int i = numStr.Length - 1; i >= 0; i--)
            {
                int k = numStr[i] - '0';
                if (k < 0 || k > 9)
                {
                    if (current.ZeroState)
                    {
                        return StateId.SIFIR.Lemma;
                    }
                    else
                    {
                        break;
                    }
                }
                if (k > 0 && current.ZeroState)
                {
                    if (current == SIFIR)
                    {
                        return current.Transitions[k].Id.Lemma;
                    }
                    break;
                }
                current = current.Transitions[k];
                if (current == null)
                {
                    return StateId.ERROR.Lemma;
                }
                if (!current.ZeroState)
                { // we are done
                    break;
                }
            }
            return current.Id.Lemma;
        }

        internal class StateId : IClassEnum
        {
            public struct Constants
            {
                public const string ROOT = "ROOT";
                public const string ERROR = "ERROR";
                public const string SIFIR = "SIFIR";
                public const string BIR = "BIR";
                public const string IKI = "IKI";
                public const string UC = "UC";
                public const string DORT = "DORT";
                public const string BES = "BES";
                public const string ALTI = "ALTI";
                public const string YEDI = "YEDI";
                public const string SEKIZ = "SEKIZ";
                public const string DOKUZ = "DOKUZ";
                public const string ON = "ON";
                public const string YIRMI = "YIRMI";
                public const string OTUZ = "OTUZ";
                public const string KIRK = "KIRK";
                public const string ELLI = "ELLI";
                public const string ALTMIS = "ALTMIS";
                public const string YETMIS = "YETMIS";
                public const string SEKSEN = "SEKSEN";
                public const string DOKSAN = "DOKSAN";
                public const string YUZ = "YUZ";
                public const string BIN = "BIN";
                public const string MILYON = "MILYON";
                public const string MILYAR = "MILYAR";
            }
            public static readonly StateId ROOT = new StateId(0, Constants.ROOT, "");
            public static readonly StateId ERROR = new StateId(1, Constants.ERROR, "");
            public static readonly StateId SIFIR = new StateId(2, Constants.SIFIR, "sıfır");
            public static readonly StateId BIR = new StateId(3, Constants.BIR, "bir");
            public static readonly StateId IKI = new StateId(4, Constants.IKI, "iki");
            public static readonly StateId UC = new StateId(5, Constants.UC, "üç");
            public static readonly StateId DORT = new StateId(6, Constants.DORT, "dört");
            public static readonly StateId BES = new StateId(7, Constants.BES, "beş");
            public static readonly StateId ALTI = new StateId(8, Constants.ALTI, "altı");
            public static readonly StateId YEDI = new StateId(9, Constants.YEDI, "yedi");
            public static readonly StateId SEKIZ = new StateId(10, Constants.SEKIZ, "sekiz");
            public static readonly StateId DOKUZ = new StateId(11, Constants.DOKUZ, "dokuz");
            public static readonly StateId ON = new StateId(12, Constants.ON, "on");
            public static readonly StateId YIRMI = new StateId(13, Constants.YIRMI, "yirmi");
            public static readonly StateId OTUZ = new StateId(14, Constants.OTUZ, "otuz");
            public static readonly StateId KIRK = new StateId(15, Constants.KIRK, "kırk");
            public static readonly StateId ELLI = new StateId(16, Constants.ELLI, "elli");
            public static readonly StateId ALTMIS = new StateId(17, Constants.ALTMIS, "altmış");
            public static readonly StateId YETMIS = new StateId(18, Constants.YETMIS, "yetmiş");
            public static readonly StateId SEKSEN = new StateId(19, Constants.SEKSEN, "seksen");
            public static readonly StateId DOKSAN = new StateId(20, Constants.DOKSAN, "doksan");
            public static readonly StateId YUZ = new StateId(21, Constants.YUZ, "yüz");
            public static readonly StateId BIN = new StateId(22, Constants.BIN, "bin");
            public static readonly StateId MILYON = new StateId(23, Constants.MILYON, "milyon");
            public static readonly StateId MILYAR = new StateId(24, Constants.MILYAR, "milyar");

            string lemma;
            public string DefinedName { get; set; }
            public string Lemma { get => lemma; set => lemma = value; }

            private int index;

            StateId(int index, string definedName, string lemma)
            {
                this.index = index;
                this.DefinedName = definedName;
                this.Lemma = lemma;
            }

            public int GetIndex()
            {
                return index;
            }
        }

        internal class State
        {
            StateId id;
            State[] transitions;
            bool zeroState;

            internal State(StateId id)
            {
                this.Id = id;
                Transitions = new State[10];
            }

            public bool ZeroState { get => zeroState; set => zeroState = value; }
            internal State[] Transitions { get => transitions; set => transitions = value; }
            internal StateId Id { get => id; set => id = value; }

            internal void Add(int i, State state)
            {
                Transitions[i] = state;
            }
        }
    }
}
