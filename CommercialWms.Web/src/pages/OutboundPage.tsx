import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import {
  DataTable,
  ErrorBanner,
  Modal,
  NoticeBanner,
  PageHeading,
} from '../components/Ui'
import { useApiQuery } from '../hooks/useApiQuery'
import { getErrorMessage } from '../services/apiClient'
import { outboundApi, outboundRoutes } from '../services/outboundApi'
import type {
  ConfirmOutboundOrderRequest,
  CreateOutboundOrderLineRequest,
  CreateOutboundOrderRequest,
  OutboundAllocationRequest,
  OutboundAvailableStockResponse,
  OutboundDeliveryListItemResponse,
  OutboundDeliveryResponse,
  OutboundOptionsResponse,
  OutboundOrderLineResponse,
  OutboundOrderListItemResponse,
  OutboundOrderResponse,
} from '../types/contracts'

type OutboundTab = 'orders' | 'deliveries'
type AllocationEditor = {
  salesOrderDetailId: number
  lotId: string
  binId: string
  quantity: string
}

const EMPTY_ORDERS: OutboundOrderListItemResponse[] = []
const EMPTY_DELIVERIES: OutboundDeliveryListItemResponse[] = []
const EMPTY_STOCK: OutboundAvailableStockResponse[] = []
const EMPTY_OPTIONS: OutboundOptionsResponse = {
  customers: [],
  currencies: [],
  products: [],
  warehouses: [],
  employees: [],
}

function queryPath(
  path: string,
  values: Record<string, string | number | undefined>,
) {
  const query = new URLSearchParams()
  Object.entries(values).forEach(([key, value]) => {
    if (value !== undefined && String(value).length > 0) query.set(key, String(value))
  })
  const suffix = query.toString()
  return suffix.length > 0 ? `${path}?${suffix}` : path
}

function formatQuantity(value: number) {
  return new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 3 }).format(value)
}

function statusText(status: string) {
  const labels: Record<string, string> = {
    Draft: 'Nháp',
    Confirmed: 'Đã duyệt',
    PartiallyDelivered: 'Giao một phần',
    Delivered: 'Đã xuất đủ',
    Cancelled: 'Đã hủy',
    Picking: 'Đang soạn hàng',
    Shipped: 'Đã xuất kho',
    Failed: 'Giao thất bại',
  }
  return labels[status] ?? status
}

