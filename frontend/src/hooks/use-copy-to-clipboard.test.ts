/**
 * Tests for useCopyToClipboard hook
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useCopyToClipboard } from './use-copy-to-clipboard';

describe('useCopyToClipboard', () => {
  const mockWriteText = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    
    // Mock clipboard API
    Object.assign(navigator, {
      clipboard: {
        writeText: mockWriteText.mockResolvedValue(undefined),
      },
    });
  });

  it('should return isCopied as false initially', () => {
    const { result } = renderHook(() => useCopyToClipboard());
    
    expect(result.current.isCopied).toBe(false);
  });

  it('should copy text to clipboard', async () => {
    const { result } = renderHook(() => useCopyToClipboard());

    await act(async () => {
      result.current.copyToClipboard('test text');
    });

    expect(mockWriteText).toHaveBeenCalledWith('test text');
    expect(result.current.isCopied).toBe(true);
  });

  it('should reset isCopied after timeout', async () => {
    vi.useFakeTimers();
    
    const { result } = renderHook(() => useCopyToClipboard({ timeout: 1000 }));

    await act(async () => {
      result.current.copyToClipboard('test text');
    });

    expect(result.current.isCopied).toBe(true);

    await act(async () => {
      vi.advanceTimersByTime(1000);
    });

    expect(result.current.isCopied).toBe(false);
    
    vi.useRealTimers();
  });

  it('should call onCopy callback when text is copied', async () => {
    const onCopy = vi.fn();
    const { result } = renderHook(() => useCopyToClipboard({ onCopy }));

    await act(async () => {
      result.current.copyToClipboard('test text');
    });

    expect(onCopy).toHaveBeenCalled();
  });

  it('should not copy empty value', async () => {
    const { result } = renderHook(() => useCopyToClipboard());

    await act(async () => {
      result.current.copyToClipboard('');
    });

    expect(mockWriteText).not.toHaveBeenCalled();
  });
});
