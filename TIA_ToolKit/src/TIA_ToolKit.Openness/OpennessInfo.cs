namespace TIA_ToolKit
{
    /// <summary>Verzija TIA Portala, za katero je ta gradnja (TiaVersion v Directory.Build.props).
    /// Openness se priklopi samo na odprt TIA Portal iste verzije.</summary>
    public static class OpennessInfo
    {
#if TIA_V18
        public const string TiaVersion = "V18";
#elif TIA_V19
        public const string TiaVersion = "V19";
#else
        public const string TiaVersion = "V20";
#endif
        /// <summary>Mapa z Openness DLL-ji te verzije.</summary>
        public const string OpennessDir = @"C:\Program Files\Siemens\Automation\Portal " + TiaVersion + @"\PublicAPI\" + TiaVersion;
    }
}
