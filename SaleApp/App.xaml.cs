using System.Windows;
using SaleApp.Infrastructure;

namespace SaleApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize DB (fix placeholder hashes)
        var (ok, msg) = DatabaseInitializer.Initialize();
        if (!ok)
        {
            System.IO.File.WriteAllText("db_init_error.txt", msg);
            MessageBox.Show(
                $"Không thể kết nối database.\n\n{msg}\n\nVui lòng kiểm tra App.config và SQL Server.",
                "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        // Show Login
        var login = new Views.LoginWindow();
        login.Show();
    }
}
