using DeliveryService.Services;
using Microsoft.Data.Sqlite;

namespace DeliveryService.Data;

public static class DbInitializer
{
    public static void Init()
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Roles (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE
);
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Login TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    PasswordSalt TEXT NOT NULL,
    RoleId INTEGER NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
CREATE TABLE IF NOT EXISTS LoginAttempts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserLogin TEXT NOT NULL,
    IsSuccess INTEGER NOT NULL,
    Message TEXT,
    CreatedAt TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS Clients (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Phone TEXT,
    Address TEXT
);
CREATE TABLE IF NOT EXISTS Couriers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Phone TEXT,
    IsBusy INTEGER NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS Orders (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ClientId INTEGER NOT NULL,
    CourierId INTEGER,
    Address TEXT,
    Status TEXT NOT NULL DEFAULT 'новый',
    Total REAL NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (ClientId) REFERENCES Clients(Id),
    FOREIGN KEY (CourierId) REFERENCES Couriers(Id)
);";
        cmd.ExecuteNonQuery();

        SeedRolesAndUsers(conn);
    }

    private static void SeedRolesAndUsers(SqliteConnection conn)
    {
        using (var check = conn.CreateCommand())
        {
            check.CommandText = "SELECT COUNT(*) FROM Roles";
            if (Convert.ToInt64(check.ExecuteScalar()) > 0) return;
        }

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Roles (Name) VALUES ('admin'), ('operator'), ('user');";
        cmd.ExecuteNonQuery();

        CreateUser(conn, "Администратор", "admin", "admin123", 1);
        CreateUser(conn, "Диспетчер", "operator", "operator123", 2);
        CreateUser(conn, "Курьер", "user", "user123", 3);

        using var c2 = conn.CreateCommand();
        c2.CommandText = @"
INSERT INTO Clients (FullName, Phone, Address) VALUES
 ('Иванов И.И.', '+79001112233', 'ул. Ленина, 1'),
 ('Петров П.П.', '+79002223344', 'ул. Мира, 5');
INSERT INTO Couriers (FullName, Phone, IsBusy) VALUES
 ('Сидоров С.С.', '+79003334455', 0),
 ('Кузнецов К.К.', '+79004445566', 0);";
        c2.ExecuteNonQuery();
    }

    private static void CreateUser(SqliteConnection conn, string fullName,
        string login, string password, int roleId)
    {
        var salt = PasswordHasher.GenerateSalt();
        var hash = PasswordHasher.Hash(password, salt);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Users
            (FullName, Login, PasswordHash, PasswordSalt, RoleId, IsActive)
            VALUES ($f, $l, $h, $s, $r, 1)";
        cmd.Parameters.AddWithValue("$f", fullName);
        cmd.Parameters.AddWithValue("$l", login);
        cmd.Parameters.AddWithValue("$h", hash);
        cmd.Parameters.AddWithValue("$s", salt);
        cmd.Parameters.AddWithValue("$r", roleId);
        cmd.ExecuteNonQuery();
    }
}