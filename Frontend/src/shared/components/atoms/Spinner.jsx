export default function Spinner({ className = '' }) {
  return (
    <span
      role="status"
      aria-label="Cargando"
      className={`inline-block h-5 w-5 animate-spin rounded-full border-2 border-gray-300 border-t-primary ${className}`}
    />
  );
}
