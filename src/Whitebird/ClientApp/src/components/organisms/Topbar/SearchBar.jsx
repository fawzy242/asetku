import React, { useState, useRef, useEffect } from 'react';
import { FiSearch, FiX } from 'react-icons/fi';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { HomeData } from '../../../core/services';
import { useDebounce } from '../../../hooks/useDebounce';

const homeData = new HomeData();

const fetchSearchResults = async (keyword) => {
  if (!keyword || keyword.trim().length < 2) return [];
  try {
    const result = await homeData.search(keyword);
    if (result.success && result.data) {
      return result.data.results || [];
    }
    return [];
  } catch (error) {
    console.error("Search failed:", error);
    return [];
  }
};

const SearchBar = () => {
  const [searchValue, setSearchValue] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [showSearchResults, setShowSearchResults] = useState(false);
  const searchRef = useRef(null);
  const navigate = useNavigate();

  const { data: searchResults = [], isLoading: isSearching, refetch } = useQuery({
    queryKey: ['global-search', debouncedSearch],
    queryFn: () => fetchSearchResults(debouncedSearch),
    enabled: debouncedSearch.length >= 2,
    staleTime: 30000,
    placeholderData: (prev) => prev,
  });

  const debouncedSetSearch = useDebounce((value) => {
    setDebouncedSearch(value);
    if (value.length >= 2) {
      refetch();
    }
  }, 300);

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (searchRef.current && !searchRef.current.contains(e.target)) {
        setShowSearchResults(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleSearchInput = (e) => {
    const v = e.target.value;
    setSearchValue(v);
    setShowSearchResults(true);
    debouncedSetSearch(v);
  };

  const handleClearSearch = () => {
    setSearchValue("");
    setDebouncedSearch("");
    setShowSearchResults(false);
  };

  const handleSearchSelect = (r) => {
    setSearchValue("");
    setDebouncedSearch("");
    setShowSearchResults(false);
    if (r.type === "asset") {
      navigate(`/tracking?assetId=${r.id}`);
    } else if (r.type === "employee") {
      navigate(`/employee-summary?employeeId=${r.id}`);
    }
  };

  return (
    <div className="topbar__search" ref={searchRef}>
      <FiSearch className="topbar__search-icon" aria-hidden="true" />
      <input
        type="text"
        className="topbar__search-input"
        placeholder="Search assets, employees..."
        value={searchValue}
        onChange={handleSearchInput}
        onFocus={() => setShowSearchResults(true)}
        aria-label="Search assets and employees"
      />
      {searchValue && (
        <button className="topbar__search-clear" onClick={handleClearSearch} aria-label="Clear search">
          <FiX size={16} />
        </button>
      )}
      {showSearchResults && (searchResults.length > 0 || isSearching || searchValue.length >= 2) && (
        <div className="topbar__search-results">
          {isSearching ? (
            <div className="topbar__search-loading"><div className="global-spinner" /></div>
          ) : searchResults.length > 0 ? (
            searchResults.map((r, i) => (
              <div key={`${r.type}-${r.id}-${i}`} className="topbar__search-result" onClick={() => handleSearchSelect(r)}>
                <span className="topbar__search-result-type">{r.type}</span>
                <span className="topbar__search-result-title">{r.title}</span>
                <span className="topbar__search-result-code">{r.code}</span>
              </div>
            ))
          ) : searchValue.length >= 2 && (
            <div className="topbar__search-empty">No results found for "{searchValue}"</div>
          )}
        </div>
      )}
    </div>
  );
};

export default SearchBar;