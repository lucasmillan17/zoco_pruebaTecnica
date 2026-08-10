import Select from '../atoms/Select';

export default function FilterSelect({ label, options = [], className = '', ...props }) {
  return (
    <div className={className}>
      {label && (
        <span className="mb-1 block text-xs font-medium text-gray-500">{label}</span>
      )}
      <Select className="w-full" {...props}>
        {options.map((opcion) => (
          <option key={opcion.value} value={opcion.value}>
            {opcion.label}
          </option>
        ))}
      </Select>
    </div>
  );
}
