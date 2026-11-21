import { apiClient } from './apiClient'
import type {
  BrandCreateRequest,
  BrandResponse,
  BrandUpdateRequest,
  CategoryCreateRequest,
  CategoryResponse,
  CategoryUpdateRequest,
  CountryCreateRequest,
  CountryResponse,
  CountryUpdateRequest,
  CurrencyCreateRequest,
  CurrencyResponse,
  CurrencyUpdateRequest,
  ManufacturerCreateRequest,
  ManufacturerResponse,
  ManufacturerUpdateRequest,
  ProductCreateRequest,
  ProductResponse,
  ProductUpdateRequest,
  SupplierContactCreateRequest,
  SupplierContactResponse,
  SupplierContactUpdateRequest,
  SupplierCreateRequest,
  SupplierProductCreateRequest,
  SupplierProductResponse,
  SupplierProductUpdateRequest,
  SupplierResponse,
  SupplierStatusUpdateRequest,
  SupplierUpdateRequest,
  UnitCreateRequest,
  UnitResponse,
  UnitUpdateRequest,
} from '../types/contracts'

const encode = (value: string | number) => encodeURIComponent(String(value))

export const apiRoutes = {
  categories: '/api/Categories',
  category: (id: number) => `/api/Categories/${encode(id)}`,
  countries: '/api/Countries',
  country: (id: number) => `/api/Countries/${encode(id)}`,
  currencies: '/api/Currencies',
  currency: (id: number) => `/api/Currencies/${encode(id)}`,
  units: '/api/Units',
  unit: (id: number) => `/api/Units/${encode(id)}`,
  manufacturers: '/api/Manufacturers',
  manufacturer: (id: number) => `/api/Manufacturers/${encode(id)}`,
  brands: '/api/Brands',
  brand: (id: number) => `/api/Brands/${encode(id)}`,
  products: '/api/Products',
  product: (id: number) => `/api/Products/${encode(id)}`,
  suppliers: '/api/Suppliers',
  supplier: (id: number) => `/api/Suppliers/${encode(id)}`,
  supplierStatus: (id: number) => `/api/Suppliers/${encode(id)}/status`,
  supplierProducts: (id: number) => `/api/Suppliers/${encode(id)}/products`,
  supplierProduct: (supplierId: number, productId: number, manufacturerId: number) =>
    `/api/Suppliers/${encode(supplierId)}/products/${encode(productId)}?manufacturerId=${encode(manufacturerId)}`,
  supplierContacts: (supplierId: number) => `/api/suppliers/${encode(supplierId)}/contacts`,
  supplierContact: (supplierId: number, contactId: number) =>
    `/api/suppliers/${encode(supplierId)}/contacts/${encode(contactId)}`,
}