function CreateOrderDialog({
  options,
  onClose,
  onSaved,
}: {
  options: OutboundOptionsResponse
  onClose: () => void
  onSaved: (order: OutboundOrderResponse) => void
}) {
  const [customerId, setCustomerId] = useState('')
  const [currencyId, setCurrencyId] = useState('')
  const [employeeId, setEmployeeId] = useState('')
  const [exchangeRate, setExchangeRate] = useState('1')
  const [note, setNote] = useState('')
  const [lines, setLines] = useState<CreateOutboundOrderLineRequest[]>([
    { productId: 0, unitId: 0, quantity: 0, unitPrice: 0, discountAmount: 0 },
  ])
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  function updateLine(index: number, patch: Partial<CreateOutboundOrderLineRequest>) {
    setLines((current) => current.map((line, lineIndex) =>
      lineIndex === index ? { ...line, ...patch } : line))
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    if (!Number.isInteger(Number(customerId)) || !Number.isInteger(Number(currencyId))
      || Number(exchangeRate) <= 0
      || lines.some(line => line.productId <= 0 || line.unitId <= 0
        || line.quantity <= 0 || line.unitPrice < 0 || line.discountAmount < 0)) {
      setError('Kiểm tra khách hàng, tiền tệ, tỷ giá và các dòng hàng.')
      return
    }

    setSaving(true)
    try {
      const request: CreateOutboundOrderRequest = {
        customerId: Number(customerId),
        currencyId: Number(currencyId),
        exchangeRate: Number(exchangeRate),
        createdByEmployeeId: employeeId ? Number(employeeId) : null,
        note: note.trim() || null,
        lines,
      }
      onSaved(await outboundApi.createOrder(request))
    } catch (reason) {
      setError(getErrorMessage(reason))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal description="Đơn ở trạng thái nháp cho đến khi phân bổ tồn kho." onClose={onClose} title="Tạo đơn bán hàng" wide>
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {error ? <ErrorBanner message={error} /> : null}
        <div className="form-grid">
          <label className="form-field">
            <span>Khách hàng <b>*</b></span>
            <select onChange={(event) => setCustomerId(event.target.value)} required value={customerId}>
              <option value="">Chọn khách hàng</option>
              {options.customers.map(customer => (
                <option key={customer.customerId} value={customer.customerId}>
                  {customer.customerCode} — {customer.customerName}
                </option>
              ))}
            </select>
          </label>
          <label className="form-field">
            <span>Tiền tệ <b>*</b></span>
            <select onChange={(event) => setCurrencyId(event.target.value)} required value={currencyId}>
              <option value="">Chọn tiền tệ</option>
              {options.currencies.map(currency => (
                <option key={currency.currencyId} value={currency.currencyId}>
                  {currency.currencyCode} — {currency.currencyName}
                </option>
              ))}
            </select>
          </label>
          <label className="form-field">
            <span>Tỷ giá <b>*</b></span>
            <input min="0.000001" onChange={(event) => setExchangeRate(event.target.value)} required step="0.000001" type="number" value={exchangeRate} />
          </label>
          <label className="form-field">
            <span>Nhân viên lập</span>
            <select onChange={(event) => setEmployeeId(event.target.value)} value={employeeId}>
              <option value="">Không chọn</option>
              {options.employees.map(employee => (
                <option key={employee.employeeId} value={employee.employeeId}>
                  {employee.employeeCode} — {employee.fullName}
                </option>
              ))}
            </select>
          </label>
        </div>

        <div className="receiving-section-heading">
          <h3>Sản phẩm và số lượng</h3>
          <button className="btn btn-secondary btn-small" onClick={() => setLines(current => [
            ...current,
            { productId: 0, unitId: 0, quantity: 0, unitPrice: 0, discountAmount: 0 },
          ])} type="button">Thêm dòng</button>
        </div>
        <div className="receiving-lines">
          {lines.map((line, index) => {
            const product = options.products.find(item => item.productId === line.productId)
            return (
              <div className="receiving-order-line" key={index}>
                <label className="form-field">
                  <span>Sản phẩm <b>*</b></span>
                  <select
                    onChange={(event) => updateLine(index, { productId: Number(event.target.value), unitId: 0 })}
                    required
                    value={line.productId || ''}
                  >
                    <option value="">Chọn sản phẩm</option>
                    {options.products.map(item => (
                      <option key={item.productId} value={item.productId}>{item.sku} — {item.productName}</option>
                    ))}
                  </select>
                </label>
                <label className="form-field">
                  <span>Đơn vị <b>*</b></span>
                  <select
                    disabled={!product}
                    onChange={(event) => updateLine(index, { unitId: Number(event.target.value) })}
                    required
                    value={line.unitId || ''}
                  >
                    <option value="">Chọn đơn vị</option>
                    {(product?.units ?? []).map(unit => (
                      <option key={unit.unitId} value={unit.unitId}>{unit.unitCode}</option>
                    ))}
                  </select>
                </label>
                <label className="form-field">
                  <span>Số lượng <b>*</b></span>
                  <input min="0.001" onChange={(event) => updateLine(index, { quantity: Number(event.target.value) })} required step="0.001" type="number" value={line.quantity || ''} />
                </label>
                <label className="form-field">
                  <span>Đơn giá <b>*</b></span>
                  <input min="0" onChange={(event) => updateLine(index, { unitPrice: Number(event.target.value) })} required step="0.0001" type="number" value={line.unitPrice} />
                </label>
                <label className="form-field">
                  <span>Chiết khấu</span>
                  <input min="0" onChange={(event) => updateLine(index, { discountAmount: Number(event.target.value) })} step="0.0001" type="number" value={line.discountAmount} />
                </label>
                <button
                  aria-label="Xóa dòng sản phẩm"
                  className="btn btn-small btn-danger-ghost"
                  disabled={lines.length === 1}
                  onClick={() => setLines(current => current.filter((_, lineIndex) => lineIndex !== index))}
                  type="button"
                >Xóa</button>
              </div>
            )
          })}
        </div>
        <label className="form-field">
          <span>Ghi chú</span>
          <textarea maxLength={500} onChange={(event) => setNote(event.target.value)} rows={2} value={note} />
        </label>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">Đóng</button>
          <button className="btn btn-primary" disabled={saving} type="submit">{saving ? 'Đang lưu…' : 'Tạo đơn nháp'}</button>
        </div>
      </form>
    </Modal>
  )
}

function AllocationRow({
  orderLines,
  orderLine,
  warehouseId,
  reservations,
  value,
  onChange,
  onRemove,
  canRemove,
}: {
  orderLines: OutboundOrderLineResponse[]
  orderLine: OutboundOrderLineResponse | undefined
  warehouseId: number
  reservations: OutboundOrderLineResponse['activeReservations']
  value: AllocationEditor
  onChange: (value: AllocationEditor) => void
  onRemove: () => void
  canRemove: boolean
}) {
  const availabilityPath = orderLine && warehouseId > 0
    ? outboundRoutes.availability(warehouseId, orderLine.productId, orderLine.unitId)
    : null
  const stockQuery = useApiQuery<OutboundAvailableStockResponse[]>(availabilityPath)
  const stockRows = stockQuery.data ?? EMPTY_STOCK
  const mergedOptions = useMemo(() => {
    const map = new Map<string, { lotId: number; binId: number; lotCode: string; binCode: string; locationCode: string; quantity: number; reserved: boolean }>()
    for (const stock of stockRows) {
      map.set(`${stock.lotId}:${stock.binId}`, {
        lotId: stock.lotId,
        binId: stock.binId,
        lotCode: stock.lotCode,
        binCode: stock.binCode,
        locationCode: stock.locationCode,
        quantity: stock.quantityAvailableInOrderUnit,
        reserved: false,
      })
    }
    for (const reservation of reservations) {
      const key = `${reservation.lotId}:${reservation.binId}`
      const current = map.get(key)
      if (current) {
        current.quantity += reservation.reservedQuantityInOrderUnit
        current.reserved = true
      } else {
        map.set(key, {
          lotId: reservation.lotId,
          binId: reservation.binId,
          lotCode: reservation.lotCode,
          binCode: reservation.binCode,
          locationCode: 'Đã giữ chỗ',
          quantity: reservation.reservedQuantityInOrderUnit,
          reserved: true,
        })
      }
    }
    return [...map.values()]
  }, [reservations, stockRows])

  return (
    <div className="receiving-order-line">
      <label className="form-field">
        <span>Dòng đơn hàng</span>
        <select
          onChange={(event) => onChange({ ...value, salesOrderDetailId: Number(event.target.value) })}
          required
          value={value.salesOrderDetailId || ''}
        >
          <option value="">Chọn sản phẩm</option>
          {orderLines.filter(line => line.quantity > line.quantityDelivered).map(line => (
            <option key={line.salesOrderDetailId} value={line.salesOrderDetailId}>{line.sku} — {line.productName}</option>
          ))}
        </select>
      </label>
      <label className="form-field">
        <span>Lô / vị trí</span>
        <select
          disabled={!orderLine || warehouseId <= 0 || stockQuery.loading}
          onChange={(event) => {
            const [lotId, binId] = event.target.value.split(':').map(Number)
            onChange({ ...value, lotId: String(lotId || ''), binId: String(binId || '') })
          }}
          required
          value={value.lotId && value.binId ? `${value.lotId}:${value.binId}` : ''}
        >
          <option value="">{stockQuery.loading ? 'Đang tải tồn…' : 'Chọn lô / vị trí'}</option>
          {mergedOptions.map(option => (
            <option key={`${option.lotId}:${option.binId}`} value={`${option.lotId}:${option.binId}`}>
              {option.lotCode} · {option.binCode} · khả dụng {formatQuantity(option.quantity)}
              {option.reserved ? ' (có giữ chỗ)' : ''}
            </option>
          ))}
        </select>
        {stockQuery.error ? <small className="field-error">{stockQuery.error}</small> : null}
      </label>
      <label className="form-field">
        <span>Số lượng ({orderLine?.unitCode ?? 'đơn vị'})</span>
        <input min="0.001" onChange={(event) => onChange({ ...value, quantity: event.target.value })} required step="0.001" type="number" value={value.quantity} />
      </label>
      <button aria-label="Xóa phân bổ" className="btn btn-small btn-danger-ghost" disabled={!canRemove} onClick={onRemove} type="button">Xóa</button>
    </div>
  )
}

function OrderDialog({
  orderId,
  options,
  onClose,
  onChanged,
  onOpenDelivery,
}: {
  orderId: number
  options: OutboundOptionsResponse
  onClose: () => void
  onChanged: () => void
  onOpenDelivery: (id: number) => void
}) {
  const orderQuery = useApiQuery<OutboundOrderResponse>(outboundRoutes.order(orderId))
  const order = orderQuery.data
  const [allocations, setAllocations] = useState<AllocationEditor[]>([])
  const [warehouseId, setWarehouseId] = useState('')
  const [addressId, setAddressId] = useState('')
  const [mode, setMode] = useState<'details' | 'allocate'>('details')
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  function beginAllocation(currentOrder: OutboundOrderResponse) {
    setWarehouseId('')
    const customer = options.customers.find(item => item.customerId === currentOrder.customerId)
    const defaultAddress = customer?.addresses.find(item => item.isDefault) ?? customer?.addresses[0]
    setAddressId(defaultAddress ? String(defaultAddress.addressId) : '')
    const rows = currentOrder.lines.flatMap(line => {
      const remaining = line.quantity - line.quantityDelivered
      if (remaining <= 0) return []
      if (line.activeReservations.length > 0) {
        return line.activeReservations.map(reservation => ({
          salesOrderDetailId: line.salesOrderDetailId,
          lotId: String(reservation.lotId),
          binId: String(reservation.binId),
          quantity: String(reservation.reservedQuantityInOrderUnit),
        }))
      }
      return [{
        salesOrderDetailId: line.salesOrderDetailId,
        lotId: '',
        binId: '',
        quantity: String(remaining),
      }]
    })
    setAllocations(rows)
    setMode('allocate')
  }

  async function confirmOrder(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!order) return
    setError(null)
    const items: OutboundAllocationRequest[] = allocations.map(item => ({
      salesOrderDetailId: item.salesOrderDetailId,
      lotId: Number(item.lotId),
      binId: Number(item.binId),
      quantity: Number(item.quantity),
    }))
    const quantities = new Map<number, number>()
    for (const item of items) {
      quantities.set(item.salesOrderDetailId, (quantities.get(item.salesOrderDetailId) ?? 0) + item.quantity)
    }
    const valid = order.lines.every(line => {
      const expected = line.quantity - line.quantityDelivered
      return expected <= 0 ? !quantities.has(line.salesOrderDetailId)
        : Math.round((quantities.get(line.salesOrderDetailId) ?? 0) * 1000) === Math.round(expected * 1000)
    })
    if (!valid || items.some(item => item.lotId <= 0 || item.binId <= 0 || item.quantity <= 0)) {
      setError('Phân bổ đủ số lượng còn lại cho từng dòng đơn; chọn lô và vị trí hợp lệ.')
      return
    }
    setSaving(true)
    try {
      const request: ConfirmOutboundOrderRequest = {
        warehouseId: Number(warehouseId),
        deliveryAddressId: addressId ? Number(addressId) : null,
        allocations: items,
      }
      await outboundApi.confirmOrder(order.salesOrderId, request)
      orderQuery.reload()
      onChanged()
      setMode('details')
    } catch (reason) {
      setError(getErrorMessage(reason))
    } finally {
      setSaving(false)
    }
  }

  async function cancelOrder() {
    if (!order) return
    setSaving(true)
    setError(null)
    try {
      await outboundApi.cancelOrder(order.salesOrderId)
      orderQuery.reload()
      onChanged()
    } catch (reason) {
      setError(getErrorMessage(reason))
    } finally {
      setSaving(false)
    }
  }

  const customer = order ? options.customers.find(item => item.customerId === order.customerId) : undefined
  const canPrepare = order?.status === 'Draft' || order?.status === 'PartiallyDelivered'

  return (
    <Modal
      description={order ? `${order.salesOrderNumber} · ${order.customerName} · ${statusText(order.status)}` : undefined}
      onClose={onClose}
      title={mode === 'allocate' ? 'Phân bổ tồn kho và duyệt đơn' : 'Chi tiết đơn bán hàng'}
      wide
    >
      {orderQuery.error ? <ErrorBanner message={orderQuery.error} onRetry={orderQuery.reload} /> : null}
      {error ? <ErrorBanner message={error} /> : null}
      {orderQuery.loading || !order ? <p>Đang tải đơn hàng…</p> : mode === 'allocate' ? (
        <form className="form-stack" onSubmit={(event) => void confirmOrder(event)}>
          <div className="form-grid">
            <label className="form-field">
              <span>Kho xuất <b>*</b></span>
              <select onChange={(event) => setWarehouseId(event.target.value)} required value={warehouseId}>
                <option value="">Chọn kho</option>
                {options.warehouses.map(warehouse => (
                  <option key={warehouse.warehouseId} value={warehouse.warehouseId}>
                    {warehouse.warehouseCode} — {warehouse.warehouseName}
                  </option>
                ))}
              </select>
            </label>
            <label className="form-field">
              <span>Địa chỉ giao hàng</span>
              <select onChange={(event) => setAddressId(event.target.value)} value={addressId}>
                <option value="">Không chọn</option>
                {(customer?.addresses ?? []).map(address => (
                  <option key={address.addressId} value={address.addressId}>
                    {address.addressLine}{address.district ? `, ${address.district}` : ''}{address.province ? `, ${address.province}` : ''}
                  </option>
                ))}
              </select>
            </label>
          </div>
          <div className="notice-box">
            Tổng phân bổ mỗi dòng phải bằng số lượng chưa giao. Tồn kho được giữ chỗ khi duyệt; chỉ bị trừ khi xác nhận xuất kho.
          </div>
          <div className="receiving-section-heading">
            <h3>Chọn lô và vị trí xuất</h3>
            <button
              className="btn btn-secondary btn-small"
              onClick={() => setAllocations(current => [...current, {
                salesOrderDetailId: order.lines.find(line => line.quantity > line.quantityDelivered)?.salesOrderDetailId ?? 0,
                lotId: '',
                binId: '',
                quantity: '',
              }])}
              type="button"
            >Thêm phân bổ</button>
          </div>
          <div className="receiving-lines">
            {allocations.map((allocation, index) => {
              const line = order.lines.find(item => item.salesOrderDetailId === allocation.salesOrderDetailId)
              return (
                <AllocationRow
                  canRemove={allocations.length > 1}
                  key={`${allocation.salesOrderDetailId}-${index}`}
                  onChange={(value) => setAllocations(current => current.map((item, itemIndex) => itemIndex === index ? value : item))}
                  onRemove={() => setAllocations(current => current.filter((_, itemIndex) => itemIndex !== index))}
                  orderLines={order.lines}
                  orderLine={line}
                  reservations={line?.activeReservations ?? []}
                  value={allocation}
                  warehouseId={Number(warehouseId)}
                />
              )
            })}
          </div>
          <div className="modal-actions">
            <button className="btn btn-secondary" disabled={saving} onClick={() => setMode('details')} type="button">Quay lại</button>
            <button className="btn btn-primary" disabled={saving || !warehouseId} type="submit">{saving ? 'Đang giữ chỗ…' : 'Duyệt và tạo phiếu xuất'}</button>
          </div>
        </form>
      ) : (
        <div className="form-stack">
          <div className="receiving-order-summary">
            <span>Ngày tạo</span><strong>{new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium' }).format(new Date(order.orderDate))}</strong>
            <span>Tiền tệ / tỷ giá</span><strong>{order.currencyCode} / {order.exchangeRate}</strong>
            {order.note ? <><span>Ghi chú</span><strong>{order.note}</strong></> : null}
          </div>
          <div className="table-scroll">
            <table className="data-table">
              <thead><tr><th>Sản phẩm</th><th>Đặt</th><th>Đã giao</th><th>Giá</th><th>Giữ chỗ</th></tr></thead>
              <tbody>
                {order.lines.map(line => (
                  <tr key={line.salesOrderDetailId}>
                    <td><span className="table-main-cell"><strong>{line.sku}</strong><small>{line.productName}</small></span></td>
                    <td>{formatQuantity(line.quantity)} {line.unitCode}</td>
                    <td>{formatQuantity(line.quantityDelivered)} {line.unitCode}</td>
                    <td>{formatQuantity(line.unitPrice)} {order.currencyCode}</td>
                    <td>{formatQuantity(line.activeReservations.reduce((sum, item) => sum + item.reservedQuantityInOrderUnit, 0))} {line.unitCode}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="receiving-section-heading"><h3>Phiếu xuất</h3></div>
          {order.deliveries.length === 0 ? <p>Chưa có phiếu xuất.</p> : order.deliveries.map(delivery => (
            <div className="receiving-order-summary" key={delivery.deliveryId}>
              <strong>{delivery.deliveryNumber}</strong>
              <span>{delivery.warehouseCode} · {statusText(delivery.status)}</span>
              <button className="btn btn-link btn-small" onClick={() => onOpenDelivery(delivery.deliveryId)} type="button">Mở phiếu</button>
            </div>
          ))}
          <div className="modal-actions">
            {canPrepare ? <button className="btn btn-primary" disabled={saving} onClick={() => beginAllocation(order)} type="button">Phân bổ và duyệt đơn</button> : null}
            {order.status === 'Draft' ? <button className="btn btn-danger-ghost" disabled={saving} onClick={() => void cancelOrder()} type="button">Hủy đơn</button> : null}
            <button className="btn btn-secondary" onClick={onClose} type="button">Đóng</button>
          </div>
        </div>
      )}
    </Modal>
  )
}

function DeliveryDialog({
  deliveryId,
  onClose,
  onChanged,
}: {
  deliveryId: number
  onClose: () => void
  onChanged: () => void
}) {
  const deliveryQuery = useApiQuery<OutboundDeliveryResponse>(outboundRoutes.delivery(deliveryId))
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const delivery = deliveryQuery.data

  async function runAction(action: () => Promise<OutboundDeliveryResponse>) {
    setError(null)
    setBusy(true)
    try {
      await action()
      deliveryQuery.reload()
      onChanged()
    } catch (reason) {
      setError(getErrorMessage(reason))
    } finally {
      setBusy(false)
    }
  }

  return (
    <Modal
      description={delivery ? `${delivery.deliveryNumber} · ${delivery.salesOrderNumber} · ${statusText(delivery.status)}` : undefined}
      onClose={onClose}
      title="Chi tiết phiếu xuất"
      wide
    >
      {deliveryQuery.error ? <ErrorBanner message={deliveryQuery.error} onRetry={deliveryQuery.reload} /> : null}
      {error ? <ErrorBanner message={error} /> : null}
      {deliveryQuery.loading || !delivery ? <p>Đang tải phiếu xuất…</p> : (
        <div className="form-stack">
          <div className="receiving-order-summary">
            <span>Khách hàng</span><strong>{delivery.customerName}</strong>
            <span>Kho</span><strong>{delivery.warehouseCode} — {delivery.warehouseName}</strong>
            <span>Trạng thái</span><strong>{statusText(delivery.status)}</strong>
            {delivery.shippedAt ? <><span>Thời gian xuất</span><strong>{new Date(delivery.shippedAt).toLocaleString('vi-VN')}</strong></> : null}
          </div>
          <div className="table-scroll">
            <table className="data-table">
              <thead><tr><th>Sản phẩm</th><th>Lô</th><th>Số lượng</th></tr></thead>
              <tbody>
                {delivery.lines.map(line => (
                  <tr key={line.deliveryDetailId}>
                    <td><span className="table-main-cell"><strong>{line.sku}</strong><small>{line.productName}</small></span></td>
                    <td>{line.lotCode}</td>
                    <td>{formatQuantity(line.quantity)} {line.unitCode}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="modal-actions">
            {delivery.status === 'Draft' ? <button className="btn btn-primary" disabled={busy} onClick={() => void runAction(() => outboundApi.prepareDelivery(delivery.deliveryId))} type="button">Bắt đầu soạn hàng</button> : null}
            {delivery.status === 'Picking' ? (
              <>
                <button className="btn btn-primary" disabled={busy} onClick={() => void runAction(() => outboundApi.shipDelivery(delivery.deliveryId, { shippedByEmployeeId: null }))} type="button">Xác nhận xuất kho</button>
                <button className="btn btn-danger-ghost" disabled={busy} onClick={() => void runAction(() => outboundApi.cancelDelivery(delivery.deliveryId))} type="button">Hủy phiếu</button>
              </>
            ) : null}
            {delivery.status === 'Shipped' ? <button className="btn btn-primary" disabled={busy} onClick={() => void runAction(() => outboundApi.markDelivered(delivery.deliveryId))} type="button">Xác nhận đã giao</button> : null}
            <button className="btn btn-secondary" disabled={busy} onClick={onClose} type="button">Đóng</button>
          </div>
        </div>
      )}
    </Modal>
  )
}

function OutboundPage() {
  const [tab, setTab] = useState<OutboundTab>('orders')
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [page, setPage] = useState(1)
  const [createOpen, setCreateOpen] = useState(false)
  const [orderId, setOrderId] = useState<number | null>(null)
  const [deliveryId, setDeliveryId] = useState<number | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const optionsQuery = useApiQuery<OutboundOptionsResponse>(outboundRoutes.options)
  const options = optionsQuery.data ?? EMPTY_OPTIONS
  const listPath = queryPath(
    tab === 'orders' ? outboundRoutes.orders : outboundRoutes.deliveries,
    { search: search.trim(), status: status || undefined, page, pageSize: 25 },
  )
  const ordersQuery = useApiQuery<{ items: OutboundOrderListItemResponse[]; page: number; pageSize: number; totalCount: number }>(
    tab === 'orders' ? listPath : null,
  )
  const deliveriesQuery = useApiQuery<{ items: OutboundDeliveryListItemResponse[]; page: number; pageSize: number; totalCount: number }>(
    tab === 'deliveries' ? listPath : null,
  )
  const orders = ordersQuery.data?.items ?? EMPTY_ORDERS
  const deliveries = deliveriesQuery.data?.items ?? EMPTY_DELIVERIES
  const totalCount = tab === 'orders'
    ? ordersQuery.data?.totalCount ?? 0
    : deliveriesQuery.data?.totalCount ?? 0
  const pageCount = Math.max(1, Math.ceil(totalCount / 25))

  const orderColumns = useMemo(() => [
    { key: 'order', title: 'Đơn bán hàng', render: (row: OutboundOrderListItemResponse) => <span className="table-main-cell"><strong>{row.salesOrderNumber}</strong><small>{row.customerCode} — {row.customerName}</small></span> },
    { key: 'date', title: 'Ngày tạo', render: (row: OutboundOrderListItemResponse) => new Date(row.orderDate).toLocaleDateString('vi-VN') },
    { key: 'lines', title: 'Dòng hàng', align: 'right' as const, render: (row: OutboundOrderListItemResponse) => row.lineCount },
    { key: 'deliveries', title: 'Phiếu xuất', align: 'right' as const, render: (row: OutboundOrderListItemResponse) => row.deliveryCount },
    { key: 'status', title: 'Trạng thái', render: (row: OutboundOrderListItemResponse) => <span className="receiving-status">{statusText(row.status)}</span> },
    { key: 'actions', title: '', align: 'right' as const, render: (row: OutboundOrderListItemResponse) => <button className="btn btn-link btn-small" onClick={() => setOrderId(row.salesOrderId)} type="button">Chi tiết</button> },
  ], [])
  const deliveryColumns = useMemo(() => [
    { key: 'delivery', title: 'Phiếu xuất', render: (row: OutboundDeliveryListItemResponse) => <span className="table-main-cell"><strong>{row.deliveryNumber}</strong><small>{row.warehouseCode} — {row.warehouseName}</small></span> },
    { key: 'planned', title: 'Ngày dự kiến', render: (row: OutboundDeliveryListItemResponse) => row.plannedAt ? new Date(row.plannedAt).toLocaleDateString('vi-VN') : '—' },
    { key: 'lines', title: 'Dòng hàng', align: 'right' as const, render: (row: OutboundDeliveryListItemResponse) => row.lineCount },
    { key: 'status', title: 'Trạng thái', render: (row: OutboundDeliveryListItemResponse) => <span className="receiving-status">{statusText(row.status)}</span> },
    { key: 'actions', title: '', align: 'right' as const, render: (row: OutboundDeliveryListItemResponse) => <button className="btn btn-link btn-small" onClick={() => setDeliveryId(row.deliveryId)} type="button">Chi tiết</button> },
  ], [])
  const listError = tab === 'orders' ? ordersQuery.error : deliveriesQuery.error

  function refresh() {
    if (tab === 'orders') ordersQuery.reload()
    else deliveriesQuery.reload()
  }

  return (
    <>
      <PageHeading
        action={tab === 'orders' ? <button className="btn btn-primary" disabled={optionsQuery.loading || options.customers.length === 0} onClick={() => setCreateOpen(true)} type="button">+ Tạo đơn bán</button> : undefined}
        description="Quản lý đơn bán, giữ chỗ hàng và theo dõi quá trình xuất kho."
        eyebrow="Nghiệp vụ kho"
        title="Xuất hàng"
      />
      {notice ? <NoticeBanner message={notice} onDismiss={() => setNotice(null)} /> : null}
      {optionsQuery.error ? <ErrorBanner message={optionsQuery.error} onRetry={optionsQuery.reload} /> : null}
      {listError ? <ErrorBanner message={listError} onRetry={refresh} /> : null}
      <section className="panel receiving-workspace">
        <div className="receiving-tabs">
          <button className={`receiving-tab${tab === 'orders' ? ' active' : ''}`} onClick={() => { setTab('orders'); setPage(1) }} type="button">Đơn bán hàng</button>
          <button className={`receiving-tab${tab === 'deliveries' ? ' active' : ''}`} onClick={() => { setTab('deliveries'); setPage(1) }} type="button">Lịch sử xuất hàng</button>
        </div>
        <div className="table-toolbar">
          <div className="table-toolbar-copy">
            <strong>{tab === 'orders' ? 'Đơn bán hàng' : 'Phiếu xuất kho'}</strong>
            <span>{tab === 'orders' ? 'Tìm kiếm, duyệt và phân bổ đơn hàng.' : 'Theo dõi tiến độ soạn hàng và giao hàng.'}</span>
          </div>
          <div className="table-filters">
            <label className="search-box">
              <span aria-hidden="true">⌕</span>
              <input onChange={(event) => { setSearch(event.target.value); setPage(1) }} placeholder={tab === 'orders' ? 'Tìm mã đơn, khách hàng…' : 'Tìm mã phiếu…'} value={search} />
            </label>
            <select onChange={(event) => { setStatus(event.target.value); setPage(1) }} value={status}>
              <option value="">Tất cả trạng thái</option>
              {(tab === 'orders'
                ? [['Draft', 'Nháp'], ['Confirmed', 'Đã duyệt'], ['PartiallyDelivered', 'Giao một phần'], ['Delivered', 'Đã xuất đủ'], ['Cancelled', 'Đã hủy']]
                : [['Draft', 'Nháp'], ['Picking', 'Đang soạn hàng'], ['Shipped', 'Đã xuất kho'], ['Delivered', 'Đã giao'], ['Failed', 'Giao thất bại'], ['Cancelled', 'Đã hủy']])
                .map(([value, label]) => <option key={value} value={value}>{label}</option>)}
            </select>
          </div>
        </div>
        {tab === 'orders' ? (
          <DataTable
            columns={orderColumns}
            emptyMessage="Chưa có đơn bán hàng phù hợp."
            getKey={row => row.salesOrderId}
            loading={ordersQuery.loading}
            rows={orders}
          />
        ) : (
          <DataTable
            columns={deliveryColumns}
            emptyMessage="Chưa có phiếu xuất phù hợp."
            getKey={row => row.deliveryId}
            loading={deliveriesQuery.loading}
            rows={deliveries}
          />
        )}
        <div className="table-pagination">
          <span>Trang {page} / {pageCount}</span>
          <div>
            <button className="btn btn-secondary btn-small" disabled={page <= 1} onClick={() => setPage(current => current - 1)} type="button">Trước</button>
            <button className="btn btn-secondary btn-small" disabled={page >= pageCount} onClick={() => setPage(current => current + 1)} type="button">Sau</button>
          </div>
        </div>
      </section>
      {createOpen ? (
        <CreateOrderDialog
          onClose={() => setCreateOpen(false)}
          onSaved={(order) => {
            setCreateOpen(false)
            setOrderId(order.salesOrderId)
            setNotice(`Đã tạo đơn nháp ${order.salesOrderNumber}.`)
            refresh()
          }}
          options={options}
        />
      ) : null}
      {orderId !== null ? (
        <OrderDialog
          key={orderId}
          onChanged={refresh}
          onClose={() => setOrderId(null)}
          onOpenDelivery={(id) => { setOrderId(null); setDeliveryId(id) }}
          options={options}
          orderId={orderId}
        />
      ) : null}
      {deliveryId !== null ? (
        <DeliveryDialog
          key={deliveryId}
          deliveryId={deliveryId}
          onChanged={refresh}
          onClose={() => setDeliveryId(null)}
        />
      ) : null}
    </>
  )
}

export default OutboundPage
