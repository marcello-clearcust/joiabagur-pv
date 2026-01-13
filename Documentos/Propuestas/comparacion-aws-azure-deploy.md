# Comparación AWS vs Azure para Deploy de JoiabagurPV

## Resumen Ejecutivo

| Criterio | AWS | Azure | Ganador |
|----------|-----|-------|---------|
| **Experiencia del equipo** | ✅ Ya conocido | ❌ Curva de aprendizaje | AWS |
| **Free-tier para MVP** | ✅ 12 meses generosos | ⚠️ 12 meses pero más limitado | AWS |
| **Costo post-free-tier** | ⚠️ Ligeramente mayor | ✅ Ligeramente menor | Azure |
| **Facilidad de configuración** | ⚠️ Más servicios = más complejidad | ✅ App Service más integrado | Azure |
| **PostgreSQL gestionado** | ✅ RDS con backups automáticos | ✅ Azure DB con backups automáticos | Empate |
| **Object Storage** | ✅ S3 muy maduro | ✅ Blob Storage equivalente | Empate |
| **Implementación File Storage** | ✅ SDK más documentado | ⚠️ SDK funcional pero menos ejemplos | AWS |
| **GitHub Actions integración** | ✅ Excelente | ✅ Excelente | Empate |

**Recomendación: AWS** - Dada la experiencia previa, el free-tier más generoso para MVP, y la madurez del ecosistema para aplicaciones .NET + React.

---

## 1. Arquitectura Propuesta

### Componentes Necesarios

| Componente | Descripción | Requerimientos |
|------------|-------------|----------------|
| **Backend** | .NET 10 API | Container o App Service |
| **Frontend** | React SPA estática | CDN + Static hosting |
| **Base de datos** | PostgreSQL 15+ | Managed DB con backups |
| **File Storage** | Fotos de productos/ventas | Object storage con URLs firmadas |
| **ML Model Storage** | Modelo TensorFlow/ONNX | Parte del frontend build |

---

## 2. AWS - Análisis Detallado

### 2.1 Servicios Recomendados

| Servicio | Uso | Free-tier (12 meses) | Post Free-tier |
|----------|-----|---------------------|----------------|
| **AWS App Runner** | Backend .NET | 100 horas/mes (solo build) | ~$5-15/mes |
| **Amazon RDS PostgreSQL** | Base de datos | 750 hrs/mes db.t3.micro | ~$15-25/mes |
| **Amazon S3** | File storage | 5GB, 20K GET, 2K PUT | ~$1-3/mes |
| **Amazon CloudFront** | CDN frontend | 1TB transfer, 10M requests | ~$1-2/mes |
| **S3 Static Hosting** | Frontend SPA | Incluido en S3 | $0 |

**Costo estimado Free-tier:** $0/mes (12 meses)  
**Costo estimado Post Free-tier:** ~$25-45/mes

### 2.2 Arquitectura AWS

```
┌─────────────────────────────────────────────────────────────────┐
│                         Internet                                 │
└─────────────────────────────────────────────────────────────────┘
                    │                           │
                    ▼                           ▼
        ┌───────────────────┐       ┌───────────────────┐
        │   CloudFront      │       │   App Runner      │
        │   (CDN)           │       │   (.NET API)      │
        │                   │       │                   │
        │  React SPA        │       │  - REST API       │
        │  + ML Model       │       │  - JWT Auth       │
        └───────────────────┘       └───────────────────┘
                    │                           │
                    │                           ▼
                    │               ┌───────────────────┐
                    │               │   RDS PostgreSQL  │
                    │               │   (db.t3.micro)   │
                    │               │                   │
                    │               │  - Auto backups   │
                    │               │  - 7 días ret.    │
                    │               └───────────────────┘
                    │                           │
                    └───────────┬───────────────┘
                                ▼
                    ┌───────────────────┐
                    │   Amazon S3       │
                    │                   │
                    │  /product-photos  │
                    │  /sale-photos     │
                    │  /ml-models       │
                    └───────────────────┘
```

### 2.3 Pros AWS

1. **Experiencia existente** - Menor curva de aprendizaje
2. **Free-tier generoso** - 12 meses con límites adecuados para MVP
3. **App Runner simplicidad** - Deploy de containers sin gestionar infraestructura
4. **S3 madurez** - SDK extremadamente bien documentado para .NET
5. **RDS backups automáticos** - Retención de 7 días incluida sin costo adicional
6. **CloudFront + S3** - Excelente para SPAs con invalidación de cache simple
7. **Ecosistema .NET** - AWS tiene excelente soporte para .NET con AWS SDK

