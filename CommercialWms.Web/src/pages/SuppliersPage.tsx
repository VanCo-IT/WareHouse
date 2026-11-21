import { Link } from 'react-router-dom'
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
  CountryResponse,
  SupplierCreateRequest,
  SupplierResponse,
  SupplierUpdateRequest,
} from '../types/contracts'

type DialogSupplier = SupplierResponse | null | undefined
const EMPTY_SUPPLIERS: SupplierResponse[] = []

function SupplierDialog({
  supplier,
  countries,
  onClose,
  onSaved,
}: {
  supplier: SupplierResponse | null
  countries: CountryResponse[]
  onClose: () => void
  onSaved: () => void
}) {
  const [supplierCode, setSupplierCode] = useState(supplier?.supplierCode ?? '')
  const [supplierName, setSupplierName] = useState(supplier?.supplierName ?? '')
  const [countryId, setCountryId] = useState(supplier ? String(supplier.countryId) : '')
  const [taxCode, setTaxCode] = useState(supplier?.taxCode ?? '')
  const [email, setEmail] = useState(supplier?.email ?? '')
  const [phone, setPhone] = useState(supplier?.phone ?? '')
  const [address, setAddress] = useState(supplier?.address ?? '')
  const [isActive, setIsActive] = useState(supplier?.isActive ?? true)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [validationMessages, setValidationMessages] = useState<string[]>([])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFormError(null)
    setValidationMessages([])
    const code = supplierCode.trim()
    const name = supplierName.trim()
    const id = Number(countryId)
    const values = {
      taxCode: taxCode.trim() || null,
      email: email.trim() || null,
      phone: phone.trim() || null,
      address: address.trim() || null,
    }
    const errors: string[] = []
    if (code.length === 0 || code.length > 30) errors.push('Mã nhà cung cấp phải có từ 1 đến 30 ký tự.')
    if (name.length === 0 || name.length > 180) errors.push('Tên nhà cung cấp phải có từ 1 đến 180 ký tự.')
    if (!Number.isInteger(id) || id < 1) errors.push('CountryId phải là số nguyên dương.')
    if (!values.email && !values.phone) errors.push('Cần cung cấp email hoặc số điện thoại liên hệ.')
    if (values.taxCode && values.taxCode.length > 40) errors.push('Mã số thuế không được vượt quá 40 ký tự.')
    if (values.email && values.email.length > 150) errors.push('Email không được vượt quá 150 ký tự.')
    if (values.phone && values.phone.length > 30) errors.push('Số điện thoại không được vượt quá 30 ký tự.')
    if (values.address && values.address.length > 300) errors.push('Địa chỉ không được vượt quá 300 ký tự.')
    if (errors.length > 0) {
      setValidationMessages(errors)
      return
    }

    setSaving(true)
    try {
      if (supplier) {
        const request: SupplierUpdateRequest = {
          supplierCode: code,
          supplierName: name,
          countryId: id,
          ...values,
          isActive,
        }
        await catalogApi.updateSupplier(supplier.supplierId, request)
      } else {
        const request: SupplierCreateRequest = {
          supplierCode: code,
          supplierName: name,
          countryId: id,
          ...values,
          isActive,
        }
        await catalogApi.createSupplier(request)
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
      description="Quốc gia và thông tin liên hệ phải được lấy từ dữ liệu thực của nhà cung cấp."
      onClose={onClose}
      title={supplier ? 'Cập nhật nhà cung cấp' : 'Thêm nhà cung cấp'}
      wide
    >
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {formError ? <ErrorBanner message={formError} /> : null}
        <ValidationSummary messages={validationMessages} />
        <div className="form-grid">
          <label className="form-field">
            <span>Mã nhà cung cấp <b>*</b></span>
            <input autoFocus maxLength={30} onChange={(event) => setSupplierCode(event.target.value)} required value={supplierCode} />
          </label>
          <label className="form-field">
            <span>Tên nhà cung cấp <b>*</b></span>
            <input maxLength={180} onChange={(event) => setSupplierName(event.target.value)} required value={supplierName} />
          </label>
          <label className="form-field">
            <span>Quốc gia <b>*</b></span>
            <select onChange={(event) => setCountryId(event.target.value)} required value={countryId}>
              <option value="">Chọn quốc gia</option>
              {countries.map((country) => (
                <option key={country.countryId} value={country.countryId}>
                  {country.countryCode} — {country.countryName}
                </option>
              ))}
            </select>
          </label>
          <label className="form-field">
            <span>Mã số thuế</span>
            <input maxLength={40} onChange={(event) => setTaxCode(event.target.value)} value={taxCode} />
          </label>
          <label className="form-field">
            <span>Email {!phone.trim() ? <b>*</b> : null}</span>
            <input maxLength={150} onChange={(event) => setEmail(event.target.value)} type="email" value={email} />
          </label>
          <label className="form-field">
            <span>Số điện thoại {!email.trim() ? <b>*</b> : null}</span>
            <input maxLength={30} onChange={(event) => setPhone(event.target.value)} value={phone} />
          </label>
          <label className="form-field form-field-wide">
            <span>Địa chỉ</span>
            <textarea maxLength={300} onChange={(event) => setAddress(event.target.value)} rows={2} value={address} />
          </label>
        </div>
        <label className="checkbox-field">
          <input checked={isActive} onChange={(event) => setIsActive(event.target.checked)} type="checkbox" />
          <span>Đang hoạt động</span>
        </label>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">
            Hủy
          </button>
          <button className="btn btn-primary" disabled={saving} type="submit">
            {saving ? 'Đang lưu…' : 'Lưu nhà cung cấp'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

function SuppliersPage() {
  const query = useApiQuery<SupplierResponse[]>(apiRoutes.suppliers)
  const countriesQuery = useApiQuery<CountryResponse[]>(apiRoutes.countries)
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')
  const [dialogSupplier, setDialogSupplier] = useState<DialogSupplier>(undefined)
  const [actionError, setActionError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const suppliers = query.data ?? EMPTY_SUPPLIERS
  const countries = countriesQuery.data ?? []

  const filtered = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('vi')
    return suppliers.filter((supplier) => {
      const matchesSearch =
        normalized.length === 0
        || supplier.supplierCode.toLocaleLowerCase('vi').includes(normalized)
        || supplier.supplierName.toLocaleLowerCase('vi').includes(normalized)
        || (supplier.email ?? '').toLocaleLowerCase('vi').includes(normalized)
      const matchesStatus =
        statusFilter === 'all'
        || (statusFilter === 'active' ? supplier.isActive : !supplier.isActive)
      return matchesSearch && matchesStatus
    })
  }, [suppliers, search, statusFilter])

  async function toggleStatus(supplier: SupplierResponse) {
    setActionError(null)
    setNotice(null)
    try {
      await catalogApi.updateSupplierStatus(supplier.supplierId, { isActive: !supplier.isActive })
      setNotice(supplier.isActive ? 'Đã ngừng hoạt động nhà cung cấp.' : 'Đã kích hoạt nhà cung cấp.')
      query.reload()
    } catch (error) {
      setActionError(getErrorMessage(error))
    }
  }

  async function deleteSupplier(supplier: SupplierResponse) {
    if (!window.confirm(`Xóa nhà cung cấp "${supplier.supplierName}"?`)) return
    setActionError(null)
    setNotice(null)
    try {
      await catalogApi.deleteSupplier(supplier.supplierId)
      setNotice('Đã xóa nhà cung cấp.')
      query.reload()
    } catch (error) {
      setActionError(getErrorMessage(error))
    }
  }

  function saved() {
    setDialogSupplier(undefined)
    setNotice(dialogSupplier ? 'Đã cập nhật nhà cung cấp.' : 'Đã thêm nhà cung cấp.')
    setActionError(null)
    query.reload()
  }

  const columns = [
    {
      key: 'supplier',
      title: 'Nhà cung cấp',
      render: (item: SupplierResponse) => (
        <div className="supplier-cell">
          <span className="supplier-avatar">{item.supplierName.trim().charAt(0).toLocaleUpperCase('vi')}</span>
          <span>
            <strong className="table-primary">{item.supplierName}</strong>
            <small className="code-text">{item.supplierCode}</small>
          </span>
        </div>
      ),
    },
    {
      key: 'country',
      title: 'Quốc gia',
      render: (item: SupplierResponse) =>
        countries.find((country) => country.countryId === item.countryId)?.countryName ?? `#${item.countryId}`,
    },
    {
      key: 'contact',
      title: 'Liên hệ',
      render: (item: SupplierResponse) => (
        <span className="contact-summary">
          {item.email ?? '—'}
          <small>{item.phone ?? ''}</small>
        </span>
      ),
    },
    {
      key: 'status',
      title: 'Trạng thái',
      render: (item: SupplierResponse) => <StatusBadge active={item.isActive} />,
    },
    {
      key: 'actions',
      title: 'Thao tác',
      align: 'right' as const,
      render: (item: SupplierResponse) => (
        <div className="row-actions">
          <Link className="btn btn-small btn-secondary" to={`/suppliers/${item.supplierId}`}>
            Chi tiết
          </Link>
          <button className="btn btn-small btn-secondary" onClick={() => setDialogSupplier(item)} type="button">
            Sửa
          </button>
          <button className="btn btn-small btn-link" onClick={() => void toggleStatus(item)} type="button">
            {item.isActive ? 'Ngừng' : 'Kích hoạt'}
          </button>
          <button
            className="btn btn-small btn-danger-ghost"
            onClick={() => void deleteSupplier(item)}
            type="button"
          >
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
          <button className="btn btn-primary" onClick={() => setDialogSupplier(null)} type="button">
            <span className="button-plus">+</span> Thêm nhà cung cấp
          </button>
        }
        description="Quản lý thông tin đối tác và trạng thái hoạt động."
        title="Nhà cung cấp"
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
            <strong>Danh sách nhà cung cấp</strong>
            <span>{query.loading ? 'Đang đồng bộ…' : `${filtered.length} bản ghi`}</span>
          </div>
          <div className="table-filters">
            <label className="search-box">
              <span aria-hidden="true">⌕</span>
              <input
                aria-label="Tìm nhà cung cấp"
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Tìm mã, tên, email..."
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
          emptyMessage={search ? 'Không tìm thấy nhà cung cấp phù hợp.' : 'Chưa có nhà cung cấp nào.'}
          getKey={(item) => item.supplierId}
          loading={query.loading}
          rows={filtered}
        />
      </section>
      {dialogSupplier !== undefined ? (
        <SupplierDialog
          countries={countries}
          key={dialogSupplier?.supplierId ?? 'new'}
          onClose={() => setDialogSupplier(undefined)}
          onSaved={saved}
          supplier={dialogSupplier}
        />
      ) : null}
    </>
  )
}

export default SuppliersPage
