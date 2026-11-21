import { apiClient } from './apiClient'
import type {
  WarehouseAisleCreateRequest,
  WarehouseAisleResponse,
  WarehouseAisleUpdateRequest,
  WarehouseBinCreateRequest,
  WarehouseBinResponse,
  WarehouseBinUpdateRequest,
  WarehouseCreateRequest,
  WarehouseRackCreateRequest,
  WarehouseRackResponse,
  WarehouseRackUpdateRequest,
  WarehouseResponse,
  WarehouseShelfCreateRequest,
  WarehouseShelfResponse,
  WarehouseShelfUpdateRequest,
  WarehouseStructureResponse,
  WarehouseUpdateRequest,
  WarehouseZoneCreateRequest,
  WarehouseZoneResponse,
  WarehouseZoneUpdateRequest,
} from '../types/contracts'

const encode = (value: string | number) => encodeURIComponent(String(value))

export const warehouseRoutes = {
  warehouses: '/api/Warehouses',
  warehouse: (id: number) => `/api/Warehouses/${encode(id)}`,
  structure: (id: number) => `/api/Warehouses/${encode(id)}/structure`,
  zones: '/api/WarehouseZones',
  zone: (id: number) => `/api/WarehouseZones/${encode(id)}`,
  aisles: '/api/WarehouseAisles',
  aisle: (id: number) => `/api/WarehouseAisles/${encode(id)}`,
  racks: '/api/WarehouseRacks',
  rack: (id: number) => `/api/WarehouseRacks/${encode(id)}`,
  shelves: '/api/WarehouseShelves',
  shelf: (id: number) => `/api/WarehouseShelves/${encode(id)}`,
  bins: '/api/WarehouseBins',
  bin: (id: number) => `/api/WarehouseBins/${encode(id)}`,
}

export const warehouseApi = {
  getWarehouses: () => apiClient.get<WarehouseResponse[]>(warehouseRoutes.warehouses),
  getWarehouse: (id: number) => apiClient.get<WarehouseResponse>(warehouseRoutes.warehouse(id)),
  getStructure: (id: number) =>
    apiClient.get<WarehouseStructureResponse>(warehouseRoutes.structure(id)),
  createWarehouse: (request: WarehouseCreateRequest) =>
    apiClient.post<WarehouseResponse>(warehouseRoutes.warehouses, request),
  updateWarehouse: (id: number, request: WarehouseUpdateRequest) =>
    apiClient.put<WarehouseResponse>(warehouseRoutes.warehouse(id), request),
  deleteWarehouse: (id: number) => apiClient.delete(warehouseRoutes.warehouse(id)),

  createZone: (request: WarehouseZoneCreateRequest) =>
    apiClient.post<WarehouseZoneResponse>(warehouseRoutes.zones, request),
  updateZone: (id: number, request: WarehouseZoneUpdateRequest) =>
    apiClient.put<WarehouseZoneResponse>(warehouseRoutes.zone(id), request),
  deleteZone: (id: number) => apiClient.delete(warehouseRoutes.zone(id)),

  createAisle: (request: WarehouseAisleCreateRequest) =>
    apiClient.post<WarehouseAisleResponse>(warehouseRoutes.aisles, request),
  updateAisle: (id: number, request: WarehouseAisleUpdateRequest) =>
    apiClient.put<WarehouseAisleResponse>(warehouseRoutes.aisle(id), request),
  deleteAisle: (id: number) => apiClient.delete(warehouseRoutes.aisle(id)),

  createRack: (request: WarehouseRackCreateRequest) =>
    apiClient.post<WarehouseRackResponse>(warehouseRoutes.racks, request),
  updateRack: (id: number, request: WarehouseRackUpdateRequest) =>
    apiClient.put<WarehouseRackResponse>(warehouseRoutes.rack(id), request),
  deleteRack: (id: number) => apiClient.delete(warehouseRoutes.rack(id)),

  createShelf: (request: WarehouseShelfCreateRequest) =>
    apiClient.post<WarehouseShelfResponse>(warehouseRoutes.shelves, request),
  updateShelf: (id: number, request: WarehouseShelfUpdateRequest) =>
    apiClient.put<WarehouseShelfResponse>(warehouseRoutes.shelf(id), request),
  deleteShelf: (id: number) => apiClient.delete(warehouseRoutes.shelf(id)),

  createBin: (request: WarehouseBinCreateRequest) =>
    apiClient.post<WarehouseBinResponse>(warehouseRoutes.bins, request),
  updateBin: (id: number, request: WarehouseBinUpdateRequest) =>
    apiClient.put<WarehouseBinResponse>(warehouseRoutes.bin(id), request),
  deleteBin: (id: number) => apiClient.delete(warehouseRoutes.bin(id)),
}
