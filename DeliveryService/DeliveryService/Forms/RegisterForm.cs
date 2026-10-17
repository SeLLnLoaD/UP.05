using DeliveryService.Services;

namespace DeliveryService.Forms;

public class RegisterForm : Form
{
    private TextBox txtName = new();
    private TextBox txtLogin = new();
    private TextBox txtPass = new();
    private TextBox txtConfirm = new();
    private Label lblError = new();

    public RegisterForm()
    {
        Text = "Регистрация";
        Width = 400; Height = 320;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        int top = 20;
        Controls.Add(new Label { Text = "ФИО:", Left = 20, Top = top, Width = 130 });
        txtName.SetBounds(160, top - 3, 200, 25);
        Controls.Add(txtName);
        top += 40;

        Controls.Add(new Label { Text = "Логин:", Left = 20, Top = top, Width = 130 });
        txtLogin.SetBounds(160, top - 3, 200, 25);
        Controls.Add(txtLogin);
        top += 40;

        Controls.Add(new Label { Text = "Пароль (от 6):", Left = 20, Top = top, Width = 130 });
        txtPass.SetBounds(160, top - 3, 200, 25);
        txtPass.UseSystemPasswordChar = true;
        Controls.Add(txtPass);
        top += 40;

        Controls.Add(new Label { Text = "Подтверждение:", Left = 20, Top = top, Width = 130 });
        txtConfirm.SetBounds(160, top - 3, 200, 25);
        txtConfirm.UseSystemPasswordChar = true;
        Controls.Add(txtConfirm);
        top += 45;

        var btn = new Button { Text = "Зарегистрироваться" };
        btn.SetBounds(20, top, 340, 32);
        btn.Click += BtnRegister_Click;
        Controls.Add(btn);

        lblError.SetBounds(20, top + 40, 340, 60);
        lblError.ForeColor = Color.Firebrick;
        Controls.Add(lblError);
    }

    private void BtnRegister_Click(object? sender, EventArgs e)
    {
        var name = txtName.Text.Trim();
        var login = txtLogin.Text.Trim();
        var pass = txtPass.Text;
        var confirm = txtConfirm.Text;

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(login) ||
            string.IsNullOrWhiteSpace(pass))
        {
            lblError.Text = "Заполните все поля.";
            return;
        }
        if (pass.Length < 6)
        {
            lblError.Text = "Пароль должен содержать минимум 6 символов.";
            return;
        }
        if (pass != confirm)
        {
            lblError.Text = "Пароли не совпадают.";
            return;
        }
        if (AuthService.LoginExists(login))
        {
            lblError.Text = "Такой логин уже занят.";
            return;
        }

        AuthService.Register(name, login, pass);
        MessageBox.Show("Пользователь успешно создан.", "Готово",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }

    private void InitializeComponent()
    {

    }
}