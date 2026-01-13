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
    return new MockTensor(this.data, newShape);
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

// Mock model class
export class MockGraphModel {
  public predict(input: MockTensor): MockTensor {
    // Return mock predictions (5 classes with varying confidence)
    const predictions = new Float32Array([
      0.65, // 65% confidence for class 0
      0.50, // 50% confidence for class 1
      0.45, // 45% confidence for class 2
      0.30, // 30% confidence for class 3
      0.10, // 10% confidence for class 4
    ]);
    return new MockTensor(predictions, [5]);
  }

  dispose(): void {
    // Mock dispose - no-op
  }
}

// Mock layers model
export class MockLayersModel {
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
        return new MockTensor(tensor.data.slice(0, size[0] * size[1] * 3), [...size, 3]);
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
