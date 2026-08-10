import { MessagesSquare, Pencil, RotateCcw, Sparkles, Trash2 } from 'lucide-react';
import Button from '../atoms/Button';
import IconButton from '../atoms/IconButton';

export default function AccionesComercio({
  comercio,
  accion = null,
  onEditar,
  onVerInteracciones,
  onAnalizar,
  onReactivar,
  onEliminar,
  conEtiquetas = false,
}) {
  const deshabilitado = accion === comercio.id;
  const mostrarReactivar = onReactivar && (comercio.activo === false || comercio.estado === 'Rechazado');
  const clsIcono = 'h-4 w-4';

  if (conEtiquetas) {
    return (
      <div className="flex flex-wrap gap-2">
        {onEditar && (
          <Button size="sm" onClick={() => onEditar(comercio)} disabled={deshabilitado}>
            <Pencil className={clsIcono} /> Editar
          </Button>
        )}
        {onVerInteracciones && (
          <Button size="sm" onClick={() => onVerInteracciones(comercio)} disabled={deshabilitado}>
            <MessagesSquare className={clsIcono} /> Interacciones
          </Button>
        )}
        {onAnalizar && (
          <Button size="sm" onClick={() => onAnalizar(comercio)} disabled={deshabilitado}>
            <Sparkles className={clsIcono} /> Oportunidad
          </Button>
        )}
        {mostrarReactivar && (
          <Button size="sm" onClick={() => onReactivar(comercio)} disabled={deshabilitado}>
            <RotateCcw className={clsIcono} /> Reactivar
          </Button>
        )}
        {onEliminar && (
          <Button size="sm" variant="danger" onClick={() => onEliminar(comercio)} disabled={deshabilitado}>
            <Trash2 className={clsIcono} /> Eliminar
          </Button>
        )}
      </div>
    );
  }

  return (
    <div className="flex items-center justify-end gap-0.5">
      {onEditar && (
        <IconButton titulo="Editar" onClick={() => onEditar(comercio)} disabled={deshabilitado}>
          <Pencil className={clsIcono} />
        </IconButton>
      )}
      {onVerInteracciones && (
        <IconButton titulo="Interacciones" onClick={() => onVerInteracciones(comercio)} disabled={deshabilitado}>
          <MessagesSquare className={clsIcono} />
        </IconButton>
      )}
      {onAnalizar && (
        <IconButton titulo="Oportunidad" onClick={() => onAnalizar(comercio)} disabled={deshabilitado}>
          <Sparkles className={clsIcono} />
        </IconButton>
      )}
      {mostrarReactivar && (
        <IconButton titulo="Reactivar" onClick={() => onReactivar(comercio)} disabled={deshabilitado}>
          <RotateCcw className={clsIcono} />
        </IconButton>
      )}
      {onEliminar && (
        <IconButton
          titulo="Eliminar"
          onClick={() => onEliminar(comercio)}
          disabled={deshabilitado}
          className="hover:text-danger"
        >
          <Trash2 className={clsIcono} />
        </IconButton>
      )}
    </div>
  );
}