### 2.4 Contras AWS

1. **Complejidad inicial** - Más servicios que configurar (IAM, VPC, Security Groups)
2. **IAM learning curve** - Permisos granulares pueden ser confusos
3. **Costos post-free-tier** - Ligeramente más caro que Azure para workloads pequeños
4. **App Runner limitaciones** - Menos control que ECS/Fargate

### 2.5 Configuración de Backups (RDS)

```yaml
# Configuración automática en RDS
Backup retention: 7 days
Backup window: 03:00-04:00 UTC (configurable)
Automated backups: Enabled by default
Point-in-time recovery: Included
```

**Costo adicional:** $0 (incluido en RDS)

---

## 3. Azure - Análisis Detallado

### 3.1 Servicios Recomendados

| Servicio | Uso | Free-tier (12 meses) | Post Free-tier |
|----------|-----|---------------------|----------------|
| **Azure App Service** | Backend .NET | F1 tier gratis siempre* | B1: ~$13/mes |
| **Azure DB PostgreSQL** | Base de datos | Burstable B1ms: 750 hrs | ~$12-20/mes |
| **Azure Blob Storage** | File storage | 5GB LRS | ~$1-2/mes |
| **Azure CDN** | CDN frontend | 15GB transfer | ~$1-2/mes |
| **Static Web Apps** | Frontend SPA | 100GB gratis siempre | $0 |

*F1 tiene limitaciones: 60 min CPU/día, 1GB RAM, no custom domain SSL

**Costo estimado Free-tier:** $0/mes (con limitaciones)  
**Costo estimado Post Free-tier:** ~$20-35/mes

### 3.2 Arquitectura Azure

```
┌─────────────────────────────────────────────────────────────────┐
│                         Internet                                 │
└─────────────────────────────────────────────────────────────────┘
                    │                           │
                    ▼                           ▼
        ┌───────────────────┐       ┌───────────────────┐
        │   Azure CDN       │       │   App Service     │
        │                   │       │   (.NET API)      │
        │  Static Web App   │       │                   │
        │  React SPA        │       │  - REST API       │
        │  + ML Model       │       │  - JWT Auth       │
        └───────────────────┘       └───────────────────┘
                    │                           │
                    │                           ▼
                    │               ┌───────────────────┐
                    │               │ Azure PostgreSQL  │
                    │               │ Flexible Server   │
                    │               │                   │
                    │               │  - Auto backups   │
                    │               │  - 7 días ret.    │
                    │               └───────────────────┘
                    │                           │
                    └───────────┬───────────────┘
                                ▼
                    ┌───────────────────┐
                    │   Blob Storage    │
                    │                   │
                    │  /product-photos  │
                    │  /sale-photos     │
                    │  /ml-models       │
                    └───────────────────┘
```

### 3.3 Pros Azure

1. **App Service integración** - Todo en uno: hosting, SSL, logging, métricas
2. **Static Web Apps** - Hosting gratuito permanente para SPAs (no solo 12 meses)
3. **Costo post-free-tier menor** - ~15-20% más barato para workloads pequeños
4. **Portal unificado** - Gestión más centralizada que AWS Console
5. **.NET nativo** - Microsoft = excelente integración con .NET
6. **Backups PostgreSQL** - Incluidos sin costo adicional

### 3.4 Contras Azure

1. **Curva de aprendizaje** - Nuevo para el equipo
2. **Free-tier F1 limitado** - 60 min CPU/día puede ser insuficiente
3. **Documentación menos ejemplos** - Menos Stack Overflow answers que AWS
4. **Blob Storage SDK** - Funcional pero menos ejemplos .NET que S3
5. **Naming conventions** - Más confusos (Resource Groups, Subscriptions, etc.)

### 3.5 Configuración de Backups (Azure PostgreSQL)

```yaml
# Configuración en Flexible Server
Backup retention: 7-35 days (default 7)
Geo-redundant backup: Optional (costo extra)
Point-in-time recovery: Included
Automatic backups: Every 24 hours
```

**Costo adicional:** $0 (incluido en Flexible Server)

---

## 4. Implementación del CloudFileStorageService

### 4.1 Estado Actual

El proyecto ya tiene la abstracción `IFileStorageService` correctamente diseñada:

