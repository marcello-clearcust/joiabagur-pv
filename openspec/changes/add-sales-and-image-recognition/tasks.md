## 1. Backend Domain Layer - Sales Entities

- [ ] 1.1 Create Sale entity (verify matches data model: ProductId, PointOfSaleId, UserId, PaymentMethodId, Price, Quantity, Notes, SaleDate, CreatedAt)
- [ ] 1.2 Create SalePhoto entity (verify matches data model: SaleId, FilePath, FileName, FileSize, MimeType, CreatedAt)
- [ ] 1.3 Configure Sale entity relationships (Product, PointOfSale, User, PaymentMethod, SalePhoto, InventoryMovement)
- [ ] 1.4 Configure SalePhoto entity relationship (Sale, one-to-one nullable)
- [ ] 1.5 Add indexes as defined in data model (PointOfSaleId+SaleDate, ProductId+SaleDate, UserId+SaleDate, PaymentMethodId+SaleDate)
- [ ] 1.6 Add database migration for Sale and SalePhoto tables
- [ ] 1.7 Verify migration includes all indexes and foreign keys

## 2. Backend Infrastructure Layer - Repositories

- [ ] 2.1 Create Sale repository interface (ISaleRepository)
- [ ] 2.2 Implement Sale repository (SaleRepository) with EF Core
- [ ] 2.3 Create SalePhoto repository interface (ISalePhotoRepository)
- [ ] 2.4 Implement SalePhoto repository (SalePhotoRepository) with EF Core
- [ ] 2.5 Add repository methods: CreateSale, FindById, FindByPointOfSale, FindByFilters
- [ ] 2.6 Add repository methods for SalePhoto: CreatePhoto, FindBySaleId, DeletePhoto
- [ ] 2.7 Implement pagination support for sale queries (max 50 items/page)
- [ ] 2.8 Implement filtering by date range, product, POS, user, payment method

## 3. Backend Application Layer - Payment Method Validation Service

- [ ] 3.1 Create IPaymentMethodValidationService interface
- [ ] 3.2 Implement PaymentMethodValidationService
- [ ] 3.3 Implement ValidatePaymentMethodAvailability method (check assignment + IsActive)
- [ ] 3.4 Query PointOfSalePaymentMethod table for active assignment
- [ ] 3.5 Return error if payment method not assigned to POS
- [ ] 3.6 Return error if payment method assigned but IsActive = false
- [ ] 3.7 Register service in DI container

## 4. Backend Application Layer - Sales Service

- [ ] 4.1 Create ISalesService interface
- [ ] 4.2 Implement SalesService with transaction management
- [ ] 4.3 Implement CreateSale method (main entry point)
- [ ] 4.4 Validate operator is assigned to point of sale (use IAccessControlService)
- [ ] 4.5 Validate product exists and is active
- [ ] 4.6 Validate stock availability - FIRST CHECK (call IStockValidationService from inventory-management)
- [ ] 4.7 Validate payment method availability (call IPaymentMethodValidationService)
- [ ] 4.8 Validate quantity > 0
- [ ] 4.9 Get current product price and create price snapshot in Sale.Price
- [ ] 4.10 Begin database transaction
- [ ] 4.11 Validate stock availability - SECOND CHECK (double validation for concurrency safety)
- [ ] 4.12 If stock changed, rollback and return error: "Stock cambió. Disponible: X, Solicitado: Y"
- [ ] 4.13 Create Sale record with all validated data
- [ ] 4.14 Create SalePhoto record if photo provided (compress first, see 4.15)
- [ ] 4.15 Compress photo to JPEG quality 80%, max 2MB (use IImageCompressionService)
- [ ] 4.16 Upload photo to storage (use IFileStorageService)
- [ ] 4.17 Call IInventoryService.CreateSaleMovement (automatic inventory update)
- [ ] 4.18 Commit transaction if all steps succeed
- [ ] 4.19 Rollback transaction on any failure
- [ ] 4.20 Return sale record with success/warning flags (lowStockWarning if applicable)
- [ ] 4.21 Implement GetSaleById method (admin: all sales, operator: assigned POS only)
- [ ] 4.22 Implement GetSalesHistory method with filters and pagination
- [ ] 4.23 Apply operator filtering (restrict to assigned POS via access control)

