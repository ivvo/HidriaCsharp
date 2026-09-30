namespace Snap7Manager
{
    /// <summary>
    /// Enum represents PLC connection status.
    /// </summary>
    public enum PLCConnectionStatus
    {
        Offline,
        Online,
        Error
    }

    /// <summary>
    /// Enum represents S7 connection type (which PLC connection resource pool is used).
    /// When many PCs connect to the same PLC and the PG resources run out, switch some clients to OP or Basic.
    /// </summary>
    public enum PLCConnectionType
    {
        PG = 1,
        OP = 2,
        Basic = 3
    }
}