```csharp
// Ya implementado en el proyecto
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folder = null);
    Task<(Stream Stream, string ContentType)?> DownloadAsync(string storedFileName, string? folder = null);
    Task<bool> DeleteAsync(string storedFileName, string? folder = null);
    Task<string> GetUrlAsync(string storedFileName, string? folder = null);
    (bool IsValid, string? ErrorMessage) ValidateFile(...);
}
```

### 4.2 Implementación AWS S3

**Dependencias requeridas:**
```xml
<PackageReference Include="AWSSDK.S3" Version="3.7.x" />
```

**Esfuerzo estimado:** 4-6 horas

**Ejemplo de implementación:**

```csharp
public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly int _presignedUrlExpirationMinutes;

    public S3FileStorageService(IConfiguration configuration)
    {
        _s3Client = new AmazonS3Client(
            configuration["AWS:AccessKeyId"],
            configuration["AWS:SecretAccessKey"],
            RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
        );
        _bucketName = configuration["AWS:S3:BucketName"];
        _presignedUrlExpirationMinutes = int.Parse(
            configuration["AWS:S3:PresignedUrlExpirationMinutes"] ?? "60"
        );
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, 
        string contentType, string? folder = null)
    {
        var uniqueFileName = GenerateUniqueFileName(fileName);
        var key = folder != null ? $"{folder}/{uniqueFileName}" : uniqueFileName;

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(request);
        return uniqueFileName;
    }

    public async Task<string> GetUrlAsync(string storedFileName, string? folder = null)
    {
        var key = folder != null ? $"{folder}/{storedFileName}" : storedFileName;
        
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(_presignedUrlExpirationMinutes)
        };

        return await Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    // ... resto de métodos similares
}
```

### 4.3 Implementación Azure Blob Storage

**Dependencias requeridas:**
```xml
<PackageReference Include="Azure.Storage.Blobs" Version="12.x" />
```

**Esfuerzo estimado:** 4-6 horas

**Ejemplo de implementación:**

```csharp
public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly int _sasTokenExpirationMinutes;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        _blobServiceClient = new BlobServiceClient(
            configuration["Azure:Storage:ConnectionString"]
        );
        _containerName = configuration["Azure:Storage:ContainerName"];
        _sasTokenExpirationMinutes = int.Parse(
            configuration["Azure:Storage:SasTokenExpirationMinutes"] ?? "60"
        );
    }

    public async Task<string> UploadAsync(Stream stream, string fileName,
        string contentType, string? folder = null)
    {
        var uniqueFileName = GenerateUniqueFileName(fileName);
        var blobName = folder != null ? $"{folder}/{uniqueFileName}" : uniqueFileName;
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(stream, new BlobHttpHeaders 
        { 
            ContentType = contentType 
        });

        return uniqueFileName;
    }

    public async Task<string> GetUrlAsync(string storedFileName, string? folder = null)
    {
        var blobName = folder != null ? $"{folder}/{storedFileName}" : storedFileName;
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(_sasTokenExpirationMinutes)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    // ... resto de métodos similares
}
```

### 4.4 Comparación de Implementación

| Aspecto | AWS S3 | Azure Blob |
|---------|--------|------------|
| **Complejidad SDK** | Media | Media |
| **Documentación .NET** | ⭐⭐⭐⭐⭐ Excelente | ⭐⭐⭐⭐ Buena |
| **Ejemplos en StackOverflow** | Muy abundantes | Abundantes |
| **Pre-signed URLs** | Simple (GetPreSignedURL) | Más verboso (SAS tokens) |
| **Tiempo implementación** | 4-6 horas | 4-6 horas |
| **Testing local** | LocalStack disponible | Azurite disponible |

---

## 5. Configuración de CI/CD con GitHub Actions

### 5.1 AWS - GitHub Actions Workflow

```yaml
# .github/workflows/deploy-aws.yml
name: Deploy to AWS

on:
  push:
    branches: [main]

jobs:
  deploy-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: eu-west-1

      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v2

      - name: Build, tag, and push image to Amazon ECR
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: joiabagur-api
          IMAGE_TAG: ${{ github.sha }}
        run: |
          docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG ./backend
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG

      - name: Deploy to App Runner
        run: |
          aws apprunner update-service \
            --service-arn ${{ secrets.APP_RUNNER_SERVICE_ARN }} \
            --source-configuration "ImageRepository={ImageIdentifier=$ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG,ImageRepositoryType=ECR}"

  deploy-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install and Build
        working-directory: frontend
        run: |
          npm ci
          npm run build

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: eu-west-1

      - name: Deploy to S3
        run: |
          aws s3 sync frontend/dist s3://${{ secrets.S3_BUCKET_NAME }} --delete

      - name: Invalidate CloudFront
        run: |
          aws cloudfront create-invalidation \
            --distribution-id ${{ secrets.CLOUDFRONT_DISTRIBUTION_ID }} \
            --paths "/*"
```

