using System;
using System.Collections.Generic;

namespace ZemberekDotNet.Morphology.Analysis.TR
{
    /// <summary>
    /// Converts a number from text form to digit form for turkish.
    /// </summary>
    public class TurkishTextToNumberConverter
    {
        /// <summary>
        /// Converts an array of digit text Strings to a long number.
        /// <p>Example:
        /// <p>[on,iki] returns 12
        /// <p>[bin,on,iki] returns 1012
        /// <p>[seksen,iki,milyon,iki] returns 82000002
        /// </summary>
        /// <param name="words"> digit string array</param>
        /// <returns>number equivalent of the word array, or -1 if word array is not parseable like
        /// [bir,bin] [milyon] [on,bir,iki]</returns>
        public long Convert(params string[] words)
        {
            return Convert(new List<string>(words));
        }

        public long Convert(ICollection<string> words)
        {
            Context context = new Context();
            State state = State.START;
            foreach (string word in words)
            {
                state = context.AcceptTransition(state, new Transition(word));
                if (state == State.ERROR)
                {
                    return -1;
                }
            }
            return context.valueToAdd + context.total;
        }

        private class Context
        {
            internal long total;
            internal long valueToAdd;
            private Transition previousMil = new Transition("sıfır");

            internal State AcceptTransition(State currentState, Transition transition)
            {

                switch (transition.type.Name)
                {

                    case TransitionType.Constants.T_ZERO:
                        if (currentState == State.START)
                        {
                            return total == 0 ? State.END : State.ERROR;
                        }
                        break;

                    case TransitionType.Constants.T_ONE:
                        switch (currentState)
                        {
                            case State.START:
                                Add(1);
                                return State.ST_ONES_1;
                            case State.ST_TENS_1:
                                Add(1);
                                return State.ST_ONES_3;
                            case State.ST_TENS_2:
                            case State.ST_HUNDREDS_1:
                                Add(1);
                                return State.ST_ONES_4;
                            case State.ST_TENS_3:
                            case State.ST_HUNDREDS_2:
                            case State.ST_THOUSAND:
                                Add(1);
                                return State.ST_ONES_5;
                        }
                        break;

                    case TransitionType.Constants.T_TWO_TO_NINE:
                        switch (currentState)
                        {
                            case State.START:
                                Add(transition.value);
                                return State.ST_ONES_2;
                            case State.ST_TENS_1:
                                Add(transition.value);
                                return State.ST_ONES_3;
                            case State.ST_TENS_2:
                            case State.ST_HUNDREDS_1:
                                Add(transition.value);
                                return State.ST_ONES_4;
                            case State.ST_TENS_3:
                            case State.ST_HUNDREDS_2:
                                Add(transition.value);
                                return State.ST_ONES_5;
                            case State.ST_THOUSAND:
                                Add(transition.value);
                                return State.ST_ONES_6;
                        }
                        break;

                    case TransitionType.Constants.T_TENS:
                        switch (currentState)
                        {
                            case State.START:
                                Add(transition.value);
                                return State.ST_TENS_1;
                            case State.ST_HUNDREDS_1:
                                Add(transition.value);
                                return State.ST_TENS_2;
                            case State.ST_HUNDREDS_2:
                            case State.ST_THOUSAND:
                                Add(transition.value);
                                return State.ST_TENS_3;
                        }
                        break;

                    case TransitionType.Constants.T_HUNDRED:
                        switch (currentState)
                        {
                            case State.START:
                            case State.ST_ONES_2:
                                Mul100();
                                return State.ST_HUNDREDS_1;
                            case State.ST_THOUSAND:
                            case State.ST_ONES_6:
                                Mul100();
                                return State.ST_HUNDREDS_2;
                        }
                        break;

                    case TransitionType.Constants.T_THOUSAND:
                        switch (currentState)
                        {
                            case State.START:
                            case State.ST_ONES_2:
                            case State.ST_ONES_3:
                            case State.ST_ONES_4:
                            case State.ST_TENS_1:
                            case State.ST_TENS_2:
                            case State.ST_HUNDREDS_1:
                                AddToTotal(transition.value);
                                return State.ST_THOUSAND;
                        }
                        break;

                    case TransitionType.Constants.T_MILLION:
                    case TransitionType.Constants.T_BILLION:
                    case TransitionType.Constants.T_TRILLION:
                    case TransitionType.Constants.T_QUADRILLION:
                        switch (currentState)
                        {
                            case State.ST_ONES_1:
                            case State.ST_ONES_2:
                            case State.ST_ONES_3:
                            case State.ST_ONES_4:
                            case State.ST_TENS_1:
                            case State.ST_TENS_2:
                            case State.ST_HUNDREDS_1:
                                // millions, billions etc behaves the same.
                                // here we prevent "billion" comes after a "million"
                                // for this, we remember the last big number in previousMil variable..
                                if (previousMil.value == 0 || previousMil.value > transition.value)
                                {
                                    previousMil = transition;
                                    AddToTotal(transition.value);
                                    return State.START;
                                }
                                else
                                {
                                    return State.ERROR;
                                }
                        }
                        break;

                }
                return State.ERROR;
            }

