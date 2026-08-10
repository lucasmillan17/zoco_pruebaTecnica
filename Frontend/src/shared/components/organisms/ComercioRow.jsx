import { MessagesSquare, Pencil, RotateCcw, Sparkles, Trash2 } from 'lucide-react';
import EstadoBadge from '../molecules/EstadoBadge';
import Badge from '../atoms/Badge';
import IconButton from '../atoms/IconButton';
import { formatearFecha } from '../../utils/format';

export default function ComercioRow({
  comercio,
  accion = null,
  onEditar,
  onVerInteracciones,
  onAnalizar,
  onReactivar,
  onEliminar,
}) {
  const deshabilitado = accion === comercio.id;

  return (
    <tr className="border-b border-gray-200 transition-colors hover:bg-gray-50">
      <td className="px-3 py-2.5">
        <p className="truncate text-sm font-medium text-gray-900" title={comercio.razonSocial}>
          {comercio.razonSocial}
        </p>
      </td>
      <td className="whitespace-nowrap px-3 py-2.5 text-sm text-gray-600">{comercio.cuit}</td>
      <td className="px-3 py-2.5 text-sm text-gray-600">{comercio.rubro || '—'}</td>
      <td className="px-3 py-2.5 text-sm text-gray-600">{comercio.nombreDelContacto || '—'}</td>
      <td className="whitespace-nowrap px-3 py-2.5 text-sm text-gray-600">{comercio.telefono || '—'}</td>
      <td className="px-3 py-2.5 text-sm text-gray-600">{comercio.email || '—'}</td>
      <td className="whitespace-nowrap px-3 py-2.5 text-sm text-gray-600">
        {formatearFecha(comercio.fechaDeCreacionEmpresa)}
      </td>
      <td className="whitespace-nowrap px-3 py-2.5">
        <div className="flex items-center gap-1.5">
          <EstadoBadge estado={comercio.estado} />
          {comercio.activo === false && (
            <Badge variant="gray" tone="soft">Inactivo</Badge>
          )}
        </div>
      </td>
      <td className="px-3 py-2.5">
        <div className="flex items-center justify-end gap-0.5">
          {onEditar && (
            <IconButton titulo="Editar" onClick={() => onEditar(comercio)} disabled={deshabilitado}>
              <Pencil className="h-4 w-4" />
            </IconButton>
          )}
          {onVerInteracciones && (
            <IconButton titulo="Interacciones" onClick={() => onVerInteracciones(comercio)} disabled={deshabilitado}>
              <MessagesSquare className="h-4 w-4" />
            </IconButton>
          )}
          {onAnalizar && (
            <IconButton titulo="Oportunidad" onClick={() => onAnalizar(comercio)} disabled={deshabilitado}>
              <Sparkles className="h-4 w-4" />
            </IconButton>
          )}
          {onReactivar && (comercio.activo === false || comercio.estado === 'Rechazado') && (
            <IconButton titulo="Reactivar" onClick={() => onReactivar(comercio)} disabled={deshabilitado}>
              <RotateCcw className="h-4 w-4" />
            </IconButton>
          )}
          {onEliminar && (
            <IconButton
              titulo="Eliminar"
              onClick={() => onEliminar(comercio)}
              disabled={deshabilitado}
              className="hover:text-danger"
            >
              <Trash2 className="h-4 w-4" />
            </IconButton>
          )}
        </div>
      </td>
    </tr>
  );
}
