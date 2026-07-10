namespace LedController
{
    /// <summary>
    /// This enum represents all led channels on the RGBW controller
    /// </summary>
    public enum LedChannel
    {
        Channel_1 = 1,
        Channel_2 = 2,
        Channel_3 = 3,
        Channel_4 = 4
    }

    /// <summary>
    /// This enum represents led trigger modes
    /// </summary>
    public enum LedTriggerMode
    {
        Internal,
        External
    }

    /// <summary>
    /// This enum represents internal trigger modes
    /// </summary>
    public enum LedTriggerInternalMode
    {
        LedOff,
        LedOn
    }

    /// <summary>
    /// This enum represents surface fx states
    /// </summary>
    public enum SurfaceFxState
    {
        Disabled,
        Enabled
    }

    /// <summary>
    /// This enum represents surface fx modes
    /// </summary>
    public enum SurfaceFxMode
    {
        Normal,
        Trigger
    }

    /// <summary>
    /// This enum represents available baudrates for Rs232 communication with RGBW controller
    /// </summary>
    public enum Rs232baudrate
    {
        Baudrate_9600,
        Baudrate_14400,
        Baudrate_19200,
        baudrate_38400,
        Baudrate_57600,
        Baudrate_115200
    }
}
