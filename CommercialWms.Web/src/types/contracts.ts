export interface CategoryResponse {
  categoryId: number
  categoryCode: string
  categoryName: string
  parentCategoryId: number | null
  isActive: boolean
}

export interface CategoryCreateRequest {
  categoryCode: string
  categoryName: string
  parentCategoryId: number | null
  isActive: boolean | null
}

export interface CategoryUpdateRequest {
  categoryCode: string
  categoryName: string
  parentCategoryId: number | null
  isActive: boolean
}

export interface BrandResponse {
  brandId: number
  brandName: string
  countryId: number
  isActive: boolean
}

export interface BrandCreateRequest {
  brandName: string
  countryId: number
  isActive: boolean | null
}

export interface BrandUpdateRequest {
  brandName: string
  countryId: number
  isActive: boolean
}

export interface CountryResponse {
  countryId: number
  countryCode: string
  countryName: string
  isActive: boolean
}

export interface CountryCreateRequest {
  countryCode: string
  countryName: string
  isActive: boolean | null
}

export interface CountryUpdateRequest {
  countryCode: string
  countryName: string
  isActive: boolean
}

export interface CurrencyResponse {
  currencyId: number
  currencyCode: string
  currencyName: string
  isActive: boolean
}

export interface CurrencyCreateRequest {
  currencyCode: string
  currencyName: string
  isActive: boolean | null
}

export interface CurrencyUpdateRequest {
  currencyCode: string
  currencyName: string
  isActive: boolean
}

export interface UnitResponse {
  unitId: number
  unitCode: string
  unitName: string
  isActive: boolean
}

export interface UnitCreateRequest {
  unitCode: string
  unitName: string
  isActive: boolean | null
}

export interface UnitUpdateRequest {
  unitCode: string
  unitName: string
  isActive: boolean
}

export interface ManufacturerResponse {
  manufacturerId: number
  manufacturerCode: string
  manufacturerName: string
  countryId: number
  address: string | null
  isActive: boolean
}

export interface ManufacturerCreateRequest {
  manufacturerCode: string
  manufacturerName: string
  countryId: number
  address: string | null
  isActive: boolean | null
}

export interface ManufacturerUpdateRequest {
  manufacturerCode: string
  manufacturerName: string
  countryId: number
  address: string | null
  isActive: boolean
}

export interface ProductResponse {
  productId: number
  sku: string
  productName: string
  categoryId: number
  unitId: number | null
  brandId: number | null
  barcode: string | null
  description: string | null
  isPerishable: boolean
  isActive: boolean
  createdAt: string
}

export interface ProductCreateRequest {
  sku: string
  productName: string
  categoryId: number
  unitId: number
  brandId: number | null
  barcode: string | null
  description: string | null
  isPerishable: boolean | null
  isActive: boolean | null
}

export interface ProductUpdateRequest {
  sku: string
  productName: string
  categoryId: number
  unitId: number
  brandId: number | null
  barcode: string | null
  description: string | null
  isPerishable: boolean
  isActive: boolean
}

export interface SupplierResponse {
  supplierId: number
  supplierCode: string
  supplierName: string
  countryId: number
  taxCode: string | null
  email: string | null
  phone: string | null
  address: string | null
  isActive: boolean
}

export interface SupplierCreateRequest {
  supplierCode: string
  supplierName: string
  countryId: number
  taxCode: string | null
  email: string | null
  phone: string | null
  address: string | null
  isActive: boolean | null
}

export interface SupplierUpdateRequest {
  supplierCode: string
  supplierName: string
  countryId: number
  taxCode: string | null
  email: string | null
  phone: string | null
  address: string | null
  isActive: boolean
}

export interface SupplierStatusUpdateRequest {
  isActive: boolean
}

export interface SupplierProductResponse {
  supplierProductId: number
  supplierId: number
  productId: number
  productSKU: string
  productName: string
  manufacturerId: number
  manufacturerName: string
  supplierProductCode: string | null
  leadTimeDays: number
  isActive: boolean
}

export interface SupplierProductCreateRequest {
  productId: number
  manufacturerId: number
  supplierProductCode: string | null
  leadTimeDays: number
  isActive: boolean | null
}

export interface SupplierProductUpdateRequest {
  supplierProductCode: string | null
  leadTimeDays: number
  isActive: boolean
}

export interface SupplierContactResponse {
  contactId: number
  supplierId: number
  contactName: string
  jobTitle: string | null
  email: string | null
  phone: string | null
}

