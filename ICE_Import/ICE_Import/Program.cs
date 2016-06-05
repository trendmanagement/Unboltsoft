using System;
using System.Windows.Forms;

namespace ICE_Import
{
    static class Program
    {
        public static FormCSV csvf;
        public static FormDB dbf;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            csvf = new FormCSV();
            dbf = new FormDB();
            Application.Run(csvf);
        }
    }
}
