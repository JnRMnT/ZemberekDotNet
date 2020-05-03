using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    public interface ICondition
    {
        bool Accept(SearchPath path);

        ICondition And(ICondition other);

        ICondition AndNot(ICondition other);

        ICondition Or(ICondition other);

        ICondition OrNot(ICondition other);

        ICondition Not();

    }
}
