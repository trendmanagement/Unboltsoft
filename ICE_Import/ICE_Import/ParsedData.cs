using System.Windows.Forms;

namespace ICE_Import
{
    internal static class ParsedData
    {
        public delegate void ParseEventHandler();
        public static event ParseEventHandler ParseComplete;
        public static EOD_Futures_578[] FutureRecords;
        public static EOD_Options_578[] OptionRecords;
        public static bool FuturesOnly;

        public static bool IsReady
        {
            get
            {
                if (FuturesOnly)
                {
                    return FutureRecords != null;
                }
                else
                {
                    return OptionRecords != null && FutureRecords != null;
                }
            }
        }

        public static void OnParseComplete()
        {
            if (ConformityCheck())
            {
                Program.csvf.Hide();
                if (Program.dbf == null)
                {
                    Program.dbf = new FormDB();
                }
                Program.dbf.Show();

                // Raise event
                ParseComplete();
            }
            else
            {
                MessageBox.Show("Check the input data files correspond to each other!");
            }
        }

        private static bool ConformityCheck()
        {
            if (IsReady)
            {
                foreach (EOD_Futures_578 f in FutureRecords)
                {
                    foreach (EOD_Options_578 o in OptionRecords)
                    {
                        if (f.StripName == o.StripName)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            return false;

        }
    }
}
