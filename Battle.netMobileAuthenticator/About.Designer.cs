
namespace Battle.netMobileAuthenticator
{
    partial class About
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            this.aboutLabel = new MetroFramework.Controls.MetroLabel();
            this.Label_License = new MetroFramework.Controls.MetroLabel();
            this.richTextBox_License = new System.Windows.Forms.RichTextBox();
            this.metroLabel_FunctionDescription = new MetroFramework.Controls.MetroLabel();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.metroButton2 = new MetroFramework.Controls.MetroButton();
            this.SuspendLayout();
            // 
            // aboutLabel
            // 
            resources.ApplyResources(this.aboutLabel, "aboutLabel");
            this.aboutLabel.Name = "aboutLabel";
            this.aboutLabel.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // Label_License
            // 
            resources.ApplyResources(this.Label_License, "Label_License");
            this.Label_License.Name = "Label_License";
            this.Label_License.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // richTextBox_License
            // 
            resources.ApplyResources(this.richTextBox_License, "richTextBox_License");
            this.richTextBox_License.Name = "richTextBox_License";
            this.richTextBox_License.ReadOnly = true;
            this.richTextBox_License.TabStop = false;
            // 
            // metroLabel_FunctionDescription
            // 
            resources.ApplyResources(this.metroLabel_FunctionDescription, "metroLabel_FunctionDescription");
            this.metroLabel_FunctionDescription.FontSize = MetroFramework.MetroLabelSize.Small;
            this.metroLabel_FunctionDescription.Name = "metroLabel_FunctionDescription";
            this.metroLabel_FunctionDescription.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // metroButton1
            // 
            this.metroButton1.FontWeight = MetroFramework.MetroButtonWeight.Regular;
            resources.ApplyResources(this.metroButton1, "metroButton1");
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.UseSelectable = true;
            // 
            // metroButton2
            // 
            this.metroButton2.FontWeight = MetroFramework.MetroButtonWeight.Light;
            resources.ApplyResources(this.metroButton2, "metroButton2");
            this.metroButton2.Name = "metroButton2";
            this.metroButton2.UseSelectable = true;
            // 
            // About
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.metroButton2);
            this.Controls.Add(this.metroButton1);
            this.Controls.Add(this.metroLabel_FunctionDescription);
            this.Controls.Add(this.richTextBox_License);
            this.Controls.Add(this.Label_License);
            this.Controls.Add(this.aboutLabel);
            this.MaximizeBox = false;
            this.Name = "About";
            this.Resizable = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroLabel aboutLabel;
        private MetroFramework.Controls.MetroLabel Label_License;
        private System.Windows.Forms.RichTextBox richTextBox_License;
        private MetroFramework.Controls.MetroLabel metroLabel_FunctionDescription;
        private MetroFramework.Controls.MetroButton metroButton1;
        private MetroFramework.Controls.MetroButton metroButton2;
    }
}

