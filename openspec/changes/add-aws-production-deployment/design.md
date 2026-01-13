# Design: AWS Production Deployment

## Context

JoiabagurPV requires production deployment to deliver the MVP. The current implementation uses local filesystem for file storage and Docker Compose PostgreSQL for development. Production requires:

1. **Persistent file storage**: S3 for product photos, sale photos, and ML models
2. **Managed database**: RDS PostgreSQL with automated backups
3. **Secure credentials**: No secrets in source code
4. **Automated deployment**: CI/CD via GitHub Actions

**Stakeholders:**
- Development team (implementation)
- Business owner (cost, availability)
- Operations (monitoring, maintenance)

**Constraints:**
- Minimize costs (free-tier optimization)
- Team already knows AWS
- Simple architecture (no over-engineering)
- 7-day backup retention required for database

## Goals / Non-Goals

### Goals
- Implement production-ready S3FileStorageService
- Secure credential management via AWS Secrets Manager
- Automated daily database backups with 7-day retention
- CI/CD pipeline for automated deployments
- Comprehensive documentation for future maintenance

### Non-Goals
- Multi-region deployment (single region sufficient)
- High availability (99.9%+ uptime not required)
- Staging environment (MVP direct to production)
- Custom domain setup (separate change)
- Monitoring dashboards (CloudWatch basics sufficient)

## Architecture

### Production Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              Internet                                    │
└─────────────────────────────────────────────────────────────────────────┘
                    │                                    │
                    ▼                                    ▼
        ┌───────────────────────┐           ┌───────────────────────┐
        │      CloudFront       │           │      App Runner       │
        │  (CDN for Frontend)   │           │   (.NET 10 API)       │
        │                       │           │                       │
        │  - React SPA          │           │  - JWT Auth           │
        │  - ML Model files     │           │  - REST API           │
        │  - Static assets      │           │  - Health checks      │
        └───────────────────────┘           └───────────────────────┘
                    │                                    │
                    │                    ┌───────────────┼───────────────┐
                    │                    │               │               │
                    ▼                    ▼               ▼               ▼
        ┌───────────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐
        │    S3 Bucket      │  │     RDS     │  │   Secrets   │  │     ECR     │
        │  (Frontend SPA)   │  │ PostgreSQL  │  │   Manager   │  │  (Images)   │
        │                   │  │             │  │             │  │             │
        │  - index.html     │  │ db.t3.micro │  │ - DB conn   │  │ - Backend   │
        │  - JS/CSS bundles │  │ 7-day backup│  │ - JWT key   │  │   image     │
        └───────────────────┘  └─────────────┘  └─────────────┘  └─────────────┘
                                     │
        ┌───────────────────┐        │
        │    S3 Bucket      │        │
        │  (File Storage)   │◄───────┘
        │                   │
        │  /product-photos  │
        │  /sale-photos     │
        │  /ml-models       │
        └───────────────────┘
```

### GitHub Actions CI/CD Flow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Push to   │────▶│   Build &   │────▶│   Push to   │────▶│  Deploy to  │
│    main     │     │    Test     │     │     ECR     │     │ App Runner  │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                           │
                           ▼
                    ┌─────────────┐     ┌─────────────┐
                    │   Build     │────▶│  Deploy to  │
                    │  Frontend   │     │ S3+CloudFrt │
                    └─────────────┘     └─────────────┘
```

## Decisions

### Decision 1: AWS S3 for File Storage

**What:** Use Amazon S3 for all file storage (product photos, sale photos, ML models)

**Why:**
- Team has AWS experience
- S3 SDK for .NET is mature and well-documented
- Free-tier includes 5GB storage, 20K GET, 2K PUT (sufficient for MVP)
- Pre-signed URLs provide secure temporary access
- Integrates well with CloudFront for CDN delivery

**Alternatives Considered:**
- Azure Blob Storage: Viable but team lacks experience
- DigitalOcean Spaces: S3-compatible but less ecosystem integration
- Local filesystem in container: Not persistent across deployments

### Decision 2: AWS Secrets Manager for Credentials

**What:** Use AWS Secrets Manager to store all sensitive configuration

