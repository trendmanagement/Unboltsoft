using System;
using System.Windows.Forms;

namespace ICE_Import
{
    static class Program
    {
        public static FormCSV csvf = new FormCSV();
        public static FormDB dbf = new FormDB();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(csvf);
        }
    }
}
