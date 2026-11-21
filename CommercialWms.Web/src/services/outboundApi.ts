import { apiClient } from './apiClient'
import type {
  ConfirmOutboundOrderRequest,
  CreateOutboundOrderRequest,
  OutboundAvailableStockResponse,
  OutboundDeliveryResponse,
  OutboundOptionsResponse,
  OutboundOrderResponse,
  OutboundPageResponse,
  OutboundOrderListItemResponse,
  OutboundDeliveryListItemResponse,
  ShipOutboundDeliveryRequest,
} from '../types/contracts'

function withQuery(path: string, values: Record<string, string | number | undefined>) {
  const query = new URLSearchParams()
  Object.entries(values).forEach(([key, value]) => {
    if (value !== undefined && String(value).length > 0) query.set(key, String(value))
  })
  const suffix = query.toString()
  return suffix.length > 0 ? `${path}?${suffix}` : path
}

export const outboundRoutes = {
  options: '/api/outbound/options',
  orders: '/api/outbound/orders',
  order: (id: number) => `/api/outbound/orders/${id}`,
  confirmOrder: (id: number) => `/api/outbound/orders/${id}/confirm`,
  cancelOrder: (id: number) => `/api/outbound/orders/${id}/cancel`,
  deliveries: '/api/outbound/deliveries',
  delivery: (id: number) => `/api/outbound/deliveries/${id}`,
  prepareDelivery: (id: number) => `/api/outbound/deliveries/${id}/prepare`,
  shipDelivery: (id: number) => `/api/outbound/deliveries/${id}/ship`,
  markDelivered: (id: number) => `/api/outbound/deliveries/${id}/delivered`,
  cancelDelivery: (id: number) => `/api/outbound/deliveries/${id}/cancel`,
  availability: (warehouseId: number, productId: number, unitId: number) =>
    withQuery('/api/outbound/availability', { warehouseId, productId, unitId }),
}

export const outboundApi = {
  options: () => apiClient.get<OutboundOptionsResponse>(outboundRoutes.options),
  orders: (filters: { search?: string; status?: string; page?: number; pageSize?: number }) =>
    apiClient.get<OutboundPageResponse<OutboundOrderListItemResponse>>(
      withQuery(outboundRoutes.orders, filters),
    ),
  order: (id: number) => apiClient.get<OutboundOrderResponse>(outboundRoutes.order(id)),
  createOrder: (request: CreateOutboundOrderRequest) =>
    apiClient.post<OutboundOrderResponse>(outboundRoutes.orders, request),
  confirmOrder: (id: number, request: ConfirmOutboundOrderRequest) =>
    apiClient.post<OutboundOrderResponse>(outboundRoutes.confirmOrder(id), request),
  cancelOrder: (id: number) =>
    apiClient.post<OutboundOrderResponse>(outboundRoutes.cancelOrder(id), {}),
  deliveries: (filters: { search?: string; status?: string; page?: number; pageSize?: number }) =>
    apiClient.get<OutboundPageResponse<OutboundDeliveryListItemResponse>>(
      withQuery(outboundRoutes.deliveries, filters),
    ),
  delivery: (id: number) => apiClient.get<OutboundDeliveryResponse>(outboundRoutes.delivery(id)),
  prepareDelivery: (id: number) =>
    apiClient.post<OutboundDeliveryResponse>(outboundRoutes.prepareDelivery(id), {}),
  shipDelivery: (id: number, request: ShipOutboundDeliveryRequest) =>
    apiClient.post<OutboundDeliveryResponse>(outboundRoutes.shipDelivery(id), request),
  markDelivered: (id: number) =>
    apiClient.post<OutboundDeliveryResponse>(outboundRoutes.markDelivered(id), {}),
  cancelDelivery: (id: number) =>
    apiClient.post<OutboundDeliveryResponse>(outboundRoutes.cancelDelivery(id), {}),
  availability: (warehouseId: number, productId: number, unitId: number) =>
    apiClient.get<OutboundAvailableStockResponse[]>(
      outboundRoutes.availability(warehouseId, productId, unitId),
    ),
}
