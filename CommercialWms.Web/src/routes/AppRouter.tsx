import { BrowserRouter, Route, Routes, useLocation } from 'react-router-dom'
import AppLayout from '../layouts/AppLayout'
import BrandsPage from '../pages/BrandsPage'
import CategoriesPage from '../pages/CategoriesPage'
import CountriesPage from '../pages/CountriesPage'
import CurrenciesPage from '../pages/CurrenciesPage'
import DashboardPage from '../pages/DashboardPage'
import InventoryPage from '../pages/InventoryPage'
import ManufacturersPage from '../pages/ManufacturersPage'
import OutboundPage from '../pages/OutboundPage'
import ProductsPage from '../pages/ProductsPage'
import ReceivingPage from '../pages/ReceivingPage'
import SupplierDetailPage from '../pages/SupplierDetailPage'
import SuppliersPage from '../pages/SuppliersPage'
import UnitsPage from '../pages/UnitsPage'
import WarehousesPage from '../pages/WarehousesPage'

function InDevelopmentPage() {
  const location = useLocation()
  const names: Record<string, string> = {
    '/receiving': 'Nhập hàng',
    '/inventory': 'Tồn kho',
    '/outbound': 'Xuất hàng',
    '/reports': 'Báo cáo',
    '/settings': 'Cài đặt',
  }
  const name = names[location.pathname] ?? 'Trang'

  return (
    <>
      <div className="page-heading">
        <div>
          <div className="eyebrow">Nghiệp vụ kho</div>
          <h1 className="page-title">{name}</h1>
          <p className="page-subtitle">Màn hình này chưa có API backend tương ứng.</p>
        </div>
      </div>
      <section className="panel placeholder-card">
        <span className="placeholder-mark">•••</span>
        <h2>Đang phát triển</h2>
        <p>Không có dữ liệu mẫu thay thế. Màn hình sẽ được triển khai sau khi API nghiệp vụ sẵn sàng.</p>
      </section>
    </>
  )
}

function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AppLayout />}>
          <Route element={<DashboardPage />} path="/" />
          <Route element={<CategoriesPage />} path="/categories" />
          <Route element={<CountriesPage />} path="/countries" />
          <Route element={<CurrenciesPage />} path="/currencies" />
          <Route element={<UnitsPage />} path="/units" />
          <Route element={<ManufacturersPage />} path="/manufacturers" />
          <Route element={<BrandsPage />} path="/brands" />
          <Route element={<ProductsPage />} path="/products" />
          <Route element={<SuppliersPage />} path="/suppliers" />
          <Route element={<SupplierDetailPage />} path="/suppliers/:id" />
          <Route element={<WarehousesPage />} path="/warehouses" />
          <Route element={<ReceivingPage />} path="/receiving" />
          <Route element={<InventoryPage />} path="/inventory" />
          <Route element={<OutboundPage />} path="/outbound" />
          <Route element={<InDevelopmentPage />} path="/reports" />
          <Route element={<InDevelopmentPage />} path="/settings" />
          <Route element={<InDevelopmentPage />} path="*" />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export { AppRouter }
