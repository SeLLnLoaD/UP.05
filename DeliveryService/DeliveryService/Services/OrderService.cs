using DeliveryService.Data;
using DeliveryService.Models;

namespace DeliveryService.Services;

public static class OrderService
{
    public static List<Order> GetAll(string search = "")
    {
        var list = new List<Order>();
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT o.Id, o.ClientId, o.CourierId, c.FullName, IFNULL(k.FullName,''),
                   o.Address, o.Status, o.Total, o.CreatedAt
            FROM Orders o
            JOIN Clients c ON c.Id = o.ClientId
            LEFT JOIN Couriers k ON k.Id = o.CourierId
            WHERE c.FullName LIKE $q OR o.Address LIKE $q OR o.Status LIKE $q
            ORDER BY o.Id DESC";
        cmd.Parameters.AddWithValue("$q", $"%{search}%");
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Order
            {
                Id = r.GetInt32(0),
                ClientId = r.GetInt32(1),
                CourierId = r.IsDBNull(2) ? null : r.GetInt32(2),
                ClientName = r.GetString(3),
                CourierName = r.GetString(4),
                Address = r.IsDBNull(5) ? "" : r.GetString(5),
                Status = r.GetString(6),
                Total = (decimal)r.GetDouble(7),
                CreatedAt = DateTime.Parse(r.GetString(8))
            });
        return list;
    }

    public static List<Order> GetByCourier(int courierUserId)
    {
        // В реальной системе связь Courier <-> User.Для простоты возвращаем все
        return GetAll();
    }

    public static int Add(Order o)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Orders
            (ClientId, CourierId, Address, Status, Total, CreatedAt)
            VALUES ($c,$k,$a,$s,$t,$d);
            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$c", o.ClientId);
        cmd.Parameters.AddWithValue("$k", (object?)o.CourierId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$a", o.Address);
        cmd.Parameters.AddWithValue("$s", o.Status);
        cmd.Parameters.AddWithValue("$t", (double)o.Total);
        cmd.Parameters.AddWithValue("$d", DateTime.Now.ToString("s"));
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static void Update(Order o)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE Orders SET
            ClientId=$c, CourierId=$k, Address=$a, Status=$s, Total=$t
            WHERE Id=$id";
        cmd.Parameters.AddWithValue("$c", o.ClientId);
        cmd.Parameters.AddWithValue("$k", (object?)o.CourierId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$a", o.Address);
        cmd.Parameters.AddWithValue("$s", o.Status);
        cmd.Parameters.AddWithValue("$t", (double)o.Total);
        cmd.Parameters.AddWithValue("$id", o.Id);
        cmd.ExecuteNonQuery();
    }

    public static void Delete(int id)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Orders WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
}