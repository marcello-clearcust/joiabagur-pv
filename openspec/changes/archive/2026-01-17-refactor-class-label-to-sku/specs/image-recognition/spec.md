## MODIFIED Requirements

### Requirement: Browser-Based Model Training Capability

The system SHALL support model training executed in administrator's browser using TensorFlow.js, with automatic upload of trained models to storage. No Python installation required.

Class labels SHALL use `ProductSku` (immutable, unique) instead of `ProductName` to ensure stable model-to-product mapping after product name edits.

#### Scenario: Download photos for browser training

- **WHEN** administrator confirms training start
- **THEN** browser fetches product photos from GET /api/products/photos/training-dataset
- **AND** downloads photos in parallel batches (10 photos per batch)
- **AND** displays download progress (X/Y photos - Z MB downloaded)
- **AND** photos are held in memory (not saved to disk)

#### Scenario: Execute browser training

- **WHEN** photos are downloaded successfully
- **THEN** browser loads pre-trained MobileNetV2 from TensorFlow Hub
- **AND** applies data augmentation (rotation ±15°, brightness ±20%, flip, zoom ±10%)
- **AND** adds custom classification head for product classes
- **AND** trains for 15 epochs with validation split 0.2
- **AND** displays training progress (Epoch X/15, Accuracy: Y%, Loss: Z)
- **AND** training uses WebGL GPU acceleration if available (WASM fallback)

#### Scenario: Class labels use SKU for stability

- **WHEN** model training prepares class labels
- **THEN** system uses `ProductSku` as the class label identifier (not ProductName)
- **AND** each class index maps to a unique SKU
- **AND** SKU-to-index mapping is stored with the model for inference
- **AND** product name changes after training do NOT affect model inference

#### Scenario: Upload trained model automatically

- **WHEN** browser training completes successfully
- **THEN** browser saves checkpoint to IndexedDB (backup)
- **AND** browser uploads trained model to POST /api/image-recognition/upload-trained-model
- **AND** displays upload progress (X MB / Y MB uploaded)
- **AND** server validates model format and stores in /ml-models/v{version}/
- **AND** server updates ModelMetadata table (version, uploaded_at, accuracy_metrics)
- **AND** displays success message with model version and accuracy

#### Scenario: Validate uploaded model

- **WHEN** model files are uploaded to server
- **THEN** system validates model.json exists and is valid TensorFlow.js format
- **AND** validates weight data is complete and correct size
- **AND** validates total model size is within limits (<20MB)
- **AND** rejects upload if validation fails with descriptive error

#### Scenario: Keep browser tab open during training

- **WHEN** training is in progress
- **THEN** browser displays prominent warning: "⚠️ Keep this tab open during training"
- **AND** browser shows beforeunload warning if user tries to close tab
- **AND** training progress updates in real-time (no polling)
- **AND** admin cannot navigate away without warning

#### Scenario: Check device capabilities before training

- **WHEN** administrator clicks "Train Model" button
- **THEN** browser checks WebGL 2.0 support (GPU acceleration)
- **AND** browser checks available memory (recommend 8GB+, minimum 2GB)
- **AND** browser checks battery status (warn if on battery)
- **AND** displays estimated training duration (GPU: 15-20min, CPU: 45-60min)
- **AND** shows device capability summary (GPU detected: Yes/No, Memory: XGB)

#### Scenario: Initial deployment without model

- **WHEN** application is deployed for first time
- **AND** no model has been trained yet
- **THEN** GET /api/image-recognition/model returns 404 Not Found
- **AND** GET /api/image-recognition/model/metadata returns empty or null
- **AND** frontend detects no model available
- **AND** image recognition option shows "No disponible (entrenar modelo primero)"
- **AND** admin dashboard shows "Train First Model" button
- **AND** only manual sales entry is available

#### Scenario: First model training after deployment

- **WHEN** administrator uploads product photos (via EP1)
- **AND** trains first model via browser dashboard
- **THEN** browser trains model using TensorFlow.js
- **AND** browser uploads trained model to server
- **AND** server stores model and updates ModelMetadata
- **AND** image recognition becomes available for operators

#### Scenario: Preserve previous model on new training

