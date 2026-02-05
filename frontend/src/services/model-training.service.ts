/**
 * Client-Side Model Training Service (EP4 - Section 13B)
 * Browser-based TensorFlow.js model training (NO Python required).
 * 
 * MEMORY MANAGEMENT STRATEGY:
 * - Images are processed in small batches (8 at a time) to prevent GPU exhaustion
 * - Features are extracted immediately after loading each batch
 * - Raw images are disposed immediately after feature extraction
 * - Aggressive use of tf.tidy() and tf.nextFrame() for memory cleanup
 * - Reduced batch sizes (4 for feature extraction, 16 for training)
 * - GPU memory monitoring with warnings when approaching limits
 */

import * as tf from '@tensorflow/tfjs';
import { imageRecognitionService } from './sales.service';
import type { TrainingDataset } from '@/types/sales.types';

const MOBILENET_URL = 'https://tfhub.dev/google/tfjs-model/imagenet/mobilenet_v2_100_224/feature_vector/3/default/1';
const IMAGE_SIZE = 224;
const EPOCHS = 15;
const BATCH_SIZE = 16; // Reduced from 32 to prevent memory issues
const VALIDATION_SPLIT = 0.2;
const MAX_CONCURRENT_IMAGE_LOADS = 10; // Limit concurrent image loading

export interface TrainingCapabilities {
  hasGPU: boolean;
  hasWebGL2: boolean;
  memoryMB: number;
  backend: string;
  estimatedDuration: string;
}

export interface TrainingProgress {
  stage: 'downloading' | 'loading_model' | 'augmenting' | 'training' | 'uploading' | 'complete';
  progress: number; // 0-100
  currentEpoch?: number;
  totalEpochs?: number;
  accuracy?: number;
  loss?: number;
  message: string;
}

export type TrainingProgressCallback = (progress: TrainingProgress) => void;

/**
 * Monitor GPU memory usage and log warnings if approaching limits.
 */
function monitorGPUMemory(): void {
  const memInfo = tf.memory();
  const numTensors = memInfo.numTensors;
  const numBytes = memInfo.numBytes;
  const numBytesInGPU = memInfo.numBytesInGPU || 0;
  const numDataBuffers = memInfo.numDataBuffers;
  
  // Log if memory is high
  if (numBytesInGPU > 1024 * 1024 * 1024) { // > 1GB
    console.warn(`High GPU memory usage: ${(numBytesInGPU / (1024 * 1024)).toFixed(2)}MB`);
    console.warn(`Tensors: ${numTensors}, DataBuffers: ${numDataBuffers}`);
  }
  
  // Log if too many tensors (potential memory leak)
  if (numTensors > 100) {
    console.warn(`Large number of tensors in memory: ${numTensors}`);
  }
}

/**
 * Setup WebGL context loss recovery.
 */
function setupWebGLContextRecovery(canvas: HTMLCanvasElement): void {
  canvas.addEventListener('webglcontextlost', (event) => {
    console.error('WebGL context lost! Training will fail.');
    event.preventDefault();
  }, false);

  canvas.addEventListener('webglcontextrestored', () => {
    console.log('WebGL context restored');
  }, false);
}

/**
 * Check training capabilities of the device.
 */
export async function checkTrainingCapabilities(): Promise<TrainingCapabilities> {
  // Configure TensorFlow.js for better memory management
  try {
    await tf.setBackend('webgl');
    await tf.ready();
    
    // Configure WebGL backend for better memory efficiency
    const webglBackend = tf.backend() as any;
    if (webglBackend && webglBackend.numMBBeforeWarning !== undefined) {
      // Increase warning threshold to 1.5GB (default is 1GB)
      webglBackend.numMBBeforeWarning = 1536;
    }
  } catch (error) {
    console.error('Failed to initialize WebGL backend:', error);
    // Fallback to CPU if WebGL fails
    await tf.setBackend('cpu');
    await tf.ready();
  }
  
  const canvas = document.createElement('canvas');
  const gl2 = canvas.getContext('webgl2');
  const backend = tf.getBackend();
  
  // Setup WebGL context loss detection
  if (gl2) {
    setupWebGLContextRecovery(canvas);
  }
  
  // Estimate available memory (rough approximation)
  const performance = (window.performance as unknown) as { memory?: { jsHeapSizeLimit?: number } };
  const memoryMB = performance.memory?.jsHeapSizeLimit 
    ? Math.floor(performance.memory.jsHeapSizeLimit / (1024 * 1024))
    : 2048; // Default to 2GB if unavailable

  const hasGPU = backend === 'webgl' && !!gl2;
  const estimatedDuration = hasGPU ? '15-20 minutos' : '45-60 minutos';

  return {
    hasGPU,
    hasWebGL2: !!gl2,
    memoryMB,
    backend,
    estimatedDuration,
  };
}

