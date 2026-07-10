using System;

namespace ResultsView
{
    /// <summary>
    /// Status of the result.
    /// </summary>
    /// 
    [Serializable]
    public enum ResultStatus
    {
        Ok,
        Nok,
        Interrupted,
        Undefined
    }
}