## 5. Backend Application Layer - Image Compression Service

- [ ] 5.1 Create IImageCompressionService interface
- [ ] 5.2 Implement ImageCompressionService using SixLabors.ImageSharp (or System.Drawing)
- [ ] 5.3 Implement CompressImage method (input: byte[], output: byte[] compressed JPEG)
- [ ] 5.4 Convert all formats (PNG, HEIC, etc.) to JPEG
- [ ] 5.5 Set JPEG quality to 80%
- [ ] 5.6 Resize if image > 1920x1920 (preserve aspect ratio)
- [ ] 5.7 Validate output size <= 2MB, return error if exceeds limit
- [ ] 5.8 Register service in DI container

## 6. Backend Application Layer - Image Recognition Service

- [ ] 6.1 Create IImageRecognitionService interface
- [ ] 6.2 Implement ImageRecognitionService
- [ ] 6.3 Implement GetModelMetadata method (version, last_trained_at, accuracy)
- [ ] 6.4 Implement GetModel endpoint (serve TensorFlow.js model files: .json + .bin)
- [ ] 6.5 Implement Retrain model method (trigger training job async via BackgroundService)
- [ ] 6.6 Create ModelMetadata entity (version, last_trained_at, model_path, accuracy_metrics, total_photos_used, total_products_used)
- [ ] 6.7 Add database migration for ModelMetadata table
- [ ] 6.8 Create ModelTrainingJob BackgroundService (IHostedService) - runs on existing backend CPU
- [ ] 6.9 Implement training job: fetch ProductPhoto records, download photos from storage (~2-3 min)
- [ ] 6.10 Implement data augmentation (rotation ±15°, brightness ±20%, crop - using Python script via Process.Start or ML.NET) (~3-5 min)
- [ ] 6.11 Implement MobileNetV2 fine-tuning using Python TensorFlow (invoke via Process.Start with Python script) (~20-40 min CPU)
- [ ] 6.12 Export model to TensorFlow.js format (.json + .bin files) (~1-2 min)
- [ ] 6.13 Upload model to storage with versioned path (/models/v{timestamp}/) (~1-2 min)
- [ ] 6.14 Validate exported model (check files exist, loadable, not corrupted)
- [ ] 6.15 Update ModelMetadata table ONLY if validation passes (include total_photos_used, total_products_used)
- [ ] 6.16 Preserve previous model on training failure (do not delete or overwrite)
- [ ] 6.17 Implement rollback strategy (failed training does not affect active model)
- [ ] 6.18 Create notification system (toast on next admin login, no email)
- [ ] 6.19 Implement model validation (ensure at least 1 product has ≥1 photo before training)
- [ ] 6.20 Implement job status tracking (queued, in_progress, completed, failed with progress %)
- [ ] 6.21 Prevent concurrent training jobs (return 409 Conflict if job already running)
- [ ] 6.22 Handle initial deployment (no model exists, return 404, frontend shows "No disponible")
- [ ] 6.23 Add logging for training duration and resource usage
- [ ] 6.24 Add admin warning in training confirmation: "Se recomienda ejecutar fuera de horario laboral"
- [ ] 6.25 Register service in DI container
- [ ] 6.26 NOTE: Total expected training time on CPU: 27-52 min (avg ~35-40 min), no CPU throttling

## 7. Backend API Layer - Sales Controller

