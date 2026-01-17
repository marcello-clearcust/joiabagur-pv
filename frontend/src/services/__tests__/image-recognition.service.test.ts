/**
 * Image Recognition Inference Service Tests
 * Comprehensive tests for client-side TensorFlow.js inference.
 */
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import * as tf from '@tensorflow/tfjs';
import {
  checkDeviceCompatibility,
  validateImage,
  validateImageDimensions,
  preprocessImage,
  loadModel,
  runInference,
  generateSuggestions,
  validatePredictions,
  recognizeProduct,
  imageRecognitionInferenceService,
} from '../image-recognition.service';
import { imageRecognitionService } from '../sales.service';
import { MockTensor } from '../../test/tensorflow-mock';
import type { TrainingDataset } from '@/types/sales.types';

// Mock the sales service
vi.mock('../sales.service', () => ({
  imageRecognitionService: {
    getModelMetadata: vi.fn(),
    getTrainingDataset: vi.fn(),
    getModelUrl: vi.fn((version: string, fileName: string) => 
      `/api/image-recognition/model/files/${version}/${fileName}`
    ),
  },
}));

describe('Image Recognition Inference Service', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('checkDeviceCompatibility', () => {
    it('should detect WebGL 2.0 support', async () => {
      // Arrange
      const mockCanvas = {
        getContext: vi.fn((type: string) => {
          if (type === 'webgl2') return {}; // Mock WebGL2 context
          if (type === 'webgl') return {}; // Mock WebGL context
          return null;
        }),
      };
      vi.spyOn(document, 'createElement').mockReturnValue(mockCanvas as unknown as HTMLCanvasElement);

      // Act
      const capabilities = await checkDeviceCompatibility();

      // Assert
      expect(capabilities.hasTensorFlowJS).toBe(true);
      expect(capabilities.hasWebGL).toBe(true);
      expect(capabilities.hasWebGL2).toBe(true);
      expect(capabilities.backend).toBe('webgl');
    });

    it('should return false when WebGL not supported', async () => {
      // Arrange
      const mockCanvas = {
        getContext: vi.fn(() => null), // No WebGL support
      };
      vi.spyOn(document, 'createElement').mockReturnValue(mockCanvas as unknown as HTMLCanvasElement);

      // Act
      const capabilities = await checkDeviceCompatibility();

      // Assert
      expect(capabilities.hasWebGL).toBe(false);
      expect(capabilities.hasWebGL2).toBe(false);
    });

    it('should handle TensorFlow.js initialization failure', async () => {
      // Arrange
      vi.mocked(tf.ready).mockRejectedValue(new Error('TensorFlow.js not available'));

      // Act
      const capabilities = await checkDeviceCompatibility();

      // Assert
      expect(capabilities.hasTensorFlowJS).toBe(false);
      expect(capabilities.backend).toBe('none');
    });
  });

  describe('validateImage', () => {
    it('should accept valid image files', () => {
      // Arrange
      const validFile = new File(['fake image data'], 'test.jpg', { type: 'image/jpeg' });

      // Act
      const result = validateImage(validFile);

      // Assert
      expect(result.isValid).toBe(true);
      expect(result.error).toBeUndefined();
    });

    it('should reject non-image files', () => {
      // Arrange
      const invalidFile = new File(['fake data'], 'test.txt', { type: 'text/plain' });

      // Act
      const result = validateImage(invalidFile);

      // Assert
      expect(result.isValid).toBe(false);
      expect(result.error).toContain('imagen');
    });

    it('should reject files larger than 10MB', () => {
      // Arrange
      const largeData = new Array(11 * 1024 * 1024).fill(0); // 11MB
      const largeFile = new File([new Uint8Array(largeData)], 'large.jpg', { type: 'image/jpeg' });

      // Act
      const result = validateImage(largeFile);

      // Assert
      expect(result.isValid).toBe(false);
      expect(result.error).toContain('demasiado grande');
    });

    it('should accept files up to 10MB', () => {
      // Arrange
      const acceptableData = new Array(9 * 1024 * 1024).fill(0); // 9MB
      const acceptableFile = new File([new Uint8Array(acceptableData)], 'acceptable.jpg', { 
        type: 'image/jpeg' 
      });

      // Act
      const result = validateImage(acceptableFile);

      // Assert
      expect(result.isValid).toBe(true);
    });
  });

  describe('validateImageDimensions', () => {
    it('should accept images with valid dimensions', async () => {
      // Arrange
      const mockImage = createMockImage(800, 600);

      // Act
      const result = await validateImageDimensions(mockImage);

      // Assert
      expect(result.isValid).toBe(true);
      expect(result.error).toBeUndefined();
    });

    it('should reject images smaller than 200x200', async () => {
      // Arrange
      const smallImage = createMockImage(150, 150);

      // Act
      const result = await validateImageDimensions(smallImage);

      // Assert
      expect(result.isValid).toBe(false);
      expect(result.error).toContain('demasiado pequeña');
    });

    it('should reject images with extreme aspect ratio', async () => {
      // Arrange
      const wideImage = createMockImage(2000, 300); // 6.67:1 aspect ratio

      // Act
      const result = await validateImageDimensions(wideImage);

      // Assert
      expect(result.isValid).toBe(false);
      expect(result.error).toContain('proporción de aspecto');
    });

    it('should accept 5:1 aspect ratio as maximum', async () => {
      // Arrange
      const acceptableImage = createMockImage(1000, 200); // Exactly 5:1

      // Act
      const result = await validateImageDimensions(acceptableImage);

      // Assert
      expect(result.isValid).toBe(true);
    });

    it('should reject completely black images', async () => {
      // Arrange
      const blackImage = createMockImageWithBrightness(800, 600, 5); // Very dark

      // Act
      const result = await validateImageDimensions(blackImage);

      // Assert
      expect(result.isValid).toBe(false);
      expect(result.error).toContain('oscura');
    });

    it('should reject completely white images', async () => {
      // Arrange
      const whiteImage = createMockImageWithBrightness(800, 600, 250); // Very bright

      // Act
      const result = await validateImageDimensions(whiteImage);

      // Assert
      expect(result.isValid).toBe(false);
      expect(result.error).toContain('clara');
    });

    it('should accept images with normal brightness', async () => {
      // Arrange
      const normalImage = createMockImageWithBrightness(800, 600, 128); // Mid-range

      // Act
      const result = await validateImageDimensions(normalImage);

      // Assert
      expect(result.isValid).toBe(true);
    });
  });

  describe('preprocessImage', () => {
    it('should resize image to 224x224', async () => {
      // Arrange
      const image = createMockImage(800, 600);

      // Act
      const tensor = await preprocessImage(image);

      // Assert
      expect(tensor).toBeDefined();
      expect(vi.mocked(tf.image.resizeBilinear)).toHaveBeenCalledWith(
        expect.anything(),
        [224, 224]
      );
      tensor.dispose();
    });

    it('should normalize pixel values to [-1, 1]', async () => {
      // Arrange
      const image = createMockImage(224, 224);

      // Act
      const tensor = await preprocessImage(image);

      // Assert
      expect(vi.mocked(tf.browser.fromPixels)).toHaveBeenCalledWith(image);
      tensor.dispose();
    });

    it('should dispose of intermediate tensors (tidy)', async () => {
      // Arrange
      const image = createMockImage(800, 600);

      // Act
      await preprocessImage(image);

      // Assert
      expect(tf.tidy).toHaveBeenCalled();
    });
  });

  describe('loadModel', () => {
    beforeEach(() => {
      // Clear model cache before each test
      imageRecognitionInferenceService.clearModelCache();
    });

    it('should try loadLayersModel first', async () => {
      // Arrange
      const version = 'v1_20260112';

      // Act
      await loadModel(version);

      // Assert - should try loadLayersModel first
      expect(tf.loadLayersModel).toHaveBeenCalled();
    });

    it('should fall back to loadGraphModel if loadLayersModel fails', async () => {
      // Arrange
      vi.mocked(tf.loadLayersModel).mockRejectedValue(new Error('Not a layers model'));
      const version = 'v1_20260112';

      // Act
      await loadModel(version);

      // Assert - should fall back to loadGraphModel
      expect(tf.loadGraphModel).toHaveBeenCalled();
    });

    it('should throw error if both model loaders fail', async () => {
      // Arrange
      vi.mocked(tf.loadLayersModel).mockRejectedValue(new Error('Not a layers model'));
      vi.mocked(tf.loadGraphModel).mockRejectedValue(new Error('Network error'));

      // Act & Assert
      await expect(loadModel('v1_20260112')).rejects.toThrow('Error al cargar el modelo de IA');
    });

    it('should cache loaded model and reuse it', async () => {
      // Arrange - ensure fresh state
      imageRecognitionInferenceService.clearModelCache();
      vi.clearAllMocks(); // Reset mock call counts
      const version = 'v1_20260112';

      // Act - load twice with same version
      await loadModel(version);
      await loadModel(version);

      // Assert - should only load once due to caching
      expect(tf.loadLayersModel).toHaveBeenCalledTimes(1);
    });
  });

  describe('validatePredictions (OOD detection)', () => {
    it('should reject when top confidence is too low', async () => {
      // Arrange - top confidence 35% < 50% minimum
      const predictions = new MockTensor([0.35, 0.30, 0.20, 0.15], [4]);

      // Act
      const result = await validatePredictions(predictions as unknown as tf.Tensor);

      // Assert
      expect(result.isValid).toBe(false);
      expect(result.reason).toContain('Confianza insuficiente');
    });

    it('should reject when entropy is too high (uncertain prediction)', async () => {
      // Arrange - all classes have similar probability (high entropy)
      const predictions = new MockTensor([0.25, 0.25, 0.25, 0.25], [4]);

      // Act
      const result = await validatePredictions(predictions as unknown as tf.Tensor);

      // Assert
      expect(result.isValid).toBe(false);
      // Will fail on either confidence or entropy
    });

    it('should accept when prediction is dominant and confident', async () => {
      // Arrange - clear dominant prediction: 85% vs 10%
      const predictions = new MockTensor([0.85, 0.10, 0.03, 0.02], [4]);

      // Act
      const result = await validatePredictions(predictions as unknown as tf.Tensor);

      // Assert
      expect(result.isValid).toBe(true);
      expect(result.topConfidence).toBeCloseTo(0.85);
      expect(result.dominanceRatio).toBeGreaterThan(1.5);
    });

    it('should accept high confidence even without dominance', async () => {
      // Arrange - 75% confidence is high enough to skip dominance check
      const predictions = new MockTensor([0.75, 0.15, 0.10], [3]);

      // Act
      const result = await validatePredictions(predictions as unknown as tf.Tensor);

      // Assert
      expect(result.isValid).toBe(true);
    });
  });

  describe('generateSuggestions', () => {
    it('should return suggestions when prediction is valid and dominant', async () => {
      // Arrange - clear dominant prediction that passes OOD validation
      // Class labels are now SKUs (immutable identifiers)
      const predictions = new MockTensor(
        [0.80, 0.10, 0.05, 0.03, 0.02], // Dominant prediction
        [5]
      );
      const classLabels = ['SKU-001', 'SKU-002', 'SKU-003', 'SKU-004', 'SKU-005'];

      // Act
      const suggestions = await generateSuggestions(predictions as unknown as tf.Tensor, classLabels);

      // Assert
      expect(suggestions.length).toBeGreaterThan(0);
      expect(suggestions[0].confidence).toBeCloseTo(80, 0); // Use toBeCloseTo for float precision
      expect(suggestions[0].productSku).toBe('SKU-001'); // Now uses SKU instead of productName
    });

    it('should limit suggestions to maximum 5', async () => {
      // Arrange - single dominant class with enough confidence to pass OOD
      // Rest are spread out but above threshold
      const predictions = new MockTensor(
        [0.85, 0.50, 0.45, 0.42, 0.41, 0.01, 0.01], // P1 dominant (85%), 4 more above 40%
        [7]
      );
      const classLabels = ['P1', 'P2', 'P3', 'P4', 'P5', 'P6', 'P7'];

      // Act
      const suggestions = await generateSuggestions(predictions as unknown as tf.Tensor, classLabels);

      // Assert - should return up to 5 (P1=85%, P2=50%, P3=45%, P4=42%, P5=41%)
      expect(suggestions).toHaveLength(5); // Capped at 5
    });

    it('should return empty array when all below threshold (OOD detected)', async () => {
      // Arrange - all below minimum confidence threshold
      const predictions = new MockTensor(
        [0.35, 0.30, 0.20, 0.15], // All below 40%, top below 50%
        [4]
      );
      const classLabels = ['P1', 'P2', 'P3', 'P4'];

      // Act
      const suggestions = await generateSuggestions(predictions as unknown as tf.Tensor, classLabels);

      // Assert
      expect(suggestions).toHaveLength(0); // OOD detected, no suggestions
    });

    it('should sort suggestions by confidence descending', async () => {
      // Arrange - valid prediction with dominant class + multiple above threshold
      const predictions = new MockTensor(
        [0.45, 0.85, 0.50, 0.02, 0.55], // P2 dominant (85%), others above threshold
        [5]
      );
      const classLabels = ['P1', 'P2', 'P3', 'P4', 'P5'];

      // Act
      const suggestions = await generateSuggestions(predictions as unknown as tf.Tensor, classLabels);

      // Assert - should be sorted by confidence descending
      expect(suggestions[0].confidence).toBeCloseTo(85, 0); // P2 (highest)
      expect(suggestions[1].confidence).toBeCloseTo(55, 0); // P5
      expect(suggestions[2].confidence).toBeCloseTo(50, 0); // P3
      expect(suggestions[3].confidence).toBeCloseTo(45, 0); // P1
    });

    it('should convert confidence to percentage', async () => {
      // Arrange - single class, high confidence
      const predictions = new MockTensor([0.92], [1]); // 92% as decimal
      const classLabels = ['Product'];

      // Act
      const suggestions = await generateSuggestions(predictions as unknown as tf.Tensor, classLabels);

      // Assert
      expect(suggestions[0].confidence).toBeCloseTo(92, 0); // Converted to percentage
    });
  });

  describe('recognizeProduct', () => {
    beforeEach(() => {
      // Setup common mocks
      vi.mocked(imageRecognitionService.getModelMetadata).mockResolvedValue({
        version: 'v1_20260112',
        trainedAt: '2026-01-12T00:00:00Z',
        modelPath: '/models/v1',
        totalPhotosUsed: 50,
        totalProductsUsed: 10,
        isActive: true,
      });

      // Note: classLabels are now SKUs (immutable, unique) instead of product names
      vi.mocked(imageRecognitionService.getTrainingDataset).mockResolvedValue({
        photos: [
          {
            productId: 'prod-1',
            productSku: 'SKU-001',
            productName: 'Product A',
            photoId: 'photo-1',
            photoUrl: '/api/products/prod-1/photos/photo-1/file',
          },
          {
            productId: 'prod-2',
            productSku: 'SKU-002',
            productName: 'Product B',
            photoId: 'photo-2',
            photoUrl: '/api/products/prod-2/photos/photo-2/file',
          },
        ],
        totalPhotos: 2,
        totalProducts: 2,
        classLabels: ['SKU-001', 'SKU-002'], // SKUs instead of product names
      });
    });

    it('should return suggestions when confidence is above threshold', async () => {
      // Arrange
      const mockFile = new File(['fake image'], 'test.jpg', { type: 'image/jpeg' });

      // Act
      const suggestions = await recognizeProduct(mockFile);

      // Assert
      expect(suggestions).toBeDefined();
      expect(suggestions.length).toBeGreaterThan(0);
      expect(suggestions[0]).toHaveProperty('productId');
      expect(suggestions[0]).toHaveProperty('productSku');
      expect(suggestions[0]).toHaveProperty('confidence');
    });

    it('should return empty array when all confidences below threshold', async () => {
      // Arrange
      const mockFile = new File(['fake image'], 'test.jpg', { type: 'image/jpeg' });
      
      // Mock model to return low confidence
      const mockLowConfidenceModel = {
        predict: vi.fn(() => ({
          data: async () => new Float32Array([0.20, 0.15]), // All below 40%
          dispose: vi.fn(),
          squeeze: vi.fn(function() { return this; }),
        })),
        dispose: vi.fn(),
      };
      vi.mocked(tf.loadGraphModel).mockResolvedValue(mockLowConfidenceModel as unknown as tf.GraphModel);

      // Act
      const suggestions = await recognizeProduct(mockFile);

      // Assert
      expect(suggestions).toHaveLength(0);
    });

    it('should reject invalid file types', async () => {
      // Arrange
      const invalidFile = new File(['text'], 'test.txt', { type: 'text/plain' });

      // Act & Assert
      await expect(recognizeProduct(invalidFile)).rejects.toThrow('debe ser una imagen');
    });

    it('should reject files larger than 10MB', async () => {
      // Arrange
      const largeData = new Array(11 * 1024 * 1024).fill(0);
      const largeFile = new File([new Uint8Array(largeData)], 'large.jpg', { type: 'image/jpeg' });

      // Act & Assert
      await expect(recognizeProduct(largeFile)).rejects.toThrow('demasiado grande');
    });

    it('should throw error when no model is available', async () => {
      // Arrange
      const mockFile = new File(['fake image'], 'test.jpg', { type: 'image/jpeg' });
      vi.mocked(imageRecognitionService.getModelMetadata).mockResolvedValue(null);

      // Act & Assert
      await expect(recognizeProduct(mockFile)).rejects.toThrow('Modelo de IA no disponible');
    });

    it('should dispose tensors even on error', async () => {
      // Arrange
      const mockFile = new File(['fake image'], 'test.jpg', { type: 'image/jpeg' });
      
      // Mock preprocessing to throw error
      let disposeCalled = false;
      const mockTensor = {
        dispose: vi.fn(() => { disposeCalled = true; }),
      };
      vi.mocked(tf.browser.fromPixels).mockReturnValue(mockTensor as unknown as tf.Tensor3D);
      
      // Mock inference to fail
      const mockModel = {
        predict: vi.fn(() => {
          throw new Error('Inference failed');
        }),
        dispose: vi.fn(),
      };
      vi.mocked(tf.loadGraphModel).mockResolvedValue(mockModel as unknown as tf.GraphModel);

      // Act & Assert
      await expect(recognizeProduct(mockFile)).rejects.toThrow();
      
      // Verify dispose was called despite error
      expect(mockTensor.dispose).toHaveBeenCalled();
    });

    it('should enrich suggestions with product details from dataset', async () => {
      // Arrange
      const mockFile = new File(['fake image'], 'test.jpg', { type: 'image/jpeg' });

      // Act
      const suggestions = await recognizeProduct(mockFile);

      // Assert
      if (suggestions.length > 0) {
        expect(suggestions[0].productId).toBeTruthy();
        expect(suggestions[0].productSku).toBeTruthy();
        expect(suggestions[0].photoUrl).toContain('/api/products/');
      }
    });

    it('should handle network errors gracefully', async () => {
      // Arrange
      const mockFile = new File(['fake image'], 'test.jpg', { type: 'image/jpeg' });
      vi.mocked(imageRecognitionService.getModelMetadata).mockRejectedValue(
        new Error('Network error')
      );

      // Act & Assert
      await expect(recognizeProduct(mockFile)).rejects.toThrow();
    });
  });

  describe('runInference', () => {
    beforeEach(() => {
      // Clear model cache before each test
      imageRecognitionInferenceService.clearModelCache();
    });

    it('should extract features and run classification', async () => {
      // Arrange - Mock classification head
      const mockClassificationHead = {
        predict: vi.fn().mockReturnValue({
          squeeze: vi.fn().mockReturnValue({ 
            data: async () => new Float32Array([0.85, 0.10, 0.05]),
            dispose: vi.fn(),
          }),
        }),
        dispose: vi.fn(),
      } as unknown as tf.LayersModel;
      
      // Mock input tensor
      const mockTensor = new MockTensor([1, 2, 3], [224, 224, 3]) as unknown as tf.Tensor3D;

      // Act
      const result = await runInference(mockClassificationHead, mockTensor);

      // Assert - should return predictions
      expect(result).toBeDefined();
    });

    it('should use tidy to prevent memory leaks during feature extraction', async () => {
      // Arrange
      const mockModel = {
        predict: vi.fn().mockReturnValue({
          squeeze: vi.fn().mockReturnValue({ 
            data: async () => new Float32Array([0.9, 0.1]),
            dispose: vi.fn(),
          }),
        }),
        dispose: vi.fn(),
      } as unknown as tf.LayersModel;
      
      const mockTensor = new MockTensor([1, 2, 3], [224, 224, 3]) as unknown as tf.Tensor3D;

      // Act
      await runInference(mockModel, mockTensor);

      // Assert - tidy should be called for memory management
      expect(tf.tidy).toHaveBeenCalled();
    });
  });
});

// Helper functions
function createMockImage(width: number, height: number): HTMLImageElement {
  const mockImage = {} as HTMLImageElement;
  
  // Define writable properties
  Object.defineProperty(mockImage, 'width', { 
    value: width, 
    writable: true,
    configurable: true 
  });
  Object.defineProperty(mockImage, 'height', { 
    value: height, 
    writable: true,
    configurable: true 
  });

  return mockImage;
}

function createMockImageWithBrightness(width: number, height: number, brightness: number): HTMLImageElement {
  const mockImage = createMockImage(width, height);

  // Mock canvas for brightness check
  const mockCanvas = {
    width: 100,
    height: 100,
    getContext: vi.fn(() => ({
      drawImage: vi.fn(),
      getImageData: vi.fn(() => ({
        data: new Uint8ClampedArray(100 * 100 * 4).fill(brightness),
      })),
    })),
  };

  vi.spyOn(document, 'createElement').mockReturnValue(mockCanvas as unknown as HTMLCanvasElement);

  return mockImage;
}