export const catalogApi = {
  getCategories: () => apiClient.get<CategoryResponse[]>(apiRoutes.categories),
  createCategory: (request: CategoryCreateRequest) =>
    apiClient.post<CategoryResponse>(apiRoutes.categories, request),
  updateCategory: (id: number, request: CategoryUpdateRequest) =>
    apiClient.put<CategoryResponse>(apiRoutes.category(id), request),
  deleteCategory: (id: number) => apiClient.delete(apiRoutes.category(id)),

  getCountries: () => apiClient.get<CountryResponse[]>(apiRoutes.countries),
  getCountry: (id: number) => apiClient.get<CountryResponse>(apiRoutes.country(id)),
  createCountry: (request: CountryCreateRequest) =>
    apiClient.post<CountryResponse>(apiRoutes.countries, request),
  updateCountry: (id: number, request: CountryUpdateRequest) =>
    apiClient.put<CountryResponse>(apiRoutes.country(id), request),
  deleteCountry: (id: number) => apiClient.delete(apiRoutes.country(id)),

  getCurrencies: () => apiClient.get<CurrencyResponse[]>(apiRoutes.currencies),
  getCurrency: (id: number) => apiClient.get<CurrencyResponse>(apiRoutes.currency(id)),
  createCurrency: (request: CurrencyCreateRequest) =>
    apiClient.post<CurrencyResponse>(apiRoutes.currencies, request),
  updateCurrency: (id: number, request: CurrencyUpdateRequest) =>
    apiClient.put<CurrencyResponse>(apiRoutes.currency(id), request),
  deleteCurrency: (id: number) => apiClient.delete(apiRoutes.currency(id)),

  getUnits: () => apiClient.get<UnitResponse[]>(apiRoutes.units),
  getUnit: (id: number) => apiClient.get<UnitResponse>(apiRoutes.unit(id)),
  createUnit: (request: UnitCreateRequest) => apiClient.post<UnitResponse>(apiRoutes.units, request),
  updateUnit: (id: number, request: UnitUpdateRequest) =>
    apiClient.put<UnitResponse>(apiRoutes.unit(id), request),
  deleteUnit: (id: number) => apiClient.delete(apiRoutes.unit(id)),

  getManufacturers: () => apiClient.get<ManufacturerResponse[]>(apiRoutes.manufacturers),
  getManufacturer: (id: number) => apiClient.get<ManufacturerResponse>(apiRoutes.manufacturer(id)),
  createManufacturer: (request: ManufacturerCreateRequest) =>
    apiClient.post<ManufacturerResponse>(apiRoutes.manufacturers, request),
  updateManufacturer: (id: number, request: ManufacturerUpdateRequest) =>
    apiClient.put<ManufacturerResponse>(apiRoutes.manufacturer(id), request),
  deleteManufacturer: (id: number) => apiClient.delete(apiRoutes.manufacturer(id)),

  getBrands: () => apiClient.get<BrandResponse[]>(apiRoutes.brands),
  createBrand: (request: BrandCreateRequest) =>
    apiClient.post<BrandResponse>(apiRoutes.brands, request),
  updateBrand: (id: number, request: BrandUpdateRequest) =>
    apiClient.put<BrandResponse>(apiRoutes.brand(id), request),
  deleteBrand: (id: number) => apiClient.delete(apiRoutes.brand(id)),

  getProducts: () => apiClient.get<ProductResponse[]>(apiRoutes.products),
  createProduct: (request: ProductCreateRequest) =>
    apiClient.post<ProductResponse>(apiRoutes.products, request),
  updateProduct: (id: number, request: ProductUpdateRequest) =>
    apiClient.put<ProductResponse>(apiRoutes.product(id), request),
  deleteProduct: (id: number) => apiClient.delete(apiRoutes.product(id)),

  getSuppliers: () => apiClient.get<SupplierResponse[]>(apiRoutes.suppliers),
  getSupplier: (id: number) => apiClient.get<SupplierResponse>(apiRoutes.supplier(id)),
  createSupplier: (request: SupplierCreateRequest) =>
    apiClient.post<SupplierResponse>(apiRoutes.suppliers, request),
  updateSupplier: (id: number, request: SupplierUpdateRequest) =>
    apiClient.put<SupplierResponse>(apiRoutes.supplier(id), request),
  deleteSupplier: (id: number) => apiClient.delete(apiRoutes.supplier(id)),
  updateSupplierStatus: (id: number, request: SupplierStatusUpdateRequest) =>
    apiClient.patch<SupplierResponse>(apiRoutes.supplierStatus(id), request),

  getSupplierProducts: (id: number) =>
    apiClient.get<SupplierProductResponse[]>(apiRoutes.supplierProducts(id)),
  createSupplierProduct: (id: number, request: SupplierProductCreateRequest) =>
    apiClient.post<SupplierProductResponse>(apiRoutes.supplierProducts(id), request),
  updateSupplierProduct: (
    supplierId: number,
    productId: number,
    manufacturerId: number,
    request: SupplierProductUpdateRequest,
  ) =>
    apiClient.put<SupplierProductResponse>(
      apiRoutes.supplierProduct(supplierId, productId, manufacturerId),
      request,
    ),
  deleteSupplierProduct: (supplierId: number, productId: number, manufacturerId: number) =>
    apiClient.delete(apiRoutes.supplierProduct(supplierId, productId, manufacturerId)),

  getSupplierContacts: (supplierId: number) =>
    apiClient.get<SupplierContactResponse[]>(apiRoutes.supplierContacts(supplierId)),
  createSupplierContact: (supplierId: number, request: SupplierContactCreateRequest) =>
    apiClient.post<SupplierContactResponse>(apiRoutes.supplierContacts(supplierId), request),
  updateSupplierContact: (
    supplierId: number,
    contactId: number,
    request: SupplierContactUpdateRequest,
  ) =>
    apiClient.put<SupplierContactResponse>(
      apiRoutes.supplierContact(supplierId, contactId),
      request,
    ),
  deleteSupplierContact: (supplierId: number, contactId: number) =>
    apiClient.delete(apiRoutes.supplierContact(supplierId, contactId)),
}
