using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace restaurant
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool debug = true;

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (debug)
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
                Code_Console code = new Code_Console();
                NativeMethods.AllocConsole();
                //Application.Run(new Login()); If we want to start working with forms this should be uncommented
/*                string input = Console.ReadLine();

                if (input == "1")
                {
                    Code_Eigenaar_menu eigenaar = new Code_Eigenaar_menu();
                    eigenaar.fillReservations();
                }*/
                do
                {
                    // Here we incorporate our visualization of the application
                    // The code in the do while loop will be called every 100ms so every visualization should be refreshed
                    // If the user asks for input the thread should be paused
                    code.Display();
                    code.Refresh();
                } while (true);
            }
        }
    }

    internal static class NativeMethods
    {
        // http://msdn.microsoft.com/en-us/library/ms681944(VS.85).aspx
        /// <summary>
        /// Allocates a new console for the calling process.
        /// </summary>
        /// <returns>nonzero if the function succeeds; otherwise, zero.</returns>
        /// <remarks>
        /// A process can be associated with only one console,
        /// so the function fails if the calling process already has a console.
        /// </remarks>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int AllocConsole();

        // http://msdn.microsoft.com/en-us/library/ms683150(VS.85).aspx
        /// <summary>
        /// Detaches the calling process from its console.
        /// </summary>
        /// <returns>nonzero if the function succeeds; otherwise, zero.</returns>
        /// <remarks>
        /// If the calling process is not already attached to a console,
        /// the error code returned is ERROR_INVALID_PARAMETER (87).
        /// </remarks>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int FreeConsole();
    }
}