/**
 * Load pre-trained MobileNetV2 base model from TensorFlow Hub.
 * Returns a GraphModel for feature extraction.
 */
async function loadMobileNetV2(onProgress: TrainingProgressCallback): Promise<tf.GraphModel> {
  onProgress({
    stage: 'loading_model',
    progress: 10,
    message: 'Cargando MobileNetV2 desde TensorFlow Hub...',
  });

  try {
    // Load MobileNetV2 feature extractor (without top classification layer)
    // This returns feature vectors of shape [batch, 1280]
    const model = await tf.loadGraphModel(MOBILENET_URL, { fromTFHub: true });
    
    onProgress({
      stage: 'loading_model',
      progress: 20,
      message: 'Modelo base cargado correctamente',
    });

    return model;
  } catch (error) {
    console.error('Failed to load MobileNetV2:', error);
    throw new Error('Error al cargar el modelo base MobileNetV2');
  }
}

/**
 * Extract features from images using MobileNetV2.
 * Converts raw images to 1280-dimensional feature vectors.
 * Processes in small batches with aggressive memory cleanup to prevent GPU exhaustion.
 */
async function extractFeatures(
  featureExtractor: tf.GraphModel,
  images: tf.Tensor4D,
  onProgress: TrainingProgressCallback
): Promise<tf.Tensor2D> {
  onProgress({
    stage: 'augmenting',
    progress: 32,
    message: 'Extrayendo características con MobileNetV2...',
  });

  // Process in smaller batches to avoid memory issues
  const batchSize = 4; // Reduced from 8 to 4 for better memory management
  const numImages = images.shape[0];
  const featureArrays: tf.Tensor2D[] = [];

  for (let i = 0; i < numImages; i += batchSize) {
    const end = Math.min(i + batchSize, numImages);
    
    // Use tf.tidy to automatically clean up intermediate tensors
    const reshapedFeatures = await tf.tidy(() => {
      const batch = images.slice([i, 0, 0, 0], [end - i, -1, -1, -1]);
      
      // Run feature extraction
      const features = featureExtractor.predict(batch) as tf.Tensor;
      
      // MobileNetV2 from TF Hub outputs [batch, 1, 1, 1280], reshape to [batch, 1280]
      return features.reshape([end - i, -1]) as tf.Tensor2D;
    });
    
    featureArrays.push(reshapedFeatures);
    
    // Force WebGL to complete operations and free memory
    await tf.nextFrame();
    
    // Update progress
    const progress = 32 + Math.floor(((i + batchSize) / numImages) * 5);
    onProgress({
      stage: 'augmenting',
      progress: Math.min(progress, 37),
      message: `Extrayendo características: ${Math.min(end, numImages)}/${numImages}`,
    });
  }

  // Concatenate all feature batches
  const allFeatures = tf.concat(featureArrays, 0) as tf.Tensor2D;
  
  // Clean up intermediate tensors
  featureArrays.forEach(t => t.dispose());
  
  return allFeatures;
}

/**
 * Integrated function to download, preprocess, and extract features in batches.
 * This is more memory-efficient than loading all images first.
 */
