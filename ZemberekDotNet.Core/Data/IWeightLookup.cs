using System;

namespace ZemberekDotNet.Core.Data
{
    public interface IWeightLookup
    {
        float Get(String key);

        int Size();
    }
}
