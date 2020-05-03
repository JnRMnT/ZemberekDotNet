using System;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    public class CombinedCondition : AbstractCondition
    {
        Operator op;
        List<ICondition> conditions;

        public static ICondition Of(Operator op, ICondition left, ICondition right)
        {
            return new CombinedCondition(op, left, right);
        }

        private CombinedCondition(Operator op, ICondition left, ICondition right) : this(op, 2)
        {
            Add(op, left);
            Add(op, right);
        }

        private CombinedCondition(Operator op, int size)
        {
            this.op = op;
            this.conditions = new List<ICondition>(size);
        }

        private CombinedCondition Add(Operator op, ICondition condition)
        {
            if (condition is CombinedCondition)
            {
                CombinedCondition combinedCondition = (CombinedCondition)condition;

                if (combinedCondition.op == op)
                {
                    this.conditions.AddRange(combinedCondition.conditions);
                }
                else
                {
                    this.conditions.Add(condition);
                }
            }
            else if (condition == null)
            {
                throw new ArgumentException("The argument 'conditions' must not contain null");
            }
            else
            {
                this.conditions.Add(condition);
            }

            return this;
        }

        public static ICondition Of<T>(Operator op, ICollection<T> conditions) where T : ICondition
        {
            if (conditions == null || conditions.IsEmpty())
            {
                throw new ArgumentException("conditions must not be null or empty.");
            }

            CombinedCondition result = null;
            ICondition first = null;

            foreach (ICondition condition in conditions)
            {
                if (first == null)
                {
                    first = condition;
                }
                else if (result == null)
                {
                    (result = new CombinedCondition(op, conditions.Count))
                        .Add(op, first)
                        .Add(op, condition);
                }
                else
                {
                    result.Add(op, condition);
                }
            }

            if (result != null)
            {
                return result;
            }
            else
            {
                return first;
            }
        }

        public override bool Accept(SearchPath path)
        {
            if (conditions.Count == 0)
            {
                return true;
            }
            if (conditions.Count == 1)
            {
                return conditions[0].Accept(path);
            }
            if (op == Operator.AND)
            {
                foreach (ICondition condition in conditions)
                {
                    if (!condition.Accept(path))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                foreach (ICondition condition in conditions)
                {
                    if (condition.Accept(path))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public ICondition GetFailingCondition(SearchPath path)
        {
            if (conditions.Count == 0)
            {
                return null;
            }
            if (conditions.Count == 1)
            {
                ICondition condition = conditions[0];
                return condition.Accept(path) ? null : condition;
            }
            if (op == Operator.AND)
            {
                foreach (ICondition condition in conditions)
                {
                    if (!condition.Accept(path))
                    {
                        return condition;
                    }
                }
                return null;
            }
            else
            {
                bool pass = false;
                foreach (ICondition condition in conditions)
                {
                    if (condition.Accept(path))
                    {
                        pass = true;
                    }
                }
                // for OR, we do not have specific failing condition.
                // So we return this as failing condition.
                return pass ? null : this;
            }
        }

        public override string ToString()
        {
            if (conditions.Count == 0)
            {
                return "[No-Condition]";
            }
            if (conditions.Count == 1)
            {
                return conditions[0].ToString();
            }
            if (op == Operator.AND)
            {
                StringBuilder sb = new StringBuilder();
                int i = 0;
                foreach (ICondition condition in conditions)
                {
                    sb.Append(condition.ToString());
                    if (i++ < conditions.Count - 1)
                    {
                        sb.Append(" AND ");
                    }
                }
                return sb.ToString();
            }
            else
            {
                int i = 0;
                StringBuilder sb = new StringBuilder();
                foreach (ICondition condition in conditions)
                {
                    sb.Append(condition.ToString());
                    if (i++ < conditions.Count - 1)
                    {
                        sb.Append(" OR ");
                    }
                }
                return sb.ToString();
            }
        }

        // counts the number of conditions.
        public int Count()
        {
            if (conditions.Count == 0)
            {
                return 0;
            }
            if (conditions.Count == 1)
            {
                ICondition first = conditions[0];
                if (first is CombinedCondition)
                {
                    return ((CombinedCondition)first).Count();
                }
                else
                {
                    return 1;
                }
            }
            int cnt = 0;
            foreach (ICondition condition in conditions)
            {
                if (condition is CombinedCondition)
                {
                    cnt += ((CombinedCondition)condition).Count();
                }
                else
                {
                    cnt++;
                }
            }
            return cnt;
        }
    }
}
