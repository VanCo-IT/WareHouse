import { Link, useParams } from 'react-router-dom'
import { useState } from 'react'
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
  ProductResponse,
  ManufacturerResponse,
  SupplierContactCreateRequest,
  SupplierContactResponse,
  SupplierContactUpdateRequest,
  SupplierProductCreateRequest,
  SupplierProductResponse,
  SupplierProductUpdateRequest,
  SupplierResponse,
} from '../types/contracts'

type ContactDialog = SupplierContactResponse | null | undefined
type SupplierProductDialog = SupplierProductResponse | null | undefined

function ContactDialogForm({
  supplierId,
  contact,
  onClose,
  onSaved,
}: {
  supplierId: number
  contact: SupplierContactResponse | null
  onClose: () => void
  onSaved: () => void
}) {
  const [contactName, setContactName] = useState(contact?.contactName ?? '')
  const [jobTitle, setJobTitle] = useState(contact?.jobTitle ?? '')
  const [email, setEmail] = useState(contact?.email ?? '')
  const [phone, setPhone] = useState(contact?.phone ?? '')
  const [saving, setSaving] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [validationMessages, setValidationMessages] = useState<string[]>([])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setErrorMessage(null)
    setValidationMessages([])
    const name = contactName.trim()
    const values = {
      jobTitle: jobTitle.trim() || null,
      email: email.trim() || null,
      phone: phone.trim() || null,
    }
    const errors: string[] = []
    if (name.length === 0 || name.length > 120) errors.push('Tên liên hệ phải có từ 1 đến 120 ký tự.')
    if (values.jobTitle && values.jobTitle.length > 80) errors.push('Chức vụ không được vượt quá 80 ký tự.')
    if (values.email && values.email.length > 150) errors.push('Email không được vượt quá 150 ký tự.')
    if (values.phone && values.phone.length > 30) errors.push('Số điện thoại không được vượt quá 30 ký tự.')
    if (errors.length > 0) {
      setValidationMessages(errors)
      return
    }

    setSaving(true)
    try {
      if (contact) {
        const request: SupplierContactUpdateRequest = { contactName: name, ...values }
        await catalogApi.updateSupplierContact(supplierId, contact.contactId, request)
      } else {
        const request: SupplierContactCreateRequest = { contactName: name, ...values }
        await catalogApi.createSupplierContact(supplierId, request)
      }
      onSaved()
    } catch (error) {
      setErrorMessage(getErrorMessage(error))
      if (error instanceof ApiError) setValidationMessages(Object.values(error.validationErrors).flat())
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal onClose={onClose} title={contact ? 'Cập nhật liên hệ' : 'Thêm liên hệ'}>
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {errorMessage ? <ErrorBanner message={errorMessage} /> : null}
        <ValidationSummary messages={validationMessages} />
        <label className="form-field">
          <span>Họ tên <b>*</b></span>
          <input autoFocus maxLength={120} onChange={(event) => setContactName(event.target.value)} required value={contactName} />
        </label>
        <label className="form-field">
          <span>Chức vụ</span>
          <input maxLength={80} onChange={(event) => setJobTitle(event.target.value)} value={jobTitle} />
        </label>
        <label className="form-field">
          <span>Email</span>
          <input maxLength={150} onChange={(event) => setEmail(event.target.value)} type="email" value={email} />
        </label>
        <label className="form-field">
          <span>Số điện thoại</span>
          <input maxLength={30} onChange={(event) => setPhone(event.target.value)} value={phone} />
        </label>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">Hủy</button>
          <button className="btn btn-primary" disabled={saving} type="submit">{saving ? 'Đang lưu…' : 'Lưu liên hệ'}</button>
        </div>
      </form>
    </Modal>
  )
}

