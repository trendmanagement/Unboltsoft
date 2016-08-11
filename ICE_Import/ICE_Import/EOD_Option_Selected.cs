using System;

namespace ICE_Import
{
    class EOD_Option_Selected
    {
        public DateTime FutureStripName;
        public EOD_Option Option;

        public EOD_Option_Selected(DateTime futureStripName, EOD_Option option)
        {
            FutureStripName = futureStripName;
            Option = option;
        }
    }
}
