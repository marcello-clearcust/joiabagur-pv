# Change: Add AWS Production Deployment Infrastructure

## Why

The system currently operates only in local development mode with local filesystem storage and Docker-based PostgreSQL. To deploy to production and make the application available to real users, we need to implement cloud-native services for file storage (AWS S3), managed database (AWS RDS PostgreSQL), and secure credential management (AWS Secrets Manager). This change enables the transition from development to production environment while maintaining the existing code abstraction patterns.

**Business Need:**
- Application requires production deployment for MVP delivery
- Local filesystem storage is not suitable for production (ephemeral containers)
- Database requires managed backups with 7-day retention
- Credentials must be securely managed (not in source code or plain text)
- Team has AWS experience, reducing implementation risk

**Decision Context:**
- AWS selected over Azure based on team experience and free-tier analysis (see `Documentos/Propuestas/comparacion-aws-azure-deploy.md`)
- Focus on cost optimization: AWS free-tier for 12 months, then ~$25-45/month
- No high availability requirements (downtime for maintenance acceptable)
- ML model training executed locally on admin's machine (not on server) to stay within free-tier

## What Changes

- **MODIFIED Capability**: `backend` - Implements S3FileStorageService, AWS Secrets Manager integration, and production configuration patterns
- **NEW Documentation**: Comprehensive AWS deployment guide in `Documentos/Guias/deploy-aws-production.md`
- **NEW Infrastructure**: Terraform/CloudFormation templates for reproducible infrastructure
- **NEW CI/CD**: GitHub Actions workflows for automated deployment to AWS
- **NEW Tools**: Local ML training scripts for admin to run on their machine

**File Storage (S3):**
- Implement `S3FileStorageService` replacing the placeholder `CloudFileStorageService`
- Pre-signed URLs for secure file access (60-minute expiration)
- CORS configuration for frontend uploads
- Folder structure: `/product-photos`, `/sale-photos`, `/ml-models`
- Lifecycle policies for cost optimization

**Database (RDS PostgreSQL):**
- Managed PostgreSQL 15+ on db.t3.micro (free-tier eligible)
- Automated daily backups with 7-day retention
- Connection pooling optimized for free-tier (5-10 connections max)
- Security group restricting access to App Runner only
- Encryption at rest enabled

**Secrets Management:**
- AWS Secrets Manager for all sensitive credentials
- Secrets: Database connection string, JWT secret, AWS access keys
- Environment-based configuration (development uses local .env, production uses Secrets Manager)
- No secrets in source code or GitHub repository

**ML Model Storage (S3):**
- Trained models uploaded to S3 `/ml-models` folder
- Model versioning with path: `/ml-models/v{version}/`
- Backend serves model metadata and download URLs
- Model training executed locally on admin's machine (see `add-sales-and-image-recognition` proposal)
- Local training scripts provided in `scripts/ml/`

**CI/CD Pipeline:**
- GitHub Actions workflow for backend deployment to App Runner
- GitHub Actions workflow for frontend deployment to S3 + CloudFront
- Secrets injected from GitHub Secrets (which reference AWS Secrets Manager)
- Automatic database migrations on deployment

## Scope Clarifications

**In Scope (This Change):**
- ✅ S3FileStorageService implementation
- ✅ AWS Secrets Manager integration
- ✅ RDS PostgreSQL configuration with backups
- ✅ GitHub Actions CI/CD workflows
- ✅ Comprehensive deployment documentation
- ✅ Infrastructure as Code (Terraform/CloudFormation)
- ✅ Local ML training scripts (Python, run on admin's machine)

**Out of Scope (Deferred):**
- ⏭️ Custom domain and SSL configuration (separate change)
- ⏭️ Multi-environment setup (staging) - MVP is production only
- ⏭️ Automated monitoring and alerting (CloudWatch basic is sufficient for MVP)
- ⏭️ Blue/green deployments (simple rolling updates for MVP)
- ⏭️ Server-side ML training (requires larger instances, exceeds free-tier)

## Impact

- **Affected specs**: 
  - Modified capability: `backend` (adds S3 implementation, secrets management)
  
- **Affected code**: 
  - New: `JoiabagurPV.Infrastructure/Services/S3FileStorageService.cs`
  - Modified: `JoiabagurPV.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
  - New: `JoiabagurPV.Infrastructure/Configuration/AwsSecretsManagerConfigurationProvider.cs`
  - Modified: `JoiabagurPV.API/Program.cs` (secrets configuration)
  - New: `backend/src/JoiabagurPV.API/Dockerfile.prod` (.NET only, optimized)
  - New: `.github/workflows/deploy-backend-aws.yml`
  - New: `.github/workflows/deploy-frontend-aws.yml`
  - New: `infrastructure/terraform/` or `infrastructure/cloudformation/`
  - New: `scripts/ml/` (local training scripts, not deployed to server)
  
- **Dependencies**: 
  - New NuGet package: `AWSSDK.S3` (latest version)
  - New NuGet package: `AWSSDK.SecretsManager` (latest version)
  - New NuGet package: `Amazon.Extensions.Configuration.SecretsManager` (latest version)
  - AWS account with appropriate IAM permissions
  - GitHub repository secrets configured
  - Local: Python 3.11+, TensorFlow (on admin's machine only, not on server)
  
- **Breaking changes**: None (existing IFileStorageService abstraction maintained)

- **Infrastructure**: 
  - AWS Account required
  - Services: App Runner, RDS PostgreSQL, S3, CloudFront, ECR, Secrets Manager
  - App Runner instance: 0.25 vCPU, 0.5GB RAM (free-tier eligible)
  - Docker image size: ~200MB (.NET only, no Python)
  - Estimated cost: $0/month (free-tier first 12 months), ~$25-45/month after
  - Region: eu-west-3 (Paris)
