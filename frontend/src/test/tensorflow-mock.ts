/**
 * TensorFlow.js Mock for Testing
 * Provides mock implementations of TensorFlow.js functions to avoid loading real models in tests.
 */
import { vi } from 'vitest';

// Mock tensor class
export class MockTensor {
  public shape: number[];
  private _data: Float32Array;

  constructor(data: number[] | Float32Array, shape: number[]) {
    this.shape = shape;
    this._data = data instanceof Float32Array ? data : new Float32Array(data);
  }

  async data(): Promise<Float32Array> {
    return this._data;
  }

  dispose(): void {
    // Mock dispose - no-op
  }

  expandDims(axis: number): MockTensor {
    const newShape = [...this.shape];
    newShape.splice(axis, 0, 1);
    return new MockTensor(this.data, newShape);
  }

  squeeze(axis?: number[]): MockTensor {
    const newShape = this.shape.filter((dim, i) => !axis || !axis.includes(i));
    return new MockTensor(this._data, newShape);
  }

  reshape(newShape: number[]): MockTensor {
    return new MockTensor(this._data, newShape);
  }

  slice(begin: number[], size: number[]): MockTensor {
    // Simplified slice - just return a tensor with the expected shape
    const newShape = size.map((s, i) => (s === -1 ? this.shape[i] - begin[i] : s));
    const newSize = newShape.reduce((a, b) => a * b, 1);
    return new MockTensor(new Float32Array(newSize), newShape);
  }

  toFloat(): MockTensor {
    return this;
  }

  div(value: number): MockTensor {
    const newData = new Float32Array(this._data.length);
    for (let i = 0; i < this._data.length; i++) {
      newData[i] = this._data[i] / value;
    }
    return new MockTensor(newData, this.shape);
  }

  sub(value: number): MockTensor {
    const newData = new Float32Array(this._data.length);
    for (let i = 0; i < this._data.length; i++) {
      newData[i] = this._data[i] - value;
    }
    return new MockTensor(newData, this.shape);
  }
}

// Mock model class (used as feature extractor - MobileNetV2)
export class MockGraphModel {
  public predict(input: MockTensor): MockTensor {
    // Return mock feature vectors (1280-dimensional for MobileNetV2)
    // Shape is [batch, 1, 1, 1280] for TF Hub MobileNetV2
    const batchSize = input.shape[0] || 1;
    const featureSize = 1280;
    const features = new Float32Array(batchSize * featureSize);
    // Fill with mock feature values
    for (let i = 0; i < features.length; i++) {
      features[i] = Math.random() * 0.5 + 0.25; // Random values between 0.25 and 0.75
    }
    return new MockTensor(features, [batchSize, 1, 1, featureSize]);
  }

  dispose(): void {
    // Mock dispose - no-op
  }
}

// Mock layers model
export class MockLayersModel {
  public layers: unknown[] = [];

  public predict(input: MockTensor): MockTensor {
    // Return mock predictions with a dominant class (passes OOD validation)
    const predictions = new Float32Array([
      0.85, // 85% confidence for class 0 (dominant)
      0.10, // 10% confidence for class 1
      0.03, // 3% confidence for class 2
      0.02, // 2% confidence for class 3
    ]);
    return new MockTensor(predictions, [4]);
  }

  public async fit(
    x: MockTensor,
    y: MockTensor,
    config: {
      epochs: number;
      batchSize: number;
      validationSplit: number;
      shuffle: boolean;
      callbacks?: {
        onEpochEnd?: (epoch: number, logs: { val_acc?: number; acc?: number; val_loss?: number; loss?: number }) => void;
      };
    }
  ): Promise<void> {
    // Simulate training with callbacks
    for (let epoch = 0; epoch < config.epochs; epoch++) {
      await new Promise((resolve) => setTimeout(resolve, 10)); // Simulate training time

      const accuracy = 0.5 + (epoch / config.epochs) * 0.3; // Simulate improving accuracy
      const loss = 2.0 - (epoch / config.epochs) * 1.5; // Simulate decreasing loss

      if (config.callbacks?.onEpochEnd) {
        config.callbacks.onEpochEnd(epoch, {
          val_acc: accuracy,
          acc: accuracy,
          val_loss: loss,
          loss: loss,
        });
      }
    }
  }

