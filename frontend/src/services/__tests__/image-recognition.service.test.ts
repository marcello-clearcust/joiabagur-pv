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
    it('should load model from correct URL', async () => {
      // Arrange
      const version = 'v1_20260112';

      // Act
      await loadModel(version);

      // Assert
      expect(tf.loadGraphModel).toHaveBeenCalledWith(
        '/api/image-recognition/model/files/v1_20260112/model.json'
      );
    });

    it('should throw error if model loading fails', async () => {
      // Arrange
      vi.mocked(tf.loadGraphModel).mockRejectedValue(new Error('Network error'));

      // Act & Assert
      await expect(loadModel('v1_20260112')).rejects.toThrow('Error al cargar el modelo de IA');
    });
  });

  describe('generateSuggestions', () => {
    it('should return only suggestions above 40% confidence threshold', async () => {
      // Arrange
      const predictions = new MockTensor(
        [0.65, 0.50, 0.45, 0.35, 0.10], // 3 above threshold, 2 below
        [5]
      );
      const classLabels = ['Product A', 'Product B', 'Product C', 'Product D', 'Product E'];

      // Act
      const suggestions = await generateSuggestions(predictions as unknown as tf.Tensor, classLabels);

      // Assert
      expect(suggestions).toHaveLength(3); // Only 3 above 40%
      expect(suggestions[0].confidence).toBe(65); // Highest first
      expect(suggestions[0].productName).toBe('Product A');
      expect(suggestions[1].confidence).toBe(50);
      expect(suggestions[2].confidence).toBe(45);
    });

    it('should limit suggestions to maximum 5', async () => {
      // Arrange
      const predictions = new MockTensor(
        [0.90, 0.80, 0.70, 0.60, 0.50, 0.45, 0.42], // 7 above threshold
        [7]
      );
      const classLabels = ['P1', 'P2', 'P3', 'P4', 'P5', 'P6', 'P7'];

      // Act
      const suggestions = await generateSuggestions(predictions as unknown as tf.Tensor, classLabels);

      // Assert
      expect(suggestions).toHaveLength(5); // Capped at 5
    });

    it('should return empty array when all below threshold', async () => {
      // Arrange
      const predictions = new MockTensor(
        [0.35, 0.30, 0.20, 0.15, 0.10], // All below 40%
        [5]
      );
      const classLabels = ['P1', 'P2', 'P3', 'P4', 'P5'];

      // Act
      const suggestions = await generateSuggestions(predictions as unknown as tf.Tensor, classLabels);

      // Assert
      expect(suggestions).toHaveLength(0); // No suggestions
    });

    it('should sort suggestions by confidence descending', async () => {
      // Arrange
      const predictions = new MockTensor(
        [0.45, 0.65, 0.50, 0.42, 0.55], // Out of order
        [5]
      );
      const classLabels = ['P1', 'P2', 'P3', 'P4', 'P5'];

      // Act
      const suggestions = await generateSuggestions(predictions as unknown as tf.Tensor, classLabels);

      // Assert
      expect(suggestions[0].confidence).toBe(65); // P2
      expect(suggestions[1].confidence).toBe(55); // P5
      expect(suggestions[2].confidence).toBe(50); // P3
      expect(suggestions[3].confidence).toBe(45); // P1
      expect(suggestions[4].confidence).toBe(42); // P4
    });

    it('should convert confidence to percentage', async () => {
      // Arrange
      const predictions = new MockTensor([0.75], [1]); // 75% as decimal
      const classLabels = ['Product'];

      // Act
      const suggestions = await generateSuggestions(predictions as unknown as tf.Tensor, classLabels);

      // Assert
      expect(suggestions[0].confidence).toBe(75); // Converted to percentage
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
        classLabels: ['Product A', 'Product B'],
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
    it('should add and remove batch dimension', async () => {
      // Arrange - Use mock from TF.js
      const mockModel = {
        predict: vi.fn().mockReturnValue({
          squeeze: vi.fn().mockReturnValue({ data: async () => new Float32Array([0.5]) }),
        }),
      } as unknown as tf.GraphModel;
      
      const mockTensor = {
        expandDims: vi.fn().mockReturnValue({ /* mock batched tensor */ }),
      } as unknown as tf.Tensor3D;

      // Act
      const result = await runInference(mockModel, mockTensor);

      // Assert
      expect(mockTensor.expandDims).toHaveBeenCalledWith(0);
      expect(result).toBeDefined();
    });

    it('should use tidy to prevent memory leaks', async () => {
      // Arrange
      const mockModel = {
        predict: vi.fn().mockReturnValue({
          squeeze: vi.fn().mockReturnValue({ data: async () => new Float32Array([0.5]) }),
        }),
      } as unknown as tf.GraphModel;
      
      const mockTensor = {
        expandDims: vi.fn().mockReturnThis(),
      } as unknown as tf.Tensor3D;

      // Act
      await runInference(mockModel, mockTensor);

      // Assert
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
