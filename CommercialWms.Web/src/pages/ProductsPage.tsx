import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import {
  DataTable,
  ErrorBanner,
  Modal,
  NoticeBanner,
  PageHeading,
  StatusBadge,
  ValidationSummary,
} from '../components/Ui'
import { useApiQuery } from '../hooks/useApiQuery'
import { ApiError, getErrorMessage } from '../services/apiClient'
import { apiRoutes, catalogApi } from '../services/catalogApi'
import type {
  BrandResponse,
  CategoryResponse,
  ProductCreateRequest,
  ProductResponse,
  ProductUpdateRequest,
  UnitResponse,
} from '../types/contracts'

type DialogProduct = ProductResponse | null | undefined
const EMPTY_PRODUCTS: ProductResponse[] = []
const EMPTY_CATEGORIES: CategoryResponse[] = []
const EMPTY_BRANDS: BrandResponse[] = []
const EMPTY_UNITS: UnitResponse[] = []

function ProductDialog({
  product,
  categories,
  brands,
  units,
  onClose,
  onSaved,
}: {
  product: ProductResponse | null
  categories: CategoryResponse[]
  brands: BrandResponse[]
  units: UnitResponse[]
  onClose: () => void
  onSaved: () => void
}) {
  const [sku, setSku] = useState(product?.sku ?? '')
  const [productName, setProductName] = useState(product?.productName ?? '')
  const [categoryId, setCategoryId] = useState(product ? String(product.categoryId) : '')
  const [unitId, setUnitId] = useState(product?.unitId ? String(product.unitId) : '')
  const [brandId, setBrandId] = useState(
    product?.brandId === null || product?.brandId === undefined ? '' : String(product.brandId),
  )
  const [barcode, setBarcode] = useState(product?.barcode ?? '')
  const [description, setDescription] = useState(product?.description ?? '')
  const [isPerishable, setIsPerishable] = useState(product?.isPerishable ?? false)
  const [isActive, setIsActive] = useState(product?.isActive ?? true)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [validationMessages, setValidationMessages] = useState<string[]>([])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFormError(null)
    setValidationMessages([])

    const productSku = sku.trim()
    const name = productName.trim()
    const selectedCategory = Number(categoryId)
    const selectedUnit = Number(unitId)
    const selectedBrand = brandId === '' ? null : Number(brandId)
    const productBarcode = barcode.trim() || null
    const productDescription = description.trim() || null
    const errors: string[] = []
    if (productSku.length === 0 || productSku.length > 40) errors.push('SKU phải có từ 1 đến 40 ký tự.')
    if (name.length === 0 || name.length > 180) errors.push('Tên sản phẩm phải có từ 1 đến 180 ký tự.')
    if (!Number.isInteger(selectedCategory) || selectedCategory < 1) errors.push('Vui lòng chọn danh mục hợp lệ.')
    if (!Number.isInteger(selectedUnit) || selectedUnit < 1) errors.push('Vui lòng chọn đơn vị tính hợp lệ.')
    if (selectedBrand !== null && (!Number.isInteger(selectedBrand) || selectedBrand < 1)) {
      errors.push('Thương hiệu không hợp lệ.')
    }
    if (productBarcode && productBarcode.length > 60) errors.push('Mã vạch không được vượt quá 60 ký tự.')
    if (productDescription && productDescription.length > 500) errors.push('Mô tả không được vượt quá 500 ký tự.')
    if (errors.length > 0) {
      setValidationMessages(errors)
      return
    }

    const payload = {
      sku: productSku,
      productName: name,
      categoryId: selectedCategory,
      unitId: selectedUnit,
      brandId: selectedBrand,
      barcode: productBarcode,
      description: productDescription,
    }
    setSaving(true)
    try {
      if (product) {
        const request: ProductUpdateRequest = { ...payload, isPerishable, isActive }
        await catalogApi.updateProduct(product.productId, request)
      } else {
        const request: ProductCreateRequest = { ...payload, isPerishable, isActive }
        await catalogApi.createProduct(request)
      }
      onSaved()
    } catch (error) {
      setFormError(getErrorMessage(error))
      if (error instanceof ApiError) {
        setValidationMessages(Object.values(error.validationErrors).flat())
      }
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal
      description="Các trường trong biểu mẫu khớp với hợp đồng Product API."
      onClose={onClose}
      title={product ? 'Cập nhật sản phẩm' : 'Thêm sản phẩm'}
      wide
    >
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {formError ? <ErrorBanner message={formError} /> : null}
        <ValidationSummary messages={validationMessages} />
        <div className="form-grid">
          <label className="form-field">
            <span>SKU <b>*</b></span>
            <input autoFocus maxLength={40} onChange={(event) => setSku(event.target.value)} required value={sku} />
          </label>
          <label className="form-field">
            <span>Tên sản phẩm <b>*</b></span>
            <input maxLength={180} onChange={(event) => setProductName(event.target.value)} required value={productName} />
          </label>
          <label className="form-field">
            <span>Danh mục <b>*</b></span>
            <select onChange={(event) => setCategoryId(event.target.value)} required value={categoryId}>
              <option value="">Chọn danh mục</option>
              {categories.map((category) => (
                <option key={category.categoryId} value={category.categoryId}>
                  {category.categoryCode} — {category.categoryName}
                </option>
              ))}
            </select>
          </label>
          <label className="form-field">
            <span>Đơn vị cơ sở <b>*</b></span>
            <select
              disabled={Boolean(product?.unitId)}
              onChange={(event) => setUnitId(event.target.value)}
              required
              value={unitId}
            >
              <option value="">Chọn đơn vị tính</option>
              {units
                .filter((unit) => unit.isActive || unit.unitId === product?.unitId)
                .map((unit) => (
                  <option key={unit.unitId} value={unit.unitId}>
                    {unit.unitCode} — {unit.unitName}
                  </option>
                ))}
            </select>
            {product?.unitId ? <small>Đơn vị cơ sở không thể đổi khi chưa xác định hệ số quy đổi.</small> : null}
          </label>
          <label className="form-field">
            <span>Thương hiệu</span>
            <select onChange={(event) => setBrandId(event.target.value)} value={brandId}>
              <option value="">Không chọn</option>
              {brands.map((brand) => (
                <option key={brand.brandId} value={brand.brandId}>
                  {brand.brandName}
                </option>
              ))}
            </select>
          </label>
          <label className="form-field">
            <span>Mã vạch</span>
            <input maxLength={60} onChange={(event) => setBarcode(event.target.value)} value={barcode} />
          </label>
          <label className="form-field form-field-wide">
            <span>Mô tả</span>
            <textarea maxLength={500} onChange={(event) => setDescription(event.target.value)} rows={3} value={description} />
          </label>
        </div>
        <div className="checkbox-row">
          <label className="checkbox-field">
            <input checked={isPerishable} onChange={(event) => setIsPerishable(event.target.checked)} type="checkbox" />
            <span>Hàng dễ hỏng</span>
          </label>
          <label className="checkbox-field">
            <input checked={isActive} onChange={(event) => setIsActive(event.target.checked)} type="checkbox" />
            <span>Đang hoạt động</span>
          </label>
        </div>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">
            Hủy
          </button>
          <button className="btn btn-primary" disabled={saving} type="submit">
            {saving ? 'Đang lưu…' : 'Lưu sản phẩm'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

function ProductsPage() {
  const query = useApiQuery<ProductResponse[]>(apiRoutes.products)
  const categoriesQuery = useApiQuery<CategoryResponse[]>(apiRoutes.categories)
  const brandsQuery = useApiQuery<BrandResponse[]>(apiRoutes.brands)
  const unitsQuery = useApiQuery<UnitResponse[]>(apiRoutes.units)
  const [search, setSearch] = useState('')
  const [categoryFilter, setCategoryFilter] = useState('all')
  const [statusFilter, setStatusFilter] = useState('all')
  const [dialogProduct, setDialogProduct] = useState<DialogProduct>(undefined)
  const [actionError, setActionError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const products = query.data ?? EMPTY_PRODUCTS
  const categories = categoriesQuery.data ?? EMPTY_CATEGORIES
  const brands = brandsQuery.data ?? EMPTY_BRANDS
  const units = unitsQuery.data ?? EMPTY_UNITS

  const filtered = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('vi')
    return products.filter((product) => {
      const matchesSearch =
        normalized.length === 0
        || product.sku.toLocaleLowerCase('vi').includes(normalized)
        || product.productName.toLocaleLowerCase('vi').includes(normalized)
        || (product.barcode ?? '').toLocaleLowerCase('vi').includes(normalized)
      const matchesCategory = categoryFilter === 'all' || String(product.categoryId) === categoryFilter
      const matchesStatus =
        statusFilter === 'all'
        || (statusFilter === 'active' ? product.isActive : !product.isActive)
      return matchesSearch && matchesCategory && matchesStatus
    })
  }, [products, search, categoryFilter, statusFilter])

  async function deleteProduct(product: ProductResponse) {
    if (!window.confirm(`Xóa sản phẩm "${product.productName}" (${product.sku})?`)) return
    setActionError(null)
    setNotice(null)
    try {
      await catalogApi.deleteProduct(product.productId)
      setNotice('Đã xóa sản phẩm.')
      query.reload()
    } catch (error) {
      setActionError(getErrorMessage(error))
    }
  }

  function saved() {
    setDialogProduct(undefined)
    setNotice(dialogProduct ? 'Đã cập nhật sản phẩm.' : 'Đã thêm sản phẩm.')
    setActionError(null)
    query.reload()
  }

  const columns = [
    {
      key: 'product',
      title: 'Sản phẩm',
      render: (item: ProductResponse) => (
        <div className="product-cell">
          <span className="product-avatar">{item.productName.trim().charAt(0).toLocaleUpperCase('vi')}</span>
          <span>
            <strong className="table-primary">{item.productName}</strong>
            <small className="code-text">{item.sku}</small>
          </span>
        </div>
      ),
    },
    {
      key: 'category',
      title: 'Danh mục',
      render: (item: ProductResponse) =>
        categories.find((category) => category.categoryId === item.categoryId)?.categoryName ?? `#${item.categoryId}`,
    },
    {
      key: 'brand',
      title: 'Thương hiệu',
      render: (item: ProductResponse) =>
        brands.find((brand) => brand.brandId === item.brandId)?.brandName ?? '—',
    },
    {
      key: 'unit',
      title: 'Đơn vị cơ sở',
      render: (item: ProductResponse) =>
        units.find((unit) => unit.unitId === item.unitId)?.unitName ?? (item.unitId ? `#${item.unitId}` : '—'),
    },
    {
      key: 'perishable',
      title: 'Bảo quản',
      render: (item: ProductResponse) => (
        <span className={`simple-tag${item.isPerishable ? ' tag-warm' : ''}`}>
          {item.isPerishable ? 'Dễ hỏng' : 'Thông thường'}
        </span>
      ),
    },
    {
      key: 'status',
      title: 'Trạng thái',
      render: (item: ProductResponse) => <StatusBadge active={item.isActive} />,
    },
    {
      key: 'actions',
      title: 'Thao tác',
      align: 'right' as const,
      render: (item: ProductResponse) => (
        <div className="row-actions">
          <button className="btn btn-small btn-secondary" onClick={() => setDialogProduct(item)} type="button">
            Sửa
          </button>
          <button className="btn btn-small btn-danger-ghost" onClick={() => void deleteProduct(item)} type="button">
            Xóa
          </button>
        </div>
      ),
    },
  ]

  return (
    <>
      <PageHeading
        action={
          <button className="btn btn-primary" onClick={() => setDialogProduct(null)} type="button">
            <span className="button-plus">+</span> Thêm sản phẩm
          </button>
        }
        description="Tra cứu và quản lý thông tin sản phẩm trong danh mục WMS."
        title="Sản phẩm"
      />
      {notice ? <NoticeBanner message={notice} onDismiss={() => setNotice(null)} /> : null}
      {actionError ? <ErrorBanner message={actionError} /> : null}
      {query.error ? <ErrorBanner message={query.error} onRetry={query.reload} /> : null}
      {categoriesQuery.error ? <ErrorBanner message={`Không tải được danh mục: ${categoriesQuery.error}`} onRetry={categoriesQuery.reload} /> : null}
      {brandsQuery.error ? <ErrorBanner message={`Không tải được thương hiệu: ${brandsQuery.error}`} onRetry={brandsQuery.reload} /> : null}
      {unitsQuery.error ? <ErrorBanner message={`Không tải được đơn vị tính: ${unitsQuery.error}`} onRetry={unitsQuery.reload} /> : null}
      <section className="panel data-panel">
        <div className="table-toolbar">
          <div className="table-toolbar-copy">
            <strong>Danh sách sản phẩm</strong>
            <span>{query.loading ? 'Đang đồng bộ…' : `${filtered.length} bản ghi`}</span>
          </div>
          <div className="table-filters">
            <label className="search-box">
              <span aria-hidden="true">⌕</span>
              <input
                aria-label="Tìm sản phẩm"
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Tìm SKU, tên, mã vạch..."
                value={search}
              />
            </label>
            <select aria-label="Lọc danh mục" onChange={(event) => setCategoryFilter(event.target.value)} value={categoryFilter}>
              <option value="all">Tất cả danh mục</option>
              {categories.map((category) => (
                <option key={category.categoryId} value={category.categoryId}>
                  {category.categoryName}
                </option>
              ))}
            </select>
            <select aria-label="Lọc theo trạng thái" onChange={(event) => setStatusFilter(event.target.value)} value={statusFilter}>
              <option value="all">Tất cả trạng thái</option>
              <option value="active">Đang hoạt động</option>
              <option value="inactive">Ngừng hoạt động</option>
            </select>
          </div>
        </div>
        <DataTable
          columns={columns}
          emptyMessage={search ? 'Không tìm thấy sản phẩm phù hợp.' : 'Chưa có sản phẩm nào.'}
          getKey={(item) => item.productId}
          loading={query.loading}
          rows={filtered}
        />
      </section>
      {dialogProduct !== undefined ? (
        <ProductDialog
          brands={brands}
          categories={categories}
          units={units}
          key={dialogProduct?.productId ?? 'new'}
          onClose={() => setDialogProduct(undefined)}
          onSaved={saved}
          product={dialogProduct}
        />
      ) : null}
    </>
  )
}

export default ProductsPage
