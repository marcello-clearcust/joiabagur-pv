/**
 * Tests for utility functions
 */

import { describe, it, expect } from 'vitest';
import { cn } from './utils';

describe('cn', () => {
  it('should merge class names correctly', () => {
    const result = cn('foo', 'bar');
    expect(result).toBe('foo bar');
  });

  it('should handle conditional classes', () => {
    const isActive = true;
    const result = cn('base', isActive && 'active');
    expect(result).toBe('base active');
  });

  it('should filter falsy values', () => {
    const result = cn('foo', false, null, undefined, 'bar');
    expect(result).toBe('foo bar');
  });

  it('should handle Tailwind class conflicts correctly', () => {
    // twMerge should resolve conflicts - later classes win
    const result = cn('p-4', 'p-2');
    expect(result).toBe('p-2');
  });

  it('should merge Tailwind variants correctly', () => {
    const result = cn('text-red-500', 'hover:text-blue-500');
    expect(result).toContain('text-red-500');
    expect(result).toContain('hover:text-blue-500');
  });

  it('should handle array of classes', () => {
    const result = cn(['foo', 'bar']);
    expect(result).toBe('foo bar');
  });

  it('should handle object syntax', () => {
    const result = cn({ 'active': true, 'disabled': false });
    expect(result).toBe('active');
  });

  it('should return empty string for no classes', () => {
    const result = cn();
    expect(result).toBe('');
  });
});
