using System;
using System.Windows.Forms;
using StudentConsultationScheduler.Data;
using StudentConsultationScheduler.Forms;

namespace StudentConsultationScheduler
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Db.EnsureDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "資料庫初始化失敗。\n\n請確認已安裝 SQL Server Express LocalDB。\n\n錯誤訊息：" + ex.Message,
                    "資料庫錯誤",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}
