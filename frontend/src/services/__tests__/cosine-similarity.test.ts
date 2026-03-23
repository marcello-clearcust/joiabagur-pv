/**
 * Tests for the cosineSimilarity function.
 */
import { describe, it, expect } from 'vitest';
import { cosineSimilarity } from '../image-recognition.service';

describe('cosineSimilarity', () => {
  it('should return 1.0 for identical vectors', () => {
    const a = new Float32Array([1, 2, 3, 4, 5]);
    const b = new Float32Array([1, 2, 3, 4, 5]);
    expect(cosineSimilarity(a, b)).toBeCloseTo(1.0, 5);
  });

  it('should return 0.0 for orthogonal vectors', () => {
    const a = new Float32Array([1, 0, 0]);
    const b = new Float32Array([0, 1, 0]);
    expect(cosineSimilarity(a, b)).toBeCloseTo(0.0, 5);
  });

  it('should return -1.0 for opposite vectors', () => {
    const a = new Float32Array([1, 2, 3]);
    const b = new Float32Array([-1, -2, -3]);
    expect(cosineSimilarity(a, b)).toBeCloseTo(-1.0, 5);
  });

  it('should return a value in [-1, 1] for arbitrary 1280-dim vectors', () => {
    const size = 1280;
    const a = new Float32Array(size).map(() => Math.random() * 2 - 1);
    const b = new Float32Array(size).map(() => Math.random() * 2 - 1);
    const sim = cosineSimilarity(a, b);
    expect(sim).toBeGreaterThanOrEqual(-1);
    expect(sim).toBeLessThanOrEqual(1);
  });

  it('should return 0 for zero vectors', () => {
    const a = new Float32Array([0, 0, 0]);
    const b = new Float32Array([1, 2, 3]);
    expect(cosineSimilarity(a, b)).toBe(0);
  });

  it('should compute known pre-computed value correctly', () => {
    // [1,0] · [1,1]/sqrt(2) = 1/sqrt(2) ≈ 0.7071
    const a = new Float32Array([1, 0]);
    const b = new Float32Array([1, 1]);
    expect(cosineSimilarity(a, b)).toBeCloseTo(1 / Math.sqrt(2), 4);
  });
});
