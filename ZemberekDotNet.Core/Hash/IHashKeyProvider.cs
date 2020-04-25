using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Hash
{
    public interface IHashKeyProvider
    {
        /// <summary>
        ///  reads the byte array representation of the key with index.
        /// </summary>
        /// <param name="index">id of the key.</param>
        /// <returns>byte array representation of the key.</returns>
        byte[] GetKeyAsBytes(int index);

        /// <summary>
        /// Total amount of the keys.
        /// </summary>
        /// <returns>amount of keys.</returns>
        int KeyAmount();
    }
}