async function downloadPreprocessAndExtractFeatures(
  dataset: TrainingDataset,
  featureExtractor: tf.GraphModel,
  onProgress: TrainingProgressCallback
): Promise<{ features: tf.Tensor2D; labels: tf.Tensor2D; classLabels: string[] }> {
  onProgress({
    stage: 'downloading',
    progress: 0,
    message: `Procesando ${dataset.totalPhotos} fotos...`,
  });

  const { photos, classLabels } = dataset;

  // Get API base URL from environment
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5056/api';
  const backendBaseUrl = apiBaseUrl.replace('/api', '');

  // Create class label (SKU) to index mapping
  const classToIndex = new Map(classLabels.map((label, index) => [label, index]));

  // Process images in small batches: load -> extract features -> dispose images
  const PROCESS_BATCH_SIZE = 8; // Process 8 images at a time
  const featureArrays: tf.Tensor2D[] = [];
  const labelIndices: number[] = [];

  for (let batchStart = 0; batchStart < photos.length; batchStart += PROCESS_BATCH_SIZE) {
    const batchEnd = Math.min(batchStart + PROCESS_BATCH_SIZE, photos.length);
    const batchPhotos = photos.slice(batchStart, batchEnd);

    // Load batch of images
    const batchPromises = batchPhotos.map(async (photo) => {
      const labelIndex = classToIndex.get(photo.productSku);
      
      if (labelIndex === undefined) {
        console.warn(`Unknown class label for SKU: ${photo.productSku} (${photo.productName})`);
        return null;
      }

      const absolutePhotoUrl = photo.photoUrl.startsWith('http') 
        ? photo.photoUrl 
        : `${backendBaseUrl}${photo.photoUrl}`;

      try {
        const tensor = await loadAndPreprocessImage(absolutePhotoUrl);
        return { img: tensor, labelIndex };
      } catch (error) {
        console.error(`Failed to load image ${absolutePhotoUrl}:`, error);
        return null;
      }
    });

    const batchResults = await Promise.all(batchPromises);
    const validBatchResults = batchResults.filter((res): res is { img: tf.Tensor3D; labelIndex: number } => res !== null);

    if (validBatchResults.length > 0) {
      // Stack batch images and extract features immediately
      const batchImages = tf.stack(validBatchResults.map(r => r.img)) as tf.Tensor4D;
      
      // Extract features from this batch
      const batchFeatures = await tf.tidy(() => {
        const features = featureExtractor.predict(batchImages) as tf.Tensor;
        // MobileNetV2 from TF Hub outputs [batch, 1, 1, 1280], reshape to [batch, 1280]
        return features.reshape([validBatchResults.length, -1]) as tf.Tensor2D;
      });
      
      featureArrays.push(batchFeatures);
      
      // Store label indices for successfully processed images
      validBatchResults.forEach(r => labelIndices.push(r.labelIndex));
      
      // Dispose batch images and individual tensors immediately
      batchImages.dispose();
      validBatchResults.forEach(r => r.img.dispose());
      
      // Force GPU cleanup between batches
      await tf.nextFrame();
    }

    // Update progress
    const progress = Math.floor((batchEnd / photos.length) * 35);
    onProgress({
      stage: 'downloading',
      progress,
      message: `Procesadas ${batchEnd}/${photos.length} fotos (${progress}%)`,
    });

    // Monitor memory periodically
    if (batchStart % 40 === 0) {
      monitorGPUMemory();
    }
  }

  if (featureArrays.length === 0) {
    throw new Error('No se pudieron procesar imágenes para entrenamiento');
  }

  onProgress({
    stage: 'augmenting',
    progress: 37,
    message: `Consolidando características...`,
  });

  // Concatenate all feature batches
  const allFeatures = tf.concat(featureArrays, 0) as tf.Tensor2D;
  
  // Clean up intermediate feature tensors
  featureArrays.forEach(t => t.dispose());

  // Create one-hot encoded labels
  const labelsTensor = tf.oneHot(labelIndices, classLabels.length) as tf.Tensor2D;

  return {
    features: allFeatures,
    labels: labelsTensor,
    classLabels,
  };
}

