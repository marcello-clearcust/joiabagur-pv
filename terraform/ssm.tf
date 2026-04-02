# ─── Mejora 2: SSM Parameter Store ──────────────────────────────────────────
# Replaces Secrets Manager (~$0.40/secret/month) with SSM SecureString
# parameters (~$0.05/parameter/month). The .NET app reads these as environment
# variables injected at container start time by the deploy script.
#
# Naming: /jpv/prod/<EnvVarName> where EnvVarName uses __ as : separator
# (standard ASP.NET Core convention for nested config via env vars).

locals {
  # Connection string built from RDS outputs so it stays in sync automatically.
  db_connection_string = "Host=${aws_db_instance.postgres.address};Port=5432;Database=jpv;Username=postgres;Password=${var.db_password};SSL Mode=Require"
}

resource "aws_ssm_parameter" "db_connection" {
  name        = "/jpv/prod/ConnectionStrings__DefaultConnection"
  description = "PostgreSQL connection string"
  type        = "SecureString"
  value       = local.db_connection_string

  tags = { Name = "jpv-db-connection" }
}

resource "aws_ssm_parameter" "jwt_secret_key" {
  name        = "/jpv/prod/Jwt__SecretKey"
  description = "JWT signing secret"
  type        = "SecureString"
  value       = var.jwt_secret_key

  tags = { Name = "jpv-jwt-secret" }
}

resource "aws_ssm_parameter" "jwt_issuer" {
  name  = "/jpv/prod/Jwt__Issuer"
  type  = "String"
  value = var.jwt_issuer
}

resource "aws_ssm_parameter" "jwt_audience" {
  name  = "/jpv/prod/Jwt__Audience"
  type  = "String"
  value = var.jwt_audience
}

resource "aws_ssm_parameter" "s3_bucket_name" {
  name  = "/jpv/prod/Aws__S3__BucketName"
  type  = "String"
  value = aws_s3_bucket.files.bucket
}

resource "aws_ssm_parameter" "s3_presigned_expiration" {
  name  = "/jpv/prod/Aws__S3__PresignedUrlExpirationMinutes"
  type  = "String"
  value = tostring(var.s3_presigned_url_expiration_minutes)
}

resource "aws_ssm_parameter" "cors_origin" {
  name        = "/jpv/prod/Cors__AllowedOrigins__0"
  description = "Primary allowed CORS origin"
  type        = "String"
  value       = "https://${var.domain_name}"
}
