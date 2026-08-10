const VARIANTES = {
  primary: 'bg-primary text-white border-transparent hover:bg-primary-hover focus-visible:ring-primary',
  secondary: 'bg-surface text-text border-border hover:bg-gray-200 focus-visible:ring-gray-400',
  danger: 'bg-white text-danger border-danger/40 hover:bg-danger/5 focus-visible:ring-danger',
  ghost: 'bg-transparent text-muted border-transparent hover:bg-gray-200 focus-visible:ring-gray-400',
};

const TAMANIOS = {
  sm: 'px-2.5 py-1.5 text-xs',
  md: 'px-3.5 py-2 text-sm',
};

export default function Button({ variant = 'secondary', size = 'md', className = '', ...props }) {
  return (
    <button
      type="button"
      className={`inline-flex items-center justify-center gap-1.5 rounded-md border font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-1 disabled:cursor-not-allowed disabled:opacity-50 ${VARIANTES[variant]} ${TAMANIOS[size]} ${className}`}
      {...props}
    />
  );
}
