import { forwardRef } from 'react';
import { Search } from 'lucide-react';

const SearchInput = forwardRef(function SearchInput({ className = '', ...props }, ref) {
  return (
    <div className={`relative ${className}`}>
      <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" />
      <input
        ref={ref}
        type="search"
        className="w-full rounded-md border border-border bg-surface py-2 pl-9 pr-3 text-sm text-text placeholder:text-muted focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary"
        {...props}
      />
    </div>
  );
});

export default SearchInput;