  public async save(handler: unknown): Promise<{
    modelTopology?: object;
    weightData?: ArrayBuffer;
    weightSpecs?: unknown[];
  }> {
    // Return mock model artifacts
    return {
      modelTopology: { format: 'graph-model', generatedBy: 'mock-test' },
      weightData: new ArrayBuffer(1024), // Mock 1KB of weights
      weightSpecs: [{ name: 'dense/kernel', shape: [128, 5], dtype: 'float32' }],
    };
  }

  public compile(config: unknown): void {
    // Mock compile - no-op
  }

  public add(layer: unknown): void {
    // Mock add - no-op
  }

  dispose(): void {
    // Mock dispose - no-op
  }
}

// Create TensorFlow.js mock
export function createTensorFlowMock() {
  return {
    // Core functions
    ready: vi.fn().mockResolvedValue(undefined),
    getBackend: vi.fn().mockReturnValue('webgl'),
    setBackend: vi.fn(),

    // Model loading
    loadGraphModel: vi.fn().mockResolvedValue(new MockGraphModel()),
    loadLayersModel: vi.fn().mockResolvedValue(new MockLayersModel()),
    
    // Tensor operations
    tidy: vi.fn((fn) => fn()),
    dispose: vi.fn(),

    // Browser operations
    browser: {
      fromPixels: vi.fn((img: HTMLImageElement) => {
        // Create mock tensor from image dimensions
        const size = 224 * 224 * 3; // RGB image
        const data = new Float32Array(size).fill(128); // Gray image
        return new MockTensor(data, [224, 224, 3]);
      }),
    },

    // Image operations
    image: {
      resizeBilinear: vi.fn((tensor: MockTensor, size: [number, number]) => {
        const length = size[0] * size[1] * 3;
        const data = new Float32Array(length).fill(128);
        return new MockTensor(data, [...size, 3]);
      }),
    },

    // Layers API
    sequential: vi.fn(() => new MockLayersModel()),
    layers: {
      dense: vi.fn((config: unknown) => ({ config })),
      dropout: vi.fn((config: unknown) => ({ config })),
    },

    // Training
    train: {
      adam: vi.fn((learningRate: number) => ({ learningRate })),
    },

    // Regularizers
    regularizers: {
      l2: vi.fn((config: unknown) => config),
    },

    // Tensor creation
    stack: vi.fn((tensors: MockTensor[]) => {
      return new MockTensor(
        new Float32Array(tensors.length * 224 * 224 * 3),
        [tensors.length, 224, 224, 3]
      );
    }),

    oneHot: vi.fn((indices: number[], depth: number) => {
      const data = new Float32Array(indices.length * depth).fill(0);
      indices.forEach((idx, i) => {
        data[i * depth + idx] = 1;
      });
      return new MockTensor(data, [indices.length, depth]);
    }),

    concat: vi.fn((tensors: MockTensor[], axis: number) => {
      // Simplified concat - just combine the data
      const totalSize = tensors.reduce((sum, t) => {
        const tData = (t as any)._data as Float32Array;
        return sum + tData.length;
      }, 0);
      const combinedData = new Float32Array(totalSize);
      let offset = 0;
      for (const t of tensors) {
        const tData = (t as any)._data as Float32Array;
        combinedData.set(tData, offset);
        offset += tData.length;
      }
      // Calculate new shape
      const newShape = [...tensors[0].shape];
      newShape[axis] = tensors.reduce((sum, t) => sum + t.shape[axis], 0);
      return new MockTensor(combinedData, newShape);
    }),

    // IO operations
    io: {
      withSaveHandler: vi.fn((handler: (artifacts: unknown) => unknown) => ({
        save: handler,
      })),
    },
  };
}

// Export type for better TypeScript support
export type MockTensorFlowJS = ReturnType<typeof createTensorFlowMock>;
