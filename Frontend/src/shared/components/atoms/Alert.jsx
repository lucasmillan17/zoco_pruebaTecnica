const TONOS = {
  info: 'border-blue-200 bg-blue-50 text-blue-800',
  error: 'border-red-200 bg-red-50 text-red-800',
  success: 'border-green-200 bg-green-50 text-green-800',
};

export default function Alert({ tono = 'info', children }) {
  if (!children) return null;
  return (
    <div className={`rounded-md border px-3.5 py-2.5 text-sm ${TONOS[tono] ?? TONOS.info}`}>
      {children}
    </div>
  );
}