- [ ] 7.1 Create SalesController
- [ ] 7.2 Add POST /api/sales endpoint (create sale with optional photo)
- [ ] 7.3 Add GET /api/sales/{id} endpoint (sale details with photo metadata)
- [ ] 7.4 Add GET /api/sales/{id}/photo/file endpoint (stream photo file via IFileStorageService)
- [ ] 7.5 Add GET /api/sales endpoint (sales history with filters: POS, product, date range, user, payment method)
- [ ] 7.6 Create DTOs: CreateSaleDto, SaleDto, SalePhotoDto, SalesHistoryFilterDto, SalesHistoryResponseDto
- [ ] 7.7 Add input validation using FluentValidation (quantity > 0, required fields, photo size <= 2MB pre-compression)
- [ ] 7.8 Add JWT authentication to all endpoints
- [ ] 7.9 Add role-based authorization (operators + admins for create, operators restricted to assigned POS for history)
- [ ] 7.10 Add operator filtering (restrict to assigned points of sale)
- [ ] 7.11 Add error handling and appropriate HTTP status codes (400 validation, 403 unauthorized POS, 404 not found)
- [ ] 7.12 Return low stock warning in response (non-blocking, informational)
- [ ] 7.13 Set appropriate Content-Type and caching headers for photo streaming

## 8. Backend API Layer - Image Recognition Controller

- [ ] 8.1 Create ImageRecognitionController
- [ ] 8.2 Add GET /api/image-recognition/model endpoint (serve TensorFlow.js model files)
- [ ] 8.3 Add GET /api/image-recognition/model/metadata endpoint (version, last_trained_at, accuracy)
- [ ] 8.4 Add GET /api/image-recognition/model/health endpoint (admin only, return metrics + alert level)
- [ ] 8.5 Add POST /api/image-recognition/retrain endpoint (admin only, trigger model retraining)
- [ ] 8.6 Add GET /api/image-recognition/retrain/status/{jobId} endpoint (check training job status)
- [ ] 8.7 Create DTOs: ModelMetadataDto, ModelHealthDto, RetrainRequestDto, RetrainStatusDto
- [ ] 8.8 Add JWT authentication to all endpoints
- [ ] 8.9 Add role-based authorization (admins only for retrain/health, all authenticated for model download)
- [ ] 8.10 Add rate limiting for model download (prevent abuse)
- [ ] 8.11 Add caching headers for model files (cache for 24 hours, check version)
- [ ] 8.12 Return 202 Accepted for async retrain request with status URL
- [ ] 8.13 Return 503 Service Unavailable if training in progress

## 8A. Backend Application Layer - Model Health Service

- [ ] 8A.1 Create IModelHealthService interface
- [ ] 8A.2 Implement ModelHealthService
- [ ] 8A.3 Implement GetModelHealth method (calculate all metrics + alert level)
- [ ] 8A.4 Calculate catalog metrics (total products, with/without photos, new since training)
- [ ] 8A.5 Calculate photo metrics (total photos, added/deleted since training, net change %)
- [ ] 8A.6 Calculate alert level using scoring system (CRITICAL, HIGH, RECOMMENDED, OK)
- [ ] 8A.7 Query ProductPhoto table to count photos added after last model training
- [ ] 8A.8 Query Product table to count products added after last model training
- [ ] 8A.9 Implement precision calculation (Phase 2 - optional, return null in MVP)
- [ ] 8A.10 Return ModelHealthDto with all metrics
- [ ] 8A.11 Add unit tests for alert level calculation (various scenarios)

## 9. Backend Testing - Sales Management

- [ ] 9.1 Write unit tests for PaymentMethodValidationService (available, not assigned, inactive)
- [ ] 9.2 Write unit tests for SalesService.CreateSale (success, insufficient stock, invalid payment method, operator not assigned)
- [ ] 9.3 Write unit tests for double stock validation (first check passes, second fails due to concurrent sale)
- [ ] 9.4 Write unit tests for concurrent sales scenario (two operators selling last unit)
- [ ] 9.5 Write unit tests for transaction rollback (inventory update fails, sale creation fails)
- [ ] 9.6 Write unit tests for price snapshot (verify current price frozen in Sale.Price)
- [ ] 9.7 Write unit tests for ImageCompressionService (JPEG conversion, quality, size validation)
- [ ] 9.8 Write unit tests for low stock warning logic (threshold check)
- [ ] 9.9 Write integration tests for SalesController.CreateSale endpoint (with Testcontainers)
- [ ] 9.10 Write integration tests for sales history with operator filtering
- [ ] 9.11 Write integration tests for transaction integrity (sale + inventory + photo atomic)
- [ ] 9.12 Write integration tests for photo upload and compression workflow
- [ ] 9.13 Write integration tests for photo file streaming endpoint (GET /api/sales/{id}/photo/file)
- [ ] 9.14 Achieve minimum 70% code coverage for sales services

