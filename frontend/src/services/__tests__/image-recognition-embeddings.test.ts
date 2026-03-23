/**
 * Tests for embedding-related functions in image-recognition.service.ts:
 * - loadEmbeddingsIndex (caching + staleness)
 * - clearEmbeddingsCache
 * - recognizeProduct fallback logic (similarity vs classifier)
 */
import { describe, it, expect, vi, beforeEach } from 'vitest';
import {
  loadEmbeddingsIndex,
  clearEmbeddingsCache,
  recognizeProduct,
  cosineSimilarity,
  SIMILARITY_THRESHOLD,
  MIN_TOP_SIMILARITY,
} from '../image-recognition.service';

// Mock the sales service with all embedding methods
vi.mock('../sales.service', () => ({
  imageRecognitionService: {
    getModelMetadata: vi.fn(),
    getTrainingDataset: vi.fn(),
    getModelUrl: vi.fn(),
    getClassLabels: vi.fn(),
    getEmbeddingsStatus: vi.fn(),
    getAllEmbeddings: vi.fn(),
    saveEmbedding: vi.fn(),
    deleteEmbedding: vi.fn(),
    deleteAllEmbeddings: vi.fn(),
  },
}));

// Mock TensorFlow entirely so no model loads happen
vi.mock('@tensorflow/tfjs', () => ({
  ready: vi.fn().mockResolvedValue(undefined),
  getBackend: vi.fn().mockReturnValue('webgl'),
  setBackend: vi.fn().mockResolvedValue(undefined),
  loadGraphModel: vi.fn(),
  loadLayersModel: vi.fn(),
  tidy: vi.fn((fn: () => unknown) => fn()),
  browser: { fromPixels: vi.fn() },
  image: { resizeBilinear: vi.fn() },
  memory: vi.fn().mockReturnValue({ numTensors: 0, numBytes: 0, numBytesInGPU: 0, numDataBuffers: 0 }),
  nextFrame: vi.fn().mockResolvedValue(undefined),
}));

import { imageRecognitionService } from '../sales.service';
import type { EmbeddingsIndexResponse, EmbeddingsStatusResponse } from '@/types/sales.types';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function makeStatusResponse(count: number, lastUpdated: string | null = null): EmbeddingsStatusResponse {
  return { count, lastUpdated };
}

function makeIndexResponse(count: number, lastUpdated: string | null = null): EmbeddingsIndexResponse {
  const embeddings = Array.from({ length: count }, (_, i) => ({
    photoId: `photo-${i}`,
    productId: `product-${i}`,
    sku: `SKU-${i}`,
    vector: Array.from({ length: 1280 }, () => Math.random()),
  }));
  return { count, embeddings, lastUpdated };
}

// ---------------------------------------------------------------------------
// loadEmbeddingsIndex
// ---------------------------------------------------------------------------

describe('loadEmbeddingsIndex', () => {
  beforeEach(() => {
    clearEmbeddingsCache();
    vi.clearAllMocks();
  });

  it('should fetch from API on first call and cache the result', async () => {
    const ts = '2026-01-01T00:00:00Z';
    vi.mocked(imageRecognitionService.getEmbeddingsStatus).mockResolvedValue(makeStatusResponse(2, ts));
    vi.mocked(imageRecognitionService.getAllEmbeddings).mockResolvedValue(makeIndexResponse(2, ts));

    const result = await loadEmbeddingsIndex();

    expect(imageRecognitionService.getAllEmbeddings).toHaveBeenCalledTimes(1);
    expect(result.embeddings).toHaveLength(2);
    expect(result.lastUpdated).toBe(ts);
  });

  it('should return cached result when server timestamp matches', async () => {
    const ts = '2026-01-01T00:00:00Z';
    vi.mocked(imageRecognitionService.getEmbeddingsStatus).mockResolvedValue(makeStatusResponse(2, ts));
    vi.mocked(imageRecognitionService.getAllEmbeddings).mockResolvedValue(makeIndexResponse(2, ts));

    // First call populates cache
    await loadEmbeddingsIndex();
    // Second call — status check returns same timestamp
    await loadEmbeddingsIndex();

    // getAllEmbeddings should only have been called once
    expect(imageRecognitionService.getAllEmbeddings).toHaveBeenCalledTimes(1);
  });

  it('should re-fetch when server has a newer timestamp', async () => {
    const ts1 = '2026-01-01T00:00:00Z';
    const ts2 = '2026-01-02T00:00:00Z';

    // First call: no status check (cache null), goes straight to getAllEmbeddings
    vi.mocked(imageRecognitionService.getAllEmbeddings)
      .mockResolvedValueOnce(makeIndexResponse(2, ts1))
      .mockResolvedValueOnce(makeIndexResponse(3, ts2));

    // Second call: status check compares server timestamp to cached ts1
    // Returning ts2 means it's newer → triggers re-fetch
    vi.mocked(imageRecognitionService.getEmbeddingsStatus)
      .mockResolvedValue(makeStatusResponse(3, ts2));

    await loadEmbeddingsIndex();
    const result2 = await loadEmbeddingsIndex();

    expect(imageRecognitionService.getAllEmbeddings).toHaveBeenCalledTimes(2);
    expect(result2.embeddings).toHaveLength(3);
  });

  it('should return empty index after clearEmbeddingsCache', async () => {
    const ts = '2026-01-01T00:00:00Z';
    vi.mocked(imageRecognitionService.getEmbeddingsStatus).mockResolvedValue(makeStatusResponse(2, ts));
    vi.mocked(imageRecognitionService.getAllEmbeddings).mockResolvedValue(makeIndexResponse(2, ts));

    await loadEmbeddingsIndex();
    clearEmbeddingsCache();

    // After clearing, next call should fetch again
    await loadEmbeddingsIndex();
    expect(imageRecognitionService.getAllEmbeddings).toHaveBeenCalledTimes(2);
  });
});

