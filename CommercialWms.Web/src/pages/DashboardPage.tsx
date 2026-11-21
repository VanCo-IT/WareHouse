import { Link } from 'react-router-dom'
import { ErrorBanner, PageHeading } from '../components/Ui'
import { useApiQuery } from '../hooks/useApiQuery'
import { apiRoutes } from '../services/catalogApi'
import type {
  BrandResponse,
  CategoryResponse,
  ProductResponse,
  SupplierResponse,
} from '../types/contracts'

const cards = [
  { title: 'Sản phẩm', path: '/products', symbol: 'SP' },
  { title: 'Danh mục', path: '/categories', symbol: 'DM' },
  { title: 'Thương hiệu', path: '/brands', symbol: 'TH' },
  { title: 'Nhà cung cấp', path: '/suppliers', symbol: 'NCC' },
]

function DashboardPage() {
  const products = useApiQuery<ProductResponse[]>(apiRoutes.products)
  const categories = useApiQuery<CategoryResponse[]>(apiRoutes.categories)
  const brands = useApiQuery<BrandResponse[]>(apiRoutes.brands)
  const suppliers = useApiQuery<SupplierResponse[]>(apiRoutes.suppliers)
  const queries = [products, categories, brands, suppliers]
  const firstError = queries.find((query) => query.error)

  const counts = [
    products.data?.length,
    categories.data?.length,
    brands.data?.length,
    suppliers.data?.length,
  ]

  return (
    <>
      <PageHeading
        description="Tổng quan số liệu danh mục được lấy trực tiếp từ CommercialWms.Api."
        eyebrow="Không gian làm việc"
        title="Tổng quan"
      />

      {firstError?.error ? (
        <ErrorBanner message={firstError.error} onRetry={() => queries.forEach((query) => query.reload())} />
      ) : null}

      <div className="dashboard-grid">
        {cards.map((card, index) => {
          const query = queries[index]
          const count = counts[index]
          return (
            <Link className="metric-card panel" key={card.path} to={card.path}>
              <span className="metric-symbol">{card.symbol}</span>
              <span className="metric-copy">
                <span className="metric-label">{card.title}</span>
                <strong className="metric-value">
                  {query.loading ? '…' : query.error ? '—' : count ?? 0}
                </strong>
                <span className="metric-footnote">
                  {query.error ? 'Không tải được dữ liệu' : 'Bản ghi thực tế'}
                </span>
              </span>
              <span aria-hidden="true" className="metric-arrow">
                ↗
              </span>
            </Link>
          )
        })}
      </div>

      <section className="panel dashboard-note">
        <div>
          <div className="eyebrow">Trạng thái tích hợp</div>
          <h2>Danh mục đang kết nối API thật</h2>
          <p>
            Các số liệu phía trên lấy từ endpoint danh mục. Dashboard nghiệp vụ kho sẽ được bổ sung
            khi backend có API tồn kho, nhập hàng và xuất hàng.
          </p>
        </div>
        <span className="live-indicator">
          <span />
          API trực tiếp
        </span>
      </section>
    </>
  )
}

export default DashboardPage