## 10. Backend Testing - Image Recognition

- [ ] 10.1 Write unit tests for ImageRecognitionService.GetModelMetadata
- [ ] 10.2 Write unit tests for model validation (at least 1 product with photo)
- [ ] 10.3 Write unit tests for training failure rollback (preserve previous model)
- [ ] 10.4 Write unit tests for ModelHealthService.GetModelHealth (alert level calculation)
- [ ] 10.5 Write integration tests for model download endpoint (404 when no model, 200 when exists)
- [ ] 10.6 Write integration tests for retrain endpoint (admin authorization, async job creation)
- [ ] 10.7 Write integration tests for initial deployment scenario (no model exists)
- [ ] 10.8 Test model file serving (correct Content-Type, caching headers)
- [ ] 10.9 Test concurrent retrain requests (ensure only one job runs at a time)
- [ ] 10.10 Test model validation on export (detect corrupted files)
- [ ] 10.11 Achieve minimum 70% code coverage for image recognition services

## 11. Frontend - Sales Registration Module (Manual Entry)

- [ ] 11.1 Create sales registration page route (/sales/new)
- [ ] 11.2 Create product search/selector component (SKU or name search)
- [ ] 11.3 Add quantity input field (default 1, editable, validates > 0)
- [ ] 11.4 Create payment method selector (fetch from /api/payment-methods?pointOfSaleId={id}, show only active + assigned)
- [ ] 11.5 Add notes text area (optional, max 500 characters)
- [ ] 11.6 Add optional photo upload field (manual entry can attach photo)
- [ ] 11.7 Create sale summary display (product, quantity, price, total, payment method)
- [ ] 11.8 Add stock validation before submit (show available stock, block if insufficient)
- [ ] 11.9 Add confirmation dialog before submitting sale
- [ ] 11.10 Handle API response (success, low stock warning, validation errors)
- [ ] 11.11 Display low stock warning toast (non-blocking, "⚠️ Quedan solo X unidades")
- [ ] 11.12 Add success notification and navigation options (new sale, view history)
- [ ] 11.13 Add error handling (insufficient stock, payment method not available, operator not assigned)
- [ ] 11.14 Add loading states during sale creation

## 12. Frontend - Sales Registration with Image Recognition

- [ ] 12.1 Create sales with image recognition page route (/sales/new/image)
- [ ] 12.2 Add device compatibility check (WebGL 2.0, TensorFlow.js support, browser version)
- [ ] 12.3 Show error if device incompatible: "Tu dispositivo no es compatible. Requisitos: iOS 12+, Android 8+, navegador moderno"
- [ ] 12.4 Redirect to manual entry if compatibility check fails
- [ ] 12.5 Create image capture component (mobile camera access)
- [ ] 12.6 Add camera permission handling (request permission, show instructions)
- [ ] 12.7 Add photo content validation (min 200x200px, max aspect ratio 5:1, histogram check for black/white)
- [ ] 12.8 Display validation errors immediately (client-side, before upload)
- [ ] 12.9 Add photo preview after capture (show captured image, retake option)
- [ ] 12.10 Integrate TensorFlow.js library (install @tensorflow/tfjs)
- [ ] 12.11 Implement model loading service (download model from server, online required - NO caching in MVP)
- [ ] 12.12 Add network connectivity check (require online connection for image recognition)
- [ ] 12.13 Add model loading progress indicator (show download progress, "Descargando modelo de IA...")
- [ ] 12.14 Handle model not available (404) - show "Modelo no disponible aún, entrenar primero" + redirect manual
- [ ] 12.15 Implement image preprocessing (resize to model input size, normalize pixel values)
- [ ] 12.16 Implement inference execution (run model.predict on preprocessed image)
- [ ] 12.17 Generate 3-5 product suggestions (variable based on confidence >= 40%, max 5)
- [ ] 12.18 Create suggestions display component (product photo, SKU, name, confidence %)
- [ ] 12.19 Add confidence threshold check (40% default, redirect to manual if all below)
- [ ] 12.20 Implement fallback flow (show message "No se encontró correspondencia fiable", options: retake photo or manual entry)
- [ ] 12.21 Add network error handling (show "Se requiere conexión a internet", redirect to manual entry)
- [ ] 12.22 Add product selection from suggestions (tap to select, highlight selected)
- [ ] 12.23 Continue to quantity + payment method selection after product selected
- [ ] 12.24 Reuse manual entry components (quantity, payment method, notes, confirmation)
- [ ] 12.25 Include captured photo in sale creation request
- [ ] 12.26 Add cancel workflow (discard photo, return to home)

