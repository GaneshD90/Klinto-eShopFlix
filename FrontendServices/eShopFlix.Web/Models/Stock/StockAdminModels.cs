namespace eShopFlix.Web.Models.Stock;

// ============ Admin Warehouse Models ============

public class WarehouseModel
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public int? Capacity { get; set; }
    public string? ContactDetails { get; set; }
    public string? OperatingHours { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string StatusBadgeClass => IsActive ? "bg-success" : "bg-secondary";
    public string StatusText => IsActive ? "Active" : "Inactive";
    public string TypeBadgeClass => Type switch
    {
        "DropShip" => "bg-info text-dark",
        "Consignment" => "bg-warning text-dark",
        _ => "bg-primary"
    };
}

// ============ Admin Stock Item Models ============

public class StockItemModel
{
    public Guid StockItemId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariationId { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string? Sku { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int InTransitQuantity { get; set; }
    public int DamagedQuantity { get; set; }
    public int? MinimumStockLevel { get; set; }
    public int? MaximumStockLevel { get; set; }
    public int? ReorderQuantity { get; set; }
    public decimal? UnitCost { get; set; }
    public DateTime? LastRestockedAt { get; set; }
    public string? BatchNumber { get; set; }
    public string? BinLocation { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public int TotalQuantity => AvailableQuantity + ReservedQuantity + InTransitQuantity;
    public bool IsLowStock => MinimumStockLevel.HasValue && AvailableQuantity <= MinimumStockLevel.Value;
    public bool IsOverStock => MaximumStockLevel.HasValue && AvailableQuantity >= MaximumStockLevel.Value;
    public decimal? TotalValue => UnitCost.HasValue ? UnitCost.Value * AvailableQuantity : null;

    public string StockStatusBadgeClass => IsLowStock
        ? "bg-danger"
        : IsOverStock
            ? "bg-warning text-dark"
            : "bg-success";

    public string StockStatusText => IsLowStock
        ? "Low Stock"
        : IsOverStock
            ? "Over Stock"
            : "Normal";
}

// ============ Alert Model ============

public class StockAlertModel
{
    public Guid AlertId { get; set; }
    public Guid StockItemId { get; set; }
    public Guid? ProductId { get; set; }
    public string? Sku { get; set; }
    public string? WarehouseName { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string AlertStatus { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime TriggeredAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public string AlertBadgeClass => AlertType switch
    {
        "LowStock" => "bg-danger",
        "OverStock" => "bg-warning text-dark",
        "Expiry" => "bg-info text-dark",
        _ => "bg-secondary"
    };

    public string StatusBadgeClass => AlertStatus switch
    {
        "Active" => "bg-danger",
        "Acknowledged" => "bg-warning text-dark",
        "Resolved" => "bg-success",
        _ => "bg-secondary"
    };
}

// ============ Low Stock Report Model ============

public class LowStockReportModel
{
    public Guid StockItemId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public int AvailableQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public int ShortfallQuantity { get; set; }

    public string SeverityClass => ShortfallQuantity switch
    {
        > 50 => "table-danger",
        > 20 => "table-warning",
        _ => "table-light"
    };
}

// ============ Stock Adjustment Result ============

public class StockAdjustmentResultModel
{
    public Guid AdjustmentId { get; set; }
    public Guid StockItemId { get; set; }
    public string? AdjustmentType { get; set; }
    public int AdjustmentQuantity { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public string? Reason { get; set; }
}
