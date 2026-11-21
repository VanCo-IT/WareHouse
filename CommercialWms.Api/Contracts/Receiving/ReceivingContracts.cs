using System.ComponentModel.DataAnnotations;

namespace CommercialWms.Api.Contracts.Receiving;

/// <summary>One product unit configured for a product.</summary>
public sealed record ProductUnitOptionResponse(int UnitId, string UnitCode, string UnitName, decimal ConversionToBase, bool IsBaseUnit);

/// <summary>An active warehouse bin available for a receipt put-away.</summary>
public sealed record ReceivingBinOptionResponse(int BinId, string BinCode, string ShelfCode, string RackCode, string AisleCode, string ZoneCode, string WarehouseCode);

/// <summary>A purchase-order line item supplied when creating a purchase order.</summary>
public sealed record CreatePurchaseOrderLineRequest
{
    /// <summary>Product to purchase.</summary>
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    /// <summary>Ordered quantity in the selected product unit.</summary>
    [Range(typeof(decimal), "0.001", "999999999999999.999")]
    public decimal OrderedQuantity { get; init; }

    /// <summary>Unit configured for the product.</summary>
    [Range(1, int.MaxValue)]
    public int UnitId { get; init; }

    /// <summary>Unit price in the purchase order currency.</summary>
    [Range(typeof(decimal), "0", "99999999999999.9999")]
    public decimal UnitPrice { get; init; }
}

/// <summary>Payload for creating a draft purchase order.</summary>
public sealed record CreatePurchaseOrderRequest
{
    /// <summary>Supplier for the order.</summary>
    [Range(1, int.MaxValue)]
    public int SupplierId { get; init; }

    /// <summary>Currency used for line prices.</summary>
    [Range(1, int.MaxValue)]
    public int CurrencyId { get; init; }

    /// <summary>Exchange rate captured for this order.</summary>
    [Range(typeof(decimal), "0.000001", "999999999999.999999")]
    public decimal ExchangeRate { get; init; }

    /// <summary>Order date and offset.</summary>
    public DateTimeOffset OrderDate { get; init; }

    /// <summary>Optional expected delivery date.</summary>
    public DateOnly? ExpectedDate { get; init; }

    /// <summary>Optional employee creating the order.</summary>
    [Range(1, int.MaxValue)]
    public int? CreatedByEmployeeId { get; init; }

    /// <summary>Optional order note.</summary>
    [StringLength(500)]
    public string? Note { get; init; }

    /// <summary>At least one product line.</summary>
    [Required, MinLength(1)]
    public required IReadOnlyList<CreatePurchaseOrderLineRequest> Lines { get; init; }
}

/// <summary>A purchase-order line with product, unit, price, and receiving totals.</summary>
public sealed record PurchaseOrderLineResponse(
    long PurchaseOrderDetailId,
    int ProductId,
    string SKU,
    string ProductName,
    int UnitId,
    string UnitCode,
    string UnitName,
    decimal OrderedQuantity,
    decimal PreviouslyAcceptedQuantity,
    decimal UnitPrice);

/// <summary>A purchase order and its line items.</summary>
public sealed record PurchaseOrderResponse(
    long PurchaseOrderId,
    string PONumber,
    int SupplierId,
    string SupplierCode,
    string SupplierName,
    int CurrencyId,
    string CurrencyCode,
    string CurrencyName,
    DateTimeOffset OrderDate,
    DateOnly? ExpectedDate,
    decimal ExchangeRate,
    string Status,
    int? CreatedByEmployeeId,
    string? Note,
    IReadOnlyList<PurchaseOrderLineResponse> Lines);

/// <summary>A list summary for a goods receipt.</summary>
public sealed record GoodsReceiptListItemResponse(
    long GoodsReceiptId,
    string ReceiptNumber,
    long PurchaseOrderId,
    string PONumber,
    string SupplierName,
    int WarehouseId,
    string WarehouseName,
    DateTimeOffset ReceivedAt,
    string Status,
    int LineCount,
    decimal ReceivedQuantity,
    decimal AcceptedQuantity,
    decimal RejectedQuantity);

/// <summary>A goods-receipt detail line with ordered and cumulative accepted quantities.</summary>
public sealed record GoodsReceiptDetailResponse(
    long GoodsReceiptDetailId,
    long PurchaseOrderDetailId,
    int ProductId,
    string SKU,
    string ProductName,
    int UnitId,
    string UnitCode,
    string UnitName,
    decimal OrderedQuantity,
    decimal PreviouslyAcceptedQuantity,
    decimal UnitPrice,
    decimal ReceivedQuantity,
    decimal AcceptedQuantity,
    decimal RejectedQuantity);

/// <summary>A quality check recorded for a receipt detail.</summary>
public sealed record QualityInspectionCheckResponse(
    long QualityInspectionDetailId,
    long GoodsReceiptDetailId,
    string CheckName,
    string Result,
    string? Note);

/// <summary>The quality-inspection result and its checks.</summary>
public sealed record QualityInspectionResponse(
    long QualityInspectionId,
    string InspectionNumber,
    DateTimeOffset InspectedAt,
    int? InspectorEmployeeId,
    string Result,
    string? Note,
    IReadOnlyList<QualityInspectionCheckResponse> Checks);

