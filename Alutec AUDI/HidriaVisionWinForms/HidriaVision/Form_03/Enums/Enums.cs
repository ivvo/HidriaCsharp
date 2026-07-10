using System;

namespace HidriaVision
{
    /// <summary>
    /// Status flags for common status.
    /// </summary>
    /// 
    [Serializable]
    [Flags]
    public enum Station03CommonStatusFlags
    {
        Ready = 0x01,
        Error = 0x02,
       
    }

    public enum Station03OperationSteps
    {
        StartStep = 0x00,
        Initialization = 0x01,
        CalibrationRun = 0x02,
        ToolblockRun = 0x03,
        ProcessResults = 0x04,
        ShowResults = 0x05,
        EndStep = 0x06
    }
}