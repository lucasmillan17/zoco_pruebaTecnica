import { forwardRef } from 'react';

const Input = forwardRef(function Input({ className = '', invalid = false, ...props }, ref) {
  return (
    <input
      ref={ref}
      className={`w-full rounded-md border border-border bg-surface px-3 py-2 text-sm text-text placeholder:text-muted focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary disabled:cursor-not-allowed disabled:bg-gray-50 ${
        invalid ? 'border-danger' : 'border-border'
      } ${className}`}
      {...props}
    />
  );
});

export default Input;