function SupplierProductDialogForm({
  supplierId,
  supplierProduct,
  products,
  manufacturers,
  onClose,
  onSaved,
}: {
  supplierId: number
  supplierProduct: SupplierProductResponse | null
  products: ProductResponse[]
  manufacturers: ManufacturerResponse[]
  onClose: () => void
  onSaved: () => void
}) {
  const [productId, setProductId] = useState(supplierProduct ? String(supplierProduct.productId) : '')
  const [manufacturerId, setManufacturerId] = useState(
    supplierProduct ? String(supplierProduct.manufacturerId) : '',
  )
  const [supplierProductCode, setSupplierProductCode] = useState(supplierProduct?.supplierProductCode ?? '')
  const [leadTimeDays, setLeadTimeDays] = useState(
    supplierProduct ? String(supplierProduct.leadTimeDays) : '0',
  )
  const [isActive, setIsActive] = useState(supplierProduct?.isActive ?? true)
  const [saving, setSaving] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [validationMessages, setValidationMessages] = useState<string[]>([])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setErrorMessage(null)
    setValidationMessages([])
    const product = Number(productId)
    const manufacturer = Number(manufacturerId)
    const leadTime = Number(leadTimeDays)
    const code = supplierProductCode.trim() || null
    const errors: string[] = []
    if (!supplierProduct && (!Number.isInteger(product) || product < 1)) errors.push('Vui lòng chọn sản phẩm hợp lệ.')
    if (!Number.isInteger(manufacturer) || manufacturer < 1) errors.push('Vui lòng chọn nhà sản xuất hợp lệ.')
    if (!Number.isInteger(leadTime) || leadTime < 0) errors.push('Thời gian cung ứng phải là số nguyên không âm.')
    if (code && code.length > 60) errors.push('Mã sản phẩm của nhà cung cấp không được vượt quá 60 ký tự.')
    if (errors.length > 0) {
      setValidationMessages(errors)
      return
    }

    setSaving(true)
    try {
      if (supplierProduct) {
        const request: SupplierProductUpdateRequest = {
          supplierProductCode: code,
          leadTimeDays: leadTime,
          isActive,
        }
        await catalogApi.updateSupplierProduct(
          supplierId,
          supplierProduct.productId,
          supplierProduct.manufacturerId,
          request,
        )
      } else {
        const request: SupplierProductCreateRequest = {
          productId: product,
          manufacturerId: manufacturer,
          supplierProductCode: code,
          leadTimeDays: leadTime,
          isActive,
        }
        await catalogApi.createSupplierProduct(supplierId, request)
      }
      onSaved()
    } catch (error) {
      setErrorMessage(getErrorMessage(error))
      if (error instanceof ApiError) setValidationMessages(Object.values(error.validationErrors).flat())
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal
      description="Sản phẩm và nhà sản xuất được chọn từ danh sách backend."
      onClose={onClose}
      title={supplierProduct ? 'Cập nhật liên kết sản phẩm' : 'Thêm sản phẩm cung ứng'}
    >
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {errorMessage ? <ErrorBanner message={errorMessage} /> : null}
        <ValidationSummary messages={validationMessages} />
        {!supplierProduct ? (
          <label className="form-field">
            <span>Sản phẩm <b>*</b></span>
            <select onChange={(event) => setProductId(event.target.value)} required value={productId}>
              <option value="">Chọn sản phẩm</option>
              {products.map((product) => (
                <option key={product.productId} value={product.productId}>
                  {product.sku} — {product.productName}
                </option>
              ))}
            </select>
          </label>
        ) : (
          <div className="readonly-field">
            <span>Sản phẩm</span>
            <strong>{supplierProduct.productSKU} — {supplierProduct.productName}</strong>
          </div>
        )}
        <label className="form-field">
          <span>Nhà sản xuất <b>*</b></span>
          <select
            disabled={Boolean(supplierProduct)}
            onChange={(event) => setManufacturerId(event.target.value)}
            required
            value={manufacturerId}
          >
            <option value="">Chọn nhà sản xuất</option>
            {manufacturers
              .filter((manufacturer) => manufacturer.isActive || manufacturer.manufacturerId === supplierProduct?.manufacturerId)
              .map((manufacturer) => (
                <option key={manufacturer.manufacturerId} value={manufacturer.manufacturerId}>
                  {manufacturer.manufacturerCode} — {manufacturer.manufacturerName}
                </option>
              ))}
          </select>
        </label>
        <label className="form-field">
          <span>Mã sản phẩm của nhà cung cấp</span>
          <input maxLength={60} onChange={(event) => setSupplierProductCode(event.target.value)} value={supplierProductCode} />
        </label>
        <label className="form-field">
          <span>Số ngày cung ứng</span>
          <input min={0} onChange={(event) => setLeadTimeDays(event.target.value)} required type="number" value={leadTimeDays} />
        </label>
        <label className="checkbox-field">
          <input checked={isActive} onChange={(event) => setIsActive(event.target.checked)} type="checkbox" />
          <span>Đang hoạt động</span>
        </label>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">Hủy</button>
          <button className="btn btn-primary" disabled={saving} type="submit">{saving ? 'Đang lưu…' : 'Lưu liên kết'}</button>
        </div>
      </form>
    </Modal>
  )
}

