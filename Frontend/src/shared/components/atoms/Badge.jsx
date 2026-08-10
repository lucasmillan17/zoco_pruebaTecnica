const VARIANTES = {
  gray: { solid: 'bg-gray-500 text-white', soft: 'bg-gray-100 text-gray-700' },
  green: { solid: 'bg-success text-white', soft: 'bg-success/10 text-success' },
  amber: { solid: 'bg-warning text-white', soft: 'bg-warning/10 text-warning' },
  red: { solid: 'bg-danger text-white', soft: 'bg-danger/10 text-danger' },
  blue: { solid: 'bg-blue-500 text-white', soft: 'bg-blue-50 text-blue-700' },
  indigo: { solid: 'bg-indigo-500 text-white', soft: 'bg-indigo-50 text-indigo-700' },
  cyan: { solid: 'bg-cyan-500 text-white', soft: 'bg-cyan-50 text-cyan-700' },
};

export default function Badge({ variant = 'gray', tone = 'solid', className = '', children }) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold uppercase tracking-wide whitespace-nowrap ${VARIANTES[variant]?.[tone] ?? VARIANTES.gray[tone]} ${className}`}
    >
      {children}
    </span>
  );
}
