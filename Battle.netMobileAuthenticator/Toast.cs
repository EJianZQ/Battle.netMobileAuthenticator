using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace Battle.netMobileAuthenticator
{
    class Toast
    {
        /// <summary>
        /// Toast 通知(不限显示时间)
        /// </summary>
        public static void ShowNotifiy(string Title, string Text, Icon Ico)
        {
            NotifyIcon fyIcon = new NotifyIcon();
            fyIcon.Icon = Ico;
            fyIcon.BalloonTipText = Text;
            fyIcon.BalloonTipTitle = Title;
            fyIcon.Visible = true;/*必须设置显隐，因为默认值是 false 不显示通知*/
            fyIcon.ShowBalloonTip(0);
        }

        /// <summary>
        /// Toast 通知
        /// </summary>
        public static void ShowNotifiy(string Title, string Text, Icon Ico, int Timeout)
        {
            NotifyIcon fyIcon = new NotifyIcon();
            fyIcon.Icon = Ico;
            fyIcon.BalloonTipText = Text;
            fyIcon.BalloonTipTitle = Title;
            fyIcon.Visible = true;/*必须设置显隐，因为默认值是 false 不显示通知*/
            fyIcon.ShowBalloonTip(Timeout);
            Thread.Sleep(Timeout);
            fyIcon.Dispose();//释放资源来消灭右下角的图标
        }
    }
}
