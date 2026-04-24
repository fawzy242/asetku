import React, { useState, useCallback } from "react";
import { FiSearch, FiX, FiFilter } from "react-icons/fi";
import Button from "../../atoms/Button/Button";
import "./SearchToolbar.scss";

const SearchToolbar = ({ onSearch, onFilterToggle, showFilters, placeholder = "Search...", className = "" }) => {
  const [searchInput, setSearchInput] = useState("");

  const handleSubmit = (e) => {
    e.preventDefault();
    onSearch(searchInput);
  };

  const handleClear = () => {
    setSearchInput("");
    onSearch("");
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      onSearch(searchInput);
    }
  };

  return (
    <div className={`search-toolbar ${className}`}>
      <form className="search-toolbar__search" onSubmit={handleSubmit}>
        <FiSearch className="search-toolbar__search-icon" />
        <input
          type="text"
          className="search-toolbar__search-input"
          placeholder={placeholder}
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          onKeyDown={handleKeyDown}
        />
        {searchInput && (
          <button type="button" className="search-toolbar__search-clear" onClick={handleClear}>
            <FiX size={16} />
          </button>
        )}
        <Button type="submit" variant="primary" size="sm">Search</Button>
      </form>
      {onFilterToggle && (
        <Button variant="outline" size="sm" onClick={onFilterToggle} startIcon={<FiFilter />}>
          {showFilters ? "Hide" : "Filter"}
        </Button>
      )}
    </div>
  );
};

export default SearchToolbar;