/**
 * Image Recognition Inference Service (EP4 - Section 13A)
 * Client-side TensorFlow.js inference for product identification.
 * 
 * Uses MobileNetV2 as feature extractor + trained classification head.
 */

import * as tf from '@tensorflow/tfjs';
import { imageRecognitionService } from './sales.service';
import type { ProductSuggestion } from '@/types/sales.types';

const CONFIDENCE_THRESHOLD = 0.4; // 40% confidence threshold for individual suggestions
const MIN_TOP_CONFIDENCE = 0.5; // 50% minimum for top prediction to be valid
const DOMINANCE_RATIO = 1.5; // Top prediction must be 1.5x higher than second
const MAX_ENTROPY_THRESHOLD = 2.0; // Maximum entropy for valid predictions
const IMAGE_SIZE = 224; // MobileNetV2 input size
const MAX_SUGGESTIONS = 5;

// MobileNetV2 feature extractor URL (same as training)
const MOBILENET_URL = 'https://tfhub.dev/google/tfjs-model/imagenet/mobilenet_v2_100_224/feature_vector/3/default/1';

// Cache for loaded models to avoid reloading
let cachedFeatureExtractor: tf.GraphModel | null = null;
let cachedClassificationHead: tf.LayersModel | null = null;
let cachedModelVersion: string | null = null;
let cachedClassLabels: string[] | null = null;

/**
 * Stored class labels from training (saved in class_labels.json).
 */
