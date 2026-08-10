import { createContext, useCallback, useContext, useMemo, useRef, useState } from 'react';
import { CheckCircle2, Info, X, XCircle } from 'lucide-react';

const ToastContext = createContext(null);

const TONOS = {
  success: { icono: CheckCircle2, clases: 'border-green-200 bg-green-50 text-green-800' },
  error: { icono: XCircle, clases: 'border-red-200 bg-red-50 text-red-800' },
  info: { icono: Info, clases: 'border-blue-200 bg-blue-50 text-blue-800' },
};

const DURACION = 4000;

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);
  const contador = useRef(0);

  const quitar = useCallback((id) => {
    setToasts((actuales) => actuales.filter((t) => t.id !== id));
  }, []);

  const mostrar = useCallback(
    (mensaje, tipo) => {
      const id = ++contador.current;
      setToasts((actuales) => [...actuales, { id, mensaje, tipo }]);
      setTimeout(() => quitar(id), DURACION);
    },
    [quitar]
  );

  const value = useMemo(
    () => ({
      success: (mensaje) => mostrar(mensaje, 'success'),
      error: (mensaje) => mostrar(mensaje, 'error'),
      info: (mensaje) => mostrar(mensaje, 'info'),
    }),
    [mostrar]
  );

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="pointer-events-none fixed right-4 top-4 z-[60] flex w-full max-w-sm flex-col gap-2">
        {toasts.map((t) => (
          <ToastItem key={t.id} toast={t} onCerrar={() => quitar(t.id)} />
        ))}
      </div>
    </ToastContext.Provider>
  );
}

function ToastItem({ toast, onCerrar }) {
  const config = TONOS[toast.tipo] ?? TONOS.info;
  const Icono = config.icono;
  return (
    <div
      className={`pointer-events-auto flex items-start gap-2.5 rounded-md border px-3.5 py-2.5 text-sm shadow-sm ${config.clases}`}
    >
      <Icono className="mt-0.5 h-4 w-4 shrink-0" />
      <p className="min-w-0 flex-1">{toast.mensaje}</p>
      <button
        type="button"
        onClick={onCerrar}
        aria-label="Cerrar notificación"
        className="shrink-0 rounded p-0.5 opacity-60 transition-opacity hover:opacity-100"
      >
        <X className="h-3.5 w-3.5" />
      </button>
    </div>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast debe usarse dentro de un ToastProvider');
  return ctx;
}
