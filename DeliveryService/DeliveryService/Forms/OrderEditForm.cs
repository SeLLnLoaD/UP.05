using DeliveryService.Models;
using DeliveryService.Services;

namespace DeliveryService.Forms;

public class OrderEditForm : Form
{
    private readonly Order? _editing;
    private ComboBox cbClient = new();
    private ComboBox cbCourier = new();
    private ComboBox cbStatus = new();
    private TextBox txtAddress = new();
    private NumericUpDown numTotal = new();

    public OrderEditForm(Order? order)
    {
        _editing = order;
        Text = order == null ? "Новый заказ" : $"Заказ №{order.Id}";
        Width = 460; Height = 380;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        int top = 20;
        AddRow("Клиент:", cbClient, ref top);
        AddRow("Курьер:", cbCourier, ref top);
        AddRow("Адрес:", txtAddress, ref top);

        Controls.Add(new Label { Text = "Статус:", Left = 20, Top = top, Width = 130 });
        cbStatus.SetBounds(160, top - 3, 260, 25);
        cbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cbStatus.Items.AddRange(new object[]
        {
            "новый", "в пути", "доставлен", "отменён"
        });
        cbStatus.SelectedIndex = 0;
        Controls.Add(cbStatus);
        top += 40;

        Controls.Add(new Label { Text = "Сумма:", Left = 20, Top = top, Width = 130 });
        numTotal.SetBounds(160, top - 3, 260, 25);
        numTotal.Maximum = 1_000_000;
        numTotal.DecimalPlaces = 2;
        Controls.Add(numTotal);
        top += 50;

        var btnSave = new Button { Text = "Сохранить" };
        btnSave.SetBounds(160, top, 120, 32);
        btnSave.Click += BtnSave_Click;
        Controls.Add(btnSave);

        var btnCancel = new Button { Text = "Отмена" };
        btnCancel.SetBounds(300, top, 120, 32);
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        Controls.Add(btnCancel);

        LoadClients();
        LoadCouriers();

        if (order != null) Fill(order);
    }

    private void AddRow(string label, Control input, ref int top)
    {
        Controls.Add(new Label { Text = label, Left = 20, Top = top, Width = 130 });
        input.SetBounds(160, top - 3, 260, 25);
        Controls.Add(input);
        top += 40;
    }

    private void LoadClients()
    {
        cbClient.DropDownStyle = ComboBoxStyle.DropDownList;
        cbClient.DataSource = ClientService.GetAll();
        cbClient.DisplayMember = "FullName";
        cbClient.ValueMember = "Id";
    }

    private void LoadCouriers()
    {
        var list = new List<object> { new { Id = 0, FullName = "(не назначен)" } };
        list.AddRange(CourierService.GetAll().Select(c => (object)new { c.Id, c.FullName }));
        cbCourier.DropDownStyle = ComboBoxStyle.DropDownList;
        cbCourier.DataSource = list;
        cbCourier.DisplayMember = "FullName";
        cbCourier.ValueMember = "Id";
    }

    private void Fill(Order o)
    {
        cbClient.SelectedValue = o.ClientId;
        cbCourier.SelectedValue = o.CourierId ?? 0;
        txtAddress.Text = o.Address;
        cbStatus.SelectedItem = o.Status;
        numTotal.Value = o.Total;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (cbClient.SelectedValue == null)
        {
            MessageBox.Show("Выберите клиента."); return;
        }
        if (string.IsNullOrWhiteSpace(txtAddress.Text))
        {
            MessageBox.Show("Введите адрес."); return;
        }

        int clientId = (int)cbClient.SelectedValue;
        int courierId = (int)cbCourier.SelectedValue;

        var order = _editing ?? new Order();
        order.ClientId = clientId;
        order.Address = txtAddress.Text.Trim();
        order.Status = cbStatus.SelectedItem?.ToString() ?? "новый";
        order.Total = numTotal.Value;

        // === Индивидуальная модификация: автоназначение курьера ===
        if (courierId == 0 && order.Status == "новый")
        {
            var free = CourierService.FindFree();
            if (free != null)
            {
                courierId = free.Id;
                CourierService.SetBusy(free.Id, true);
                MessageBox.Show($"Автоматически назначен курьер: {free.FullName}");
            }
            else
            {
                MessageBox.Show("Свободных курьеров нет. Заказ сохранён без курьера.");
            }
        }
        order.CourierId = courierId == 0 ? null : courierId;

        if (_editing == null)
            OrderService.Add(order);
        else
            OrderService.Update(order);

        DialogResult = DialogResult.OK;
    }
}