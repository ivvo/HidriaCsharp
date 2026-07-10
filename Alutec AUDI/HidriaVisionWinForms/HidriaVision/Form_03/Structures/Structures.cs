using ResultsView;
using System;

namespace HidriaVision
{
    /// <summary>
    /// This structure represents status of the vision station
    /// </summary>
    public struct Station03CommonStatus
    {
        public byte ProgramRequest;
        public byte ProgramResponse;
        public byte TypeRequest;
        public byte TypeResponse;
        public byte ResultRequest;
        public byte ResultResponse;
        public float AngleRequest;
        public float AngleResponse;
        public long CycleTime;
        public Station03CommonStatusFlags Status;
    }
    /// <summary>
    /// This structure represents vision results
    /// </summary>
    /// 
    [Serializable]
    public struct Station03CommonResults
    {
        public bool PartPresent;
        public bool PartUpsideDown;
        public double FoundAt;
        public bool OrientationOk;
    }

    /// <summary>
    /// This structure represents vision results inside log
    /// </summary>
    [Serializable]
    public struct Station03CommonLogResults
    {
        [Column("PHEV", "StartID")]
        public string StartID;

        [Column("PHEV", "CycleTime")]
        public string CycleTime;

        [Column("PHEV", "Image1")]
        public ImageSource Image1;      

        [Column("PHEV", "EndStatus")]
        public ResultStatus<string> EndStatus;
    }

    /// <summary>
    /// This structure represents vision inspection parameters
    /// </summary>
    public struct Station03CommonParameters
    {

    }
}
