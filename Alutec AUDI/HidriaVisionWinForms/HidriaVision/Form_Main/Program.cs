using System;
using System.Windows.Forms;
using System.Threading;
using Logger;
using System.Threading.Tasks;

namespace HidriaVision
{
    static class Program
    {
        #region Private static fields
        private static Form Frm_Main;
        private static Form Frm_Running;
        private static FileEventLogger Logger;
        #endregion

        #region Private static methods
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            using (var mutex = new Mutex(false, "HidriaVision"))
            {
                if (!mutex.WaitOne(TimeSpan.FromSeconds(1), false))
                {
                    // instance allready running
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    Frm_Running = new Form_Running();
                    Application.Run(Frm_Running);

                    return;
                }

                // Register global exception handlers
                Application.ThreadException += ThreadExceptionHandler;
                AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

                Logger = new FileEventLogger("./Log", 1000, LoggerErrorHandler);
                Frm_Main = new Form_Main(Logger);

                // Prepare logger
                Logger.Prepare();
                Logger.Start();

                Application.Run(Frm_Main);

                // Stop logger
                Logger.Stop();
            }
        }
        #endregion

        #region Private static events
        /// <summary>
        /// Event fires when there is a problem with logger.
        /// </summary>
        /// <param name="t">Task object.</param>
        private static void LoggerErrorHandler(Task t)
        {
            using (Form_Error errorFrm = new Form_Error(t.Exception.Message, t.Exception.ToString()))
            {
                errorFrm.ShowDialog();
            }

            // Close forms
            Frm_Main.Invoke(new Action(() => Frm_Main.Close()));
        }

        /// <summary>
        /// Event fires when exception has not been handled.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private static void ThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
        {
            // Add entry to the logger
            Logger.AddEntry(LoggingLevel.Critical, $"Critical error: {e.Exception.Message}");

            using (Form_Error errorFrm = new Form_Error(e.Exception.Message, e.Exception.ToString()))
            {
                errorFrm.ShowDialog();
            }

            // Close forms
            Frm_Main.Invoke(new Action(() => Frm_Main.Close()));
        }

        /// <summary>
        /// Event fires when exception has not been handled.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            // Add entry to the logger
            Logger.AddEntry(LoggingLevel.Critical, $"Critical error: {((Exception)e.ExceptionObject).Message}");

            using (Form_Error errorFrm = new Form_Error(((Exception)e.ExceptionObject).Message, ((Exception)e.ExceptionObject).ToString()))
            {
                errorFrm.ShowDialog();
            }

            // Close forms
            Frm_Main.Invoke(new Action(() => Frm_Main.Close()));
        }
        #endregion
    }
}
