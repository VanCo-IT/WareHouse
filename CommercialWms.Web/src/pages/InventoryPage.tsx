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
import {
  inventoryApi,
  inventoryRoutes,
} from '../services/inventoryApi'
import { apiRoutes } from '../services/catalogApi'
import { receivingRoutes } from '../services/receivingApi'
import { warehouseRoutes } from '../services/warehouseApi'
import type {
  CreateInventoryAdjustmentRequest,
  CreateInventoryTransferRequest,
  InventoryStockPageResponse,
  InventoryStockResponse,
  InventoryTransactionPageResponse,
  ProductResponse,
  ReceivingBinOptionResponse,
  WarehouseResponse,
} from '../types/contracts'

type InventoryTab = 'stock' | 'history'

const EMPTY_PRODUCTS: ProductResponse[] = []
const EMPTY_WAREHOUSES: WarehouseResponse[] = []
const EMPTY_BINS: ReceivingBinOptionResponse[] = []
const PAGE_SIZE = 25

function formatQuantity(value: number) {
  return new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 3 }).format(value)
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' })
    .format(new Date(value))
}

function queryPath(path: string, values: Record<string, string | number | boolean | undefined>) {
  const query = new URLSearchParams()
  Object.entries(values).forEach(([key, value]) => {
    if (value !== undefined && String(value).length > 0) query.set(key, String(value))
  })
  const suffix = query.toString()
  return suffix.length > 0 ? `${path}?${suffix}` : path
}

