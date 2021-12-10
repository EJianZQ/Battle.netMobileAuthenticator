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
        public HistoryRecords history = new HistoryRecords();
        public static SettingManager smg;
        //private object _newAuthenticatorLocker = new object();
        private readonly object _historyHandle = new object();
        private object _toastLocker = new object();
        IntPtr nextClipboardViewer;//剪切板观察链
        Image[] blackIcon = new Image[3] { FontImages.GetImage(FontIcons.A_fa_shield, 36, Color.Black) , FontImages.GetImage(FontIcons.A_fa_list, 36, Color.Black), FontImages.GetImage(FontIcons.A_fa_cogs, 36, Color.Black) };
        Image[] blueIcon = new Image[3] { FontImages.GetImage(FontIcons.A_fa_shield, 36, Color.FromArgb(0, 174, 219)), FontImages.GetImage(FontIcons.A_fa_list, 36, Color.FromArgb(0, 174, 219)), FontImages.GetImage(FontIcons.A_fa_cogs, 36, Color.FromArgb(0, 174, 219)) };
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

            Thread iconGetThread = new Thread(() =>
            {
                ucBtnImg_Index.Image = blueIcon[0];
                ucBtnImg_SaveList.Image = blackIcon[1];
                ucBtnImg_Setting.Image = blackIcon[2];
                pictureBox_Region.Image = FontImages.GetImage(FontIcons.A_fa_globe, 25, Color.Black);
                pictureBox_History.Image = FontImages.GetImage(FontIcons.A_fa_history, 25, Color.Black);
                pictureBox_CopyCustomFormatInfo.Image = FontImages.GetImage(FontIcons.A_fa_exclamation_circle, 21, Color.FromArgb(0, 174, 219));
                pictureBox_CopyCustomFormatCheck.Image = null;
                ucBtnImg_AddList.Image = FontImages.GetImage(FontIcons.A_fa_plus, 26, Color.FromArgb(102, 102, 102));//_square
                ucBtnImg_CreateSetting.Image = FontImages.GetImage(FontIcons.A_fa_cog, 26, Color.FromArgb(102, 102, 102));

                metroTextBox_Serial.Icon = FontImages.GetImage(FontIcons.A_fa_bars, 48, Color.FromArgb(102, 102, 102));
                metroTextBox_RestoreCode.Icon = FontImages.GetImage(FontIcons.A_fa_undo, 48, Color.FromArgb(102, 102, 102));
                metroTextBox_CurrentCode.Icon = FontImages.GetImage(FontIcons.A_fa_check_circle_o, 48, Color.FromArgb(102, 102, 102));
            });
            iconGetThread.Start();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tabControl_Main.Region = new Region(new RectangleF(tabPage_Create.Left, tabPage_Create.Top, tabPage_Create.Width, tabPage_Create.Height));
            //tabControl_Create.Region = new Region(new RectangleF(tabPage_Create_Main.Left, tabPage_Create_Main.Top, tabPage_Create_Main.Width, tabPage_Create_Main.Height));
        }

        #region 剪切板监控
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
                    switch (metroComboBox_AutoIdentifyFormot.SelectedIndex)
                    {
                        case 0:
                            {
                                //地表最强的模糊匹配!!!
                                Regex Serial = new Regex(@"(CN|KR|EU|US)(-[0-9]{4}){3}");
                                Regex RestoreCode = new Regex(@"[A-Z0-9_]{10}");
                                var temp = (string)iData.GetData(DataFormats.Text);
                                if (Serial.IsMatch(temp) == true && RestoreCode.IsMatch(temp) == true)
                                {
                                    metroTextBox_Serial.Text = Serial.Match(temp).Value;
                                    metroTextBox_RestoreCode.Text = RestoreCode.Match(temp).Value;
                                    metroTextBox_CurrentCode.Text = "自动识别成功请还原安全令";
                                    metroProgressBar.Visible = false;
                                    authenticatorTimer.Enabled = false;
                                    Authenticator.AuthenticatorData = null;
                                    Thread toastThread = new Thread(() =>
                                    {
                                        Toast.ShowNotifiy("剪切板监控 - 自动识别安全令", string.Format("已从剪切板自动识别到安全令\n序列号：{0}\n还原码：{1}", metroTextBox_Serial.Text, metroTextBox_RestoreCode.Text), this.Icon, 5000);
                                    });
                                    toastThread.Start();
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
                                    Thread toastThread = new Thread(() =>
                                    {
                                        Toast.ShowNotifiy("剪切板监控 - 自动识别安全令", string.Format("已从剪切板自动识别到安全令\n序列号：{0}\n还原码：{1}", metroTextBox_Serial.Text, metroTextBox_RestoreCode.Text), this.Icon, 5000);
                                    });
                                    toastThread.Start();
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
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }
        #endregion

        #region 总选项卡切换
        private void ucBtnImg_Index_BtnClick(object sender, EventArgs e)
        {
            ucBtnImg_Index.Image = blueIcon[0];
            ucBtnImg_SaveList.Image = blackIcon[1];
            ucBtnImg_Setting.Image = blackIcon[2];
            tabControl_Main.SelectedIndex = 0;
        }

        private void ucBtnImg_SaveList_BtnClick(object sender, EventArgs e)
        {
            ucBtnImg_Index.Image = blackIcon[0];
            ucBtnImg_SaveList.Image = blueIcon[1];
            ucBtnImg_Setting.Image = blackIcon[2];
        }

        private void ucBtnImg_Setting_BtnClick(object sender, EventArgs e)
        {
            ucBtnImg_Index.Image = blackIcon[0];
            ucBtnImg_SaveList.Image = blackIcon[1];
            ucBtnImg_Setting.Image = blueIcon[2];
            tabControl_Main.SelectedIndex = 2;
        }
        #endregion

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

            BattleNetAuthenticator authenticator = new BattleNetAuthenticator();
            authenticator.Enroll(metroComboBox_Region.SelectedIndex);
            Authenticator.AuthenticatorData = authenticator;
            metroTextBox_Serial.Text = authenticator.Serial;
            metroTextBox_RestoreCode.Text = authenticator.RestoreCode;
            metroTextBox_CurrentCode.Text = authenticator.CurrentCode;

            metroProgressBar.Visible = true;
            authenticatorTimer.Enabled = true;
            metroButton_Create.Enabled = metroButton_Restore.Enabled = true;

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
                            break;
                        }
                    case 2:
                        {
                            copiedText = string.Format("{0}——{1}——{2}", authenticator.Serial, authenticator.RestoreCode, authenticator.CurrentCode);
                            break;
                        }
                    default:
                        {
                            copiedText = authenticator.CurrentCode;
                            break;
                        }
                }
                Clipboard.SetDataObject(copiedText);

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
            }
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

            BattleNetAuthenticator authenticator = new BattleNetAuthenticator();
            authenticator.Restore(metroTextBox_Serial.Text, metroTextBox_RestoreCode.Text);
            Authenticator.AuthenticatorData = authenticator;
            metroTextBox_Serial.Text = authenticator.Serial;
            metroTextBox_CurrentCode.Text = authenticator.CurrentCode;
            metroTextBox_RestoreCode.Text = authenticator.RestoreCode;

            metroProgressBar.Visible = true;
            authenticatorTimer.Enabled = true;
            metroButton_Create.Enabled = metroButton_Restore.Enabled = true;

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
                    metroTextBox_CurrentCode.Text = this.Authenticator.AuthenticatorData.CurrentCode;
                    if(metroCheckBox_NewCodeToast.CheckState == CheckState.Checked)
                    {
                        Thread toastThread = new Thread(() =>
                        {
                            string text = string.Empty;
                            text = metroCheckBox_AutoCopy.CheckState is CheckState.Checked ? "验证码已刷新，新的验证码为" + Authenticator.CurrentCode : "验证码已刷新，新的验证码为" + Authenticator.CurrentCode + "\n已自动复制至剪切板";
                            Toast.ShowNotifiy("验证码刷新", text, this.Icon, 5000);
                        });
                        toastThread.Start();
                    }
                }
            }
        }

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
            if(metroTextBox_Serial.Text.Length >= 27)
            {
                Thread autoIdentifyThread = new Thread(() =>
                {
                    switch (metroComboBox_AutoIdentifyFormot.SelectedIndex)
                    {
                        case 0:
                            {
                                //地表最强的模糊匹配!!!
                                Regex Serial = new Regex(@"(CN|KR|EU|US)(-[0-9]{4}){3}");
                                Regex RestoreCode = new Regex(@"[A-Z0-9_]{10}");
                                var temp = metroTextBox_Serial.Text;
                                if (Serial.IsMatch(temp) == true && RestoreCode.IsMatch(temp) == true)
                                {
                                    metroTextBox_Serial.Text = Serial.Match(temp).Value;
                                    metroTextBox_RestoreCode.Text = RestoreCode.Match(temp).Value;
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
                        default:break;
                    }
                });
                autoIdentifyThread.Start();
            }
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

        private void metroCheckBox_History_CheckStateChanged(object sender, EventArgs e)
        {
            metroComboBox_HistoryMaxNum.Enabled = metroCheckBox_History.Checked;
            if (smg != null)
            {
                if(metroComboBox_HistoryMaxNum.SelectedIndex == -1)
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
            SettingManager.Write(SettingType.CreateRestore,"HistoryEnabled", metroCheckBox_History.CheckState is CheckState.Checked ? "True" : "False");
        }

        private void metroComboBox_HistoryMaxNum_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingManager.Write(SettingType.CreateRestore, "HistoryMaxNum", metroComboBox_HistoryMaxNum.SelectedIndex.ToString());
        }

        private void metroComboBox_RegionSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if(smg != null)
            SettingManager.Write(SettingType.CreateRestore, "Region", metroComboBox_RegionSetting.SelectedIndex.ToString());
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

        private void pictureBox_CopyCustomFormatInfo_Click(object sender, EventArgs e)
        {
            MessageBoxEx messageBox = new MessageBoxEx("A为序列号B为还原码C为验证码\n如填写的是：A&&&&B&&&&C\n则自动复制为：序列号&&&&还原码&&&&验证码\nA、B、C三个元素可以不用同时出现\n填写完后需点击绿色的确认按钮才可保存\nBTW：请不要填写一些奇奇怪怪的格式以免出错", "格式说明",false);
            messageBox.ShowDialog();
        }

        private void MetroTextBox_CopyCustomFormat_TextChanged(object sender, EventArgs e)
        {
            if (MetroTextBox_CopyCustomFormat.Text.Length > 0 && MetroTextBox_CopyCustomFormat.Text != smg.CopyCustomFormat)
            {
                pictureBox_CopyCustomFormatCheck.Image = FontImages.GetImage(FontIcons.A_fa_check_circle_o, 21, Color.LimeGreen);
                pictureBox_CopyCustomFormatCheck.Enabled = true;
                pictureBox_CopyCustomFormatCheck.Cursor = Cursors.Hand;
            }
        }

        private void pictureBox_CopyCustomFormatCheck_Click(object sender, EventArgs e)
        {
            SettingManager.Write(SettingType.CreateRestore, "CopyCustomFormat", MetroTextBox_CopyCustomFormat.Text.Trim());
            Thread toastThread = new Thread(() =>
            {
                lock(_toastLocker)
                {
                    Toast.ShowNotifiy("设置 - Setting", "自动复制中的自定义格式内容已保存", this.Icon, 5000);
                }
            });
            toastThread.Start();
            pictureBox_CopyCustomFormatCheck.Image = null;
            pictureBox_CopyCustomFormatCheck.Enabled = false;
            pictureBox_CopyCustomFormatCheck.Cursor = Cursors.Default;
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
    }
}
