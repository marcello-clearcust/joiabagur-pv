# ML Training Scripts

This directory contains Python scripts for training the AI product classification model.

## Requirements

- Python 3.9+
- TensorFlow 2.15
- PostgreSQL connection

## Setup

```bash
cd backend/scripts/ml-training
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt
```

## Usage

The training script is invoked by the BackgroundService automatically:

```bash
python train_model.py \
  --job-id <uuid> \
  --connection-string "Host=localhost;Database=db;Username=user;Password=pass" \
  --storage-path "C:/path/to/storage" \
  --output-path "C:/path/to/models"
```

## Training Process

1. **Fetch Photos** (5%) - Query ProductPhotos table
2. **Prepare Dataset** (10-20%) - Load images, group by product
3. **Build Model** (25%) - Create MobileNetV2 architecture
4. **Train** (30-80%) - Fine-tune model (15 epochs)
5. **Export** (85-95%) - Convert to TensorFlow.js format
6. **Deploy** (95-100%) - Update ModelMetadata table

## Expected Duration

- Small dataset (50-100 photos, ~10 products): 10-15 minutes
- Medium dataset (100-500 photos, ~25 products): 20-30 minutes
- Large dataset (500-1000 photos, ~50 products): 30-45 minutes

## Output

- `models/v{timestamp}_{date}/model.json` - TensorFlow.js model
- `models/v{timestamp}_{date}/group1-shard*.bin` - Model weights
- `models/v{timestamp}_{date}/product_mapping.json` - Product ID mapping
- `models/v{timestamp}_{date}/metadata.json` - Training metadata

## Troubleshooting

- **Insufficient photos**: At least 10 photos from 2+ products required
- **Memory issues**: Reduce batch_size in train_model.py
- **Low accuracy**: Add more photos per product (recommended: 5-10 per product)

## Class Labels

Class labels use **Product SKU** (not product name) as the identifier. This ensures:
- **Stability**: SKUs are immutable; renaming a product won't break the model
- **Uniqueness**: SKUs are guaranteed unique; no collisions between similar names

> **Migration Note**: If upgrading from a version that used product names as class labels,
> you must retrain the model after deployment. Existing models will be incompatible.
