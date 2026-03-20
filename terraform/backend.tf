# Remote state in S3.
#
# PREREQUISITE: Create the bucket manually BEFORE running terraform init:
#
#   aws s3api create-bucket --bucket jpv-terraform-state --region eu-west-3 \
#     --create-bucket-configuration LocationConstraint=eu-west-3
#
#   aws s3api put-bucket-versioning --bucket jpv-terraform-state \
#     --versioning-configuration Status=Enabled
#
#   aws s3api put-public-access-block --bucket jpv-terraform-state \
#     --public-access-block-configuration \
#     BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true

terraform {
  backend "s3" {
    bucket  = "jpv-terraform-state"
    key     = "prod/terraform.tfstate"
    region  = "eu-west-3"
    encrypt = true
  }
}
