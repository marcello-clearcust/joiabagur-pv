/**
 * Client-Side Model Training Service (EP4 - Section 13B)
 * Browser-based TensorFlow.js model training (NO Python required).
 */

import * as tf from '@tensorflow/tfjs';
import { imageRecognitionService } from './sales.service';
import type { TrainingDataset } from '@/types/sales.types';

const MOBILENET_URL = 'https://tfhub.dev/google/tfjs-model/imagenet/mobilenet_v2_100_224/feature_vector/3/default/1';
const IMAGE_SIZE = 224;
const EPOCHS = 15;
const BATCH_SIZE = 32;
const VALIDATION_SPLIT = 0.2;

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
 * Check training capabilities of the device.
 */
export async function checkTrainingCapabilities(): Promise<TrainingCapabilities> {
  await tf.ready();
  
  const canvas = document.createElement('canvas');
  const gl2 = canvas.getContext('webgl2');
  const backend = tf.getBackend();
  
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

  // Process in batches to avoid memory issues
  const batchSize = 8;
  const numImages = images.shape[0];
  const featureArrays: tf.Tensor2D[] = [];

  for (let i = 0; i < numImages; i += batchSize) {
    const end = Math.min(i + batchSize, numImages);
    const batch = images.slice([i, 0, 0, 0], [end - i, -1, -1, -1]);
    
    // Run feature extraction
    const features = featureExtractor.predict(batch) as tf.Tensor;
    
    // MobileNetV2 from TF Hub outputs [batch, 1, 1, 1280], reshape to [batch, 1280]
    const reshapedFeatures = features.reshape([end - i, -1]) as tf.Tensor2D;
    featureArrays.push(reshapedFeatures);
    
    // Clean up batch tensor
    batch.dispose();
    features.dispose();
    
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
 * Download and preprocess product photos for training.
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
  const imagePromises: Promise<{ img: tf.Tensor3D; labelIndex: number } | null>[] = [];
  const labelIndices: number[] = [];

  // Get API base URL from environment
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5056/api';
  // Remove '/api' suffix if present since photo URLs already include it
  const backendBaseUrl = apiBaseUrl.replace('/api', '');

  // Create class label (SKU) to index mapping
  // Class labels are now SKUs (immutable, unique) instead of product names
  const classToIndex = new Map(classLabels.map((label, index) => [label, index]));

  for (let i = 0; i < photos.length; i++) {
    const photo = photos[i];
    // Use productSku for mapping (matches class labels from backend)
    const labelIndex = classToIndex.get(photo.productSku);
    
    if (labelIndex === undefined) {
      console.warn(`Unknown class label for SKU: ${photo.productSku} (${photo.productName})`);
      continue;
    }

    labelIndices.push(labelIndex);

    // Construct absolute URL for image loading
    // Backend returns relative URLs like "/api/files/products/filename.jpg"
    // We need to prepend the backend base URL
    const absolutePhotoUrl = photo.photoUrl.startsWith('http') 
      ? photo.photoUrl 
      : `${backendBaseUrl}${photo.photoUrl}`;

    // Load and preprocess image, preserving label association
    imagePromises.push(
      loadAndPreprocessImage(absolutePhotoUrl)
        .then((tensor) => ({ img: tensor, labelIndex }))
        .catch((error) => {
          console.error(`Failed to load image ${absolutePhotoUrl}:`, error);
          return null;
        })
    );

    // Update progress
    if (i % 10 === 0) {
      const progress = Math.floor((i / photos.length) * 30);
      onProgress({
        stage: 'downloading',
        progress,
        message: `Descargadas ${i}/${photos.length} fotos (${progress}%)`,
      });
    }
  }

  const loadedResults = await Promise.all(imagePromises);
  const validResults = loadedResults.filter((res): res is { img: tf.Tensor3D; labelIndex: number } => res !== null);

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
 */
async function trainModel(
  model: tf.LayersModel,
  images: tf.Tensor4D,
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

  await model.fit(images, labels, {
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

    // Step 3: Load MobileNetV2 feature extractor
    const featureExtractor = await loadMobileNetV2(onProgress);

    // Step 4: Download and preprocess photos
    const { images, labels, classLabels } = await downloadAndPreprocessPhotos(dataset, onProgress);

    // Step 5: Extract features using MobileNetV2
    // This is the key step - we use pretrained features instead of raw pixels
    const features = await extractFeatures(featureExtractor, images, onProgress);
    
    // We can dispose the raw images now, we only need features
    images.dispose();
    
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
    const { accuracy } = await trainModel(classificationHead, features as unknown as tf.Tensor4D, labels, onProgress);
    
    // Clean up feature extractor (we don't need it anymore)
    featureExtractor.dispose();

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

    console.log('Training complete!');
  } catch (error) {
    console.error('Training error:', error);
    throw error;
  }
}

export const modelTrainingService = {
  checkTrainingCapabilities,
  executeClientSideTraining,
};
