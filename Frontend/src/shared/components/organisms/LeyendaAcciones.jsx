import { MessagesSquare, Pencil, RotateCcw, Sparkles, Trash2 } from 'lucide-react';

const ACCIONES = [
  { icono: Pencil, etiqueta: 'Editar' },
  { icono: MessagesSquare, etiqueta: 'Interacciones' },
  { icono: Sparkles, etiqueta: 'Oportunidad' },
  { icono: RotateCcw, etiqueta: 'Reactivar' },
  { icono: Trash2, etiqueta: 'Eliminar' },
];

export default function LeyendaAcciones() {
  return (
    <div className="flex flex-wrap items-center justify-end gap-x-4 gap-y-1 text-xs text-muted">
      {ACCIONES.map(({ icono: Icono, etiqueta }) => (
        <span key={etiqueta} className="inline-flex items-center gap-1.5">
          <Icono className="h-3.5 w-3.5" />
          {etiqueta}
        </span>
      ))}
    </div>
  );
}