/**
 * DEPRECATED: Use downloadPreprocessAndExtractFeatures instead.
 * Download and preprocess product photos for training.
 * Uses batched loading to prevent GPU memory exhaustion.
 */
async function downloadAndPreprocessPhotos(
  dataset: TrainingDataset,
  onProgress: TrainingProgressCallback
): Promise<{ images: tf.Tensor4D; labels: tf.Tensor2D; classLabels: string[] }> {
  onProgress({
    stage: 'downloading',
    progress: 0,
    message: `Descargando ${dataset.totalPhotos} fotos...`,
  });

  const { photos, classLabels } = dataset;

  // Get API base URL from environment
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5056/api';
  // Remove '/api' suffix if present since photo URLs already include it
  const backendBaseUrl = apiBaseUrl.replace('/api', '');

  // Create class label (SKU) to index mapping
  // Class labels are now SKUs (immutable, unique) instead of product names
  const classToIndex = new Map(classLabels.map((label, index) => [label, index]));

  // Process images in batches to prevent GPU memory exhaustion
  const LOAD_BATCH_SIZE = 10; // Load max 10 images at a time
  const validResults: { img: tf.Tensor3D; labelIndex: number }[] = [];

  for (let batchStart = 0; batchStart < photos.length; batchStart += LOAD_BATCH_SIZE) {
    const batchEnd = Math.min(batchStart + LOAD_BATCH_SIZE, photos.length);
    const batchPhotos = photos.slice(batchStart, batchEnd);

    // Load batch of images
    const batchPromises = batchPhotos.map(async (photo) => {
      const labelIndex = classToIndex.get(photo.productSku);
      
      if (labelIndex === undefined) {
        console.warn(`Unknown class label for SKU: ${photo.productSku} (${photo.productName})`);
        return null;
      }

      // Construct absolute URL for image loading
      const absolutePhotoUrl = photo.photoUrl.startsWith('http') 
        ? photo.photoUrl 
        : `${backendBaseUrl}${photo.photoUrl}`;

      try {
        const tensor = await loadAndPreprocessImage(absolutePhotoUrl);
        return { img: tensor, labelIndex };
      } catch (error) {
        console.error(`Failed to load image ${absolutePhotoUrl}:`, error);
        return null;
      }
    });

    const batchResults = await Promise.all(batchPromises);
    const validBatchResults = batchResults.filter((res): res is { img: tf.Tensor3D; labelIndex: number } => res !== null);
    validResults.push(...validBatchResults);

    // Update progress
    const progress = Math.floor((batchEnd / photos.length) * 30);
    onProgress({
      stage: 'downloading',
      progress,
      message: `Descargadas ${batchEnd}/${photos.length} fotos (${progress}%)`,
    });

    // Force garbage collection between batches
    await tf.nextFrame();
  }

  if (validResults.length === 0) {
    throw new Error('No se pudieron cargar imágenes para entrenamiento');
  }

  onProgress({
    stage: 'downloading',
    progress: 30,
    message: `Fotos descargadas: ${validResults.length}/${photos.length}`,
  });

  // Stack images into 4D tensor [numImages, height, width, channels]
  const imagesTensor = tf.stack(validResults.map((r) => r.img)) as tf.Tensor4D;
  
  // Clean up individual tensors
  validResults.forEach((r) => r.img.dispose());

  // Create one-hot encoded labels aligned with successfully loaded images
  const labelsTensor = tf.oneHot(validResults.map((r) => r.labelIndex), classLabels.length);

  return {
    images: imagesTensor,
    labels: labelsTensor as tf.Tensor2D,
    classLabels,
  };
}

/**
 * Load and preprocess a single image from URL.
 */
