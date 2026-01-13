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
 */
async function loadMobileNetV2(onProgress: TrainingProgressCallback): Promise<tf.LayersModel> {
  onProgress({
    stage: 'loading_model',
    progress: 10,
    message: 'Cargando MobileNetV2 desde TensorFlow Hub...',
  });

  try {
    // Load MobileNetV2 feature extractor (without top classification layer)
    const model = await tf.loadGraphModel(MOBILENET_URL, { fromTFHub: true });
    
    onProgress({
      stage: 'loading_model',
      progress: 20,
      message: 'Modelo base cargado correctamente',
    });

    return model as unknown as tf.LayersModel;
  } catch (error) {
    console.error('Failed to load MobileNetV2:', error);
    throw new Error('Error al cargar el modelo base MobileNetV2');
  }
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
  const imagePromises: Promise<tf.Tensor3D>[] = [];
  const labelIndices: number[] = [];

  // Create class label to index mapping
  const classToIndex = new Map(classLabels.map((label, index) => [label, index]));

  for (let i = 0; i < photos.length; i++) {
    const photo = photos[i];
    const labelIndex = classToIndex.get(photo.productName);
    
    if (labelIndex === undefined) {
      console.warn(`Unknown class label: ${photo.productName}`);
      continue;
    }

    labelIndices.push(labelIndex);

    // Load and preprocess image
    imagePromises.push(
      loadAndPreprocessImage(photo.photoUrl).catch((error) => {
        console.error(`Failed to load image ${photo.photoUrl}:`, error);
        return null;
      }) as Promise<tf.Tensor3D>
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

  const loadedImages = await Promise.all(imagePromises);
  const validImages = loadedImages.filter((img): img is tf.Tensor3D => img !== null);

  if (validImages.length === 0) {
    throw new Error('No se pudieron cargar imágenes para entrenamiento');
  }

  onProgress({
    stage: 'downloading',
    progress: 30,
    message: `Fotos descargadas: ${validImages.length}/${photos.length}`,
  });

  // Stack images into 4D tensor [numImages, height, width, channels]
  const imagesTensor = tf.stack(validImages) as tf.Tensor4D;
  
  // Clean up individual tensors
  validImages.forEach((img) => img.dispose());

  // Create one-hot encoded labels
  const labelsTensor = tf.oneHot(labelIndices.slice(0, validImages.length), classLabels.length);

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
 * Build classification model on top of MobileNetV2.
 */
function buildClassificationModel(baseModel: tf.LayersModel, numClasses: number): tf.LayersModel {
  // Create sequential model
  const model = tf.sequential();

  // Add MobileNetV2 feature extractor (frozen)
  model.add(baseModel);

  // Add classification layers
  model.add(tf.layers.dense({
    units: 128,
    activation: 'relu',
    kernelRegularizer: tf.regularizers.l2({ l2: 0.01 }),
  }));
  
  model.add(tf.layers.dropout({ rate: 0.5 }));
  
  model.add(tf.layers.dense({
    units: numClasses,
    activation: 'softmax',
  }));

  // Compile model
  model.compile({
    optimizer: tf.train.adam(0.001),
    loss: 'categoricalCrossentropy',
    metrics: ['accuracy'],
  });

  return model;
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
 */
async function exportAndUploadModel(
  model: tf.LayersModel,
  version: string,
  accuracy: number,
  totalPhotos: number,
  totalProducts: number,
  trainingDurationSeconds: number,
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
  const modelArtifacts = await model.save(tf.io.withSaveHandler(async (artifacts) => artifacts));

  if (!modelArtifacts.modelTopology || !modelArtifacts.weightData) {
    throw new Error('Error al exportar el modelo');
  }

  onProgress({
    stage: 'uploading',
    progress: 92,
    message: 'Preparando archivos para subir...',
  });

  // Convert model topology to JSON string
  const modelTopologyJson = JSON.stringify(modelArtifacts.modelTopology);

  // Create weight files
  const weightSpecs = modelArtifacts.weightSpecs || [];
  const weightData = modelArtifacts.weightData;

  // Split weight data into files (simplified - single file for now)
  const weightBlob = new Blob([weightData], { type: 'application/octet-stream' });
  const weightFile = new File([weightBlob], 'weights.bin', { type: 'application/octet-stream' });

  onProgress({
    stage: 'uploading',
    progress: 95,
    message: 'Subiendo modelo al servidor...',
  });

  // Upload to server
  await imageRecognitionService.uploadTrainedModel(
    version,
    modelTopologyJson,
    accuracy, // Training accuracy (same as validation for simplicity)
    accuracy, // Validation accuracy
    totalPhotos,
    totalProducts,
    trainingDurationSeconds,
    [weightFile]
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

    // Step 3: Load MobileNetV2 base model
    const baseModel = await loadMobileNetV2(onProgress);

    // Step 4: Download and preprocess photos
    const { images, labels, classLabels } = await downloadAndPreprocessPhotos(dataset, onProgress);

    // Step 5: Build classification model
    onProgress({
      stage: 'loading_model',
      progress: 35,
      message: 'Construyendo modelo de clasificación...',
    });

    const model = buildClassificationModel(baseModel, classLabels.length);

    // Step 6: Train model
    const { accuracy } = await trainModel(model, images, labels, onProgress);

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
      onProgress
    );

    // Cleanup tensors
    images.dispose();
    labels.dispose();
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
