using System;
using System.IO;
using System.Windows.Forms;

namespace AstralPartyModManager
{
    // 应用程序入口
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // 添加全局异常捕获
            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show($"发生错误：{e.Exception.Message}\n\n{e.Exception.StackTrace}", 
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(e.Exception);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    MessageBox.Show($"致命错误：{ex.Message}\n\n{ex.StackTrace}", 
                        "致命错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogError(ex);
                }
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.Run(new MainForm());
        }

        static void LogError(Exception ex)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                File.AppendAllText(logPath, 
                    $"[{DateTime.Now}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n\n");
            }
            catch { }
        }
    }
}
