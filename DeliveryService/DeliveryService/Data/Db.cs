using Microsoft.Data.Sqlite;

namespace DeliveryService.Data;

public static class Db
{
    public const string ConnectionString = "Data Source=delivery.db";

    public static SqliteConnection Open()
    {
        var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        return conn;
    }
}