function AdjustmentDialog({
  stock,
  onClose,
  onSaved,
}: {
  stock: InventoryStockResponse
  onClose: () => void
  onSaved: () => void
}) {
  const [quantityChange, setQuantityChange] = useState('')
  const [reason, setReason] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    const quantity = Number(quantityChange)
    if (!Number.isFinite(quantity) || quantity === 0 || Math.round(quantity * 1000) !== quantity * 1000) {
      setError('Nhập lượng điều chỉnh khác 0, tối đa 3 chữ số thập phân.')
      return
    }

    setSaving(true)
    try {
      const request: CreateInventoryAdjustmentRequest = {
        lotId: stock.lotId,
        binId: stock.binId,
        quantityChange: quantity,
        createdByEmployeeId: null,
        reason: reason.trim(),
      }
      await inventoryApi.createAdjustment(request)
      onSaved()
    } catch (reason) {
      setError(getErrorMessage(reason))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal
      description={`${stock.sku} · Lô ${stock.lotCode} · ${stock.warehouseCode} / ${stock.binCode}`}
      onClose={onClose}
      title="Điều chỉnh tồn kho"
    >
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {error ? <ErrorBanner message={error} /> : null}
        <div className="notice-box">
          Tồn hiện tại {formatQuantity(stock.quantityOnHand)}; giữ chỗ {formatQuantity(stock.quantityReserved)};
          khả dụng {formatQuantity(stock.quantityAvailable)}.
        </div>
        <label className="form-field">
          <span>Lượng thay đổi (+ tăng, − giảm) <b>*</b></span>
          <input
            max="999999999999999.999"
            min="-999999999999999.999"
            onChange={(event) => setQuantityChange(event.target.value)}
            required
            step="0.001"
            type="number"
            value={quantityChange}
          />
        </label>
        <label className="form-field">
          <span>Lý do điều chỉnh <b>*</b></span>
          <textarea
            maxLength={300}
            minLength={3}
            onChange={(event) => setReason(event.target.value)}
            required
            rows={3}
            value={reason}
          />
        </label>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">Hủy</button>
          <button className="btn btn-primary" disabled={saving} type="submit">
            {saving ? 'Đang lưu…' : 'Ghi điều chỉnh'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

function TransferDialog({
  stock,
  warehouses,
  onClose,
  onSaved,
}: {
  stock: InventoryStockResponse
  warehouses: WarehouseResponse[]
  onClose: () => void
  onSaved: () => void
}) {
  const [warehouseId, setWarehouseId] = useState('')
  const [binId, setBinId] = useState('')
  const [quantity, setQuantity] = useState('')
  const [reason, setReason] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)
  const parsedWarehouseId = Number(warehouseId)
  const binsQuery = useApiQuery<ReceivingBinOptionResponse[]>(
    parsedWarehouseId > 0 ? receivingRoutes.warehouseBins(parsedWarehouseId) : null,
  )
  const bins = binsQuery.data ?? EMPTY_BINS

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    const moveQuantity = Number(quantity)
    if (!Number.isFinite(moveQuantity)
      || moveQuantity <= 0
      || moveQuantity > stock.quantityAvailable
      || Math.round(moveQuantity * 1000) !== moveQuantity * 1000) {
      setError('Số lượng phải dương, tối đa 3 chữ số thập phân và không vượt tồn khả dụng.')
      return
    }
    if (!Number.isInteger(parsedWarehouseId) || parsedWarehouseId < 1
      || !Number.isInteger(Number(binId)) || Number(binId) < 1) {
      setError('Chọn kho và vị trí đích hợp lệ.')
      return
    }

    setSaving(true)
    try {
      const request: CreateInventoryTransferRequest = {
        lotId: stock.lotId,
        sourceBinId: stock.binId,
        destinationBinId: Number(binId),
        quantity: moveQuantity,
        createdByEmployeeId: null,
        reason: reason.trim(),
      }
      await inventoryApi.createTransfer(request)
      onSaved()
    } catch (reason) {
      setError(getErrorMessage(reason))
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal
      description={`${stock.sku} · Lô ${stock.lotCode} · Nguồn ${stock.warehouseCode} / ${stock.binCode}`}
      onClose={onClose}
      title="Chuyển tồn kho"
    >
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {error ? <ErrorBanner message={error} /> : null}
        <div className="notice-box">
          Khả dụng tại vị trí nguồn: {formatQuantity(stock.quantityAvailable)}.
          Hai bút toán chuyển đi/chuyển đến được ghi trong cùng một transaction.
        </div>
        <div className="form-grid">
          <label className="form-field">
            <span>Kho đích <b>*</b></span>
            <select
              onChange={(event) => {
                setWarehouseId(event.target.value)
                setBinId('')
              }}
              required
              value={warehouseId}
            >
              <option value="">Chọn kho</option>
              {warehouses.filter((warehouse) => warehouse.isActive).map((warehouse) => (
                <option key={warehouse.warehouseId} value={warehouse.warehouseId}>
                  {warehouse.warehouseCode} — {warehouse.warehouseName}
                </option>
              ))}
            </select>
          </label>
          <label className="form-field">
            <span>Vị trí đích <b>*</b></span>
            <select
              disabled={!warehouseId || binsQuery.loading}
              onChange={(event) => setBinId(event.target.value)}
              required
              value={binId}
            >
              <option value="">
                {binsQuery.loading ? 'Đang tải vị trí…' : 'Chọn vị trí'}
              </option>
              {bins.map((bin) => (
                <option key={bin.binId} value={bin.binId}>
                  {bin.binCode} — {bin.zoneCode}/{bin.aisleCode}/{bin.rackCode}/{bin.shelfCode}
                </option>
              ))}
            </select>
            {binsQuery.error ? <small className="field-error">{binsQuery.error}</small> : null}
          </label>
          <label className="form-field">
            <span>Số lượng <b>*</b></span>
            <input
              max={stock.quantityAvailable}
              min="0.001"
              onChange={(event) => setQuantity(event.target.value)}
              required
              step="0.001"
              type="number"
              value={quantity}
            />
          </label>
          <label className="form-field">
            <span>Lý do chuyển <b>*</b></span>
            <input
              maxLength={300}
              minLength={3}
              onChange={(event) => setReason(event.target.value)}
              required
              value={reason}
            />
          </label>
        </div>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">Hủy</button>
          <button className="btn btn-primary" disabled={saving || binsQuery.loading} type="submit">
            {saving ? 'Đang chuyển…' : 'Xác nhận chuyển'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

export default function InventoryPage() {
  const [tab, setTab] = useState<InventoryTab>('stock')
  const [search, setSearch] = useState('')
  const [warehouseId, setWarehouseId] = useState('')
  const [productId, setProductId] = useState('')
  const [availableOnly, setAvailableOnly] = useState(false)
  const [transactionType, setTransactionType] = useState('')
  const [binId, setBinId] = useState('')
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [page, setPage] = useState(1)
  const [selectedStock, setSelectedStock] = useState<InventoryStockResponse | null>(null)
  const [dialog, setDialog] = useState<'adjustment' | 'transfer' | null>(null)
  const [notice, setNotice] = useState<string | null>(null)

  const stockPath = useMemo(() => queryPath(inventoryRoutes.stock, {
    search,
    warehouseId: warehouseId ? Number(warehouseId) : undefined,
    productId: productId ? Number(productId) : undefined,
    binId: binId ? Number(binId) : undefined,
    availableOnly,
    page,
    pageSize: PAGE_SIZE,
  }), [availableOnly, binId, page, productId, search, warehouseId])
  const selectedWarehouseId = Number(warehouseId)
  const binsQuery = useApiQuery<ReceivingBinOptionResponse[]>(
    selectedWarehouseId > 0 ? receivingRoutes.warehouseBins(selectedWarehouseId) : null,
  )
  const historyPath = useMemo(() => tab === 'history'
    ? queryPath(inventoryRoutes.transactions, {
      warehouseId: warehouseId ? Number(warehouseId) : undefined,
      productId: productId ? Number(productId) : undefined,
      transactionType,
      fromDate: fromDate || undefined,
      toDate: toDate || undefined,
      page,
      pageSize: PAGE_SIZE,
    })
    : null, [fromDate, page, productId, tab, toDate, transactionType, warehouseId])
  const stockQuery = useApiQuery<InventoryStockPageResponse>(stockPath)
  const historyQuery = useApiQuery<InventoryTransactionPageResponse>(historyPath)
  const productsQuery = useApiQuery<ProductResponse[]>(apiRoutes.products)
  const warehousesQuery = useApiQuery<WarehouseResponse[]>(warehouseRoutes.warehouses)
  const products = productsQuery.data ?? EMPTY_PRODUCTS
  const warehouses = warehousesQuery.data ?? EMPTY_WAREHOUSES
  const stock = stockQuery.data
  const history = historyQuery.data

  function clearFilters() {
    setSearch('')
    setWarehouseId('')
    setProductId('')
    setBinId('')
    setAvailableOnly(false)
    setTransactionType('')
    setFromDate('')
    setToDate('')
    setPage(1)
  }

  function saved(message: string) {
    setDialog(null)
    setSelectedStock(null)
    setNotice(message)
    stockQuery.reload()
    historyQuery.reload()
  }

  const stockColumns = [
    {
      key: 'product',
      title: 'Sản phẩm / Lô',
      render: (row: InventoryStockResponse) => (
        <div className="table-main-cell">
          <strong>{row.sku} — {row.productName}</strong>
          <small>Lô {row.lotCode} · {row.manufacturerName} · {row.countryName}</small>
        </div>
      ),
    },
    {
      key: 'location',
      title: 'Kho / Vị trí',
      render: (row: InventoryStockResponse) => (
        <div className="table-main-cell">
          <strong>{row.warehouseCode} — {row.warehouseName}</strong>
          <small>{row.locationCode}/{row.binCode}</small>
        </div>
      ),
    },
    {
      key: 'onHand',
      title: 'Tồn thực tế',
      align: 'right' as const,
      render: (row: InventoryStockResponse) => formatQuantity(row.quantityOnHand),
    },
    {
      key: 'reserved',
      title: 'Giữ chỗ',
      align: 'right' as const,
      render: (row: InventoryStockResponse) => formatQuantity(row.quantityReserved),
    },
    {
      key: 'available',
      title: 'Khả dụng',
      align: 'right' as const,
      render: (row: InventoryStockResponse) => (
        <strong className={row.quantityAvailable > 0 ? 'inventory-positive' : 'inventory-zero'}>
          {formatQuantity(row.quantityAvailable)}
        </strong>
      ),
    },
    {
      key: 'actions',
      title: 'Thao tác',
      align: 'right' as const,
      render: (row: InventoryStockResponse) => (
        <div className="table-actions">
          <button
            className="btn btn-small btn-secondary"
            onClick={() => {
              setSelectedStock(row)
              setDialog('adjustment')
            }}
            type="button"
          >
            Điều chỉnh
          </button>
          <button
            className="btn btn-small btn-secondary"
            disabled={row.quantityAvailable <= 0}
            onClick={() => {
              setSelectedStock(row)
              setDialog('transfer')
            }}
            type="button"
          >
            Chuyển
          </button>
        </div>
      ),
    },
  ]

  const historyColumns = [
    {
      key: 'occurredAt',
      title: 'Thời gian',
      render: (row: InventoryTransactionPageResponse['items'][number]) => formatDate(row.occurredAt),
    },
    {
      key: 'product',
      title: 'Sản phẩm / Lô',
      render: (row: InventoryTransactionPageResponse['items'][number]) => (
        <div className="table-main-cell">
          <strong>{row.sku} — {row.productName}</strong>
          <small>Lô {row.lotCode}</small>
        </div>
      ),
    },
    {
      key: 'location',
      title: 'Kho / Vị trí',
      render: (row: InventoryTransactionPageResponse['items'][number]) => (
        <span>{row.warehouseCode}{row.binCode ? ` / ${row.binCode}` : ''}</span>
      ),
    },
    {
      key: 'type',
      title: 'Loại giao dịch',
      render: (row: InventoryTransactionPageResponse['items'][number]) => row.transactionType,
    },
    {
      key: 'quantity',
      title: 'Thay đổi',
      align: 'right' as const,
      render: (row: InventoryTransactionPageResponse['items'][number]) => (
        <strong className={row.quantityChange >= 0 ? 'inventory-positive' : 'inventory-zero'}>
          {row.quantityChange > 0 ? '+' : ''}{formatQuantity(row.quantityChange)}
        </strong>
      ),
    },
    {
      key: 'note',
      title: 'Ghi chú',
      render: (row: InventoryTransactionPageResponse['items'][number]) => row.note ?? '—',
    },
  ]

  return (
    <>
      <PageHeading
        description="Theo dõi tồn theo sản phẩm, lô, kho và vị trí; ghi nhận điều chỉnh và chuyển kho."
        eyebrow="Nghiệp vụ kho"
        title="Tồn kho"
      />
      {notice ? <NoticeBanner message={notice} onDismiss={() => setNotice(null)} /> : null}
      <div className="alert alert-info inventory-info">
        Tồn khả dụng = tồn thực tế − giữ chỗ. Schema có bảng ProductWarehousePolicies với ngưỡng tối thiểu,
        tối đa và an toàn; API và giao diện quản lý các ngưỡng này chưa được tích hợp nên chưa có cảnh báo theo ngưỡng.
        Điều chỉnh và chuyển kho được lưu thành lịch sử giao dịch.
      </div>
      {productsQuery.error ? <ErrorBanner message={productsQuery.error} onRetry={productsQuery.reload} /> : null}
      {warehousesQuery.error ? <ErrorBanner message={warehousesQuery.error} onRetry={warehousesQuery.reload} /> : null}
      <section className="panel inventory-workspace">
        <div className="inventory-tabs" role="tablist" aria-label="Tồn kho">
          <button
            aria-selected={tab === 'stock'}
            className={tab === 'stock' ? 'inventory-tab active' : 'inventory-tab'}
            onClick={() => {
              setTab('stock')
              setPage(1)
            }}
            role="tab"
            type="button"
          >
            Số dư tồn
          </button>
          <button
            aria-selected={tab === 'history'}
            className={tab === 'history' ? 'inventory-tab active' : 'inventory-tab'}
            onClick={() => {
              setTab('history')
              setPage(1)
            }}
            role="tab"
            type="button"
          >
            Lịch sử giao dịch
          </button>
        </div>
        <div className="inventory-toolbar">
          <label>
            <span>Tìm kiếm</span>
            <input
              onChange={(event) => {
                setSearch(event.target.value)
                setPage(1)
              }}
              placeholder="SKU, sản phẩm, lô, kho hoặc vị trí"
              value={search}
            />
          </label>
          <label>
            <span>Kho</span>
            <select
              onChange={(event) => {
                setWarehouseId(event.target.value)
                setBinId('')
                setPage(1)
              }}
              value={warehouseId}
            >
              <option value="">Tất cả kho</option>
              {warehouses.filter((warehouse) => warehouse.isActive).map((warehouse) => (
                <option key={warehouse.warehouseId} value={warehouse.warehouseId}>
                  {warehouse.warehouseCode} — {warehouse.warehouseName}
                </option>
              ))}
            </select>
          </label>
          <label>
            <span>Vị trí</span>
            <select
              disabled={!warehouseId || binsQuery.loading}
              onChange={(event) => {
                setBinId(event.target.value)
                setPage(1)
              }}
              value={binId}
            >
              <option value="">{binsQuery.loading ? 'Đang tải vị trí…' : 'Tất cả vị trí'}</option>
              {(binsQuery.data ?? EMPTY_BINS).map((bin) => (
                <option key={bin.binId} value={bin.binId}>
                  {bin.binCode} — {bin.zoneCode}/{bin.aisleCode}/{bin.rackCode}/{bin.shelfCode}
                </option>
              ))}
            </select>
          </label>
          <label>
            <span>Sản phẩm</span>
            <select
              onChange={(event) => {
                setProductId(event.target.value)
                setPage(1)
              }}
              value={productId}
            >
              <option value="">Tất cả sản phẩm</option>
              {products.map((product) => (
                <option key={product.productId} value={product.productId}>
                  {product.sku} — {product.productName}
                </option>
              ))}
            </select>
          </label>
          {tab === 'stock' ? (
            <label className="inventory-checkbox">
              <input
                checked={availableOnly}
                onChange={(event) => {
                  setAvailableOnly(event.target.checked)
                  setPage(1)
                }}
                type="checkbox"
              />
              Chỉ khả dụng
            </label>
          ) : (
            <>
              <label>
                <span>Giao dịch</span>
                <select
                  onChange={(event) => {
                    setTransactionType(event.target.value)
                    setPage(1)
                  }}
                  value={transactionType}
                >
                  <option value="">Tất cả</option>
                  <option value="Receipt">Nhập kho</option>
                  <option value="Issue">Xuất kho</option>
                  <option value="Adjustment">Điều chỉnh</option>
                  <option value="TransferOut">Chuyển đi</option>
                  <option value="TransferIn">Chuyển đến</option>
                  <option value="Return">Trả hàng</option>
                </select>
              </label>
              <label>
                <span>Từ ngày</span>
                <input
                  onChange={(event) => {
                    setFromDate(event.target.value)
                    setPage(1)
                  }}
                  type="date"
                  value={fromDate}
                />
              </label>
              <label>
                <span>Đến ngày</span>
                <input
                  onChange={(event) => {
                    setToDate(event.target.value)
                    setPage(1)
                  }}
                  type="date"
                  value={toDate}
                />
              </label>
            </>
          )}
          <button className="btn btn-small btn-secondary" onClick={clearFilters} type="button">Xóa lọc</button>
        </div>
        {tab === 'stock' ? (
          <>
            {stockQuery.error ? <ErrorBanner message={stockQuery.error} onRetry={stockQuery.reload} /> : null}
            {stock ? (
              <div className="inventory-summary">
                <span><small>Tồn thực tế</small><strong>{formatQuantity(stock.totalOnHand)}</strong></span>
                <span><small>Giữ chỗ</small><strong>{formatQuantity(stock.totalReserved)}</strong></span>
                <span><small>Khả dụng</small><strong>{formatQuantity(stock.totalAvailable)}</strong></span>
                <span><small>Dòng tồn</small><strong>{stock.totalCount}</strong></span>
              </div>
            ) : null}
            <DataTable
              columns={stockColumns}
              emptyMessage={stockQuery.error ? 'Không tải được dữ liệu tồn kho.' : 'Không có số dư tồn kho phù hợp.'}
              getKey={(row) => row.balanceId}
              loading={stockQuery.loading}
              rows={stock?.items ?? []}
            />
            <div className="inventory-pagination">
              <span>{stock ? `${stock.totalCount} dòng tồn` : ''}</span>
              <div>
                <button
                  className="btn btn-small btn-secondary"
                  disabled={page <= 1 || stockQuery.loading}
                  onClick={() => setPage((current) => current - 1)}
                  type="button"
                >
                  Trước
                </button>
                <span>Trang {page}</span>
                <button
                  className="btn btn-small btn-secondary"
                  disabled={!stock || page * PAGE_SIZE >= stock.totalCount || stockQuery.loading}
                  onClick={() => setPage((current) => current + 1)}
                  type="button"
                >
                  Tiếp
                </button>
              </div>
            </div>
          </>
        ) : (
          <>
            {historyQuery.error ? <ErrorBanner message={historyQuery.error} onRetry={historyQuery.reload} /> : null}
            <DataTable
              columns={historyColumns}
              emptyMessage={historyQuery.error ? 'Không tải được lịch sử.' : 'Chưa có giao dịch phù hợp.'}
              getKey={(row) => row.transactionId}
              loading={historyQuery.loading}
              rows={history?.items ?? []}
            />
            <div className="inventory-pagination">
              <span>{history ? `${history.totalCount} giao dịch` : ''}</span>
              <div>
                <button
                  className="btn btn-small btn-secondary"
                  disabled={page <= 1 || historyQuery.loading}
                  onClick={() => setPage((current) => current - 1)}
                  type="button"
                >
                  Trước
                </button>
                <span>Trang {page}</span>
                <button
                  className="btn btn-small btn-secondary"
                  disabled={!history || page * PAGE_SIZE >= history.totalCount || historyQuery.loading}
                  onClick={() => setPage((current) => current + 1)}
                  type="button"
                >
                  Tiếp
                </button>
              </div>
            </div>
          </>
        )}
      </section>
      {dialog === 'adjustment' && selectedStock ? (
        <AdjustmentDialog
          onClose={() => {
            setDialog(null)
            setSelectedStock(null)
          }}
          onSaved={() => saved('Điều chỉnh tồn kho đã được ghi nhận.')}
          stock={selectedStock}
        />
      ) : null}
      {dialog === 'transfer' && selectedStock ? (
        <TransferDialog
          onClose={() => {
            setDialog(null)
            setSelectedStock(null)
          }}
          onSaved={() => saved('Chuyển kho đã được ghi nhận.')}
          stock={selectedStock}
          warehouses={warehouses}
        />
      ) : null}
    </>
  )
}