- **WHEN** administrator trains new model version
- **THEN** system does not delete previous model files
- **AND** previous model remains accessible via versioned path
- **AND** ModelMetadata history preserves all versions
- **AND** admin can rollback by updating metadata to point to previous version (Phase 2)

#### Scenario: Server-side training disabled

- **WHEN** POST /api/image-recognition/retrain is called
- **THEN** system returns 501 Not Implemented
- **AND** response message: "Server-side training is disabled. Use browser-based training."
- **AND** no BackgroundService or Python scripts are executed

### Requirement: Product Suggestions with Confidence Scores

The system SHALL generate 3-5 product suggestions ordered by confidence score, displaying reference photos, product details, and confidence percentages to assist operator selection.

Inference matching SHALL use `ProductSku` to resolve class predictions to product details, ensuring stability when product names are edited.

#### Scenario: Display suggestions with confidence scores

- **WHEN** inference completes successfully
- **AND** at least one product has confidence >= 40% threshold
- **THEN** system displays top 3-5 suggestions ordered by confidence (highest first)
- **AND** number of suggestions shown depends on how many exceed 40% threshold (variable: 3-5 max)
- **AND** each suggestion shows:
  - Product reference photo (primary ProductPhoto)
  - SKU
  - Product name
  - Confidence score as percentage (e.g., "85%")
- **AND** suggestions are visually distinct (cards or list with photos)
- **AND** operator can select any suggestion by tapping/clicking

#### Scenario: Match predictions to products by SKU

- **WHEN** model outputs confidence scores for each class
- **THEN** system maps class index to SKU (from trained class labels)
- **AND** system fetches product details (name, photo, ID) using SKU as key
- **AND** product name displayed is the CURRENT name from database (not training-time name)
- **AND** matching succeeds even if product was renamed after training

#### Scenario: Debug logging shows SKU and name

- **WHEN** inference generates predictions
- **THEN** debug logs display both SKU and product name for each prediction
- **AND** format: `[index] SKU (ProductName): confidence%`
- **AND** developers can identify products easily while maintaining SKU-based matching

#### Scenario: Show 3 suggestions when only 3 exceed threshold

- **WHEN** inference generates exactly 3 products with confidence >= 40%
- **THEN** system displays exactly 3 suggestions
- **AND** does not show 4th or 5th product if confidence < 40%

#### Scenario: Show 5 suggestions when many exceed threshold

- **WHEN** inference generates 5 or more products with confidence >= 40%
- **THEN** system displays top 5 suggestions (capped at 5)
- **AND** does not show 6th product even if confidence >= 40%

#### Scenario: Show 4 suggestions when 4 exceed threshold

- **WHEN** inference generates exactly 4 products with confidence >= 40%
- **THEN** system displays exactly 4 suggestions

#### Scenario: Highlight top suggestion

- **WHEN** suggestions are displayed
- **THEN** system visually highlights first suggestion (highest confidence)
- **AND** first suggestion is pre-selected or emphasized (e.g., larger card, different color)
- **AND** operator can confirm top suggestion or select alternative

#### Scenario: Select product from suggestions

- **WHEN** operator taps/clicks a suggestion
- **THEN** system marks selected product
- **AND** continues to quantity and payment method selection (sales-management workflow)
- **AND** captured photo is attached to sale

## ADDED Requirements

### Requirement: Model Retraining Required After SKU Migration

The system SHALL require model retraining when migrating from ProductName-based to ProductSku-based class labels, as existing trained models become incompatible with the new mapping scheme.

#### Scenario: Existing model incompatible after migration

- **WHEN** system is upgraded from ProductName to ProductSku class labels
- **AND** previously trained model exists
- **THEN** inference will fail to match class labels to current product mappings
- **AND** system detects mismatch between model outputs and class labels
- **AND** displays warning: "El modelo fue entrenado con una versión anterior. Re-entrene el modelo."
- **AND** redirects to manual entry until model is retrained

#### Scenario: Administrator retrains model after migration

- **WHEN** administrator accesses model training after migration
- **THEN** system displays notice: "Se requiere re-entrenamiento para usar reconocimiento de imágenes"
- **AND** training proceeds normally using SKU-based class labels
- **AND** new model is compatible with inference system