**Why:**
- Native integration with .NET via `Amazon.Extensions.Configuration.SecretsManager`
- Automatic secret rotation capability (future use)
- Audit trail of secret access
- No secrets in source code or environment variables in GitHub

**Secrets to Store:**
```json
{
  "joiabagur-prod": {
    "ConnectionStrings:DefaultConnection": "Host=xxx;Database=xxx;...",
    "Jwt:SecretKey": "xxx",
    "Jwt:Issuer": "JoiabagurPV",
    "Jwt:Audience": "JoiabagurPV-Client",
    "Aws:S3:BucketName": "joiabagur-files-prod",
    "Aws:Region": "eu-west-1"
  }
}
```

**Alternatives Considered:**
- GitHub Secrets only: Less secure, no rotation, limited to CI/CD
- AWS Parameter Store: Free but less features than Secrets Manager
- HashiCorp Vault: Overkill for MVP, additional complexity

### Decision 3: RDS PostgreSQL with Automated Backups

**What:** Use Amazon RDS PostgreSQL with automated daily backups and 7-day retention

**Why:**
- Managed service (no DBA required)
- Free-tier: 750 hours/month db.t3.micro, 20GB storage
- Automated backups included at no additional cost
- Point-in-time recovery capability
- Encryption at rest enabled by default

**Configuration:**
- Instance: db.t3.micro (2 vCPU, 1GB RAM)
- Storage: 20GB gp2 (SSD)
- Backup window: 03:00-04:00 UTC
- Backup retention: 7 days
- Multi-AZ: No (not required for MVP)
- Encryption: Yes (AWS managed key)

**Alternatives Considered:**
- Azure Database for PostgreSQL: Team lacks experience
- Self-managed PostgreSQL on EC2: More maintenance, less reliability
- Aurora Serverless: Not in free-tier, overkill for MVP

### Decision 4: App Runner for Backend Hosting

**What:** Use AWS App Runner to host the .NET backend container

**Why:**
- Simplest container hosting option (no ECS/Fargate complexity)
- Automatic scaling and load balancing
- Built-in HTTPS with AWS-managed certificate
- Direct ECR integration
- Pay per request when idle (cost optimization)

**Configuration:**
- vCPU: 0.25 (minimum)
- Memory: 0.5GB (minimum)
- Min instances: 0 (scale to zero when idle)
- Max instances: 2 (cost control)
- Port: 8080

**Alternatives Considered:**
- ECS Fargate: More complex, better for larger workloads
- EC2: More maintenance, less flexible scaling
- Lambda + API Gateway: Cold starts problematic for .NET

### Decision 5: GitHub Actions for CI/CD

**What:** Use GitHub Actions for all CI/CD automation

**Why:**
- Already using GitHub for source control
- Free tier generous (2,000 minutes/month)
- Official AWS actions available
- Team familiarity
- Secrets management integrated

**Alternatives Considered:**
- AWS CodePipeline: Additional learning curve, more complex
- Azure DevOps: Team unfamiliar
- CircleCI/TravisCI: Additional tool, less integration

### Decision 6: Local ML Training (Admin's Machine)

**What:** ML model training executes on admin's local machine, not on the backend server. Trained model is uploaded to S3.

**Why:**
- **Free-tier compatibility:** Keeps App Runner at 0.25 vCPU, 0.5GB RAM (fully within free-tier)
- **Better hardware:** Admin laptops typically have 8-16GB RAM + GPU (vs 0.5GB server)
- **Cost control:** $0 additional AWS infrastructure for training
- **Simpler deployment:** No Python/TensorFlow in Docker image (~200MB vs ~2-3GB)
- **Infrequent operation:** Training only needed when catalog changes (1-2x/month)

**Local Training Workflow:**
```
1. Admin sees alert in /admin/ai-model dashboard
2. Admin downloads photos: python scripts/ml/download_photos.py
3. Admin trains model: python scripts/ml/train_model.py
4. Admin uploads model: python scripts/ml/upload_model.py
   OR uses dashboard "Upload Model" button
5. Backend detects new model via ModelMetadata
6. Frontend downloads new model on next access
```

