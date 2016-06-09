using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICE_Import
{
    public partial class FormDB : Form
    {
        string ValidationResult;

        private async void buttonCheckPushedData_Click(object sender, EventArgs e)
        {
            EnableDisable(true);
            await Task.Run(() => ValidatePushedFuturesData());
            await Task.Run(() => ValidatePushedDailyFuturesData());
            if (!ParsedData.FuturesOnly)
            {
                await Task.Run(() => ValidatePushedOptionsData());
                await Task.Run(() => ValidatePushedDailyOptionsData());
            }
            EnableDisable(false);

            MessageBox.Show(ValidationResult);
        }

        private void ValidatePushedFuturesData()
        {
            var futureHash = new HashSet<DateTime>(ParsedData.FutureRecords.Select(item => item.StripName));
            string logMessage = string.Empty;

            for (int i = 0; i < dataGridViewContract.Rows.Count; i++)
            {
                string month = dataGridViewContract[3, i].Value.ToString();
                string year = dataGridViewContract[4, i].Value.ToString();
                string stripName = month + "." + year;
                DateTime itemDT = Convert.ToDateTime(stripName);
                futureHash.Remove(itemDT);
            }

            ValidationLogHelper(futureHash, "futures", "tblcontracts");

            ValidationResult += logMessage + "\n";
        }

        private void ValidatePushedOptionsData()
        {
            var optionHash = new HashSet<DateTime>(ParsedData.OptionRecords.Select(item => item.StripName));
            string logMessage = string.Empty;

            for (int i = 0; i < dataGridViewOption.Rows.Count; i++)
            {
                string month = dataGridViewOption[3, i].Value.ToString();
                string year = dataGridViewOption[4, i].Value.ToString();
                string stripName = month + "." + year;
                DateTime itemDT = Convert.ToDateTime(stripName);
                optionHash.Remove(itemDT);
            }

            ValidationLogHelper(optionHash, "options", "tbloptions");

            ValidationResult += logMessage + "\n";
        }

        private void ValidatePushedDailyFuturesData()
        {
            var futureDailyHash = new List<Tuple<DateTime, DateTime>>();
            string logMessage = string.Empty;

            foreach (var item in ParsedData.FutureRecords)
            {
                var tumple = Tuple.Create(item.StripName, item.Date);
                futureDailyHash.Add(tumple);
            }

            string id;
            DateTime stripName;
            DateTime date;

            for (int i = 0; i < dataGridViewDailyContract.Rows.Count; i++)
            {
                id = dataGridViewDailyContract[1, i].Value.ToString();
                stripName = GetStripNameContractFromGrid(id, dataGridViewContract);
                date = (DateTime)dataGridViewDailyContract[2, i].Value;

                var tumple = Tuple.Create(stripName, date);

                futureDailyHash.Remove(tumple);
            }

            ValidationLogHelper(futureDailyHash, "futures", "tbldailycontractsettlements");

            ValidationResult += logMessage + "\n";
        }

        private void ValidatePushedDailyOptionsData()
        {
            var optionDataHash = new HashSet<Tuple<DateTime, DateTime, double>>();
            string id;
            DateTime stripName;
            DateTime date;
            double expirationdate;
            double futureYear;
            double expirateYear;
            string logMessage = string.Empty;

            foreach (var item in ParsedData.OptionRecords)
            {
                futureYear = item.StripName.Year + item.StripName.Month / 12.0;
                expirateYear = item.Date.Year + item.Date.Month / 12.0;
                expirationdate = futureYear - expirateYear;
                var tuple = Tuple.Create(item.StripName, item.Date, expirationdate);
                optionDataHash.Add(tuple);
            }

            for (int i = 0; i < dataGridViewOptionData.Rows.Count; i++)
            {
                id = dataGridViewOptionData[1, i].Value.ToString();
                stripName = GetStripNameContractFromGrid(id, dataGridViewOption);
                date = (DateTime)dataGridViewOptionData[2, i].Value;

                futureYear = stripName.Year + stripName.Month / 12.0;
                expirateYear = date.Year + date.Month / 12.0;
                expirationdate = futureYear - expirateYear;

                var tuple = Tuple.Create(stripName, date, expirationdate);

                optionDataHash.Remove(tuple);
            }

            ValidationLogHelper(optionDataHash, "options", "tbloptiondata");

            ValidationResult += logMessage + "\n";
        }

        private DateTime GetStripNameContractFromGrid(string id, DataGridView dgv)
        {
            var itemDT = new DateTime();
            string month;
            string year;
            string stripName;

            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                if (id == dgv[0, i].Value.ToString())
                {
                    month = Utilities.MonthToStringMonthCode(Convert.ToInt32(dgv[3, i].Value.ToString()));
                    year = dgv[4, i].Value.ToString();
                    stripName = month + year;
                    return itemDT = Convert.ToDateTime(stripName);
                }
            }
            return itemDT;
        }

        private void ValidationLogHelper<T>(HashSet<T> hash, string symbTypePlural, string tblName)
        {
            if (hash.Count == 0)
            {
                AsyncTaskListener.LogMessageFormat("All {0} were pushed to {1} successfully", symbTypePlural, tblName);
            }
            else
            {
                AsyncTaskListener.LogMessageFormat("Failed to push {0} {1} to {2}:", hash.Count, symbTypePlural, tblName);
                AsyncTaskListener.LogMessage("----------------------------------");
                foreach (T item in hash)
                {
                    LogInvalidItem((dynamic)item);
                    AsyncTaskListener.LogMessage("----------------------------------");
                }
            }
        }

        private void LogInvalidItem(DateTime dt)
        {
            AsyncTaskListener.LogMessage(" - StripName " + dt);
        }

        private void LogInvalidItem(Tuple<DateTime, DateTime> tuple)
        {
            AsyncTaskListener.LogMessage(" - StripName " + tuple.Item1);
            AsyncTaskListener.LogMessage(" - Date " + tuple.Item2);
        }

        private void LogInvalidItem(Tuple<DateTime, DateTime, double> tuple)
        {
            AsyncTaskListener.LogMessage(" - StripName " + tuple.Item1);
            AsyncTaskListener.LogMessage(" - Date " + tuple.Item2);
            AsyncTaskListener.LogMessage(" - Expirationdate " + tuple.Item3);
        }
    }
}
