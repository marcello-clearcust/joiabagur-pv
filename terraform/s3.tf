# Files bucket only — product photos, sale photos, ML models.
# Frontend is bundled inside the Docker image; no frontend S3 bucket needed.

resource "aws_s3_bucket" "files" {
  bucket        = "jpv-files-prod"
  force_destroy = false

  tags = { Name = "jpv-files-prod" }
}

resource "aws_s3_bucket_versioning" "files" {
  bucket = aws_s3_bucket.files.id

  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "files" {
  bucket = aws_s3_bucket.files.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_public_access_block" "files" {
  bucket                  = aws_s3_bucket.files.id
  block_public_acls       = true
  ignore_public_acls      = true
  block_public_policy     = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_cors_configuration" "files" {
  bucket = aws_s3_bucket.files.id

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["GET", "PUT", "POST", "DELETE"]
    allowed_origins = ["https://${var.domain_name}", "http://localhost:5173"]
    expose_headers  = ["ETag"]
    max_age_seconds = 3000
  }
}

resource "aws_s3_bucket_lifecycle_configuration" "files" {
  bucket = aws_s3_bucket.files.id

  rule {
    id     = "expire-old-versions"
    status = "Enabled"

    noncurrent_version_expiration {
      noncurrent_days = 30
    }
  }
}