**Training Scripts (Local Only - Not Deployed):**
```
scripts/ml/
├── README.md              # Step-by-step training guide
├── requirements.txt       # Python dependencies
├── download_photos.py     # Download photos from S3
├── train_model.py         # MobileNetV2 transfer learning
├── upload_model.py        # Upload model to S3
└── validate_model.py      # Test model accuracy locally
```

**Local Requirements:**
- Python 3.11+
- TensorFlow 2.15+ (CPU or GPU version)
- 8GB+ RAM (16GB recommended)
- AWS CLI configured with credentials

**Alternatives Considered:**
- **Server-side training (BackgroundService):** Rejected (requires 2-4GB RAM, exceeds free-tier)
- **AWS Lambda:** Rejected (15-minute timeout insufficient for 30-45 min training)
- **ECS Fargate Spot:** Rejected (not in free-tier, adds ~$0.50-1/training)
- **Browser-based TensorFlow.js:** Deferred to Phase 2 (complex, slower)

### Decision 7: App Runner Free-Tier Configuration

**What:** Configure App Runner with minimum resources to stay within AWS free-tier

**Why:**
- Backend only serves API + model metadata (no ML training)
- 0.25 vCPU + 0.5GB RAM sufficient for .NET API with 2-3 concurrent users
- Scale to zero when idle saves costs
- Free-tier provides significant credits for first 12 months

**Configuration:**
```json
{
  "Cpu": "0.25 vCPU",
  "Memory": "0.5 GB",
  "InstanceRoleArn": "arn:aws:iam::xxx:role/joiabagur-apprunner-instance-role"
}
```

**Cost Impact:**
- Free-tier: Fully covered for first 12 months
- Post free-tier: ~$5-15/month (minimal usage)
- Significant savings vs 2GB RAM configuration (~$15-25/month)

**Scaling Configuration:**
- Min instances: 0 (scale to zero when idle)
- Max instances: 2 (cost control, sufficient for 2-3 users)
- Scale up threshold: On incoming requests

## Implementation Details

### Production Dockerfile (.NET Only)

```dockerfile
# ============================================
# Stage 1: Build .NET application
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY src/JoiabagurPV.Domain/*.csproj JoiabagurPV.Domain/
COPY src/JoiabagurPV.Infrastructure/*.csproj JoiabagurPV.Infrastructure/
COPY src/JoiabagurPV.Application/*.csproj JoiabagurPV.Application/
COPY src/JoiabagurPV.API/*.csproj JoiabagurPV.API/
RUN dotnet restore JoiabagurPV.API/JoiabagurPV.API.csproj

# Copy everything and build
COPY src/ .
RUN dotnet publish JoiabagurPV.API/JoiabagurPV.API.csproj -c Release -o /app/publish

# ============================================
# Stage 2: Runtime (.NET only - no Python)
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install curl for health checks only
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Copy .NET application
COPY --from=build /app/publish .

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "JoiabagurPV.API.dll"]
```

**Image Size:** ~200MB (vs ~2-3GB with Python/TensorFlow)

### Local Training Scripts Structure (Not Deployed to Server)

```
scripts/ml/
├── README.md              # Step-by-step training guide
├── requirements.txt       # Python dependencies (TensorFlow, etc.)
├── download_photos.py     # Download photos from S3 to local
├── train_model.py         # MobileNetV2 transfer learning
├── upload_model.py        # Upload trained model to S3
└── validate_model.py      # Test model accuracy locally
```

These scripts run on admin's local machine, NOT on the server.

### S3FileStorageService Implementation

```csharp
public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly int _presignedUrlExpirationMinutes;
    private readonly ILogger<S3FileStorageService> _logger;

    public S3FileStorageService(
        IAmazonS3 s3Client,
        IConfiguration configuration,
        ILogger<S3FileStorageService> logger)
    {
        _s3Client = s3Client;
        _bucketName = configuration["Aws:S3:BucketName"] 
            ?? throw new ArgumentNullException("Aws:S3:BucketName not configured");
        _presignedUrlExpirationMinutes = int.Parse(
            configuration["Aws:S3:PresignedUrlExpirationMinutes"] ?? "60");
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        Stream stream, 
        string fileName, 
        string contentType, 
        string? folder = null)
    {
        var uniqueFileName = GenerateUniqueFileName(fileName);
        var key = BuildKey(folder, uniqueFileName);

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        await _s3Client.PutObjectAsync(request);
        _logger.LogInformation("File uploaded to S3: {Key}", key);

        return uniqueFileName;
    }

    public async Task<string> GetUrlAsync(string storedFileName, string? folder = null)
    {
        var key = BuildKey(folder, storedFileName);
        
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(_presignedUrlExpirationMinutes),
            Verb = HttpVerb.GET
        };

        return await Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    // Additional methods: DownloadAsync, DeleteAsync, ValidateFile
}
```

