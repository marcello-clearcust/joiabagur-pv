/**
 * Tests for image URL utilities
 */
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { getImageUrl } from './image-url';

describe('getImageUrl', () => {
  beforeEach(() => {
    // Mock environment variable
    vi.stubEnv('VITE_API_BASE_URL', 'http://localhost:5056/api');
  });

  it('should return undefined for null or undefined input', () => {
    expect(getImageUrl(null)).toBeUndefined();
    expect(getImageUrl(undefined)).toBeUndefined();
  });

  it('should return undefined for empty string', () => {
    expect(getImageUrl('')).toBeUndefined();
  });

  it('should return absolute HTTP URLs as-is', () => {
    const url = 'http://example.com/photo.jpg';
    expect(getImageUrl(url)).toBe(url);
  });

  it('should return absolute HTTPS URLs as-is (cloud storage)', () => {
    const url = 'https://bucket.s3.amazonaws.com/products/photo.jpg';
    expect(getImageUrl(url)).toBe(url);
  });

  it('should convert relative API URLs to absolute using env config', () => {
    const relativeUrl = '/api/files/products/photo.jpg';
    const result = getImageUrl(relativeUrl);
    expect(result).toBe('http://localhost:5056/api/files/products/photo.jpg');
  });

  it('should handle API base URL with trailing slash', () => {
    vi.stubEnv('VITE_API_BASE_URL', 'http://localhost:5056/api/');
    const relativeUrl = '/api/files/products/photo.jpg';
    const result = getImageUrl(relativeUrl);
    expect(result).toBe('http://localhost:5056/api/files/products/photo.jpg');
  });

  it('should handle API base URL without /api suffix', () => {
    vi.stubEnv('VITE_API_BASE_URL', 'http://localhost:5056');
    const relativeUrl = '/api/files/products/photo.jpg';
    const result = getImageUrl(relativeUrl);
    expect(result).toBe('http://localhost:5056/api/files/products/photo.jpg');
  });

  it('should handle different ports from env config', () => {
    vi.stubEnv('VITE_API_BASE_URL', 'http://localhost:8080/api');
    const relativeUrl = '/api/files/products/photo.jpg';
    const result = getImageUrl(relativeUrl);
    expect(result).toBe('http://localhost:8080/api/files/products/photo.jpg');
  });

  it('should handle production API URLs', () => {
    vi.stubEnv('VITE_API_BASE_URL', 'https://api.joiabagur.com/api');
    const relativeUrl = '/api/files/products/photo.jpg';
    const result = getImageUrl(relativeUrl);
    expect(result).toBe('https://api.joiabagur.com/api/files/products/photo.jpg');
  });

  it('should handle root-relative URLs', () => {
    const relativeUrl = '/files/photo.jpg';
    const result = getImageUrl(relativeUrl);
    expect(result).toBe('http://localhost:5056/files/photo.jpg');
  });
});
