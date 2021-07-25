using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battle.netMobileAuthenticator
{
    public partial class MessageBoxEx : ResourceForm
    {
        /// <summary>
        /// 自定义信息框
        /// </summary>
        /// <param name="LabelText">信息框内容</param>
        /// <param name="Title">信息框标题</param>
        /// <param name="showCancel">是否显示取消按钮</param>
        /// <param name="OKButtonText">确认按钮内容</param>
        /// <param name="CancelButtonText">取消按钮内容</param>
        public MessageBoxEx(string LabelText,string Title,bool showCancel = true,string OKButtonText = "好的",string CancelButtonText = "取消")
        {
            InitializeComponent();

            this.Text = Title;
            metroButton_Cancel.Visible = showCancel;
            metroButton_Cancel.Text = CancelButtonText;
            metroButton_OK.Text = OKButtonText;

            #region 计算行数
            int newLineCount = 0;
            string search = "\n";
            for (int i = 0; i < LabelText.Length - search.Length; i++)
            {
                if (LabelText.Substring(i, search.Length) == search)
                {
                    newLineCount++;
                }
            }

            metroLabel_Text.Text = LabelText;
            if (newLineCount > 0 && LabelText.Length > 36)
                metroLabel_Text.Size = new Size(metroLabel_Text.Size.Width, (LabelText.Length / 18 + newLineCount - 3) * 18);
            else if (newLineCount > 0)
                metroLabel_Text.Size = new Size(metroLabel_Text.Size.Width, (LabelText.Length / 18 + + newLineCount + 1) * 18);
            else
                metroLabel_Text.Size = new Size(metroLabel_Text.Size.Width, (LabelText.Length / 18 + 1) * 18);
            buttonPanel.Location = new Point(0, metroLabel_Text.Location.Y + metroLabel_Text.Size.Height + 2);
            this.Size = new Size(this.Size.Width, buttonPanel.Location.Y + buttonPanel.Size.Height + 6);
            #endregion
        }

        private void metroButton_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void metroButton_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}