/// <summary>A goods receipt with its order, lines, and optional quality inspection.</summary>
public sealed record GoodsReceiptResponse(
    long GoodsReceiptId,
    string ReceiptNumber,
    long PurchaseOrderId,
    string PONumber,
    string CurrencyCode,
    int SupplierId,
    string SupplierName,
    int WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    DateTimeOffset ReceivedAt,
    int? ReceivedByEmployeeId,
    string Status,
    string? Note,
    IReadOnlyList<GoodsReceiptDetailResponse> Lines,
    QualityInspectionResponse? QualityInspection);

/// <summary>Payload for opening a draft receipt against an approved purchase order.</summary>
public sealed record CreateGoodsReceiptRequest
{
    /// <summary>Approved or partially received purchase order.</summary>
    [Range(1, long.MaxValue)]
    public long PurchaseOrderId { get; init; }

    /// <summary>Warehouse that physically receives the goods.</summary>
    [Range(1, int.MaxValue)]
    public int WarehouseId { get; init; }

    /// <summary>Optional receiving employee.</summary>
    [Range(1, int.MaxValue)]
    public int? ReceivedByEmployeeId { get; init; }

    /// <summary>Optional receipt note.</summary>
    [StringLength(500)]
    public string? Note { get; init; }
}

/// <summary>Received, accepted, and rejected quantities for one receipt line.</summary>
public sealed record GoodsReceiptCheckInLineRequest
{
    /// <summary>Receipt detail to update.</summary>
    [Range(1, long.MaxValue)]
    public long GoodsReceiptDetailId { get; init; }

    /// <summary>Physical quantity received in the purchase-order unit.</summary>
    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal ReceivedQuantity { get; init; }

    /// <summary>Quantity accepted for quality review in the purchase-order unit.</summary>
    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal AcceptedQuantity { get; init; }

    /// <summary>Quantity rejected in the purchase-order unit.</summary>
    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal RejectedQuantity { get; init; }
}

/// <summary>Payload for saving the physical check-in quantities of a receipt.</summary>
public sealed record GoodsReceiptCheckInRequest
{
    /// <summary>All receipt lines and the quantities counted for each.</summary>
    [Required, MinLength(1)]
    public required IReadOnlyList<GoodsReceiptCheckInLineRequest> Lines { get; init; }
}

/// <summary>A quality-inspection check supplied by the inspector.</summary>
public sealed record SaveQualityInspectionCheckRequest
{
    /// <summary>Receipt detail being inspected.</summary>
    [Range(1, long.MaxValue)]
    public long GoodsReceiptDetailId { get; init; }

    /// <summary>Inspector-defined check name.</summary>
    [Required, StringLength(120, MinimumLength = 1)]
    public required string CheckName { get; init; }

    /// <summary>Allowed values: Pass, Fail, Pending, NotApplicable.</summary>
    [Required, RegularExpression("^(Pass|Fail|Pending|NotApplicable)$")]
    public required string Result { get; init; }

    /// <summary>Optional check note.</summary>
    [StringLength(300)]
    public string? Note { get; init; }
}

/// <summary>Payload for creating or updating the quality inspection of a receipt.</summary>
public sealed record SaveQualityInspectionRequest
{
    /// <summary>Optional employee who performed the inspection.</summary>
    [Range(1, int.MaxValue)]
    public int? InspectorEmployeeId { get; init; }

    /// <summary>Optional inspection note.</summary>
    [StringLength(500)]
    public string? Note { get; init; }

    /// <summary>At least one check defined for the receipt.</summary>
    [Required, MinLength(1)]
    public required IReadOnlyList<SaveQualityInspectionCheckRequest> Checks { get; init; }
}

/// <summary>One accepted quantity allocated to a bin and an inventory lot.</summary>
public sealed record GoodsReceiptPutawayItemRequest
{
    /// <summary>Receipt detail whose accepted quantity is being put away.</summary>
    [Range(1, long.MaxValue)]
    public long GoodsReceiptDetailId { get; init; }

    /// <summary>Active bin in the receipt warehouse.</summary>
    [Range(1, int.MaxValue)]
    public int BinId { get; init; }

    /// <summary>Quantity allocated in the receipt line's unit.</summary>
    [Range(typeof(decimal), "0.001", "999999999999999.999")]
    public decimal Quantity { get; init; }

    /// <summary>Inventory lot code.</summary>
    [Required, StringLength(60, MinimumLength = 1)]
    public required string LotCode { get; init; }

    /// <summary>Manufacturer recorded for this received lot.</summary>
    [Range(1, int.MaxValue)]
    public int ManufacturerId { get; init; }

    /// <summary>Country of origin recorded for this received lot.</summary>
    [Range(1, int.MaxValue)]
    public int CountryId { get; init; }

    /// <summary>Optional manufacture date.</summary>
    public DateOnly? ManufactureDate { get; init; }

    /// <summary>Optional expiry date.</summary>
    public DateOnly? ExpiryDate { get; init; }

    /// <summary>Optional supplier's lot code.</summary>
    [StringLength(60)]
    public string? SupplierLotCode { get; init; }
}

/// <summary>Payload for finalizing inspection and applying accepted quantities to stock.</summary>
public sealed record ConfirmGoodsReceiptRequest
{
    /// <summary>Put-away allocations. Required when accepted quantity is greater than zero.</summary>
    [Required]
    public required IReadOnlyList<GoodsReceiptPutawayItemRequest> Putaway { get; init; }
}
