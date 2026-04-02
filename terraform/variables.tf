variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "eu-west-3"
}

variable "domain_name" {
  description = "Primary domain name served by the EC2 instance (e.g. joiabagur.com)"
  type        = string
}

variable "github_repo" {
  description = "GitHub repository in owner/name format used for OIDC trust (e.g. my-org/joiabagur-pv)"
  type        = string
}

variable "db_password" {
  description = "RDS master user (postgres) password — min 8 chars; avoid characters that break connection strings in .NET"
  type        = string
  sensitive   = true
}

variable "jwt_secret_key" {
  description = "JWT signing secret (min 32 characters)"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "JWT issuer claim"
  type        = string
  default     = "JoiabagurPV"
}

variable "jwt_audience" {
  description = "JWT audience claim"
  type        = string
  default     = "JoiabagurPV"
}

variable "s3_presigned_url_expiration_minutes" {
  description = "Pre-signed URL expiration in minutes for S3 file downloads"
  type        = number
  default     = 60
}

variable "ec2_instance_type" {
  description = "EC2 instance type (t3.micro is free-tier eligible for the first 12 months)"
  type        = string
  default     = "t3.micro"
}

variable "db_instance_class" {
  description = "RDS instance class (db.t3.micro is free-tier eligible for the first 12 months)"
  type        = string
  default     = "db.t3.micro"
}

variable "ami_id" {
  description = <<-EOT
    Amazon Linux 2023 AMI for eu-west-3. Update before applying.
    Find latest: aws ec2 describe-images --owners amazon \
      --filters "Name=name,Values=al2023-ami-*-x86_64" \
      --query 'sort_by(Images, &CreationDate)[-1].ImageId' \
      --output text --region eu-west-3
  EOT
  type        = string
}

variable "key_pair_name" {
  description = "EC2 key pair for emergency SSH access. Leave empty to disable SSH (SSM is used for deployments)."
  type        = string
  default     = ""
}
