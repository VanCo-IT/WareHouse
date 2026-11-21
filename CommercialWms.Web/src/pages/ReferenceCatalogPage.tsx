import { useEffect, useMemo, useState } from 'react'
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
import { ApiError, getErrorMessage } from '../services/apiClient'
import { useApiQuery } from '../hooks/useApiQuery'
import { apiRoutes, catalogApi } from '../services/catalogApi'
import type { CountryResponse } from '../types/contracts'

export type ReferenceCatalogKind = 'countries' | 'currencies' | 'units' | 'manufacturers'

interface ReferenceCatalogRow {
  id: number
  code: string
  name: string
  isActive: boolean
  countryId?: number
  address?: string | null
}

interface CatalogDefinition {
  title: string
  singular: string
  codeLabel: string
  codeMaxLength: number
  nameMaxLength: number
  codeHint?: string
}

const definitions: Record<ReferenceCatalogKind, CatalogDefinition> = {
  countries: {
    title: 'Quốc gia',
    singular: 'quốc gia',
    codeLabel: 'Mã quốc gia',
    codeMaxLength: 2,
    nameMaxLength: 100,
    codeHint: 'Mã gồm đúng 2 chữ cái.',
  },
  currencies: {
    title: 'Tiền tệ',
    singular: 'tiền tệ',
    codeLabel: 'Mã tiền tệ',
    codeMaxLength: 3,
    nameMaxLength: 80,
    codeHint: 'Mã gồm đúng 3 chữ cái.',
  },
  units: {
    title: 'Đơn vị tính',
    singular: 'đơn vị tính',
    codeLabel: 'Mã đơn vị',
    codeMaxLength: 20,
    nameMaxLength: 60,
  },
  manufacturers: {
    title: 'Nhà sản xuất',
    singular: 'nhà sản xuất',
    codeLabel: 'Mã nhà sản xuất',
    codeMaxLength: 30,
    nameMaxLength: 180,
  },
}

function mapCountry(row: CountryResponse): ReferenceCatalogRow {
  return {
    id: row.countryId,
    code: row.countryCode,
    name: row.countryName,
    isActive: row.isActive,
  }
}

async function loadRows(kind: ReferenceCatalogKind): Promise<ReferenceCatalogRow[]> {
  switch (kind) {
    case 'countries':
      return (await catalogApi.getCountries()).map(mapCountry)
    case 'currencies':
      return (await catalogApi.getCurrencies()).map((row) => ({
        id: row.currencyId,
        code: row.currencyCode,
        name: row.currencyName,
        isActive: row.isActive,
      }))
    case 'units':
      return (await catalogApi.getUnits()).map((row) => ({
        id: row.unitId,
        code: row.unitCode,
        name: row.unitName,
        isActive: row.isActive,
      }))
    case 'manufacturers':
      return (await catalogApi.getManufacturers()).map((row) => ({
        id: row.manufacturerId,
        code: row.manufacturerCode,
        name: row.manufacturerName,
        countryId: row.countryId,
        address: row.address,
        isActive: row.isActive,
      }))
  }
}

interface CatalogFormValues {
  code: string
  name: string
  countryId: string
  address: string
  isActive: boolean
}

async function saveRow(
  kind: ReferenceCatalogKind,
  rowId: number | null,
  values: CatalogFormValues,
): Promise<void> {
  const isActive = values.isActive
  switch (kind) {
    case 'countries': {
      const request = {
        countryCode: values.code.trim().toUpperCase(),
        countryName: values.name.trim(),
        isActive,
      }
      if (rowId === null) await catalogApi.createCountry(request)
      else await catalogApi.updateCountry(rowId, request)
      return
    }
    case 'currencies': {
      const request = {
        currencyCode: values.code.trim().toUpperCase(),
        currencyName: values.name.trim(),
        isActive,
      }
      if (rowId === null) await catalogApi.createCurrency(request)
      else await catalogApi.updateCurrency(rowId, request)
      return
    }
    case 'units': {
      const request = {
        unitCode: values.code.trim(),
        unitName: values.name.trim(),
        isActive,
      }
      if (rowId === null) await catalogApi.createUnit(request)
      else await catalogApi.updateUnit(rowId, request)
      return
    }
    case 'manufacturers': {
      const request = {
        manufacturerCode: values.code.trim(),
        manufacturerName: values.name.trim(),
        countryId: Number(values.countryId),
        address: values.address.trim() || null,
        isActive,
      }
      if (rowId === null) await catalogApi.createManufacturer(request)
      else await catalogApi.updateManufacturer(rowId, request)
    }
  }
}

