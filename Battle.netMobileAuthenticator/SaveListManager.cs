using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Battle.netMobileAuthenticator
{
    public class SaveListRecords
    {
        public List<SaveListRecord> Records { get; set; }

        public SaveListRecords()
        {
            Records = new List<SaveListRecord>();
        }
    }

    public class SaveListManager
    {
        public static void Save(SaveListRecords savelist)
        {
            try
            {
                FileStream fs = new FileStream("SaveList.json", FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(JsonConvert.SerializeObject(savelist));
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            catch { }
        }

        public static SaveListRecords Read()
        {
            string sourceText = string.Empty;
            SaveListRecords saveListRecords = new SaveListRecords();
            try
            {
                FileStream fs = new FileStream("SaveList.json", FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                sourceText = sr.ReadToEnd();
                sr.Close();
                saveListRecords = JsonConvert.DeserializeObject<SaveListRecords>(sourceText);
                return saveListRecords;
            }
            catch
            {
                return saveListRecords;
            }
        }
    }
}
