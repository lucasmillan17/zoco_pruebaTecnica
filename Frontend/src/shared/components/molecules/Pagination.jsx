import Button from '../atoms/Button';
import Select from '../atoms/Select';

export default function Pagination({
  page,
  pageSize,
  total,
  totalPages,
  hasPreviousPage,
  hasNextPage,
  onPageChange,
  onPageSizeChange,
  etiqueta = 'comercios',
}) {
  return (
    <div className="mt-5 flex flex-col gap-3 border-t border-gray-200 pt-4 text-sm text-gray-500 sm:flex-row sm:items-center sm:justify-between">
      <span>
        Página {page} de {totalPages} · {total} {etiqueta}
      </span>
      <div className="flex items-center gap-2">
        <Button size="sm" disabled={!hasPreviousPage} onClick={() => onPageChange(page - 1)}>
          ← Anterior
        </Button>
        <Select
          aria-label="Comercios por página"
          className="py-1.5 text-xs"
          value={pageSize}
          onChange={(e) => {
            onPageSizeChange(Number(e.target.value));
          }}
        >
          <option value={10}>10</option>
          <option value={25}>25</option>
          <option value={50}>50</option>
        </Select>
        <Button size="sm" disabled={!hasNextPage} onClick={() => onPageChange(page + 1)}>
          Siguiente →
        </Button>
      </div>
    </div>
  );
}
