using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Battle.netMobileAuthenticator
{
    /// <summary>
    /// 历史记录的类
    /// </summary>
    public class HistoryRecord : AuthenticatorRecord
    {
        public HistoryRecord(string serial,string restoreCode)
        {
            Serial = serial;
            RestoreCode = restoreCode;
        }
    }

    public class HistoryRecords
    {
        public List<HistoryRecord> Records { get; set; }
    }

    public class HistoryManager
    {
        public static void Save(HistoryRecords history)
        {
            try
            {
                FileStream fs = new FileStream("History.json", FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(JsonConvert.SerializeObject(history));
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            catch { }
        }

        public static HistoryRecords Read()
        {
            string sourceText = string.Empty;
            HistoryRecords historyRecords = new HistoryRecords();
            try
            {
                FileStream fs = new FileStream("History.json", FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                sourceText = sr.ReadToEnd();
                sr.Close();
                historyRecords = JsonConvert.DeserializeObject<HistoryRecords>(sourceText);
                return historyRecords;
            }
            catch
            {
                return historyRecords;
            }
        }
    }
}