export interface SupplierContactCreateRequest {
  contactName: string
  jobTitle: string | null
  email: string | null
  phone: string | null
}

export interface SupplierContactUpdateRequest {
  contactName: string
  jobTitle: string | null
  email: string | null
  phone: string | null
}

export interface WarehouseResponse {
  warehouseId: number
  warehouseCode: string
  warehouseName: string
  address: string
  isActive: boolean
}

export interface WarehouseCreateRequest {
  warehouseCode: string
  warehouseName: string
  address: string
  isActive: boolean | null
}

export interface WarehouseUpdateRequest {
  warehouseCode: string
  warehouseName: string
  address: string
  isActive: boolean
}

export interface WarehouseBinResponse {
  binId: number
  shelfId: number
  binCode: string
  capacity: number | null
  isActive: boolean
}

export interface WarehouseShelfNodeResponse {
  shelfId: number
  shelfCode: string
  bins: WarehouseBinResponse[]
}

export interface WarehouseRackNodeResponse {
  rackId: number
  rackCode: string
  shelves: WarehouseShelfNodeResponse[]
}

export interface WarehouseAisleNodeResponse {
  aisleId: number
  aisleCode: string
  racks: WarehouseRackNodeResponse[]
}

export interface WarehouseZoneNodeResponse {
  zoneId: number
  zoneCode: string
  zoneName: string
  aisles: WarehouseAisleNodeResponse[]
}

export interface WarehouseStructureResponse {
  warehouse: WarehouseResponse
  zones: WarehouseZoneNodeResponse[]
}

export interface WarehouseZoneResponse {
  zoneId: number
  warehouseId: number
  zoneCode: string
  zoneName: string
}

export interface WarehouseAisleResponse {
  aisleId: number
  zoneId: number
  aisleCode: string
}

export interface WarehouseRackResponse {
  rackId: number
  aisleId: number
  rackCode: string
}

export interface WarehouseShelfResponse {
  shelfId: number
  rackId: number
  shelfCode: string
}

export interface WarehouseZoneCreateRequest {
  warehouseId: number
  zoneCode: string
  zoneName: string
}

export type WarehouseZoneUpdateRequest = WarehouseZoneCreateRequest

export interface WarehouseAisleCreateRequest {
  zoneId: number
  aisleCode: string
}

export type WarehouseAisleUpdateRequest = WarehouseAisleCreateRequest

export interface WarehouseRackCreateRequest {
  aisleId: number
  rackCode: string
}

export type WarehouseRackUpdateRequest = WarehouseRackCreateRequest

export interface WarehouseShelfCreateRequest {
  rackId: number
  shelfCode: string
}

export type WarehouseShelfUpdateRequest = WarehouseShelfCreateRequest

export interface WarehouseBinCreateRequest {
  shelfId: number
  binCode: string
  capacity: number | null
  isActive: boolean | null
}

export interface WarehouseBinUpdateRequest {
  shelfId: number
  binCode: string
  capacity: number | null
  isActive: boolean
}

export interface ProductUnitOptionResponse {
  unitId: number
  unitCode: string
  unitName: string
  conversionToBase: number
  isBaseUnit: boolean
}

export interface ReceivingBinOptionResponse {
  binId: number
  binCode: string
  shelfCode: string
  rackCode: string
  aisleCode: string
  zoneCode: string
  warehouseCode: string
}

export interface CreatePurchaseOrderLineRequest {
  productId: number
  orderedQuantity: number
  unitId: number
  unitPrice: number
}

export interface CreatePurchaseOrderRequest {
  supplierId: number
  currencyId: number
  exchangeRate: number
  orderDate: string
  expectedDate: string | null
  createdByEmployeeId: number | null
  note: string | null
  lines: CreatePurchaseOrderLineRequest[]
}

export interface PurchaseOrderLineResponse {
  purchaseOrderDetailId: number
  productId: number
  sku: string
  productName: string
  unitId: number
  unitCode: string
  unitName: string
  orderedQuantity: number
  previouslyAcceptedQuantity: number
  unitPrice: number
}

export interface PurchaseOrderResponse {
  purchaseOrderId: number
  poNumber: string
  supplierId: number
  supplierCode: string
  supplierName: string
  currencyId: number
  currencyCode: string
  currencyName: string
  orderDate: string
  expectedDate: string | null
  exchangeRate: number
  status: string
  createdByEmployeeId: number | null
  note: string | null
  lines: PurchaseOrderLineResponse[]
}

