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
  CategoryCreateRequest,
  CategoryResponse,
  CategoryUpdateRequest,
} from '../types/contracts'

type DialogCategory = CategoryResponse | null | undefined
const EMPTY_CATEGORIES: CategoryResponse[] = []

interface CategoryDialogProps {
  category: CategoryResponse | null
  categories: CategoryResponse[]
  onClose: () => void
  onSaved: () => void
}

function CategoryDialog({ category, categories, onClose, onSaved }: CategoryDialogProps) {
  const [categoryCode, setCategoryCode] = useState(category?.categoryCode ?? '')
  const [categoryName, setCategoryName] = useState(category?.categoryName ?? '')
  const [parentCategoryId, setParentCategoryId] = useState(
    category?.parentCategoryId === null || category?.parentCategoryId === undefined
      ? ''
      : String(category.parentCategoryId),
  )
  const [isActive, setIsActive] = useState(category?.isActive ?? true)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [validationMessages, setValidationMessages] = useState<string[]>([])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFormError(null)
    setValidationMessages([])

    const code = categoryCode.trim()
    const name = categoryName.trim()
    const parentId = parentCategoryId === '' ? null : Number(parentCategoryId)
    const errors: string[] = []
    if (code.length === 0 || code.length > 30) errors.push('Mã danh mục phải có từ 1 đến 30 ký tự.')
    if (name.length === 0 || name.length > 100) errors.push('Tên danh mục phải có từ 1 đến 100 ký tự.')
    if (parentId !== null && (!Number.isInteger(parentId) || parentId < 1)) {
      errors.push('Danh mục cha phải là ID hợp lệ.')
    }
    if (category && parentId === category.categoryId) {
      errors.push('Danh mục không thể là danh mục cha của chính nó.')
    }
    if (errors.length > 0) {
      setValidationMessages(errors)
      return
    }

    const payload = {
      categoryCode: code,
      categoryName: name,
      parentCategoryId: parentId,
    }

    setSaving(true)
    try {
      if (category) {
        const request: CategoryUpdateRequest = { ...payload, isActive }
        await catalogApi.updateCategory(category.categoryId, request)
      } else {
        const request: CategoryCreateRequest = { ...payload, isActive }
        await catalogApi.createCategory(request)
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
      description="Thông tin được lưu bằng API danh mục sản phẩm."
      onClose={onClose}
      title={category ? 'Cập nhật danh mục' : 'Thêm danh mục'}
    >
      <form className="form-stack" onSubmit={(event) => void submit(event)}>
        {formError ? <ErrorBanner message={formError} /> : null}
        <ValidationSummary messages={validationMessages} />
        <label className="form-field">
          <span>Mã danh mục <b>*</b></span>
          <input
            autoFocus
            maxLength={30}
            onChange={(event) => setCategoryCode(event.target.value)}
            required
            value={categoryCode}
          />
        </label>
        <label className="form-field">
          <span>Tên danh mục <b>*</b></span>
          <input
            maxLength={100}
            onChange={(event) => setCategoryName(event.target.value)}
            required
            value={categoryName}
          />
        </label>
        <label className="form-field">
          <span>Danh mục cha</span>
          <select onChange={(event) => setParentCategoryId(event.target.value)} value={parentCategoryId}>
            <option value="">Không có</option>
            {categories
              .filter((item) => item.categoryId !== category?.categoryId)
              .map((item) => (
                <option key={item.categoryId} value={item.categoryId}>
                  {item.categoryCode} — {item.categoryName}
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
            {saving ? 'Đang lưu…' : 'Lưu danh mục'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

function CategoriesPage() {
  const query = useApiQuery<CategoryResponse[]>(apiRoutes.categories)
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')
  const [dialogCategory, setDialogCategory] = useState<DialogCategory>(undefined)
  const [actionError, setActionError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  const categories = query.data ?? EMPTY_CATEGORIES

  const filtered = useMemo(() => {
    const normalized = search.trim().toLocaleLowerCase('vi')
    return categories.filter((category) => {
      const matchesSearch =
        normalized.length === 0
        || category.categoryCode.toLocaleLowerCase('vi').includes(normalized)
        || category.categoryName.toLocaleLowerCase('vi').includes(normalized)
      const matchesStatus =
        statusFilter === 'all'
        || (statusFilter === 'active' ? category.isActive : !category.isActive)
      return matchesSearch && matchesStatus
    })
  }, [categories, search, statusFilter])

  async function deleteCategory(category: CategoryResponse) {
    if (!window.confirm(`Xóa danh mục "${category.categoryName}"?`)) return
    setActionError(null)
    setNotice(null)
    try {
      await catalogApi.deleteCategory(category.categoryId)
      setNotice('Đã xóa danh mục.')
      query.reload()
    } catch (error) {
      setActionError(getErrorMessage(error))
    }
  }

  function saved() {
    setDialogCategory(undefined)
    setNotice(dialogCategory ? 'Đã cập nhật danh mục.' : 'Đã thêm danh mục.')
    setActionError(null)
    query.reload()
  }

  const columns = [
    {
      key: 'code',
      title: 'Mã danh mục',
      render: (item: CategoryResponse) => <span className="code-text">{item.categoryCode}</span>,
    },
    {
      key: 'name',
      title: 'Tên danh mục',
      render: (item: CategoryResponse) => <strong className="table-primary">{item.categoryName}</strong>,
    },
    {
      key: 'parent',
      title: 'Danh mục cha',
      render: (item: CategoryResponse) =>
        categories.find((candidate) => candidate.categoryId === item.parentCategoryId)?.categoryName ?? '—',
    },
    {
      key: 'status',
      title: 'Trạng thái',
      render: (item: CategoryResponse) => <StatusBadge active={item.isActive} />,
    },
    {
      key: 'actions',
      title: 'Thao tác',
      align: 'right' as const,
      render: (item: CategoryResponse) => (
        <div className="row-actions">
          <button className="btn btn-small btn-secondary" onClick={() => setDialogCategory(item)} type="button">
            Sửa
          </button>
          <button
            className="btn btn-small btn-danger-ghost"
            onClick={() => void deleteCategory(item)}
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
          <button className="btn btn-primary" onClick={() => setDialogCategory(null)} type="button">
            <span className="button-plus">+</span> Thêm danh mục
          </button>
        }
        description="Quản lý cây danh mục sản phẩm đang lưu trong hệ thống."
        title="Danh mục sản phẩm"
      />
      {notice ? <NoticeBanner message={notice} onDismiss={() => setNotice(null)} /> : null}
      {actionError ? <ErrorBanner message={actionError} /> : null}
      {query.error ? <ErrorBanner message={query.error} onRetry={query.reload} /> : null}
      <section className="panel data-panel">
        <div className="table-toolbar">
          <div className="table-toolbar-copy">
            <strong>Danh sách danh mục</strong>
            <span>{query.loading ? 'Đang đồng bộ…' : `${filtered.length} bản ghi`}</span>
          </div>
          <div className="table-filters">
            <label className="search-box">
              <span aria-hidden="true">⌕</span>
              <input
                aria-label="Tìm mã hoặc tên danh mục"
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Tìm danh mục..."
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
          emptyMessage={search ? 'Không tìm thấy danh mục phù hợp.' : 'Chưa có danh mục nào.'}
          getKey={(item) => item.categoryId}
          loading={query.loading}
          rows={filtered}
        />
      </section>
      {dialogCategory !== undefined ? (
        <CategoryDialog
          categories={categories}
          category={dialogCategory}
          key={dialogCategory?.categoryId ?? 'new'}
          onClose={() => setDialogCategory(undefined)}
          onSaved={saved}
        />
      ) : null}
    </>
  )
}

export default CategoriesPage
