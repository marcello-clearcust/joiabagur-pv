#!/usr/bin/env python3
"""
ML Model Training Script for Jewelry Product Classification
Uses MobileNetV2 transfer learning to classify jewelry products from photos
Exports model to TensorFlow.js format for client-side inference
"""

import os
import sys
import json
import argparse
import logging
from datetime import datetime
from pathlib import Path
import psycopg2
from PIL import Image
import numpy as np
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras.applications import MobileNetV2
from tensorflow.keras.preprocessing.image import ImageDataGenerator
from sklearn.model_selection import train_test_split
import tensorflowjs as tfjs

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


class ModelTrainer:
    """Handles ML model training for product classification"""
    
    def __init__(self, job_id, connection_string, storage_path, output_path):
        self.job_id = job_id
        self.connection_string = connection_string
        self.storage_path = Path(storage_path)
        self.output_path = Path(output_path)
        self.img_size = (224, 224)  # MobileNetV2 input size
        self.batch_size = 32
        self.epochs = 15
        self.conn = None
        
    def update_job_progress(self, progress, stage, error=None):
        """Updates training job status in database"""
        try:
            if not self.conn:
                self.conn = psycopg2.connect(self.connection_string)
            
            cursor = self.conn.cursor()
            if error:
                cursor.execute("""
                    UPDATE "ModelTrainingJobs"
                    SET "Status" = 'Failed',
                        "ProgressPercentage" = %s,
                        "CurrentStage" = %s,
                        "CompletedAt" = NOW(),
                        "ErrorMessage" = %s,
                        "DurationSeconds" = EXTRACT(EPOCH FROM (NOW() - "StartedAt"))::int,
                        "UpdatedAt" = NOW()
                    WHERE "Id" = %s
                """, (progress, stage, error, self.job_id))
            else:
                cursor.execute("""
                    UPDATE "ModelTrainingJobs"
                    SET "ProgressPercentage" = %s,
                        "CurrentStage" = %s,
                        "UpdatedAt" = NOW()
                    WHERE "Id" = %s
                """, (progress, stage, self.job_id))
            
            self.conn.commit()
            logger.info(f"Job {self.job_id} progress: {progress}% - {stage}")
            
        except Exception as e:
            logger.error(f"Failed to update job progress: {e}")
    
    def fetch_product_photos(self):
        """Fetches product photos from database"""
        self.update_job_progress(5, "Fetching product photos from database")
        
        if not self.conn:
            self.conn = psycopg2.connect(self.connection_string)
        
        cursor = self.conn.cursor()
        cursor.execute("""
            SELECT 
                pp."Id",
                pp."ProductId",
                pp."FileName",
                p."SKU",
                p."Name"
            FROM "ProductPhotos" pp
            INNER JOIN "Products" p ON pp."ProductId" = p."Id"
            WHERE p."IsActive" = true
            ORDER BY pp."ProductId", pp."DisplayOrder"
        """)
        
        photos = cursor.fetchall()
        logger.info(f"Fetched {len(photos)} photos from {len(set(p[1] for p in photos))} products")
        
        return photos
    
    def download_and_prepare_dataset(self, photos):
        """Downloads photos and prepares dataset"""
        self.update_job_progress(10, f"Preparing dataset with {len(photos)} photos")
        
        # Group photos by product
        product_photos = {}
        for photo in photos:
            product_id = str(photo[1])
            sku = photo[3]
            
            if product_id not in product_photos:
                product_photos[product_id] = {
                    'sku': sku,
                    'photos': []
                }
            
            # Construct file path (assuming local storage for MVP)
            photo_path = self.storage_path / "products" / product_id / photo[2]
            if photo_path.exists():
                product_photos[product_id]['photos'].append(str(photo_path))
        
        # Filter products with at least 1 photo
        valid_products = {k: v for k, v in product_photos.items() if len(v['photos']) > 0}
        
        logger.info(f"Found {len(valid_products)} products with photos")
        
        if len(valid_products) < 2:
            raise ValueError("At least 2 products with photos required for training")
        
        return valid_products
    
    def augment_and_load_data(self, product_photos):
        """Loads and augments image data"""
        self.update_job_progress(20, "Loading and augmenting images")
        
        X = []
        y = []
        product_ids = list(product_photos.keys())
        
        for idx, product_id in enumerate(product_ids):
            photos = product_photos[product_id]['photos']
            
            for photo_path in photos:
                try:
                    # Load and resize image
                    img = Image.open(photo_path)
                    img = img.convert('RGB')
                    img = img.resize(self.img_size)
                    img_array = np.array(img) / 255.0  # Normalize
                    
                    X.append(img_array)
                    y.append(idx)  # Use index as class label
                    
                except Exception as e:
                    logger.warning(f"Failed to load {photo_path}: {e}")
        
        X = np.array(X)
        y = np.array(y)
        
        logger.info(f"Loaded {len(X)} images from {len(product_ids)} classes")
        
        # Split dataset
        X_train, X_val, y_train, y_val = train_test_split(
            X, y, test_size=0.2, random_state=42, stratify=y if len(np.unique(y)) > 1 else None
        )
        
        return X_train, X_val, y_train, y_val, product_ids
    
    def create_model(self, num_classes):
        """Creates MobileNetV2-based model for transfer learning"""
        self.update_job_progress(25, "Building MobileNetV2 model architecture")
        
        # Load pre-trained MobileNetV2
        base_model = MobileNetV2(
            input_shape=(*self.img_size, 3),
            include_top=False,
            weights='imagenet'
        )
        
        # Freeze base model layers
        base_model.trainable = False
        
        # Add custom classification layers
        model = keras.Sequential([
            base_model,
            keras.layers.GlobalAveragePooling2D(),
            keras.layers.Dense(128, activation='relu'),
            keras.layers.Dropout(0.2),
            keras.layers.Dense(num_classes, activation='softmax')
        ])
        
        model.compile(
            optimizer=keras.optimizers.Adam(learning_rate=0.001),
            loss='sparse_categorical_crossentropy',
            metrics=['accuracy']
        )
        
        logger.info(f"Model created with {num_classes} output classes")
        return model
    
    def train_model(self, model, X_train, y_train, X_val, y_val):
        """Trains the model"""
        self.update_job_progress(30, f"Training model (0/{self.epochs} epochs)")
        
        # Data augmentation for training
        datagen = ImageDataGenerator(
            rotation_range=15,
            width_shift_range=0.1,
            height_shift_range=0.1,
            brightness_range=[0.8, 1.2],
            horizontal_flip=True,
            zoom_range=0.1
        )
        
        # Custom callback to update progress
        class ProgressCallback(keras.callbacks.Callback):
            def __init__(self, trainer, total_epochs):
                super().__init__()
                self.trainer = trainer
                self.total_epochs = total_epochs
            
            def on_epoch_end(self, epoch, logs=None):
                progress = 30 + int((epoch + 1) / self.total_epochs * 50)  # 30-80% range
                stage = f"Training epoch {epoch + 1}/{self.total_epochs} - Accuracy: {logs.get('accuracy', 0):.2%}"
                self.trainer.update_job_progress(progress, stage)
        
        # Train the model
        history = model.fit(
            datagen.flow(X_train, y_train, batch_size=self.batch_size),
            validation_data=(X_val, y_val),
            epochs=self.epochs,
            callbacks=[ProgressCallback(self, self.epochs)],
            verbose=1
        )
        
        # Calculate accuracy metrics
        val_loss, val_accuracy = model.evaluate(X_val, y_val, verbose=0)
        
        logger.info(f"Training complete. Validation accuracy: {val_accuracy:.2%}")
        
        return history, val_accuracy
    
    def export_model(self, model, product_ids, val_accuracy):
        """Exports model to TensorFlow.js format"""
        self.update_job_progress(85, "Exporting model to TensorFlow.js format")
        
        # Generate version string
        version = f"v{int(datetime.utcnow().timestamp())}_{datetime.utcnow().strftime('%Y%m%d')}"
        model_dir = self.output_path / version
        model_dir.mkdir(parents=True, exist_ok=True)
        
        # Save product ID mapping
        mapping = {idx: product_id for idx, product_id in enumerate(product_ids)}
        mapping_file = model_dir / "product_mapping.json"
        with open(mapping_file, 'w') as f:
            json.dump(mapping, f, indent=2)
        
        # Save metadata
        metadata = {
            "version": version,
            "trained_at": datetime.utcnow().isoformat(),
            "num_products": len(product_ids),
            "validation_accuracy": float(val_accuracy),
            "model_architecture": "MobileNetV2",
            "input_size": list(self.img_size),
            "description": "Jewelry product classification model"
        }
        
        metadata_file = model_dir / "metadata.json"
        with open(metadata_file, 'w') as f:
            json.dump(metadata, f, indent=2)
        
        # Export to TensorFlow.js format
        tfjs.converters.save_keras_model(model, str(model_dir))
        
        logger.info(f"Model exported to {model_dir}")
        
        return version, metadata
    
    def update_model_metadata_db(self, version, metadata, num_photos):
        """Updates ModelMetadata table with new model"""
        self.update_job_progress(95, "Updating model metadata in database")
        
        cursor = self.conn.cursor()
        
        # Deactivate previous models
        cursor.execute("""
            UPDATE "ModelMetadata"
            SET "IsActive" = false,
                "UpdatedAt" = NOW()
            WHERE "IsActive" = true
        """)
        
        # Insert new model metadata
        model_path = f"models/{version}"
        accuracy_metrics = json.dumps({
            "validation_accuracy": metadata["validation_accuracy"],
            "top1": metadata["validation_accuracy"]
        })
        
        cursor.execute("""
            INSERT INTO "ModelMetadata" 
            ("Id", "Version", "TrainedAt", "ModelPath", "AccuracyMetrics", 
             "TotalPhotosUsed", "TotalProductsUsed", "IsActive", "CreatedAt", "UpdatedAt")
            VALUES (gen_random_uuid(), %s, NOW(), %s, %s, %s, %s, true, NOW(), NOW())
        """, (
            version,
            model_path,
            accuracy_metrics,
            num_photos,
            metadata["num_products"]
        ))
        
        # Update training job as completed
        cursor.execute("""
            UPDATE "ModelTrainingJobs"
            SET "Status" = 'Completed',
                "ProgressPercentage" = 100,
                "CurrentStage" = 'Training completed successfully',
                "CompletedAt" = NOW(),
                "ResultModelVersion" = %s,
                "DurationSeconds" = EXTRACT(EPOCH FROM (NOW() - "StartedAt"))::int,
                "UpdatedAt" = NOW()
            WHERE "Id" = %s
        """, (version, self.job_id))
        
        self.conn.commit()
        logger.info(f"Model metadata updated in database: {version}")
    
    def train(self):
        """Main training workflow"""
        try:
            # Start job
            if not self.conn:
                self.conn = psycopg2.connect(self.connection_string)
            
            cursor = self.conn.cursor()
            cursor.execute("""
                UPDATE "ModelTrainingJobs"
                SET "Status" = 'InProgress',
                    "StartedAt" = NOW(),
                    "ProgressPercentage" = 0,
                    "CurrentStage" = 'Initializing training',
                    "UpdatedAt" = NOW()
                WHERE "Id" = %s
            """, (self.job_id,))
            self.conn.commit()
            
            logger.info(f"Starting training job {self.job_id}")
            
            # 1. Fetch photos from database
            photos = self.fetch_product_photos()
            
            if len(photos) < 10:
                raise ValueError(f"Insufficient photos for training. Found: {len(photos)}, Required: 10+")
            
            # 2. Prepare dataset
            product_photos = self.download_and_prepare_dataset(photos)
            
            # 3. Load and augment data
            X_train, X_val, y_train, y_val, product_ids = self.augment_and_load_data(product_photos)
            
            # 4. Create model
            num_classes = len(product_ids)
            model = self.create_model(num_classes)
            
            # 5. Train model
            history, val_accuracy = self.train_model(model, X_train, y_train, X_val, y_val)
            
            # 6. Export model
            version, metadata = self.export_model(model, product_ids, val_accuracy)
            
            # 7. Update database
            self.update_model_metadata_db(version, metadata, len(photos))
            
            self.update_job_progress(100, "Training completed successfully")
            
            logger.info(f"Training job {self.job_id} completed successfully")
            logger.info(f"Model version: {version}, Validation accuracy: {val_accuracy:.2%}")
            
            return True
            
        except Exception as e:
            logger.error(f"Training failed: {e}", exc_info=True)
            self.update_job_progress(0, "Training failed", str(e))
            return False
            
        finally:
            if self.conn:
                self.conn.close()


def main():
    """Main entry point"""
    parser = argparse.ArgumentParser(description='Train jewelry product classification model')
    parser.add_argument('--job-id', required=True, help='Training job ID (UUID)')
    parser.add_argument('--connection-string', required=True, help='PostgreSQL connection string')
    parser.add_argument('--storage-path', required=True, help='Path to photo storage directory')
    parser.add_argument('--output-path', required=True, help='Path to output model directory')
    
    args = parser.parse_args()
    
    logger.info("=== ML Model Training Started ===")
    logger.info(f"Job ID: {args.job_id}")
    logger.info(f"Storage Path: {args.storage_path}")
    logger.info(f"Output Path: {args.output_path}")
    
    trainer = ModelTrainer(
        job_id=args.job_id,
        connection_string=args.connection_string,
        storage_path=args.storage_path,
        output_path=args.output_path
    )
    
    success = trainer.train()
    
    sys.exit(0 if success else 1)


if __name__ == "__main__":
    main()
