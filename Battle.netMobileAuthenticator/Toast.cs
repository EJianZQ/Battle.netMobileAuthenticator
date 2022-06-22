using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using Notifications.Wpf;
using System;

namespace Battle.netMobileAuthenticator
{
    class Toast
    {
        private static readonly NotificationManager _notificationManager = new NotificationManager();
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
        /// Toast 通知 WPF版
        /// </summary>
        public static void ShowNotifiy(string Title, string Text, NotificationType Type)
        {
            var content = new NotificationContent
            {
                Title = Title,
                Message = Text,
                Type = Type
            };
            _notificationManager.Show(content);
        }

        /// <summary>
        /// Toast 通知 WPF版 带方法
        /// </summary>
        public static void ShowNotifiy(string Title, string Text, NotificationType Type, Action Click = null, Action Close = null)
        {
            var content = new NotificationContent
            {
                Title = Title,
                Message = Text,
                Type = Type
            };
            if(Click == null)
                Click = new Action(WPFError);
            if(Close == null)
                Close = new Action(WPFError);
            _notificationManager.Show(content, "", onClick: () => Click(), onClose: () => Close());
        }

        /// <summary>
        /// Toast 通知(有显示时间)
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

        private static void WPFError()
        {
            Console.WriteLine("未设置Toast点击或结束时的方法");
        }
    }
}
