using DeliveryService.Data;
using DeliveryService.Forms;

namespace DeliveryService;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        DbInitializer.Init();
        Application.Run(new LoginForm());
    }
}