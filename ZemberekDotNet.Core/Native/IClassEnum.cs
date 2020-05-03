using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Native
{
    /// <summary>
    /// Interface to differentiate a class type that behaves like an enum
    /// </summary>
    public interface IClassEnum
    {
        /// <summary>
        /// Returns the int representation of the instance that behaves like a value in an enum
        /// </summary>
        /// <returns>Index of the instance that was defined as an enum value</returns>
        int GetIndex();
    }
}