function CatalogDialog({
  kind,
  row,
  countries,
  onClose,
  onSaved,
}: {
  kind: ReferenceCatalogKind
  row: ReferenceCatalogRow | null
  countries: CountryResponse[]
  onClose: () => void
  onSaved: () => void
}) {
  const definition = definitions[kind]
  const [values, setValues] = useState<CatalogFormValues>({
    code: row?.code ?? '',
    name: row?.name ?? '',
    countryId: row?.countryId === undefined ? '' : String(row.countryId),
    address: row?.address ?? '',
    isActive: row?.isActive ?? true,
  })
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [validationMessages, setValidationMessages] = useState<string[]>([])

  function updateValue<Key extends keyof CatalogFormValues>(key: Key, value: CatalogFormValues[Key]) {
    setValues((current) => ({ ...current, [key]: value }))
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFormError(null)
    setValidationMessages([])

    const code = values.code.trim()
    const name = values.name.trim()
    const errors: string[] = []
    if (code.length === 0 || code.length > definition.codeMaxLength) {
      errors.push(`${definition.codeLabel} phải có từ 1 đến ${definition.codeMaxLength} ký tự.`)
    }
    if (name.length === 0 || name.length > definition.nameMaxLength) {
      errors.push(`Tên ${definition.singular} phải có từ 1 đến ${definition.nameMaxLength} ký tự.`)
    }
    if (kind === 'countries' && !/^[a-z]{2}$/i.test(code)) {
      errors.push('Mã quốc gia phải gồm đúng 2 chữ cái.')
    }
    if (kind === 'currencies' && !/^[a-z]{3}$/i.test(code)) {
      errors.push('Mã tiền tệ phải gồm đúng 3 chữ cái.')
    }
    if (kind === 'manufacturers') {
      const countryId = Number(values.countryId)
      if (!Number.isInteger(countryId) || countryId < 1) {
        errors.push('Vui lòng chọn quốc gia hợp lệ.')
      }
      if (values.address.trim().length > 300) {
        errors.push('Địa chỉ không được vượt quá 300 ký tự.')
      }
    }
    if (errors.length > 0) {
      setValidationMessages(errors)
      return
    }

    setSaving(true)
    try {
      await saveRow(kind, row?.id ?? null, values)
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
      description={`Thông tin được lưu bằng API ${definition.title.toLocaleLowerCase('vi')}.`}
      onClose={onClose}
      title={row ? `Cập nhật ${definition.singular}` : `Thêm ${definition.singular}`}
    >
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {formError ? <ErrorBanner message={formError} /> : null}
        <ValidationSummary messages={validationMessages} />
        <label className="form-field">
          <span>{definition.codeLabel} <b>*</b></span>
          <input
            autoFocus
            maxLength={definition.codeMaxLength}
            onChange={(event) => updateValue('code', event.target.value)}
            required
            value={values.code}
          />
          {definition.codeHint ? <small>{definition.codeHint}</small> : null}
        </label>
        <label className="form-field">
          <span>Tên {definition.singular} <b>*</b></span>
          <input
            maxLength={definition.nameMaxLength}
            onChange={(event) => updateValue('name', event.target.value)}
            required
            value={values.name}
          />
        </label>
        {kind === 'manufacturers' ? (
          <>
            <label className="form-field">
              <span>Quốc gia <b>*</b></span>
              <select
                onChange={(event) => updateValue('countryId', event.target.value)}
                required
                value={values.countryId}
              >
                <option value="">Chọn quốc gia</option>
                {countries.map((country) => (
                  <option key={country.countryId} value={country.countryId}>
                    {country.countryCode} — {country.countryName}
                  </option>
                ))}
              </select>
            </label>
            <label className="form-field">
              <span>Địa chỉ</span>
              <textarea
                maxLength={300}
                onChange={(event) => updateValue('address', event.target.value)}
                rows={2}
                value={values.address}
              />
            </label>
          </>
        ) : null}
        <label className="checkbox-field">
          <input
            checked={values.isActive}
            onChange={(event) => updateValue('isActive', event.target.checked)}
            type="checkbox"
          />
          <span>Đang hoạt động</span>
        </label>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">
            Hủy
          </button>
          <button className="btn btn-primary" disabled={saving} type="submit">
            {saving ? 'Đang lưu…' : 'Lưu'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

export function ReferenceCatalogPage({ kind }: { kind: ReferenceCatalogKind }) {
  const definition = definitions[kind]
  const [rows, setRows] = useState<ReferenceCatalogRow[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')
  const [page, setPage] = useState(1)
  const [dialogRow, setDialogRow] = useState<ReferenceCatalogRow | null | undefined>(undefined)
  const [detailRow, setDetailRow] = useState<ReferenceCatalogRow | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const countriesQuery = useApiQuery<CountryResponse[]>(
    kind === 'manufacturers' ? apiRoutes.countries : null,
  )
  const countries = countriesQuery.data ?? []

  async function reload() {
    try {
      const result = await loadRows(kind)
      setRows(result)
      setError(null)
    } catch (loadError) {
      setError(getErrorMessage(loadError))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    let active = true
    loadRows(kind)
      .then((result) => {
        if (active) {
          setRows(result)
          setError(null)
          setLoading(false)
        }
      })
      .catch((loadError: unknown) => {
        if (active) {
          setError(getErrorMessage(loadError))
          setLoading(false)
        }
      })
    return () => {
      active = false
    }
  }, [kind])

  function refresh() {
    setLoading(true)
    setError(null)
    void reload()
  }

  const filtered = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('vi')
    return rows.filter((row) => {
      const matchesSearch =
        normalized.length === 0
        || row.code.toLocaleLowerCase('vi').includes(normalized)
        || row.name.toLocaleLowerCase('vi').includes(normalized)
      const matchesStatus =
        statusFilter === 'all'
        || (statusFilter === 'active' ? row.isActive : !row.isActive)
      return matchesSearch && matchesStatus
    })
  }, [rows, search, statusFilter])

  const pageSize = 10
  const pageCount = Math.max(1, Math.ceil(filtered.length / pageSize))
  const visibleRows = filtered.slice((page - 1) * pageSize, page * pageSize)
  const manufacturerLookupUnavailable =
    kind === 'manufacturers'
    && (countriesQuery.loading || countriesQuery.error !== null || countries.length === 0)

  async function deleteRow(row: ReferenceCatalogRow) {
    if (!window.confirm(`Xóa "${row.name}"?`)) return
    setActionError(null)
    setNotice(null)
    try {
      switch (kind) {
        case 'countries':
          await catalogApi.deleteCountry(row.id)
          break
        case 'currencies':
          await catalogApi.deleteCurrency(row.id)
          break
        case 'units':
          await catalogApi.deleteUnit(row.id)
          break
        case 'manufacturers':
          await catalogApi.deleteManufacturer(row.id)
          break
      }
      setNotice(`Đã xóa ${definition.singular}.`)
      setLoading(true)
      await reload()
    } catch (deleteError) {
      setActionError(getErrorMessage(deleteError))
    }
  }

  function saved() {
    setDialogRow(undefined)
    setNotice(dialogRow ? `Đã cập nhật ${definition.singular}.` : `Đã thêm ${definition.singular}.`)
    setActionError(null)
    setLoading(true)
    void reload()
  }

  return (
    <>
      <PageHeading
        action={
          <button
            className="btn btn-primary"
            disabled={manufacturerLookupUnavailable}
            onClick={() => setDialogRow(null)}
            type="button"
          >
            <span className="button-plus">+</span> Thêm {definition.singular}
          </button>
        }
        description={`Quản lý ${definition.title.toLocaleLowerCase('vi')} bằng dữ liệu từ backend.`}
        title={definition.title}
      />
      {notice ? <NoticeBanner message={notice} onDismiss={() => setNotice(null)} /> : null}
      {actionError ? <ErrorBanner message={actionError} /> : null}
      {error ? <ErrorBanner message={error} onRetry={refresh} /> : null}
      {countriesQuery.error ? (
        <ErrorBanner
          message={`Không tải được danh sách quốc gia: ${countriesQuery.error}`}
          onRetry={countriesQuery.reload}
        />
      ) : null}
      {kind === 'manufacturers' && countriesQuery.loading ? (
        <div className="alert alert-info">Đang tải quốc gia để gắn nhà sản xuất…</div>
      ) : null}
      {kind === 'manufacturers' && !countriesQuery.loading && !countriesQuery.error && countries.length === 0 ? (
        <div className="alert alert-info">Chưa có quốc gia để gắn cho nhà sản xuất; hãy thêm quốc gia trước.</div>
      ) : null}
      <section className="panel data-panel">
        <div className="table-toolbar">
          <div className="table-toolbar-copy">
            <strong>Danh sách {definition.title.toLocaleLowerCase('vi')}</strong>
            <span>{loading ? 'Đang đồng bộ…' : `${filtered.length} bản ghi`}</span>
          </div>
          <div className="table-filters">
            <label className="search-box">
              <span aria-hidden="true">⌕</span>
              <input
                aria-label={`Tìm ${definition.singular}`}
                onChange={(event) => {
                  setSearch(event.target.value)
                  setPage(1)
                }}
                placeholder={`Tìm ${definition.singular}...`}
                value={search}
              />
            </label>
            <select
              aria-label="Lọc theo trạng thái"
              onChange={(event) => {
                setStatusFilter(event.target.value)
                setPage(1)
              }}
              value={statusFilter}
            >
              <option value="all">Tất cả trạng thái</option>
              <option value="active">Đang hoạt động</option>
              <option value="inactive">Ngừng hoạt động</option>
            </select>
          </div>
        </div>
        <DataTable
          columns={[
            {
              key: 'code',
              title: definition.codeLabel,
              render: (row: ReferenceCatalogRow) => <span className="code-text">{row.code}</span>,
            },
            {
              key: 'name',
              title: 'Tên',
              render: (row: ReferenceCatalogRow) => <strong className="table-primary">{row.name}</strong>,
            },
            ...(kind === 'manufacturers'
              ? [{
                  key: 'country',
                  title: 'Quốc gia',
                  render: (row: ReferenceCatalogRow) =>
                    countries.find((country) => country.countryId === row.countryId)?.countryName
                    ?? (row.countryId ? `#${row.countryId}` : '—'),
                }]
              : []),
            {
              key: 'status',
              title: 'Trạng thái',
              render: (row: ReferenceCatalogRow) => <StatusBadge active={row.isActive} />,
            },
            {
              key: 'actions',
              title: 'Thao tác',
              align: 'right' as const,
              render: (row: ReferenceCatalogRow) => (
                <div className="row-actions">
                  <button className="btn btn-small btn-secondary" onClick={() => setDetailRow(row)} type="button">
                    Xem
                  </button>
                  <button
                    className="btn btn-small btn-secondary"
                    disabled={manufacturerLookupUnavailable}
                    onClick={() => setDialogRow(row)}
                    type="button"
                  >
                    Sửa
                  </button>
                  <button
                    className="btn btn-small btn-danger-ghost"
                    onClick={() => void deleteRow(row)}
                    type="button"
                  >
                    Xóa
                  </button>
                </div>
              ),
            },
          ]}
          emptyMessage={search ? `Không tìm thấy ${definition.singular} phù hợp.` : `Chưa có ${definition.singular} nào.`}
          getKey={(row) => row.id}
          loading={loading}
          rows={visibleRows}
        />
        {!loading && filtered.length > pageSize ? (
          <div className="table-pagination">
            <span>Trang {page} / {pageCount}</span>
            <div className="row-actions">
              <button
                className="btn btn-small btn-secondary"
                disabled={page <= 1}
                onClick={() => setPage((current) => current - 1)}
                type="button"
              >
                Trước
              </button>
              <button
                className="btn btn-small btn-secondary"
                disabled={page >= pageCount}
                onClick={() => setPage((current) => current + 1)}
                type="button"
              >
                Sau
              </button>
            </div>
          </div>
        ) : null}
      </section>
      {dialogRow !== undefined ? (
        <CatalogDialog
          countries={countries}
          key={dialogRow?.id ?? 'new'}
          kind={kind}
          onClose={() => setDialogRow(undefined)}
          onSaved={saved}
          row={dialogRow}
        />
      ) : null}
      {detailRow ? (
        <Modal
          description={`Thông tin ${definition.singular} từ API.`}
          onClose={() => setDetailRow(null)}
          title={`Chi tiết ${definition.singular}`}
        >
          <dl className="detail-list">
            <div><dt>ID</dt><dd>{detailRow.id}</dd></div>
            <div><dt>{definition.codeLabel}</dt><dd>{detailRow.code}</dd></div>
            <div><dt>Tên</dt><dd>{detailRow.name}</dd></div>
            {kind === 'manufacturers' ? (
              <>
                <div>
                  <dt>Quốc gia</dt>
                  <dd>{countries.find((country) => country.countryId === detailRow.countryId)?.countryName ?? `#${detailRow.countryId}`}</dd>
                </div>
                <div><dt>Địa chỉ</dt><dd>{detailRow.address || '—'}</dd></div>
              </>
            ) : null}
            <div><dt>Trạng thái</dt><dd>{detailRow.isActive ? 'Đang hoạt động' : 'Ngừng hoạt động'}</dd></div>
          </dl>
        </Modal>
      ) : null}
    </>
  )
}
