using DeliveryService.Models;
using DeliveryService.Services;

namespace DeliveryService.Forms;

public class OrderForm : Form
{
    private readonly User _user;
    private DataGridView grid = new();
    private TextBox txtSearch = new();

    public OrderForm(User user)
    {
        _user = user;
        Text = "Заказы";
        Width = 900; Height = 500;

        var panel = new Panel { Dock = DockStyle.Top, Height = 50 };
        txtSearch.SetBounds(10, 12, 250, 25);
        panel.Controls.Add(txtSearch);

        var btnSearch = new Button { Text = "Поиск", Left = 270, Top = 11, Width = 80 };
        btnSearch.Click += (_, _) => LoadOrders();
        panel.Controls.Add(btnSearch);

        var btnAdd = new Button { Text = "Добавить", Left = 360, Top = 11, Width = 90 };
        btnAdd.Click += BtnAdd_Click;
        panel.Controls.Add(btnAdd);

        var btnUpd = new Button { Text = "Изменить", Left = 460, Top = 11, Width = 90 };
        btnUpd.Click += BtnUpdate_Click;
        panel.Controls.Add(btnUpd);

        var btnDel = new Button { Text = "Удалить", Left = 560, Top = 11, Width = 90 };
        btnDel.Click += BtnDelete_Click;
        panel.Controls.Add(btnDel);

        Controls.Add(panel);

        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoGenerateColumns = true;
        Controls.Add(grid);

        LoadOrders();
    }

    private void LoadOrders()
    {
        grid.DataSource = OrderService.GetAll(txtSearch.Text.Trim());
        if (grid.Columns.Count > 0)
        {
            grid.Columns["Id"].Visible = false;
            grid.Columns["ClientId"].Visible = false;
            grid.Columns["CourierId"].Visible = false;
        }
    }

    private Order? Selected()
    {
        if (grid.CurrentRow?.DataBoundItem is Order o) return o;
        MessageBox.Show("Выберите запись.");
        return null;
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        var f = new OrderEditForm(null);
        if (f.ShowDialog() == DialogResult.OK) LoadOrders();
    }

    private void BtnUpdate_Click(object? sender, EventArgs e)
    {
        var o = Selected(); if (o == null) return;
        var f = new OrderEditForm(o);
        if (f.ShowDialog() == DialogResult.OK) LoadOrders();
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        var o = Selected(); if (o == null) return;
        if (MessageBox.Show("Удалить заказ?", "Подтверждение",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        OrderService.Delete(o.Id);
        if (o.CourierId.HasValue) CourierService.SetBusy(o.CourierId.Value, false);
        LoadOrders();
    }
}