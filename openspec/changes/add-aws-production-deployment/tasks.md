# Tasks: AWS Production Deployment

## 1. AWS Infrastructure Setup

- [x] 1.1 Create AWS account or use existing account
- [x] 1.2 Create IAM user for CI/CD with minimal permissions (ECR, App Runner, S3, CloudFront)
- [x] 1.3 Create IAM role for App Runner with S3 and RDS access
- [x] 1.4 Create VPC security group for RDS (configured to allow App Runner access)
- [x] 1.5 Create RDS PostgreSQL instance (jpv-db-prod.cqykppym59mj.eu-west-3.rds.amazonaws.com)
- [x] 1.6 Create S3 bucket for file storage (jpv-files-prod)
- [x] 1.7 Configure S3 bucket CORS for frontend uploads
- [x] 1.8 Create S3 bucket for frontend hosting (jpv-frontend-prod)
- [x] 1.9 Create CloudFront distribution for frontend (E2S48GEEJKB3V - d18h1b8g3dhajw.cloudfront.net)
- [x] 1.10 Create ECR repository for backend Docker images (jpv-backend)
- [x] 1.11 Configure secrets via App Runner environment variables (alternative to Secrets Manager)

## 2. Backend Implementation - S3 & Secrets

- [x] 2.1 Add NuGet packages: AWSSDK.S3, AWSSDK.Extensions.NETCore.Setup
- [x] 2.2 Implement `S3FileStorageService` in `JoiabagurPV.Infrastructure/Services/`
- [ ] 2.3 Add unit tests for `S3FileStorageService`
- [x] 2.4 Configure secrets via environment variables in App Runner (alternative to Secrets Manager)
- [x] 2.5 Update `ServiceCollectionExtensions.cs` to register S3 service based on configuration
- [x] 2.6 Add `appsettings.Production.json` with AWS-specific configuration
- [ ] 2.7 Test S3FileStorageService locally with AWS credentials
- [x] 2.8 Implement token-based auth (localStorage + Authorization header) for cross-origin support

## 3. Production Dockerfile (.NET Only)

- [x] 3.1 Create `backend/src/JoiabagurPV.API/Dockerfile.prod` (multi-stage, .NET only)
- [x] 3.2 Configure health check endpoint in Dockerfile (/api/health)
- [x] 3.3 Configure non-root user for security
- [x] 3.4 Docker build tested via GitHub Actions CI/CD
- [x] 3.5 Container running successfully in App Runner (r9pnmccpwf.eu-west-3.awsapprunner.com)

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

- [x] 5.1 Create GitHub Secrets (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, APP_RUNNER_SERVICE_ARN, PRODUCTION_API_URL, CLOUDFRONT_DISTRIBUTION_ID)
- [x] 5.2 Create `.github/workflows/deploy-backend-aws.yml` for backend deployment (with AWS CLI upgrade and polling)
- [x] 5.3 Create `.github/workflows/deploy-frontend-aws.yml` for frontend deployment (with --legacy-peer-deps fix)
- [x] 5.4 Create App Runner service (jpv-backend via AWS Console)
- [x] 5.5 Test deployment pipeline - both backend and frontend deploying successfully
- [x] 5.6 Configure automatic deployment on push to master branch

## 6. Database Migration

- [x] 6.1 EF Core migrations applied automatically on App Runner startup
- [x] 6.2 RDS security group configured to allow App Runner access
- [x] 6.3 Apply EF Core migrations to RDS (automatic via Program.cs)
- [x] 6.4 Run database seeder for initial data (admin user + payment methods created)
- [x] 6.5 Verify data integrity - login working with admin/Admin123!
- [x] 6.6 Security group properly configured for App Runner only

## 7. File Migration

- [ ] 7.1 Upload existing product photos to S3 /product-photos folder
- [ ] 7.2 Upload existing sale photos to S3 /sale-photos folder (if any)
- [ ] 7.3 Verify file URLs work with pre-signed URL generation
- [ ] 7.4 Update database records with new S3 file references (if needed)

## 8. Documentation

- [x] 8.1 Created `Documentos/Guias/deploy-aws-production.md` with comprehensive instructions
- [ ] 8.2 Update `Documentos/arquitectura.md` with production deployment references
- [ ] 8.3 Update `README.md` with deployment documentation links
- [ ] 8.4 Document AWS resource naming conventions and tags
- [ ] 8.5 Document backup and recovery procedures
- [ ] 8.6 Document cost monitoring and billing alerts setup
- [ ] 8.7 Document local ML training procedures in `scripts/ml/README.md`

## 9. Testing & Go-Live

- [x] 9.1 Deploy to production environment (https://d18h1b8g3dhajw.cloudfront.net)
- [x] 9.2 Verify API health check endpoint (https://r9pnmccpwf.eu-west-3.awsapprunner.com/api/health)
- [x] 9.3 Test authentication flow - login working with admin/Admin123!
- [x] 9.4 S3 CORS configured, photo upload UI working (manual test pending)
- [x] 9.5 Test product management CRUD - created TEST-001 "Anillo de Prueba AWS"
- [ ] 9.6 Test on mobile devices
- [x] 9.7 Verify App Runner logs are capturing correctly
- [ ] 9.8 Set up billing alerts ($20, $40, $60 thresholds)

## 10. Custom Domain Setup (Optional)

- [ ] 10.1 Request SSL certificate in AWS ACM (us-east-1 region for CloudFront)
- [ ] 10.2 Validate certificate via DNS (add CNAME records in external DNS provider)
- [ ] 10.3 Configure CloudFront alternate domain name (e.g., pv.joiabagur.com)
- [ ] 10.4 Configure App Runner custom domain (e.g., api.joiabagur.com)
- [ ] 10.5 Add CNAME records in external DNS provider
- [ ] 10.6 Update GitHub secret PRODUCTION_API_URL with custom domain
- [ ] 10.7 Update App Runner CORS with custom frontend domain
- [ ] 10.8 Redeploy frontend and backend with new URLs

## 11. Infrastructure as Code (Optional)

- [ ] 11.1 Create Terraform configuration for all AWS resources
- [ ] 11.2 Store Terraform state in S3 with DynamoDB locking
- [ ] 11.3 Document Terraform usage in deployment guide
- [ ] 11.4 Test infrastructure recreation with Terraform
