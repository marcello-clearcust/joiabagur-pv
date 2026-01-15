/**
 * Sales Service (EP3)
 * Handles sales registration, history, and image recognition operations.
 */

import apiClient from './api.service';
import type {
  CreateSaleRequest,
  CreateSaleResponse,
  Sale,
  SalesHistoryFilterRequest,
  SalesHistoryResponse,
  ModelMetadata,
  ModelHealth,
  TrainingDataset,
  ClassLabelsResponse,
  UploadTrainedModelResult,
} from '@/types/sales.types';

const SALES_ENDPOINT = '/sales';
const IMAGE_RECOGNITION_ENDPOINT = '/image-recognition';

/**
 * Sales API operations.
 */
export const salesService = {
  /**
   * Creates a new sale.
   */
  createSale: async (request: CreateSaleRequest): Promise<CreateSaleResponse> => {
    const response = await apiClient.post<CreateSaleResponse>(SALES_ENDPOINT, request);
    return response.data;
  },

  /**
   * Gets a sale by ID.
   */
  getSaleById: async (id: string): Promise<Sale> => {
    const response = await apiClient.get<Sale>(`${SALES_ENDPOINT}/${id}`);
    return response.data;
  },

  /**
   * Gets sales history with filters.
   */
  getSalesHistory: async (filters: SalesHistoryFilterRequest): Promise<SalesHistoryResponse> => {
    const params = new URLSearchParams();
    
    if (filters.startDate) params.append('startDate', filters.startDate);
    if (filters.endDate) params.append('endDate', filters.endDate);
    if (filters.pointOfSaleId) params.append('pointOfSaleId', filters.pointOfSaleId);
    if (filters.productId) params.append('productId', filters.productId);
    if (filters.userId) params.append('userId', filters.userId);
    if (filters.paymentMethodId) params.append('paymentMethodId', filters.paymentMethodId);
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());

    const response = await apiClient.get<SalesHistoryResponse>(`${SALES_ENDPOINT}?${params.toString()}`);
    return response.data;
  },

  /**
   * Gets the photo URL for a sale.
   */
  getSalePhotoUrl: (saleId: string): string => {
    return `${apiClient.defaults.baseURL}${SALES_ENDPOINT}/${saleId}/photo/file`;
  },
};

/**
 * Image Recognition API operations.
 */
export const imageRecognitionService = {
  /**
   * Gets the active model metadata.
   */
  getModelMetadata: async (): Promise<ModelMetadata | null> => {
    try {
      const response = await apiClient.get<ModelMetadata>(`${IMAGE_RECOGNITION_ENDPOINT}/model/metadata`);
      return response.data;
    } catch (error: unknown) {
      if ((error as { statusCode?: number }).statusCode === 404) {
        return null; // No model available
      }
      throw error;
    }
  },

  /**
   * Gets model health metrics (admin only).
   */
  getModelHealth: async (): Promise<ModelHealth> => {
    const response = await apiClient.get<ModelHealth>(`${IMAGE_RECOGNITION_ENDPOINT}/model/health`);
    return response.data;
  },

  /**
   * Gets all model versions (training history).
   */
  getModelVersions: async (): Promise<ModelMetadata[]> => {
    const response = await apiClient.get<ModelMetadata[]>(`${IMAGE_RECOGNITION_ENDPOINT}/model/versions`);
    return response.data;
  },

  /**
   * Gets the training dataset for browser-based training (admin only).
   */
  getTrainingDataset: async (): Promise<TrainingDataset> => {
    const response = await apiClient.get<TrainingDataset>(`${IMAGE_RECOGNITION_ENDPOINT}/training-dataset`);
    return response.data;
  },

  /**
   * Gets class labels and product mappings for inference (accessible to all users).
   */
  getClassLabels: async (): Promise<ClassLabelsResponse> => {
    const response = await apiClient.get<ClassLabelsResponse>(`${IMAGE_RECOGNITION_ENDPOINT}/model/class-labels`);
    return response.data;
  },

  /**
   * Uploads a browser-trained model.
   */
  uploadTrainedModel: async (
    version: string,
    modelTopologyJson: string,
    trainingAccuracy: number,
    validationAccuracy: number,
    totalPhotosUsed: number,
    totalProductsUsed: number,
    trainingDurationSeconds: number,
    weightFiles: File[]
  ): Promise<UploadTrainedModelResult> => {
    const formData = new FormData();
    formData.append('version', version);
    formData.append('modelTopologyJson', modelTopologyJson);
    formData.append('trainingAccuracy', trainingAccuracy.toString());
    formData.append('validationAccuracy', validationAccuracy.toString());
    formData.append('totalPhotosUsed', totalPhotosUsed.toString());
    formData.append('totalProductsUsed', totalProductsUsed.toString());
    formData.append('trainingDurationSeconds', trainingDurationSeconds.toString());
    
    weightFiles.forEach(file => {
      formData.append('weightFiles', file);
    });

    const response = await apiClient.post<UploadTrainedModelResult>(
      `${IMAGE_RECOGNITION_ENDPOINT}/upload-trained-model`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  /**
   * Gets the model file URL for downloading.
   */
  getModelUrl: (version: string, fileName: string): string => {
    return `${apiClient.defaults.baseURL}${IMAGE_RECOGNITION_ENDPOINT}/model/files/${version}/${fileName}`;
  },
};

export default salesService;
