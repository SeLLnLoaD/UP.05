using DeliveryService.Data;
using DeliveryService.Models;

namespace DeliveryService.Services;

public class LoginResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public User? User { get; set; }

    public static LoginResult Ok(User u) => new() { Success = true, User = u };
    public static LoginResult Fail(string msg) => new() { Success = false, Message = msg };
}

public static class AuthService
{
    private static DateTime _lockUntil = DateTime.MinValue;
    public const int MaxFailures = 3;
    public const int LockSeconds = 60;

    public static LoginResult Login(string login, string password, string captcha)
    {
        if (DateTime.Now < _lockUntil)
            return LoginResult.Fail(
                $"Слишком много неудачных попыток. Вход заблокирован до {_lockUntil:HH:mm:ss}");

        if (CaptchaService.CaptchaRequired && !CaptchaService.Verify(captcha))
        {
            LogAttempt(login, false, "Неверная CAPTCHA");
            CaptchaService.RegisterFailure();
            CheckLock(login);
            return LoginResult.Fail("Неверная CAPTCHA");
        }

        var user = GetUserByLogin(login);
        if (user == null || !user.IsActive)
        {
            LogAttempt(login, false, "Пользователь не найден или заблокирован");
            CaptchaService.RegisterFailure();
            CheckLock(login);
            return LoginResult.Fail("Пользователь не найден или заблокирован");
        }

        if (!PasswordHasher.Verify(password, user.PasswordSalt, user.PasswordHash))
        {
            LogAttempt(login, false, "Неверный пароль");
            CaptchaService.RegisterFailure();
            CheckLock(login);
            return LoginResult.Fail("Неверный пароль");
        }

        LogAttempt(login, true, "Успешный вход");
        CaptchaService.Reset();
        return LoginResult.Ok(user);
    }

    private static void CheckLock(string login)
    {
        if (CaptchaService.Failures >= MaxFailures)
        {
            _lockUntil = DateTime.Now.AddSeconds(LockSeconds);
            LogAttempt(login, false, $"Блокировка на {LockSeconds} сек.");
        }
    }

    public static bool LoginExists(string login)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Login = $l";
        cmd.Parameters.AddWithValue("$l", login);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    public static void Register(string fullName, string login, string password)
    {
        var salt = PasswordHasher.GenerateSalt();
        var hash = PasswordHasher.Hash(password, salt);
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Users
            (FullName, Login, PasswordHash, PasswordSalt, RoleId, IsActive)
            VALUES ($f, $l, $h, $s, 3, 1)";
        cmd.Parameters.AddWithValue("$f", fullName);
        cmd.Parameters.AddWithValue("$l", login);
        cmd.Parameters.AddWithValue("$h", hash);
        cmd.Parameters.AddWithValue("$s", salt);
        cmd.ExecuteNonQuery();
    }

    public static User? GetUserByLogin(string login)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT u.Id, u.FullName, u.Login, u.PasswordHash, u.PasswordSalt,
                                   u.RoleId, u.IsActive, r.Name
                            FROM Users u JOIN Roles r ON r.Id = u.RoleId
                            WHERE u.Login = $l";
        cmd.Parameters.AddWithValue("$l", login);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new User
        {
            Id = r.GetInt32(0),
            FullName = r.GetString(1),
            Login = r.GetString(2),
            PasswordHash = r.GetString(3),
            PasswordSalt = r.GetString(4),
            RoleId = r.GetInt32(5),
            IsActive = r.GetInt32(6) == 1,
            RoleName = r.GetString(7)
        };
    }

    private static void LogAttempt(string login, bool success, string message)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO LoginAttempts
            (UserLogin, IsSuccess, Message, CreatedAt)
            VALUES ($l, $s, $m, $c)";
        cmd.Parameters.AddWithValue("$l", login);
        cmd.Parameters.AddWithValue("$s", success ? 1 : 0);
        cmd.Parameters.AddWithValue("$m", message);
        cmd.Parameters.AddWithValue("$c", DateTime.Now.ToString("s"));
        cmd.ExecuteNonQuery();
    }

    public static List<LoginAttempt> GetAttempts()
    {
        var list = new List<LoginAttempt>();
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, UserLogin, IsSuccess, Message, CreatedAt " +
                          "FROM LoginAttempts ORDER BY Id DESC";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new LoginAttempt
            {
                Id = r.GetInt32(0),
                UserLogin = r.GetString(1),
                IsSuccess = r.GetInt32(2) == 1,
                Message = r.GetString(3),
                CreatedAt = DateTime.Parse(r.GetString(4))
            });
        return list;
    }
}