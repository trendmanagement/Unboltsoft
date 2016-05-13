using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    static class Program
    {
        public static Form1 f1 = new Form1();
        public static DataBaseForm dbf = new DataBaseForm();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(f1);
        }
    }
}