export interface GoodsReceiptListItemResponse {
  goodsReceiptId: number
  receiptNumber: string
  purchaseOrderId: number
  poNumber: string
  supplierName: string
  warehouseId: number
  warehouseName: string
  receivedAt: string
  status: string
  lineCount: number
  receivedQuantity: number
  acceptedQuantity: number
  rejectedQuantity: number
}

export interface GoodsReceiptDetailResponse {
  goodsReceiptDetailId: number
  purchaseOrderDetailId: number
  productId: number
  sku: string
  productName: string
  unitId: number
  unitCode: string
  unitName: string
  orderedQuantity: number
  previouslyAcceptedQuantity: number
  unitPrice: number
  receivedQuantity: number
  acceptedQuantity: number
  rejectedQuantity: number
}

export interface QualityInspectionCheckResponse {
  qualityInspectionDetailId: number
  goodsReceiptDetailId: number
  checkName: string
  result: string
  note: string | null
}

export interface QualityInspectionResponse {
  qualityInspectionId: number
  inspectionNumber: string
  inspectedAt: string
  inspectorEmployeeId: number | null
  result: string
  note: string | null
  checks: QualityInspectionCheckResponse[]
}

export interface GoodsReceiptResponse {
  goodsReceiptId: number
  receiptNumber: string
  purchaseOrderId: number
  poNumber: string
  currencyCode: string
  supplierId: number
  supplierName: string
  warehouseId: number
  warehouseCode: string
  warehouseName: string
  receivedAt: string
  receivedByEmployeeId: number | null
  status: string
  note: string | null
  lines: GoodsReceiptDetailResponse[]
  qualityInspection: QualityInspectionResponse | null
}

export interface CreateGoodsReceiptRequest {
  purchaseOrderId: number
  warehouseId: number
  receivedByEmployeeId: number | null
  note: string | null
}

export interface GoodsReceiptCheckInLineRequest {
  goodsReceiptDetailId: number
  receivedQuantity: number
  acceptedQuantity: number
  rejectedQuantity: number
}

export interface GoodsReceiptCheckInRequest {
  lines: GoodsReceiptCheckInLineRequest[]
}

export interface SaveQualityInspectionCheckRequest {
  goodsReceiptDetailId: number
  checkName: string
  result: 'Pass' | 'Fail' | 'Pending' | 'NotApplicable'
  note: string | null
}

export interface SaveQualityInspectionRequest {
  inspectorEmployeeId: number | null
  note: string | null
  checks: SaveQualityInspectionCheckRequest[]
}

export interface GoodsReceiptPutawayItemRequest {
  goodsReceiptDetailId: number
  binId: number
  quantity: number
  lotCode: string
  manufacturerId: number
  countryId: number
  manufactureDate: string | null
  expiryDate: string | null
  supplierLotCode: string | null
}

export interface ConfirmGoodsReceiptRequest {
  putaway: GoodsReceiptPutawayItemRequest[]
}

export interface InventoryStockResponse {
  balanceId: number
  lotId: number
  lotCode: string
  productId: number
  sku: string
  productName: string
  warehouseId: number
  warehouseCode: string
  warehouseName: string
  binId: number
  binCode: string
  locationCode: string
  manufacturerName: string
  countryName: string
  manufactureDate: string | null
  expiryDate: string | null
  quantityOnHand: number
  quantityReserved: number
  quantityAvailable: number
  updatedAt: string
}

export interface InventoryStockPageResponse {
  items: InventoryStockResponse[]
  page: number
  pageSize: number
  totalCount: number
  totalOnHand: number
  totalReserved: number
  totalAvailable: number
}

export interface InventoryTransactionResponse {
  transactionId: number
  lotId: number
  lotCode: string
  productId: number
  sku: string
  productName: string
  warehouseId: number
  warehouseCode: string
  warehouseName: string
  binId: number | null
  binCode: string | null
  transactionType: string
  quantityChange: number
  referenceType: string | null
  referenceId: number | null
  occurredAt: string
  note: string | null
}

export interface InventoryTransactionPageResponse {
  items: InventoryTransactionResponse[]
  page: number
  pageSize: number
  totalCount: number
}

export interface CreateInventoryAdjustmentRequest {
  lotId: number
  binId: number
  quantityChange: number
  createdByEmployeeId: number | null
  reason: string
}

export interface CreateInventoryTransferRequest {
  lotId: number
  sourceBinId: number
  destinationBinId: number
  quantity: number
  createdByEmployeeId: number | null
  reason: string
}

