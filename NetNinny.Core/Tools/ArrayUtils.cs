using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetNinny.Tools
{
    /// <summary>
    /// Contains utility functions for manipulating byte arrays.
    /// </summary>
    public static class ArrayUtils
    {
        /// <summary>
        /// Creates a new array containing the bytes from 0 to bytesToCopy from the srcArray.
        /// </summary>
        /// <param name="srcArray">Source array from where to copy the bytes</param>
        /// <param name="bytesToCopy">Number of bytes to copy</param>
        /// <returns>The new byte array.</returns>
        public static byte[] NewFromArray(byte[] srcArray, int bytesToCopy)
        {
            byte[] dstArray = new byte[bytesToCopy];
            for (int i = 0; i < bytesToCopy; i++)
            {
                dstArray[i] = srcArray[i];
            }
            return dstArray;
        }

        /// <summary>
        /// Merge a list of bytes array into one byte array.
        /// </summary>
        public static byte[] Merge(List<byte[]> arrays)
        {
            long size = 0;
            long i = 0;
            foreach (byte[] arr in arrays)
                size += arr.Length;
            byte[] mergedArray = new byte[size];
            foreach (byte[] arr in arrays)
            {
                foreach (byte b in arr)
                {
                    mergedArray[i] = b;
                    i++;
                }
            }
            return mergedArray;
        }

    }
}