### 5.2 Azure - GitHub Actions Workflow

```yaml
# .github/workflows/deploy-azure.yml
name: Deploy to Azure

on:
  push:
    branches: [main]

jobs:
  deploy-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Build and publish
        working-directory: backend
        run: |
          dotnet publish src/JoiabagurPV.API -c Release -o ./publish

      - name: Deploy to Azure App Service
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_WEBAPP_NAME }}
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: backend/publish

  deploy-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install and Build
        working-directory: frontend
        run: |
          npm ci
          npm run build

      - name: Deploy to Azure Static Web Apps
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
          action: "upload"
          app_location: "frontend"
          output_location: "dist"
```

### 5.3 Comparación CI/CD

| Aspecto | AWS | Azure |
|---------|-----|-------|
| **Complejidad workflow** | Media-Alta | Media |
| **GitHub Actions oficiales** | ✅ Sí | ✅ Sí |
| **Secrets requeridos** | 4-5 | 2-3 |
| **Tiempo deploy backend** | ~3-5 min | ~2-4 min |
| **Tiempo deploy frontend** | ~2-3 min | ~1-2 min |

---

## 6. Comparación de Free-Tier Detallada

### 6.1 AWS Free-Tier (12 meses)

| Servicio | Límite | ¿Suficiente para MVP? |
|----------|--------|----------------------|
| **EC2/App Runner** | 750 hrs/mes t2.micro | ⚠️ App Runner cobra por builds |
| **RDS PostgreSQL** | 750 hrs/mes db.t3.micro, 20GB | ✅ Perfecto |
| **S3** | 5GB, 20K GET, 2K PUT | ✅ Suficiente |
| **CloudFront** | 1TB transfer, 10M requests | ✅ Más que suficiente |
| **Data Transfer** | 100GB/mes OUT | ✅ Suficiente |

**Limitaciones importantes:**
- App Runner: El build time se cobra (~$0.007/min de build)
- RDS: Solo single-AZ en free-tier (pero suficiente para MVP)

### 6.2 Azure Free-Tier (12 meses + Always Free)

| Servicio | Límite | ¿Suficiente para MVP? |
|----------|--------|----------------------|
| **App Service F1** | 60 min CPU/día, 1GB RAM | ⚠️ Puede quedarse corto |
| **App Service B1** | 750 hrs/mes (12 meses) | ✅ Mejor opción |
| **PostgreSQL Flexible** | 750 hrs/mes B1ms, 32GB | ✅ Perfecto |
| **Blob Storage** | 5GB LRS | ✅ Suficiente |
| **Static Web Apps** | 100GB gratis **siempre** | ✅ Excelente |
| **Bandwidth** | 15GB/mes | ✅ Suficiente |

**Limitaciones importantes:**
- App Service F1: 60 min CPU/día es muy limitante para una API
- Necesitarás B1 (~$13/mes) post free-tier

### 6.3 Resumen Costos

**Nota:** ML training se ejecuta localmente en la máquina del administrador, permitiendo mantener App Runner en configuración free-tier (0.5GB RAM).

| Fase | AWS | Azure |
|------|-----|-------|
| **MVP (0-12 meses)** | ~$1-6/mes | ~$0-5/mes |
| **Post Free-tier mínimo** | ~$25-35/mes | ~$20-30/mes |
| **Post Free-tier recomendado** | ~$35-50/mes | ~$30-40/mes |

*ML training local = $0 adicional en infraestructura cloud.*

---

## 7. Consideraciones Adicionales

### 7.1 Modelo ML (TensorFlow.js/ONNX)

El modelo se genera en el backend y se sirve como asset estático. Opciones:

**Opción A: Incluir en frontend build**
- El modelo se incluye en el bundle del frontend
- Desplegado junto con la SPA en S3/Static Web Apps
- ✅ Simple, ✅ Sin costo adicional, ⚠️ Aumenta bundle size

