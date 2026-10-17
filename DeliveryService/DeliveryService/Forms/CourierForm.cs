using DeliveryService.Models;
using DeliveryService.Services;

namespace DeliveryService.Forms;

public class CourierForm : Form
{
    private DataGridView grid = new();
    private TextBox txtSearch = new();

    public CourierForm()
    {
        Text = "Курьеры";
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
            if (grid.CurrentRow?.DataBoundItem is Courier c) Edit(c);
        };
        panel.Controls.Add(btnUpd);

        var btnDel = new Button { Text = "Удалить", Left = 560, Top = 11, Width = 90 };
        btnDel.Click += (_, _) =>
        {
            if (grid.CurrentRow?.DataBoundItem is Courier c &&
                MessageBox.Show("Удалить курьера?", "Подтверждение",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                CourierService.Delete(c.Id);
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
        grid.DataSource = CourierService.GetAll(txtSearch.Text.Trim());
        if (grid.Columns.Count > 0) grid.Columns["Id"].Visible = false;
    }

    private void Edit(Courier? c)
    {
        using var f = new Form
        {
            Text = c == null ? "Новый курьер" : "Редактирование курьера",
            Width = 400,
            Height = 220,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false
        };

        var txtName = new TextBox { Left = 140, Top = 20, Width = 220 };
        var txtPhone = new TextBox { Left = 140, Top = 60, Width = 220 };

        if (c != null)
        {
            txtName.Text = c.FullName;
            txtPhone.Text = c.Phone;
        }

        f.Controls.Add(new Label { Text = "ФИО:", Left = 20, Top = 23, Width = 110 });
        f.Controls.Add(txtName);
        f.Controls.Add(new Label { Text = "Телефон:", Left = 20, Top = 63, Width = 110 });
        f.Controls.Add(txtPhone);

        var btn = new Button { Text = "Сохранить", Left = 140, Top = 110, Width = 220 };
        btn.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите ФИО."); return;
            }
            var model = c ?? new Courier();
            model.FullName = txtName.Text.Trim();
            model.Phone = txtPhone.Text.Trim();
            if (c == null) CourierService.Add(model);
            else CourierService.Update(model);
            f.DialogResult = DialogResult.OK;
        };
        f.Controls.Add(btn);

        if (f.ShowDialog() == DialogResult.OK) LoadData();
    }
}