
namespace Battle.netMobileAuthenticator
{
    partial class MessageBoxEx
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.metroButton_Cancel = new MetroFramework.Controls.MetroButton();
            this.metroButton_OK = new MetroFramework.Controls.MetroButton();
            this.metroLabel_Text = new MetroFramework.Controls.MetroLabel();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.metroButton_Cancel);
            this.buttonPanel.Controls.Add(this.metroButton_OK);
            this.buttonPanel.Location = new System.Drawing.Point(4, 116);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(316, 27);
            this.buttonPanel.TabIndex = 0;
            // 
            // metroButton_Cancel
            // 
            this.metroButton_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.metroButton_Cancel.FontWeight = MetroFramework.MetroButtonWeight.Light;
            this.metroButton_Cancel.Location = new System.Drawing.Point(157, 2);
            this.metroButton_Cancel.Name = "metroButton_Cancel";
            this.metroButton_Cancel.Size = new System.Drawing.Size(75, 23);
            this.metroButton_Cancel.TabIndex = 1;
            this.metroButton_Cancel.Text = "取消";
            this.metroButton_Cancel.UseSelectable = true;
            this.metroButton_Cancel.Click += new System.EventHandler(this.metroButton_Cancel_Click);
            // 
            // metroButton_OK
            // 
            this.metroButton_OK.FontWeight = MetroFramework.MetroButtonWeight.Light;
            this.metroButton_OK.Location = new System.Drawing.Point(238, 2);
            this.metroButton_OK.Name = "metroButton_OK";
            this.metroButton_OK.Size = new System.Drawing.Size(75, 23);
            this.metroButton_OK.TabIndex = 0;
            this.metroButton_OK.Text = "好的";
            this.metroButton_OK.UseSelectable = true;
            this.metroButton_OK.Click += new System.EventHandler(this.metroButton_OK_Click);
            // 
            // metroLabel_Text
            // 
            this.metroLabel_Text.Location = new System.Drawing.Point(22, 56);
            this.metroLabel_Text.Name = "metroLabel_Text";
            this.metroLabel_Text.Size = new System.Drawing.Size(295, 59);
            this.metroLabel_Text.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroLabel_Text.TabIndex = 1;
            this.metroLabel_Text.Text = "我先预言一手，腾讯看黑悟空潜力无穷，在发售那天全网大力宣传推广，为的就是打好关系，然后就以各种理由开始挖墙脚，最后成功";
            this.metroLabel_Text.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // MessageBoxEx
            // 
            this.AcceptButton = this.metroButton_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.metroButton_Cancel;
            this.ClientSize = new System.Drawing.Size(324, 147);
            this.Controls.Add(this.metroLabel_Text);
            this.Controls.Add(this.buttonPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageBoxEx";
            this.Resizable = false;
            this.Text = "MessageBox";
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel buttonPanel;
        private MetroFramework.Controls.MetroButton metroButton_OK;
        private MetroFramework.Controls.MetroButton metroButton_Cancel;
        private MetroFramework.Controls.MetroLabel metroLabel_Text;
    }
}