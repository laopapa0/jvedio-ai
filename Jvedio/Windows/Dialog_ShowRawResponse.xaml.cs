using System.Windows;
using System.Windows.Controls;

namespace Jvedio
{
    public partial class Dialog_ShowRawResponse : BaseDialog
    {
        public Dialog_ShowRawResponse(string rawResponse)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(rawResponse))
            {
                txtRawResponse.Text = rawResponse;
            }
            else
            {
                txtRawResponse.Text = "没有原始返回内容";
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtRawResponse.Text))
            {
                Clipboard.SetText(txtRawResponse.Text);
                MessageNotify.Success("已复制到剪贴板");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
