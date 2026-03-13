namespace JoiabagurPV.Application.DTOs.Inventory;

public class InventoryMovementSummaryRow
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int Additions { get; set; }
    public int Subtractions { get; set; }
    public int Difference { get; set; }
}