**Opción B: Almacenar en Object Storage**
- Modelo en S3/Blob como archivo separado
- Frontend lo descarga cuando necesita
- ✅ Bundle pequeño, ✅ Actualizable sin redeploy, ⚠️ Latencia inicial

**Recomendación:** Opción B para flexibilidad en actualizaciones del modelo.

### 7.2 Dominio y SSL

| Aspecto | AWS | Azure |
|---------|-----|-------|
| **SSL gratuito** | ✅ ACM (gratuito) | ✅ App Service managed (gratuito) |
| **Custom domain** | Route 53 (~$0.50/mes) | Azure DNS (~$0.50/mes) |
| **Wildcard SSL** | ✅ Incluido en ACM | ✅ Incluido |

### 7.3 Logging y Monitoreo

| Aspecto | AWS | Azure |
|---------|-----|-------|
| **Logs básicos** | CloudWatch (gratis tier) | App Insights (gratis tier) |
| **Alertas** | CloudWatch Alarms | Azure Monitor |
| **APM** | X-Ray ($5/100K traces) | App Insights (incluido) |

**Ventaja Azure:** Application Insights incluye APM básico sin costo adicional.

---

## 8. Recomendación Final

### Decisión: **AWS**

**Razones principales:**

1. **Experiencia existente** - El equipo ya conoce AWS, lo que reduce significativamente el tiempo de implementación inicial y los posibles errores de configuración.

2. **Free-tier más predecible** - El free-tier de AWS RDS y S3 es más generoso y tiene menos gotchas que Azure App Service F1.

3. **Documentación superior para S3** - La implementación del `CloudFileStorageService` con S3 tiene más ejemplos y documentación disponible.

4. **App Runner simplicidad** - Deploy de containers sin gestionar infraestructura, perfecto para equipos pequeños.

5. **GitHub Actions madurez** - Los actions de AWS están más maduros y tienen más ejemplos en la comunidad.

### Plan de Implementación Sugerido

```
Semana 1:
├── Día 1-2: Crear cuenta AWS, configurar IAM
├── Día 3-4: Configurar RDS PostgreSQL (con backups 7 días)
└── Día 5: Configurar S3 bucket y políticas

Semana 2:
├── Día 1-2: Implementar S3FileStorageService
├── Día 3: Configurar App Runner para backend
├── Día 4: Configurar CloudFront + S3 para frontend
└── Día 5: Configurar GitHub Actions CI/CD

Semana 3:
├── Día 1-2: Testing integración completa
├── Día 3-4: Configurar dominio y SSL
└── Día 5: Documentación y go-live
```

### Próximos Pasos Inmediatos

1. **Crear infraestructura AWS** usando Terraform o CloudFormation (recomendado para reproducibilidad)
2. **Implementar `S3FileStorageService`** reemplazando el placeholder actual
3. **Configurar variables de entorno** para producción
4. **Crear GitHub Actions workflows** para CI/CD
5. **Migrar base de datos** de desarrollo a RDS

---

## 9. Anexo: Checklist de Configuración AWS

### 9.1 IAM

- [ ] Crear usuario IAM para CI/CD con permisos mínimos
- [ ] Crear rol para App Runner
- [ ] Configurar MFA en cuenta root

### 9.2 RDS PostgreSQL

- [ ] Crear instancia db.t3.micro
- [ ] Habilitar backups automáticos (7 días)
- [ ] Configurar security group
- [ ] Crear usuario de aplicación (no usar master)

### 9.3 S3

- [ ] Crear bucket con versionado habilitado
- [ ] Configurar CORS para frontend
- [ ] Crear política de bucket para pre-signed URLs
- [ ] Configurar lifecycle rules (opcional)

### 9.4 App Runner

- [ ] Crear servicio desde ECR
- [ ] Configurar variables de entorno
- [ ] Configurar auto-scaling (mínimo 0 para ahorrar)
- [ ] Configurar health check

### 9.5 CloudFront + S3 (Frontend)

- [ ] Crear bucket para frontend (static website hosting)
- [ ] Crear distribución CloudFront
- [ ] Configurar OAI para seguridad
- [ ] Configurar custom error pages (SPA routing)

### 9.6 GitHub Secrets

- [ ] `AWS_ACCESS_KEY_ID`
- [ ] `AWS_SECRET_ACCESS_KEY`
- [ ] `APP_RUNNER_SERVICE_ARN`
- [ ] `S3_BUCKET_NAME`
- [ ] `CLOUDFRONT_DISTRIBUTION_ID`
- [ ] `ECR_REPOSITORY`
