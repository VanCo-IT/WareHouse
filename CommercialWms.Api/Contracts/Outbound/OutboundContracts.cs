namespace CommercialWms.Api.Contracts.Outbound;

public sealed record OutboundPageResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record OutboundOptionsResponse(
    IReadOnlyList<OutboundCustomerOptionResponse> Customers,
    IReadOnlyList<OutboundCurrencyOptionResponse> Currencies,
    IReadOnlyList<OutboundProductOptionResponse> Products,
    IReadOnlyList<OutboundWarehouseOptionResponse> Warehouses,
    IReadOnlyList<OutboundEmployeeOptionResponse> Employees);

public sealed record OutboundCustomerOptionResponse(
    int CustomerId,
    string CustomerCode,
    string CustomerName,
    IReadOnlyList<OutboundAddressOptionResponse> Addresses);

public sealed record OutboundAddressOptionResponse(
    int AddressId,
    string AddressLine,
    string? Province,
    string? District,
    bool IsDefault);

public sealed record OutboundCurrencyOptionResponse(int CurrencyId, string CurrencyCode, string CurrencyName);

public sealed record OutboundProductOptionResponse(
    int ProductId,
    string Sku,
    string ProductName,
    IReadOnlyList<OutboundUnitOptionResponse> Units);

public sealed record OutboundUnitOptionResponse(
    int UnitId,
    string UnitCode,
    string UnitName,
    decimal ConversionToBase,
    bool IsBaseUnit);

public sealed record OutboundWarehouseOptionResponse(int WarehouseId, string WarehouseCode, string WarehouseName);

public sealed record OutboundEmployeeOptionResponse(int EmployeeId, string EmployeeCode, string FullName);

public sealed record OutboundAvailableStockResponse(
    long LotId,
    string LotCode,
    DateOnly? ExpiryDate,
    int BinId,
    string BinCode,
    string LocationCode,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    decimal QuantityAvailableInOrderUnit);

public sealed record OutboundOrderListItemResponse(
    long SalesOrderId,
    string SalesOrderNumber,
    int CustomerId,
    string CustomerCode,
    string CustomerName,
    DateTime OrderDate,
    int CurrencyId,
    string CurrencyCode,
    decimal ExchangeRate,
    string Status,
    int LineCount,
    int DeliveryCount);

public sealed record OutboundOrderResponse(
    long SalesOrderId,
    string SalesOrderNumber,
    int CustomerId,
    string CustomerCode,
    string CustomerName,
    DateTime OrderDate,
    int CurrencyId,
    string CurrencyCode,
    decimal ExchangeRate,
    string Status,
    int? CreatedByEmployeeId,
    string? Note,
    IReadOnlyList<OutboundOrderLineResponse> Lines,
    IReadOnlyList<OutboundDeliveryListItemResponse> Deliveries);

public sealed record OutboundOrderLineResponse(
    long SalesOrderDetailId,
    int ProductId,
    string Sku,
    string ProductName,
    decimal Quantity,
    int UnitId,
    string UnitCode,
    decimal ConversionToBase,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal QuantityDelivered,
    IReadOnlyList<OutboundReservationResponse> ActiveReservations);

public sealed record OutboundReservationResponse(
    long ReservationId,
    long LotId,
    string LotCode,
    int BinId,
    string BinCode,
    decimal ReservedQuantity,
    decimal ReservedQuantityInOrderUnit);

public sealed record OutboundDeliveryListItemResponse(
    long DeliveryId,
    string DeliveryNumber,
    int WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    int? DeliveryAddressId,
    DateTime? PlannedAt,
    DateTime? ShippedAt,
    string Status,
    int LineCount);

public sealed record OutboundDeliveryResponse(
    long DeliveryId,
    string DeliveryNumber,
    long SalesOrderId,
    string SalesOrderNumber,
    string CustomerName,
    int WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    int? DeliveryAddressId,
    DateTime? PlannedAt,
    DateTime? ShippedAt,
    string Status,
    IReadOnlyList<OutboundDeliveryLineResponse> Lines);

public sealed record OutboundDeliveryLineResponse(
    long DeliveryDetailId,
    long SalesOrderDetailId,
    int ProductId,
    string Sku,
    string ProductName,
    long LotId,
    string LotCode,
    decimal Quantity,
    string UnitCode);

public sealed record CreateOutboundOrderRequest(
    int CustomerId,
    int CurrencyId,
    decimal ExchangeRate,
    int? CreatedByEmployeeId,
    string? Note,
    IReadOnlyList<CreateOutboundOrderLineRequest> Lines);

public sealed record CreateOutboundOrderLineRequest(
    int ProductId,
    int UnitId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount);

public sealed record ConfirmOutboundOrderRequest(
    int WarehouseId,
    int? DeliveryAddressId,
    IReadOnlyList<OutboundAllocationRequest> Allocations);

public sealed record OutboundAllocationRequest(
    long SalesOrderDetailId,
    long LotId,
    int BinId,
    decimal Quantity);

public sealed record ShipOutboundDeliveryRequest(int? ShippedByEmployeeId);
