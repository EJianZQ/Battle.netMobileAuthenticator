using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Battle.netMobileAuthenticator
{
    public class OperateIniFile
    {
        #region DLL引用声明

        [DllImport("kernel32")] //返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")] //返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal,
            int size, string filePath);

        #endregion

        #region 读Ini文件

        public static string ReadIniData(string section, string key, string noText, string iniFilePath)
        {
            if (iniFilePath.IndexOf('\\') == -1)
            {
                iniFilePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + iniFilePath;
            }

            if (!File.Exists(iniFilePath))
            {
                File.CreateText(iniFilePath);
            }

            StringBuilder temp = new StringBuilder(1024);

            GetPrivateProfileString(section, key, noText, temp, 1024, iniFilePath);

            return temp.ToString();
        }

        #endregion

        #region 写Ini文件

        public static bool WriteIniData(string section, string key, string value, string iniFilePath)
        {
            if (iniFilePath.IndexOf('\\') == -1)
            {
                iniFilePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + iniFilePath;
            }

            if (!File.Exists(iniFilePath))
            {
                File.CreateText(iniFilePath);
            }

            var opStation = WritePrivateProfileString(section, key, value, iniFilePath);

            return opStation != 0;
        }

        #endregion
    }
}
