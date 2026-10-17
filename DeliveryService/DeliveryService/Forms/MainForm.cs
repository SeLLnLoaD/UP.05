using DeliveryService.Models;
using DeliveryService.Services;

namespace DeliveryService.Forms;

public class MainForm : Form
{
    private readonly User _user;
    private MenuStrip _menu = new();

    public MainForm(User user)
    {
        _user = user;
        Text = $"Сервис доставки — {user.FullName} ({user.RoleName})";
        Width = 1000; Height = 800;
        StartPosition = FormStartPosition.CenterScreen;
        IsMdiContainer = true;

        BuildMenu();

        var status = new StatusStrip();
        status.Items.Add($"Роль: {user.RoleName}");
        status.Items.Add($"Логин: {user.Login}");
        Controls.Add(status);
    }

    private void BuildMenu()
    {
        var miOrders = new ToolStripMenuItem("Заказы");
        miOrders.Click += (_, _) => OpenChild(new OrderForm(_user));

        var miClients = new ToolStripMenuItem("Клиенты");
        miClients.Click += (_, _) => OpenChild(new ClientForm());

        var miCouriers = new ToolStripMenuItem("Курьеры");
        miCouriers.Click += (_, _) => OpenChild(new CourierForm());

        var miJournal = new ToolStripMenuItem("Журнал входов");
        miJournal.Click += (_, _) => ShowJournal();

        var miExit = new ToolStripMenuItem("Выход");
        miExit.Click += (_, _) => Close();

        _menu.Items.Add(miOrders);
        _menu.Items.Add(miClients);
        _menu.Items.Add(miCouriers);
        _menu.Items.Add(miJournal);
        _menu.Items.Add(miExit);

        Controls.Add(_menu);

        ApplyRolePermissions(miOrders, miClients, miCouriers, miJournal);
    }

    private void ApplyRolePermissions(ToolStripMenuItem orders,
        ToolStripMenuItem clients, ToolStripMenuItem couriers,
        ToolStripMenuItem journal)
    {
        switch (_user.RoleName)
        {
            case "admin":
                break;
            case "operator":
                journal.Visible = false;
                break;
            case "user":
                clients.Visible = false;
                couriers.Visible = false;
                journal.Visible = false;
                orders.Text = "Мои заказы";
                break;
        }
    }

    private void OpenChild(Form f)
    {
        f.MdiParent = this;
        f.Show();
    }

    private void ShowJournal()
    {
        var attempts = AuthService.GetAttempts();
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = true,
            DataSource = attempts
        };
        var f = new Form { Text = "Журнал входов", Width = 700, Height = 400 };
        f.Controls.Add(grid);
        OpenChild(f);
    }
}