function SupplierDetailPage() {
  const { id: rawId } = useParams()
  const supplierId = Number(rawId)
  const validId = Number.isInteger(supplierId) && supplierId > 0
  const supplierQuery = useApiQuery<SupplierResponse>(validId ? apiRoutes.supplier(supplierId) : null)
  const contactsQuery = useApiQuery<SupplierContactResponse[]>(
    validId ? apiRoutes.supplierContacts(supplierId) : null,
  )
  const supplierProductsQuery = useApiQuery<SupplierProductResponse[]>(
    validId ? apiRoutes.supplierProducts(supplierId) : null,
  )
  const productsQuery = useApiQuery<ProductResponse[]>(validId ? apiRoutes.products : null)
  const manufacturersQuery = useApiQuery<ManufacturerResponse[]>(
    validId ? apiRoutes.manufacturers : null,
  )
  const [contactDialog, setContactDialog] = useState<ContactDialog>(undefined)
  const [productDialog, setProductDialog] = useState<SupplierProductDialog>(undefined)
  const [actionError, setActionError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const supplier = supplierQuery.data
  const contacts = contactsQuery.data ?? []
  const supplierProducts = supplierProductsQuery.data ?? []
  const products = productsQuery.data ?? []
  const manufacturers = manufacturersQuery.data ?? []

  async function deleteContact(contact: SupplierContactResponse) {
    if (!window.confirm(`Xóa liên hệ "${contact.contactName}"?`)) return
    setActionError(null)
    setNotice(null)
    try {
      await catalogApi.deleteSupplierContact(supplierId, contact.contactId)
      setNotice('Đã xóa liên hệ.')
      contactsQuery.reload()
    } catch (error) {
      setActionError(getErrorMessage(error))
    }
  }

  async function deleteSupplierProduct(item: SupplierProductResponse) {
    if (!window.confirm(`Gỡ sản phẩm "${item.productName}" khỏi nhà cung cấp?`)) return
    setActionError(null)
    setNotice(null)
    try {
      await catalogApi.deleteSupplierProduct(supplierId, item.productId, item.manufacturerId)
      setNotice('Đã gỡ liên kết sản phẩm.')
      supplierProductsQuery.reload()
    } catch (error) {
      setActionError(getErrorMessage(error))
    }
  }

  function contactSaved() {
    setContactDialog(undefined)
    setNotice(contactDialog ? 'Đã cập nhật liên hệ.' : 'Đã thêm liên hệ.')
    contactsQuery.reload()
  }

  function supplierProductSaved() {
    setProductDialog(undefined)
    setNotice(productDialog ? 'Đã cập nhật liên kết sản phẩm.' : 'Đã thêm liên kết sản phẩm.')
    supplierProductsQuery.reload()
  }

  const contactColumns = [
    {
      key: 'name',
      title: 'Liên hệ',
      render: (item: SupplierContactResponse) => (
        <div className="table-main-cell">
          <strong className="table-primary">{item.contactName}</strong>
          <small>{item.jobTitle ?? '—'}</small>
        </div>
      ),
    },
    {
      key: 'email',
      title: 'Email / Điện thoại',
      render: (item: SupplierContactResponse) => (
        <span className="contact-summary">
          {item.email ?? '—'}
          <small>{item.phone ?? ''}</small>
        </span>
      ),
    },
    {
      key: 'actions',
      title: '',
      align: 'right' as const,
      render: (item: SupplierContactResponse) => (
        <div className="row-actions">
          <button className="btn btn-small btn-secondary" onClick={() => setContactDialog(item)} type="button">Sửa</button>
          <button className="btn btn-small btn-danger-ghost" onClick={() => void deleteContact(item)} type="button">Xóa</button>
        </div>
      ),
    },
  ]

  const supplierProductColumns = [
    {
      key: 'product',
      title: 'Sản phẩm',
      render: (item: SupplierProductResponse) => (
        <div className="table-main-cell">
          <strong className="table-primary">{item.productName}</strong>
          <small className="code-text">{item.productSKU}</small>
        </div>
      ),
    },
    { key: 'manufacturer', title: 'Nhà sản xuất', render: (item: SupplierProductResponse) => item.manufacturerName },
    { key: 'lead', title: 'Thời gian cung ứng', render: (item: SupplierProductResponse) => `${item.leadTimeDays} ngày` },
    { key: 'status', title: 'Trạng thái', render: (item: SupplierProductResponse) => <StatusBadge active={item.isActive} /> },
    {
      key: 'actions',
      title: '',
      align: 'right' as const,
      render: (item: SupplierProductResponse) => (
        <div className="row-actions">
          <button className="btn btn-small btn-secondary" onClick={() => setProductDialog(item)} type="button">Sửa</button>
          <button className="btn btn-small btn-danger-ghost" onClick={() => void deleteSupplierProduct(item)} type="button">Gỡ</button>
        </div>
      ),
    },
  ]

  if (!validId) {
    return (
      <>
        <PageHeading description="Mã nhà cung cấp không hợp lệ." title="Không tìm thấy nhà cung cấp" />
        <Link className="btn btn-secondary" to="/suppliers">Quay lại danh sách</Link>
      </>
    )
  }

  return (
    <>
      <PageHeading
        action={<Link className="btn btn-secondary" to="/suppliers">← Danh sách nhà cung cấp</Link>}
        description="Thông tin liên hệ và các sản phẩm được nhà cung cấp cung ứng."
        eyebrow="Đối tác"
        title={supplier?.supplierName ?? (supplierQuery.loading ? 'Đang tải…' : 'Chi tiết nhà cung cấp')}
      />
      {supplierQuery.error ? <ErrorBanner message={supplierQuery.error} onRetry={supplierQuery.reload} /> : null}
      {actionError ? <ErrorBanner message={actionError} /> : null}
      {notice ? <NoticeBanner message={notice} onDismiss={() => setNotice(null)} /> : null}
      {supplier ? (
        <section className="panel supplier-summary-panel">
          <div className="supplier-summary-avatar">{supplier.supplierName.trim().charAt(0).toLocaleUpperCase('vi')}</div>
          <div className="supplier-summary-main">
            <div className="eyebrow">{supplier.supplierCode}</div>
            <h2>{supplier.supplierName}</h2>
            <div className="supplier-summary-items">
              <span>{supplier.email ?? 'Chưa có email'}</span>
              <span>{supplier.phone ?? 'Chưa có số điện thoại'}</span>
              <span>CountryId: {supplier.countryId}</span>
            </div>
          </div>
          <StatusBadge active={supplier.isActive} />
        </section>
      ) : null}
      <div className="detail-grid">
        <section className="panel data-panel">
          <div className="section-heading">
            <div>
              <h2>Danh bạ liên hệ</h2>
              <p>{contactsQuery.loading ? 'Đang tải…' : `${contacts.length} liên hệ`}</p>
            </div>
            <button className="btn btn-primary btn-small" onClick={() => setContactDialog(null)} type="button">
              + Thêm liên hệ
            </button>
          </div>
          {contactsQuery.error ? <ErrorBanner message={contactsQuery.error} onRetry={contactsQuery.reload} /> : null}
          <DataTable
            columns={contactColumns}
            emptyMessage="Chưa có liên hệ nào."
            getKey={(item) => item.contactId}
            loading={contactsQuery.loading}
            rows={contacts}
          />
        </section>
        <section className="panel data-panel">
          <div className="section-heading">
            <div>
              <h2>Sản phẩm cung ứng</h2>
              <p>{supplierProductsQuery.loading ? 'Đang tải…' : `${supplierProducts.length} sản phẩm`}</p>
            </div>
            <button
              className="btn btn-primary btn-small"
              disabled={Boolean(productsQuery.error || manufacturersQuery.error)}
              onClick={() => setProductDialog(null)}
              type="button"
            >
              + Liên kết sản phẩm
            </button>
          </div>
          {supplierProductsQuery.error ? (
            <ErrorBanner message={supplierProductsQuery.error} onRetry={supplierProductsQuery.reload} />
          ) : null}
          {productsQuery.error ? <ErrorBanner message={`Không tải được sản phẩm: ${productsQuery.error}`} onRetry={productsQuery.reload} /> : null}
          {manufacturersQuery.error ? (
            <ErrorBanner
              message={`Không tải được nhà sản xuất: ${manufacturersQuery.error}`}
              onRetry={manufacturersQuery.reload}
            />
          ) : null}
          <DataTable
            columns={supplierProductColumns}
            emptyMessage="Chưa có sản phẩm cung ứng."
            getKey={(item) => item.supplierProductId}
            loading={supplierProductsQuery.loading}
            rows={supplierProducts}
          />
        </section>
      </div>
      {contactDialog !== undefined ? (
        <ContactDialogForm
          contact={contactDialog}
          key={contactDialog?.contactId ?? 'new'}
          onClose={() => setContactDialog(undefined)}
          onSaved={contactSaved}
          supplierId={supplierId}
        />
      ) : null}
      {productDialog !== undefined ? (
        <SupplierProductDialogForm
          key={productDialog?.supplierProductId ?? 'new'}
          onClose={() => setProductDialog(undefined)}
          onSaved={supplierProductSaved}
          products={products}
          manufacturers={manufacturers}
          supplierId={supplierId}
          supplierProduct={productDialog}
        />
      ) : null}
    </>
  )
}

export default SupplierDetailPage
