
namespace Battle.netMobileAuthenticator
{
    partial class SaveListNameForm
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
            this.materialFlatButton_Login = new MaterialSkin.Controls.MaterialFlatButton();
            this.materialFlatButton1 = new MaterialSkin.Controls.MaterialFlatButton();
            this.materialSingleLineTextField_AuthName = new MaterialSkin.Controls.MaterialSingleLineTextField();
            this.SuspendLayout();
            // 
            // materialFlatButton_Login
            // 
            this.materialFlatButton_Login.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialFlatButton_Login.AutoSize = true;
            this.materialFlatButton_Login.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.materialFlatButton_Login.Depth = 0;
            this.materialFlatButton_Login.Icon = null;
            this.materialFlatButton_Login.Location = new System.Drawing.Point(235, 82);
            this.materialFlatButton_Login.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.materialFlatButton_Login.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialFlatButton_Login.Name = "materialFlatButton_Login";
            this.materialFlatButton_Login.Primary = true;
            this.materialFlatButton_Login.Size = new System.Drawing.Size(42, 36);
            this.materialFlatButton_Login.TabIndex = 1;
            this.materialFlatButton_Login.Text = "确定\r\nOK";
            this.materialFlatButton_Login.UseVisualStyleBackColor = true;
            this.materialFlatButton_Login.Click += new System.EventHandler(this.materialFlatButton_OK_Click);
            // 
            // materialFlatButton1
            // 
            this.materialFlatButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialFlatButton1.AutoSize = true;
            this.materialFlatButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.materialFlatButton1.Depth = 0;
            this.materialFlatButton1.Icon = null;
            this.materialFlatButton1.Location = new System.Drawing.Point(153, 81);
            this.materialFlatButton1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.materialFlatButton1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialFlatButton1.Name = "materialFlatButton1";
            this.materialFlatButton1.Primary = true;
            this.materialFlatButton1.Size = new System.Drawing.Size(74, 36);
            this.materialFlatButton1.TabIndex = 2;
            this.materialFlatButton1.Text = "取消\r\nCancel";
            this.materialFlatButton1.UseVisualStyleBackColor = true;
            this.materialFlatButton1.Click += new System.EventHandler(this.materialFlatButton_Cancel_Click);
            // 
            // materialSingleLineTextField_AuthName
            // 
            this.materialSingleLineTextField_AuthName.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.materialSingleLineTextField_AuthName.BackColor = System.Drawing.Color.White;
            this.materialSingleLineTextField_AuthName.Depth = 0;
            this.materialSingleLineTextField_AuthName.ForeColor = System.Drawing.Color.Transparent;
            this.materialSingleLineTextField_AuthName.Hint = "在此键入你为此安全令准备的专属名字";
            this.materialSingleLineTextField_AuthName.Location = new System.Drawing.Point(15, 46);
            this.materialSingleLineTextField_AuthName.MaxLength = 16;
            this.materialSingleLineTextField_AuthName.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialSingleLineTextField_AuthName.Name = "materialSingleLineTextField_AuthName";
            this.materialSingleLineTextField_AuthName.PasswordChar = '\0';
            this.materialSingleLineTextField_AuthName.SelectedText = "";
            this.materialSingleLineTextField_AuthName.SelectionLength = 0;
            this.materialSingleLineTextField_AuthName.SelectionStart = 0;
            this.materialSingleLineTextField_AuthName.Size = new System.Drawing.Size(262, 23);
            this.materialSingleLineTextField_AuthName.TabIndex = 3;
            this.materialSingleLineTextField_AuthName.TabStop = false;
            this.materialSingleLineTextField_AuthName.UseSystemPasswordChar = false;
            // 
            // SaveListNameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 120);
            this.Controls.Add(this.materialSingleLineTextField_AuthName);
            this.Controls.Add(this.materialFlatButton1);
            this.Controls.Add(this.materialFlatButton_Login);
            this.DisplayHeader = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SaveListNameForm";
            this.Padding = new System.Windows.Forms.Padding(20, 30, 20, 20);
            this.Resizable = false;
            this.ShowIcon = false;
            this.Style = MetroFramework.MetroColorStyle.Orange;
            this.Text = "请在此窗口键入一个此安全令的名字";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialSkin.Controls.MaterialFlatButton materialFlatButton_Login;
        private MaterialSkin.Controls.MaterialFlatButton materialFlatButton1;
        private MaterialSkin.Controls.MaterialSingleLineTextField materialSingleLineTextField_AuthName;
    }
}