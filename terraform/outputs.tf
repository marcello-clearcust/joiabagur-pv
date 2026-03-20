output "ec2_public_ip" {
  description = "Elastic IP of the EC2 instance — point your DNS A record here"
  value       = aws_eip.api.public_ip
}

output "rds_endpoint" {
  description = "RDS hostname (used when importing data via SSM port forwarding)"
  value       = aws_db_instance.postgres.address
}

output "ecr_repository_url" {
  description = "ECR URI — set as ECR_REPOSITORY in GitHub Actions"
  value       = aws_ecr_repository.api.repository_url
}

output "github_actions_role_arn" {
  description = "IAM Role ARN — set as DEPLOY_ROLE_ARN in GitHub Actions (replaces AWS_ACCESS_KEY_ID/SECRET)"
  value       = aws_iam_role.github_actions.arn
}

output "ec2_instance_id" {
  description = "EC2 instance ID — set as EC2_INSTANCE_ID in GitHub Actions"
  value       = aws_instance.api.id
}

output "s3_files_bucket" {
  description = "S3 bucket name for files"
  value       = aws_s3_bucket.files.bucket
}