### AWS Secrets Manager Configuration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Configuration.AddSecretsManager(configurator: options =>
    {
        options.SecretFilter = entry => entry.Name == "joiabagur-prod";
        options.KeyGenerator = (_, key) => key.Replace(":", "__");
    });
}
```

### Service Registration

```csharp
// ServiceCollectionExtensions.cs
public static IServiceCollection AddFileStorage(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    var storageType = configuration["FileStorage:Type"] ?? "Local";
    
    if (storageType.Equals("S3", StringComparison.OrdinalIgnoreCase))
    {
        services.AddAWSService<IAmazonS3>();
        services.AddScoped<IFileStorageService, S3FileStorageService>();
    }
    else
    {
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
    }
    
    return services;
}
```

## Risks / Trade-offs

### Risk 1: Free-tier Expiration

**Risk:** After 12 months, AWS free-tier expires and costs increase

**Mitigation:**
- Document expected costs post-free-tier (~$25-45/month)
- Set up billing alerts at $20, $40, $60
- Review usage monthly
- Consider Reserved Instances for RDS if continuing

### Risk 2: Cold Starts with App Runner

**Risk:** App Runner with min instances = 0 causes cold start delays

**Mitigation:**
- Set min instances to 1 if response time is critical
- Accept 2-3 second cold start for cost savings
- Health check endpoint keeps container warm during active use

### Risk 3: S3 Pre-signed URL Expiration

**Risk:** Pre-signed URLs expire, causing broken images

**Mitigation:**
- Set reasonable expiration (60 minutes)
- Frontend requests fresh URL when displaying images
- Add retry logic for expired URL errors

### Risk 4: Database Connection Limits

**Risk:** RDS db.t3.micro has limited connection capacity

**Mitigation:**
- Connection pooling with 5-10 max connections
- Application-level connection management
- Monitor connection count in CloudWatch

## Migration Plan

### Phase 1: Infrastructure Setup (Day 1-2)
1. Create AWS account (if not exists)
2. Configure IAM users and roles
3. Create RDS PostgreSQL instance
4. Create S3 buckets
5. Configure Secrets Manager
6. Test connectivity

### Phase 2: Code Implementation (Day 3-4)
1. Implement S3FileStorageService
2. Add Secrets Manager configuration
3. Update service registration
4. Test locally with AWS services

### Phase 3: CI/CD Setup (Day 5)
1. Create ECR repository
2. Configure GitHub Secrets
3. Create deployment workflows
4. Test deployment pipeline

### Phase 4: Data Migration (Day 6)
1. Export local PostgreSQL data
2. Import to RDS
3. Upload existing files to S3
4. Verify data integrity

### Phase 5: Go-Live (Day 7)
1. Final testing in production
2. DNS configuration (if domain ready)
3. Monitor logs and metrics
4. Document lessons learned

### Rollback Plan

If production deployment fails:
1. Revert GitHub Actions workflow to previous version
2. App Runner automatically maintains previous deployment
3. Database: Restore from automated backup (within 7 days)
4. S3: Objects are versioned (if enabled)

## Open Questions

1. **Domain name**: Will a custom domain be configured now or later?
   - **Answer**: Later (separate change)

2. **SSL certificate**: Use AWS Certificate Manager or bring your own?
   - **Answer**: AWS Certificate Manager (free, automatic renewal)

3. **Region selection**: eu-west-1 (Ireland) or us-east-1 (N. Virginia)?
   - **Answer**: eu-west-1 (closer to Spain, GDPR compliance)

4. **Monitoring level**: Basic CloudWatch or additional monitoring?
   - **Answer**: Basic CloudWatch (sufficient for MVP)
