using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using HZH_Controls;
using Newtonsoft.Json;
using Notifications.Wpf;

namespace Battle.netMobileAuthenticator
{
    public partial class MainForm : ResourceForm
    {
        #region WindowsAPI申明
        /// <summary>
        /// 将CWnd加入一个窗口链，每当剪贴板的内容发生变化时，就会通知这些窗口
        /// </summary>
        /// <param name="hWndNewViewer">句柄</param>
        /// <returns>返回剪贴板观察器链中下一个窗口的句柄</returns>
        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        /// <summary>
        /// 从剪贴板链中移出的窗口句柄
        /// </summary>
        /// <param name="hWndRemove">从剪贴板链中移出的窗口句柄</param>
        /// <param name="hWndNewNext">hWndRemove的下一个在剪贴板链中的窗口句柄</param>
        /// <returns>如果成功，非零;否则为0。</returns>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        /// <summary>
        /// 将指定的消息发送到一个或多个窗口
        /// </summary>
        /// <param name="hwnd">其窗口程序将接收消息的窗口的句柄</param>
        /// <param name="wMsg">指定被发送的消息</param>
        /// <param name="wParam">指定附加的消息特定信息</param>
        /// <param name="lParam">指定附加的消息特定信息</param>
        /// <returns>消息处理的结果</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        #endregion
        public WinAuthAuthenticator Authenticator { get; set; }
        BattleNetAuthenticator authenticator = new BattleNetAuthenticator();
        public HistoryRecords history = new HistoryRecords();
        public SaveListRecords savelist = new SaveListRecords();
        public List<WinAuthAuthenticator> SaveListAuthenticators = new List<WinAuthAuthenticator>();
        public static SettingManager smg;
        //private object _newAuthenticatorLocker = new object();
        private readonly object _historyHandle = new object();
        private int timerToastLocker = 0;
        private bool autoIdentifyLocker = false;
        IntPtr nextClipboardViewer;//剪切板观察链
        Image[] blackIcon = new Image[4] { FontImages.GetImage(FontIcons.A_fa_shield, 36, Color.Black) , FontImages.GetImage(FontIcons.A_fa_list, 36, Color.Black), FontImages.GetImage(FontIcons.A_fa_cogs, 36, Color.Black) , FontImages.GetImage(FontIcons.A_fa_sellsy, 36, Color.Black) };
        Image[] blueIcon = new Image[4] { FontImages.GetImage(FontIcons.A_fa_shield, 36, Color.FromArgb(0, 174, 219)), FontImages.GetImage(FontIcons.A_fa_list, 36, Color.FromArgb(0, 174, 219)), FontImages.GetImage(FontIcons.A_fa_cogs, 36, Color.FromArgb(0, 174, 219)) , FontImages.GetImage(FontIcons.A_fa_sellsy, 36, Color.FromArgb(0, 174, 219)) };
        Image[] commonIcon = new Image[4] { FontImages.GetImage(FontIcons.A_fa_bars, 20, Color.FromArgb(80, 80, 80)), FontImages.GetImage(FontIcons.A_fa_undo, 20, Color.FromArgb(80, 80, 80)) , FontImages.GetImage(FontIcons.A_fa_pencil, 20, Color.FromArgb(80, 80, 80)) , FontImages.GetImage(FontIcons.A_fa_clone, 20, Color.FromArgb(80, 80, 80)) };
        public MainForm()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;

            Authenticator = new WinAuthAuthenticator();
            history.Records = new List<HistoryRecord>();
            
