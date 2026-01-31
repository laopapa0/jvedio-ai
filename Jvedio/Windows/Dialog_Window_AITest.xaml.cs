using Jvedio.Core.AI;
using System.Windows;
using System.Windows.Controls;

namespace Jvedio
{
    /// <summary>
    /// Dialog_Window_AITest.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_Window_AITest : BaseDialog
    {
        private string _diagnosticResult = "";

        public Dialog_Window_AITest()
        {
            InitializeComponent();
        }

        private async void RunDiagnostics_Click(object sender, RoutedEventArgs e)
        {
            btnRunDiagnostics.IsEnabled = false;
            btnRunDiagnostics.Content = "诊断中...";

            try
            {
                txtResult.Text = "正在运行诊断，请稍候...";
                _diagnosticResult = await AITestTool.RunDiagnostics();
                txtResult.Text = _diagnosticResult;
            }
            catch (System.Exception ex)
            {
                txtResult.Text = $"诊断失败：{ex.Message}\n\n{ex.StackTrace}";
            }
            finally
            {
                btnRunDiagnostics.IsEnabled = true;
                btnRunDiagnostics.Content = "运行诊断";
            }
        }

        private void CopyResult_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_diagnosticResult))
            {
                Clipboard.SetText(_diagnosticResult);
                MessageNotify.Success("诊断结果已复制到剪贴板");
            }
            else
            {
                MessageNotify.Warn("没有可复制的结果");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
