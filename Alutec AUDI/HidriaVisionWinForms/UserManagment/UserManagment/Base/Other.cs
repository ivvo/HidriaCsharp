using System;

namespace UserManagment
{
    /// <summary>
    /// Represents custom event argument for the remaining login minutes
    /// </summary>
    public class RemainingMinutesLeftEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the remaining login minutes
        /// </summary>
        public int MinutesLeft { get; }

        /// <summary>
        /// Initializes new instance of RemainingMinutesLeftEventArgs
        /// </summary>
        /// <param name="remainingMinutes"></param>
        public RemainingMinutesLeftEventArgs(int minutesLeft)
        {
            MinutesLeft = minutesLeft;
        }
    }

    /// <summary>
    /// This structure represents string in base64 encoding
    /// </summary>
    public struct Base64String
    {
        #region Public fields
        public readonly string Base64Str;
        #endregion

        public Base64String(string str, bool isbase64)
        {
            // Convert string to base64 encoding if not already in base64
            if (!isbase64)
                Base64Str = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
            else
                Base64Str = str;
        }
    }
}
