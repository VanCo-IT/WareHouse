import { apiClient } from './apiClient'
import type {
  CreateInventoryAdjustmentRequest,
  CreateInventoryTransferRequest,
  InventoryStockPageResponse,
  InventoryTransactionPageResponse,
  InventoryTransactionResponse,
} from '../types/contracts'

export const inventoryRoutes = {
  stock: '/api/inventory',
  transactions: '/api/inventory/transactions',
  adjustments: '/api/inventory/adjustments',
  transfers: '/api/inventory/transfers',
}

function withQuery(path: string, values: Record<string, string | number | boolean | undefined>) {
  const query = new URLSearchParams()
  Object.entries(values).forEach(([key, value]) => {
    if (value !== undefined && String(value).length > 0) query.set(key, String(value))
  })
  const suffix = query.toString()
  return suffix.length > 0 ? `${path}?${suffix}` : path
}

export const inventoryApi = {
  stock: (filters: {
    search?: string
    warehouseId?: number
    productId?: number
    binId?: number
    availableOnly?: boolean
    page?: number
    pageSize?: number
  }) =>
    apiClient.get<InventoryStockPageResponse>(withQuery(inventoryRoutes.stock, filters)),
  transactions: (filters: {
    warehouseId?: number
    productId?: number
    transactionType?: string
    fromDate?: string
    toDate?: string
    page?: number
    pageSize?: number
  }) =>
    apiClient.get<InventoryTransactionPageResponse>(
      withQuery(inventoryRoutes.transactions, filters),
    ),
  createAdjustment: (request: CreateInventoryAdjustmentRequest) =>
    apiClient.post<InventoryTransactionResponse>(inventoryRoutes.adjustments, request),
  createTransfer: (request: CreateInventoryTransferRequest) =>
    apiClient.post<InventoryTransactionResponse[]>(inventoryRoutes.transfers, request),
}
