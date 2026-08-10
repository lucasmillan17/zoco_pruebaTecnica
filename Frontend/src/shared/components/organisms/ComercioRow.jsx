import EstadoBadge from '../molecules/EstadoBadge';
import Badge from '../atoms/Badge';
import AccionesComercio from '../molecules/AccionesComercio';
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
        <AccionesComercio
          comercio={comercio}
          accion={accion}
          onEditar={onEditar}
          onVerInteracciones={onVerInteracciones}
          onAnalizar={onAnalizar}
          onReactivar={onReactivar}
          onEliminar={onEliminar}
        />
      </td>
    </tr>
  );
}
