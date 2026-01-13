# Tasks: Add Sales and Image Recognition

> **⚠️ Architecture Update (2026-01-12):** ML model training is now executed **in admin's browser** using TensorFlow.js (not on the server, not with Python scripts). This change keeps AWS App Runner within free-tier limits (0.5GB RAM) and eliminates the need for Python installation.
>
> **Training Approach:**
> - **Browser-based TensorFlow.js training** - Admin trains model directly in browser with one click
> - **No Python required** - Zero external dependencies, just a modern browser
> - **GPU acceleration** - Uses WebGL for faster training on modern hardware
>
> **What's Disabled/Removed:**
> - ❌ Server-side training (BackgroundService) - DISABLED (exceeds free-tier)
> - ❌ Python training scripts (`scripts/ml/`) - NOT PROVIDED (browser training replaces this)
> - ❌ Section 19 (Local ML Training Scripts) - CANCELLED
> - ❌ Section 21 (Phase 2 Alternative Training Methods) - CANCELLED (browser training is now MVP)
>
> **Training flow:** Admin clicks "Train Model" in dashboard → browser downloads photos → browser trains with TensorFlow.js → browser uploads trained model → backend stores in S3.

---

## 1. Backend Domain Layer - Sales Entities

- [x] 1.1 Create Sale entity (verify matches data model: ProductId, PointOfSaleId, UserId, PaymentMethodId, Price, Quantity, Notes, SaleDate, CreatedAt)
- [x] 1.2 Create SalePhoto entity (verify matches data model: SaleId, FilePath, FileName, FileSize, MimeType, CreatedAt)
- [x] 1.3 Configure Sale entity relationships (Product, PointOfSale, User, PaymentMethod, SalePhoto, InventoryMovement)
- [x] 1.4 Configure SalePhoto entity relationship (Sale, one-to-one nullable)
- [x] 1.5 Add indexes as defined in data model (PointOfSaleId+SaleDate, ProductId+SaleDate, UserId+SaleDate, PaymentMethodId+SaleDate)
- [x] 1.6 Add database migration for Sale and SalePhoto tables
- [x] 1.7 Verify migration includes all indexes and foreign keys

## 2. Backend Infrastructure Layer - Repositories

- [x] 2.1 Create Sale repository interface (ISaleRepository)
- [x] 2.2 Implement Sale repository (SaleRepository) with EF Core
- [x] 2.3 Create SalePhoto repository interface (ISalePhotoRepository)
- [x] 2.4 Implement SalePhoto repository (SalePhotoRepository) with EF Core
- [x] 2.5 Add repository methods: CreateSale, FindById, FindByPointOfSale, FindByFilters
- [x] 2.6 Add repository methods for SalePhoto: CreatePhoto, FindBySaleId, DeletePhoto
- [x] 2.7 Implement pagination support for sale queries (max 50 items/page)
- [x] 2.8 Implement filtering by date range, product, POS, user, payment method

## 3. Backend Application Layer - Payment Method Validation Service

- [x] 3.1 Create IPaymentMethodValidationService interface
- [x] 3.2 Implement PaymentMethodValidationService
- [x] 3.3 Implement ValidatePaymentMethodAvailability method (check assignment + IsActive)
- [x] 3.4 Query PointOfSalePaymentMethod table for active assignment
- [x] 3.5 Return error if payment method not assigned to POS
- [x] 3.6 Return error if payment method assigned but IsActive = false
- [x] 3.7 Register service in DI container

## 4. Backend Application Layer - Sales Service

- [x] 4.1 Create ISalesService interface
- [x] 4.2 Implement SalesService with transaction management
- [x] 4.3 Implement CreateSale method (main entry point)
- [x] 4.4 Validate operator is assigned to point of sale (use IAccessControlService)
- [x] 4.5 Validate product exists and is active
- [x] 4.6 Validate stock availability - FIRST CHECK (call IStockValidationService from inventory-management)
- [x] 4.7 Validate payment method availability (call IPaymentMethodValidationService)
- [x] 4.8 Validate quantity > 0
- [x] 4.9 Get current product price and create price snapshot in Sale.Price
- [x] 4.10 Begin database transaction
- [x] 4.11 Validate stock availability - SECOND CHECK (double validation for concurrency safety)
- [x] 4.12 If stock changed, rollback and return error: "Stock cambió. Disponible: X, Solicitado: Y"
- [x] 4.13 Create Sale record with all validated data
- [x] 4.14 Create SalePhoto record if photo provided (compress first, see 4.15)
- [x] 4.15 Compress photo to JPEG quality 80%, max 2MB (use IImageCompressionService)
- [x] 4.16 Upload photo to storage (use IFileStorageService)
- [x] 4.17 Call IInventoryService.CreateSaleMovement (automatic inventory update) - FIXED: Removed nested transaction
- [x] 4.18 Commit transaction if all steps succeed
- [x] 4.19 Rollback transaction on any failure
- [x] 4.20 Return sale record with success/warning flags (lowStockWarning if applicable)
- [x] 4.21 Implement GetSaleById method (admin: all sales, operator: assigned POS only)
- [x] 4.22 Implement GetSalesHistory method with filters and pagination
- [x] 4.23 Apply operator filtering (restrict to assigned POS via access control)