## 13. Frontend - Image Recognition Service

- [ ] 13.1 Create ImageRecognitionService (TypeScript service)
- [ ] 13.2 Implement device compatibility check (WebGL 2.0, TensorFlow.js backend detection)
- [ ] 13.3 Implement model download (fetch from /api/image-recognition/model, online only - no caching in MVP)
- [ ] 13.4 Handle model not found (404) gracefully - redirect to manual entry
- [ ] 13.5 Implement version check (always check on feature access, online required)
- [ ] 13.6 Implement image content validation utilities (dimensions, aspect ratio, histogram analysis)
- [ ] 13.7 Implement image preprocessing utilities (resize, normalize)
- [ ] 13.8 Implement inference method (input: image blob, output: sorted suggestions with confidence)
- [ ] 13.9 Implement variable suggestion count (3-5 based on confidence >= 40%, max 5)
- [ ] 13.10 Add error handling (model not found, inference failure, unsupported image format, network error)
- [ ] 13.11 Implement confidence threshold filtering (return only suggestions >= 40%)
- [ ] 13.12 Add fallback detection (return empty array if all confidences <40%)
- [ ] 13.13 Log inference metrics (inference time, confidence scores) for analytics (optional Phase 2)
- [ ] 13.14 Require network connectivity for image recognition (show error if offline)

## 14. Frontend - Sales History Module

- [ ] 14.1 Create sales history page route (/sales/history)
- [ ] 14.2 Create filter component (date range, product, payment method, operator - admin only)
- [ ] 14.3 Set default date range to last 30 days
- [ ] 14.4 Create sales table component (date, product, quantity, price, total, payment method, operator, photo indicator)
- [ ] 14.5 Add pagination controls (max 50 items/page)
- [ ] 14.6 Add sale details modal (show full sale info, photo if exists, inventory movement link)
- [ ] 14.7 Add photo viewer modal (enlarge sale photo, download option)
- [ ] 14.8 Apply operator filtering (show only sales from assigned POS)
- [ ] 14.9 Add export functionality (CSV download - optional for Phase 2)
- [ ] 14.10 Add loading states and error handling
- [ ] 14.11 Add empty state (no sales found for filters)

## 15. Frontend - Admin Model Management Module

- [ ] 15.1 Create model management page route (/admin/ai-model) - admin only
- [ ] 15.2 Create ModelHealthService (calculate alert level based on criteria)
- [ ] 15.3 Implement alert level calculation (CRITICAL, HIGH, RECOMMENDED, OK)
- [ ] 15.4 Create Model Status Card component (version, date, accuracy, alert)
- [ ] 15.5 Create Catalog Metrics Card component (products, with/without photos, new products)
- [ ] 15.6 Create Photo Metrics Card component (total, added, deleted, net change %)
- [ ] 15.7 Create Precision Metrics Card component (placeholder for Phase 2)
- [ ] 15.8 Add "Re-train Model Now" button (trigger POST /api/image-recognition/retrain)
- [ ] 15.9 Add confirmation dialog before retraining ("Re-training will take ~30-45 minutes. Continue?")
- [ ] 15.10 Show retraining progress with polling (status, epoch, estimated time remaining)
- [ ] 15.11 Display training history (previous versions, dates, accuracy)
- [ ] 15.12 Add training requirements validation (show error if no products have photos)
- [ ] 15.13 Implement toast notification on admin login (if alert level ≥ HIGH)
- [ ] 15.14 Add toast with "View Dashboard" button linking to /admin/ai-model
- [ ] 15.15 Hide dashboard from operators (403 Forbidden, no menu link)
- [ ] 15.16 Add loading states and error handling

