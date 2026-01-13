# Guía de Deploy en AWS - JoiabagurPV

## Índice

1. [Introducción](#1-introducción)
2. [Prerrequisitos](#2-prerrequisitos)
3. [Arquitectura de Producción](#3-arquitectura-de-producción)
4. [Configuración de AWS](#4-configuración-de-aws)
5. [Configuración de Secrets Manager](#5-configuración-de-secrets-manager)
6. [Configuración de RDS PostgreSQL](#6-configuración-de-rds-postgresql)
7. [Configuración de S3](#7-configuración-de-s3)
8. [Configuración de App Runner](#8-configuración-de-app-runner)
9. [Configuración de CloudFront](#9-configuración-de-cloudfront)
10. [Configuración de CI/CD](#10-configuración-de-cicd)
11. [Migración de Datos](#11-migración-de-datos)
12. [Verificación y Go-Live](#12-verificación-y-go-live)
13. [Monitoreo y Mantenimiento](#13-monitoreo-y-mantenimiento)
14. [Backup y Recuperación](#14-backup-y-recuperación)
15. [Costos y Optimización](#15-costos-y-optimización)
16. [Troubleshooting](#16-troubleshooting)

---

## 1. Introducción

Esta guía proporciona instrucciones paso a paso para desplegar JoiabagurPV en AWS. La arquitectura elegida optimiza costos utilizando el free-tier de AWS durante los primeros 12 meses.

### Documentos Relacionados

- **Análisis de decisión**: `Documentos/Propuestas/comparacion-aws-azure-deploy.md`
- **Propuesta OpenSpec**: `openspec/changes/add-aws-production-deployment/`
- **Arquitectura general**: `Documentos/arquitectura.md`

### Servicios AWS Utilizados

| Servicio | Uso | Free-tier |
|----------|-----|-----------|
| App Runner | Backend .NET | ✅ Incluido (0.5GB RAM) |
| RDS PostgreSQL | Base de datos | 750 hrs/mes db.t3.micro |
| S3 | File storage + Frontend + ML Models | 5GB, 20K GET, 2K PUT |
| CloudFront | CDN | 1TB transfer, 10M requests |
| ECR | Docker images | 500MB storage |
| Secrets Manager | Credenciales | $0.40/secret/mes* |

*Secrets Manager no tiene free-tier, pero el costo es mínimo (~$1-2/mes total)

### Consideraciones Especiales: ML Training

El entrenamiento de modelos ML se ejecuta **localmente en la máquina del administrador**, no en el servidor:

- **Dockerfile**: Solo .NET 10, sin Python ni TensorFlow
- **Tamaño de imagen**: ~200MB (optimizado)
- **RAM del servidor**: 0.5GB (free-tier)
- **Tiempo de build CI**: ~2-3 minutos
- **Entrenamiento local**: Admin ejecuta scripts Python en su laptop (8-16GB RAM)
- **Scripts proporcionados**: `scripts/ml/` con herramientas de training y upload

---

## 2. Prerrequisitos

### 2.1 Cuenta AWS

1. Crear cuenta AWS en https://aws.amazon.com
2. Activar MFA en cuenta root
3. Crear usuario IAM para administración diaria

### 2.2 Herramientas Locales

```bash
# AWS CLI v2
# Windows (winget)
winget install Amazon.AWSCLI

# Verificar instalación
aws --version

# Configurar credenciales
aws configure
# AWS Access Key ID: [tu-access-key]
# AWS Secret Access Key: [tu-secret-key]
# Default region name: eu-west-1
# Default output format: json
```

### 2.3 Permisos IAM Necesarios

Crear usuario IAM `joiabagur-deploy` con las siguientes políticas:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "ecr:*",
        "apprunner:*",
        "s3:*",
        "cloudfront:*",
        "secretsmanager:GetSecretValue",
        "secretsmanager:DescribeSecret",
        "rds:DescribeDBInstances",
        "logs:*"
      ],
      "Resource": "*"
    }
  ]
}
```

---

## 3. Arquitectura de Producción

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              Internet                                    │
└─────────────────────────────────────────────────────────────────────────┘
                    │                                    │
                    ▼                                    ▼
        ┌───────────────────────┐           ┌───────────────────────┐
        │      CloudFront       │           │      App Runner       │
        │  (Frontend CDN)       │           │   (.NET 10 API)       │
        │                       │           │                       │
        │  joiabagur.com        │           │  api.joiabagur.com    │
        └───────────────────────┘           └───────────────────────┘
                    │                                    │
                    ▼                    ┌───────────────┼───────────────┐
        ┌───────────────────┐            │               │               │
        │  S3: Frontend     │            ▼               ▼               ▼
        │                   │   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐
        │  /index.html      │   │     RDS     │  │   Secrets   │  │  S3: Files  │
        │  /assets/*        │   │ PostgreSQL  │  │   Manager   │  │             │
        └───────────────────┘   └─────────────┘  └─────────────┘  └─────────────┘
```

### Naming Convention

| Recurso | Nombre |
|---------|--------|
| RDS Instance | `joiabagur-db-prod` |
| S3 Files | `joiabagur-files-prod` |
| S3 Frontend | `joiabagur-frontend-prod` |
| ECR Repository | `joiabagur-api` |
| App Runner Service | `joiabagur-api-prod` |
| Secrets Manager | `joiabagur-prod` |
| CloudFront Distribution | (auto-generated ID) |

---

## 4. Configuración de AWS

### 4.1 Selección de Región

Usaremos **eu-west-1 (Ireland)** por:
- Proximidad geográfica a España
- Cumplimiento GDPR
- Disponibilidad completa de servicios

```bash
# Verificar región configurada
aws configure get region
# Debería mostrar: eu-west-1
```

### 4.2 Crear VPC (Opcional - Usar Default VPC)

Para simplicidad del MVP, usaremos la VPC por defecto. Para producción avanzada, considerar VPC personalizada.

```bash
# Verificar VPC por defecto
aws ec2 describe-vpcs --filters "Name=isDefault,Values=true"
```

---

## 5. Configuración de Secrets Manager

### 5.1 Crear Secreto

```bash
# Crear secreto con todas las credenciales necesarias
aws secretsmanager create-secret \
  --name "joiabagur-prod" \
  --description "Production secrets for JoiabagurPV" \
  --secret-string '{
    "ConnectionStrings__DefaultConnection": "Host=YOUR_RDS_ENDPOINT;Port=5432;Database=joiabagur_prod;Username=joiabagur_admin;Password=YOUR_SECURE_PASSWORD;SSL Mode=Require",
    "Jwt__SecretKey": "YOUR_JWT_SECRET_KEY_MIN_32_CHARS_LONG_CHANGE_THIS",
    "Jwt__Issuer": "JoiabagurPV",
    "Jwt__Audience": "JoiabagurPV-Client",
    "Jwt__ExpirationMinutes": "480",
    "Aws__S3__BucketName": "joiabagur-files-prod",
    "Aws__Region": "eu-west-1",
    "FileStorage__Type": "S3",
    "FileStorage__PresignedUrlExpirationMinutes": "60"
  }' \
  --region eu-west-1
```

### 5.2 Actualizar Secreto (Cuando sea necesario)

```bash
# Actualizar valores específicos
aws secretsmanager update-secret \
  --secret-id "joiabagur-prod" \
  --secret-string '{...valores actualizados...}'
```

### 5.3 Verificar Secreto

```bash
# Ver valor del secreto (solo en entorno seguro)
aws secretsmanager get-secret-value \
  --secret-id "joiabagur-prod" \
  --query 'SecretString' \
  --output text | jq .
```

---

## 6. Configuración de RDS PostgreSQL

### 6.1 Crear Security Group para RDS

```bash
# Obtener VPC ID por defecto
VPC_ID=$(aws ec2 describe-vpcs --filters "Name=isDefault,Values=true" --query 'Vpcs[0].VpcId' --output text)

# Crear security group
aws ec2 create-security-group \
  --group-name "joiabagur-rds-sg" \
  --description "Security group for JoiabagurPV RDS" \
  --vpc-id $VPC_ID

# Obtener el ID del security group creado
SG_ID=$(aws ec2 describe-security-groups --filters "Name=group-name,Values=joiabagur-rds-sg" --query 'SecurityGroups[0].GroupId' --output text)

# Permitir acceso PostgreSQL desde cualquier IP (temporalmente para setup)
# NOTA: Después de configurar App Runner, restringir a solo App Runner
aws ec2 authorize-security-group-ingress \
  --group-id $SG_ID \
  --protocol tcp \
  --port 5432 \
  --cidr 0.0.0.0/0

echo "Security Group ID: $SG_ID"
```

### 6.2 Crear Subnet Group

```bash
# Obtener subnets de la VPC por defecto
SUBNET_IDS=$(aws ec2 describe-subnets --filters "Name=vpc-id,Values=$VPC_ID" --query 'Subnets[*].SubnetId' --output text | tr '\t' ',')

# Crear subnet group para RDS
aws rds create-db-subnet-group \
  --db-subnet-group-name "joiabagur-db-subnet-group" \
  --db-subnet-group-description "Subnet group for JoiabagurPV database" \
  --subnet-ids $(echo $SUBNET_IDS | tr ',' ' ')
```

### 6.3 Crear Instancia RDS

```bash
aws rds create-db-instance \
  --db-instance-identifier "joiabagur-db-prod" \
  --db-instance-class "db.t3.micro" \
  --engine "postgres" \
  --engine-version "15.4" \
  --master-username "joiabagur_admin" \
  --master-user-password "YOUR_SECURE_PASSWORD_CHANGE_THIS" \
  --allocated-storage 20 \
  --storage-type "gp2" \
  --db-name "joiabagur_prod" \
  --vpc-security-group-ids $SG_ID \
  --db-subnet-group-name "joiabagur-db-subnet-group" \
  --backup-retention-period 7 \
  --preferred-backup-window "03:00-04:00" \
  --preferred-maintenance-window "Mon:04:00-Mon:05:00" \
  --storage-encrypted \
  --publicly-accessible \
  --no-multi-az \
  --deletion-protection \
  --tags Key=Project,Value=JoiabagurPV Key=Environment,Value=Production
```

### 6.4 Esperar a que RDS esté disponible

```bash
# Este comando espera hasta que la instancia esté disponible (puede tardar 5-10 minutos)
aws rds wait db-instance-available --db-instance-identifier "joiabagur-db-prod"

# Obtener el endpoint
aws rds describe-db-instances \
  --db-instance-identifier "joiabagur-db-prod" \
  --query 'DBInstances[0].Endpoint.Address' \
  --output text
```

### 6.5 Actualizar Secrets Manager con Endpoint RDS

Una vez obtenido el endpoint de RDS, actualizar el secreto en Secrets Manager con la connection string correcta.

---

## 7. Configuración de S3

### 7.1 Crear Bucket para Files (Fotos)

```bash
# Crear bucket para archivos
aws s3api create-bucket \
  --bucket "joiabagur-files-prod" \
  --region eu-west-1 \
  --create-bucket-configuration LocationConstraint=eu-west-1

# Habilitar versionado (para recuperación accidental)
aws s3api put-bucket-versioning \
  --bucket "joiabagur-files-prod" \
  --versioning-configuration Status=Enabled

# Bloquear acceso público (usamos pre-signed URLs)
aws s3api put-public-access-block \
  --bucket "joiabagur-files-prod" \
  --public-access-block-configuration \
  "BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true"

# Configurar encriptación por defecto
aws s3api put-bucket-encryption \
  --bucket "joiabagur-files-prod" \
  --server-side-encryption-configuration '{
    "Rules": [
      {
        "ApplyServerSideEncryptionByDefault": {
          "SSEAlgorithm": "AES256"
        }
      }
    ]
  }'
```

### 7.2 Configurar CORS para el Bucket de Files

```bash
aws s3api put-bucket-cors \
  --bucket "joiabagur-files-prod" \
  --cors-configuration '{
    "CORSRules": [
      {
        "AllowedHeaders": ["*"],
        "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
        "AllowedOrigins": ["https://*.cloudfront.net", "http://localhost:*"],
        "ExposeHeaders": ["ETag"],
        "MaxAgeSeconds": 3000
      }
    ]
  }'
```

### 7.3 Crear Bucket para Frontend

```bash
# Crear bucket para frontend
aws s3api create-bucket \
  --bucket "joiabagur-frontend-prod" \
  --region eu-west-1 \
  --create-bucket-configuration LocationConstraint=eu-west-1

# Configurar como sitio web estático
aws s3 website s3://joiabagur-frontend-prod/ \
  --index-document index.html \
  --error-document index.html
```

### 7.4 Crear Carpetas en Bucket de Files

```bash
# Crear estructura de carpetas
echo "" | aws s3 cp - s3://joiabagur-files-prod/product-photos/.keep
echo "" | aws s3 cp - s3://joiabagur-files-prod/sale-photos/.keep
echo "" | aws s3 cp - s3://joiabagur-files-prod/ml-models/.keep
```

---

## 8. Configuración de App Runner

### 8.1 Crear Repositorio ECR

```bash
# Crear repositorio ECR
aws ecr create-repository \
  --repository-name "joiabagur-api" \
  --image-scanning-configuration scanOnPush=true \
  --region eu-west-1

# Obtener URI del repositorio
ECR_URI=$(aws ecr describe-repositories --repository-names "joiabagur-api" --query 'repositories[0].repositoryUri' --output text)
echo "ECR URI: $ECR_URI"
```

### 8.2 Push de Imagen Docker

```bash
# Login a ECR
aws ecr get-login-password --region eu-west-1 | docker login --username AWS --password-stdin $ECR_URI

# Build de la imagen (desde directorio backend)
cd backend
docker build -t joiabagur-api:latest -f src/JoiabagurPV.API/Dockerfile .

# Tag y push
docker tag joiabagur-api:latest $ECR_URI:latest
docker push $ECR_URI:latest
```

### 8.3 Crear Rol IAM para App Runner

```bash
# Crear política para App Runner
cat > /tmp/apprunner-trust-policy.json << 'EOF'
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "build.apprunner.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF

# Crear rol
aws iam create-role \
  --role-name "joiabagur-apprunner-role" \
  --assume-role-policy-document file:///tmp/apprunner-trust-policy.json

# Adjuntar política para ECR
aws iam attach-role-policy \
  --role-name "joiabagur-apprunner-role" \
  --policy-arn "arn:aws:iam::aws:policy/service-role/AWSAppRunnerServicePolicyForECRAccess"
```

### 8.4 Crear Rol de Instancia para App Runner

```bash
# Crear política para acceso a recursos
cat > /tmp/apprunner-instance-policy.json << 'EOF'
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "secretsmanager:GetSecretValue",
        "secretsmanager:DescribeSecret"
      ],
      "Resource": "arn:aws:secretsmanager:eu-west-1:*:secret:joiabagur-prod*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject",
        "s3:DeleteObject",
        "s3:ListBucket"
      ],
      "Resource": [
        "arn:aws:s3:::joiabagur-files-prod",
        "arn:aws:s3:::joiabagur-files-prod/*"
      ]
    }
  ]
}
EOF

# Trust policy para tasks
cat > /tmp/apprunner-instance-trust.json << 'EOF'
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "tasks.apprunner.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF

# Crear rol
aws iam create-role \
  --role-name "joiabagur-apprunner-instance-role" \
  --assume-role-policy-document file:///tmp/apprunner-instance-trust.json

# Crear y adjuntar política
aws iam put-role-policy \
  --role-name "joiabagur-apprunner-instance-role" \
  --policy-name "joiabagur-instance-policy" \
  --policy-document file:///tmp/apprunner-instance-policy.json
```

### 8.5 Crear Servicio App Runner

```bash
# Obtener ARNs de roles
ACCESS_ROLE_ARN=$(aws iam get-role --role-name "joiabagur-apprunner-role" --query 'Role.Arn' --output text)
INSTANCE_ROLE_ARN=$(aws iam get-role --role-name "joiabagur-apprunner-instance-role" --query 'Role.Arn' --output text)
ACCOUNT_ID=$(aws sts get-caller-identity --query 'Account' --output text)

# Crear servicio App Runner
aws apprunner create-service \
  --service-name "joiabagur-api-prod" \
  --source-configuration '{
    "AuthenticationConfiguration": {
      "AccessRoleArn": "'$ACCESS_ROLE_ARN'"
    },
    "AutoDeploymentsEnabled": false,
    "ImageRepository": {
      "ImageIdentifier": "'$ACCOUNT_ID'.dkr.ecr.eu-west-1.amazonaws.com/joiabagur-api:latest",
      "ImageRepositoryType": "ECR",
      "ImageConfiguration": {
        "Port": "8080",
        "RuntimeEnvironmentVariables": {
          "ASPNETCORE_ENVIRONMENT": "Production",
          "AWS_REGION": "eu-west-1"
        }
      }
    }
  }' \
  --instance-configuration '{
    "Cpu": "0.25 vCPU",
    "Memory": "0.5 GB",
    "InstanceRoleArn": "'$INSTANCE_ROLE_ARN'"
  }' \
  --health-check-configuration '{
    "Protocol": "HTTP",
    "Path": "/health",
    "Interval": 10,
    "Timeout": 5,
    "HealthyThreshold": 1,
    "UnhealthyThreshold": 5
  }' \
  --auto-scaling-configuration-arn "arn:aws:apprunner:eu-west-1:'$ACCOUNT_ID':autoscalingconfiguration/DefaultConfiguration/1/00000000000000000000000000000001" \
  --region eu-west-1
```

### 8.6 Obtener URL del Servicio

```bash
# Esperar a que el servicio esté running (puede tardar 2-5 minutos)
aws apprunner describe-service \
  --service-arn $(aws apprunner list-services --query 'ServiceSummaryList[?ServiceName==`joiabagur-api-prod`].ServiceArn' --output text) \
  --query 'Service.ServiceUrl' \
  --output text
```

---

## 9. Configuración de CloudFront

### 9.1 Crear Origin Access Identity

```bash
# Crear OAI para acceso seguro a S3
aws cloudfront create-cloud-front-origin-access-identity \
  --cloud-front-origin-access-identity-config '{
    "CallerReference": "joiabagur-frontend-oai",
    "Comment": "OAI for JoiabagurPV frontend"
  }'

# Guardar el ID del OAI
OAI_ID=$(aws cloudfront list-cloud-front-origin-access-identities --query 'CloudFrontOriginAccessIdentityList.Items[?Comment==`OAI for JoiabagurPV frontend`].Id' --output text)
echo "OAI ID: $OAI_ID"
```

### 9.2 Actualizar Política del Bucket Frontend

```bash
ACCOUNT_ID=$(aws sts get-caller-identity --query 'Account' --output text)

aws s3api put-bucket-policy \
  --bucket "joiabagur-frontend-prod" \
  --policy '{
    "Version": "2012-10-17",
    "Statement": [
      {
        "Sid": "AllowCloudFrontOAI",
        "Effect": "Allow",
        "Principal": {
          "AWS": "arn:aws:iam::cloudfront:user/CloudFront Origin Access Identity '$OAI_ID'"
        },
        "Action": "s3:GetObject",
        "Resource": "arn:aws:s3:::joiabagur-frontend-prod/*"
      }
    ]
  }'
```

### 9.3 Crear Distribución CloudFront

```bash
# Crear archivo de configuración
cat > /tmp/cloudfront-config.json << 'EOF'
{
  "CallerReference": "joiabagur-frontend-dist",
  "Comment": "JoiabagurPV Frontend Distribution",
  "DefaultRootObject": "index.html",
  "Origins": {
    "Quantity": 1,
    "Items": [
      {
        "Id": "S3-joiabagur-frontend-prod",
        "DomainName": "joiabagur-frontend-prod.s3.eu-west-1.amazonaws.com",
        "S3OriginConfig": {
          "OriginAccessIdentity": "origin-access-identity/cloudfront/OAI_ID_PLACEHOLDER"
        }
      }
    ]
  },
  "DefaultCacheBehavior": {
    "TargetOriginId": "S3-joiabagur-frontend-prod",
    "ViewerProtocolPolicy": "redirect-to-https",
    "AllowedMethods": {
      "Quantity": 2,
      "Items": ["GET", "HEAD"]
    },
    "CachedMethods": {
      "Quantity": 2,
      "Items": ["GET", "HEAD"]
    },
    "Compress": true,
    "ForwardedValues": {
      "QueryString": false,
      "Cookies": {
        "Forward": "none"
      }
    },
    "MinTTL": 0,
    "DefaultTTL": 86400,
    "MaxTTL": 31536000
  },
  "CustomErrorResponses": {
    "Quantity": 1,
    "Items": [
      {
        "ErrorCode": 404,
        "ResponsePagePath": "/index.html",
        "ResponseCode": "200",
        "ErrorCachingMinTTL": 300
      }
    ]
  },
  "PriceClass": "PriceClass_100",
  "Enabled": true,
  "ViewerCertificate": {
    "CloudFrontDefaultCertificate": true
  }
}
EOF

# Reemplazar OAI ID
sed -i "s/OAI_ID_PLACEHOLDER/$OAI_ID/g" /tmp/cloudfront-config.json

# Crear distribución
aws cloudfront create-distribution \
  --distribution-config file:///tmp/cloudfront-config.json
```

### 9.4 Obtener URL de CloudFront

```bash
# La URL será algo como: d1234567890.cloudfront.net
aws cloudfront list-distributions \
  --query 'DistributionList.Items[?Comment==`JoiabagurPV Frontend Distribution`].DomainName' \
  --output text
```

---

## 10. Configuración de CI/CD

### 10.1 Configurar GitHub Secrets

En el repositorio de GitHub, ir a **Settings > Secrets and variables > Actions** y crear los siguientes secrets:

| Secret Name | Descripción |
|-------------|-------------|
| `AWS_ACCESS_KEY_ID` | Access key del usuario IAM joiabagur-deploy |
| `AWS_SECRET_ACCESS_KEY` | Secret key del usuario IAM |
| `AWS_REGION` | `eu-west-1` |
| `ECR_REPOSITORY` | URI completo del repositorio ECR |
| `APP_RUNNER_SERVICE_ARN` | ARN del servicio App Runner |
| `S3_FRONTEND_BUCKET` | `joiabagur-frontend-prod` |
| `CLOUDFRONT_DISTRIBUTION_ID` | ID de la distribución CloudFront |

### 10.2 Workflow de Deploy Backend

Crear archivo `.github/workflows/deploy-backend-aws.yml`:

```yaml
name: Deploy Backend to AWS

on:
  push:
    branches: [main]
    paths:
      - 'backend/**'
      - '.github/workflows/deploy-backend-aws.yml'
  workflow_dispatch:

env:
  AWS_REGION: eu-west-1

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: ${{ env.AWS_REGION }}

      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v2

      - name: Build, tag, and push image to Amazon ECR
        id: build-image
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: joiabagur-api
          IMAGE_TAG: ${{ github.sha }}
        working-directory: backend
        run: |
          docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG -f src/JoiabagurPV.API/Dockerfile .
          docker tag $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG $ECR_REGISTRY/$ECR_REPOSITORY:latest
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:latest
          echo "image=$ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG" >> $GITHUB_OUTPUT

      - name: Deploy to App Runner
        run: |
          aws apprunner start-deployment \
            --service-arn ${{ secrets.APP_RUNNER_SERVICE_ARN }}

      - name: Wait for deployment
        run: |
          echo "Waiting for deployment to complete..."
          sleep 60
          aws apprunner describe-service \
            --service-arn ${{ secrets.APP_RUNNER_SERVICE_ARN }} \
            --query 'Service.Status' \
            --output text
```

### 10.3 Workflow de Deploy Frontend

Crear archivo `.github/workflows/deploy-frontend-aws.yml`:

```yaml
name: Deploy Frontend to AWS

on:
  push:
    branches: [main]
    paths:
      - 'frontend/**'
      - '.github/workflows/deploy-frontend-aws.yml'
  workflow_dispatch:

env:
  AWS_REGION: eu-west-1

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install dependencies
        working-directory: frontend
        run: npm ci

      - name: Build frontend
        working-directory: frontend
        env:
          VITE_API_BASE_URL: https://YOUR_APP_RUNNER_URL/api
        run: npm run build

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: ${{ env.AWS_REGION }}

      - name: Deploy to S3
        run: |
          aws s3 sync frontend/dist s3://${{ secrets.S3_FRONTEND_BUCKET }} \
            --delete \
            --cache-control "public, max-age=31536000" \
            --exclude "index.html"
          
          aws s3 cp frontend/dist/index.html s3://${{ secrets.S3_FRONTEND_BUCKET }}/index.html \
            --cache-control "public, max-age=0, must-revalidate"

      - name: Invalidate CloudFront cache
        run: |
          aws cloudfront create-invalidation \
            --distribution-id ${{ secrets.CLOUDFRONT_DISTRIBUTION_ID }} \
            --paths "/*"
```

---

## 11. Migración de Datos

### 11.1 Exportar Datos de Desarrollo

```bash
# Desde el contenedor de desarrollo PostgreSQL
docker exec -t joyeria-postgres-dev pg_dump -U dev_user -d joyeria_dev > backup_dev.sql
```

### 11.2 Importar Datos a RDS

```bash
# Conectar a RDS (temporalmente con acceso público habilitado)
RDS_ENDPOINT=$(aws rds describe-db-instances --db-instance-identifier "joiabagur-db-prod" --query 'DBInstances[0].Endpoint.Address' --output text)

# Importar datos
PGPASSWORD=YOUR_SECURE_PASSWORD psql -h $RDS_ENDPOINT -U joiabagur_admin -d joiabagur_prod < backup_dev.sql
```

### 11.3 Migrar Archivos a S3

```bash
# Subir fotos de productos existentes
aws s3 sync ./uploads/product-photos s3://joiabagur-files-prod/product-photos/

# Subir fotos de ventas existentes (si hay)
aws s3 sync ./uploads/sale-photos s3://joiabagur-files-prod/sale-photos/
```

### 11.4 Aplicar Migraciones EF Core

```bash
# Conectar a RDS y aplicar migraciones
cd backend/src/JoiabagurPV.API
dotnet ef database update --connection "Host=$RDS_ENDPOINT;Database=joiabagur_prod;Username=joiabagur_admin;Password=YOUR_PASSWORD;SSL Mode=Require"
```

---

## 12. Verificación y Go-Live

### 12.1 Checklist Pre-Go-Live

- [ ] RDS está running y accesible
- [ ] S3 buckets creados con configuración correcta
- [ ] Secrets Manager contiene todas las credenciales
- [ ] App Runner servicio running
- [ ] CloudFront distribución deployed
- [ ] GitHub Secrets configurados
- [ ] CI/CD workflows creados

### 12.2 Verificar Backend

```bash
# Obtener URL del App Runner
APP_RUNNER_URL=$(aws apprunner describe-service \
  --service-arn $APP_RUNNER_SERVICE_ARN \
  --query 'Service.ServiceUrl' \
  --output text)

# Verificar health check
curl -s https://$APP_RUNNER_URL/health | jq .

# Verificar API
curl -s https://$APP_RUNNER_URL/api/v1/products | jq .
```

### 12.3 Verificar Frontend

```bash
# Obtener URL de CloudFront
CLOUDFRONT_URL=$(aws cloudfront list-distributions \
  --query 'DistributionList.Items[?Comment==`JoiabagurPV Frontend Distribution`].DomainName' \
  --output text)

# Verificar acceso
curl -s -o /dev/null -w "%{http_code}" https://$CLOUDFRONT_URL
```

### 12.4 Tests Funcionales

1. **Autenticación**: Login con usuario admin
2. **Productos**: Listar, crear, editar productos
3. **Fotos**: Subir y visualizar fotos de productos
4. **Inventario**: Verificar stock por punto de venta
5. **Móvil**: Probar desde dispositivo móvil

---

## 13. Monitoreo y Mantenimiento

### 13.1 CloudWatch Logs

```bash
# Ver logs de App Runner
aws logs describe-log-groups --log-group-name-prefix "/aws/apprunner/joiabagur"

# Tail logs en tiempo real
aws logs tail "/aws/apprunner/joiabagur-api-prod/service" --follow
```

### 13.2 Métricas Importantes

- **App Runner**: RequestCount, 5xxCount, ActiveInstances
- **RDS**: CPUUtilization, FreeableMemory, DatabaseConnections
- **S3**: NumberOfObjects, BucketSizeBytes

### 13.3 Alertas Básicas

```bash
# Crear alarma para errores 5xx
aws cloudwatch put-metric-alarm \
  --alarm-name "joiabagur-5xx-errors" \
  --metric-name "5xxCount" \
  --namespace "AWS/AppRunner" \
  --statistic Sum \
  --period 300 \
  --threshold 10 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 1 \
  --dimensions Name=ServiceName,Value=joiabagur-api-prod
```

---

## 14. Backup y Recuperación

### 14.1 Backups Automáticos de RDS

RDS está configurado con:
- **Retención**: 7 días
- **Ventana de backup**: 03:00-04:00 UTC
- **Tipo**: Snapshots automáticos diarios

### 14.2 Verificar Backups

```bash
# Listar snapshots disponibles
aws rds describe-db-snapshots \
  --db-instance-identifier "joiabagur-db-prod" \
  --query 'DBSnapshots[*].[DBSnapshotIdentifier,SnapshotCreateTime,Status]' \
  --output table
```

### 14.3 Restaurar desde Backup

```bash
# Restaurar a nueva instancia (no destructivo)
aws rds restore-db-instance-from-db-snapshot \
  --db-instance-identifier "joiabagur-db-restored" \
  --db-snapshot-identifier "rds:joiabagur-db-prod-2026-01-12-03-00" \
  --db-instance-class "db.t3.micro"
```

### 14.4 Point-in-Time Recovery

```bash
# Restaurar a un momento específico
aws rds restore-db-instance-to-point-in-time \
  --source-db-instance-identifier "joiabagur-db-prod" \
  --target-db-instance-identifier "joiabagur-db-pitr" \
  --restore-time "2026-01-12T10:30:00Z"
```

### 14.5 Backup Manual de S3

```bash
# Sincronizar S3 a backup local (periódicamente)
aws s3 sync s3://joiabagur-files-prod ./backups/s3-files/
```

---

## 15. Costos y Optimización

### 15.1 Estimación de Costos Mensual

| Servicio | Free-tier (12 meses) | Post Free-tier |
|----------|---------------------|----------------|
| App Runner (0.5GB RAM) | ~$0-5* | ~$5-15 |
| RDS PostgreSQL | $0 (750 hrs) | ~$15-25 |
| S3 | $0 (5GB) | ~$1-3 |
| CloudFront | $0 (1TB) | ~$1-2 |
| Secrets Manager | ~$1 | ~$1 |
| ECR | $0 (500MB) | ~$1 |
| **Total** | **~$1-6** | **~$25-45** |

*App Runner cobra por tiempo de build. Con 0.5GB RAM se mantiene dentro del free-tier.
**ML training se ejecuta localmente en laptop del admin = $0 adicional.**

### 15.2 Configurar Alertas de Billing

```bash
# Crear alerta de billing (requiere habilitar billing alerts primero)
aws cloudwatch put-metric-alarm \
  --alarm-name "joiabagur-billing-alert-20" \
  --metric-name "EstimatedCharges" \
  --namespace "AWS/Billing" \
  --statistic Maximum \
  --period 21600 \
  --threshold 20 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 1 \
  --dimensions Name=Currency,Value=USD
```

### 15.3 Optimizaciones de Costo

1. **App Runner**: Configurar min instances = 0 para scale-to-zero
2. **RDS**: Usar db.t3.micro (free-tier eligible)
3. **S3**: Lifecycle policy para eliminar versiones antiguas
4. **CloudFront**: Price class 100 (solo edge locations principales)

---

## 16. Troubleshooting

### 16.1 App Runner no inicia

```bash
# Ver eventos del servicio
aws apprunner describe-service \
  --service-arn $APP_RUNNER_SERVICE_ARN \
  --query 'Service.ServiceStatus'

# Ver logs de error
aws logs filter-log-events \
  --log-group-name "/aws/apprunner/joiabagur-api-prod/service" \
  --filter-pattern "ERROR"
```

### 16.2 RDS Connection Refused

1. Verificar security group permite tráfico desde App Runner
2. Verificar endpoint en Secrets Manager es correcto
3. Verificar SSL Mode=Require en connection string

```bash
# Verificar conectividad
nc -zv RDS_ENDPOINT 5432
```

### 16.3 S3 Access Denied

1. Verificar IAM role del App Runner tiene permisos S3
2. Verificar nombre del bucket en configuración
3. Verificar CORS configurado correctamente

```bash
# Verificar política del bucket
aws s3api get-bucket-policy --bucket joiabagur-files-prod
```

### 16.4 CloudFront Error 403

1. Verificar OAI configurado correctamente
2. Verificar política del bucket permite OAI
3. Verificar index.html existe en S3

```bash
# Verificar contenido del bucket
aws s3 ls s3://joiabagur-frontend-prod/
```

### 16.5 Secrets Manager Access Denied

1. Verificar IAM role tiene política para secretsmanager:GetSecretValue
2. Verificar nombre del secreto es correcto
3. Verificar región del secreto

```bash
# Verificar secreto existe
aws secretsmanager describe-secret --secret-id joiabagur-prod
```

---

## Apéndice A: Comandos Útiles

```bash
# Estado general
aws apprunner describe-service --service-arn $SERVICE_ARN
aws rds describe-db-instances --db-instance-identifier joiabagur-db-prod
aws s3 ls s3://joiabagur-files-prod --recursive --summarize

# Logs
aws logs tail "/aws/apprunner/joiabagur-api-prod/service" --follow

# Redeploy manual
aws apprunner start-deployment --service-arn $SERVICE_ARN

# Invalidar cache CloudFront
aws cloudfront create-invalidation --distribution-id $DIST_ID --paths "/*"

# Ver costos actuales
aws ce get-cost-and-usage \
  --time-period Start=2026-01-01,End=2026-01-31 \
  --granularity MONTHLY \
  --metrics "BlendedCost"
```

---

## Apéndice B: Recursos Adicionales

- [AWS App Runner Documentation](https://docs.aws.amazon.com/apprunner/)
- [AWS RDS PostgreSQL](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/CHAP_PostgreSQL.html)
- [AWS S3 Developer Guide](https://docs.aws.amazon.com/AmazonS3/latest/userguide/)
- [AWS Secrets Manager](https://docs.aws.amazon.com/secretsmanager/)
- [GitHub Actions for AWS](https://github.com/aws-actions)

---

*Última actualización: Enero 2026*
*Versión: 1.0*
