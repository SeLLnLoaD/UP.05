namespace DeliveryService.Models;

public class Order
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int? CourierId { get; set; }
    public string ClientName { get; set; } = "";
    public string CourierName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Status { get; set; } = "новый";
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}