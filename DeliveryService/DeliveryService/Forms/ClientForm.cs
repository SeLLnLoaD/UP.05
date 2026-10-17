using DeliveryService.Models;
using DeliveryService.Services;

namespace DeliveryService.Forms;

public class ClientForm : Form
{
    private DataGridView grid = new ();
    private TextBox txtSearch = new();

    public ClientForm()
    {
        Text = "Клиенты";
        Width = 800; Height = 480;

        var panel = new Panel { Dock = DockStyle.Top, Height = 50 };
        txtSearch.SetBounds(10, 12, 250, 25);
        panel.Controls.Add(txtSearch);

        var btnSearch = new Button { Text = "Поиск", Left = 270, Top = 11, Width = 80 };
        btnSearch.Click += (_, _) => LoadData();
        panel.Controls.Add(btnSearch);

        var btnAdd = new Button { Text = "Добавить", Left = 360, Top = 11, Width = 90 };
        btnAdd.Click += (_, _) => Edit(null);
        panel.Controls.Add(btnAdd);

        var btnUpd = new Button { Text = "Изменить", Left = 460, Top = 11, Width = 90 };
        btnUpd.Click += (_, _) =>
        {
            if (grid.CurrentRow?.DataBoundItem is Client c) Edit(c);
        };
        panel.Controls.Add(btnUpd);

        var btnDel = new Button { Text = "Удалить", Left = 560, Top = 11, Width = 90 };
        btnDel.Click += (_, _) =>
        {
            if (grid.CurrentRow?.DataBoundItem is Client c &&
                MessageBox.Show("Удалить клиента?", "Подтверждение",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ClientService.Delete(c.Id);
                LoadData();
            }
        };
        panel.Controls.Add(btnDel);

        Controls.Add(panel);

        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        Controls.Add(grid);

        LoadData();
    }

    private void LoadData()
    {
        grid.DataSource = ClientService.GetAll(txtSearch.Text.Trim());
        if (grid.Columns.Count > 0) grid.Columns["Id"].Visible = false;
    }

    private void Edit(Client? c)
    {
        using var f = new Form
        {
            Text = c == null ? "Новый клиент" : "Редактирование клиента",
            Width = 400,
            Height = 250,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false
        };

        var txtName = new TextBox { Left = 140, Top = 20, Width = 220 };
        var txtPhone = new TextBox { Left = 140, Top = 60, Width = 220 };
        var txtAddr = new TextBox { Left = 140, Top = 100, Width = 220 };

        if (c != null)
        {
            txtName.Text = c.FullName;
            txtPhone.Text = c.Phone;
            txtAddr.Text = c.Address;
        }

        f.Controls.Add(new Label { Text = "ФИО:", Left = 20, Top = 23, Width = 110 });
        f.Controls.Add(txtName);
        f.Controls.Add(new Label { Text = "Телефон:", Left = 20, Top = 63, Width = 110 });
        f.Controls.Add(txtPhone);
        f.Controls.Add(new Label { Text = "Адрес:", Left = 20, Top = 103, Width = 110 });
        f.Controls.Add(txtAddr);

        var btn = new Button { Text = "Сохранить", Left = 140, Top = 150, Width = 220 };
        btn.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите ФИО."); return;
            }
            var model = c ?? new Client();
            model.FullName = txtName.Text.Trim();
            model.Phone = txtPhone.Text.Trim();
            model.Address = txtAddr.Text.Trim();
            if (c == null) ClientService.Add(model);
            else ClientService.Update(model);
            f.DialogResult = DialogResult.OK;
        };
        f.Controls.Add(btn);

        if (f.ShowDialog() == DialogResult.OK) LoadData();
    }
}