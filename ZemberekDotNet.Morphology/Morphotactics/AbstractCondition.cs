using System;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    public abstract class AbstractCondition : ICondition
    {
        public abstract bool Accept(SearchPath path);

        public ICondition And(ICondition other)
        {
            return Conditions.And(this, other);
        }


        public ICondition AndNot(ICondition other)
        {
            return And(other.Not());
        }


        public ICondition Or(ICondition other)
        {
            return Conditions.Or(this, other);
        }


        public ICondition OrNot(ICondition other)
        {
            return Or(other.Not());
        }

        public ICondition Not()
        {
            return new Conditions.NotCondition(this);
        }
    }
}
