import type { ReactNode } from 'react'

interface PageHeadingProps {
  eyebrow?: string
  title: string
  description: string
  action?: ReactNode
}

export function PageHeading({ eyebrow = 'Danh mục', title, description, action }: PageHeadingProps) {
  return (
    <div className="page-heading">
      <div>
        <div className="eyebrow">{eyebrow}</div>
        <h1 className="page-title">{title}</h1>
        <p className="page-subtitle">{description}</p>
      </div>
      {action ? <div className="page-heading-action">{action}</div> : null}
    </div>
  )
}

export interface TableColumn<T> {
  key: string
  title: string
  render: (row: T) => ReactNode
  align?: 'left' | 'right' | 'center'
}

interface DataTableProps<T> {
  columns: TableColumn<T>[]
  rows: T[]
  getKey: (row: T) => string | number
  loading: boolean
  emptyMessage: string
}

export function DataTable<T>({
  columns,
  rows,
  getKey,
  loading,
  emptyMessage,
}: DataTableProps<T>) {
  return (
    <div className="table-scroll">
      <table className="data-table">
        <thead>
          <tr>
            {columns.map((column) => (
              <th className={`align-${column.align ?? 'left'}`} key={column.key} scope="col">
                {column.title}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {loading ? (
            <tr>
              <td className="table-message" colSpan={columns.length}>
                <span className="spinner" aria-hidden="true" />
                Đang tải dữ liệu...
              </td>
            </tr>
          ) : rows.length === 0 ? (
            <tr>
              <td className="table-message" colSpan={columns.length}>
                {emptyMessage}
              </td>
            </tr>
          ) : (
            rows.map((row) => (
              <tr key={getKey(row)}>
                {columns.map((column) => (
                  <td className={`align-${column.align ?? 'left'}`} key={column.key}>
                    {column.render(row)}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  )
}

interface ModalProps {
  title: string
  description?: string
  onClose: () => void
  children: ReactNode
  wide?: boolean
}

export function Modal({ title, description, onClose, children, wide = false }: ModalProps) {
  return (
    <div
      className="modal-backdrop"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) {
          onClose()
        }
      }}
      role="presentation"
    >
      <section
        aria-labelledby="modal-title"
        aria-modal="true"
        className={`modal-panel${wide ? ' modal-wide' : ''}`}
        role="dialog"
      >
        <div className="modal-heading">
          <div>
            <h2 id="modal-title">{title}</h2>
            {description ? <p>{description}</p> : null}
          </div>
          <button aria-label="Đóng" className="icon-button" onClick={onClose} type="button">
            ×
          </button>
        </div>
        {children}
      </section>
    </div>
  )
}

export function ErrorBanner({ message, onRetry }: { message: string; onRetry?: () => void }) {
  return (
    <div className="alert alert-error" role="alert">
      <span>{message}</span>
      {onRetry ? (
        <button className="text-button" onClick={onRetry} type="button">
          Thử lại
        </button>
      ) : null}
    </div>
  )
}

export function NoticeBanner({ message, onDismiss }: { message: string; onDismiss: () => void }) {
  return (
    <div className="alert alert-success" role="status">
      <span>{message}</span>
      <button aria-label="Đóng thông báo" className="icon-button" onClick={onDismiss} type="button">
        ×
      </button>
    </div>
  )
}

export function ValidationSummary({ messages }: { messages: string[] }) {
  if (messages.length === 0) {
    return null
  }

  return (
    <div className="alert alert-error validation-summary" role="alert">
      <ul>
        {messages.map((message, index) => (
          <li key={`${message}-${index}`}>{message}</li>
        ))}
      </ul>
    </div>
  )
}

export function StatusBadge({ active }: { active: boolean }) {
  return (
    <span className={`status-badge ${active ? 'status-active' : 'status-inactive'}`}>
      <span className="status-dot" />
      {active ? 'Đang hoạt động' : 'Ngừng hoạt động'}
    </span>
  )
}

export function EmptyState({ title, detail }: { title: string; detail: string }) {
  return (
    <div className="empty-state">
      <span className="empty-mark">—</span>
      <strong>{title}</strong>
      <span>{detail}</span>
    </div>
  )
}

export function FieldError({ message }: { message?: string }) {
  return message ? <span className="field-error">{message}</span> : null
}
