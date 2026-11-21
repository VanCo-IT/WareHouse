import { NavLink, Outlet } from 'react-router-dom'

const catalogItems = [
  { label: 'Quốc gia', path: '/countries', symbol: 'QG' },
  { label: 'Tiền tệ', path: '/currencies', symbol: 'TT' },
  { label: 'Đơn vị tính', path: '/units', symbol: 'DV' },
  { label: 'Nhà sản xuất', path: '/manufacturers', symbol: 'NSX' },
  { label: 'Danh mục', path: '/categories', symbol: 'DM' },
  { label: 'Thương hiệu', path: '/brands', symbol: 'TH' },
  { label: 'Sản phẩm', path: '/products', symbol: 'SP' },
  { label: 'Nhà cung cấp', path: '/suppliers', symbol: 'NCC' },
]

const operationalItems = [
  { label: 'Nhập hàng', path: '/receiving', symbol: 'NH' },
  { label: 'Tồn kho', path: '/inventory', symbol: 'TK' },
  { label: 'Xuất hàng', path: '/outbound', symbol: 'XH' },
]

const plannedItems = [
  { label: 'Báo cáo', path: '/reports', symbol: 'BC' },
  { label: 'Cài đặt', path: '/settings', symbol: 'CD' },
]

function AppLayout() {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <NavLink className="brand-lockup" to="/" aria-label="Commercial WMS - Tổng quan">
          <span className="brand-mark">CW</span>
          <span>
            <span className="brand-name">COMMERCIAL WMS</span>
            <span className="brand-subtitle">Warehouse platform</span>
          </span>
        </NavLink>

        <nav className="sidebar-nav" aria-label="Điều hướng chính">
          <span className="nav-label">Không gian làm việc</span>
          <NavLink className="nav-link" end to="/">
            <span className="nav-symbol">OV</span>
            <span className="nav-link-label">Tổng quan</span>
          </NavLink>

          <span className="nav-label">Quản lý kho</span>
          <NavLink className="nav-link" to="/warehouses">
            <span className="nav-symbol">WH</span>
            <span className="nav-link-label">Kho và vị trí</span>
          </NavLink>

          <span className="nav-label">Danh mục</span>
          {catalogItems.map((item) => (
            <NavLink className="nav-link" key={item.path} to={item.path}>
              <span className="nav-symbol">{item.symbol}</span>
              <span className="nav-link-label">{item.label}</span>
            </NavLink>
          ))}

          <span className="nav-label">Nghiệp vụ</span>
          {operationalItems.map((item) => (
            <NavLink className="nav-link" key={item.path} to={item.path}>
              <span className="nav-symbol">{item.symbol}</span>
              <span className="nav-link-label">{item.label}</span>
            </NavLink>
          ))}
          {plannedItems.map((item) => (
            <NavLink className="nav-link" key={item.path} to={item.path}>
              <span className="nav-symbol">{item.symbol}</span>
              <span className="nav-link-label">{item.label}</span>
              <span className="nav-status">Đang phát triển</span>
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-footer">
          <strong>Commercial WMS</strong>
          <br />
          Kết nối API qua proxy phát triển
        </div>
      </aside>

      <div className="workspace">
        <header className="topbar">
          <div className="topbar-context">
            Không gian / <strong>Quản trị kho</strong>
          </div>
          <div className="user-chip">
            <span className="user-avatar">QT</span>
            <span>
              <strong>Quản trị viên</strong>
              <span className="user-role">Tài khoản demo</span>
            </span>
          </div>
        </header>
        <main className="main-content">
          <Outlet />
        </main>
      </div>
    </div>
  )
}

export default AppLayout