            private void Add(long val)
            {
                valueToAdd += val;
            }

            private void Mul100()
            {
                if (valueToAdd == 0)
                {
                    valueToAdd = 100;
                }
                else
                {
                    valueToAdd = valueToAdd * 100;
                }
            }

            private void AddToTotal(long val)
            {
                if (valueToAdd == 0)
                {
                    total += val;
                }
                else
                {
                    total += valueToAdd * val;
                }
                valueToAdd = 0;
            }
        }


        private static int DigitCount(long num)
        {
            int i = 0;
            do
            {
                num = num / 10;
                i++;
            } while (num > 0);
            return i;
        }

        private enum State
        {
            START,
            ST_ONES_1, ST_ONES_2, ST_ONES_3, ST_ONES_4, ST_ONES_5, ST_ONES_6,
            ST_TENS_1, ST_TENS_2, ST_TENS_3,
            ST_HUNDREDS_1, ST_HUNDREDS_2,
            ST_THOUSAND,
            END,
            ERROR
        }

        private class TransitionType
        {
            internal struct Constants
            {
                public const string T_ZERO = "T_ZERO";
                public const string T_ONE = "T_ONE";
                public const string T_TWO_TO_NINE = "T_TWO_TO_NINE";
                public const string T_TENS = "T_TENS";
                public const string T_HUNDRED = "T_HUNDRED";
                public const string T_THOUSAND = "T_THOUSAND";
                public const string T_MILLION = "T_MILLION";
                public const string T_BILLION = "T_BILLION";
                public const string T_TRILLION = "T_TRILLION";
                public const string T_QUADRILLION = "T_QUADRILLION";
            }
            public static readonly TransitionType T_ZERO = new TransitionType(Constants.T_ZERO);
            public static readonly TransitionType T_ONE = new TransitionType(Constants.T_ONE);
            public static readonly TransitionType T_TWO_TO_NINE = new TransitionType(Constants.T_TWO_TO_NINE);
            public static readonly TransitionType T_TENS = new TransitionType(Constants.T_TENS);
            public static readonly TransitionType T_HUNDRED = new TransitionType(Constants.T_HUNDRED);
            public static readonly TransitionType T_THOUSAND = new TransitionType(Constants.T_THOUSAND);
            public static readonly TransitionType T_MILLION = new TransitionType(Constants.T_MILLION);
            public static readonly TransitionType T_BILLION = new TransitionType(Constants.T_BILLION);
            public static readonly TransitionType T_TRILLION = new TransitionType(Constants.T_TRILLION);
            public static readonly TransitionType T_QUADRILLION = new TransitionType(Constants.T_QUADRILLION);

            private readonly string name;
            public string Name
            {
                get { return name; }
            }
            public TransitionType(string name)
            {
                this.name = name;
            }
            public static TransitionType GetTypeByValue(long number)
            {
                switch (DigitCount(number))
                {
                    case 1:
                        if (number == 1)
                        {
                            return T_ONE;
                        }
                        else if (number == 0)
                        {
                            return T_ZERO;
                        }
                        else
                        {
                            return T_TWO_TO_NINE;
                        }
                    case 2:
                        return T_TENS;
                    case 3:
                        return T_HUNDRED;
                    case 4:
                        return T_THOUSAND;
                    case 7:
                        return T_MILLION;
                    case 10:
                        return T_BILLION;
                    case 13:
                        return T_TRILLION;
                    case 16:
                        return T_QUADRILLION;
                    default:
                        throw new ArgumentException("cannot create a Transition from value: " + number);
                }
            }
        }

        private class Transition
        {
            internal readonly long value;
            internal readonly TransitionType type;

            internal Transition(string str)
            {
                this.value = TurkishNumbers.SingleWordNumberValue(str);
                this.type = TransitionType.GetTypeByValue(this.value);
            }

            public override string ToString()
            {
                return "[" + type.Name + ":" + value + "]";
            }
        }
    }
}
