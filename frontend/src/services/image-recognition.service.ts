/**
 * Image Recognition Inference Service (EP4 - Section 13A)
 * Client-side TensorFlow.js inference for product identification.
 */

import * as tf from '@tensorflow/tfjs';
import { imageRecognitionService } from './sales.service';
import type { ProductSuggestion } from '@/types/sales.types';

const CONFIDENCE_THRESHOLD = 0.4; // 40% confidence threshold
const IMAGE_SIZE = 224; // MobileNetV2 input size
const MAX_SUGGESTIONS = 5;

export interface DeviceCapabilities {
  hasWebGL: boolean;
  hasWebGL2: boolean;
  hasTensorFlowJS: boolean;
  backend: string;
  gpuInfo?: string;
}

/**
 * Check device compatibility for image recognition.
 */
export async function checkDeviceCompatibility(): Promise<DeviceCapabilities> {
  try {
    await tf.ready();
    
    const canvas = document.createElement('canvas');
    const gl = canvas.getContext('webgl');
    const gl2 = canvas.getContext('webgl2');
    
    return {
      hasWebGL: !!gl,
      hasWebGL2: !!gl2,
      hasTensorFlowJS: true,
      backend: tf.getBackend(),
      gpuInfo: gl2 ? 'WebGL 2.0 detected' : gl ? 'WebGL 1.0 detected' : 'No WebGL',
    };
  } catch (error) {
    console.error('Device compatibility check failed:', error);
    return {
      hasWebGL: false,
      hasWebGL2: false,
      hasTensorFlowJS: false,
      backend: 'none',
    };
  }
}

/**
 * Validate image content before inference.
 */
export function validateImage(imageFile: File): { isValid: boolean; error?: string } {
  // Check file type
  if (!imageFile.type.startsWith('image/')) {
    return { isValid: false, error: 'El archivo debe ser una imagen' };
  }

  // Check file size (max 10MB)
  if (imageFile.size > 10 * 1024 * 1024) {
    return { isValid: false, error: 'La imagen es demasiado grande (máx. 10MB)' };
  }

  return { isValid: true };
}

/**
 * Validate image dimensions and content.
 */
export async function validateImageDimensions(
  imageElement: HTMLImageElement
): Promise<{ isValid: boolean; error?: string }> {
  const { width, height } = imageElement;

  // Check minimum dimensions
  if (width < 200 || height < 200) {
    return { isValid: false, error: 'La imagen es demasiado pequeña (mín. 200x200px)' };
  }

  // Check aspect ratio (reject extremely wide/tall images)
  const aspectRatio = Math.max(width, height) / Math.min(width, height);
  if (aspectRatio > 5) {
    return { isValid: false, error: 'La imagen tiene una proporción de aspecto extrema' };
  }

  // Check if image is not completely black or white (histogram check)
  const canvas = document.createElement('canvas');
  const ctx = canvas.getContext('2d');
  if (!ctx) {
    return { isValid: false, error: 'Error al procesar la imagen' };
  }

  canvas.width = Math.min(width, 100);
  canvas.height = Math.min(height, 100);
  ctx.drawImage(imageElement, 0, 0, canvas.width, canvas.height);

  const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
  const data = imageData.data;
  let sumBrightness = 0;

  for (let i = 0; i < data.length; i += 4) {
    const r = data[i];
    const g = data[i + 1];
    const b = data[i + 2];
    sumBrightness += (r + g + b) / 3;
  }

  const avgBrightness = sumBrightness / (data.length / 4);

  // Reject if too dark (avg < 20) or too bright (avg > 235)
  if (avgBrightness < 20) {
    return { isValid: false, error: 'La imagen es demasiado oscura' };
  }
  if (avgBrightness > 235) {
    return { isValid: false, error: 'La imagen es demasiado clara' };
  }

  return { isValid: true };
}

/**
 * Preprocess image for model inference.
 */
export async function preprocessImage(imageElement: HTMLImageElement): Promise<tf.Tensor3D> {
  return tf.tidy(() => {
    // Convert to tensor and resize
    let tensor = tf.browser.fromPixels(imageElement);
    
    // Resize to model input size (preserving aspect ratio)
    tensor = tf.image.resizeBilinear(tensor, [IMAGE_SIZE, IMAGE_SIZE]);
    
    // Normalize pixel values to [-1, 1] for MobileNetV2
    tensor = tensor.toFloat().div(127.5).sub(1);
    
    return tensor as tf.Tensor3D;
  });
}

/**
 * Load model from server.
 */
export async function loadModel(version: string): Promise<tf.GraphModel> {
  try {
    const modelUrl = imageRecognitionService.getModelUrl(version, 'model.json');
    const model = await tf.loadGraphModel(modelUrl);
    return model;
  } catch (error) {
    console.error('Failed to load model:', error);
    throw new Error('Error al cargar el modelo de IA');
  }
}

