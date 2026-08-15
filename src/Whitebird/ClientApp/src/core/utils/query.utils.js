export const buildQueryString = (params) => {
  const query = Object.entries(params)
    .filter(([_, value]) => value !== null && value !== undefined && value !== '')
    .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value)}`)
    .join('&');
  
  return query ? `?${query}` : '';
};

export const parseQueryString = (queryString) => {
  const params = new URLSearchParams(queryString);
  const result = {};
  for (const [key, value] of params) {
    result[key] = value;
  }
  return result;
};

export const getQueryParam = (key, queryString = window.location.search) => {
  const params = new URLSearchParams(queryString);
  return params.get(key);
};

export const hasQueryParam = (key, queryString = window.location.search) => {
  const params = new URLSearchParams(queryString);
  return params.has(key);
};