// ---------------------------------------------------------------------------
// recognizeProduct fallback logic
// The full recognizeProduct function calls into classifier/similarity paths that
// depend on TF model loading (which would hang in a test environment).
// We test the observable gateway behavior: which API calls are (not) made,
// using a very short timeout so tests fail fast rather than hanging for 10s.
// ---------------------------------------------------------------------------

describe('recognizeProduct — fallback logic', () => {
  beforeEach(() => {
    clearEmbeddingsCache();
    vi.clearAllMocks();
  });

  it('should NOT call getAllEmbeddings when embedding count is 0', async () => {
    vi.mocked(imageRecognitionService.getEmbeddingsStatus).mockResolvedValue(makeStatusResponse(0));

    const file = new File([''], 'test.jpg', { type: 'image/jpeg' });
    // Classifier path will hang because TF is mocked; we only care about what
    // API calls were made before the classifier starts.
    // Use a short race so we can assert on completed API calls.
    await Promise.race([
      recognizeProduct(file).catch(() => {}),
      new Promise((r) => setTimeout(r, 200)), // give enough time for status check
    ]);

    // getAllEmbeddings should never be called when count = 0
    expect(imageRecognitionService.getAllEmbeddings).not.toHaveBeenCalled();
  });

  it('should call getEmbeddingsStatus to decide which path to use', async () => {
    vi.mocked(imageRecognitionService.getEmbeddingsStatus).mockResolvedValue(makeStatusResponse(0));

    const file = new File([''], 'test.jpg', { type: 'image/jpeg' });
    await Promise.race([
      recognizeProduct(file).catch(() => {}),
      new Promise((r) => setTimeout(r, 200)),
    ]);

    expect(imageRecognitionService.getEmbeddingsStatus).toHaveBeenCalledTimes(1);
  });

  it('should NOT call getAllEmbeddings when status check fails', async () => {
    vi.mocked(imageRecognitionService.getEmbeddingsStatus).mockRejectedValue(new Error('network error'));

    const file = new File([''], 'test.jpg', { type: 'image/jpeg' });
    await Promise.race([
      recognizeProduct(file).catch(() => {}),
      new Promise((r) => setTimeout(r, 200)),
    ]);

    expect(imageRecognitionService.getAllEmbeddings).not.toHaveBeenCalled();
  });
});

// ---------------------------------------------------------------------------
// SIMILARITY_THRESHOLD and MIN_TOP_SIMILARITY constants
// ---------------------------------------------------------------------------

describe('Similarity threshold constants', () => {
  it('SIMILARITY_THRESHOLD should be 0.70', () => {
    expect(SIMILARITY_THRESHOLD).toBe(0.70);
  });

  it('MIN_TOP_SIMILARITY should be 0.50', () => {
    expect(MIN_TOP_SIMILARITY).toBe(0.50);
  });
});
