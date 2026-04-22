import React, { useState, useEffect } from 'react';
import { FiSearch, FiX } from 'react-icons/fi';
import Input from '../../atoms/Input/Input';
import './SearchBar.scss';

const SearchBar = ({
  placeholder = 'Search...',
  value = '',
  onSearch,
  onClear,
  debounceTime = 300,
  className = ''
}) => {
  const [searchTerm, setSearchTerm] = useState(value);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchTerm !== value) {
        onSearch?.(searchTerm);
      }
    }, debounceTime);

    return () => clearTimeout(timer);
  }, [searchTerm, debounceTime, onSearch, value]);

  const handleClear = () => {
    setSearchTerm('');
    onClear?.();
    onSearch?.('');
  };

  return (
    <div className={`search-bar ${className}`}>
      <Input
        placeholder={placeholder}
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        startAdornment={<FiSearch />}
        endAdornment={searchTerm && (
          <button className="search-bar__clear" onClick={handleClear} type="button">
            <FiX />
          </button>
        )}
      />
    </div>
  );
};

export default SearchBar;