# Tasks: AWS Production Deployment

## 1. AWS Infrastructure Setup

- [ ] 1.1 Create AWS account or use existing account
- [ ] 1.2 Create IAM user for CI/CD with minimal permissions (ECR, App Runner, S3)
- [ ] 1.3 Create IAM role for App Runner with S3, RDS, and Secrets Manager access
- [ ] 1.4 Create VPC security group for RDS (restrict to App Runner only)
- [ ] 1.5 Create RDS PostgreSQL instance (db.t3.micro, 20GB, 7-day backup retention)
- [ ] 1.6 Create S3 bucket for file storage (joiabagur-files-prod)
- [ ] 1.7 Configure S3 bucket CORS for frontend uploads
- [ ] 1.8 Create S3 bucket for frontend hosting (joiabagur-frontend-prod)
- [ ] 1.9 Create CloudFront distribution for frontend
- [ ] 1.10 Create ECR repository for backend Docker images
- [ ] 1.11 Create secrets in AWS Secrets Manager (joiabagur-prod)

## 2. Backend Implementation - S3 & Secrets

- [ ] 2.1 Add NuGet packages: AWSSDK.S3, AWSSDK.SecretsManager, Amazon.Extensions.Configuration.SecretsManager
- [ ] 2.2 Implement `S3FileStorageService` in `JoiabagurPV.Infrastructure/Services/`
- [ ] 2.3 Add unit tests for `S3FileStorageService`
- [ ] 2.4 Add AWS Secrets Manager configuration provider in `Program.cs`
- [ ] 2.5 Update `ServiceCollectionExtensions.cs` to register S3 service based on configuration
- [ ] 2.6 Add `appsettings.Production.json` with AWS-specific configuration
- [ ] 2.7 Test S3FileStorageService locally with AWS credentials

## 3. Production Dockerfile (.NET Only)

- [ ] 3.1 Create `backend/src/JoiabagurPV.API/Dockerfile.prod` (multi-stage, .NET only)
- [ ] 3.2 Configure health check endpoint in Dockerfile
- [ ] 3.3 Configure non-root user for security
- [ ] 3.4 Test Docker build locally (verify image size ~200MB)
- [ ] 3.5 Test container runs correctly with environment variables

## 4. Local ML Training Scripts

- [ ] 4.1 Create `scripts/ml/` directory structure
- [ ] 4.2 Create `scripts/ml/requirements.txt` with TensorFlow and dependencies
- [ ] 4.3 Implement `scripts/ml/download_photos.py` - Download photos from S3 to local
- [ ] 4.4 Implement `scripts/ml/train_model.py` - MobileNetV2 transfer learning
- [ ] 4.5 Implement `scripts/ml/upload_model.py` - Upload trained model to S3
- [ ] 4.6 Implement `scripts/ml/validate_model.py` - Test model accuracy locally
- [ ] 4.7 Create `scripts/ml/README.md` - Step-by-step training guide
- [ ] 4.8 Test full training workflow locally

## 5. CI/CD Pipeline

- [ ] 5.1 Create GitHub Secrets for AWS credentials and configuration
- [ ] 5.2 Create `.github/workflows/deploy-backend-aws.yml` for backend deployment
- [ ] 5.3 Create `.github/workflows/deploy-frontend-aws.yml` for frontend deployment
- [ ] 5.4 Create App Runner service configuration (0.25 vCPU, 0.5GB RAM)
- [ ] 5.5 Test deployment pipeline with manual trigger
- [ ] 5.6 Configure automatic deployment on push to main branch

## 6. Database Migration

- [ ] 6.1 Export schema and seed data from local PostgreSQL
- [ ] 6.2 Connect to RDS from local machine (temporarily open security group)
- [ ] 6.3 Apply EF Core migrations to RDS
- [ ] 6.4 Run database seeder for initial data
- [ ] 6.5 Verify data integrity in RDS
- [ ] 6.6 Close temporary security group access

## 7. File Migration

- [ ] 7.1 Upload existing product photos to S3 /product-photos folder
- [ ] 7.2 Upload existing sale photos to S3 /sale-photos folder (if any)
- [ ] 7.3 Verify file URLs work with pre-signed URL generation
- [ ] 7.4 Update database records with new S3 file references (if needed)

## 8. Documentation

- [ ] 8.1 Update `Documentos/Guias/deploy-aws-production.md` with final instructions
- [ ] 8.2 Update `Documentos/arquitectura.md` with production deployment references
- [ ] 8.3 Update `README.md` with deployment documentation links
- [ ] 8.4 Document AWS resource naming conventions and tags
- [ ] 8.5 Document backup and recovery procedures
- [ ] 8.6 Document cost monitoring and billing alerts setup
- [ ] 8.7 Document local ML training procedures in `scripts/ml/README.md`

## 9. Testing & Go-Live

- [ ] 9.1 Deploy to production environment
- [ ] 9.2 Verify API health check endpoint
- [ ] 9.3 Test authentication flow
- [ ] 9.4 Test file upload and download
- [ ] 9.5 Test product management CRUD
- [ ] 9.6 Test on mobile devices
- [ ] 9.7 Verify CloudWatch logs are capturing correctly
- [ ] 9.8 Set up billing alerts ($20, $40, $60 thresholds)

## 10. Infrastructure as Code (Optional but Recommended)

- [ ] 10.1 Create Terraform configuration for all AWS resources
- [ ] 10.2 Store Terraform state in S3 with DynamoDB locking
- [ ] 10.3 Document Terraform usage in deployment guide
- [ ] 10.4 Test infrastructure recreation with Terraform
