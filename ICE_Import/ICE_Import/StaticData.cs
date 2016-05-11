using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE_Import
{
    public static class StaticData
    {
        public delegate void ParseEventHandler();
        public static event ParseEventHandler ParseComplete;
        public static DataBaseForm dbf;
        public static object[] optionRecords;
        public static object[] futureRecords;

        public static void OnParseComplete()
        {
            if(dbf == null)
            {
                dbf = new DataBaseForm();
                dbf.Show();
            }
            else if (dbf.Visible == false)
            {
                dbf = new DataBaseForm();
                dbf.Show();
            }
            ParseComplete();
        }
    }
}
