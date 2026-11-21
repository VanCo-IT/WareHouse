import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import {
  DataTable,
  ErrorBanner,
  Modal,
  NoticeBanner,
  PageHeading,
  ValidationSummary,
} from '../components/Ui'
import { useApiQuery } from '../hooks/useApiQuery'
import { ApiError, getErrorMessage } from '../services/apiClient'
import { apiRoutes } from '../services/catalogApi'
import { receivingApi, receivingRoutes } from '../services/receivingApi'
import { warehouseRoutes } from '../services/warehouseApi'
import type {
  ConfirmGoodsReceiptRequest,
  CreatePurchaseOrderLineRequest,
  CreatePurchaseOrderRequest,
  CountryResponse,
  CurrencyResponse,
  GoodsReceiptCheckInLineRequest,
  GoodsReceiptListItemResponse,
  GoodsReceiptPutawayItemRequest,
  GoodsReceiptResponse,
  ManufacturerResponse,
  ProductResponse,
  ProductUnitOptionResponse,
  PurchaseOrderResponse,
  ReceivingBinOptionResponse,
  SaveQualityInspectionCheckRequest,
  SupplierResponse,
  WarehouseResponse,
} from '../types/contracts'

type ReceivingTab = 'receipts' | 'orders'

const EMPTY_ORDERS: PurchaseOrderResponse[] = []
const EMPTY_RECEIPTS: GoodsReceiptListItemResponse[] = []

function statusText(status: string) {
  const labels: Record<string, string> = {
    Draft: 'Nháp',
    Submitted: 'Chờ duyệt',
    Approved: 'Đã duyệt',
    PartiallyReceived: 'Nhận một phần',
    Received: 'Đã nhận đủ',
    Checking: 'Đang kiểm nhận',
    Accepted: 'Đã nhập kho',
    PartiallyAccepted: 'Nhập một phần',
    Rejected: 'Từ chối',
    Cancelled: 'Đã hủy',
    Passed: 'Đạt',
    Failed: 'Không đạt',
    Pending: 'Chờ kiểm tra',
    Conditional: 'Có điều kiện',
  }
  return labels[status] ?? status
}

function formatQuantity(value: number) {
  return new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 3 }).format(value)
}

function formatPrice(value: number, currencyCode: string) {
  return `${new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 4 }).format(value)} ${currencyCode}`
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium' }).format(new Date(value))
}

interface PurchaseOrderLineEditorProps {
  index: number
  products: ProductResponse[]
  line: { productId: string; unitId: string; quantity: string; unitPrice: string }
  onChange: (index: number, line: PurchaseOrderLineEditorProps['line']) => void
  onRemove: (index: number) => void
  canRemove: boolean
}

function PurchaseOrderLineEditor({
  index,
  products,
  line,
  onChange,
  onRemove,
  canRemove,
}: PurchaseOrderLineEditorProps) {
  const productId = Number(line.productId)
  const unitsQuery = useApiQuery<ProductUnitOptionResponse[]>(
    productId > 0 ? receivingRoutes.productUnits(productId) : null,
  )

  return (
    <div className="receiving-order-line">
      <label className="form-field">
        <span>Sản phẩm <b>*</b></span>
        <select
          onChange={(event) => onChange(index, { ...line, productId: event.target.value, unitId: '' })}
          required
          value={line.productId}
        >
          <option value="">Chọn sản phẩm</option>
          {products.filter((product) => product.isActive).map((product) => (
            <option key={product.productId} value={product.productId}>{product.sku} — {product.productName}</option>
          ))}
        </select>
      </label>
      <label className="form-field">
        <span>Đơn vị <b>*</b></span>
        <select
          disabled={productId <= 0 || unitsQuery.loading}
          onChange={(event) => onChange(index, { ...line, unitId: event.target.value })}
          required
          value={line.unitId}
        >
          <option value="">{unitsQuery.loading ? 'Đang tải đơn vị…' : 'Chọn đơn vị'}</option>
          {(unitsQuery.data ?? []).map((unit) => (
            <option key={unit.unitId} value={unit.unitId}>{unit.unitCode} — {unit.unitName}</option>
          ))}
        </select>
        {unitsQuery.error ? (
          <div className="receiving-lookup-error">
            <small className="field-error">{unitsQuery.error}</small>
            <button className="text-button" onClick={unitsQuery.reload} type="button">Thử tải lại</button>
          </div>
        ) : null}
      </label>
      <label className="form-field">
        <span>Số lượng đặt <b>*</b></span>
        <input
          min="0.001"
          onChange={(event) => onChange(index, { ...line, quantity: event.target.value })}
          required
          step="0.001"
          type="number"
          value={line.quantity}
        />
      </label>
      <label className="form-field">
        <span>Đơn giá <b>*</b></span>
        <input
          min="0"
          onChange={(event) => onChange(index, { ...line, unitPrice: event.target.value })}
          required
          step="0.0001"
          type="number"
          value={line.unitPrice}
        />
      </label>
      <button
        aria-label="Xóa dòng sản phẩm"
        className="btn btn-small btn-danger-ghost"
        disabled={!canRemove}
        onClick={() => onRemove(index)}
        type="button"
      >
        Xóa
      </button>
    </div>
  )
}

