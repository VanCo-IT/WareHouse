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
  BrandCreateRequest,
  BrandResponse,
  BrandUpdateRequest,
  CountryResponse,
} from '../types/contracts'

type DialogBrand = BrandResponse | null | undefined
const EMPTY_BRANDS: BrandResponse[] = []

function BrandDialog({
  brand,
  countries,
  onClose,
  onSaved,
}: {
  brand: BrandResponse | null
  countries: CountryResponse[]
  onClose: () => void
  onSaved: () => void
}) {
  const [brandName, setBrandName] = useState(brand?.brandName ?? '')
  const [countryId, setCountryId] = useState(brand ? String(brand.countryId) : '')
  const [isActive, setIsActive] = useState(brand?.isActive ?? true)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [validationMessages, setValidationMessages] = useState<string[]>([])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFormError(null)
    setValidationMessages([])

    const name = brandName.trim()
    const id = Number(countryId)
    const errors: string[] = []
    if (name.length === 0 || name.length > 120) errors.push('Tên thương hiệu phải có từ 1 đến 120 ký tự.')
    if (!Number.isInteger(id) || id < 1) errors.push('CountryId phải là số nguyên dương.')
    if (errors.length > 0) {
      setValidationMessages(errors)
      return
    }

    setSaving(true)
    try {
      if (brand) {
        const request: BrandUpdateRequest = { brandName: name, countryId: id, isActive }
        await catalogApi.updateBrand(brand.brandId, request)
      } else {
        const request: BrandCreateRequest = { brandName: name, countryId: id, isActive }
        await catalogApi.createBrand(request)
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
      description="Chọn quốc gia từ dữ liệu backend."
      onClose={onClose}
      title={brand ? 'Cập nhật thương hiệu' : 'Thêm thương hiệu'}
    >
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {formError ? <ErrorBanner message={formError} /> : null}
        <ValidationSummary messages={validationMessages} />
        <label className="form-field">
          <span>Tên thương hiệu <b>*</b></span>
          <input
            autoFocus
            maxLength={120}
            onChange={(event) => setBrandName(event.target.value)}
            required
            value={brandName}
          />
        </label>
        <label className="form-field">
          <span>Quốc gia <b>*</b></span>
          <select
            onChange={(event) => setCountryId(event.target.value)}
            required
            value={countryId}
          >
            <option value="">Chọn quốc gia</option>
            {countries.map((country) => (
              <option key={country.countryId} value={country.countryId}>
                {country.countryCode} — {country.countryName}
              </option>
            ))}
          </select>
        </label>
        <label className="checkbox-field">
          <input checked={isActive} onChange={(event) => setIsActive(event.target.checked)} type="checkbox" />
          <span>Đang hoạt động</span>
        </label>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">
            Hủy
          </button>
          <button className="btn btn-primary" disabled={saving} type="submit">
            {saving ? 'Đang lưu…' : 'Lưu thương hiệu'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

function BrandsPage() {
  const query = useApiQuery<BrandResponse[]>(apiRoutes.brands)
  const countriesQuery = useApiQuery<CountryResponse[]>(apiRoutes.countries)
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')
  const [dialogBrand, setDialogBrand] = useState<DialogBrand>(undefined)
  const [actionError, setActionError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const brands = query.data ?? EMPTY_BRANDS
  const countries = countriesQuery.data ?? []

  const filtered = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('vi')
    return brands.filter((brand) => {
      const matchesSearch = brand.brandName.toLocaleLowerCase('vi').includes(normalized)
      const matchesStatus =
        statusFilter === 'all'
        || (statusFilter === 'active' ? brand.isActive : !brand.isActive)
      return matchesSearch && matchesStatus
    })
  }, [brands, search, statusFilter])

  async function deleteBrand(brand: BrandResponse) {
    if (!window.confirm(`Xóa thương hiệu "${brand.brandName}"?`)) return
    setActionError(null)
    setNotice(null)
    try {
      await catalogApi.deleteBrand(brand.brandId)
      setNotice('Đã xóa thương hiệu.')
      query.reload()
    } catch (error) {
      setActionError(getErrorMessage(error))
    }
  }

  function saved() {
    setDialogBrand(undefined)
    setNotice(dialogBrand ? 'Đã cập nhật thương hiệu.' : 'Đã thêm thương hiệu.')
    setActionError(null)
    query.reload()
  }

  const columns = [
    {
      key: 'name',
      title: 'Tên thương hiệu',
      render: (item: BrandResponse) => <strong className="table-primary">{item.brandName}</strong>,
    },
    {
      key: 'country',
      title: 'Quốc gia',
      render: (item: BrandResponse) =>
        countries.find((country) => country.countryId === item.countryId)?.countryName ?? `#${item.countryId}`,
    },
    {
      key: 'status',
      title: 'Trạng thái',
      render: (item: BrandResponse) => <StatusBadge active={item.isActive} />,
    },
    {
      key: 'actions',
      title: 'Thao tác',
      align: 'right' as const,
      render: (item: BrandResponse) => (
        <div className="row-actions">
          <button className="btn btn-small btn-secondary" onClick={() => setDialogBrand(item)} type="button">
            Sửa
          </button>
          <button className="btn btn-small btn-danger-ghost" onClick={() => void deleteBrand(item)} type="button">
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
          <button className="btn btn-primary" onClick={() => setDialogBrand(null)} type="button">
            <span className="button-plus">+</span> Thêm thương hiệu
          </button>
        }
        description="Quản lý thương hiệu và quốc gia liên kết theo dữ liệu backend."
        title="Thương hiệu"
      />
      {notice ? <NoticeBanner message={notice} onDismiss={() => setNotice(null)} /> : null}
      {actionError ? <ErrorBanner message={actionError} /> : null}
      {query.error ? <ErrorBanner message={query.error} onRetry={query.reload} /> : null}
      {countriesQuery.error ? (
        <ErrorBanner message={`Không tải được danh sách quốc gia: ${countriesQuery.error}`} onRetry={countriesQuery.reload} />
      ) : null}
      <section className="panel data-panel">
        <div className="table-toolbar">
          <div className="table-toolbar-copy">
            <strong>Danh sách thương hiệu</strong>
            <span>{query.loading ? 'Đang đồng bộ…' : `${filtered.length} bản ghi`}</span>
          </div>
          <div className="table-filters">
            <label className="search-box">
              <span aria-hidden="true">⌕</span>
              <input
                aria-label="Tìm thương hiệu"
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Tìm thương hiệu..."
                value={search}
              />
            </label>
            <select aria-label="Lọc theo trạng thái" onChange={(event) => setStatusFilter(event.target.value)} value={statusFilter}>
              <option value="all">Tất cả trạng thái</option>
              <option value="active">Đang hoạt động</option>
              <option value="inactive">Ngừng hoạt động</option>
            </select>
          </div>
        </div>
        <DataTable
          columns={columns}
          emptyMessage={search ? 'Không tìm thấy thương hiệu phù hợp.' : 'Chưa có thương hiệu nào.'}
          getKey={(item) => item.brandId}
          loading={query.loading}
          rows={filtered}
        />
      </section>
      {dialogBrand !== undefined ? (
        <BrandDialog
          brand={dialogBrand}
          countries={countries}
          key={dialogBrand?.brandId ?? 'new'}
          onClose={() => setDialogBrand(undefined)}
          onSaved={saved}
        />
      ) : null}
    </>
  )
}

export default BrandsPage
