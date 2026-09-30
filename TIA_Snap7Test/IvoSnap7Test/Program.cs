namespace IvoSnap7Test
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            //XMLSettings.XMLSettings.XMLSettingsManager.Load("Settings.Xml"); ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}