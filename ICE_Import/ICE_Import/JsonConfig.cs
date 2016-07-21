using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE_Import
{
    public class JsonConfig
    {
        public ICEConfiguration ICE_Configuration;

        public class ICEConfiguration
        {
            public string TMLDB_Description;
            public string[] Regular_Options;
        }

    }
}
