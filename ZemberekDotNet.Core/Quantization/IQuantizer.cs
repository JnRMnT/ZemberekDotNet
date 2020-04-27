using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Quantization
{
    public interface IQuantizer
    {
        int GetQuantizationIndex(double value);

        double GetQuantizedValue(double value);

        DoubleLookup GetDequantizer();
    }
}