async function loadAndPreprocessImage(url: string): Promise<tf.Tensor3D> {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.crossOrigin = 'anonymous';
    
    img.onload = () => {
      try {
        const tensor = tf.tidy(() => {
          // Convert to tensor
          let imgTensor = tf.browser.fromPixels(img);
          
          // Resize to model input size
          imgTensor = tf.image.resizeBilinear(imgTensor, [IMAGE_SIZE, IMAGE_SIZE]);
          
          // Normalize to [-1, 1] for MobileNetV2
          imgTensor = imgTensor.toFloat().div(127.5).sub(1);
          
          return imgTensor as tf.Tensor3D;
        });
        resolve(tensor);
      } catch (error) {
        reject(error);
      }
    };
    
    img.onerror = () => reject(new Error(`Failed to load image: ${url}`));
    img.src = url;
  });
}

/**
 * Build classification head model for MobileNetV2 features.
 * Takes 1280-dimensional feature vectors as input.
 */
function buildClassificationHead(numClasses: number, featureDim: number = 1280): tf.LayersModel {
  const model = tf.sequential();

  // Input layer for feature vectors
  model.add(tf.layers.inputLayer({ inputShape: [featureDim] }));

  // Classification layers with regularization to prevent overfitting
  model.add(tf.layers.dense({
    units: 256,
    activation: 'relu',
    kernelRegularizer: tf.regularizers.l2({ l2: 0.01 }),
  }));
  
  model.add(tf.layers.batchNormalization({}));
  model.add(tf.layers.dropout({ rate: 0.5 }));
  
  model.add(tf.layers.dense({
    units: 128,
    activation: 'relu',
    kernelRegularizer: tf.regularizers.l2({ l2: 0.01 }),
  }));
  
  model.add(tf.layers.dropout({ rate: 0.3 }));
  
  model.add(tf.layers.dense({
    units: numClasses,
    activation: 'softmax',
  }));

  // Compile model with a lower learning rate for better generalization
  model.compile({
    optimizer: tf.train.adam(0.0005),
    loss: 'categoricalCrossentropy',
    metrics: ['accuracy'],
  });

  return model;
}

/**
 * Build a complete inference model that includes feature extraction.
 * 
 * Note: Since we can't directly combine GraphModel with LayersModel,
 * we save just the classification head. The inference service will:
 * 1. Load MobileNetV2 separately (cached, same for all models)
 * 2. Extract features from the input image
 * 3. Run the classification head on the features
 * 
 * The classification head is already properly configured, so we just return it.
 */
async function buildInferenceModel(
  classificationHead: tf.LayersModel,
  _featureDim: number,
  _numClasses: number
): Promise<tf.LayersModel> {
  // The classification head is already a valid LayersModel that expects
  // feature vectors as input. We can save it directly.
  // The inference service will handle feature extraction with MobileNetV2.
  return classificationHead;
}

/**
 * Train the model with progress updates.
 * Now accepts feature vectors (Tensor2D) instead of raw images (Tensor4D).
 */
async function trainModel(
  model: tf.LayersModel,
  features: tf.Tensor2D,
  labels: tf.Tensor2D,
  onProgress: TrainingProgressCallback
): Promise<{ accuracy: number; loss: number }> {
  onProgress({
    stage: 'training',
    progress: 40,
    totalEpochs: EPOCHS,
    currentEpoch: 0,
    message: 'Iniciando entrenamiento...',
  });

  let finalAccuracy = 0;
  let finalLoss = 0;

  await model.fit(features, labels, {
    epochs: EPOCHS,
    batchSize: BATCH_SIZE,
    validationSplit: VALIDATION_SPLIT,
    shuffle: true,
    callbacks: {
      onEpochEnd: (epoch, logs) => {
        const progress = 40 + Math.floor(((epoch + 1) / EPOCHS) * 50);
        finalAccuracy = (logs?.val_acc || logs?.acc || 0) * 100;
        finalLoss = logs?.val_loss || logs?.loss || 0;
        
        onProgress({
          stage: 'training',
          progress,
          currentEpoch: epoch + 1,
          totalEpochs: EPOCHS,
          accuracy: finalAccuracy,
          loss: finalLoss,
          message: `Época ${epoch + 1}/${EPOCHS} - Precisión: ${finalAccuracy.toFixed(1)}% - Pérdida: ${finalLoss.toFixed(4)}`,
        });
      },
    },
  });

  return { accuracy: finalAccuracy, loss: finalLoss };
}