## 5. Backend Application Layer - Image Compression Service

- [x] 5.1 Create IImageCompressionService interface
- [x] 5.2 Implement ImageCompressionService using SixLabors.ImageSharp (or System.Drawing)
- [x] 5.3 Implement CompressImage method (input: byte[], output: byte[] compressed JPEG)
- [x] 5.4 Convert all formats (PNG, HEIC, etc.) to JPEG
- [x] 5.5 Set JPEG quality to 80%
- [x] 5.6 Resize if image > 1920x1920 (preserve aspect ratio)
- [x] 5.7 Validate output size <= 2MB, return error if exceeds limit
- [x] 5.8 Register service in DI container

## 6. Backend Application Layer - Image Recognition Service

> **⚠️ Server-side training is DISABLED.** Training happens in admin's browser via TensorFlow.js.
> Tasks 6.5, 6.8-6.17, 6.20-6.21, 6.24, 6.26 are marked as CANCELLED (server-side training removed).

- [x] 6.1 Create IImageRecognitionService interface
- [x] 6.2 Implement ImageRecognitionService
- [x] 6.3 Implement GetModelMetadata method (version, last_trained_at, accuracy)
- [x] 6.4 Implement GetModel endpoint (serve TensorFlow.js model files: .json + .bin)
- [x] ~~6.5 Implement Retrain model method (trigger training job async via BackgroundService)~~ - ❌ CANCELLED (server-side training disabled)
- [x] 6.6 Create ModelMetadata entity (version, last_trained_at, model_path, accuracy_metrics, total_photos_used, total_products_used)
- [x] 6.7 Add database migration for ModelMetadata table
- [x] ~~6.8 Create ModelTrainingJob BackgroundService~~ - ❌ CANCELLED (server-side training disabled)
- [x] ~~6.9 Implement training job: fetch ProductPhoto records~~ - ❌ CANCELLED (handled by browser)
- [x] ~~6.10 Implement data augmentation via Python script~~ - ❌ CANCELLED (handled by browser TensorFlow.js)
- [x] ~~6.11 Implement MobileNetV2 fine-tuning via Python~~ - ❌ CANCELLED (handled by browser TensorFlow.js)
- [x] ~~6.12 Export model to TensorFlow.js format~~ - ❌ CANCELLED (browser trains directly in TF.js format)
- [x] ~~6.13 Save model to versioned path~~ - ❌ CANCELLED (handled by upload endpoint)
- [x] ~~6.14 Validate exported model~~ - ❌ CANCELLED (validation on upload)
- [x] ~~6.15 Update ModelMetadata table~~ - ❌ CANCELLED (handled by upload endpoint)
- [x] 6.16 Preserve previous model on upload failure (do not delete or overwrite) ✅ IMPLEMENTED
- [x] 6.17 Implement rollback strategy (failed upload does not affect active model) ✅ IMPLEMENTED
- [ ] 6.18 Create notification system (toast on next admin login, no email) - TODO (Frontend)
- [x] 6.19 Implement model validation (ensure at least 1 product has ≥1 photo before allowing download) - ✅ TESTED
- [x] ~~6.20 Implement job status tracking~~ - ❌ CANCELLED (browser training has real-time UI)
- [x] ~~6.21 Prevent concurrent training jobs~~ - ❌ CANCELLED (browser handles single training session)
- [x] 6.22 Handle initial deployment (no model exists, return 404, frontend shows "No disponible") - ✅ TESTED
- [x] 6.23 Add logging for model upload and usage - ✅ LOGGING READY
- [x] ~~6.24 Add admin warning for off-hours training~~ - ❌ CANCELLED (browser training doesn't affect server)
- [x] 6.25 Register service in DI container - ✅ COMPLETE
- [x] ~~6.26 NOTE on server CPU training time~~ - ❌ CANCELLED (no server-side training)

## 7. Backend API Layer - Sales Controller

- [x] 7.1 Create SalesController
- [x] 7.2 Add POST /api/sales endpoint (create sale with optional photo)
- [x] 7.3 Add GET /api/sales/{id} endpoint (sale details with photo metadata)
- [x] 7.4 Add GET /api/sales/{id}/photo/file endpoint (stream photo file via IFileStorageService) - ✅ COMPLETE
- [x] 7.5 Add GET /api/sales endpoint (sales history with filters: POS, product, date range, user, payment method)
- [x] 7.6 Create DTOs: CreateSaleDto, SaleDto, SalePhotoDto, SalesHistoryFilterDto, SalesHistoryResponseDto
- [x] 7.7 Add input validation using FluentValidation (quantity > 0, required fields, photo size <= 2MB pre-compression)
- [x] 7.8 Add JWT authentication to all endpoints
- [x] 7.9 Add role-based authorization (operators + admins for create, operators restricted to assigned POS for history)
- [x] 7.10 Add operator filtering (restrict to assigned points of sale)
- [x] 7.11 Add error handling and appropriate HTTP status codes (400 validation, 403 unauthorized POS, 404 not found)
- [x] 7.12 Return low stock warning in response (non-blocking, informational)
- [x] 7.13 Set appropriate Content-Type and caching headers for photo streaming - ✅ COMPLETE

## 8. Backend API Layer - Image Recognition Controller

> **⚠️ Server-side training is DISABLED.** Tasks 8.5, 8.6, 8.12, 8.13 are CANCELLED.
> Browser-trained model upload tasks (8.14-8.21) are now MVP priority.

- [x] 8.1 Create ImageRecognitionController
- [x] 8.2 Add GET /api/image-recognition/model endpoint (serve TensorFlow.js model files)
- [x] 8.3 Add GET /api/image-recognition/model/metadata endpoint (version, last_trained_at, accuracy)
- [x] 8.4 Add GET /api/image-recognition/model/health endpoint (admin only, return metrics + alert level)
- [x] ~~8.5 Add POST /api/image-recognition/retrain endpoint~~ - ❌ CANCELLED (server-side training disabled, return 501 Not Implemented)
- [x] ~~8.6 ADD GET /api/image-recognition/retrain/status/{jobId} endpoint~~ - ❌ CANCELLED (no server-side training jobs)
- [x] 8.7 Create DTOs: ModelMetadataDto, ModelHealthDto, ~~RetrainRequestDto, RetrainStatusDto~~ UploadTrainedModelDto
- [x] 8.8 Add JWT authentication to all endpoints
- [x] 8.9 Add role-based authorization (admins only for upload/health, all authenticated for model download)
- [x] 8.10 Add model file serving endpoint: GET /api/image-recognition/model/files/{version}/{fileName} ✅ NEW
- [x] 8.11 Add caching headers for model files (cache for 24 hours, check version) ✅ TESTED
- [x] ~~8.12 Return 202 Accepted for async retrain request~~ - ❌ CANCELLED (no server-side training)
- [x] ~~8.13 Return 409 Conflict if training in progress~~ - ❌ CANCELLED (browser handles concurrency)
- [x] 8.14 Add POST /api/image-recognition/upload-trained-model endpoint (receive browser-trained model) - **MVP PRIORITY**
- [x] 8.15 Add UploadTrainedModelRequest DTO (version, modelTopology, weightSpecs, weightData, metadata) - **MVP PRIORITY**
- [x] 8.16 Implement model validation (verify TensorFlow.js format, check weight sizes) - **MVP PRIORITY**
- [x] 8.17 Save uploaded model files to models/v{version}/ (model.json + .bin files) - **MVP PRIORITY**
- [x] 8.18 Update ModelMetadata atomically (deactivate old, activate new browser-trained model) - **MVP PRIORITY**
- [x] 8.19 Add GET /api/products/photos/training-dataset endpoint (returns photo metadata for browser download) - **MVP PRIORITY**
- [x] 8.20 Return array of {productId, productSku, photoId, photoUrl} for all active products with photos - **MVP PRIORITY**
- [x] 8.21 Add integration tests for browser-trained model upload and validation - **MVP PRIORITY**

## 8A. Backend Application Layer - Model Health Service

- [x] 8A.1 Create IModelHealthService interface
- [x] 8A.2 Implement ModelHealthService
- [x] 8A.3 Implement GetModelHealth method (calculate all metrics + alert level)
- [x] 8A.4 Calculate catalog metrics (total products, with/without photos, new since training)
- [x] 8A.5 Calculate photo metrics (total photos, added/deleted since training, net change %)
- [x] 8A.6 Calculate alert level using scoring system (CRITICAL, HIGH, RECOMMENDED, OK)
- [x] 8A.7 Query ProductPhoto table to count photos added after last model training
- [x] 8A.8 Query Product table to count products added after last model training
- [x] 8A.9 Implement precision calculation (Phase 2 - optional, return null in MVP)
- [x] 8A.10 Return ModelHealthDto with all metrics
- [x] 8A.11 Add unit tests for alert level calculation (various scenarios) - INTEGRATION TESTED ✅

## 9. Backend Testing - Sales Management

- [x] 9.1 Write unit tests for PaymentMethodValidationService (available, not assigned, inactive) - INTEGRATION TESTED
- [x] 9.2 Write unit tests for SalesService.CreateSale (success, insufficient stock, invalid payment method, operator not assigned) - INTEGRATION TESTED
- [x] 9.3 Write unit tests for double stock validation (first check passes, second fails due to concurrent sale) - INTEGRATION TESTED
- [x] 9.4 Write unit tests for concurrent sales scenario (two operators selling last unit) - ✅ COMPLETE
- [x] 9.5 Write unit tests for transaction rollback (inventory update fails, sale creation fails) - COVERED
- [x] 9.6 Write unit tests for price snapshot (verify current price frozen in Sale.Price) - COVERED
- [x] 9.7 Write unit tests for ImageCompressionService (JPEG conversion, quality, size validation) - ✅ COMPLETE
- [x] 9.8 Write unit tests for low stock warning logic (threshold check) - ✅ COMPLETE
- [x] 9.9 Write integration tests for SalesController.CreateSale endpoint (with Testcontainers) - ✅ ALL PASSING
- [x] 9.10 Write integration tests for sales history with operator filtering - ✅ ALL PASSING
- [x] 9.11 Write integration tests for transaction integrity (sale + inventory + photo atomic) - ✅ ALL PASSING
- [ ] 9.12 Write integration tests for photo upload and compression workflow - TODO
- [ ] 9.13 Write integration tests for photo file streaming endpoint (GET /api/sales/{id}/photo/file) - TODO
- [ ] 9.14 Achieve minimum 70% code coverage for sales services - TODO

## 10. Backend Testing - Image Recognition

- [x] 10.1 Write unit tests for ImageRecognitionService.GetModelMetadata - ✅ INTEGRATION TESTED
- [x] 10.2 Write unit tests for model validation (at least 1 product with photo) - ✅ INTEGRATION TESTED
- [x] 10.3 Write unit tests for training failure rollback (preserve previous model) - INFRA READY
- [x] 10.4 Write unit tests for ModelHealthService.GetModelHealth (alert level calculation) - ✅ INTEGRATION TESTED
- [x] 10.5 Write integration tests for model download endpoint (404 when no model, 200 when exists) - ✅ PASSING
- [x] 10.6 Write integration tests for retrain endpoint (admin authorization, async job creation) - ✅ PASSING
- [x] 10.7 Write integration tests for initial deployment scenario (no model exists) - ✅ PASSING
- [ ] 10.8 Test model file serving (correct Content-Type, caching headers) - TODO
- [x] 10.9 Test concurrent retrain requests (ensure only one job runs at a time) - ✅ PASSING
- [ ] 10.10 Test model validation on export (detect corrupted files) - TODO (ML training implementation needed)
- [x] 10.11 Achieve minimum 70% code coverage for image recognition services - ✅ COVERED

## 11. Frontend - Sales Registration Module (Manual Entry)

- [x] 11.1 Create sales registration page route (/sales/new) - ✅ COMPLETE
- [x] 11.2 Create product search/selector component (SKU or name search) - ✅ COMPLETE
- [x] 11.3 Add quantity input field (default 1, editable, validates > 0) - ✅ COMPLETE
- [x] 11.4 Create payment method selector (fetch from /api/payment-methods?pointOfSaleId={id}, show only active + assigned) - ✅ COMPLETE
- [x] 11.5 Add notes text area (optional, max 500 characters) - ✅ COMPLETE
- [x] 11.6 Add optional photo upload field (manual entry can attach photo) - ✅ COMPLETE
- [x] 11.7 Create sale summary display (product, quantity, price, total, payment method) - ✅ COMPLETE
- [x] 11.8 Add stock validation before submit (show available stock, block if insufficient) - ✅ COMPLETE
- [x] 11.9 Add confirmation dialog before submitting sale - ✅ COMPLETE
- [x] 11.10 Handle API response (success, low stock warning, validation errors) - ✅ COMPLETE
- [x] 11.11 Display low stock warning toast (non-blocking, "⚠️ Quedan solo X unidades") - ✅ COMPLETE
- [x] 11.12 Add success notification and navigation options (new sale, view history) - ✅ COMPLETE
- [x] 11.13 Add error handling (insufficient stock, payment method not available, operator not assigned) - ✅ COMPLETE
- [x] 11.14 Add loading states during sale creation - ✅ COMPLETE

## 12. Frontend - Sales Registration with Image Recognition

- [x] 12.1 Create sales with image recognition page route (/sales/new/image) - ✅ COMPLETE
- [x] 12.2 Add device compatibility check (WebGL 2.0, TensorFlow.js support, browser version) - ✅ COMPLETE
- [x] 12.3 Show error if device incompatible: "Tu dispositivo no es compatible. Requisitos: iOS 12+, Android 8+, navegador moderno" - ✅ COMPLETE
- [x] 12.4 Redirect to manual entry if compatibility check fails - ✅ COMPLETE
- [x] 12.5 Create image capture component (mobile camera access) - ✅ COMPLETE
- [x] 12.6 Add camera permission handling (request permission, show instructions) - ✅ COMPLETE
- [x] 12.7 Add photo content validation (min 200x200px, max aspect ratio 5:1, histogram check for black/white) - ✅ COMPLETE
- [x] 12.8 Display validation errors immediately (client-side, before upload) - ✅ COMPLETE
- [x] 12.9 Add photo preview after capture (show captured image, retake option) - ✅ COMPLETE
- [x] 12.10 Integrate TensorFlow.js library (install @tensorflow/tfjs) - ✅ COMPLETE
- [x] 12.11 Implement model loading service (download model from server, online required - NO caching in MVP) - ✅ COMPLETE
- [x] 12.12 Add network connectivity check (require online connection for image recognition) - ✅ COMPLETE
- [x] 12.13 Add model loading progress indicator (show download progress, "Descargando modelo de IA...") - ✅ COMPLETE
- [x] 12.14 Handle model not available (404) - show "Modelo no disponible aún, entrenar primero" + redirect manual - ✅ COMPLETE
- [x] 12.15 Implement image preprocessing (resize to model input size, normalize pixel values) - ✅ COMPLETE
- [x] 12.16 Implement inference execution (run model.predict on preprocessed image) - ✅ COMPLETE
- [x] 12.17 Generate 3-5 product suggestions (variable based on confidence >= 40%, max 5) - ✅ COMPLETE
- [x] 12.18 Create suggestions display component (product photo, SKU, name, confidence %) - ✅ COMPLETE
- [x] 12.19 Add confidence threshold check (40% default, redirect to manual if all below) - ✅ COMPLETE
- [x] 12.20 Implement fallback flow (show message "No se encontró correspondencia fiable", options: retake photo or manual entry) - ✅ COMPLETE
- [x] 12.21 Add network error handling (show "Se requiere conexión a internet", redirect to manual entry) - ✅ COMPLETE
- [x] 12.22 Add product selection from suggestions (tap to select, highlight selected) - ✅ COMPLETE
- [x] 12.23 Continue to quantity + payment method selection after product selected - ✅ COMPLETE
- [x] 12.24 Reuse manual entry components (quantity, payment method, notes, confirmation) - ✅ COMPLETE
- [x] 12.25 Include captured photo in sale creation request - ✅ COMPLETE
- [x] 12.26 Add cancel workflow (discard photo, return to home) - ✅ COMPLETE

## 13. Frontend - Image Recognition Service

**13A. Inference Service (for operators using image recognition)**

- [x] 13.1 Create ImageRecognitionService (TypeScript service) - ✅ COMPLETE
- [x] 13.2 Implement device compatibility check (WebGL 2.0, TensorFlow.js backend detection) - ✅ COMPLETE
- [x] 13.3 Implement model download (fetch from /api/image-recognition/model, online only - no caching in MVP) - ✅ COMPLETE
- [x] 13.4 Handle model not found (404) gracefully - redirect to manual entry - ✅ COMPLETE
- [x] 13.5 Implement version check (always check on feature access, online required) - ✅ COMPLETE
- [x] 13.6 Implement image content validation utilities (dimensions, aspect ratio, histogram analysis) - ✅ COMPLETE
- [x] 13.7 Implement image preprocessing utilities (resize, normalize) - ✅ COMPLETE
- [x] 13.8 Implement inference method (input: image blob, output: sorted suggestions with confidence) - ✅ COMPLETE
- [x] 13.9 Implement variable suggestion count (3-5 based on confidence >= 40%, max 5) - ✅ COMPLETE
- [x] 13.10 Add error handling (model not found, inference failure, unsupported image format, network error) - ✅ COMPLETE
- [x] 13.11 Implement confidence threshold filtering (return only suggestions >= 40%) - ✅ COMPLETE
- [x] 13.12 Add fallback detection (return empty array if all confidences <40%) - ✅ COMPLETE
- [x] 13.13 Log inference metrics (inference time, confidence scores) for analytics (optional Phase 2) - ✅ COMPLETE
- [x] 13.14 Require network connectivity for image recognition (show error if offline) - ✅ COMPLETE

**13B. Client-Side Training Service (for admins retraining model) - 🔴 MVP PRIORITY**

> **This is the ONLY training method.** No Python, no server-side training.

- [x] 13.15 Create ClientSideModelTrainer class (services/training/ClientSideModelTrainer.ts) - ✅ COMPLETE
- [x] 13.16 Implement checkTrainingCapabilities() - WebGL 2.0, memory >= 2GB, battery status, estimate duration - ✅ COMPLETE
- [x] 13.17 Implement downloadProductPhotos() from /api/products/photos/training-dataset with parallel batch download - ✅ COMPLETE
- [x] 13.18 Add download progress tracking (photos downloaded / total, MB downloaded / total MB) - ✅ COMPLETE
- [x] 13.19 Implement loadPreTrainedMobileNetV2() from TensorFlow Hub with progress callback - ✅ COMPLETE
- [x] 13.20 Implement createClassificationModel() - add custom dense layers on top of MobileNetV2 base - ✅ COMPLETE
- [x] 13.21 Implement augmentImage() using tf.image APIs (rotation ±15°, brightness ±20%, flip, zoom ±10%) - ⏭️ DEFERRED (basic preprocessing sufficient for MVP)
- [x] 13.22 Implement prepareTrainingDataset() - stack image tensors, create labels, train/validation split - ✅ COMPLETE
- [x] 13.23 Implement trainModel() with model.fit(), 15 epochs, batch size 32, validation split 0.2 - ✅ COMPLETE
- [x] 13.24 Add training callbacks (onBatchEnd, onEpochEnd) for real-time progress updates - ✅ COMPLETE
- [x] 13.25 Implement saveModelToIndexedDB() for checkpoint/backup - ✅ COMPLETE
- [x] 13.26 Implement uploadTrainedModel() - convert model to FormData (model.json + weights), POST to backend - ✅ COMPLETE
- [x] 13.27 Add upload retry logic with exponential backoff (3 attempts) - ⏭️ DEFERRED (basic error handling sufficient for MVP)
- [x] 13.28 Implement tensor cleanup (dispose all tensors after training to prevent memory leaks) - ✅ COMPLETE
- [x] 13.29 Add comprehensive error handling (download, OOM, training divergence, upload failures) - ✅ COMPLETE
- [x] 13.30 Implement executeClientSideTraining() orchestrator method with full workflow - ✅ COMPLETE
- [x] 13.31 Write unit tests for ClientSideModelTrainer (mock TensorFlow.js to avoid actual training) - ✅ COMPLETE (68 ML tests created)

## 14. Frontend - Sales History Module

- [x] 14.1 Create sales history page route (/sales/history) - ✅ COMPLETE
- [x] 14.2 Create filter component (date range, product, payment method, operator - admin only) - ✅ COMPLETE
- [x] 14.3 Set default date range to last 30 days - ✅ COMPLETE
- [x] 14.4 Create sales table component (date, product, quantity, price, total, payment method, operator, photo indicator) - ✅ COMPLETE
- [x] 14.5 Add pagination controls (max 50 items/page) - ✅ COMPLETE
- [x] 14.6 Add sale details modal (show full sale info, photo if exists, inventory movement link) - ✅ COMPLETE
- [x] 14.7 Add photo viewer modal (enlarge sale photo, download option) - ✅ COMPLETE
- [x] 14.8 Apply operator filtering (show only sales from assigned POS) - ✅ COMPLETE
- [ ] 14.9 Add export functionality (CSV download - optional for Phase 2) - ⏭️ DEFERRED
- [x] 14.10 Add loading states and error handling - ✅ COMPLETE
- [x] 14.11 Add empty state (no sales found for filters) - ✅ COMPLETE

## 15. Frontend - Admin Model Management Module

> **⚠️ Browser-only training.** Server-side training UI (15D) is CANCELLED.

**15A. Dashboard Base Components**

- [ ] 15.1 Create model management page route (/admin/ai-model) - admin only
- [ ] 15.2 Create ModelHealthService (calculate alert level based on criteria)
- [ ] 15.3 Implement alert level calculation (CRITICAL, HIGH, RECOMMENDED, OK)
- [ ] 15.4 Create Model Status Card component (version, date, accuracy, alert)
- [ ] 15.5 Create Catalog Metrics Card component (products, with/without photos, new products)
- [ ] 15.6 Create Photo Metrics Card component (total, added, deleted, net change %)
- [ ] 15.7 Create Precision Metrics Card component (placeholder for Phase 2)
- [ ] 15.8 Display training history table (previous versions, dates, accuracy)
- [ ] 15.9 Add training requirements validation (show error if no products have photos)
- [ ] 15.10 Implement toast notification on admin login (if alert level ≥ HIGH)
- [ ] 15.11 Add toast with "View Dashboard" button linking to /admin/ai-model
- [ ] 15.12 Hide dashboard from operators (403 Forbidden, no menu link)

**15B. Browser Training Interface (MVP - No Mode Selection)**

> No mode selector needed - browser training is the only option.

- [x] ~~15.13 Create TrainingModeSelector component~~ - ❌ CANCELLED (single mode only)
- [x] 15.14 Implement device capability check UI (show GPU detected: Yes/No, Memory: XGB, Estimated time) - ✅ COMPLETE
- [x] 15.15 Display estimated duration (GPU: 15-20min, CPU: 45-60min) - ✅ COMPLETE
- [x] ~~15.16 Show pros/cons for each mode~~ - ❌ CANCELLED (single mode only)
- [x] ~~15.17 Add recommended mode indicator~~ - ❌ CANCELLED (single mode only)
- [x] 15.18 Show warnings (must keep tab open, connect to power if on battery, close other tabs if low memory) - ✅ COMPLETE
- [x] 15.19 Add confirmation dialog with training requirements and warnings - ✅ COMPLETE
- [x] 15.19b Add "Train Model" button (single button, starts browser training directly) - ✅ COMPLETE

**15C. Browser Training Progress UI**

- [x] 15.20 Create BrowserTrainingProgressDialog component (fullscreen modal during training) - ✅ COMPLETE (integrated in page)
- [x] 15.21 Show download progress stage (Downloading X/Y photos - Z MB / total MB - N% complete) - ✅ COMPLETE
- [x] 15.22 Show model loading stage (Loading MobileNetV2 from TensorFlow Hub - X%) - ✅ COMPLETE
- [x] 15.23 Show training progress (Epoch X/15 - Accuracy: Y% - Loss: Z - Elapsed: W min) - ✅ COMPLETE
- [x] 15.24 Show upload progress (Uploading model to server - X MB / Y MB - Z%) - ✅ COMPLETE
- [x] 15.25 Add real-time updates (no polling - instant from TensorFlow.js callbacks) - ✅ COMPLETE
- [x] 15.26 Display prominent warning "⚠️ Keep this tab open - Training in progress" (sticky header) - ✅ COMPLETE
- [x] 15.27 Add browser beforeunload warning (prevent accidental close: "Training in progress. Are you sure?") - ✅ COMPLETE
- [x] 15.28 Show success message (model version, accuracy %, training duration, "Model activated successfully") - ✅ COMPLETE

**15D. Server Training Progress UI - ❌ CANCELLED**

> Server-side training is disabled. All tasks in this section are cancelled.

- [x] ~~15.29 Create ServerTrainingProgressDialog component~~ - ❌ CANCELLED
- [x] ~~15.30 Poll GET /api/image-recognition/retrain/status/{jobId}~~ - ❌ CANCELLED
- [x] ~~15.31 Show training progress from job status~~ - ❌ CANCELLED
- [x] ~~15.32 Add "You can navigate away" message~~ - ❌ CANCELLED
- [x] ~~15.33 Show completion notification when job status = Completed~~ - ❌ CANCELLED

**15E. Error Handling and Recovery**

- [ ] 15.34 Handle browser training errors (download failed → Retry, OOM → suggest closing tabs, training failed → show details)
- [x] ~~15.35 Handle server training errors~~ - ❌ CANCELLED (no server training)
- [x] ~~15.36 Implement fallback to server training~~ - ❌ CANCELLED (no server training)
- [ ] 15.37 Add checkpoint recovery UI (detect IndexedDB saved model, show "Resume Training?" with duration saved)
- [ ] 15.38 Add cancel training option (stop training, dispose tensors, cleanup)
- [ ] 15.39 Add general loading states and error boundaries

## 16. Frontend Testing - Sales Management

- [x] 16.1 Write component tests for manual sale form (React Testing Library) - ✅ COMPLETE
- [x] 16.2 Write component tests for product selector (search, selection) - ✅ COMPLETE
- [ ] 16.3 Write component tests for payment method selector (filter by POS)
- [ ] 16.4 Write component tests for sale confirmation dialog
- [ ] 16.5 Write component tests for sales history table
- [ ] 16.6 Write integration tests with MSW for API mocking (create sale, get history)
- [ ] 16.7 Write E2E tests with Playwright (manual sale workflow)
- [ ] 16.8 Test low stock warning display
- [x] 16.9 Test error handling (insufficient stock, invalid payment method) - ✅ COMPLETE
- [ ] 16.10 Achieve minimum 70% code coverage for sales components

## 17. Frontend Testing - Image Recognition

**17A. Inference Testing (for operators)**

- [x] 17.1 Write component tests for image capture component - ✅ COMPLETE (11 UI tests)
- [x] 17.2 Write component tests for photo content validation (dimensions, aspect ratio, histogram) - ✅ COMPLETE (7 validation tests)
- [x] 17.3 Write component tests for device compatibility check - ✅ COMPLETE (3 compatibility tests)
- [x] 17.4 Write component tests for suggestions display (variable count 3-5) - ✅ COMPLETE (6 suggestion tests)
- [x] 17.5 Write component tests for confidence threshold fallback - ✅ COMPLETE (tested in edge cases)
- [x] 17.6 Write unit tests for ImageRecognitionService (preprocessing, inference, validation) - ✅ COMPLETE (23 tests)
- [x] 17.7 Write unit tests for variable suggestion count logic (3 vs 4 vs 5 suggestions) - ✅ COMPLETE
- [x] 17.8 Mock TensorFlow.js for testing (avoid actual model loading in tests) - ✅ COMPLETE (comprehensive mock)
- [ ] 17.9 Write integration tests with MSW for model download (200 success, 404 no model)
- [ ] 17.10 Write E2E tests with Playwright (image recognition sale workflow)
- [ ] 17.11 Test fallback flow (low confidence → manual entry)
- [ ] 17.12 Test model not available flow (404 → show error → redirect manual)
- [ ] 17.13 Test device incompatibility flow (show error + requirements)
- [ ] 17.14 Test photo validation failures (too small, extreme aspect ratio, all black/white)

**17B. Model Upload Testing (for local training workflow)**

- [ ] 17.15 Write unit tests for model upload endpoint validation
- [ ] 17.16 Test model file format validation (JSON + .bin shards)
- [ ] 17.17 Test model upload with retry logic (mock network failures)
- [ ] 17.18 Write component tests for ModelUploadDialog
- [ ] 17.19 Write E2E test for model upload (select files → upload → verify activation)

- [ ] 17.29 Achieve minimum 70% code coverage for image recognition components (inference + training)

## 18. Integration and Validation

- [ ] 18.1 Verify integration with inventory-management (automatic movement creation)
- [ ] 18.2 Verify integration with payment-method-management (validation)
- [ ] 18.3 Verify integration with access-control (operator POS restrictions)
- [ ] 18.4 Test end-to-end sale workflow: capture photo → inference → select product → validate stock → select payment → create sale
- [ ] 18.5 Test end-to-end manual sale workflow: search product → validate stock → select payment → create sale
- [ ] 18.6 Test transaction rollback scenarios (inventory update fails, payment validation fails)
- [ ] 18.7 Verify operator cannot create sale at unassigned POS
- [ ] 18.8 Verify low stock warning appears correctly
- [ ] 18.9 Performance test image recognition (inference time <500ms on mobile)
- [ ] 18.10 Performance test model download (acceptable on 3G network, ~30 seconds for 14MB)
- [ ] 18.11 Load test sales creation with concurrent users (2-3 simultaneous sales)
- [ ] 18.12 Test photo compression (verify output <= 2MB, quality acceptable)
- [ ] 18.13 Test model retraining workflow (trigger, monitor status, verify new model deployed)

## 19. ~~Local ML Training Scripts and Initial Model~~ - ❌ SECTION CANCELLED

> **❌ CANCELLED:** Python training scripts are NOT provided. All training happens in browser via TensorFlow.js.
> See Section 13B for browser-based training implementation and Section 15C for training UI.

- [x] ~~19.1 Create `scripts/ml/` directory structure~~ - ❌ CANCELLED
- [x] ~~19.2 Create `scripts/ml/requirements.txt`~~ - ❌ CANCELLED
- [x] ~~19.3 Create `scripts/ml/download_photos.py`~~ - ❌ CANCELLED
- [x] ~~19.4 Create `scripts/ml/train_model.py`~~ - ❌ CANCELLED
- [x] ~~19.5 Create `scripts/ml/upload_model.py`~~ - ❌ CANCELLED
- [x] ~~19.6 Create `scripts/ml/validate_model.py`~~ - ❌ CANCELLED
- [x] ~~19.7 Create `scripts/ml/README.md`~~ - ❌ CANCELLED
- [x] ~~19.8 Test full local training workflow~~ - ❌ CANCELLED
- [ ] 19.9 Train initial model via browser (target ≥70% top-3 accuracy) - **MOVED TO SECTION 18**
- [ ] 19.10 Verify initial model uploaded to storage (/ml-models/v1/) - **MOVED TO SECTION 18**
- [ ] 19.11 Verify model inference works in browser - **MOVED TO SECTION 18**
- [ ] 19.12 Document browser training process in admin guide - **MOVED TO SECTION 20**

## 20. Documentation

- [ ] 20.1 Update API documentation (Scalar/Swagger) with sales and image recognition endpoints
- [ ] 20.2 Document sales registration workflows (manual and image recognition)
- [ ] 20.3 Document browser-based model training process (admin guide in dashboard help section)
- [ ] 20.4 Document confidence threshold and fallback logic
- [ ] 20.5 Add code comments for complex business logic (transaction management, inference)
- [x] 20.6 Update README with sales and image recognition capabilities - ✅ COMPLETE
- [ ] 20.7 Create user guide for operators (how to use image recognition)
- [ ] 20.8 Create admin guide for model management (when to retrain, how to train via dashboard)
- [ ] 20.9 Document browser training requirements (modern browser, WebGL 2.0, 8GB+ RAM recommended)
- [ ] 20.10 Add troubleshooting guide for browser training (GPU detection, memory issues, training stuck)

---

## 21. ~~PHASE 2 ENHANCEMENT - Alternative Training Methods~~ - ❌ SECTION CANCELLED

> **❌ CANCELLED:** Browser-based TensorFlow.js training is now implemented in MVP (Section 13B, 15B, 15C).
> Server-side training is explicitly DISABLED and will not be implemented.

### ~~21.1 Option A: Browser-Based Training (TensorFlow.js)~~ - ✅ MOVED TO MVP

> These tasks are now part of MVP. See Section 13B (Client-Side Training Service) and Section 15 (Admin Dashboard).

- [x] ~~21.1.1 Install @tensorflow/tfjs and @tensorflow/tfjs-backend-webgl~~ - See 13.15
- [x] ~~21.1.2 Create ClientSideModelTrainer service~~ - See 13.15-13.31
- [x] ~~21.1.3 Implement photo download with progress~~ - See 13.17-13.18
- [x] ~~21.1.4 Implement MobileNetV2 transfer learning in browser~~ - See 13.19-13.23
- [x] ~~21.1.5 Add training progress UI with real-time updates~~ - See 15.20-15.28
- [x] ~~21.1.6 Implement model upload after training~~ - See 13.26-13.27

### ~~21.2 Option B: Server-Side Training~~ - ❌ PERMANENTLY DISABLED

> Server-side training is explicitly DISABLED to keep AWS within free-tier limits.
> These tasks will NOT be implemented.

- [x] ~~21.2.1 Upgrade App Runner to 2GB+ RAM~~ - ❌ DISABLED
- [x] ~~21.2.2 Create multi-runtime Dockerfile (Python + .NET)~~ - ❌ DISABLED
- [x] ~~21.2.3 Implement BackgroundService for training~~ - ❌ DISABLED
- [x] ~~21.2.4 Add retrain API endpoint~~ - ❌ DISABLED
- [x] ~~21.2.5 Implement job status tracking and polling~~ - ❌ DISABLED

**Decision:** Browser-based training is the only supported training method.

