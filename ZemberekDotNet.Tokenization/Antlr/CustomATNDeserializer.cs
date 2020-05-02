using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZemberekDotNet.Tokenization.Antlr
{
    public class CustomATNDeserializer : ATNDeserializer
    {
        private int ToInt(char character)
        {
            return (int)character;
        }

        public override ATN Deserialize(char[] data)
        {
            //return base.Deserialize(data);
            data = (char[])data.Clone();
            this.Reset(data);
            this.CheckVersion();
            this.CheckUUID();

            bool supportsPrecedencePredicates = true;// IsFeatureSupported(AddedPrecedenceTransitions, this.uuid);
            bool supportsLexerActions = true;//isFeatureSupported(ADDED_LEXER_ACTIONS, uuid);
            int p = 9;
            ATNType grammarType = (ATNType)ToInt(data[p++]);
            int maxTokenType = ToInt(data[p++]);
            ATN atn = new ATN(grammarType, maxTokenType);

            //
            // STATES
            //
            List<KeyValuePair<LoopEndState, int>> loopBackStateNumbers = new List<KeyValuePair<LoopEndState, int>>();
            List<KeyValuePair<BlockStartState, int>> endStateNumbers = new List<KeyValuePair<BlockStartState, int>>();
            int nstates = ToInt(data[p++]);

            for (int i = 0; i < nstates; i++)
            {
                int stype = ToInt(data[p++]);
                // ignore bad type of states
                if (stype == ATNState.serializationNames.IndexOf("INVALID"))
                {
                    atn.AddState(null);
                    continue;
                }

                int ruleIndex = ToInt(data[p++]);
                if (ruleIndex == char.MaxValue)
                {
                    ruleIndex = -1;
                }

                ATNState s = StateFactory((StateType)stype, ruleIndex);
                if (stype == ATNState.serializationNames.IndexOf("LOOP_END"))
                { // special case
                    int loopBackStateNumber = ToInt(data[p++]);
                    loopBackStateNumbers.Add(new KeyValuePair<LoopEndState, int>((LoopEndState)s, loopBackStateNumber));
                }
                else if (s is BlockStartState)
                {
                    int endStateNumber = ToInt(data[p++]);
                    endStateNumbers.Add(new KeyValuePair<BlockStartState, int>((BlockStartState)s, endStateNumber));
                }
                atn.AddState(s);
            }

            // delay the assignment of loop back and end states until we know all the state instances have been initialized
            foreach (KeyValuePair<LoopEndState, int> pair in loopBackStateNumbers)
            {
                pair.Key.loopBackState = atn.states[pair.Value];
            }

            foreach (KeyValuePair<BlockStartState, int> pair in endStateNumbers)
            {
                pair.Key.endState = (BlockEndState)atn.states[pair.Value];
            }

            int numNonGreedyStates = ToInt(data[p++]);
            for (int i = 0; i < numNonGreedyStates; i++)
            {
                int stateNumber = ToInt(data[p++]);
                if (atn.states[stateNumber] is DecisionState)
                {
                    ((DecisionState)atn.states[stateNumber]).nonGreedy = true;
                }
            }

            if (supportsPrecedencePredicates)
            {
                int numPrecedenceStates = ToInt(data[p++]);
                for (int i = 0; i < numPrecedenceStates; i++)
                {
                    int stateNumber = ToInt(data[p++]);
                    //((RuleStartState)atn.states[stateNumber]).isLeftRecursiveRule = true;
                }
            }

            //
            // RULES
            //
            int nrules = ToInt(data[p++]);
            if (atn.grammarType == ATNType.Lexer)
            {
                atn.ruleToTokenType = new int[nrules];
            }

            atn.ruleToStartState = new RuleStartState[nrules];
            for (int i = 0; i < nrules; i++)
            {
                int s = ToInt(data[p++]);
                RuleStartState startState = (RuleStartState)atn.states[s];
                atn.ruleToStartState[i] = startState;
                if (atn.grammarType == ATNType.Lexer)
                {
                    int tokenType = ToInt(data[p++]);
                    if (tokenType == 0xFFFF)
                    {
                        tokenType = TokenConstants.EOF;
                    }

                    atn.ruleToTokenType[i] = tokenType;

                    if (!supportsLexerActions)
                    {
                        // this piece of unused metadata was serialized prior to the
                        // addition of LexerAction
                        int actionIndexIgnored = ToInt(data[p++]);
                    }
                }
            }

            atn.ruleToStopState = new RuleStopState[nrules];
            foreach (ATNState state in atn.states)
            {
                if (!(state is RuleStopState))
                {
                    continue;
                }

                RuleStopState stopState = (RuleStopState)state;
                atn.ruleToStopState[state.ruleIndex] = stopState;
                atn.ruleToStartState[state.ruleIndex].stopState = stopState;
            }

            //
            // MODES
            //
            int nmodes = ToInt(data[p++]);
            for (int i = 0; i < nmodes; i++)
            {
                int s = ToInt(data[p++]);
                atn.modeToStartState.Add((TokensStartState)atn.states[s]);
            }

            //
            // SETS
            //
            //List<IntervalSet> sets = new ArrayList<IntervalSet>();
            IList<IntervalSet> sets = (IList<IntervalSet>)new List<IntervalSet>();
            this.ReadSets(atn, sets, new Func<int>(this.ReadInt));
            this.ReadSets(atn, sets, new Func<int>(this.ReadInt32));
            this.ReadEdges(atn, sets);
            this.ReadDecisions(atn);
            this.ReadLexerActions(atn);
            this.MarkPrecedenceDecisions(atn);
            //this.IdentifyTailCalls(atn);
            return atn;
            //// First, read all sets with 16-bit Unicode code points <= U+FFFF.
            //p = deserializeSets(data, p, sets, getUnicodeDeserializer(UnicodeDeserializingMode.UNICODE_BMP));

            //// Next, if the ATN was serialized with the Unicode SMP feature,
            //// deserialize sets with 32-bit arguments <= U+10FFFF.
            //if (isFeatureSupported(ADDED_UNICODE_SMP, uuid))
            //{
            //    p = deserializeSets(data, p, sets, getUnicodeDeserializer(UnicodeDeserializingMode.UNICODE_SMP));
            //}

            ////
            //// EDGES
            ////
            //int nedges = toInt(data[p++]);
            //for (int i = 0; i < nedges; i++)
            //{
            //    int src = toInt(data[p]);
            //    int trg = toInt(data[p + 1]);
            //    int ttype = toInt(data[p + 2]);
            //    int arg1 = toInt(data[p + 3]);
            //    int arg2 = toInt(data[p + 4]);
            //    int arg3 = toInt(data[p + 5]);
            //    Transition trans = edgeFactory(atn, ttype, src, trg, arg1, arg2, arg3, sets);
            //    //			System.out.println("EDGE "+trans.getClass().getSimpleName()+" "+
            //    //							   src+"->"+trg+
            //    //					   " "+Transition.serializationNames[ttype]+
            //    //					   " "+arg1+","+arg2+","+arg3);
            //    ATNState srcState = atn.states.get(src);
            //    srcState.addTransition(trans);
            //    p += 6;
            //}

            //// edges for rule stop states can be derived, so they aren't serialized
            //for (ATNState state : atn.states)
            //{
            //    for (int i = 0; i < state.getNumberOfTransitions(); i++)
            //    {
            //        Transition t = state.transition(i);
            //        if (!(t is RuleTransition))
            //        {
            //            continue;
            //        }

            //        RuleTransition ruleTransition = (RuleTransition)t;
            //        int outermostPrecedenceReturn = -1;
            //        if (atn.ruleToStartState[ruleTransition.target.ruleIndex].isLeftRecursiveRule)
            //        {
            //            if (ruleTransition.precedence == 0)
            //            {
            //                outermostPrecedenceReturn = ruleTransition.target.ruleIndex;
            //            }
            //        }

            //        EpsilonTransition returnTransition = new EpsilonTransition(ruleTransition.followState, outermostPrecedenceReturn);
            //        atn.ruleToStopState[ruleTransition.target.ruleIndex].addTransition(returnTransition);
            //    }
            //}

            //for (ATNState state : atn.states)
            //{
            //    if (state is BlockStartState)
            //    {
            //        // we need to know the end state to set its start state
            //        if (((BlockStartState)state).endState == null)
            //        {
            //            throw new IllegalStateException();
            //        }

            //        // block end states can only be associated to a single block start state
            //        if (((BlockStartState)state).endState.startState != null)
            //        {
            //            throw new IllegalStateException();
            //        }

            //        ((BlockStartState)state).endState.startState = (BlockStartState)state;
            //    }

            //    if (state is PlusLoopbackState)
            //    {
            //        PlusLoopbackState loopbackState = (PlusLoopbackState)state;
            //        for (int i = 0; i < loopbackState.getNumberOfTransitions(); i++)
            //        {
            //            ATNState target = loopbackState.transition(i).target;
            //            if (target is PlusBlockStartState)
            //            {
            //                ((PlusBlockStartState)target).loopBackState = loopbackState;
            //            }
            //        }
            //    }
            //    else if (state is StarLoopbackState)
            //    {
            //        StarLoopbackState loopbackState = (StarLoopbackState)state;
            //        for (int i = 0; i < loopbackState.getNumberOfTransitions(); i++)
            //        {
            //            ATNState target = loopbackState.transition(i).target;
            //            if (target is StarLoopEntryState)
            //            {
            //                ((StarLoopEntryState)target).loopBackState = loopbackState;
            //            }
            //        }
            //    }
            //}

            ////
            //// DECISIONS
            ////
            //int ndecisions = toInt(data[p++]);
            //for (int i = 1; i <= ndecisions; i++)
            //{
            //    int s = toInt(data[p++]);
            //    DecisionState decState = (DecisionState)atn.states.get(s);
            //    atn.decisionToState.add(decState);
            //    decState.decision = i - 1;
            //}

            ////
            //// LEXER ACTIONS
            ////
            //if (atn.grammarType == ATNType.LEXER)
            //{
            //    if (supportsLexerActions)
            //    {
            //        atn.lexerActions = new LexerAction[toInt(data[p++])];
            //        for (int i = 0; i < atn.lexerActions.length; i++)
            //        {
            //            LexerActionType actionType = LexerActionType.values()[toInt(data[p++])];
            //            int data1 = toInt(data[p++]);
            //            if (data1 == 0xFFFF)
            //            {
            //                data1 = -1;
            //            }

            //            int data2 = toInt(data[p++]);
            //            if (data2 == 0xFFFF)
            //            {
            //                data2 = -1;
            //            }

            //            LexerAction lexerAction = lexerActionFactory(actionType, data1, data2);

            //            atn.lexerActions[i] = lexerAction;
            //        }
            //    }
            //    else
            //    {
            //        // for compatibility with older serialized ATNs, convert the old
            //        // serialized action index for action transitions to the new
            //        // form, which is the index of a LexerCustomAction
            //        List<LexerAction> legacyLexerActions = new ArrayList<LexerAction>();
            //        for (ATNState state : atn.states)
            //        {
            //            for (int i = 0; i < state.getNumberOfTransitions(); i++)
            //            {
            //                Transition transition = state.transition(i);
            //                if (!(transition is ActionTransition))
            //                {
            //                    continue;
            //                }

            //                int ruleIndex = ((ActionTransition)transition).ruleIndex;
            //                int actionIndex = ((ActionTransition)transition).actionIndex;
            //                LexerCustomAction lexerAction = new LexerCustomAction(ruleIndex, actionIndex);
            //                state.setTransition(i, new ActionTransition(transition.target, ruleIndex, legacyLexerActions.size(), false));
            //                legacyLexerActions.add(lexerAction);
            //            }
            //        }

            //        atn.lexerActions = legacyLexerActions.toArray(new LexerAction[legacyLexerActions.size()]);
            //    }
            //}

            //markPrecedenceDecisions(atn);

            //if (deserializationOptions.isVerifyATN())
            //{
            //    verifyATN(atn);
            //}

            //if (deserializationOptions.isGenerateRuleBypassTransitions() && atn.grammarType == ATNType.PARSER)
            //{
            //    atn.ruleToTokenType = new int[atn.ruleToStartState.length];
            //    for (int i = 0; i < atn.ruleToStartState.length; i++)
            //    {
            //        atn.ruleToTokenType[i] = atn.maxTokenType + i + 1;
            //    }

            //    for (int i = 0; i < atn.ruleToStartState.length; i++)
            //    {
            //        BasicBlockStartState bypassStart = new BasicBlockStartState();
            //        bypassStart.ruleIndex = i;
            //        atn.addState(bypassStart);

            //        BlockEndState bypassStop = new BlockEndState();
            //        bypassStop.ruleIndex = i;
            //        atn.addState(bypassStop);

            //        bypassStart.endState = bypassStop;
            //        atn.defineDecisionState(bypassStart);

            //        bypassStop.startState = bypassStart;

            //        ATNState endState;
            //        Transition excludeTransition = null;
            //        if (atn.ruleToStartState[i].isLeftRecursiveRule)
            //        {
            //            // wrap from the beginning of the rule to the StarLoopEntryState
            //            endState = null;
            //            for (ATNState state : atn.states)
            //            {
            //                if (state.ruleIndex != i)
            //                {
            //                    continue;
            //                }

            //                if (!(state is StarLoopEntryState))
            //                {
            //                    continue;
            //                }

            //                ATNState maybeLoopEndState = state.transition(state.getNumberOfTransitions() - 1).target;
            //                if (!(maybeLoopEndState is LoopEndState))
            //                {
            //                    continue;
            //                }

            //                if (maybeLoopEndState.epsilonOnlyTransitions && maybeLoopEndState.transition(0).target is RuleStopState)
            //                {
            //                    endState = state;
            //                    break;
            //                }
            //            }

            //            if (endState == null)
            //            {
            //                throw new UnsupportedOperationException("Couldn't identify final state of the precedence rule prefix section.");
            //            }

            //            excludeTransition = ((StarLoopEntryState)endState).loopBackState.transition(0);
            //        }
            //        else
            //        {
            //            endState = atn.ruleToStopState[i];
            //        }

            //        // all non-excluded transitions that currently target end state need to target blockEnd instead
            //        for (ATNState state : atn.states)
            //        {
            //            for (Transition transition : state.transitions)
            //            {
            //                if (transition == excludeTransition)
            //                {
            //                    continue;
            //                }

            //                if (transition.target == endState)
            //                {
            //                    transition.target = bypassStop;
            //                }
            //            }
            //        }

            //        // all transitions leaving the rule start state need to leave blockStart instead
            //        while (atn.ruleToStartState[i].getNumberOfTransitions() > 0)
            //        {
            //            Transition transition = atn.ruleToStartState[i].removeTransition(atn.ruleToStartState[i].getNumberOfTransitions() - 1);
            //            bypassStart.addTransition(transition);
            //        }

            //        // link the new states
            //        atn.ruleToStartState[i].addTransition(new EpsilonTransition(bypassStart));
            //        bypassStop.addTransition(new EpsilonTransition(endState));

            //        ATNState matchState = new BasicState();
            //        atn.addState(matchState);
            //        matchState.addTransition(new AtomTransition(bypassStop, atn.ruleToTokenType[i]));
            //        bypassStart.addTransition(new EpsilonTransition(matchState));
            //    }

            //    if (deserializationOptions.isVerifyATN())
            //    {
            //        // reverify after modification
            //        verifyATN(atn);
            //    }
            //}

            //return atn;
        }

    }
}