/**
 * Export trained model to TensorFlow.js format and upload to server.
 * IMPORTANT: Also saves class_labels.json to ensure inference uses the same labels.
 */
async function exportAndUploadModel(
  model: tf.LayersModel,
  version: string,
  accuracy: number,
  totalPhotos: number,
  totalProducts: number,
  trainingDurationSeconds: number,
  classLabels: string[],
  onProgress: TrainingProgressCallback
): Promise<void> {
  onProgress({
    stage: 'uploading',
    progress: 90,
    message: 'Exportando modelo...',
  });

  // Save model to IndexedDB first (for checkpoint)
  await model.save('indexeddb://trained-model');

  // Export model topology and weights
  let modelArtifacts: tf.io.ModelArtifacts;
  await model.save(tf.io.withSaveHandler(async (artifacts: tf.io.ModelArtifacts) => {
    modelArtifacts = artifacts;
    return { modelArtifactsInfo: { dateSaved: new Date(), modelTopologyType: 'JSON' } };
  }));

  if (!modelArtifacts! || !modelArtifacts!.modelTopology || !modelArtifacts!.weightData) {
    throw new Error('Error al exportar el modelo');
  }

  onProgress({
    stage: 'uploading',
    progress: 92,
    message: 'Preparando archivos para subir...',
  });

  // Create weight files
  const weightSpecs = modelArtifacts.weightSpecs || [];
  const weightData = modelArtifacts.weightData;

  // Build a valid TensorFlow.js LayersModel JSON file.
  // Note: backend field is called modelTopologyJson, but we send the full model.json contents.
  const modelJson = JSON.stringify({
    format: 'layers-model',
    generatedBy: 'tensorflowjs',
    convertedBy: null,
    modelTopology: modelArtifacts.modelTopology,
    weightsManifest: [
      {
        paths: ['weights.bin'],
        weights: weightSpecs,
      },
    ],
  });

  // Split weight data into files (simplified - single file for now)
  // weightData can be ArrayBuffer or ArrayBuffer[] depending on TFJS internals; normalize to BlobPart[]
  const weightParts: BlobPart[] = Array.isArray(weightData) ? weightData : [weightData];
  const weightBlob = new Blob(weightParts, { type: 'application/octet-stream' });
  const weightFile = new File([weightBlob], 'weights.bin', { type: 'application/octet-stream' });

  // Create weight manifest file so the server can map binary weights to specs
  const weightManifestJson = JSON.stringify(weightSpecs);
  const manifestBlob = new Blob([weightManifestJson], { type: 'application/json' });
  const manifestFile = new File([manifestBlob], 'weights_manifest.json', { type: 'application/json' });

  // CRITICAL: Save class labels used during training
  // This ensures inference uses the exact same label order as training
  const classLabelsJson = JSON.stringify({
    labels: classLabels,
    trainedAt: new Date().toISOString(),
    version: version,
  });
  const classLabelsBlob = new Blob([classLabelsJson], { type: 'application/json' });
  const classLabelsFile = new File([classLabelsBlob], 'class_labels.json', { type: 'application/json' });

  onProgress({
    stage: 'uploading',
    progress: 95,
    message: 'Subiendo modelo al servidor...',
  });

  // Upload to server (include class_labels.json with weight files)
  await imageRecognitionService.uploadTrainedModel(
    version,
    modelJson,
    accuracy, // Training accuracy (same as validation for simplicity)
    accuracy, // Validation accuracy
    totalPhotos,
    totalProducts,
    trainingDurationSeconds,
    [weightFile, manifestFile, classLabelsFile]
  );

  onProgress({
    stage: 'complete',
    progress: 100,
    message: 'Modelo entrenado y subido correctamente',
  });
}

/**
 * Main orchestrator: Execute complete client-side training workflow.
 */
