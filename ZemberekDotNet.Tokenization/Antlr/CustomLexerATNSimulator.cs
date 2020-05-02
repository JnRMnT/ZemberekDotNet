using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using System;

namespace ZemberekDotNet.Tokenization.Antlr
{
    // For speeding up lexer, we had to make this hack.
    // Refer to: https://github.com/antlr/antlr4/issues/1613#issuecomment-273514372
    public class CustomLexerATNSimulator : LexerATNSimulator
    {
        public new static readonly int MAX_DFA_EDGE = 368;
        public CustomLexerATNSimulator(ATN atn, DFA[] decisionToDFA,
            PredictionContextCache sharedContextCache): base(atn, decisionToDFA, sharedContextCache)
        {

        }

        public CustomLexerATNSimulator(Lexer recog, ATN atn, DFA[] decisionToDFA,
            PredictionContextCache sharedContextCache): base(recog, atn, decisionToDFA, sharedContextCache)
        {
         
        }

        protected new DFAState GetExistingTargetState(DFAState s, int t)
        {
            if (s.edges == null || t < MIN_DFA_EDGE || t > MAX_DFA_EDGE)
            {
                return null;
            }

            DFAState target = s.edges[t - MIN_DFA_EDGE];
            if (debug && target != null)
            {
                Console.WriteLine("reuse state " + s.stateNumber +
                    " edge to " + target.stateNumber);
            }

            return target;
        }



        protected new void AddDFAEdge(DFAState p, int t, DFAState q)
        {
            if (t < MIN_DFA_EDGE || t > MAX_DFA_EDGE)
            {
                // Only track edges within the DFA bounds
                return;
            }

            if (debug)
            {
                Console.WriteLine("EDGE " + p + " -> " + q + " upon " + ((char)t));
            }

            lock (p)
            {
                if (p.edges == null)
                {
                    //  make room for tokens 1..n and -1 masquerading as index 0
                    p.edges = new DFAState[MAX_DFA_EDGE - MIN_DFA_EDGE + 1];
                }
                p.edges[t - MIN_DFA_EDGE] = q; // connect
            }
        }
    }
}
