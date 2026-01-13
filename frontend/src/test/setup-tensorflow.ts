/**
 * Test Setup for TensorFlow.js Mocking
 * Automatically mocks TensorFlow.js for all tests.
 */
import { beforeAll, vi } from 'vitest';
import { createTensorFlowMock } from './tensorflow-mock';

// Mock TensorFlow.js module
beforeAll(() => {
  vi.mock('@tensorflow/tfjs', () => createTensorFlowMock());
});