interface StoredClassLabels {
  labels: string[];
  trainedAt: string;
  version: string;
}

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

  // Reject if too dark/bright.
  // Note: many product photos are shot on white backgrounds, so be permissive on the bright side.
  // Keep only extreme cases to avoid blocking normal catalog photos.
  if (avgBrightness < 5) {
    return { isValid: false, error: 'La imagen es demasiado oscura' };
  }
  if (avgBrightness > 250) {
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
 * Load MobileNetV2 feature extractor from TensorFlow Hub.
 * Cached to avoid reloading on every inference.
 */
async function loadFeatureExtractor(): Promise<tf.GraphModel> {
  if (cachedFeatureExtractor) {
    return cachedFeatureExtractor;
  }
  
  try {
    console.log('Loading MobileNetV2 feature extractor...');
    cachedFeatureExtractor = await tf.loadGraphModel(MOBILENET_URL, { fromTFHub: true });
    console.log('MobileNetV2 loaded successfully');
    return cachedFeatureExtractor;
  } catch (error) {
    console.error('Failed to load MobileNetV2:', error);
    throw new Error('Error al cargar el extractor de características');
  }
}

/**
 * Load class labels from the model's class_labels.json file.
 * These are the labels used during training and must match the model output indices.
 */
async function loadClassLabels(version: string): Promise<string[]> {
  // Return cached if same version
  if (cachedClassLabels && cachedModelVersion === version) {
    return cachedClassLabels;
  }
  
  try {
    const classLabelsUrl = imageRecognitionService.getModelUrl(version, 'class_labels.json');
    const cacheBustedUrl = `${classLabelsUrl}${classLabelsUrl.includes('?') ? '&' : '?'}cb=${Date.now()}`;
    
    const response = await fetch(cacheBustedUrl, { credentials: 'include' });
    
    if (!response.ok) {
      console.warn('class_labels.json not found, falling back to API class labels');
      // Fallback to API for older models that don't have class_labels.json
      const apiLabels = await imageRecognitionService.getClassLabels();
      cachedClassLabels = apiLabels.classLabels;
      return cachedClassLabels;
    }
    
    const data: StoredClassLabels = await response.json();
    console.log(`Loaded ${data.labels.length} class labels from model ${version}`);
    cachedClassLabels = data.labels;
    return cachedClassLabels;
  } catch (error) {
    console.warn('Failed to load class_labels.json, falling back to API:', error);
    // Fallback to API
    const apiLabels = await imageRecognitionService.getClassLabels();
    cachedClassLabels = apiLabels.classLabels;
    return cachedClassLabels;
  }
}

/**
 * Load classification head model from server.
 */
export async function loadModel(version: string): Promise<tf.LayersModel> {
  // Return cached model if same version
  if (cachedClassificationHead && cachedModelVersion === version) {
    return cachedClassificationHead;
  }
  
  try {
    const modelUrl = imageRecognitionService.getModelUrl(version, 'model.json');
    const cacheBustedUrl = `${modelUrl}${modelUrl.includes('?') ? '&' : '?'}cb=${Date.now()}`;
    
    // Browser-trained models are LayersModel format (classification head)
    try {
      cachedClassificationHead = await tf.loadLayersModel(cacheBustedUrl, {
        requestInit: { credentials: 'include' },
      });
    } catch (layersError) {
      // Backward-compat fallback: if a GraphModel is served, try graph loader
      const graph = await tf.loadGraphModel(cacheBustedUrl, {
        requestInit: { credentials: 'include' },
      });
      cachedClassificationHead = graph as unknown as tf.LayersModel;
    }
    
    cachedModelVersion = version;
    return cachedClassificationHead;
  } catch (error) {
    console.error('Failed to load classification model:', error);
    throw new Error('Error al cargar el modelo de IA');
  }
}

/**
 * Extract features from preprocessed image using MobileNetV2.
 */
async function extractImageFeatures(preprocessedImage: tf.Tensor3D): Promise<tf.Tensor2D> {
  const featureExtractor = await loadFeatureExtractor();
  
  return tf.tidy(() => {
    // Add batch dimension [1, 224, 224, 3]
    const batched = preprocessedImage.expandDims(0);
    
    // Extract features using MobileNetV2
    const features = featureExtractor.predict(batched) as tf.Tensor;
    
    // MobileNetV2 outputs [1, 1, 1, 1280], reshape to [1, 1280]
    return features.reshape([1, -1]) as tf.Tensor2D;
  });
}

/**
 * Run inference on preprocessed image.
 * Uses MobileNetV2 for feature extraction, then classification head.
 */
export async function runInference(
  model: tf.LayersModel,
  preprocessedImage: tf.Tensor3D
): Promise<tf.Tensor> {
  // Step 1: Extract features using MobileNetV2
  const features = await extractImageFeatures(preprocessedImage);
  
  try {
    // Step 2: Run classification head on features
    const predictions = tf.tidy(() => {
      const result = (model as any).predict(features) as tf.Tensor;
      // Remove batch dimension
      return result.squeeze();
    });
    
    return predictions;
  } finally {
    // Clean up features tensor
    features.dispose();
  }
}

/**
 * Validate if predictions indicate a known product (not out-of-distribution).
 * Uses multiple heuristics to detect when the model is uncertain or the image
 * doesn't match any known product category.
 */
export interface PredictionValidation {
  isValid: boolean;
  reason?: string;
  topConfidence: number;
  entropy: number;
  dominanceRatio: number;
  numClasses: number;
}

export async function validatePredictions(predictions: tf.Tensor): Promise<PredictionValidation> {
  const confidences = await predictions.data();
  const confidenceArray = Array.from(confidences);
  const numClasses = confidenceArray.length;
  
  // Sort confidences descending
  const sortedConfidences = [...confidenceArray].sort((a, b) => b - a);
  const topConfidence = sortedConfidences[0];
  const secondConfidence = sortedConfidences[1] || 0;
  
  // Calculate entropy: H = -sum(p * log2(p)) for non-zero probabilities
  // Higher entropy = more uncertainty = less confident prediction
  let entropy = 0;
  for (const p of confidenceArray) {
    if (p > 1e-10) { // Avoid log(0)
      entropy -= p * Math.log2(p);
    }
  }
  
  // Normalize entropy by maximum possible entropy for this number of classes
  // Max entropy = log2(numClasses) when all classes have equal probability
  const maxEntropy = Math.log2(numClasses);
  const normalizedEntropy = maxEntropy > 0 ? entropy / maxEntropy : 0;
  
  // Calculate dominance ratio (how much the top prediction dominates)
  const dominanceRatio = secondConfidence > 0 ? topConfidence / secondConfidence : Infinity;
  
  // Adjust thresholds based on number of classes
  // With fewer classes, we need different validation strategies
  let requiredConfidence = MIN_TOP_CONFIDENCE;
  let requiredDominance = DOMINANCE_RATIO;
  let maxNormalizedEntropy = 0.85; // Default: allow up to 85% of max entropy
  
  if (numClasses <= 2) {
    // For 2 classes with limited training data:
    // - Even 55% is meaningful (vs 50% random chance)
    // - We accept lower confidence but log a warning
    requiredConfidence = 0.55;
    requiredDominance = 1.2; // 0.55/0.45 ≈ 1.22
    maxNormalizedEntropy = 0.98; // Almost any distribution
  } else if (numClasses <= 5) {
    requiredConfidence = 0.50;
    requiredDominance = 1.4;
    maxNormalizedEntropy = 0.90;
  } else {
    // For many classes, lower thresholds since probability is spread more
    requiredConfidence = 0.40;
    requiredDominance = 1.3;
    maxNormalizedEntropy = 0.85;
  }
  
  // Validation checks:
  
  // 1. Top confidence must meet threshold (adjusted for number of classes)
  if (topConfidence < requiredConfidence) {
    return {
      isValid: false,
      reason: `Confianza insuficiente (${(topConfidence * 100).toFixed(1)}% < ${requiredConfidence * 100}% mínimo para ${numClasses} clases)`,
      topConfidence,
      entropy,
      dominanceRatio,
      numClasses,
    };
  }
  
  // 2. Normalized entropy check - reject if predictions are too spread out
  // Skip this check for 2-class models since it's too restrictive
  if (numClasses > 2 && normalizedEntropy > maxNormalizedEntropy) {
    return {
      isValid: false,
      reason: `Alta incertidumbre (entropía normalizada: ${normalizedEntropy.toFixed(2)} > ${maxNormalizedEntropy})`,
      topConfidence,
      entropy,
      dominanceRatio,
      numClasses,
    };
  }
  
  // 3. Dominance check - top prediction should be significantly higher than second
  // For 2-class models, confidence check is sufficient; dominance is implied
  if (numClasses > 2 && dominanceRatio < requiredDominance && topConfidence < 0.80) {
    return {
      isValid: false,
      reason: `Predicción no dominante (ratio: ${dominanceRatio.toFixed(2)} < ${requiredDominance} requerido)`,
      topConfidence,
      entropy,
      dominanceRatio,
      numClasses,
    };
  }
  
  // Log warning if confidence is low (model may need more training data)
  if (topConfidence < 0.70) {
    console.warn(`⚠️ Confianza baja (${(topConfidence * 100).toFixed(1)}%). ` +
      `Considere agregar más fotos de entrenamiento para mejorar la precisión.`);
  }
  
  return {
    isValid: true,
    topConfidence,
    entropy,
    dominanceRatio,
    numClasses,
  };
}

/**
 * Convert model output to product suggestions.
 */
export async function generateSuggestions(
  predictions: tf.Tensor,
  classLabels: string[]
): Promise<ProductSuggestion[]> {
  // First, validate if the predictions indicate a known product
  const validation = await validatePredictions(predictions);
  
  console.log('=== OOD VALIDATION ===');
  console.log('Validation result:', validation);
  console.log('Class labels available:', classLabels);
  console.log('======================');
  
  if (!validation.isValid) {
    console.warn('❌ OOD Detection REJECTED image:', validation.reason);
    // Return empty array - the image doesn't match any known product
    return [];
  }
  
  console.log('✅ OOD Detection PASSED - generating suggestions');
  
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

    // Step 8: Load class labels FROM THE MODEL (not from current API)
    // This ensures we use the exact labels the model was trained with
    const trainedClassLabels = await loadClassLabels(metadata.version);
    
    // DEBUG: Log predictions and class labels
    const predictionData = await predictions.data();
    const numPredictions = predictionData.length;
    const numLabels = trainedClassLabels.length;
    
    console.log('=== IMAGE RECOGNITION DEBUG ===');
    console.log('Model version:', metadata.version);
    console.log('Number of model outputs:', numPredictions);
    console.log('Number of class labels:', numLabels);
    
    // CRITICAL: Check for mismatch between model outputs and labels
    if (numPredictions !== numLabels) {
      console.error('⚠️ MISMATCH: Model outputs', numPredictions, 'classes but we have', numLabels, 'labels!');
      console.error('This usually means the model was trained with different products than currently exist.');
      console.error('Please retrain the model to fix this issue.');
    }
    
    console.log('Class labels:', trainedClassLabels);
    console.log('Raw predictions:');
    Array.from(predictionData).forEach((p, i) => {
      const label = trainedClassLabels[i] || `Unknown-${i}`;
      const bar = '█'.repeat(Math.round(p * 20));
      console.log(`  [${i}] ${label}: ${(p * 100).toFixed(1)}% ${bar}`);
    });
    console.log('================================');
    
    // Step 9: Generate suggestions using the trained class labels
    const suggestions = await generateSuggestions(predictions, trainedClassLabels);

    // Step 10: Get current product mappings from API for enrichment (IDs, URLs, etc.)
    const classLabelsData = await imageRecognitionService.getClassLabels();
    
    // Step 11: Enrich with product details from mappings
    const enrichedSuggestions = enrichSuggestionsFromMappings(suggestions, classLabelsData.productMappings);

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
 * Enrich suggestions with product details from class label mappings.
 */
function enrichSuggestionsFromMappings(
  suggestions: ProductSuggestion[],
  productMappings: Record<string, { productId: string; productSku: string; productName: string; photoUrl: string }>
): ProductSuggestion[] {
  return suggestions.map((suggestion) => {
    // Find product in mappings by name (class label)
    const product = productMappings[suggestion.productName];
    
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

/**
 * Clear cached models (useful for testing or forcing reload).
 */
export function clearModelCache(): void {
  if (cachedFeatureExtractor) {
    cachedFeatureExtractor.dispose();
    cachedFeatureExtractor = null;
  }
  if (cachedClassificationHead) {
    cachedClassificationHead.dispose();
    cachedClassificationHead = null;
  }
  cachedModelVersion = null;
  cachedClassLabels = null;
}

export const imageRecognitionInferenceService = {
  checkDeviceCompatibility,
  validateImage,
  validateImageDimensions,
  recognizeProduct,
  clearModelCache,
};
