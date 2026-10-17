using DeliveryService.Models;
using DeliveryService.Services;

namespace DeliveryService.Forms;

public class LoginForm : Form
{
    private TextBox txtLogin = new();
    private TextBox txtPassword = new();
    private TextBox txtCaptcha = new();
    private PictureBox picCaptcha = new();
    private Button btnRefresh = new();
    private Button btnLogin = new();
    private Button btnRegister = new();
    private Label lblError = new();

    public LoginForm()
    {
        Text = "Авторизация — Сервис доставки";
        Width = 420; Height = 460;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        Controls.Add(new Label { Text = "Логин:", Left = 30, Top = 30, Width = 100});
        txtLogin.SetBounds(140, 27, 230, 25);
        Controls.Add(txtLogin);

        Controls.Add(new Label { Text = "Пароль:", Left = 30, Top = 70, Width = 100});
        txtPassword.SetBounds(140, 67, 230, 25);
        txtPassword.UseSystemPasswordChar = true;
        Controls.Add(txtPassword);

        Controls.Add(new Label { Text = "CAPTCHA:", Left = 30, Top = 110, Width = 100});
        txtCaptcha.SetBounds(140, 107, 100, 25);
        Controls.Add(txtCaptcha);

        picCaptcha.SetBounds(140, 145, 180, 50);
        picCaptcha.BorderStyle = BorderStyle.FixedSingle;
        Controls.Add(picCaptcha);

        btnRefresh.Text = "↻";
        btnRefresh.SetBounds(325, 145, 45, 50);
        btnRefresh.Click += (_, _) => RefreshCaptcha();
        Controls.Add(btnRefresh);

        btnLogin.Text = "Войти";
        btnLogin.SetBounds(140, 220, 230, 35);
        btnLogin.Click += BtnLogin_Click;
        Controls.Add(btnLogin);

        btnRegister.Text = "Регистрация";
        btnRegister.SetBounds(140, 265, 230, 30);
        btnRegister.Click += (_, _) => new RegisterForm().ShowDialog();
        Controls.Add(btnRegister);

        lblError.SetBounds(30, 310, 340, 80);
        lblError.ForeColor = Color.Firebrick;
        Controls.Add(lblError);

        RefreshCaptcha();
    }

    private void RefreshCaptcha()
    {
        picCaptcha.Image?.Dispose();
        picCaptcha.Image = CaptchaService.Generate();
        var show = CaptchaService.CaptchaRequired;
        picCaptcha.Visible = show;
        txtCaptcha.Visible = show;
        btnRefresh.Visible = show;
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        var result = AuthService.Login(txtLogin.Text.Trim(),
            txtPassword.Text, txtCaptcha.Text.Trim());

        if (!result.Success)
        {
            lblError.Text = result.Message;
            RefreshCaptcha();
            txtCaptcha.Clear();
            return;
        }

        var main = new MainForm(result.User!);
        Hide();
        main.FormClosed += (_, _) => Close();
        main.Show();
    }
}