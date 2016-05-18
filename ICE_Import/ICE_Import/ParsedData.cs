namespace ICE_Import
{
    internal static class ParsedData
    {
        public delegate void ParseEventHandler();
        public static event ParseEventHandler ParseComplete;
        public static EOD_Futures_578[] FutureRecords;
        public static EOD_Options_578[] OptionRecords;
        public static bool justFuture;

        public static bool IsReady
        {
            get
            {
                if (justFuture)
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
            Program.csvf.Hide();
            if (Program.dbf == null)
            {
                Program.dbf = new FormDB();
            }
            Program.dbf.Show();

            // Raise event
            ParseComplete();
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
