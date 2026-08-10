import { useState } from 'react';
import { ChevronDown, ChevronUp } from 'lucide-react';
import EstadoBadge from '../molecules/EstadoBadge';
import Badge from '../atoms/Badge';
import AccionesComercio from '../molecules/AccionesComercio';
import { formatearFecha } from '../../utils/format';

export default function ComercioCard({
  comercio,
  accion = null,
  onEditar,
  onVerInteracciones,
  onAnalizar,
  onReactivar,
  onEliminar,
}) {
  const [abierto, setAbierto] = useState(false);

  const campos = [
    { etiqueta: 'CUIT', valor: comercio.cuit },
    { etiqueta: 'Rubro', valor: comercio.rubro || '—' },
    { etiqueta: 'Contacto', valor: comercio.nombreDelContacto || '—' },
    { etiqueta: 'Teléfono', valor: comercio.telefono || '—' },
    { etiqueta: 'Email', valor: comercio.email || '—' },
    { etiqueta: 'Fecha de creación', valor: formatearFecha(comercio.fechaDeCreacionEmpresa) },
  ];

  return (
    <div className="rounded-md border border-gray-200 bg-white">
      <button
        type="button"
        onClick={() => setAbierto((v) => !v)}
        aria-expanded={abierto}
        className="flex w-full items-center justify-between gap-3 px-4 py-3 text-left"
      >
        <span className="min-w-0">
          <span className="block truncate text-sm font-medium text-gray-900">{comercio.razonSocial}</span>
          <span className="mt-1 flex flex-wrap items-center gap-1.5">
            <EstadoBadge estado={comercio.estado} />
            {comercio.activo === false && (
              <Badge variant="gray" tone="soft">Inactivo</Badge>
            )}
          </span>
        </span>
        <span className="shrink-0 text-gray-400">
          {abierto ? <ChevronUp className="h-5 w-5" /> : <ChevronDown className="h-5 w-5" />}
        </span>
      </button>

      {abierto && (
        <div className="border-t border-gray-100 px-4 pb-4 pt-3">
          <dl className="grid grid-cols-1 gap-x-4 gap-y-2.5 sm:grid-cols-2">
            {campos.map((campo) => (
              <div key={campo.etiqueta}>
                <dt className="text-xs font-medium text-gray-500">{campo.etiqueta}</dt>
                <dd className="truncate text-sm text-gray-800" title={campo.valor}>
                  {campo.valor}
                </dd>
              </div>
            ))}
          </dl>

          <div className="mt-3 border-t border-gray-100 pt-3">
            <AccionesComercio
              comercio={comercio}
              accion={accion}
              onEditar={onEditar}
              onVerInteracciones={onVerInteracciones}
              onAnalizar={onAnalizar}
              onReactivar={onReactivar}
              onEliminar={onEliminar}
              conEtiquetas
            />
          </div>
        </div>
      )}
    </div>
  );
}
