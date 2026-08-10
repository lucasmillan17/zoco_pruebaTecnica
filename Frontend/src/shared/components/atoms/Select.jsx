export default function Select({ className = '', ...props }) {
  return (
    <select
      className={`rounded-md border border-border bg-surface px-3 py-2 text-sm text-text focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary disabled:cursor-not-allowed disabled:bg-gray-50 ${className}`}
      {...props}
    />
  );
}
