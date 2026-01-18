## 1. Backend Changes

- [x] 1.1 Modify `GetTrainingDatasetAsync` in `ImageRecognitionService.cs` to use `product.SKU` instead of `product.Name` for class labels
- [x] 1.2 Modify `GetClassLabels` in `ImageRecognitionController.cs` to use `ProductSku` as dictionary key instead of `ProductName`
- [x] 1.3 Update `TrainingDatasetResponse` to clarify that `ClassLabels` now contains SKUs

## 2. Frontend Training Changes

- [x] 2.1 Modify `downloadAndPreprocessPhotos` in `model-training.service.ts` to use `photo.productSku` for label mapping
- [x] 2.2 Ensure class labels exported with model are SKUs

## 3. Frontend Inference Changes

- [x] 3.1 Modify `generateSuggestions` in `image-recognition.service.ts` to use SKU as the primary identifier
- [x] 3.2 Modify `enrichSuggestionsFromMappings` to match by SKU instead of name
- [x] 3.3 Add enhanced logging that shows both SKU and product name for readability

## 4. Testing

- [x] 4.1 Update unit tests for training service to expect SKU-based labels
- [x] 4.2 Update unit tests for inference service to expect SKU-based matching
- [x] 4.3 Perform end-to-end test: train model → run inference → verify suggestions

## 5. Documentation

- [x] 5.1 Update spec comments to reflect SKU usage
- [x] 5.2 Add migration note about required model retraining
