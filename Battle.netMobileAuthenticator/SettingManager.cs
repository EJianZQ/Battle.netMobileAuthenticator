using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Battle.netMobileAuthenticator
{
    public enum SettingType
    {
        Global,
        CreateRestore,
        SaveList
    }

    public class SettingManager
    {
        private static object _WriteLocker = new object();

        public int Region { get; set; }

        public bool HistoryEnabled { get; set; }

        public int HistoryMaxNum { get; set; }

        public bool AutoCopyEnabled { get; set; }

        public int AutoCopyFormat { get; set; }

        public string CopyCustomFormat { get; set; }

        public bool AutoIdentifyEnabled { get; set; }

        public bool MonitorClipboardEnabled { get; set; }

        public int AutoIdentifyFormat { get; set; }

        public string AutoIdentifyCustomFormat { get; set; }

        public bool NewCodeToastEnabled { get; set; }

        /// <summary>
        /// SettingManager类读取Config并初始化
        /// </summary>
        public void Initialize()
        {
            string temp;

            #region 历史记录
            HistoryEnabled = Read(SettingType.CreateRestore, "HistoryEnabled") is "True" ? true : false;
            temp = Read(SettingType.CreateRestore, "HistoryMaxNum");
            if (temp.Length > 0)
                HistoryMaxNum = Convert.ToInt32(temp);
            else
                HistoryMaxNum = -1;
            #endregion

            #region 默认地区
            temp = Read(SettingType.CreateRestore, "Region");
            if (temp.Length > 0)
                Region = Convert.ToInt32(temp);
            else
                Region = -1;
            #endregion

            #region 自动复制
            AutoCopyEnabled = Read(SettingType.CreateRestore, "AutoCopyEnabled") is "True" ? true : false;
            temp = Read(SettingType.CreateRestore, "AutoCopyFormat");
            if (temp.Length > 0)
                AutoCopyFormat = Convert.ToInt32(temp);
            else
                AutoCopyFormat = -1;
            CopyCustomFormat = Read(SettingType.CreateRestore, "CopyCustomFormat");
            #endregion

            #region 自动识别
            AutoIdentifyEnabled = Read(SettingType.CreateRestore, "AutoIdentifyEnabled") is "True" ? true : false;
            MonitorClipboardEnabled = Read(SettingType.CreateRestore, "MonitorClipboardEnabled") is "True" ? true : false;
            temp = Read(SettingType.CreateRestore, "AutoIdentifyFormat");
            if (temp.Length > 0)
                AutoIdentifyFormat = Convert.ToInt32(temp);
            else
                AutoIdentifyFormat = -1;
            #endregion

            #region 验证码推送
            NewCodeToastEnabled = Read(SettingType.CreateRestore, "NewCodeToastEnabled") is "True" ? true : false;
            #endregion
        }

        public static void Write(SettingType type,string key,string value)
        {
            lock(_WriteLocker)
            {
                string section = string.Empty;
                switch (type)
                {
                    case SettingType.Global:
                        section = "Global";
                        break;
                    case SettingType.CreateRestore:
                        section = "CreateRestore";
                        break;
                    case SettingType.SaveList:
                        section = "SaveList";
                        break;
                }
                OperateIniFile.WriteIniData(section, key, value, "Config.ini");
            }
        }

        public static string Read(SettingType type, string key)
        {
            string section = string.Empty;
            switch (type)
            {
                case SettingType.Global:
                    section = "Global";
                    break;
                case SettingType.CreateRestore:
                    section = "CreateRestore";
                    break;
                case SettingType.SaveList:
                    section = "SaveList";
                    break;
            }
            return OperateIniFile.ReadIniData(section, key, "", "Config.ini"); ;
        }
    }
}
