export default function EmptyState({ title, description, children }) {
  return (
    <div className="flex flex-col items-center justify-center rounded-lg border border-dashed border-border bg-gray-50 px-6 py-14 text-center">
      <p className="text-sm font-medium text-gray-700">{title}</p>
      {description && <p className="mt-1 text-sm text-gray-500">{description}</p>}
      {children && <div className="mt-4">{children}</div>}
    </div>
  );
}
