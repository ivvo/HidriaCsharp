using System;

namespace ResultsView
{
    /// <summary>
    /// This structure represents result status.
    /// </summary>
    /// <typeparam name="T">Primitive type.</typeparam>
    public struct ResultStatus<T> where T :  IConvertible
    {
        #region Public fields
        public readonly T Value;
        public readonly ResultStatus ResStatus;
        #endregion

        /// <summary>
        /// Constructs structure of type ResultStatus.
        /// </summary>
        /// <param name="value">Result value.</param>
        /// <param name="resStatus">Result status.</param>
        public ResultStatus(T value, ResultStatus resStatus)
        {
            Value = value;
            ResStatus = resStatus;
        }
    }
}
