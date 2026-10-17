using DeliveryService.Data;
using DeliveryService.Models;

namespace DeliveryService.Services;

public static class CourierService
{
    public static List<Courier> GetAll(string search = "")
    {
        var list = new List<Courier>();
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT Id, FullName, Phone, IsBusy FROM Couriers
                            WHERE FullName LIKE $q OR Phone LIKE $q ORDER BY Id DESC";
        cmd.Parameters.AddWithValue("$q", $"%{search}%");
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Courier
            {
                Id = r.GetInt32(0),
                FullName = r.GetString(1),
                Phone = r.IsDBNull(2) ? "" : r.GetString(2),
                IsBusy = r.GetInt32(3) == 1
            });
        return list;
    }

    public static Courier? FindFree()
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, FullName, Phone FROM Couriers WHERE IsBusy = 0 LIMIT 1";
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Courier
        {
            Id = r.GetInt32(0),
            FullName = r.GetString(1),
            Phone = r.IsDBNull(2) ? "" : r.GetString(2)
        };
    }

    public static void SetBusy(int id, bool busy)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Couriers SET IsBusy=$b WHERE Id=$id";
        cmd.Parameters.AddWithValue("$b", busy ? 1 : 0);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public static void Add(Courier c)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Couriers (FullName, Phone, IsBusy) VALUES ($f,$p,0)";
        cmd.Parameters.AddWithValue("$f", c.FullName);
        cmd.Parameters.AddWithValue("$p", c.Phone);
        cmd.ExecuteNonQuery();
    }

    public static void Update(Courier c)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Couriers SET FullName=$f, Phone=$p WHERE Id=$id";
        cmd.Parameters.AddWithValue("$f", c.FullName);
        cmd.Parameters.AddWithValue("$p", c.Phone);
        cmd.Parameters.AddWithValue("$id", c.Id);
        cmd.ExecuteNonQuery();
    }

    public static void Delete(int id)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Couriers WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
}