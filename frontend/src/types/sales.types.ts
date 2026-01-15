/**
 * Sales Module Types (EP3)
 */

/**
 * Request to create a new sale.
 */
export interface CreateSaleRequest {
  productId: string;
  pointOfSaleId: string;
  paymentMethodId: string;
  quantity: number;
  notes?: string;
  photoBase64?: string;
  photoFileName?: string;
}

/**
 * Response from creating a sale.
 */
export interface CreateSaleResponse {
  sale: Sale;
  warning?: string;
  isLowStock: boolean;
  remainingStock?: number;
}

/**
 * Sale details.
 */
export interface Sale {
  id: string;
  productId: string;
  productSku: string;
  productName: string;
  pointOfSaleId: string;
  pointOfSaleName: string;
  userId: string;
  userName: string;
  paymentMethodId: string;
  paymentMethodName: string;
  price: number;
  quantity: number;
  total: number;
  notes?: string;
  hasPhoto: boolean;
  saleDate: string;
  createdAt: string;
}

/**
 * Request for sales history with filters.
 */
export interface SalesHistoryFilterRequest {
  startDate?: string;
  endDate?: string;
  pointOfSaleId?: string;
  productId?: string;
  userId?: string;
  paymentMethodId?: string;
  page?: number;
  pageSize?: number;
}

/**
 * Paginated sales history response.
 */
export interface SalesHistoryResponse {
  sales: Sale[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Image Recognition Model Metadata.
 */
export interface ModelMetadata {
  version: string;
  trainedAt: string;
  modelPath: string;
  accuracyMetrics?: string;
  totalPhotosUsed: number;
  totalProductsUsed: number;
  isActive: boolean;
}

/**
 * Model health metrics for admin dashboard.
 */
export interface ModelHealth {
  alertLevel: 'OK' | 'RECOMMENDED' | 'HIGH' | 'CRITICAL';
  alertMessage: string;
  currentVersion?: string;
  lastTrainedAt?: string;
  daysSinceTraining?: number;
  catalogMetrics: {
    totalProducts: number;
    productsWithPhotos: number;
    productsWithoutPhotos: number;
    newProductsSinceTraining: number;
    newProductsPercentage: number;
  };
  photoMetrics: {
    totalPhotos: number;
    photosAddedSinceTraining: number;
    photosDeletedSinceTraining: number;
    netChangePercentage: number;
  };
  precisionMetrics?: {
    top1Accuracy?: number;
    top3Accuracy?: number;
    averageInferenceTimeMs?: number;
    fallbackRate?: number;
  };
}

/**
 * Training dataset for browser-based training.
 */
export interface TrainingDataset {
  photos: TrainingPhoto[];
  totalPhotos: number;
  totalProducts: number;
  classLabels: string[];
}

/**
 * Individual photo in training dataset.
 */
export interface TrainingPhoto {
  productId: string;
  productSku: string;
  productName: string;
  photoId: string;
  photoUrl: string;
}

/**
 * Product label mapping for inference (maps class label to product details).
 */
export interface ProductLabelMapping {
  productId: string;
  productSku: string;
  productName: string;
  photoUrl: string;
}

/**
 * Class labels response for inference (accessible to all authenticated users).
 */
export interface ClassLabelsResponse {
  classLabels: string[];
  productMappings: Record<string, ProductLabelMapping>;
}

/**
 * Request to upload a browser-trained model.
 */
export interface UploadTrainedModelRequest {
  version: string;
  modelTopologyJson: string;
  trainingAccuracy: number;
  validationAccuracy: number;
  totalPhotosUsed: number;
  totalProductsUsed: number;
  trainingDurationSeconds: number;
  weightFiles: File[];
}

/**
 * Result of model upload.
 */
export interface UploadTrainedModelResult {
  success: boolean;
  errorMessage?: string;
  version?: string;
  metadata?: ModelMetadata;
}

/**
 * Product suggestion from image recognition.
 */
export interface ProductSuggestion {
  productId: string;
  productSku: string;
  productName: string;
  confidence: number;
  photoUrl: string;
}
