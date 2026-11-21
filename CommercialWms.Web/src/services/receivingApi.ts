import { apiClient } from './apiClient'
import type {
  ConfirmGoodsReceiptRequest,
  CreateGoodsReceiptRequest,
  CreatePurchaseOrderRequest,
  GoodsReceiptCheckInRequest,
  GoodsReceiptListItemResponse,
  GoodsReceiptResponse,
  ProductUnitOptionResponse,
  PurchaseOrderResponse,
  ReceivingBinOptionResponse,
  SaveQualityInspectionRequest,
} from '../types/contracts'

const encode = (value: string | number) => encodeURIComponent(String(value))

export const receivingRoutes = {
  purchaseOrders: '/api/Receiving/purchase-orders',
  purchaseOrder: (id: number) => `/api/Receiving/purchase-orders/${encode(id)}`,
  submitPurchaseOrder: (id: number) => `/api/Receiving/purchase-orders/${encode(id)}/submit`,
  approvePurchaseOrder: (id: number) => `/api/Receiving/purchase-orders/${encode(id)}/approve`,
  cancelPurchaseOrder: (id: number) => `/api/Receiving/purchase-orders/${encode(id)}/cancel`,
  productUnits: (productId: number) => `/api/Receiving/products/${encode(productId)}/units`,
  warehouseBins: (warehouseId: number) => `/api/Receiving/warehouses/${encode(warehouseId)}/bins`,
  receipts: '/api/Receiving/receipts',
  receiptList: (filters: {
    search?: string
    status?: string
    supplierId?: number
    warehouseId?: number
    fromDate?: string
    toDate?: string
  }) => withQuery('/api/Receiving/receipts', filters),
  receipt: (id: number) => `/api/Receiving/receipts/${encode(id)}`,
  checkIn: (id: number) => `/api/Receiving/receipts/${encode(id)}/check-in`,
  qualityInspection: (id: number) => `/api/Receiving/receipts/${encode(id)}/quality-inspection`,
  confirm: (id: number) => `/api/Receiving/receipts/${encode(id)}/confirm`,
  cancelReceipt: (id: number) => `/api/Receiving/receipts/${encode(id)}/cancel`,
}

function withQuery(path: string, values: Record<string, string | number | undefined>) {
  const query = new URLSearchParams()
  Object.entries(values).forEach(([key, value]) => {
    if (value !== undefined && String(value).length > 0) query.set(key, String(value))
  })
  const suffix = query.toString()
  return suffix.length > 0 ? `${path}?${suffix}` : path
}

export const receivingApi = {
  getPurchaseOrders: (status?: string) =>
    apiClient.get<PurchaseOrderResponse[]>(withQuery(receivingRoutes.purchaseOrders, { status })),
  getPurchaseOrder: (id: number) =>
    apiClient.get<PurchaseOrderResponse>(receivingRoutes.purchaseOrder(id)),
  createPurchaseOrder: (request: CreatePurchaseOrderRequest) =>
    apiClient.post<PurchaseOrderResponse>(receivingRoutes.purchaseOrders, request),
  submitPurchaseOrder: (id: number) =>
    apiClient.post<PurchaseOrderResponse>(receivingRoutes.submitPurchaseOrder(id), {}),
  approvePurchaseOrder: (id: number) =>
    apiClient.post<PurchaseOrderResponse>(receivingRoutes.approvePurchaseOrder(id), {}),
  cancelPurchaseOrder: (id: number) =>
    apiClient.post<PurchaseOrderResponse>(receivingRoutes.cancelPurchaseOrder(id), {}),
  getProductUnits: (productId: number) =>
    apiClient.get<ProductUnitOptionResponse[]>(receivingRoutes.productUnits(productId)),
  getWarehouseBins: (warehouseId: number) =>
    apiClient.get<ReceivingBinOptionResponse[]>(receivingRoutes.warehouseBins(warehouseId)),
  getReceipts: (filters: {
    search?: string
    status?: string
    supplierId?: number
    warehouseId?: number
    fromDate?: string
    toDate?: string
  }) =>
    apiClient.get<GoodsReceiptListItemResponse[]>(receivingRoutes.receiptList(filters)),
  getReceipt: (id: number) => apiClient.get<GoodsReceiptResponse>(receivingRoutes.receipt(id)),
  createReceipt: (request: CreateGoodsReceiptRequest) =>
    apiClient.post<GoodsReceiptResponse>(receivingRoutes.receipts, request),
  saveCheckIn: (id: number, request: GoodsReceiptCheckInRequest) =>
    apiClient.put<GoodsReceiptResponse>(receivingRoutes.checkIn(id), request),
  saveQualityInspection: (id: number, request: SaveQualityInspectionRequest) =>
    apiClient.put<GoodsReceiptResponse>(receivingRoutes.qualityInspection(id), request),
  confirmReceipt: (id: number, request: ConfirmGoodsReceiptRequest) =>
    apiClient.post<GoodsReceiptResponse>(receivingRoutes.confirm(id), request),
  cancelReceipt: (id: number) =>
    apiClient.post<GoodsReceiptResponse>(receivingRoutes.cancelReceipt(id), {}),
}