function CreatePurchaseOrderDialog({
  suppliers,
  products,
  currencies,
  onClose,
  onSaved,
}: {
  suppliers: SupplierResponse[]
  products: ProductResponse[]
  currencies: CurrencyResponse[]
  onClose: () => void
  onSaved: () => void
}) {
  const [supplierId, setSupplierId] = useState('')
  const [currencyId, setCurrencyId] = useState('')
  const [exchangeRate, setExchangeRate] = useState('')
  const [orderDate, setOrderDate] = useState('')
  const [expectedDate, setExpectedDate] = useState('')
  const [note, setNote] = useState('')
  const [lines, setLines] = useState([{ productId: '', unitId: '', quantity: '', unitPrice: '' }])
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [validationMessages, setValidationMessages] = useState<string[]>([])

  function updateLine(index: number, line: typeof lines[number]) {
    setLines((current) => current.map((item, itemIndex) => itemIndex === index ? line : item))
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setValidationMessages([])

    const requestLines: CreatePurchaseOrderLineRequest[] = lines.map((line) => ({
      productId: Number(line.productId),
      unitId: Number(line.unitId),
      orderedQuantity: Number(line.quantity),
      unitPrice: Number(line.unitPrice),
    }))
    const errors: string[] = []
    if (!Number.isInteger(Number(supplierId)) || Number(supplierId) < 1) errors.push('Chọn nhà cung cấp hợp lệ.')
    if (!Number.isInteger(Number(currencyId)) || Number(currencyId) < 1) errors.push('Chọn tiền tệ hợp lệ.')
    if (!Number.isFinite(Number(exchangeRate)) || Number(exchangeRate) <= 0) errors.push('Tỷ giá phải lớn hơn 0.')
    if (requestLines.some((line) =>
      !Number.isInteger(line.productId) || line.productId < 1
      || !Number.isInteger(line.unitId) || line.unitId < 1
      || !Number.isFinite(line.orderedQuantity) || line.orderedQuantity <= 0
      || !Number.isFinite(line.unitPrice) || line.unitPrice < 0)) {
      errors.push('Mỗi dòng cần có sản phẩm, đơn vị, số lượng dương và đơn giá hợp lệ.')
    }
    if (expectedDate && expectedDate < orderDate) errors.push('Ngày dự kiến không được trước ngày đặt.')
    if (lines.some((line) => !line.productId || !line.unitId || !line.quantity || !line.unitPrice)) {
      errors.push('Vui lòng hoàn tất đầy đủ các dòng sản phẩm.')
    }
    if (errors.length > 0) {
      setValidationMessages(errors)
      return
    }

    const request: CreatePurchaseOrderRequest = {
      supplierId: Number(supplierId),
      currencyId: Number(currencyId),
      exchangeRate: Number(exchangeRate),
      orderDate: new Date(`${orderDate}T00:00:00`).toISOString(),
      expectedDate: expectedDate || null,
      createdByEmployeeId: null,
      note: note.trim() || null,
      lines: requestLines,
    }

    setSaving(true)
    try {
      await receivingApi.createPurchaseOrder(request)
      onSaved()
    } catch (saveError) {
      setError(getErrorMessage(saveError))
      if (saveError instanceof ApiError) setValidationMessages(Object.values(saveError.validationErrors).flat())
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal
      description="Tạo đơn nháp từ nhà cung cấp, sản phẩm, đơn vị, tiền tệ và giá do người dùng cung cấp."
      onClose={onClose}
      title="Tạo đơn mua hàng"
      wide
    >
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {error ? <ErrorBanner message={error} /> : null}
        <ValidationSummary messages={validationMessages} />
        <div className="form-grid">
          <label className="form-field">
            <span>Nhà cung cấp <b>*</b></span>
            <select onChange={(event) => setSupplierId(event.target.value)} required value={supplierId}>
              <option value="">Chọn nhà cung cấp</option>
              {suppliers.filter((supplier) => supplier.isActive).map((supplier) => (
                <option key={supplier.supplierId} value={supplier.supplierId}>
                  {supplier.supplierCode} — {supplier.supplierName}
                </option>
              ))}
            </select>
          </label>
          <label className="form-field">
            <span>Tiền tệ <b>*</b></span>
            <select onChange={(event) => setCurrencyId(event.target.value)} required value={currencyId}>
              <option value="">Chọn tiền tệ</option>
              {currencies.filter((currency) => currency.isActive).map((currency) => (
                <option key={currency.currencyId} value={currency.currencyId}>
                  {currency.currencyCode} — {currency.currencyName}
                </option>
              ))}
            </select>
          </label>
          <label className="form-field">
            <span>Tỷ giá <b>*</b></span>
            <input
              min="0.000001"
              onChange={(event) => setExchangeRate(event.target.value)}
              required
              step="0.000001"
              type="number"
              value={exchangeRate}
            />
            <small>Nhập tỷ giá đã được thống nhất cho đơn này; hệ thống không tự suy đoán tỷ giá.</small>
          </label>
          <label className="form-field">
            <span>Ngày đặt <b>*</b></span>
            <input onChange={(event) => setOrderDate(event.target.value)} required type="date" value={orderDate} />
          </label>
          <label className="form-field">
            <span>Ngày dự kiến</span>
            <input onChange={(event) => setExpectedDate(event.target.value)} type="date" value={expectedDate} />
          </label>
          <label className="form-field">
            <span>Ghi chú</span>
            <input maxLength={500} onChange={(event) => setNote(event.target.value)} value={note} />
          </label>
        </div>
        <section className="receiving-lines">
          <div className="receiving-section-heading">
            <h3>Sản phẩm đặt mua</h3>
            <button
              className="btn btn-secondary btn-small"
              onClick={() => setLines((current) => [...current, { productId: '', unitId: '', quantity: '', unitPrice: '' }])}
              type="button"
            >
              + Thêm dòng
            </button>
          </div>
          {lines.map((line, index) => (
            <PurchaseOrderLineEditor
              canRemove={lines.length > 1}
              index={index}
              key={index}
              line={line}
              onChange={updateLine}
              onRemove={(removeIndex) => setLines((current) => current.filter((_, itemIndex) => itemIndex !== removeIndex))}
              products={products}
            />
          ))}
        </section>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">Hủy</button>
          <button className="btn btn-primary" disabled={saving} type="submit">{saving ? 'Đang lưu…' : 'Tạo đơn nháp'}</button>
        </div>
      </form>
    </Modal>
  )
}

function CreateReceiptDialog({
  orders,
  warehouses,
  onClose,
  onSaved,
}: {
  orders: PurchaseOrderResponse[]
  warehouses: WarehouseResponse[]
  onClose: () => void
  onSaved: () => void
}) {
  const eligibleOrders = orders.filter((order) => order.status === 'Approved' || order.status === 'PartiallyReceived')
  const activeWarehouses = warehouses.filter((warehouse) => warehouse.isActive)
  const [purchaseOrderId, setPurchaseOrderId] = useState('')
  const [warehouseId, setWarehouseId] = useState('')
  const [note, setNote] = useState('')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setSaving(true)
    try {
      await receivingApi.createReceipt({
        purchaseOrderId: Number(purchaseOrderId),
        warehouseId: Number(warehouseId),
        receivedByEmployeeId: null,
        note: note.trim() || null,
      })
      onSaved()
    } catch (saveError) {
      setError(getErrorMessage(saveError))
    } finally {
      setSaving(false)
    }
  }

  const selectedOrder = eligibleOrders.find((order) => order.purchaseOrderId === Number(purchaseOrderId))

  return (
    <Modal description="Phiếu nháp được tạo từ đơn mua đã duyệt; tồn kho chưa thay đổi." onClose={onClose} title="Tạo phiếu nhập">
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {error ? <ErrorBanner message={error} /> : null}
        {eligibleOrders.length === 0 ? (
          <div className="empty-state">
            <strong>Chưa có đơn mua đủ điều kiện</strong>
            <span>Tạo đơn mua, gửi duyệt và duyệt đơn trước khi nhận hàng.</span>
          </div>
        ) : activeWarehouses.length === 0 ? (
          <div className="empty-state">
            <strong>Chưa có kho đang hoạt động</strong>
            <span>Không thể tạo phiếu nhập cho đến khi API trả về ít nhất một kho hoạt động.</span>
          </div>
        ) : (
          <>
            <label className="form-field">
              <span>Đơn mua <b>*</b></span>
              <select onChange={(event) => setPurchaseOrderId(event.target.value)} required value={purchaseOrderId}>
                <option value="">Chọn đơn mua đã duyệt</option>
                {eligibleOrders.map((order) => (
                  <option key={order.purchaseOrderId} value={order.purchaseOrderId}>
                    {order.poNumber} — {order.supplierName} ({statusText(order.status)})
                  </option>
                ))}
              </select>
            </label>
            {selectedOrder ? (
              <section className="receiving-order-summary">
                <strong>{selectedOrder.supplierName} · {selectedOrder.currencyCode} · tỷ giá {selectedOrder.exchangeRate}</strong>
                {selectedOrder.lines.map((line) => (
                  <div key={line.purchaseOrderDetailId}>
                    <span>{line.sku} — {line.productName}</span>
                    <span>
                      Còn theo đơn: {formatQuantity(Math.max(0, line.orderedQuantity - line.previouslyAcceptedQuantity))} {line.unitCode}
                      {' · '}Đơn giá: {formatPrice(line.unitPrice, selectedOrder.currencyCode)}
                    </span>
                  </div>
                ))}
              </section>
            ) : null}
            <label className="form-field">
              <span>Kho nhận <b>*</b></span>
              <select onChange={(event) => setWarehouseId(event.target.value)} required value={warehouseId}>
                <option value="">Chọn kho</option>
                {activeWarehouses.map((warehouse) => (
                  <option key={warehouse.warehouseId} value={warehouse.warehouseId}>
                    {warehouse.warehouseCode} — {warehouse.warehouseName}
                  </option>
                ))}
              </select>
            </label>
            <label className="form-field">
              <span>Ghi chú</span>
              <textarea maxLength={500} onChange={(event) => setNote(event.target.value)} rows={2} value={note} />
            </label>
            <div className="modal-actions">
              <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">Hủy</button>
              <button className="btn btn-primary" disabled={saving || activeWarehouses.length === 0} type="submit">{saving ? 'Đang tạo…' : 'Tạo phiếu nháp'}</button>
            </div>
          </>
        )}
      </form>
    </Modal>
  )
}