            #region 载入配置处理
            Thread SettingLoadThread = new Thread(() => 
            { 
                if (File.Exists("Config.ini"))
                {
                    smg = new SettingManager();
                    smg.Initialize();

                    #region 历史记录处理
                    if (smg.HistoryEnabled == true)
                    {
                        metroCheckBox_History.CheckState = CheckState.Checked;
                        if (smg.HistoryMaxNum != -1)
                            metroComboBox_HistoryMaxNum.SelectedIndex = smg.HistoryMaxNum;
                        else
                            metroComboBox_HistoryMaxNum.SelectedIndex = 0;
                        if (File.Exists("History.json"))
                        {
                            history = HistoryManager.Read();
                            if(history.Records.Count > 0)
                            {
                                if(history.Records.Count > (smg.HistoryMaxNum + 1) * 3)
                                {
                                    while(history.Records.Count > (smg.HistoryMaxNum + 1) * 3)
                                    {
                                        history.Records.RemoveAt(0);
                                    }
                                }
                                foreach(HistoryRecord record in history.Records)
                                {
                                    metroComboBox_History.Items.Add(record.Serial);
                                }
                                metroComboBox_History.PromptText = string.Format("有{0}条历史记录", metroComboBox_History.Items.Count);
                            }
                        }
                    }
                    #endregion

                    #region 默认区服处理
                    if (smg.Region != -1)
                    {
                        metroComboBox_Region.SelectedIndex = smg.Region;
                        metroComboBox_RegionSetting.SelectedIndex = smg.Region;
                    }
                    else
                    {
                        metroComboBox_Region.SelectedIndex = 0;
                    }
                    #endregion

                    #region 自动复制处理
                    if (smg.AutoCopyEnabled == true)
                    {
                        metroCheckBox_AutoCopy.CheckState = CheckState.Checked;
                        if (smg.AutoCopyFormat != -1)
                            metroComboBox_AutoCopy.SelectedIndex = smg.AutoCopyFormat;
                        else
                            metroComboBox_AutoCopy.SelectedIndex = 0;
                    }
                    if (smg.CopyCustomFormat.Length > 0)
                        MetroTextBox_CopyCustomFormat.Text = smg.CopyCustomFormat;
                    #endregion

                    #region 自动识别处理
                    if(smg.AutoIdentifyEnabled == true)
                    {
                        metroCheckBox_AutoIdentify.CheckState = CheckState.Checked;
                        if (smg.AutoIdentifyFormat != -1)
                            metroComboBox_AutoIdentifyFormot.SelectedIndex = smg.AutoIdentifyFormat;
                        else
                            metroComboBox_AutoIdentifyFormot.SelectedIndex = 0;
                        if (smg.MonitorClipboardEnabled == true)
                        {
                            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)Handle);
                            metroCheckBox_MonitorClipboard.CheckState = CheckState.Checked; 
                        }
                    }
                    #endregion

                    #region 验证码推送处理
                    if(smg.NewCodeToastEnabled == true)
                    {
                        metroCheckBox_NewCodeToast.CheckState = CheckState.Checked;
                    }
                    if (smg.NewCodeAutoCopyEnabled == true)
                    {
                        metroCheckBox_NewCodeAutoCopy.CheckState = CheckState.Checked;
                    }
                    #endregion
                }
                else
                {
                    try
                    {
                        File.Create("Config.ini").Close();//!!!Create方法返回的是文件流，不关闭在后续操作中无法写入
                        metroComboBox_Region.SelectedIndex = 0;
                    }
                    catch { }
                }
            });
            SettingLoadThread.Start();
            #endregion

            #region 设置组件图标
            Thread iconGetThread = new Thread(() =>
            {
                ucBtnImg_Index.Image = blueIcon[0];
                ucBtnImg_SaveList.Image = blackIcon[1];
                ucBtnImg_Setting.Image = blackIcon[2];
                ucBtnImg_CloudConfig.Image = blackIcon[3];
                pictureBox_Region.Image = FontImages.GetImage(FontIcons.A_fa_globe, 25, Color.Black);
                pictureBox_History.Image = FontImages.GetImage(FontIcons.A_fa_history, 25, Color.Black);
                pictureBox_CopyCustomFormatInfo.Image = FontImages.GetImage(FontIcons.A_fa_exclamation_circle, 21, Color.FromArgb(0, 174, 219));
                pictureBox_AutoIdentifyCustomFormatInfo.Image = FontImages.GetImage(FontIcons.A_fa_exclamation_circle, 21, Color.FromArgb(0, 174, 219));
                pictureBox_NewCodeAutoCopy.Image = FontImages.GetImage(FontIcons.A_fa_exclamation_circle, 21, Color.FromArgb(0, 174, 219));
                pictureBox_CopyCustomFormatCheck.Image = null;
                ucBtnImg_AddList.Image = FontImages.GetImage(FontIcons.A_fa_plus, 26, Color.FromArgb(102, 102, 102));//_square
                ucBtnImg_CreateSetting.Image = FontImages.GetImage(FontIcons.A_fa_cog, 26, Color.FromArgb(102, 102, 102));

                metroTextBox_Serial.Icon = FontImages.GetImage(FontIcons.A_fa_bars, 48, Color.FromArgb(102, 102, 102));
                metroTextBox_RestoreCode.Icon = FontImages.GetImage(FontIcons.A_fa_undo, 48, Color.FromArgb(102, 102, 102));
                metroTextBox_CurrentCode.Icon = FontImages.GetImage(FontIcons.A_fa_check_circle_o, 48, Color.FromArgb(102, 102, 102));
                pictureBox_QQ.Image = FontImages.GetImage(FontIcons.A_fa_qq, 80, Color.FromArgb(0, 174, 219));
                pictureBox_SaveListSerial1.Image = commonIcon[0];
                pictureBox_SaveListRestoreCode1.Image = commonIcon[1];
                pictureBox_auth1edit.Image = commonIcon[2];
                pictureBox_auth1copy.Image = commonIcon[3];
            });
            iconGetThread.Start();
            #endregion
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tabControl_Main.Region = new Region(new RectangleF(tabPage_Create.Left, tabPage_Create.Top, tabPage_Create.Width, tabPage_Create.Height));
            tabControl_CloudConfig.Region = new Region(new RectangleF(tabPage_LoginQQ.Left, tabPage_LoginQQ.Top, tabPage_LoginQQ.Width, tabPage_LoginQQ.Height));
            tabControl_SaveList.Region = new Region(new RectangleF(tabPage_SaveListEx.Left, tabPage_SaveListEx.Top, tabPage_SaveListEx.Width, tabPage_SaveListEx.Height));
        }

        #region 剪切板监控与自动识别
        /// <summary>
        /// 要处理的 WindowsSystem.Windows.Forms.Message。
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    DisplayClipboardData();
                    SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;
                case WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer)
                        nextClipboardViewer = m.LParam;
                    else
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <summary>
        /// 操作剪贴板内容
        /// </summary>
        public void DisplayClipboardData()
        {
            try
            {
                IDataObject iData = new DataObject();
                iData = Clipboard.GetDataObject();

                if (iData.GetDataPresent(DataFormats.Text))
                {
                    if(autoIdentifyLocker == false)
                    {
                        switch (metroComboBox_AutoIdentifyFormot.SelectedIndex)
                        {
                            case 0:
                                {
                                    //地表最强的安全令模糊匹配!!!
                                    Regex Serial = new Regex(@"(CN|KR|EU|US)(-[0-9]{4}){3}");
                                    Regex RestoreCode = new Regex(@"[A-Z0-9_]{10}");
                                    var temp = (string)iData.GetData(DataFormats.Text);
                                    if (Serial.IsMatch(temp) == true && RestoreCode.IsMatch(temp) == true)
                                    {
                                        metroTextBox_Serial.Text = Serial.Match(temp).Value;
                                        metroTextBox_RestoreCode.Text = RestoreCode.Match(temp).Value;
                                        MatchCollection RestoreCodeCollection = Regex.Matches(temp, @"[A-Z0-9_]{10}");
                                        Console.WriteLine(RestoreCodeCollection.Count);
                                        if (RestoreCodeCollection.Count == 1)
                                        {
                                            metroTextBox_RestoreCode.Text = RestoreCodeCollection[0].ToString();
                                        }
                                        else
                                        {
                                            foreach (Match m in RestoreCodeCollection)
                                            {
                                                if (Regex.Matches(m.ToString(), "[a-zA-Z]").Count > 0)
                                                {
                                                    metroTextBox_RestoreCode.Text = m.ToString();
                                                }
                                            }
                                        }
                                        metroTextBox_CurrentCode.Text = "自动识别成功请还原安全令";
                                        metroProgressBar.Visible = false;
                                        authenticatorTimer.Enabled = false;
                                        Authenticator.AuthenticatorData = null;
                                        Toast.ShowNotifiy("剪切板监控 - 自动识别安全令", string.Format("已从剪切板自动识别到安全令\n序列号：{0}\n还原码：{1}\n单击此消息可一键还原安全令", metroTextBox_Serial.Text, metroTextBox_RestoreCode.Text), NotificationType.Information, Toast_Re, null);
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    Regex regex = new Regex(@"(CN|KR|EU|US)(-[0-9]{4}){3}----[A-Z0-9_]{10}");//(CN|KR|EU|US)-[0-9]{4}-[0-9]{4}-[0-9]{4}----[A-Z0-9_]{10}
                                    if (regex.IsMatch((string)iData.GetData(DataFormats.Text)))
                                    {
                                        var temp = (string)iData.GetData(DataFormats.Text);
                                        Regex Serial = new Regex(@"(CN|KR|EU|US)(-[0-9]{4}){3}");
                                        metroTextBox_Serial.Text = Serial.Match(temp).Value;
                                        Regex RestoreCode = new Regex(@"[A-Z0-9_]{10}");
                                        metroTextBox_RestoreCode.Text = RestoreCode.Match(temp).Value;
                                        metroTextBox_CurrentCode.Text = "自动识别成功请还原安全令";
                                        metroProgressBar.Visible = false;
                                        authenticatorTimer.Enabled = false;
                                        Authenticator.AuthenticatorData = null;
                                        Toast.ShowNotifiy("剪切板监控 - 自动识别安全令", string.Format("已从剪切板自动识别到安全令\n序列号：{0}\n还原码：{1}\n单击此消息可一键还原安全令", metroTextBox_Serial.Text, metroTextBox_RestoreCode.Text), NotificationType.Information, Toast_Re, null);
                                    }
                                    else
                                    {
                                        Console.WriteLine("格式不正确");
                                    }
                                    break;
                                }
                            default: break;
                        }
                    }
                    autoIdentifyLocker = false;

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }

        private void Toast_Re()
        {
            metroButton_Restore_Click(null, null);
        }
        #endregion

        #region 总选项卡切换
        private void ucBtnImg_Index_BtnClick(object sender, EventArgs e)
        {
            ucBtnImg_Index.Image = blueIcon[0];
            ucBtnImg_SaveList.Image = blackIcon[1];
            ucBtnImg_Setting.Image = blackIcon[2];
            ucBtnImg_CloudConfig.Image = blackIcon[3];
            tabControl_Main.SelectedIndex = 0;
            if(Authenticator.AuthenticatorData != null)
                metroTextBox_CurrentCode.Text = this.Authenticator.AuthenticatorData.CurrentCode;

        }

        private void ucBtnImg_SaveList_BtnClick(object sender, EventArgs e)
        {
            ucBtnImg_Index.Image = blackIcon[0];
            ucBtnImg_SaveList.Image = blueIcon[1];
            ucBtnImg_Setting.Image = blackIcon[2];
            ucBtnImg_CloudConfig.Image = blackIcon[3];
            tabControl_Main.SelectedIndex = 1;
        }

        private void ucBtnImg_Setting_BtnClick(object sender, EventArgs e)
        {
            ucBtnImg_Index.Image = blackIcon[0];
            ucBtnImg_SaveList.Image = blackIcon[1];
            ucBtnImg_Setting.Image = blueIcon[2];
            ucBtnImg_CloudConfig.Image = blackIcon[3];
            tabControl_Main.SelectedIndex = 2;
        }

        private void ucBtnImg_CloudConfig_BtnClick(object sender, EventArgs e)
        {
            ucBtnImg_Index.Image = blackIcon[0];
            ucBtnImg_SaveList.Image = blackIcon[1];
            ucBtnImg_Setting.Image = blackIcon[2];
            ucBtnImg_CloudConfig.Image = blueIcon[3];
            tabControl_Main.SelectedIndex = 3;
        }

        private void ucBtnImg_CreateSetting_BtnClick(object sender, EventArgs e)
        {
            ucBtnImg_Index.Image = blackIcon[0];
            ucBtnImg_SaveList.Image = blackIcon[1];
            ucBtnImg_Setting.Image = blueIcon[2];
            ucBtnImg_CloudConfig.Image = blackIcon[3];
            tabControl_Main.SelectedIndex = 2;
            metroTabControl_Setting.SelectedIndex = 1;
        }
        #endregion

        #region 自动复制
        public void AutoCopy()
        { 
            string copiedText = string.Empty;

            if (metroCheckBox_AutoCopy.CheckState == CheckState.Checked && metroComboBox_AutoCopy.SelectedIndex != -1)
            {
                switch (metroComboBox_AutoCopy.SelectedIndex)
                {
                    case 0:
                        {
                            copiedText = authenticator.CurrentCode;
                            break;
                        }
                    case 1:
                        {
                            copiedText = string.Format("{0}——{1}", authenticator.Serial, authenticator.RestoreCode);
                            autoIdentifyLocker = true;
                            break;
                        }
                    case 2:
                        {
                            copiedText = string.Format("{0}——{1}——{2}", authenticator.Serial, authenticator.RestoreCode, authenticator.CurrentCode);
                            autoIdentifyLocker = true;
                            break;
                        }
                    case 3:
                        {
                            if (MetroTextBox_CopyCustomFormat.Text.Contains("序") == true && MetroTextBox_CopyCustomFormat.Text.Contains("还") == true)
                                autoIdentifyLocker = true;
                            copiedText = MetroTextBox_CopyCustomFormat.Text.Replace("序", authenticator.Serial).Replace("还", authenticator.RestoreCode).Replace("验", authenticator.CurrentCode);
                            break;
                        }
                    default:
                        {
                            copiedText = authenticator.CurrentCode;
                            break;
                        }
                }
                Clipboard.SetDataObject(copiedText);
                Toast.ShowNotifiy("暴雪安全令", "已进行自动复制，按Ctrl+V粘贴即可\n复制格式为设置中的选定格式", NotificationType.Success, null, null);
            }
        }
        #endregion

        #region 创建与还原安全令

        /// <summary>
        /// 创建安全令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButton_Create_Click(object sender, EventArgs e)
        {
            string copiedText = string.Empty;

            metroButton_Create.Enabled = metroButton_Restore.Enabled = false;
            metroButton_Create.Highlight = true;
            metroButton_Restore.Highlight = false;

            try
            {
                authenticator = new BattleNetAuthenticator();
                authenticator.Enroll(metroComboBox_Region.SelectedIndex);
            }
            catch(Exception ex)
            {
                Toast.ShowNotifiy("暴雪安全令 - 创建出现错误", string.Format("创建时出现了一个致命错误\n{0}\n单击此处显示详细错误信息", ex.Message), NotificationType.Error, Click: () => MessageBox.Show(string.Format("异常消息：\n{0}\n调用堆栈上的即时框架字符串表示形式：\n{1}\n来源：\n{2}\n有关异常的其他用户定义信息的键/值对集合：\n{3}\n提示：按 Ctrl+C 复制当前错误信息并发送至开发人员以协助解决问题", ex.Message, ex.StackTrace, ex.Source, ex.Data),"详细错误信息"));
                return;
            }
            finally
            {
                metroButton_Create.Enabled = metroButton_Restore.Enabled = true;
            }
            Authenticator.AuthenticatorData = authenticator;
            metroTextBox_Serial.Text = authenticator.Serial;
            metroTextBox_RestoreCode.Text = authenticator.RestoreCode;
            metroTextBox_CurrentCode.Text = authenticator.CurrentCode;

            metroProgressBar.Visible = true;
            authenticatorTimer.Enabled = true;

            AutoCopy();

            Thread historyHandleThread = new Thread(() =>
            {
                lock (_historyHandle)
                {
                    if (metroCheckBox_History.CheckState == CheckState.Checked && metroComboBox_HistoryMaxNum.SelectedIndex != -1)
                    {
                        if (metroComboBox_History.Items.Count >= (metroComboBox_HistoryMaxNum.SelectedIndex + 1) * 3)
                        {
                            metroComboBox_History.Items.RemoveAt(0);
                            history.Records.RemoveAt(0);
                        }
                        metroComboBox_History.Items.Add(authenticator.Serial);
                        history.Records.Add(new HistoryRecord(authenticator.Serial, authenticator.RestoreCode));
                    }
                }
                if (metroComboBox_History.Items.Count != 0)
                    metroComboBox_History.PromptText = string.Format("有{0}条历史记录", metroComboBox_History.Items.Count);
            });
            historyHandleThread.Start();
            Toast.ShowNotifiy("暴雪安全令", "已创建一个全新的安全令\n请注意保存序列号和还原码", NotificationType.Information, null, null);
        }

        /// <summary>
        /// 还原安全令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroButton_Restore_Click(object sender, EventArgs e)
        {
            metroButton_Create.Enabled = metroButton_Restore.Enabled = false;
            metroButton_Create.Highlight = false;
            metroButton_Restore.Highlight = true;

            try
            {
                authenticator = new BattleNetAuthenticator();
                authenticator.Restore(metroTextBox_Serial.Text, metroTextBox_RestoreCode.Text);
            }
            catch(Exception ex)
            {
                Toast.ShowNotifiy("暴雪安全令 - 还原出现错误", string.Format("还原时出现了一个致命错误\n{0}\n单击此处显示详细错误信息",ex.Message), NotificationType.Error, Click: () => MessageBox.Show(string.Format("异常消息:\n{0}\n调用堆栈上的即时框架字符串表示形式\n{1}\n来源：\n{2}\n有关异常的其他用户定义信息的键/值对集合：\n{3}\n提示：按 Ctrl+C 复制当前错误信息并发送至开发人员以协助解决问题", ex.Message,ex.StackTrace,ex.Source,ex.Data, "详细错误信息")));
                return;
            }
            finally
            {
                metroButton_Create.Enabled = metroButton_Restore.Enabled = true;
            }
            Authenticator.AuthenticatorData = authenticator;
            metroTextBox_Serial.Text = authenticator.Serial;
            metroTextBox_CurrentCode.Text = authenticator.CurrentCode;
            metroTextBox_RestoreCode.Text = authenticator.RestoreCode;

            metroProgressBar.Visible = true;
            authenticatorTimer.Enabled = true;


            AutoCopy();

            Thread historyHandleThread = new Thread(() =>
            {
                lock (_historyHandle)
                {
                    if (metroCheckBox_History.CheckState == CheckState.Checked && metroComboBox_HistoryMaxNum.SelectedIndex != -1 && metroComboBox_History.Items.Contains(authenticator.Serial) != true)
                    {
                        if (metroComboBox_History.Items.Count >= (metroComboBox_HistoryMaxNum.SelectedIndex + 1) * 3)
                        {
                            metroComboBox_History.Items.RemoveAt(0);
                            history.Records.RemoveAt(0);
                        }
                        metroComboBox_History.Items.Add(authenticator.Serial);
                        history.Records.Add(new HistoryRecord(authenticator.Serial, authenticator.RestoreCode));
                    }
                }
                if (metroComboBox_History.Items.Count != 0)
                    metroComboBox_History.PromptText = string.Format("有{0}条历史记录", metroComboBox_History.Items.Count);
            });
            historyHandleThread.Start();
        }
        #endregion

        #region 组件操作

        #region 纯配置类组件
        private void metroComboBox_HistoryMaxNum_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingManager.Write(SettingType.CreateRestore, "HistoryMaxNum", metroComboBox_HistoryMaxNum.SelectedIndex.ToString());
        }

        private void metroComboBox_RegionSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if(smg != null)
            SettingManager.Write(SettingType.CreateRestore, "Region", metroComboBox_RegionSetting.SelectedIndex.ToString());
        }

        private void metroCheckBox_History_CheckStateChanged(object sender, EventArgs e)
        {
            metroComboBox_HistoryMaxNum.Enabled = metroCheckBox_History.Checked;
            if (smg != null)
            {
                if (metroComboBox_HistoryMaxNum.SelectedIndex == -1)
                {
                    metroComboBox_HistoryMaxNum.SelectedIndex = 0;
                    SettingManager.Write(SettingType.CreateRestore, "HistoryMaxNum", metroComboBox_HistoryMaxNum.SelectedIndex.ToString());
                }
            }
            else
            {
                metroComboBox_HistoryMaxNum.SelectedIndex = 0;
                SettingManager.Write(SettingType.CreateRestore, "HistoryMaxNum", metroComboBox_HistoryMaxNum.SelectedIndex.ToString());
            }
            SettingManager.Write(SettingType.CreateRestore, "HistoryEnabled", metroCheckBox_History.CheckState is CheckState.Checked ? "True" : "False");
        }


        private void metroCheckBox_AutoCopy_CheckStateChanged(object sender, EventArgs e)
        {
            metroComboBox_AutoCopy.Enabled = metroCheckBox_AutoCopy.Checked;
            if (smg != null)
            {
                if (metroComboBox_AutoCopy.SelectedIndex == -1)
                {
                    metroComboBox_AutoCopy.SelectedIndex = 0;
                    SettingManager.Write(SettingType.CreateRestore, "AutoCopyFormat", metroComboBox_AutoCopy.SelectedIndex.ToString());
                }
            }
            else
            {
                metroComboBox_AutoCopy.SelectedIndex = 0;
                SettingManager.Write(SettingType.CreateRestore, "AutoCopyFormat", metroComboBox_AutoCopy.SelectedIndex.ToString());
            }
            SettingManager.Write(SettingType.CreateRestore, "AutoCopyEnabled", metroCheckBox_AutoCopy.CheckState is CheckState.Checked ? "True" : "False");
        }

        private void metroComboBox_AutoCopy_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingManager.Write(SettingType.CreateRestore, "AutoCopyFormat", metroComboBox_AutoCopy.SelectedIndex.ToString());
        }

        private void metroCheckBox_AutoIdentify_CheckStateChanged(object sender, EventArgs e)
        {
            metroCheckBox_MonitorClipboard.Enabled = metroComboBox_AutoIdentifyFormot.Enabled = metroTextBox_IdentifyCustomFormot.Enabled = metroCheckBox_AutoIdentify.Checked;
            if (smg != null)
            {
                if (metroComboBox_AutoIdentifyFormot.SelectedIndex == -1)
                {
                    metroComboBox_AutoIdentifyFormot.SelectedIndex = 0;
                    SettingManager.Write(SettingType.CreateRestore, "AutoCopyFormat", metroComboBox_AutoIdentifyFormot.SelectedIndex.ToString());
                }
            }
            else
            {
                metroComboBox_AutoIdentifyFormot.SelectedIndex = 0;
                SettingManager.Write(SettingType.CreateRestore, "AutoCopyFormat", metroComboBox_AutoIdentifyFormot.SelectedIndex.ToString());
            }
            SettingManager.Write(SettingType.CreateRestore, "AutoIdentifyEnabled", metroCheckBox_AutoIdentify.CheckState is CheckState.Checked ? "True" : "False");
        }

        private void metroComboBox_AutoIdentifyFormot_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingManager.Write(SettingType.CreateRestore, "AutoIdentifyFormat", metroComboBox_AutoIdentifyFormot.SelectedIndex.ToString());
        }

        private void metroCheckBox_MonitorClipboard_CheckStateChanged(object sender, EventArgs e)
        {
            SettingManager.Write(SettingType.CreateRestore, "MonitorClipboardEnabled", metroCheckBox_MonitorClipboard.CheckState is CheckState.Checked ? "True" : "False");
        }

        private void metroTextBox_IdentifyCustomFormot_TextChanged(object sender, EventArgs e)
        {

        }

        private void metroCheckBox_NewCodeToast_CheckStateChanged(object sender, EventArgs e)
        {
            SettingManager.Write(SettingType.CreateRestore, "NewCodeToastEnabled", metroCheckBox_NewCodeToast.CheckState is CheckState.Checked ? "True" : "False");
        }

        private void metroCheckBox_NewCodeAutoCopy_CheckStateChanged(object sender, EventArgs e)
        {
            SettingManager.Write(SettingType.CreateRestore, "NewCodeAutoCopyEnabled", metroCheckBox_NewCodeAutoCopy.CheckState is CheckState.Checked ? "True" : "False");
        }
        #endregion

        #region 功能类组件
        /// <summary>
        /// 选项卡背景重绘
        /// </summary>
        private void metroTabControl_Setting_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = metroTabControl_Setting.TabPages[e.Index];
            e.Graphics.FillRectangle(new SolidBrush(page.BackColor), e.Bounds);

            Rectangle paddedBounds = e.Bounds;
            int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            paddedBounds.Offset(1, yOffset);
            TextRenderer.DrawText(e.Graphics, page.Text, this.Font, paddedBounds, page.ForeColor);
        }

        /// <summary>
        /// 载入指定历史记录安全令并重置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroComboBox_History_SelectedIndexChanged(object sender, EventArgs e)
        {
            metroTextBox_Serial.Text = history.Records[metroComboBox_History.SelectedIndex].Serial;
            metroTextBox_RestoreCode.Text = history.Records[metroComboBox_History.SelectedIndex].RestoreCode;
            metroTextBox_CurrentCode.Text = "请还原安全令";
            metroProgressBar.Visible = false;
            authenticatorTimer.Enabled = false;
            Authenticator.AuthenticatorData = null;
        }

        /// <summary>
        /// 关闭窗口时保存历史记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (history.Records.Count > 0)
            {
                HistoryManager.Save(history);
            }
        }

        /// <summary>
        /// 自动识别安全令
        /// 正则匹配
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void metroTextBox_Serial_TextChanged(object sender, EventArgs e)
        {
            if (metroTextBox_Serial.Text.Length >= 27)
            {
                Thread autoIdentifyThread = new Thread(() =>
                {
                    if(autoIdentifyLocker == false)
                    {
                        switch (metroComboBox_AutoIdentifyFormot.SelectedIndex)
                        {
                            case 0:
                                {
                                    //地表最强的安全令模糊匹配!!!
                                    Regex Serial = new Regex(@"(CN|KR|EU|US)(-[0-9]{4}){3}");
                                    Regex RestoreCode = new Regex(@"[A-Z0-9_]{10}");
                                    var temp = metroTextBox_Serial.Text;
                                    if (Serial.IsMatch(temp) == true && RestoreCode.IsMatch(temp) == true)
                                    {
                                        metroTextBox_Serial.Text = Serial.Match(temp).Value;
                                        metroTextBox_RestoreCode.Text = RestoreCode.Match(temp).Value;
                                        MatchCollection RestoreCodeCollection = Regex.Matches(temp, @"[A-Z0-9_]{10}");
                                        Console.WriteLine(RestoreCodeCollection.Count);
                                        if (RestoreCodeCollection.Count == 1)
                                        {
                                            metroTextBox_RestoreCode.Text = RestoreCodeCollection[0].ToString();
                                        }
                                        else
                                        {
                                            foreach (Match m in RestoreCodeCollection)
                                            {
                                                if (Regex.Matches(m.ToString(), "[a-zA-Z]").Count > 0)
                                                {
                                                    metroTextBox_RestoreCode.Text = m.ToString();
                                                }
                                            }
                                        }
                                        metroTextBox_CurrentCode.Text = "自动识别成功请还原安全令";
                                        metroProgressBar.Visible = false;
                                        authenticatorTimer.Enabled = false;
                                        Authenticator.AuthenticatorData = null;
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    Regex regex = new Regex(@"(CN|KR|EU|US)(-[0-9]{4}){3}----[A-Z0-9_]{10}");//(CN|KR|EU|US)-[0-9]{4}-[0-9]{4}-[0-9]{4}----[A-Z0-9_]{10}
                                    if (regex.IsMatch(metroTextBox_Serial.Text))
                                    {
                                        var temp = metroTextBox_Serial.Text;
                                        Regex Serial = new Regex(@"(CN|KR|EU|US)(-[0-9]{4}){3}");
                                        metroTextBox_Serial.Text = Serial.Match(temp).Value;
                                        Regex RestoreCode = new Regex(@"[A-Z0-9_]{10}");
                                        metroTextBox_RestoreCode.Text = RestoreCode.Match(temp).Value;
                                        metroTextBox_CurrentCode.Text = "自动识别成功请还原安全令";
                                        metroProgressBar.Visible = false;
                                        authenticatorTimer.Enabled = false;
                                        Authenticator.AuthenticatorData = null;
                                    }
                                    else
                                    {
                                        Console.WriteLine("格式不正确");
                                    }
                                    break;
                                }
                            default: break;
                        }
                    }
                    autoIdentifyLocker = false;
                });
                autoIdentifyThread.Start();
            }
        }

        /// <summary>
        /// 自定义复制格式里的内容被改变，显示保存按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MetroTextBox_CopyCustomFormat_TextChanged(object sender, EventArgs e)
        {
            if (MetroTextBox_CopyCustomFormat.Text.Length > 0 && MetroTextBox_CopyCustomFormat.Text != smg.CopyCustomFormat)
            {
                pictureBox_CopyCustomFormatCheck.Image = FontImages.GetImage(FontIcons.A_fa_check_circle_o, 21, Color.LimeGreen);
                pictureBox_CopyCustomFormatCheck.Enabled = true;
                pictureBox_CopyCustomFormatCheck.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// 保存自定义复制格式
        /// </summary>
        private void pictureBox_CopyCustomFormatCheck_Click(object sender, EventArgs e)
        {
            SettingManager.Write(SettingType.CreateRestore, "CopyCustomFormat", MetroTextBox_CopyCustomFormat.Text.Trim());
            pictureBox_CopyCustomFormatCheck.Image = null;
            pictureBox_CopyCustomFormatCheck.Enabled = false;
            pictureBox_CopyCustomFormatCheck.Cursor = Cursors.Default;
            Toast.ShowNotifiy("设置 - Setting", "自动复制中的自定义格式内容已保存", NotificationType.Success, null, null);
        }

        /// <summary>
        /// 删除配置文件
        /// </summary>
        private void metroButton_RestoreSettings_Click(object sender, EventArgs e)
        {
            if (File.Exists("Config.ini") == true)
            {
                MessageBoxEx messageBox = new MessageBoxEx("此操作会删除保存的配置文件，但不会删除其他数据如历史记录等数据\n如果确定删除，删除后会自动重启软件，是否删除？\n\n\n\n\n", "还原默认设置", true);
                if (messageBox.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.Delete("Config.ini");
                    }
                    catch
                    {
                        MessageBoxEx messageBoxEx = new MessageBoxEx("删除失败，请尝试关闭软件后手动删除目录中的Config.ini文件\n\n", "还原默认设置", false);
                        messageBoxEx.ShowDialog();
                    }
                    finally
                    {
                        Application.Exit();
                        System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    }
                }
            }
            else
            {
                MessageBoxEx messageBox = new MessageBoxEx("配置文件不存在", "还原默认设置", false);
                messageBox.ShowDialog();
            }
        }

        /// <summary>
        /// 删除所有数据
        /// </summary>
        private void metroButton_DeleteData_Click(object sender, EventArgs e)
        {
            MessageBoxEx messageBox = new MessageBoxEx("此操作会删除所有数据，如保存的配置软件、历史记录和常驻列表等所有数据\n如果确定删除，删除后会自动重启软件，是否删除？\n\n\n\n\n", "还原默认设置", true);
            if (messageBox.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (File.Exists("Config.ini"))
                        File.Delete("Config.ini");
                    if (File.Exists("History.json"))
                        File.Delete("History.json");
                }
                catch
                {
                    MessageBoxEx messageBoxEx = new MessageBoxEx("删除失败，请尝试关闭软件后手动删除目录中的Config.ini、History.json文件\n\n", "还原默认设置", false);
                    messageBoxEx.ShowDialog();
                }
                finally
                {
                    Application.Exit();
                    System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
            }
        }
        #endregion

        #region 通知(提示)类组件
        /// <summary>
        /// 自定义复制格式的提示
        /// </summary>
        private void pictureBox_CopyCustomFormatInfo_Click(object sender, EventArgs e)
        {
            MessageBoxEx messageBox = new MessageBoxEx("将\"序\"\"还\"\"验\"三个元素作为特征\n如填写的是：序&&&还&&&验\n则自动复制为：序列号&&&还原码&&&验证码\n序、还、验三个元素可以不用同时出现\n填写完后需点击绿色的确认按钮才可保存\nBTW：请不要填写一些奇奇怪怪的格式以免出错", "格式说明", false);
            messageBox.ShowDialog();
        }

        /// <summary>
        /// 自定义自动识别格式的提示
        /// </summary>
        private void pictureBox_AutoIdentifyCustomFormatInfo_Click(object sender, EventArgs e)
        {
            MessageBoxEx messageBox = new MessageBoxEx("该功能由于较复杂且开发意义不是很大(能实现，但并不实现)，所以暂时搁置开发，请期待后续更新。等大功能实现并且完善后会来做这个，目前墙裂推荐使用智能识别！", "格式说明", false);
            messageBox.ShowDialog();
        }

        /// <summary>
        /// 验证码刷新时自动复制格式的提示
        /// </summary>
        private void pictureBox_NewCodeAutoCopy_Click(object sender, EventArgs e)
        {
            MessageBoxEx messageBox = new MessageBoxEx("复制的格式与\"自动复制\"设置中的格式一致", "格式说明", false);
            messageBox.ShowDialog();
        }


        #endregion

        #endregion

        /// <summary>
        /// 时钟刷新验证码
        /// </summary>
        private void authenticatorTimer_Tick(object sender, EventArgs e)
        {
            if (Authenticator.AuthenticatorData != null && metroProgressBar.Visible == true)
            {
                int time = (int)(this.Authenticator.AuthenticatorData.ServerTime / 1000L) % 30;
                metroProgressBar.Value = time + 1;
                if (time == 0)
                {
                    timerToastLocker++;
                    Console.WriteLine("Locker值:" + timerToastLocker);
                    metroTextBox_CurrentCode.Text = this.Authenticator.AuthenticatorData.CurrentCode;
                    if(timerToastLocker % 2 == 0)
                    {
                        if (metroCheckBox_NewCodeToast.CheckState == CheckState.Checked)
                            Toast.ShowNotifiy("暴雪安全令 - 验证码刷新推送", metroCheckBox_NewCodeAutoCopy.CheckState is CheckState.Checked ? "验证码已刷新，新的验证码为" + Authenticator.CurrentCode + "\n单击此消息可将指定格式复制至剪切板" : "验证码已刷新，新的验证码为" + Authenticator.CurrentCode + "\n已自动复制至剪切板", NotificationType.Information, AutoCopy, null);
                        if (metroCheckBox_NewCodeToast.CheckState == CheckState.Checked)
                            AutoCopy();
                    }
                }
            }
        }



        private void Auth1Timer_Tick(object sender, EventArgs e)
        {
            if (SaveListAuthenticators[0] != null)
            {
                int time = (int)(SaveListAuthenticators[0].AuthenticatorData.ServerTime / 1000L) % 30;
                circularProgressBar_Auth1.Value = time + 1;
                if (time == 0)
                    label__Auth1Code.Text = SaveListAuthenticators[0].CurrentCode;
            }
        }

        private void Auth2Timer_Tick(object sender, EventArgs e)
        {
            if (SaveListAuthenticators[1] != null)
            {
                int time = (int)(SaveListAuthenticators[1].AuthenticatorData.ServerTime / 1000L) % 30;
                circularProgressBar_Auth2.Value = time + 1;
                if (time == 0)
                    label__Auth2Code.Text = SaveListAuthenticators[1].CurrentCode;
            }
        }

        private void Auth3Timer_Tick(object sender, EventArgs e)
        {
            if (SaveListAuthenticators[2] != null)
            {
                int time = (int)(SaveListAuthenticators[2].AuthenticatorData.ServerTime / 1000L) % 30;
                circularProgressBar_Auth3.Value = time + 1;
                if (time == 0)
                    label__Auth3Code.Text = SaveListAuthenticators[2].CurrentCode;
            }
        }
        private void panel_Auth1_Paint(object sender, PaintEventArgs e)
        {
            Panel pan = (Panel)sender;
            float width = (float)1.0;
            Pen pen = new Pen(SystemColors.ControlDark, width);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            e.Graphics.DrawLine(pen, 0, 0, 0, pan.Height - 0);
            e.Graphics.DrawLine(pen, 0, 0, pan.Width - 0, 0);
            e.Graphics.DrawLine(pen, pan.Width - 1, pan.Height - 1, 0, pan.Height - 1);
            e.Graphics.DrawLine(pen, pan.Width - 1, pan.Height - 1, pan.Width - 1, 0);
        }

        private void panel_Auth2_Paint(object sender, PaintEventArgs e)
        {
            Panel pan = (Panel)sender;
            float width = (float)1.0;
            Pen pen = new Pen(SystemColors.ControlDark, width);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            e.Graphics.DrawLine(pen, 0, 0, 0, pan.Height - 0);
            e.Graphics.DrawLine(pen, 0, 0, pan.Width - 0, 0);
            e.Graphics.DrawLine(pen, pan.Width - 1, pan.Height - 1, 0, pan.Height - 1);
            e.Graphics.DrawLine(pen, pan.Width - 1, pan.Height - 1, pan.Width - 1, 0);
        }

        private void panel_Auth3_Paint(object sender, PaintEventArgs e)
        {
            Panel pan = (Panel)sender;
            float width = (float)1.0;
            Pen pen = new Pen(SystemColors.ControlDark, width);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            e.Graphics.DrawLine(pen, 0, 0, 0, pan.Height - 0);
            e.Graphics.DrawLine(pen, 0, 0, pan.Width - 0, 0);
            e.Graphics.DrawLine(pen, pan.Width - 1, pan.Height - 1, 0, pan.Height - 1);
            e.Graphics.DrawLine(pen, pan.Width - 1, pan.Height - 1, pan.Width - 1, 0);
        }

        private void ucBtnImg_AddList_BtnClick(object sender, EventArgs e)
        {
            if(Authenticator.AuthenticatorData != null)
            {
                SaveListNameForm form = new SaveListNameForm();
                form.ShowDialog();
                if(SaveListNameForm.AuthName != "nop" && SaveListNameForm.AuthName != "Test")
                {
                    if(savelist.Records.Count < 3 && SaveListAuthenticators.Count < 3)
                    {
                        savelist.Records.Add(new SaveListRecord(SaveListNameForm.AuthName, authenticator.Serial, authenticator.RestoreCode));
                        var temp = DeepCopy.DeepCopyByBinary(this.Authenticator);
                        SaveListAuthenticators.Add(temp);
                        //SaveListAuthenticators.Add(this.Authenticator);
                        if (savelist.Records.Count == SaveListAuthenticators.Count)
                        {
                            switch (savelist.Records.Count)
                            {
                                case 1:
                                    {
                                        if(SaveListAuthenticators[0].AuthenticatorData != null)
                                        {
                                            label__Auth1name.Text = savelist.Records[0].Name;
                                            label__Auth1Serial.Text = savelist.Records[0].Serial;
                                            label__Auth1RestoreCode.Text = savelist.Records[0].RestoreCode;
                                            label__Auth1Code.Text = SaveListAuthenticators[0].CurrentCode;
                                            Auth1Timer.Enabled = true;
                                        }
                                        else
                                        {
                                            MessageBoxEx messageBox = new MessageBoxEx("安全令列表第1个数据异常，重新运行软件可能会解决此问题", "常驻列表", false);
                                            messageBox.ShowDialog();
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        if (SaveListAuthenticators[1].AuthenticatorData != null)
                                        {
                                            label__Auth2name.Text = savelist.Records[1].Name;
                                            label__Auth2Serial.Text = savelist.Records[1].Serial;
                                            label__Auth2RestoreCode.Text = savelist.Records[1].RestoreCode;
                                            label__Auth2Code.Text = SaveListAuthenticators[1].CurrentCode;
                                            pictureBox_SaveListSerial2.Image = commonIcon[0];
                                            pictureBox_SaveListRestoreCode2.Image = commonIcon[1];
                                            pictureBox_auth2edit.Image = commonIcon[2];
                                            pictureBox_auth2copy.Image = commonIcon[3];
                                            panel_Auth2.Visible = true;
                                            Auth2Timer.Enabled = true;
                                        }
                                        else
                                        {
                                            MessageBoxEx messageBox = new MessageBoxEx("安全令列表第1个数据异常，重新运行软件可能会解决此问题", "常驻列表", false);
                                            messageBox.ShowDialog();
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        if (SaveListAuthenticators[2].AuthenticatorData != null)
                                        {
                                            label__Auth3name.Text = savelist.Records[2].Name;
                                            label__Auth3Serial.Text = savelist.Records[2].Serial;
                                            label__Auth3RestoreCode.Text = savelist.Records[2].RestoreCode;
                                            label__Auth3Code.Text = SaveListAuthenticators[2].CurrentCode;
                                            pictureBox_SaveListSerial3.Image = commonIcon[0];
                                            pictureBox_SaveListRestoreCode3.Image = commonIcon[1];
                                            pictureBox_auth3edit.Image = commonIcon[2];
                                            pictureBox_auth3copy.Image = commonIcon[3];
                                            panel_Auth3.Visible = true;
                                            Auth3Timer.Enabled = true;
                                        }
                                        else
                                        {
                                            MessageBoxEx messageBox = new MessageBoxEx("安全令列表第1个数据异常，重新运行软件可能会解决此问题", "常驻列表", false);
                                            messageBox.ShowDialog();
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        MessageBoxEx messageBox = new MessageBoxEx("安全令列表的数量异常，重新运行软件可能会解决此问题", "常驻列表", false);
                                        messageBox.ShowDialog();
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            MessageBoxEx messageBox = new MessageBoxEx("安全令String类型与实例类型数量不匹配，重新运行软件可能会解决此问题", "常驻列表", false);
                            messageBox.ShowDialog();
                        }
                    }
                    else
                    {
                        MessageBoxEx messageBox = new MessageBoxEx("当前常驻列表安全令数量已满\n目前只支持添加三个安全令，请删除一个后重新添加\n\n\n\n\n", "常驻列表", false);
                        messageBox.ShowDialog();
                    }
                }
                
            }
            else
            {
                MessageBoxEx messageBox = new MessageBoxEx("如果需要将当前安全令添加至常驻列表，必须先进行创建或还原操作\n只有创建或还原成功的安全令才允许被添加至常驻列表\n\n\n\n\n", "常驻列表", false);
                messageBox.ShowDialog();
            }
        }
    }
}