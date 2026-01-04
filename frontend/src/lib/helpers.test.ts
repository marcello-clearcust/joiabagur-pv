/**
 * Tests for helper functions
 */

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { 
  uid, 
  getInitials, 
  timeAgo, 
  formatDate, 
  formatDateTime,
  debounce,
  throttle,
} from './helpers';

describe('uid', () => {
  it('should generate a unique string', () => {
    const id1 = uid();
    const id2 = uid();
    expect(id1).not.toBe(id2);
  });

  it('should return a string', () => {
    expect(typeof uid()).toBe('string');
  });
});

describe('getInitials', () => {
  it('should return initials for a full name', () => {
    expect(getInitials('John Doe')).toBe('JD');
  });

  it('should return initials for a single name', () => {
    expect(getInitials('John')).toBe('J');
  });

  it('should handle multiple names', () => {
    expect(getInitials('John Michael Doe')).toBe('JMD');
  });

  it('should limit initials when count is provided', () => {
    expect(getInitials('John Michael Doe', 2)).toBe('JM');
  });

  it('should return empty string for null input', () => {
    expect(getInitials(null)).toBe('');
  });

  it('should return empty string for undefined input', () => {
    expect(getInitials(undefined)).toBe('');
  });

  it('should return empty string for empty string input', () => {
    expect(getInitials('')).toBe('');
  });

  it('should handle extra spaces', () => {
    expect(getInitials('  John   Doe  ')).toBe('JD');
  });
});

describe('timeAgo', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2025-12-14T12:00:00Z'));
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should return "just now" for recent dates', () => {
    const date = new Date('2025-12-14T11:59:30Z');
    expect(timeAgo(date)).toBe('just now');
  });

  it('should return minutes ago', () => {
    const date = new Date('2025-12-14T11:55:00Z');
    expect(timeAgo(date)).toBe('5 minutes ago');
  });

  it('should return "1 minute ago" for singular', () => {
    const date = new Date('2025-12-14T11:59:00Z');
    expect(timeAgo(date)).toBe('1 minute ago');
  });

  it('should return hours ago', () => {
    const date = new Date('2025-12-14T09:00:00Z');
    expect(timeAgo(date)).toBe('3 hours ago');
  });

  it('should return days ago', () => {
    const date = new Date('2025-12-12T12:00:00Z');
    expect(timeAgo(date)).toBe('2 days ago');
  });

  it('should accept string dates', () => {
    const date = '2025-12-14T11:55:00Z';
    expect(timeAgo(date)).toBe('5 minutes ago');
  });
});

describe('formatDate', () => {
  it('should format date correctly', () => {
    const date = new Date('2025-12-14T12:00:00Z');
    const result = formatDate(date);
    expect(result).toContain('December');
    expect(result).toContain('14');
    expect(result).toContain('2025');
  });

  it('should accept string input', () => {
    const result = formatDate('2025-12-14');
    expect(result).toContain('December');
  });

  it('should accept number (timestamp) input', () => {
    const timestamp = new Date('2025-12-14').getTime();
    const result = formatDate(timestamp);
    expect(result).toContain('December');
  });
});

describe('formatDateTime', () => {
  it('should format date and time correctly', () => {
    const date = new Date('2025-12-14T14:30:00');
    const result = formatDateTime(date);
    expect(result).toContain('December');
    expect(result).toContain('14');
    expect(result).toContain('2025');
    // Time should be included
    expect(result).toMatch(/\d{1,2}:\d{2}/);
  });
});

describe('debounce', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should debounce function calls', () => {
    const fn = vi.fn();
    const debouncedFn = debounce(fn, 100);

    debouncedFn();
    debouncedFn();
    debouncedFn();

    expect(fn).not.toHaveBeenCalled();

    vi.advanceTimersByTime(100);

    expect(fn).toHaveBeenCalledTimes(1);
  });

  it('should pass arguments to the debounced function', () => {
    const fn = vi.fn();
    const debouncedFn = debounce(fn, 100);

    debouncedFn('arg1', 'arg2');

    vi.advanceTimersByTime(100);

    expect(fn).toHaveBeenCalledWith('arg1', 'arg2');
  });
});

describe('throttle', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should call function immediately on first call', () => {
    const fn = vi.fn();
    const throttledFn = throttle(fn, 100);

    throttledFn();

    expect(fn).toHaveBeenCalledTimes(1);
  });

  it('should throttle subsequent calls', () => {
    const fn = vi.fn();
    const throttledFn = throttle(fn, 100);

    throttledFn();
    throttledFn();
    throttledFn();

    expect(fn).toHaveBeenCalledTimes(1);

    vi.advanceTimersByTime(100);

    // After throttle period, the last call should execute
    expect(fn).toHaveBeenCalledTimes(2);
  });
});
