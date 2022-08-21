using System;
using Notifications.Wpf;

namespace Battle.netMobileAuthenticator
{
    public delegate void MyDelegate(string text);
    public partial class SaveListNameForm : ResourceForm
    {
        public static string AuthName = "Test";
        public SaveListNameForm()
        {
            InitializeComponent();
        }

        private void materialFlatButton_OK_Click(object sender, EventArgs e)
        {
            if(materialSingleLineTextField_AuthName.Text != null && materialSingleLineTextField_AuthName.Text != "")
                AuthName = materialSingleLineTextField_AuthName.Text;
            else
            {
                Toast.ShowNotifiy("为安全令命名时错误", "名字不可以为空，请至少输入任意一个字符来完成命名过程", NotificationType.Error, null, null);
                return;
            }
            this.Close();
        }

        private void materialFlatButton_Cancel_Click(object sender, EventArgs e)
        {
            AuthName = "nop";
            this.Close();
        }
    }
}
