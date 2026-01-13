#!/usr/bin/env python3
"""
Mock ML Training Script for Testing
Simulates training workflow without actual ML operations
Completes in ~5 seconds for fast testing
"""

import os
import sys
import json
import argparse
import time
from datetime import datetime
from pathlib import Path

try:
    import psycopg2
except ImportError:
    print("Warning: psycopg2 not installed, using mock database operations")
    psycopg2 = None


def update_job_progress(conn, job_id, progress, stage, error=None):
    """Updates training job status"""
    if not conn:
        print(f"[MOCK] Progress: {progress}% - {stage}")
        return
        
    cursor = conn.cursor()
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
        """, (progress, stage, error, job_id))
    else:
        cursor.execute("""
            UPDATE "ModelTrainingJobs"
            SET "ProgressPercentage" = %s,
                "CurrentStage" = %s,
                "UpdatedAt" = NOW()
            WHERE "Id" = %s
        """, (progress, stage, job_id))
    
    conn.commit()
    print(f"Progress: {progress}% - {stage}")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--job-id', required=True)
    parser.add_argument('--connection-string', required=True)
    parser.add_argument('--storage-path', required=True)
    parser.add_argument('--output-path', required=True)
    
    args = parser.parse_args()
    
    print(f"=== Mock Training Started ===")
    print(f"Job ID: {args.job_id}")
    
    conn = None
    try:
        if psycopg2:
            conn = psycopg2.connect(args.connection_string)
    except Exception as e:
        print(f"Could not connect to database: {e}")
    
    try:
        # Simulate training steps
        if conn:
            cursor = conn.cursor()
            cursor.execute("""
                UPDATE "ModelTrainingJobs"
                SET "Status" = 'InProgress',
                    "StartedAt" = NOW(),
                    "UpdatedAt" = NOW()
                WHERE "Id" = %s
            """, (args.job_id,))
            conn.commit()
        
        update_job_progress(conn, args.job_id, 10, "Fetching photos")
        time.sleep(0.5)
        
        update_job_progress(conn, args.job_id, 30, "Building model")
        time.sleep(0.5)
        
        update_job_progress(conn, args.job_id, 50, "Training epoch 5/10")
        time.sleep(1)
        
        update_job_progress(conn, args.job_id, 70, "Training epoch 8/10")
        time.sleep(1)
        
        update_job_progress(conn, args.job_id, 85, "Exporting model")
        time.sleep(0.5)
        
        # Create mock model files
        version = f"v{int(datetime.utcnow().timestamp())}_{datetime.utcnow().strftime('%Y%m%d')}"
        output_path = Path(args.output_path)
        model_dir = output_path / version
        model_dir.mkdir(parents=True, exist_ok=True)
        
        # Create mock model.json
        model_json = {
            "format": "graph-model",
            "generatedBy": "mock-training",
            "convertedBy": "TensorFlow.js Converter",
            "modelTopology": {"node": []},
            "weightsManifest": [
                {
                    "paths": ["group1-shard1of1.bin"],
                    "weights": []
                }
            ]
        }
        
        with open(model_dir / "model.json", 'w') as f:
            json.dump(model_json, f)
        
        # Create mock weight file
        with open(model_dir / "group1-shard1of1.bin", 'wb') as f:
            f.write(b'\x00' * 1024)  # 1KB mock binary
        
        # Create product mapping
        mapping = {"0": "product-1", "1": "product-2"}
        with open(model_dir / "product_mapping.json", 'w') as f:
            json.dump(mapping, f)
        
        update_job_progress(conn, args.job_id, 95, "Updating database")
        time.sleep(0.5)
        
        if conn:
            # Update ModelMetadata
            cursor = conn.cursor()
            cursor.execute("""
                UPDATE "ModelMetadata"
                SET "IsActive" = false
                WHERE "IsActive" = true
            """)
            
            cursor.execute("""
                INSERT INTO "ModelMetadata" 
                ("Id", "Version", "TrainedAt", "ModelPath", "AccuracyMetrics", 
                 "TotalPhotosUsed", "TotalProductsUsed", "IsActive", "CreatedAt", "UpdatedAt")
                VALUES (gen_random_uuid(), %s, NOW(), %s, %s, %s, %s, true, NOW(), NOW())
            """, (
                version,
                f"models/{version}",
                '{"validation_accuracy": 0.85}',
                10,
                2
            ))
            
            cursor.execute("""
                UPDATE "ModelTrainingJobs"
                SET "Status" = 'Completed',
                    "ProgressPercentage" = 100,
                    "CurrentStage" = 'Training completed',
                    "CompletedAt" = NOW(),
                    "ResultModelVersion" = %s,
                    "DurationSeconds" = EXTRACT(EPOCH FROM (NOW() - "StartedAt"))::int,
                    "UpdatedAt" = NOW()
                WHERE "Id" = %s
            """, (version, args.job_id))
            
            conn.commit()
        
        print(f"Mock training completed successfully: {version}")
        sys.exit(0)
        
    except Exception as e:
        print(f"Mock training failed: {e}")
        if conn:
            update_job_progress(conn, args.job_id, 0, "Failed", str(e))
        sys.exit(1)
    finally:
        if conn:
            conn.close()


if __name__ == "__main__":
    main()
