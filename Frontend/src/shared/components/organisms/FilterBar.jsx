export default function FilterBar({ children }) {
  return (
    <div className="flex flex-col gap-3 border-b border-gray-200 pb-4 sm:flex-row sm:flex-wrap sm:items-end">
      {children}
    </div>
  );
}