/**
 * Run inference on preprocessed image.
 */
export async function runInference(
  model: tf.GraphModel,
  preprocessedImage: tf.Tensor3D
): Promise<tf.Tensor> {
  return tf.tidy(() => {
    // Add batch dimension
    const batched = preprocessedImage.expandDims(0);
    
    // Run inference
    const predictions = model.predict(batched) as tf.Tensor;
    
    // Remove batch dimension
    return predictions.squeeze();
  });
}

/**
 * Convert model output to product suggestions.
 */
export async function generateSuggestions(
  predictions: tf.Tensor,
  classLabels: string[]
): Promise<ProductSuggestion[]> {
  const confidences = await predictions.data();
  
  // Get top predictions with confidence >= threshold
  const suggestions: Array<{ index: number; confidence: number }> = [];
  
  for (let i = 0; i < confidences.length; i++) {
    if (confidences[i] >= CONFIDENCE_THRESHOLD) {
      suggestions.push({ index: i, confidence: confidences[i] });
    }
  }

  // Sort by confidence descending
  suggestions.sort((a, b) => b.confidence - a.confidence);

  // Take top MAX_SUGGESTIONS
  const topSuggestions = suggestions.slice(0, MAX_SUGGESTIONS);

  // Map to ProductSuggestion objects
  // Note: In a real implementation, you'd need to fetch product details from the backend
  // For now, this is a simplified version
  return topSuggestions.map((suggestion) => ({
    productId: '', // Would be populated from backend
    productSku: '', // Would be populated from backend
    productName: classLabels[suggestion.index],
    confidence: suggestion.confidence * 100, // Convert to percentage
    photoUrl: '', // Would be populated from backend
  }));
}

/**
 * Complete image recognition workflow.
 */
export async function recognizeProduct(imageFile: File): Promise<ProductSuggestion[]> {
  // Step 1: Validate file
  const fileValidation = validateImage(imageFile);
  if (!fileValidation.isValid) {
    throw new Error(fileValidation.error);
  }

  // Step 2: Load image
  const imageElement = await loadImageElement(imageFile);

  // Step 3: Validate dimensions
  const dimensionValidation = await validateImageDimensions(imageElement);
  if (!dimensionValidation.isValid) {
    throw new Error(dimensionValidation.error);
  }

  // Step 4: Check for model availability
  const metadata = await imageRecognitionService.getModelMetadata();
  if (!metadata) {
    throw new Error('Modelo de IA no disponible aún. Entrene el primer modelo.');
  }

  // Step 5: Load model
  const model = await loadModel(metadata.version);

  // Step 6: Preprocess image
  const preprocessed = await preprocessImage(imageElement);

  try {
    // Step 7: Run inference
    const predictions = await runInference(model, preprocessed);

    // Step 8: Get training dataset to map predictions to products
    const dataset = await imageRecognitionService.getTrainingDataset();

    // Step 9: Generate suggestions
    const suggestions = await generateSuggestions(predictions, dataset.classLabels);

    // Step 10: Enrich with product details
    const enrichedSuggestions = await enrichSuggestionsWithProductDetails(suggestions, dataset);

    // Clean up tensors
    preprocessed.dispose();
    predictions.dispose();

    return enrichedSuggestions;
  } catch (error) {
    preprocessed.dispose();
    throw error;
  }
}

/**
 * Load image file into HTMLImageElement.
 */
function loadImageElement(imageFile: File): Promise<HTMLImageElement> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    
    reader.onload = (e) => {
      const img = new Image();
      
      img.onload = () => resolve(img);
      img.onerror = () => reject(new Error('Error al cargar la imagen'));
      
      img.src = e.target?.result as string;
    };
    
    reader.onerror = () => reject(new Error('Error al leer el archivo'));
    reader.readAsDataURL(imageFile);
  });
}

/**
 * Enrich suggestions with full product details from training dataset.
 */
async function enrichSuggestionsWithProductDetails(
  suggestions: ProductSuggestion[],
  dataset: { photos: Array<{ productId: string; productSku: string; productName: string; photoUrl: string }> }
): Promise<ProductSuggestion[]> {
  return suggestions.map((suggestion) => {
    // Find product in dataset by name (class label)
    const product = dataset.photos.find((p) => p.productName === suggestion.productName);
    
    if (product) {
      return {
        ...suggestion,
        productId: product.productId,
        productSku: product.productSku,
        photoUrl: product.photoUrl,
      };
    }
    
    return suggestion;
  });
}

export const imageRecognitionInferenceService = {
  checkDeviceCompatibility,
  validateImage,
  validateImageDimensions,
  recognizeProduct,
};