## 16. Frontend Testing - Sales Management

- [ ] 16.1 Write component tests for manual sale form (React Testing Library)
- [ ] 16.2 Write component tests for product selector (search, selection)
- [ ] 16.3 Write component tests for payment method selector (filter by POS)
- [ ] 16.4 Write component tests for sale confirmation dialog
- [ ] 16.5 Write component tests for sales history table
- [ ] 16.6 Write integration tests with MSW for API mocking (create sale, get history)
- [ ] 16.7 Write E2E tests with Playwright (manual sale workflow)
- [ ] 16.8 Test low stock warning display
- [ ] 16.9 Test error handling (insufficient stock, invalid payment method)
- [ ] 16.10 Achieve minimum 70% code coverage for sales components

## 17. Frontend Testing - Image Recognition

- [ ] 17.1 Write component tests for image capture component
- [ ] 17.2 Write component tests for photo content validation (dimensions, aspect ratio, histogram)
- [ ] 17.3 Write component tests for device compatibility check
- [ ] 17.4 Write component tests for suggestions display (variable count 3-5)
- [ ] 17.5 Write component tests for confidence threshold fallback
- [ ] 17.6 Write unit tests for ImageRecognitionService (preprocessing, inference, validation)
- [ ] 17.7 Write unit tests for variable suggestion count logic (3 vs 4 vs 5 suggestions)
- [ ] 17.8 Mock TensorFlow.js for testing (avoid actual model loading in tests)
- [ ] 17.9 Write integration tests with MSW for model download (200 success, 404 no model)
- [ ] 17.10 Write E2E tests with Playwright (image recognition sale workflow)
- [ ] 17.11 Test fallback flow (low confidence → manual entry)
- [ ] 17.12 Test model not available flow (404 → show error → redirect manual)
- [ ] 17.13 Test device incompatibility flow (show error + requirements)
- [ ] 17.14 Test photo validation failures (too small, extreme aspect ratio, all black/white)
- [ ] 17.15 Achieve minimum 70% code coverage for image recognition components

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

## 19. ML Model Training and Deployment

- [ ] 19.1 Collect initial product photos from ProductPhoto table
- [ ] 19.2 Create data augmentation script (Python: rotation, brightness, crop)
- [ ] 19.3 Create MobileNetV2 fine-tuning script (Python TensorFlow/Keras)
- [ ] 19.4 Train initial model with product photos (target ≥70% top-3 accuracy)
- [ ] 19.5 Export model to TensorFlow.js format (tensorflowjs_converter)
- [ ] 19.6 Upload initial model to storage (/models/v1/)
- [ ] 19.7 Seed ModelMetadata table with initial version
- [ ] 19.8 Test model inference in browser (verify predictions work)
- [ ] 19.9 Validate model accuracy with test set
- [ ] 19.10 Document training process (README or wiki)

## 20. Documentation

- [ ] 20.1 Update API documentation (Scalar/Swagger) with sales and image recognition endpoints
- [ ] 20.2 Document sales registration workflows (manual and image recognition)
- [ ] 20.3 Document model retraining process (admin guide)
- [ ] 20.4 Document confidence threshold and fallback logic
- [ ] 20.5 Add code comments for complex business logic (transaction management, inference)
- [ ] 20.6 Update README with sales and image recognition capabilities
- [ ] 20.7 Create user guide for operators (how to use image recognition)
- [ ] 20.8 Create admin guide for model management (when to retrain, interpreting accuracy)

