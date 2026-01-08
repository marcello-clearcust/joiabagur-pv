/**
 * Image URL Utilities
 * Converts API-relative image URLs to absolute URLs for <img> tags
 */

/**
 * Get the API base URL from environment configuration
 */
function getApiBaseUrl(): string {
  const configuredUrl = import.meta.env.VITE_API_BASE_URL;
  
  if (!configuredUrl) {
    console.warn('VITE_API_BASE_URL not configured, using default');
    return 'http://localhost:5056/api';
  }
  
  return configuredUrl;
}

/**
 * Convert an API-relative URL to an absolute URL for use in img tags.
 * 
 * HTML img tags don't use Axios baseURL, so we need to convert:
 * - Relative: /api/files/products/photo.jpg (from API)
 * - Absolute: http://localhost:5056/api/files/products/photo.jpg (for browser)
 * 
 * In production with cloud storage, URLs are already absolute:
 * - https://bucket.s3.amazonaws.com/products/photo.jpg (used as-is)
 * 
 * @param relativeUrl - URL from API response (e.g., /api/files/products/photo.jpg)
 * @returns Absolute URL for img src, or undefined if input is falsy
 */
export function getImageUrl(relativeUrl: string | undefined | null): string | undefined {
  if (!relativeUrl) {
    return undefined;
  }

  // If already absolute (starts with http:// or https://), use as-is (cloud storage)
  if (relativeUrl.startsWith('http://') || relativeUrl.startsWith('https://')) {
    return relativeUrl;
  }

  // If relative API path, convert to absolute using configured base URL
  if (relativeUrl.startsWith('/api/')) {
    const apiBaseUrl = getApiBaseUrl();
    // Remove /api from the end of base URL if present, then append the full path
    const baseUrlWithoutApi = apiBaseUrl.replace(/\/api\/?$/, '');
    return `${baseUrlWithoutApi}${relativeUrl}`;
  }

  // If starts with /, assume it's relative to API base
  if (relativeUrl.startsWith('/')) {
    const apiBaseUrl = getApiBaseUrl();
    const baseUrlWithoutApi = apiBaseUrl.replace(/\/api\/?$/, '');
    return `${baseUrlWithoutApi}${relativeUrl}`;
  }

  // Otherwise return as-is
  return relativeUrl;
}
