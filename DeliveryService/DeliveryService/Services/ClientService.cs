using DeliveryService.Data;
using DeliveryService.Models;

namespace DeliveryService.Services;

public static class ClientService
{
    public static List<Client> GetAll(string search = "")
    {
        var list = new List<Client>();
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT Id, FullName, Phone, Address FROM Clients
                            WHERE FullName LIKE $q OR Phone LIKE $q OR Address LIKE $q
                            ORDER BY Id DESC";
        cmd.Parameters.AddWithValue("$q", $"%{search}%");
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Client
            {
                Id = r.GetInt32(0),
                FullName = r.GetString(1),
                Phone = r.IsDBNull(2) ? "" : r.GetString(2),
                Address = r.IsDBNull(3) ? "" : r.GetString(3)
            });
        return list;
    }

    public static void Add(Client c)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Clients (FullName, Phone, Address) VALUES ($f,$p,$a)";
        cmd.Parameters.AddWithValue("$f", c.FullName);
        cmd.Parameters.AddWithValue("$p", c.Phone);
        cmd.Parameters.AddWithValue("$a", c.Address);
        cmd.ExecuteNonQuery();
    }

    public static void Update(Client c)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE Clients SET FullName=$f, Phone=$p, Address=$a WHERE Id=$id";
        cmd.Parameters.AddWithValue("$f", c.FullName);
        cmd.Parameters.AddWithValue("$p", c.Phone);
        cmd.Parameters.AddWithValue("$a", c.Address);
        cmd.Parameters.AddWithValue("$id", c.Id);
        cmd.ExecuteNonQuery();
    }

    public static void Delete(int id)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Clients WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
}