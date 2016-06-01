using System;

namespace ICE_Import
{
    /// <summary>
    /// This helper class carries three roles for asynchronous tasks:
    /// 1. The reporter of messages.
    /// 2. The reporter of progress.
    /// 3. The measurer and reporter of the "Records Per Second" quantity (RPS).
    ///    (The class contains logic of the RPS indicator located on DB form.)
    /// </summary>
    static class AsyncTaskListener
    {
        public delegate void UpdateDelegate(string msg = null, int count = -1, double rps = double.NaN);
        public static event UpdateDelegate Updated;

        // The period of RPS calculation and reporting (in seconds)
        static TimeSpan period = new TimeSpan(0, 0, 1);

        static DateTime notchTime;
        static int notchCount;

        public static void Init(string msg = null)
        {
            notchTime = DateTime.Now;
            notchCount = 0;

            if (msg != null)
            {
                // Update text box
                Updated.Invoke(msg);
            }
        }

        public static void Update(int count = -1, string msg = null)
        {
            if (count != -1)
            {
                DateTime now = DateTime.Now;
                TimeSpan delta = now - notchTime;
                if (delta >= period)
                {
                    // It's time to calculate RPS
                    double rps = (count - notchCount) / delta.TotalSeconds;

                    // Update text box, progress bar and RPS indicator
                    Updated.Invoke(msg, count, rps);

                    // Reset the measurer
                    notchTime = now;
                    notchCount = count;
                }
                else
                {
                    // Update text box and progress bar
                    Updated.Invoke(msg, count);
                }
            }
            else if (msg != null)
            {
                // Update text box
                Updated.Invoke(msg);
            }
        }

        public static void LogMessage(string msg)
        {
            // Update text box
            Updated.Invoke(msg);
        }
    }
}
