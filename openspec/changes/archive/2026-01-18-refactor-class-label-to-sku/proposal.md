# Change: Refactor ML Class Labels from ProductName to ProductSku

## Why

Currently, the image recognition model uses `ProductName` as the class label identifier during training and inference. This creates fragility because:
1. **Names are mutable**: Administrators can edit product names after training, breaking the model-to-product mapping
2. **Names can be similar**: Products like "Pan de Chocolate" and "Pan de Chocolate Grande" could cause collisions
3. **Names are not guaranteed unique**: Nothing prevents duplicate names in the database

SKUs are immutable, unique, and stable - making them ideal class identifiers.

## What Changes

- **MODIFIED** Backend `GetTrainingDatasetAsync` to use `ProductSku` as class labels instead of `ProductName`
- **MODIFIED** Backend `GetClassLabels` to use `ProductSku` as dictionary key for product mappings
- **MODIFIED** Frontend `downloadAndPreprocessPhotos` to map photos by `productSku` instead of `productName`
- **MODIFIED** Frontend `generateSuggestions` and `enrichSuggestionsFromMappings` to use SKU for matching
- **ADDED** Enhanced logging that includes both SKU and ProductName for developer readability

**Note**: This is a non-breaking change for end users, but **BREAKING** for existing trained models - retraining will be required after deployment.

## Impact

- Affected specs: `image-recognition`
- Affected code:
  - `backend/src/JoiabagurPV.Application/Services/ImageRecognitionService.cs`
  - `backend/src/JoiabagurPV.API/Controllers/ImageRecognitionController.cs`
  - `frontend/src/services/model-training.service.ts`
  - `frontend/src/services/image-recognition.service.ts`
- **Migration**: Any existing trained model will be incompatible after this change. Model retraining is required.
