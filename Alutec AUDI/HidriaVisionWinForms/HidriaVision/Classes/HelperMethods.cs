using System;
using System.Collections;

namespace HidriaVision
{
    public static class HelperMethods
    {
        /// <summary>
        /// Get smaller value.         
        /// </summary>
        /// <param name="Value A">Value A.</param>
        /// <param name="Value B">Value B.</param>
        public static int GetSmaller(int a, int b)
        {
            if (a < b) return a;
            else return b;
        }

        /// <summary>
        /// Get smaller value.         
        /// </summary>
        /// <param name="Value A">Value A.</param>
        /// <param name="Value B">Value B.</param>
        public static double GetSmaller(double a, double b)
        {
            if (a < b) return a;
            else return b;
        }

        /// <summary>
        /// Get larger value.        
        /// </summary>
        /// <param name="Value A">Value A.</param>
        /// <param name="Value B">Value B.</param>
        public static int GetLarger(int a, int b)
        {
            if (a > b) return a;
            else return b;
        }

        /// <summary>
        /// Get larger value.         
        /// </summary>
        /// <param name="Value A">Value A.</param>
        /// <param name="Value B">Value B.</param>
        public static double GetLarger(double a, double b)
        {
            if (a > b) return a;
            else return b;
        }

        /// <summary>
        /// Get delta.
        /// </summary>
        /// <param name="Value A">Value A.</param>
        /// <param name="Value B">Value B.</param>
        public static int GetDelta(int a, int b)
        {
            return GetLarger(a, b) - GetSmaller(a, b);
        }

        /// <summary>
        /// Get delta.        
        /// </summary>
        /// <param name="Value A">Value A.</param>
        /// <param name="Value B">Value B.</param>
        public static double GetDelta(double a, double b)
        {
            return GetLarger(a, b) - GetSmaller(a, b);
        }

        /// <summary>
        /// Get absoulte delta.           
        /// </summary>
        /// <param name="Value A">Value A.</param>
        /// <param name="Value B">Value B.</param>
        public static int GetAbsoulteDelta(int a, int b)
        {
            return Math.Abs(GetDelta(a, b));
        }

        /// <summary>
        /// Get absolute delta.           
        /// </summary>
        /// <param name="Value A">Value A.</param>
        /// <param name="Value B">Value B.</param>
        public static double GetAbsoulteDelta(double a, double b)
        {
            return Math.Abs(GetDelta(a, b));
        }

        /// <summary>
        /// Is value in range?           
        /// </summary>
        /// <param name="Value val">Value.</param>
        /// <param name="Value min">Value min.</param>
        /// <param name="Value max">Value max.</param>
        public static bool IsInRange(int min, int max, int val)
        {
            if (val >= min && val <= max) return true;
            else return false;
        }

        /// <summary>
        /// Is value in range?           
        /// </summary>
        /// <param name="Value val">Value.</param>
        /// <param name="Value min">Value min.</param>
        /// <param name="Value max">Value max.</param>
        public static bool IsInRange(double min, double max, double val)
        {
            if (val >= min && val <= max) return true;
            else return false;
        }

        /// <summary>
        /// Set status bit.
        /// </summary>
        /// <param name="bitNmb">Bit number.</param>
        /// <param name="value">Value.</param>
        /// <param name="byteValue">Byte value.</param>
        public static void SetStatusBit(int bitNmb, bool value, ref byte byteValue)
        {
            byte[] bytearray = new byte[1];
            bytearray[0] = byteValue;

            var bitArray = new BitArray(bytearray);

            bitArray.Set(bitNmb, value);
            bitArray.CopyTo(bytearray, 0);
            byteValue = bytearray[0];
        }

        /// <summary>
        /// Get status bit.
        /// </summary>
        /// <param name="bitNmb">Bit number</param>
        /// <param name="value">Value.</param>
        /// <returns>Returns bit value.</returns>
        public static bool GetStatusBit(int bitNmb, byte value)
        {
            byte[] bytearray = new byte[1];
            bytearray[0] = value;

            var bitArray = new BitArray(bytearray);

            return bitArray.Get(bitNmb);
        }
    }
}
