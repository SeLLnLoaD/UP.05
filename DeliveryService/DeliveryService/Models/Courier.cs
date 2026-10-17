namespace DeliveryService.Models;

public class Courier
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public bool IsBusy { get; set; }
}