export interface OutboundPageResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export interface OutboundOptionsResponse {
  customers: OutboundCustomerOptionResponse[]
  currencies: OutboundCurrencyOptionResponse[]
  products: OutboundProductOptionResponse[]
  warehouses: OutboundWarehouseOptionResponse[]
  employees: OutboundEmployeeOptionResponse[]
}

export interface OutboundCustomerOptionResponse {
  customerId: number
  customerCode: string
  customerName: string
  addresses: OutboundAddressOptionResponse[]
}

export interface OutboundAddressOptionResponse {
  addressId: number
  addressLine: string
  province: string | null
  district: string | null
  isDefault: boolean
}

export interface OutboundCurrencyOptionResponse {
  currencyId: number
  currencyCode: string
  currencyName: string
}

export interface OutboundProductOptionResponse {
  productId: number
  sku: string
  productName: string
  units: OutboundUnitOptionResponse[]
}

export interface OutboundUnitOptionResponse {
  unitId: number
  unitCode: string
  unitName: string
  conversionToBase: number
  isBaseUnit: boolean
}

export interface OutboundWarehouseOptionResponse {
  warehouseId: number
  warehouseCode: string
  warehouseName: string
}

export interface OutboundEmployeeOptionResponse {
  employeeId: number
  employeeCode: string
  fullName: string
}

export interface OutboundAvailableStockResponse {
  lotId: number
  lotCode: string
  expiryDate: string | null
  binId: number
  binCode: string
  locationCode: string
  quantityOnHand: number
  quantityReserved: number
  quantityAvailable: number
  quantityAvailableInOrderUnit: number
}

export interface OutboundOrderListItemResponse {
  salesOrderId: number
  salesOrderNumber: string
  customerId: number
  customerCode: string
  customerName: string
  orderDate: string
  currencyId: number
  currencyCode: string
  exchangeRate: number
  status: string
  lineCount: number
  deliveryCount: number
}

export interface OutboundOrderResponse {
  salesOrderId: number
  salesOrderNumber: string
  customerId: number
  customerCode: string
  customerName: string
  orderDate: string
  currencyId: number
  currencyCode: string
  exchangeRate: number
  status: string
  createdByEmployeeId: number | null
  note: string | null
  lines: OutboundOrderLineResponse[]
  deliveries: OutboundDeliveryListItemResponse[]
}

export interface OutboundOrderLineResponse {
  salesOrderDetailId: number
  productId: number
  sku: string
  productName: string
  quantity: number
  unitId: number
  unitCode: string
  conversionToBase: number
  unitPrice: number
  discountAmount: number
  quantityDelivered: number
  activeReservations: OutboundReservationResponse[]
}

export interface OutboundReservationResponse {
  reservationId: number
  lotId: number
  lotCode: string
  binId: number
  binCode: string
  reservedQuantity: number
  reservedQuantityInOrderUnit: number
}

export interface OutboundDeliveryListItemResponse {
  deliveryId: number
  deliveryNumber: string
  warehouseId: number
  warehouseCode: string
  warehouseName: string
  deliveryAddressId: number | null
  plannedAt: string | null
  shippedAt: string | null
  status: string
  lineCount: number
}

export interface OutboundDeliveryResponse {
  deliveryId: number
  deliveryNumber: string
  salesOrderId: number
  salesOrderNumber: string
  customerName: string
  warehouseId: number
  warehouseCode: string
  warehouseName: string
  deliveryAddressId: number | null
  plannedAt: string | null
  shippedAt: string | null
  status: string
  lines: OutboundDeliveryLineResponse[]
}

export interface OutboundDeliveryLineResponse {
  deliveryDetailId: number
  salesOrderDetailId: number
  productId: number
  sku: string
  productName: string
  lotId: number
  lotCode: string
  quantity: number
  unitCode: string
}

export interface CreateOutboundOrderRequest {
  customerId: number
  currencyId: number
  exchangeRate: number
  createdByEmployeeId: number | null
  note: string | null
  lines: CreateOutboundOrderLineRequest[]
}

export interface CreateOutboundOrderLineRequest {
  productId: number
  unitId: number
  quantity: number
  unitPrice: number
  discountAmount: number
}

export interface ConfirmOutboundOrderRequest {
  warehouseId: number
  deliveryAddressId: number | null
  allocations: OutboundAllocationRequest[]
}

export interface OutboundAllocationRequest {
  salesOrderDetailId: number
  lotId: number
  binId: number
  quantity: number
}

export interface ShipOutboundDeliveryRequest {
  shippedByEmployeeId: number | null
}
