/**
 * Model Training Service Tests
 * Comprehensive tests for browser-based TensorFlow.js training.
 */
import { describe, it, expect, vi, beforeEach } from 'vitest';
import * as tf from '@tensorflow/tfjs';
import {
  checkTrainingCapabilities,
  executeClientSideTraining,
  modelTrainingService,
} from '../model-training.service';
import { imageRecognitionService } from '../sales.service';
import type { TrainingProgress } from '../model-training.service';

// Mock sales service
vi.mock('../sales.service', () => ({
  imageRecognitionService: {
    getTrainingDataset: vi.fn(),
    uploadTrainedModel: vi.fn(),
  },
}));

describe('Model Training Service', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('checkTrainingCapabilities', () => {
    it('should detect GPU support via WebGL 2.0', async () => {
      // Arrange
      const mockCanvas = {
        getContext: vi.fn((type: string) => {
          if (type === 'webgl2') return {}; // GPU available
          return null;
        }),
      };
      vi.spyOn(document, 'createElement').mockReturnValue(mockCanvas as unknown as HTMLCanvasElement);
      vi.mocked(tf.getBackend).mockReturnValue('webgl');

      // Act
      const capabilities = await checkTrainingCapabilities();

      // Assert
      expect(capabilities.hasGPU).toBe(true);
      expect(capabilities.hasWebGL2).toBe(true);
      expect(capabilities.backend).toBe('webgl');
      expect(capabilities.estimatedDuration).toContain('15-20');
    });

    it('should estimate longer duration without GPU', async () => {
      // Arrange
      const mockCanvas = {
        getContext: vi.fn(() => null), // No WebGL
      };
      vi.spyOn(document, 'createElement').mockReturnValue(mockCanvas as unknown as HTMLCanvasElement);
      vi.mocked(tf.getBackend).mockReturnValue('cpu');

      // Act
      const capabilities = await checkTrainingCapabilities();

      // Assert
      expect(capabilities.hasGPU).toBe(false);
      expect(capabilities.estimatedDuration).toContain('45-60');
    });
  });

  describe('executeClientSideTraining', () => {
    let progressUpdates: TrainingProgress[];
    let mockProgressCallback: (progress: TrainingProgress) => void;

    beforeEach(() => {
      progressUpdates = [];
      mockProgressCallback = (progress: TrainingProgress) => {
        progressUpdates.push(progress);
      };

      // Setup training dataset mock
      // Note: classLabels are now SKUs (immutable, unique) instead of product names
      vi.mocked(imageRecognitionService.getTrainingDataset).mockResolvedValue({
        photos: [
          {
            productId: 'prod-1',
            productSku: 'SKU-001',
            productName: 'Product A',
            photoId: 'photo-1',
            photoUrl: 'http://localhost/photo1.jpg',
          },
          {
            productId: 'prod-2',
            productSku: 'SKU-002',
            productName: 'Product B',
            photoId: 'photo-2',
            photoUrl: 'http://localhost/photo2.jpg',
          },
        ],
        totalPhotos: 2,
        totalProducts: 2,
        classLabels: ['SKU-001', 'SKU-002'], // SKUs instead of product names
      });

      // Mock upload
      vi.mocked(imageRecognitionService.uploadTrainedModel).mockResolvedValue({
        success: true,
        version: 'v1_test',
        metadata: {
          version: 'v1_test',
          trainedAt: new Date().toISOString(),
          modelPath: '/models/v1_test',
          totalPhotosUsed: 2,
          totalProductsUsed: 2,
          isActive: true,
        },
      });
    });

    it('should complete full training workflow', async () => {
      // Act
      await executeClientSideTraining(mockProgressCallback);

      // Assert
      expect(progressUpdates.length).toBeGreaterThan(0);
      
      // Verify all stages were called
      const stages = progressUpdates.map((p) => p.stage);
      expect(stages).toContain('downloading');
      expect(stages).toContain('loading_model');
      expect(stages).toContain('training');
      expect(stages).toContain('uploading');
      expect(stages).toContain('complete');
    });

    it('should report progress from 0 to 100', async () => {
      // Act
      await executeClientSideTraining(mockProgressCallback);

      // Assert
      const progressValues = progressUpdates.map((p) => p.progress);
      expect(Math.min(...progressValues)).toBe(0);
      expect(Math.max(...progressValues)).toBe(100);
    });

    it('should call upload with correct parameters', async () => {
      // Act
      await executeClientSideTraining(mockProgressCallback);

      // Assert
      expect(imageRecognitionService.uploadTrainedModel).toHaveBeenCalledWith(
        expect.stringContaining('v'), // Version string
        expect.any(String), // Model topology JSON
        expect.any(Number), // Training accuracy
        expect.any(Number), // Validation accuracy
        2, // Total photos
        2, // Total products
        expect.any(Number), // Duration
        expect.any(Array) // Weight files
      );
    });

    it('should throw error when no photos available', async () => {
      // Arrange
      vi.mocked(imageRecognitionService.getTrainingDataset).mockResolvedValue({
        photos: [],
        totalPhotos: 0,
        totalProducts: 0,
        classLabels: [],
      });

      // Act & Assert
      await expect(executeClientSideTraining(mockProgressCallback)).rejects.toThrow(
        'No hay fotos disponibles'
      );
    });
  });
});