export async function executeClientSideTraining(
  onProgress: TrainingProgressCallback
): Promise<void> {
  const startTime = Date.now();

  try {
    // Step 1: Check capabilities
    const capabilities = await checkTrainingCapabilities();
    console.log('Training capabilities:', capabilities);

    // Warn if low memory
    if (capabilities.memoryMB < 4096) {
      console.warn(`Low memory detected (${capabilities.memoryMB}MB). Training may be slow or fail.`);
    }

    // Step 2: Get training dataset
    onProgress({
      stage: 'downloading',
      progress: 0,
      message: 'Obteniendo conjunto de datos...',
    });

    const dataset = await imageRecognitionService.getTrainingDataset();

    if (dataset.totalPhotos === 0) {
      throw new Error('No hay fotos disponibles para entrenamiento');
    }

    // Limit dataset size to prevent memory issues
    if (dataset.totalPhotos > 300) {
      console.warn(`Large dataset detected (${dataset.totalPhotos} photos). Training may consume significant memory.`);
    }

    // Step 3: Load MobileNetV2 feature extractor
    const featureExtractor = await loadMobileNetV2(onProgress);

    // Step 4 & 5: Process images and extract features in integrated batches
    // This prevents keeping all raw images in memory at once
    const { features, labels, classLabels } = await downloadPreprocessAndExtractFeatures(
      dataset, 
      featureExtractor, 
      onProgress
    );
    
    // Clean up feature extractor early (we don't need it anymore)
    featureExtractor.dispose();
    
    // Force memory cleanup
    await tf.nextFrame();
    
    // Monitor memory after feature extraction
    monitorGPUMemory();
    
    // Get the feature dimension from extracted features
    const featureDim = features.shape[1];
    console.log(`Extracted ${features.shape[0]} feature vectors of dimension ${featureDim}`);

    // Step 6: Build classification head
    onProgress({
      stage: 'loading_model',
      progress: 38,
      message: 'Construyendo modelo de clasificación...',
    });

    const classificationHead = buildClassificationHead(classLabels.length, featureDim);

    // Step 7: Train classification head on features
    const { accuracy } = await trainModel(classificationHead, features, labels, onProgress);
    
    // Monitor memory after training
    monitorGPUMemory();

    // Step 8: Build final combined model for inference
    // Create a model that takes images as input and outputs predictions
    onProgress({
      stage: 'uploading',
      progress: 88,
      message: 'Construyendo modelo final para inferencia...',
    });

    const model = await buildInferenceModel(classificationHead, featureDim, classLabels.length);

    // Step 7: Export and upload
    const trainingDurationSeconds = Math.floor((Date.now() - startTime) / 1000);
    const version = `v${Date.now()}_${new Date().toISOString().split('T')[0].replace(/-/g, '')}`;

    await exportAndUploadModel(
      model,
      version,
      accuracy,
      dataset.totalPhotos,
      dataset.totalProducts,
      trainingDurationSeconds,
      classLabels, // Pass the class labels used during training
      onProgress
    );

    // Cleanup tensors
    features.dispose();
    labels.dispose();
    // model and classificationHead are the same object now, dispose once
    model.dispose();

    // Final cleanup
    await tf.nextFrame();

    console.log('Training complete!');
  } catch (error) {
    console.error('Training error:', error);
    
    // Attempt cleanup on error
    try {
      const memInfo = tf.memory();
      if (memInfo.numTensors > 0) {
        console.warn(`Memory leak detected: ${memInfo.numTensors} tensors not disposed`);
        console.warn(`GPU Memory: ${(memInfo.numBytesInGPU || 0) / (1024 * 1024)}MB`);
      }
    } catch (e) {
      // Ignore cleanup errors
    }
    
    // Check if error is due to WebGL context loss
    if (error instanceof Error) {
      if (error.message.includes('WebGL') || error.message.includes('context') || error.message.includes('shader')) {
        throw new Error('GPU memory exhausted. Please try: 1) Close other tabs, 2) Refresh the page, 3) Use fewer training images.');
      }
    }
    
    throw error;
  }
}

export const modelTrainingService = {
  checkTrainingCapabilities,
  executeClientSideTraining,
};
