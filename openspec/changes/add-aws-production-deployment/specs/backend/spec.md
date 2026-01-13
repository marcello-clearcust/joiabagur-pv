## MODIFIED Requirements

### Requirement: File Storage Abstraction

The backend SHALL provide an abstraction layer for file storage, supporting both local filesystem (development) and AWS S3 (production), with secure pre-signed URL generation and configurable storage providers.

#### Scenario: Storage Provider Switching
- **WHEN** deploying to different environments
- **THEN** storage implementation can be changed via configuration (`FileStorage:Type` = "Local" or "S3")
- **AND** no code changes are required

#### Scenario: Pre-signed URLs for S3
- **WHEN** client needs to access files stored in S3
- **THEN** pre-signed URLs are generated with configurable expiration (default 60 minutes)
- **AND** URLs provide temporary read access without exposing AWS credentials

#### Scenario: S3 Upload with Server-Side Encryption
- **WHEN** file is uploaded to S3
- **THEN** file is stored with AES-256 server-side encryption
- **AND** unique filename is generated with timestamp and GUID

#### Scenario: S3 Folder Organization
- **WHEN** files are uploaded to S3
- **THEN** files are organized in folders: `/product-photos`, `/sale-photos`, `/ml-models`
- **AND** folder structure mirrors local development environment

## ADDED Requirements

### Requirement: AWS Secrets Manager Integration

The backend SHALL integrate with AWS Secrets Manager for secure credential management in production environments, supporting automatic configuration loading and environment-based fallback.

#### Scenario: Production Secrets Loading
- **WHEN** application starts in Production environment
- **THEN** configuration is loaded from AWS Secrets Manager secret "joiabagur-prod"
- **AND** secrets are merged with appsettings.json configuration

#### Scenario: Development Environment Fallback
- **WHEN** application starts in Development environment
- **THEN** configuration is loaded from local appsettings.Development.json and environment variables
- **AND** AWS Secrets Manager is not accessed

#### Scenario: Required Secrets Validation
- **WHEN** production application starts
- **THEN** required secrets are validated: ConnectionStrings:DefaultConnection, Jwt:SecretKey
- **AND** application fails fast with descriptive error if secrets are missing

### Requirement: Production Database Configuration

The backend SHALL connect to AWS RDS PostgreSQL in production with optimized connection pooling and SSL encryption.

#### Scenario: RDS Connection with SSL
- **WHEN** connecting to RDS PostgreSQL in production
- **THEN** connection uses SSL/TLS encryption
- **AND** connection string includes `SSL Mode=Require`

#### Scenario: Connection Pool Optimization
- **WHEN** application handles concurrent requests
- **THEN** connection pool is limited to 5-10 connections (free-tier optimization)
- **AND** connections are reused efficiently with proper timeout handling

#### Scenario: Database Health Check
- **WHEN** health endpoint is called
- **THEN** RDS connectivity is verified
- **AND** response indicates healthy/unhealthy status with latency metrics

### Requirement: Production Deployment Configuration

The backend SHALL support production deployment via Docker container with environment-based configuration and health monitoring.

#### Scenario: Docker Production Build
- **WHEN** Docker image is built for production
- **THEN** multi-stage build creates optimized runtime image (~200MB)
- **AND** non-root user is used for security
- **AND** image contains only .NET runtime (no Python or ML dependencies)

#### Scenario: Health Check Endpoint
- **WHEN** App Runner performs health check
- **THEN** `/health` endpoint responds within timeout
- **AND** includes database and storage connectivity status

#### Scenario: CORS Production Configuration
- **WHEN** running in production environment
- **THEN** CORS is restricted to CloudFront domain and custom domains
- **AND** credentials are properly handled

### Requirement: Automated Backup Verification

The backend infrastructure SHALL maintain automated database backups with configurable retention and verification capabilities.

#### Scenario: Daily Automated Backups
- **WHEN** backup window time is reached (03:00-04:00 UTC)
- **THEN** RDS creates automated snapshot
- **AND** previous backups are retained for 7 days

#### Scenario: Point-in-Time Recovery Capability
- **WHEN** data recovery is needed
- **THEN** database can be restored to any point within retention period
- **AND** recovery creates new database instance (non-destructive)

### Requirement: ML Model Storage and Distribution

The backend SHALL store and serve ML models uploaded by administrators, supporting version management and metadata tracking.

#### Scenario: Store uploaded model
- **WHEN** administrator uploads trained model via dashboard or CLI
- **THEN** model files are stored in S3 at `/ml-models/v{version}/`
- **AND** model.json and .bin shard files are validated before storing
- **AND** ModelMetadata table is updated with version, upload date, and accuracy metrics

#### Scenario: Serve model metadata
- **WHEN** frontend requests GET /api/image-recognition/model/metadata
- **THEN** backend returns latest model version, upload date, and download URL
- **AND** response includes model size for download progress estimation

#### Scenario: Serve model files
- **WHEN** frontend requests model files
- **THEN** backend serves files from S3 via pre-signed URL or proxy
- **AND** appropriate cache headers are set for CDN optimization

#### Scenario: Preserve model history
- **WHEN** new model version is uploaded
- **THEN** previous model versions are NOT deleted
- **AND** ModelMetadata maintains history of all versions
- **AND** admin can view training history in dashboard
