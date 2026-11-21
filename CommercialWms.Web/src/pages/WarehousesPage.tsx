import { useMemo, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
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
import { warehouseApi, warehouseRoutes } from '../services/warehouseApi'
import type {
  WarehouseAisleNodeResponse,
  WarehouseBinResponse,
  WarehouseRackNodeResponse,
  WarehouseResponse,
  WarehouseShelfNodeResponse,
  WarehouseStructureResponse,
  WarehouseZoneNodeResponse,
} from '../types/contracts'

type LocationKind = 'zone' | 'aisle' | 'rack' | 'shelf' | 'bin'

const EMPTY_WAREHOUSES: WarehouseResponse[] = []

interface LocationItem {
  kind: LocationKind
  id: number
  parentId: number
  code: string
  name?: string
  capacity?: number | null
  isActive?: boolean
}

interface ParentOption {
  id: number
  label: string
}

interface LocationDefinition {
  label: string
  singular: string
  parentLabel: string
  parentKind: LocationKind | 'warehouse'
  codeLabel: string
  codeMaxLength: number
  nameMaxLength?: number
}

const locationDefinitions: Record<LocationKind, LocationDefinition> = {
  zone: {
    label: 'Khu vực',
    singular: 'khu vực',
    parentLabel: 'Kho',
    parentKind: 'warehouse',
    codeLabel: 'Mã khu vực',
    codeMaxLength: 30,
    nameMaxLength: 100,
  },
  aisle: {
    label: 'Dãy',
    singular: 'dãy',
    parentLabel: 'Khu vực',
    parentKind: 'zone',
    codeLabel: 'Mã dãy',
    codeMaxLength: 30,
  },
  rack: {
    label: 'Kệ',
    singular: 'kệ',
    parentLabel: 'Dãy',
    parentKind: 'aisle',
    codeLabel: 'Mã kệ',
    codeMaxLength: 30,
  },
  shelf: {
    label: 'Kệ con',
    singular: 'kệ con',
    parentLabel: 'Kệ',
    parentKind: 'rack',
    codeLabel: 'Mã kệ con',
    codeMaxLength: 30,
  },
  bin: {
    label: 'Vị trí',
    singular: 'vị trí',
    parentLabel: 'Kệ con',
    parentKind: 'shelf',
    codeLabel: 'Mã vị trí',
    codeMaxLength: 30,
  },
}

function mapZone(zone: WarehouseZoneNodeResponse, warehouseId: number): LocationItem {
  return {
    kind: 'zone',
    id: zone.zoneId,
    parentId: warehouseId,
    code: zone.zoneCode,
    name: zone.zoneName,
  }
}

function mapAisle(aisle: WarehouseAisleNodeResponse, zoneId: number): LocationItem {
  return { kind: 'aisle', id: aisle.aisleId, parentId: zoneId, code: aisle.aisleCode }
}

function mapRack(rack: WarehouseRackNodeResponse, aisleId: number): LocationItem {
  return { kind: 'rack', id: rack.rackId, parentId: aisleId, code: rack.rackCode }
}

function mapShelf(shelf: WarehouseShelfNodeResponse, rackId: number): LocationItem {
  return { kind: 'shelf', id: shelf.shelfId, parentId: rackId, code: shelf.shelfCode }
}

function mapBin(bin: WarehouseBinResponse): LocationItem {
  return {
    kind: 'bin',
    id: bin.binId,
    parentId: bin.shelfId,
    code: bin.binCode,
    capacity: bin.capacity,
    isActive: bin.isActive,
  }
}

interface WarehouseFormProps {
  warehouse: WarehouseResponse | null
  onClose: () => void
  onSaved: (warehouse: WarehouseResponse) => void
}

function WarehouseDialog({ warehouse, onClose, onSaved }: WarehouseFormProps) {
  const [code, setCode] = useState(warehouse?.warehouseCode ?? '')
  const [name, setName] = useState(warehouse?.warehouseName ?? '')
  const [address, setAddress] = useState(warehouse?.address ?? '')
  const [isActive, setIsActive] = useState(warehouse?.isActive ?? true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [validationMessages, setValidationMessages] = useState<string[]>([])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setValidationMessages([])
    const values = {
      warehouseCode: code.trim(),
      warehouseName: name.trim(),
      address: address.trim(),
      isActive,
    }
    const errors: string[] = []
    if (!values.warehouseCode || values.warehouseCode.length > 30) {
      errors.push('Mã kho phải có từ 1 đến 30 ký tự.')
    }
    if (!values.warehouseName || values.warehouseName.length > 150) {
      errors.push('Tên kho phải có từ 1 đến 150 ký tự.')
    }
    if (!values.address || values.address.length > 300) {
      errors.push('Địa chỉ kho phải có từ 1 đến 300 ký tự.')
    }
    if (errors.length) {
      setValidationMessages(errors)
      return
    }

    setSaving(true)
    try {
      const saved = warehouse
        ? await warehouseApi.updateWarehouse(warehouse.warehouseId, values)
        : await warehouseApi.createWarehouse(values)
      onSaved(saved)
    } catch (saveError) {
      setError(getErrorMessage(saveError))
      if (saveError instanceof ApiError) {
        setValidationMessages(Object.values(saveError.validationErrors).flat())
      }
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal
      description="Thông tin kho được lưu bằng API kho hàng."
      onClose={onClose}
      title={warehouse ? 'Cập nhật kho hàng' : 'Thêm kho hàng'}
    >
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {error ? <ErrorBanner message={error} /> : null}
        <ValidationSummary messages={validationMessages} />
        <label className="form-field">
          <span>Mã kho <b>*</b></span>
          <input autoFocus maxLength={30} onChange={(event) => setCode(event.target.value)} required value={code} />
        </label>
        <label className="form-field">
          <span>Tên kho <b>*</b></span>
          <input maxLength={150} onChange={(event) => setName(event.target.value)} required value={name} />
        </label>
        <label className="form-field">
          <span>Địa chỉ <b>*</b></span>
          <textarea maxLength={300} onChange={(event) => setAddress(event.target.value)} required rows={3} value={address} />
        </label>
        <label className="checkbox-field">
          <input checked={isActive} onChange={(event) => setIsActive(event.target.checked)} type="checkbox" />
          <span>Đang hoạt động</span>
        </label>
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">Hủy</button>
          <button className="btn btn-primary" disabled={saving} type="submit">
            {saving ? 'Đang lưu…' : 'Lưu kho'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

interface LocationDialogProps {
  kind: LocationKind
  location: LocationItem | null
  parentOptions: ParentOption[]
  initialParentId: number
  onClose: () => void
  onSaved: () => void
}

function LocationDialog({
  kind,
  location,
  parentOptions,
  initialParentId,
  onClose,
  onSaved,
}: LocationDialogProps) {
  const definition = locationDefinitions[kind]
  const [code, setCode] = useState(location?.code ?? '')
  const [name, setName] = useState(location?.name ?? '')
  const [parentId, setParentId] = useState(String(location?.parentId ?? initialParentId))
  const [capacity, setCapacity] = useState(
    location?.capacity === null || location?.capacity === undefined ? '' : String(location.capacity),
  )
  const [isActive, setIsActive] = useState(location?.isActive ?? true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [validationMessages, setValidationMessages] = useState<string[]>([])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setValidationMessages([])
    const parent = Number(parentId)
    const normalizedCode = code.trim()
    const errors: string[] = []
    if (!Number.isInteger(parent) || parent < 1 || !parentOptions.some((option) => option.id === parent)) {
      errors.push(`Vui lòng chọn ${definition.parentLabel.toLocaleLowerCase('vi')} hợp lệ.`)
    }
    if (!normalizedCode || normalizedCode.length > definition.codeMaxLength) {
      errors.push(`${definition.codeLabel} phải có từ 1 đến ${definition.codeMaxLength} ký tự.`)
    }
    if (definition.nameMaxLength && (!name.trim() || name.trim().length > definition.nameMaxLength)) {
      errors.push(`Tên khu vực phải có từ 1 đến ${definition.nameMaxLength} ký tự.`)
    }
    const parsedCapacity = capacity.trim() ? Number(capacity) : null
    if (kind === 'bin' && parsedCapacity !== null && (!Number.isFinite(parsedCapacity) || parsedCapacity <= 0)) {
      errors.push('Sức chứa phải là số dương hoặc để trống.')
    }
    if (errors.length) {
      setValidationMessages(errors)
      return
    }

    setSaving(true)
    try {
      const id = location?.id
      switch (kind) {
        case 'zone': {
          const request = { warehouseId: parent, zoneCode: normalizedCode, zoneName: name.trim() }
          if (id === undefined) await warehouseApi.createZone(request)
          else await warehouseApi.updateZone(id, request)
          break
        }
        case 'aisle': {
          const request = { zoneId: parent, aisleCode: normalizedCode }
          if (id === undefined) await warehouseApi.createAisle(request)
          else await warehouseApi.updateAisle(id, request)
          break
        }
        case 'rack': {
          const request = { aisleId: parent, rackCode: normalizedCode }
          if (id === undefined) await warehouseApi.createRack(request)
          else await warehouseApi.updateRack(id, request)
          break
        }
        case 'shelf': {
          const request = { rackId: parent, shelfCode: normalizedCode }
          if (id === undefined) await warehouseApi.createShelf(request)
          else await warehouseApi.updateShelf(id, request)
          break
        }
        case 'bin': {
          const request = { shelfId: parent, binCode: normalizedCode, capacity: parsedCapacity, isActive }
          if (id === undefined) await warehouseApi.createBin(request)
          else await warehouseApi.updateBin(id, request)
          break
        }
      }
      onSaved()
    } catch (saveError) {
      setError(getErrorMessage(saveError))
      if (saveError instanceof ApiError) {
        setValidationMessages(Object.values(saveError.validationErrors).flat())
      }
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal
      description={`Chọn ${definition.parentLabel.toLocaleLowerCase('vi')} từ cấu trúc hiện có.`}
      onClose={onClose}
      title={location ? `Cập nhật ${definition.singular}` : `Thêm ${definition.singular}`}
    >
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {error ? <ErrorBanner message={error} /> : null}
        <ValidationSummary messages={validationMessages} />
        <label className="form-field">
          <span>{definition.parentLabel} <b>*</b></span>
          <select onChange={(event) => setParentId(event.target.value)} required value={parentId}>
            <option value="">Chọn {definition.parentLabel.toLocaleLowerCase('vi')}</option>
            {parentOptions.map((option) => (
              <option key={option.id} value={option.id}>{option.label}</option>
            ))}
          </select>
        </label>
        <label className="form-field">
          <span>{definition.codeLabel} <b>*</b></span>
          <input
            autoFocus
            maxLength={definition.codeMaxLength}
            onChange={(event) => setCode(event.target.value)}
            required
            value={code}
          />
        </label>
        {kind === 'zone' ? (
          <label className="form-field">
            <span>Tên khu vực <b>*</b></span>
            <input maxLength={definition.nameMaxLength} onChange={(event) => setName(event.target.value)} required value={name} />
          </label>
        ) : null}
        {kind === 'bin' ? (
          <>
            <label className="form-field">
              <span>Sức chứa</span>
              <input min="0.001" onChange={(event) => setCapacity(event.target.value)} step="0.001" type="number" value={capacity} />
              <small>Để trống nếu schema không khai báo giới hạn sức chứa.</small>
            </label>
            <label className="checkbox-field">
              <input checked={isActive} onChange={(event) => setIsActive(event.target.checked)} type="checkbox" />
              <span>Đang hoạt động</span>
            </label>
          </>
        ) : null}
        <div className="modal-actions">
          <button className="btn btn-secondary" disabled={saving} onClick={onClose} type="button">Hủy</button>
          <button className="btn btn-primary" disabled={saving || parentOptions.length === 0} type="submit">
            {saving ? 'Đang lưu…' : 'Lưu vị trí'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

function filterStructure(zones: WarehouseZoneNodeResponse[], rawSearch: string): WarehouseZoneNodeResponse[] {
  const search = rawSearch.trim().toLocaleLowerCase('vi')
  if (!search) return zones

  const matches = (...values: (string | undefined)[]) =>
    values.some((value) => value?.toLocaleLowerCase('vi').includes(search))

  return zones.flatMap((zone) => {
    const zoneMatches = matches(zone.zoneCode, zone.zoneName)
    const aisles = zone.aisles.flatMap((aisle) => {
      const aisleMatches = matches(aisle.aisleCode)
      const racks = aisle.racks.flatMap((rack) => {
        const rackMatches = matches(rack.rackCode)
        const shelves = rack.shelves.flatMap((shelf) => {
          const shelfMatches = matches(shelf.shelfCode)
          const bins = shelf.bins.filter((bin) => matches(bin.binCode))
          if (zoneMatches || aisleMatches || rackMatches || shelfMatches || bins.length > 0) {
            return [shelfMatches || rackMatches || aisleMatches || zoneMatches ? shelf : { ...shelf, bins }]
          }
          return []
        })
        if (rackMatches || aisleMatches || zoneMatches || shelves.length > 0) {
          return [rackMatches || aisleMatches || zoneMatches ? rack : { ...rack, shelves }]
        }
        return []
      })
      if (aisleMatches || zoneMatches || racks.length > 0) {
        return [aisleMatches || zoneMatches ? aisle : { ...aisle, racks }]
      }
      return []
    })
    if (zoneMatches || aisles.length > 0) return [zoneMatches ? zone : { ...zone, aisles }]
    return []
  })
}

function WarehouseTree({
  structure,
  onAdd,
  onEdit,
  onDelete,
  onView,
}: {
  structure: WarehouseStructureResponse
  onAdd: (kind: LocationKind, parentId: number) => void
  onEdit: (item: LocationItem) => void
  onDelete: (item: LocationItem) => void
  onView: (item: LocationItem) => void
}) {
  function actions(item: LocationItem, addChild?: { kind: LocationKind; parentId: number }) {
    return (
      <div className="warehouse-node-actions">
        {addChild ? (
          <button className="btn btn-small btn-secondary" onClick={() => onAdd(addChild.kind, addChild.parentId)} type="button">
            + {locationDefinitions[addChild.kind].label}
          </button>
        ) : null}
        <button className="btn btn-small btn-secondary" onClick={() => onView(item)} type="button">Xem</button>
        <button className="btn btn-small btn-secondary" onClick={() => onEdit(item)} type="button">Sửa</button>
        <button className="btn btn-small btn-danger-ghost" onClick={() => onDelete(item)} type="button">Xóa</button>
      </div>
    )
  }

  function nodeRow(item: LocationItem, level: number, label: string, addChild?: { kind: LocationKind; parentId: number }) {
    return (
      <div className="warehouse-tree-row" key={`${item.kind}-${item.id}`} style={{ marginLeft: `${level * 20}px` }}>
        <span className="warehouse-tree-kind">{label}</span>
        <strong>{item.code}</strong>
        {item.name ? <span className="warehouse-tree-name">{item.name}</span> : null}
        {item.kind === 'bin' ? (
          <span className="warehouse-tree-meta">
            {item.capacity === null || item.capacity === undefined ? 'Sức chứa —' : `Sức chứa ${item.capacity}`}
            {' · '}
            {item.isActive ? 'Đang hoạt động' : 'Ngừng hoạt động'}
          </span>
        ) : null}
        {actions(item, addChild)}
      </div>
    )
  }

  return (
    <div className="warehouse-tree">
      {structure.zones.map((zone) => {
        const zoneItem = mapZone(zone, structure.warehouse.warehouseId)
        return (
          <section className="warehouse-tree-branch" key={zone.zoneId}>
            {nodeRow(zoneItem, 0, 'Khu vực', { kind: 'aisle', parentId: zone.zoneId })}
            {zone.aisles.map((aisle) => {
              const aisleItem = mapAisle(aisle, zone.zoneId)
              return (
                <div key={aisle.aisleId}>
                  {nodeRow(aisleItem, 1, 'Dãy', { kind: 'rack', parentId: aisle.aisleId })}
                  {aisle.racks.map((rack) => {
                    const rackItem = mapRack(rack, aisle.aisleId)
                    return (
                      <div key={rack.rackId}>
                        {nodeRow(rackItem, 2, 'Kệ', { kind: 'shelf', parentId: rack.rackId })}
                        {rack.shelves.map((shelf) => {
                          const shelfItem = mapShelf(shelf, rack.rackId)
                          return (
                            <div key={shelf.shelfId}>
                              {nodeRow(shelfItem, 3, 'Kệ con', { kind: 'bin', parentId: shelf.shelfId })}
                              {shelf.bins.map((bin) => {
                                const binItem = mapBin(bin)
                                return nodeRow(binItem, 4, 'Vị trí')
                              })}
                            </div>
                          )
                        })}
                      </div>
                    )
                  })}
                </div>
              )
            })}
          </section>
        )
      })}
      {structure.zones.length === 0 ? (
        <div className="warehouse-tree-empty">Kho chưa có khu vực nào. Thêm khu vực để bắt đầu cấu hình.</div>
      ) : null}
    </div>
  )
}

function WarehouseDetails({ warehouse, children }: { warehouse: WarehouseResponse; children: ReactNode }) {
  return (
    <div className="warehouse-detail-summary">
      <div>
        <span className="code-text">{warehouse.warehouseCode}</span>
        <h2>{warehouse.warehouseName}</h2>
        <p>{warehouse.address}</p>
      </div>
      <StatusBadge active={warehouse.isActive} />
      {children}
    </div>
  )
}

function WarehousesPage() {
  const warehousesQuery = useApiQuery<WarehouseResponse[]>(warehouseRoutes.warehouses)
  const [selectedWarehouseId, setSelectedWarehouseId] = useState<number | null>(null)
  const structureQuery = useApiQuery<WarehouseStructureResponse>(
    selectedWarehouseId === null ? null : warehouseRoutes.structure(selectedWarehouseId),
  )
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')
  const [warehousePage, setWarehousePage] = useState(1)
  const [locationSearch, setLocationSearch] = useState('')
  const [warehouseDialog, setWarehouseDialog] = useState<WarehouseResponse | null | undefined>(undefined)
  const [locationDialog, setLocationDialog] = useState<{
    kind: LocationKind
    location: LocationItem | null
    parentId: number
  } | null>(null)
  const [detailLocation, setDetailLocation] = useState<LocationItem | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const warehouses = warehousesQuery.data ?? EMPTY_WAREHOUSES

  const filteredWarehouses = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('vi')
    return warehouses.filter((warehouse) => {
      const matchesSearch =
        normalized.length === 0
        || warehouse.warehouseCode.toLocaleLowerCase('vi').includes(normalized)
        || warehouse.warehouseName.toLocaleLowerCase('vi').includes(normalized)
        || warehouse.address.toLocaleLowerCase('vi').includes(normalized)
      const matchesStatus =
        statusFilter === 'all'
        || (statusFilter === 'active' ? warehouse.isActive : !warehouse.isActive)
      return matchesSearch && matchesStatus
    })
  }, [warehouses, search, statusFilter])
  const warehousePageSize = 10
  const warehousePageCount = Math.max(1, Math.ceil(filteredWarehouses.length / warehousePageSize))
  const currentWarehousePage = Math.min(warehousePage, warehousePageCount)
  const visibleWarehouses = filteredWarehouses.slice(
    (currentWarehousePage - 1) * warehousePageSize,
    currentWarehousePage * warehousePageSize,
  )

  const structure = structureQuery.data
  const displayedZones = structure ? filterStructure(structure.zones, locationSearch) : []
  const displayedStructure = structure ? { ...structure, zones: displayedZones } : null
  const selectedWarehouse = structure?.warehouse
    ?? warehouses.find((warehouse) => warehouse.warehouseId === selectedWarehouseId)

  const parentOptions = useMemo(() => {
    if (!locationDialog) return []
    const activeStructure = structureQuery.data
    switch (locationDialog.kind) {
      case 'zone':
        return warehouses.map((warehouse) => ({
          id: warehouse.warehouseId,
          label: `${warehouse.warehouseCode} — ${warehouse.warehouseName}`,
        }))
      case 'aisle':
        return (activeStructure?.zones ?? []).map((zone) => ({
          id: zone.zoneId,
          label: `${zone.zoneCode} — ${zone.zoneName}`,
        }))
      case 'rack':
        return (activeStructure?.zones ?? []).flatMap((zone) =>
          zone.aisles.map((aisle) => ({
            id: aisle.aisleId,
            label: `${zone.zoneCode} / ${aisle.aisleCode}`,
          })),
        )
      case 'shelf':
        return (activeStructure?.zones ?? []).flatMap((zone) =>
          zone.aisles.flatMap((aisle) =>
            aisle.racks.map((rack) => ({
              id: rack.rackId,
              label: `${zone.zoneCode} / ${aisle.aisleCode} / ${rack.rackCode}`,
            })),
          ),
        )
      case 'bin':
        return (activeStructure?.zones ?? []).flatMap((zone) =>
          zone.aisles.flatMap((aisle) =>
            aisle.racks.flatMap((rack) =>
              rack.shelves.map((shelf) => ({
                id: shelf.shelfId,
                label: `${zone.zoneCode} / ${aisle.aisleCode} / ${rack.rackCode} / ${shelf.shelfCode}`,
              })),
            ),
          ),
        )
    }
  }, [locationDialog, structureQuery.data, warehouses])

  function refreshStructure() {
    structureQuery.reload()
  }

  function saveWarehouse(warehouse: WarehouseResponse) {
    const isNew = warehouseDialog === null
    setWarehouseDialog(undefined)
    setSelectedWarehouseId(warehouse.warehouseId)
    setNotice(isNew ? 'Đã thêm kho hàng.' : 'Đã cập nhật kho hàng.')
    setActionError(null)
    warehousesQuery.reload()
    if (selectedWarehouseId === warehouse.warehouseId) structureQuery.reload()
  }

  async function deleteWarehouse(warehouse: WarehouseResponse) {
    if (!window.confirm(`Xóa kho "${warehouse.warehouseName}"?`)) return
    setActionError(null)
    setNotice(null)
    try {
      await warehouseApi.deleteWarehouse(warehouse.warehouseId)
      if (selectedWarehouseId === warehouse.warehouseId) setSelectedWarehouseId(null)
      setNotice('Đã xóa kho hàng.')
      warehousesQuery.reload()
    } catch (error) {
      setActionError(getErrorMessage(error))
    }
  }

  function saveLocation() {
    const kind = locationDialog?.kind
    setLocationDialog(null)
    setActionError(null)
    setNotice(kind ? `Đã lưu ${locationDefinitions[kind].singular}.` : 'Đã lưu cấu trúc kho.')
    refreshStructure()
  }

  async function deleteLocation(location: LocationItem) {
    if (!window.confirm(`Xóa ${locationDefinitions[location.kind].singular} "${location.code}"?`)) return
    setActionError(null)
    setNotice(null)
    try {
      switch (location.kind) {
        case 'zone':
          await warehouseApi.deleteZone(location.id)
          break
        case 'aisle':
          await warehouseApi.deleteAisle(location.id)
          break
        case 'rack':
          await warehouseApi.deleteRack(location.id)
          break
        case 'shelf':
          await warehouseApi.deleteShelf(location.id)
          break
        case 'bin':
          await warehouseApi.deleteBin(location.id)
          break
      }
      setNotice(`Đã xóa ${locationDefinitions[location.kind].singular}.`)
      refreshStructure()
    } catch (error) {
      setActionError(getErrorMessage(error))
    }
  }

  function parentOptionsForEdit(kind: LocationKind, selectedParentId: number) {
    const currentOptions = parentOptions
    if (currentOptions.some((option) => option.id === selectedParentId)) return currentOptions

    const parentName = kind === 'zone'
      ? warehouses.find((warehouse) => warehouse.warehouseId === selectedParentId)?.warehouseName
      : undefined
    if (!parentName) return currentOptions
    return [...currentOptions, { id: selectedParentId, label: parentName }]
  }

  const warehouseColumns = [
    {
      key: 'warehouse',
      title: 'Kho hàng',
      render: (warehouse: WarehouseResponse) => (
        <div className="table-main-cell">
          <strong className="table-primary">{warehouse.warehouseName}</strong>
          <small className="code-text">{warehouse.warehouseCode}</small>
        </div>
      ),
    },
    { key: 'address', title: 'Địa chỉ', render: (warehouse: WarehouseResponse) => warehouse.address },
    { key: 'status', title: 'Trạng thái', render: (warehouse: WarehouseResponse) => <StatusBadge active={warehouse.isActive} /> },
    {
      key: 'actions',
      title: 'Thao tác',
      align: 'right' as const,
      render: (warehouse: WarehouseResponse) => (
        <div className="row-actions">
          <button
            className="btn btn-small btn-primary"
            onClick={() => setSelectedWarehouseId(warehouse.warehouseId)}
            type="button"
          >
            Cấu trúc
          </button>
          <button className="btn btn-small btn-secondary" onClick={() => setWarehouseDialog(warehouse)} type="button">
            Sửa
          </button>
          <button className="btn btn-small btn-danger-ghost" onClick={() => void deleteWarehouse(warehouse)} type="button">
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
          <button className="btn btn-primary" onClick={() => setWarehouseDialog(null)} type="button">
            <span className="button-plus">+</span> Thêm kho
          </button>
        }
        description="Quản lý nhiều kho và cấu trúc vị trí lưu trữ theo schema hiện có."
        title="Kho hàng và vị trí"
      />
      {notice ? <NoticeBanner message={notice} onDismiss={() => setNotice(null)} /> : null}
      {actionError ? <ErrorBanner message={actionError} /> : null}
      {warehousesQuery.error ? <ErrorBanner message={warehousesQuery.error} onRetry={warehousesQuery.reload} /> : null}
      <section className="panel data-panel warehouse-list-panel">
        <div className="table-toolbar">
          <div className="table-toolbar-copy">
            <strong>Danh sách kho</strong>
            <span>{warehousesQuery.loading ? 'Đang đồng bộ…' : `${filteredWarehouses.length} kho`}</span>
          </div>
          <div className="table-filters">
            <label className="search-box">
              <span aria-hidden="true">⌕</span>
              <input
                aria-label="Tìm kho"
                onChange={(event) => {
                  setSearch(event.target.value)
                  setWarehousePage(1)
                }}
                placeholder="Tìm mã, tên, địa chỉ..."
                value={search}
              />
            </label>
            <select
              aria-label="Lọc trạng thái kho"
              onChange={(event) => {
                setStatusFilter(event.target.value)
                setWarehousePage(1)
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
          columns={warehouseColumns}
          emptyMessage="Chưa có kho nào trong dữ liệu backend."
          getKey={(warehouse) => warehouse.warehouseId}
          loading={warehousesQuery.loading}
          rows={visibleWarehouses}
        />
        {!warehousesQuery.loading && filteredWarehouses.length > warehousePageSize ? (
          <div className="table-pagination">
            <span>Trang {currentWarehousePage} / {warehousePageCount}</span>
            <div className="row-actions">
              <button
                className="btn btn-small btn-secondary"
                disabled={currentWarehousePage <= 1}
                onClick={() => setWarehousePage((current) => current - 1)}
                type="button"
              >
                Trước
              </button>
              <button
                className="btn btn-small btn-secondary"
                disabled={currentWarehousePage >= warehousePageCount}
                onClick={() => setWarehousePage((current) => current + 1)}
                type="button"
              >
                Sau
              </button>
            </div>
          </div>
        ) : null}
      </section>

      {selectedWarehouse ? (
        <section className="panel warehouse-structure-panel">
          <WarehouseDetails warehouse={selectedWarehouse}>
            <button className="btn btn-small btn-secondary" onClick={() => setWarehouseDialog(selectedWarehouse)} type="button">
              Sửa kho
            </button>
            <button className="btn btn-small btn-primary" onClick={() => setLocationDialog({
              kind: 'zone',
              location: null,
              parentId: selectedWarehouse.warehouseId,
            })} type="button">
              + Khu vực
            </button>
          </WarehouseDetails>
          {structureQuery.error ? (
            <ErrorBanner message={structureQuery.error} onRetry={structureQuery.reload} />
          ) : null}
          <div className="warehouse-tree-toolbar">
            <div>
              <strong>Cấu trúc vị trí</strong>
              <span>Kho → Khu vực → Dãy → Kệ → Kệ con → Vị trí</span>
            </div>
            <div className="notice-box warehouse-schema-note">
              Schema hiện có WarehouseZones → WarehouseAisles → WarehouseRacks → WarehouseShelves → WarehouseBins;
              không có cấp Floor độc lập nên giao diện không tự tạo thêm cấp này.
            </div>
            <label className="search-box">
              <span aria-hidden="true">⌕</span>
              <input
                aria-label="Tìm vị trí trong cấu trúc"
                onChange={(event) => setLocationSearch(event.target.value)}
                placeholder="Tìm mã vị trí..."
                value={locationSearch}
              />
            </label>
          </div>
          {structureQuery.loading ? (
            <div className="warehouse-tree-empty"><span className="spinner" aria-hidden="true" /> Đang tải cấu trúc kho...</div>
          ) : displayedStructure ? (
            <WarehouseTree
              onAdd={(kind, parentId) => setLocationDialog({ kind, location: null, parentId })}
              onDelete={(item) => void deleteLocation(item)}
              onEdit={(item) => setLocationDialog({ kind: item.kind, location: item, parentId: item.parentId })}
              onView={setDetailLocation}
              structure={displayedStructure}
            />
          ) : null}
        </section>
      ) : (
        <section className="panel warehouse-tree-empty warehouse-select-prompt">
          Chọn một kho để xem và quản lý cấu trúc vị trí.
        </section>
      )}

      {warehouseDialog !== undefined ? (
        <WarehouseDialog
          key={warehouseDialog?.warehouseId ?? 'new'}
          onClose={() => setWarehouseDialog(undefined)}
          onSaved={saveWarehouse}
          warehouse={warehouseDialog}
        />
      ) : null}
      {locationDialog ? (
        <LocationDialog
          initialParentId={locationDialog.parentId}
          key={`${locationDialog.kind}-${locationDialog.location?.id ?? 'new'}`}
          kind={locationDialog.kind}
          location={locationDialog.location}
          onClose={() => setLocationDialog(null)}
          onSaved={saveLocation}
          parentOptions={parentOptionsForEdit(
            locationDialog.kind,
            locationDialog.location?.parentId ?? locationDialog.parentId,
          )}
        />
      ) : null}
      {detailLocation ? (
        <Modal
          description="Chi tiết vị trí từ cấu trúc kho backend."
          onClose={() => setDetailLocation(null)}
          title={`Chi tiết ${locationDefinitions[detailLocation.kind].singular}`}
        >
          <dl className="detail-list">
            <div><dt>Cấp</dt><dd>{locationDefinitions[detailLocation.kind].label}</dd></div>
            <div><dt>ID</dt><dd>{detailLocation.id}</dd></div>
            <div><dt>Mã</dt><dd>{detailLocation.code}</dd></div>
            {detailLocation.name ? <div><dt>Tên</dt><dd>{detailLocation.name}</dd></div> : null}
            <div><dt>ID cấp cha</dt><dd>{detailLocation.parentId}</dd></div>
            {detailLocation.kind === 'bin' ? (
              <>
                <div><dt>Sức chứa</dt><dd>{detailLocation.capacity ?? '—'}</dd></div>
                <div><dt>Trạng thái</dt><dd>{detailLocation.isActive ? 'Đang hoạt động' : 'Ngừng hoạt động'}</dd></div>
              </>
            ) : null}
          </dl>
        </Modal>
      ) : null}
    </>
  )
}

export default WarehousesPage