function CheckInDialog({
  receipt,
  onClose,
  onSaved,
}: {
  receipt: GoodsReceiptResponse
  onClose: () => void
  onSaved: () => void
}) {
  const [quantities, setQuantities] = useState(() => Object.fromEntries(
    receipt.lines.map((line) => [line.goodsReceiptDetailId, {
      received: String(line.receivedQuantity),
      accepted: String(line.acceptedQuantity),
      rejected: String(line.rejectedQuantity),
    }]),
  ))
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    const lines: GoodsReceiptCheckInLineRequest[] = receipt.lines.map((line) => ({
      goodsReceiptDetailId: line.goodsReceiptDetailId,
      receivedQuantity: Number(quantities[line.goodsReceiptDetailId]?.received ?? 0),
      acceptedQuantity: Number(quantities[line.goodsReceiptDetailId]?.accepted ?? 0),
      rejectedQuantity: Number(quantities[line.goodsReceiptDetailId]?.rejected ?? 0),
    }))
    if (lines.some((line) =>
      !Number.isFinite(line.receivedQuantity)
      || !Number.isFinite(line.acceptedQuantity)
      || !Number.isFinite(line.rejectedQuantity)
      || line.receivedQuantity < 0
      || line.acceptedQuantity < 0
      || line.rejectedQuantity < 0
      || line.acceptedQuantity + line.rejectedQuantity > line.receivedQuantity)) {
      setError('Số lượng phải không âm; chấp nhận cộng từ chối không được vượt quá số thực nhận.')
      return
    }

    setSaving(true)
    try {
      await receivingApi.saveCheckIn(receipt.goodsReceiptId, { lines })
      onSaved()
    } catch (saveError) {
      setError(getErrorMessage(saveError))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal description="Ghi nhận số thực nhận và phân loại đạt/từ chối; chưa cập nhật tồn kho." onClose={onClose} title="Kiểm nhận hàng" wide>
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {error ? <ErrorBanner message={error} /> : null}
        <div className="table-scroll">
          <table className="data-table">
            <thead><tr><th>Sản phẩm</th><th>Đặt</th><th>Thực nhận</th><th>Chấp nhận</th><th>Từ chối</th></tr></thead>
            <tbody>
              {receipt.lines.map((line) => {
                const values = quantities[line.goodsReceiptDetailId]
                return (
                  <tr key={line.goodsReceiptDetailId}>
                    <td>{line.sku} — {line.productName}</td>
                    <td>{formatQuantity(line.orderedQuantity)} {line.unitCode}</td>
                    {(['received', 'accepted', 'rejected'] as const).map((key) => (
                      <td key={key}>
                        <input
                          aria-label={`${key} ${line.sku}`}
                          min="0"
                          onChange={(event) => setQuantities((current) => ({
                            ...current,
                            [line.goodsReceiptDetailId]: {
                              ...current[line.goodsReceiptDetailId],
                              [key]: event.target.value,
                            },
                          }))}
                          step="0.001"
                          type="number"
                          value={values[key]}
                        />
                      </td>
                    ))}
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
        <p className="form-help">Có thể lưu kiểm nhận trước khi phân loại hết; cần phân loại toàn bộ số thực nhận trước khi lưu kiểm tra chất lượng.</p>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">Hủy</button>
          <button className="btn btn-primary" disabled={saving} type="submit">{saving ? 'Đang lưu…' : 'Lưu kiểm nhận'}</button>
        </div>
      </form>
    </Modal>
  )
}

function QualityInspectionDialog({
  receipt,
  onClose,
  onSaved,
}: {
  receipt: GoodsReceiptResponse
  onClose: () => void
  onSaved: () => void
}) {
  const [checks, setChecks] = useState<SaveQualityInspectionCheckRequest[]>(() =>
    receipt.lines.filter((line) => line.receivedQuantity > 0).map((line) => {
      const current = receipt.qualityInspection?.checks.find(
        (check) => check.goodsReceiptDetailId === line.goodsReceiptDetailId,
      )
      return {
        goodsReceiptDetailId: line.goodsReceiptDetailId,
        checkName: current?.checkName ?? '',
        result: (current?.result as SaveQualityInspectionCheckRequest['result'] | undefined) ?? 'Pending',
        note: current?.note ?? null,
      }
    }),
  )
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    if (checks.some((check) => !check.checkName.trim())) {
      setError('Nhập tên nội dung kiểm tra cho từng dòng nhận.')
      return
    }
    setSaving(true)
    try {
      await receivingApi.saveQualityInspection(receipt.goodsReceiptId, {
        inspectorEmployeeId: null,
        note: null,
        checks: checks.map((check) => ({ ...check, checkName: check.checkName.trim() })),
      })
      onSaved()
    } catch (saveError) {
      setError(getErrorMessage(saveError))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal description="Kết quả chất lượng được lưu; chỉ kết quả Đạt mới cho phép đưa lượng chấp nhận vào tồn kho." onClose={onClose} title="Kiểm tra chất lượng" wide>
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {error ? <ErrorBanner message={error} /> : null}
        {checks.length === 0 ? <ErrorBanner message="Chưa có dòng thực nhận để kiểm tra chất lượng." /> : null}
        <div className="table-scroll">
          <table className="data-table">
            <thead><tr><th>Sản phẩm</th><th>Nội dung kiểm tra</th><th>Kết quả</th></tr></thead>
            <tbody>
              {checks.map((check, index) => {
                const line = receipt.lines.find((item) => item.goodsReceiptDetailId === check.goodsReceiptDetailId)
                return (
                  <tr key={check.goodsReceiptDetailId}>
                    <td>{line?.sku} — {line?.productName}</td>
                    <td>
                      <input
                        aria-label={`Nội dung kiểm tra ${line?.sku ?? ''}`}
                        maxLength={120}
                        onChange={(event) => setChecks((current) =>
                          current.map((item, itemIndex) => itemIndex === index
                            ? { ...item, checkName: event.target.value }
                            : item))}
                        required
                        value={check.checkName}
                      />
                    </td>
                    <td>
                      <select
                        aria-label={`Kết quả kiểm tra ${line?.sku ?? ''}`}
                        onChange={(event) => setChecks((current) =>
                          current.map((item, itemIndex) => itemIndex === index
                            ? { ...item, result: event.target.value as SaveQualityInspectionCheckRequest['result'] }
                            : item))}
                        value={check.result}
                      >
                        <option value="Pending">Chờ kiểm tra</option>
                        <option value="Pass">Đạt</option>
                        <option value="Fail">Không đạt</option>
                        <option value="NotApplicable">Không áp dụng</option>
                      </select>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">Hủy</button>
          <button className="btn btn-primary" disabled={saving || checks.length === 0} type="submit">
            {saving ? 'Đang lưu…' : 'Lưu kết quả'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

function ConfirmReceiptDialog({
  receipt,
  manufacturers,
  countries,
  onClose,
  onSaved,
}: {
  receipt: GoodsReceiptResponse
  manufacturers: ManufacturerResponse[]
  countries: CountryResponse[]
  onClose: () => void
  onSaved: () => void
}) {
  const binsQuery = useApiQuery<ReceivingBinOptionResponse[]>(
    receivingRoutes.warehouseBins(receipt.warehouseId),
  )
  const acceptedLines = receipt.lines.filter((line) => line.acceptedQuantity > 0)
  const [putaway, setPutaway] = useState<Record<number, {
    lotCode: string
    manufacturerId: string
    countryId: string
    binId: string
    manufactureDate: string
    expiryDate: string
    supplierLotCode: string
  }>>({})
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  function update(
    detailId: number,
    field: keyof NonNullable<typeof putaway[number]>,
    value: string,
  ) {
    setPutaway((current) => ({
      ...current,
      [detailId]: {
        ...(current[detailId] ?? {
          lotCode: '',
          manufacturerId: '',
          countryId: '',
          binId: '',
          manufactureDate: '',
          expiryDate: '',
          supplierLotCode: '',
        }),
        [field]: value,
      },
    }))
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    const items: GoodsReceiptPutawayItemRequest[] = acceptedLines.map((line) => ({
      goodsReceiptDetailId: line.goodsReceiptDetailId,
      quantity: line.acceptedQuantity,
      lotCode: putaway[line.goodsReceiptDetailId]?.lotCode.trim() ?? '',
      manufacturerId: Number(putaway[line.goodsReceiptDetailId]?.manufacturerId),
      countryId: Number(putaway[line.goodsReceiptDetailId]?.countryId),
      binId: Number(putaway[line.goodsReceiptDetailId]?.binId),
      manufactureDate: putaway[line.goodsReceiptDetailId]?.manufactureDate || null,
      expiryDate: putaway[line.goodsReceiptDetailId]?.expiryDate || null,
      supplierLotCode: putaway[line.goodsReceiptDetailId]?.supplierLotCode.trim() || null,
    }))
    if (items.some((item) => !item.lotCode || item.manufacturerId < 1 || item.countryId < 1 || item.binId < 1)) {
      setError('Mỗi dòng được chấp nhận cần mã lô, nhà sản xuất, xuất xứ và vị trí lưu trữ.')
      return
    }

    setSaving(true)
    try {
      const request: ConfirmGoodsReceiptRequest = { putaway: items }
      await receivingApi.confirmReceipt(receipt.goodsReceiptId, request)
      onSaved()
    } catch (saveError) {
      setError(getErrorMessage(saveError))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal
      description="Xác nhận sẽ tạo lô, số dư và giao dịch nhập kho trong một transaction. Số lượng tồn dùng đơn vị cơ sở."
      onClose={onClose}
      title="Xác nhận nhập kho"
      wide
    >
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {error ? <ErrorBanner message={error} /> : null}
        {binsQuery.error ? <ErrorBanner message={binsQuery.error} onRetry={binsQuery.reload} /> : null}
        {!binsQuery.loading && !binsQuery.error && (binsQuery.data?.length ?? 0) === 0 ? (
          <div className="alert alert-info">Kho này chưa có vị trí đang hoạt động để put-away.</div>
        ) : null}
        {acceptedLines.length > 0 && manufacturers.filter((manufacturer) => manufacturer.isActive).length === 0 ? (
          <div className="alert alert-info">Chưa có nhà sản xuất hoạt động để ghi nhận lô hàng.</div>
        ) : null}
        {acceptedLines.length > 0 && countries.filter((country) => country.isActive).length === 0 ? (
          <div className="alert alert-info">Chưa có quốc gia hoạt động để ghi nhận xuất xứ lô hàng.</div>
        ) : null}
        <div className="receiving-putaway-list">
          {acceptedLines.map((line) => {
            const values = putaway[line.goodsReceiptDetailId]
            return (
              <section className="receiving-putaway-card" key={line.goodsReceiptDetailId}>
                <strong>{line.sku} — {line.productName}</strong>
                <span>Chấp nhận: {formatQuantity(line.acceptedQuantity)} {line.unitCode}</span>
                <div className="form-grid">
                  <label className="form-field">
                    <span>Mã lô <b>*</b></span>
                    <input maxLength={60} onChange={(event) => update(line.goodsReceiptDetailId, 'lotCode', event.target.value)} required value={values?.lotCode ?? ''} />
                  </label>
                  <label className="form-field">
                    <span>Vị trí <b>*</b></span>
                    <select
                      onChange={(event) => update(line.goodsReceiptDetailId, 'binId', event.target.value)}
                      required
                      value={values?.binId ?? ''}
                    >
                      <option value="">{binsQuery.loading ? 'Đang tải vị trí…' : 'Chọn vị trí'}</option>
                      {(binsQuery.data ?? []).map((bin) => (
                        <option key={bin.binId} value={bin.binId}>
                          {bin.warehouseCode}/{bin.zoneCode}/{bin.aisleCode}/{bin.rackCode}/{bin.shelfCode}/{bin.binCode}
                        </option>
                      ))}
                    </select>
                  </label>
                  <label className="form-field">
                    <span>Nhà sản xuất <b>*</b></span>
                    <select
                      onChange={(event) => update(line.goodsReceiptDetailId, 'manufacturerId', event.target.value)}
                      required
                      value={values?.manufacturerId ?? ''}
                    >
                      <option value="">Chọn nhà sản xuất</option>
                      {manufacturers.filter((manufacturer) => manufacturer.isActive).map((manufacturer) => (
                        <option key={manufacturer.manufacturerId} value={manufacturer.manufacturerId}>{manufacturer.manufacturerName}</option>
                      ))}
                    </select>
                  </label>
                  <label className="form-field">
                    <span>Quốc gia xuất xứ <b>*</b></span>
                    <select
                      onChange={(event) => update(line.goodsReceiptDetailId, 'countryId', event.target.value)}
                      required
                      value={values?.countryId ?? ''}
                    >
                      <option value="">Chọn quốc gia</option>
                      {countries.filter((country) => country.isActive).map((country) => (
                        <option key={country.countryId} value={country.countryId}>{country.countryCode} — {country.countryName}</option>
                      ))}
                    </select>
                  </label>
                  <label className="form-field">
                    <span>Mã lô nhà cung cấp</span>
                    <input maxLength={60} onChange={(event) => update(line.goodsReceiptDetailId, 'supplierLotCode', event.target.value)} value={values?.supplierLotCode ?? ''} />
                  </label>
                  <label className="form-field">
                    <span>Ngày sản xuất</span>
                    <input onChange={(event) => update(line.goodsReceiptDetailId, 'manufactureDate', event.target.value)} type="date" value={values?.manufactureDate ?? ''} />
                  </label>
                  <label className="form-field">
                    <span>Hạn sử dụng</span>
                    <input onChange={(event) => update(line.goodsReceiptDetailId, 'expiryDate', event.target.value)} type="date" value={values?.expiryDate ?? ''} />
                  </label>
                </div>
              </section>
            )
          })}
        </div>
        {acceptedLines.length === 0 ? <p>Không có số lượng được chấp nhận để nhập vào tồn kho.</p> : null}
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">Hủy</button>
          <button
            className="btn btn-primary"
            disabled={
              saving
              || binsQuery.loading
              || binsQuery.error !== null
              || (binsQuery.data?.length ?? 0) === 0
              || manufacturers.filter((manufacturer) => manufacturer.isActive).length === 0
              || countries.filter((country) => country.isActive).length === 0
              || acceptedLines.length === 0
            }
            type="submit"
          >
            {saving ? 'Đang xác nhận…' : 'Xác nhận và cập nhật tồn'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

function ReceiptDetailDialog({
  receipt,
  manufacturers,
  countries,
  onClose,
  onChanged,
}: {
  receipt: GoodsReceiptResponse
  manufacturers: ManufacturerResponse[]
  countries: CountryResponse[]
  onClose: () => void
  onChanged: () => void
}) {
  const [dialog, setDialog] = useState<'checkin' | 'quality' | 'confirm' | null>(null)
  const [error, setError] = useState<string | null>(null)
  const canEdit = receipt.status === 'Draft' || receipt.status === 'Checking'
  const hasInspection = receipt.qualityInspection !== null

  async function cancelReceipt() {
    if (!window.confirm(`Hủy phiếu nhập ${receipt.receiptNumber}?`)) return
    setError(null)
    try {
      await receivingApi.cancelReceipt(receipt.goodsReceiptId)
      onChanged()
      onClose()
    } catch (cancelError) {
      setError(getErrorMessage(cancelError))
    }
  }

  return (
    <>
      <Modal description={`${receipt.receiptNumber} · Đơn mua ${receipt.poNumber}`} onClose={onClose} title="Chi tiết phiếu nhập" wide>
        {error ? <ErrorBanner message={error} /> : null}
        <dl className="detail-list receiving-detail">
          <div><dt>Nhà cung cấp</dt><dd>{receipt.supplierName}</dd></div>
          <div><dt>Kho nhận</dt><dd>{receipt.warehouseCode} — {receipt.warehouseName}</dd></div>
          <div><dt>Ngày tạo</dt><dd>{formatDate(receipt.receivedAt)}</dd></div>
          <div><dt>Trạng thái</dt><dd>{statusText(receipt.status)}</dd></div>
        </dl>
        <div className="table-scroll">
          <table className="data-table">
            <thead><tr><th>Sản phẩm</th><th>Đơn giá</th><th>Đặt</th><th>Đã nhận</th><th>Chấp nhận</th><th>Từ chối</th></tr></thead>
            <tbody>
              {receipt.lines.map((line) => (
                <tr key={line.goodsReceiptDetailId}>
                  <td>{line.sku} — {line.productName}</td>
                  <td>{formatPrice(line.unitPrice, receipt.currencyCode)}</td>
                  <td>{formatQuantity(line.orderedQuantity)} {line.unitCode}</td>
                  <td>{formatQuantity(line.receivedQuantity)} {line.unitCode}</td>
                  <td>{formatQuantity(line.acceptedQuantity)} {line.unitCode}</td>
                  <td>{formatQuantity(line.rejectedQuantity)} {line.unitCode}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <section className="receiving-inspection">
          <h3>Kiểm tra chất lượng</h3>
          {receipt.qualityInspection ? (
            <>
              <p>{receipt.qualityInspection.inspectionNumber} · {statusText(receipt.qualityInspection.result)}</p>
              <ul>
                {receipt.qualityInspection.checks.map((check) => (
                  <li key={check.qualityInspectionDetailId}>{check.checkName}: {statusText(check.result)}{check.note ? ` — ${check.note}` : ''}</li>
                ))}
              </ul>
            </>
          ) : <p>Chưa có kết quả kiểm tra.</p>}
        </section>
        <div className="modal-actions">
          {canEdit ? (
            <>
              <button className="btn btn-secondary" onClick={() => setDialog('checkin')} type="button">Kiểm nhận</button>
              <button className="btn btn-secondary" disabled={receipt.status !== 'Checking'} onClick={() => setDialog('quality')} type="button">
                {hasInspection ? 'Cập nhật kiểm tra' : 'Kiểm tra chất lượng'}
              </button>
              {hasInspection && ['Passed', 'Failed'].includes(receipt.qualityInspection?.result ?? '') ? (
                <button className="btn btn-primary" onClick={() => setDialog('confirm')} type="button">Xác nhận nhập kho</button>
              ) : null}
              <button className="btn btn-danger-ghost" onClick={() => void cancelReceipt()} type="button">Hủy phiếu</button>
            </>
          ) : null}
          <button className="btn btn-secondary" onClick={onClose} type="button">Đóng</button>
        </div>
      </Modal>
      {dialog === 'checkin' ? (
        <CheckInDialog
          onClose={() => setDialog(null)}
          onSaved={() => {
            setDialog(null)
            onChanged()
          }}
          receipt={receipt}
        />
      ) : null}
      {dialog === 'quality' ? (
        <QualityInspectionDialog
          onClose={() => setDialog(null)}
          onSaved={() => {
            setDialog(null)
            onChanged()
          }}
          receipt={receipt}
        />
      ) : null}
      {dialog === 'confirm' ? (
        <ConfirmReceiptDialog
          countries={countries}
          manufacturers={manufacturers}
          onClose={() => setDialog(null)}
          onSaved={() => {
            setDialog(null)
            onChanged()
          }}
          receipt={receipt}
        />
      ) : null}
    </>
  )
}

function ReceivingPage() {
  const ordersQuery = useApiQuery<PurchaseOrderResponse[]>(receivingRoutes.purchaseOrders)
  const suppliersQuery = useApiQuery<SupplierResponse[]>(apiRoutes.suppliers)
  const productsQuery = useApiQuery<ProductResponse[]>(apiRoutes.products)
  const currenciesQuery = useApiQuery<CurrencyResponse[]>(apiRoutes.currencies)
  const warehousesQuery = useApiQuery<WarehouseResponse[]>(warehouseRoutes.warehouses)
  const countriesQuery = useApiQuery<CountryResponse[]>(apiRoutes.countries)
  const manufacturersQuery = useApiQuery<ManufacturerResponse[]>(apiRoutes.manufacturers)
  const [tab, setTab] = useState<ReceivingTab>('receipts')
  const [search, setSearch] = useState('')
  const [receiptStatus, setReceiptStatus] = useState('')
  const [receiptSupplierId, setReceiptSupplierId] = useState('')
  const [receiptWarehouseId, setReceiptWarehouseId] = useState('')
  const [receiptFromDate, setReceiptFromDate] = useState('')
  const [receiptToDate, setReceiptToDate] = useState('')
  const [createOrder, setCreateOrder] = useState(false)
  const [createReceipt, setCreateReceipt] = useState(false)
  const [selectedReceiptId, setSelectedReceiptId] = useState<number | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const receiptListPath = useMemo(
    () => receiptFromDate && receiptToDate && receiptFromDate > receiptToDate
      ? null
      : receivingRoutes.receiptList({
        search: search.trim() || undefined,
        status: receiptStatus || undefined,
        supplierId: receiptSupplierId ? Number(receiptSupplierId) : undefined,
        warehouseId: receiptWarehouseId ? Number(receiptWarehouseId) : undefined,
        fromDate: receiptFromDate || undefined,
        toDate: receiptToDate || undefined,
      }),
    [receiptFromDate, receiptStatus, receiptSupplierId, receiptToDate, receiptWarehouseId, search],
  )
  const receiptsQuery = useApiQuery<GoodsReceiptListItemResponse[]>(receiptListPath)
  const orderQuery = useApiQuery<GoodsReceiptResponse>(
    selectedReceiptId ? receivingRoutes.receipt(selectedReceiptId) : null,
  )

  const orders = ordersQuery.data ?? EMPTY_ORDERS
  const receipts = receiptsQuery.data ?? EMPTY_RECEIPTS
  const filteredOrders = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('vi')
    return orders.filter((order) =>
      normalized.length === 0
      || order.poNumber.toLocaleLowerCase('vi').includes(normalized)
      || order.supplierName.toLocaleLowerCase('vi').includes(normalized))
  }, [orders, search])
  const filteredReceipts = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('vi')
    return receipts.filter((receipt) =>
      normalized.length === 0
      || receipt.receiptNumber.toLocaleLowerCase('vi').includes(normalized)
      || receipt.poNumber.toLocaleLowerCase('vi').includes(normalized)
      || receipt.supplierName.toLocaleLowerCase('vi').includes(normalized))
  }, [receipts, search])
  const eligibleOrderCount = orders.filter((order) => order.status === 'Approved' || order.status === 'PartiallyReceived').length
  const activeSuppliers = (suppliersQuery.data ?? []).filter((supplier) => supplier.isActive)
  const activeProducts = (productsQuery.data ?? []).filter((product) => product.isActive)
  const activeCurrencies = (currenciesQuery.data ?? []).filter((currency) => currency.isActive)
  const activeWarehouses = (warehousesQuery.data ?? []).filter((warehouse) => warehouse.isActive)
  const orderOptionQueries = [suppliersQuery, productsQuery, currenciesQuery]
  const orderOptionsLoading = orderOptionQueries.some((query) => query.loading)
  const orderOptionsError = orderOptionQueries.some((query) => query.error !== null)
  const missingOrderOptions = [
    activeSuppliers.length === 0 ? 'nhà cung cấp hoạt động' : null,
    activeProducts.length === 0 ? 'sản phẩm hoạt động' : null,
    activeCurrencies.length === 0 ? 'tiền tệ hoạt động' : null,
  ].filter((label): label is string => label !== null)
  const canCreateOrder = !orderOptionsLoading && !orderOptionsError && missingOrderOptions.length === 0
  const canCreateReceipt =
    eligibleOrderCount > 0
    && !warehousesQuery.loading
    && !warehousesQuery.error
    && activeWarehouses.length > 0
  const invalidReceiptDateRange = Boolean(
    receiptFromDate && receiptToDate && receiptFromDate > receiptToDate,
  )
  const hasReceiptFilters = Boolean(
    search.trim() || receiptStatus || receiptSupplierId || receiptWarehouseId || receiptFromDate || receiptToDate,
  )

  function clearReceiptFilters() {
    setSearch('')
    setReceiptStatus('')
    setReceiptSupplierId('')
    setReceiptWarehouseId('')
    setReceiptFromDate('')
    setReceiptToDate('')
  }

  async function updateOrderStatus(order: PurchaseOrderResponse, action: 'submit' | 'approve' | 'cancel') {
    const actionLabels = { submit: 'gửi duyệt', approve: 'duyệt', cancel: 'hủy' }
    if (!window.confirm(`Xác nhận ${actionLabels[action]} đơn ${order.poNumber}?`)) return
    setActionError(null)
    setNotice(null)
    try {
      if (action === 'submit') await receivingApi.submitPurchaseOrder(order.purchaseOrderId)
      if (action === 'approve') await receivingApi.approvePurchaseOrder(order.purchaseOrderId)
      if (action === 'cancel') await receivingApi.cancelPurchaseOrder(order.purchaseOrderId)
      setNotice(`Đã ${actionLabels[action]} đơn mua ${order.poNumber}.`)
      ordersQuery.reload()
    } catch (error) {
      setActionError(getErrorMessage(error))
    }
  }

  const orderColumns = [
    {
      key: 'order',
      title: 'Đơn mua',
      render: (order: PurchaseOrderResponse) => (
        <div className="table-main-cell"><strong>{order.poNumber}</strong><small>{order.supplierName}</small></div>
      ),
    },
    {
      key: 'date',
      title: 'Ngày đặt',
      render: (order: PurchaseOrderResponse) => formatDate(order.orderDate),
    },
    {
      key: 'lines',
      title: 'Sản phẩm',
      align: 'right' as const,
      render: (order: PurchaseOrderResponse) => order.lines.length,
    },
    {
      key: 'currency',
      title: 'Tiền tệ',
      render: (order: PurchaseOrderResponse) => `${order.currencyCode} · tỷ giá ${order.exchangeRate}`,
    },
    {
      key: 'status',
      title: 'Trạng thái',
      render: (order: PurchaseOrderResponse) => <span className="receiving-status">{statusText(order.status)}</span>,
    },
    {
      key: 'actions',
      title: 'Thao tác',
      render: (order: PurchaseOrderResponse) => (
        <div className="table-actions">
          {order.status === 'Draft' ? (
            <button className="btn btn-small btn-secondary" onClick={() => void updateOrderStatus(order, 'submit')} type="button">Gửi duyệt</button>
          ) : null}
          {order.status === 'Submitted' ? (
            <button className="btn btn-small btn-primary" onClick={() => void updateOrderStatus(order, 'approve')} type="button">Duyệt</button>
          ) : null}
          {order.status === 'Draft' || order.status === 'Submitted' ? (
            <button className="btn btn-small btn-danger-ghost" onClick={() => void updateOrderStatus(order, 'cancel')} type="button">Hủy</button>
          ) : null}
        </div>
      ),
    },
  ]

  const receiptColumns = [
    {
      key: 'receipt',
      title: 'Phiếu nhập',
      render: (receipt: GoodsReceiptListItemResponse) => (
        <div className="table-main-cell"><strong>{receipt.receiptNumber}</strong><small>Đơn {receipt.poNumber}</small></div>
      ),
    },
    { key: 'supplier', title: 'Nhà cung cấp', render: (receipt: GoodsReceiptListItemResponse) => receipt.supplierName },
    { key: 'warehouse', title: 'Kho nhận', render: (receipt: GoodsReceiptListItemResponse) => receipt.warehouseName },
    { key: 'date', title: 'Ngày nhận', render: (receipt: GoodsReceiptListItemResponse) => formatDate(receipt.receivedAt) },
    { key: 'quantities', title: 'Thực nhận / chấp nhận / từ chối', render: (receipt: GoodsReceiptListItemResponse) =>
      `${formatQuantity(receipt.receivedQuantity)} / ${formatQuantity(receipt.acceptedQuantity)} / ${formatQuantity(receipt.rejectedQuantity)}` },
    { key: 'status', title: 'Trạng thái', render: (receipt: GoodsReceiptListItemResponse) =>
      <span className="receiving-status">{statusText(receipt.status)}</span> },
    { key: 'actions', title: 'Thao tác', render: (receipt: GoodsReceiptListItemResponse) =>
      <button className="btn btn-small btn-secondary" onClick={() => setSelectedReceiptId(receipt.goodsReceiptId)} type="button">Chi tiết</button> },
  ]

  return (
    <>
      <PageHeading
        action={(
          <div className="page-heading-actions">
            {tab === 'orders' ? (
              <button
                className="btn btn-primary"
                disabled={!canCreateOrder}
                onClick={() => setCreateOrder(true)}
                type="button"
              >
                <span className="button-plus">+</span> Tạo đơn mua
              </button>
            ) : (
              <button
                className="btn btn-primary"
                disabled={!canCreateReceipt}
                onClick={() => setCreateReceipt(true)}
                type="button"
              >
                <span className="button-plus">+</span> Tạo phiếu nhập
              </button>
            )}
          </div>
        )}
        description="Theo dõi đơn mua, kiểm nhận, chất lượng và xác nhận nhập vào tồn kho."
        eyebrow="Nghiệp vụ kho"
        title="Nhập hàng"
      />
      <div className="alert alert-info receiving-info">
        Tồn kho chỉ thay đổi sau khi hoàn tất kiểm nhận, kiểm tra chất lượng và xác nhận put-away. Ứng dụng hiện chưa có đăng nhập/phân quyền; thao tác gửi duyệt và duyệt chưa được giới hạn theo vai trò.
      </div>
      {notice ? <NoticeBanner message={notice} onDismiss={() => setNotice(null)} /> : null}
      {actionError ? <ErrorBanner message={actionError} /> : null}
      {suppliersQuery.error ? <ErrorBanner message={`Không tải được nhà cung cấp: ${suppliersQuery.error}`} onRetry={suppliersQuery.reload} /> : null}
      {productsQuery.error ? <ErrorBanner message={`Không tải được sản phẩm: ${productsQuery.error}`} onRetry={productsQuery.reload} /> : null}
      {currenciesQuery.error ? <ErrorBanner message={`Không tải được tiền tệ: ${currenciesQuery.error}`} onRetry={currenciesQuery.reload} /> : null}
      {warehousesQuery.error ? <ErrorBanner message={`Không tải được kho: ${warehousesQuery.error}`} onRetry={warehousesQuery.reload} /> : null}
      {countriesQuery.error ? <ErrorBanner message={`Không tải được quốc gia: ${countriesQuery.error}`} onRetry={countriesQuery.reload} /> : null}
      {manufacturersQuery.error ? <ErrorBanner message={`Không tải được nhà sản xuất: ${manufacturersQuery.error}`} onRetry={manufacturersQuery.reload} /> : null}
      {tab === 'orders' && !orderOptionsLoading && !orderOptionsError && missingOrderOptions.length > 0 ? (
        <div className="alert alert-info">
          Chưa thể tạo đơn mua vì chưa có {missingOrderOptions.join(', ')} trong dữ liệu backend.
        </div>
      ) : null}
      {tab === 'orders' && orderOptionsLoading ? (
        <div className="alert alert-info">Đang tải nhà cung cấp, sản phẩm và tiền tệ để tạo đơn mua…</div>
      ) : null}
      {tab === 'receipts' && eligibleOrderCount > 0 && !warehousesQuery.loading && !warehousesQuery.error && activeWarehouses.length === 0 ? (
        <div className="alert alert-info">Chưa có kho đang hoạt động; hiện chưa thể tạo phiếu nhập.</div>
      ) : null}
      {tab === 'receipts' && invalidReceiptDateRange ? (
        <div className="alert alert-error" role="alert">Ngày bắt đầu không được sau ngày kết thúc.</div>
      ) : null}

      <section className="panel receiving-workspace">
        <div className="receiving-tabs" role="tablist" aria-label="Nhập hàng">
          <button
            aria-selected={tab === 'receipts'}
            className={tab === 'receipts' ? 'receiving-tab active' : 'receiving-tab'}
            onClick={() => { setTab('receipts'); setSearch('') }}
            role="tab"
            type="button"
          >
            Phiếu nhập <span>{receipts.length}</span>
          </button>
          <button
            aria-selected={tab === 'orders'}
            className={tab === 'orders' ? 'receiving-tab active' : 'receiving-tab'}
            onClick={() => { setTab('orders'); setSearch('') }}
            role="tab"
            type="button"
          >
            Đơn mua <span>{orders.length}</span>
          </button>
        </div>
        <div className="table-toolbar">
          <div className="table-toolbar-copy">
            <strong>{tab === 'receipts' ? 'Danh sách phiếu nhập' : 'Danh sách đơn mua hàng'}</strong>
            <span>{tab === 'receipts' ? `${filteredReceipts.length} phiếu` : `${filteredOrders.length} đơn`}</span>
          </div>
          <div className="table-filters">
            <label className="search-box">
              <span aria-hidden="true">⌕</span>
              <input
                aria-label={tab === 'receipts' ? 'Tìm phiếu nhập' : 'Tìm đơn mua'}
                onChange={(event) => setSearch(event.target.value)}
                placeholder={tab === 'receipts' ? 'Tìm mã phiếu, đơn mua, nhà cung cấp…' : 'Tìm mã đơn, nhà cung cấp…'}
                value={search}
              />
            </label>
            {tab === 'receipts' ? (
              <>
                <select
                  aria-label="Lọc trạng thái phiếu nhập"
                  onChange={(event) => setReceiptStatus(event.target.value)}
                  value={receiptStatus}
                >
                  <option value="">Tất cả trạng thái</option>
                  <option value="Draft">Nháp</option>
                  <option value="Checking">Đang kiểm nhận</option>
                  <option value="Accepted">Đã nhập kho</option>
                  <option value="PartiallyAccepted">Nhập một phần</option>
                  <option value="Rejected">Từ chối</option>
                  <option value="Cancelled">Đã hủy</option>
                </select>
                <select
                  aria-label="Lọc nhà cung cấp"
                  onChange={(event) => setReceiptSupplierId(event.target.value)}
                  value={receiptSupplierId}
                >
                  <option value="">Tất cả nhà cung cấp</option>
                  {(suppliersQuery.data ?? []).map((supplier) => (
                    <option key={supplier.supplierId} value={supplier.supplierId}>
                      {supplier.supplierCode} — {supplier.supplierName}
                    </option>
                  ))}
                </select>
                <select
                  aria-label="Lọc kho nhận"
                  onChange={(event) => setReceiptWarehouseId(event.target.value)}
                  value={receiptWarehouseId}
                >
                  <option value="">Tất cả kho</option>
                  {(warehousesQuery.data ?? []).map((warehouse) => (
                    <option key={warehouse.warehouseId} value={warehouse.warehouseId}>
                      {warehouse.warehouseCode} — {warehouse.warehouseName}
                    </option>
                  ))}
                </select>
                <label className="receiving-date-filter">
                  <span>Từ ngày</span>
                  <input
                    aria-label="Phiếu nhập từ ngày"
                    max={receiptToDate || undefined}
                    onChange={(event) => setReceiptFromDate(event.target.value)}
                    type="date"
                    value={receiptFromDate}
                  />
                </label>
                <label className="receiving-date-filter">
                  <span>Đến ngày</span>
                  <input
                    aria-label="Phiếu nhập đến ngày"
                    min={receiptFromDate || undefined}
                    onChange={(event) => setReceiptToDate(event.target.value)}
                    type="date"
                    value={receiptToDate}
                  />
                </label>
                <button className="btn btn-small btn-secondary" onClick={clearReceiptFilters} type="button">
                  Xóa lọc
                </button>
              </>
            ) : null}
          </div>
        </div>
        {tab === 'receipts' ? (
          <>
            {receiptsQuery.error ? <ErrorBanner message={receiptsQuery.error} onRetry={receiptsQuery.reload} /> : null}
            <DataTable
              columns={receiptColumns}
              emptyMessage={hasReceiptFilters
                ? 'Không có phiếu nhập phù hợp với bộ lọc hiện tại.'
                : 'Chưa có phiếu nhập trong dữ liệu backend.'}
              getKey={(receipt) => receipt.goodsReceiptId}
              loading={receiptsQuery.loading}
              rows={invalidReceiptDateRange ? EMPTY_RECEIPTS : filteredReceipts}
            />
          </>
        ) : (
          <>
            {ordersQuery.error ? <ErrorBanner message={ordersQuery.error} onRetry={ordersQuery.reload} /> : null}
            <DataTable
              columns={orderColumns}
              emptyMessage="Chưa có đơn mua trong dữ liệu backend."
              getKey={(order) => order.purchaseOrderId}
              loading={ordersQuery.loading}
              rows={filteredOrders}
            />
          </>
        )}
        {tab === 'receipts' && eligibleOrderCount === 0 ? (
          <p className="receiving-hint">Hiện chưa có đơn mua ở trạng thái Đã duyệt hoặc Nhận một phần để tạo phiếu nhập.</p>
        ) : null}
      </section>

      {createOrder ? (
        <CreatePurchaseOrderDialog
          currencies={currenciesQuery.data ?? []}
          onClose={() => setCreateOrder(false)}
          onSaved={() => {
            setCreateOrder(false)
            setTab('orders')
            setNotice('Đã tạo đơn mua nháp. Gửi duyệt và duyệt đơn trước khi tạo phiếu nhập.')
            ordersQuery.reload()
          }}
          products={productsQuery.data ?? []}
          suppliers={suppliersQuery.data ?? []}
        />
      ) : null}
      {createReceipt ? (
        <CreateReceiptDialog
          onClose={() => setCreateReceipt(false)}
          onSaved={() => {
            setCreateReceipt(false)
            setNotice('Đã tạo phiếu nhập nháp; chưa ghi nhận tồn kho.')
            receiptsQuery.reload()
            ordersQuery.reload()
          }}
          orders={orders}
          warehouses={warehousesQuery.data ?? []}
        />
      ) : null}
      {selectedReceiptId !== null ? (
        orderQuery.loading ? (
          <Modal onClose={() => setSelectedReceiptId(null)} title="Đang tải phiếu nhập"><p>Đang tải dữ liệu…</p></Modal>
        ) : orderQuery.error ? (
          <Modal onClose={() => setSelectedReceiptId(null)} title="Không tải được phiếu nhập">
            <ErrorBanner message={orderQuery.error} onRetry={orderQuery.reload} />
          </Modal>
        ) : orderQuery.data ? (
          <ReceiptDetailDialog
            countries={countriesQuery.data ?? []}
            manufacturers={manufacturersQuery.data ?? []}
            onChanged={() => {
              orderQuery.reload()
              receiptsQuery.reload()
              ordersQuery.reload()
            }}
            onClose={() => setSelectedReceiptId(null)}
            receipt={orderQuery.data}
          />
        ) : null
      ) : null}
    </>
  )
}

export default ReceivingPage
