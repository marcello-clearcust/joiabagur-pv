# Guía de Migración a Nueva Cuenta AWS — Arquitectura EC2

## Índice

1. [Arquitectura nueva](#1-arquitectura-nueva)
2. [Prerrequisitos](#2-prerrequisitos)
3. [Paso 1 — Preparar nueva cuenta AWS](#3-paso-1--preparar-nueva-cuenta-aws)
4. [Paso 2 — Desplegar infraestructura con Terraform](#4-paso-2--desplegar-infraestructura-con-terraform)
5. [Paso 3 — Migrar la base de datos](#5-paso-3--migrar-la-base-de-datos)
6. [Paso 4 — Migrar ficheros S3](#6-paso-4--migrar-ficheros-s3)
7. [Paso 5 — Configurar GitHub Actions](#7-paso-5--configurar-github-actions)
8. [Paso 6 — Primer despliegue](#8-paso-6--primer-despliegue)
9. [Paso 7 — Configurar SSL y cambiar DNS](#9-paso-7--configurar-ssl-y-cambiar-dns)
10. [Paso 8 — Verificación y cierre del entorno antiguo](#10-paso-8--verificación-y-cierre-del-entorno-antiguo)
11. [Costes estimados](#11-costes-estimados)
12. [Troubleshooting](#12-troubleshooting)

---

## 1. Arquitectura nueva

```
Internet
    │ HTTPS
    ▼
EC2 t3.micro — nginx (SSL/TLS + Let's Encrypt)
    │
    └── Docker: .NET 10 API + React SPA (puerto 8080)
         │
         ├── RDS PostgreSQL db.t3.micro (acceso solo desde EC2)
         ├── S3 jpv-files-prod (fotos + modelos ML)
         ├── ECR jpv-backend (imágenes Docker)
         └── SSM Parameter Store /jpv/prod/* (credenciales)
```

**Mejoras incorporadas respecto a la arquitectura anterior:**

| Mejora | Detalle |
|--------|---------|
| Sin credenciales en GitHub | OIDC federation: GitHub Actions asume IAM Role directamente |
| SSM Parameter Store | Sustituye Secrets Manager (~$1.50/mes de ahorro) |
| Frontend bundled | React SPA incluida en imagen Docker — elimina CloudFront + S3-frontend |

---

## 2. Prerrequisitos

- AWS CLI v2 configurado con credenciales de la **nueva cuenta**
- Terraform >= 1.5 instalado
- Docker instalado localmente
- Acceso al repositorio de GitHub para actualizar Secrets

---

## 3. Paso 1 — Preparar nueva cuenta AWS

### 3.1 Crear usuario IAM para Terraform (solo para el apply inicial)

```bash
# Crear usuario solo para terraform apply
aws iam create-user --user-name jpv-terraform-admin

aws iam attach-user-policy \
  --user-name jpv-terraform-admin \
  --policy-arn arn:aws:iam::aws:policy/AdministratorAccess

aws iam create-access-key --user-name jpv-terraform-admin
# Guardar las credenciales — se eliminarán tras el terraform apply
```

### 3.2 Crear bucket S3 para estado de Terraform (prerequisito de terraform init)

```bash
aws s3api create-bucket \
  --bucket jpv-terraform-state \
  --region eu-west-3 \
  --create-bucket-configuration LocationConstraint=eu-west-3

aws s3api put-bucket-versioning \
  --bucket jpv-terraform-state \
  --versioning-configuration Status=Enabled

aws s3api put-public-access-block \
  --bucket jpv-terraform-state \
  --public-access-block-configuration \
  BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true
```

---

## 4. Paso 2 — Desplegar infraestructura con Terraform

### 4.1 Crear fichero de variables

```bash
cd terraform/
cp terraform.tfvars.example terraform.tfvars
# Editar terraform.tfvars con los valores reales
```

Obtener el AMI más reciente de Amazon Linux 2023:

```bash
aws ec2 describe-images \
  --owners amazon \
  --filters "Name=name,Values=al2023-ami-*-x86_64" \
  --query 'sort_by(Images, &CreationDate)[-1].ImageId' \
  --output text \
  --region eu-west-3
```

### 4.2 Aplicar Terraform

```bash
terraform init
terraform plan   # Revisar los recursos que se crearán
terraform apply
```

El apply crea: EC2, Elastic IP, RDS, S3, ECR, SSM parameters, IAM roles (con OIDC).

### 4.3 Anotar los outputs

```bash
terraform output
# ec2_public_ip          → para actualizar DNS
# rds_endpoint           → para importar datos
# ecr_repository_url     → para GitHub Actions
# github_actions_role_arn → para GitHub Actions
# ec2_instance_id        → para GitHub Actions
```

---

## 5. Paso 3 — Migrar la base de datos

RDS en la nueva cuenta no es accesible públicamente. Se usa SSM port forwarding para tunelizar la conexión desde el equipo local.

### 5.1 Exportar la base de datos actual (cuenta antigua)

```bash
# Obtener endpoint RDS antiguo
OLD_RDS=$(aws rds describe-db-instances \
  --db-instance-identifier jpv-db-prod \
  --query 'DBInstances[0].Endpoint.Address' \
  --output text \
  --profile cuenta-antigua)

PGPASSWORD=TU_PASSWORD_ANTIGUA pg_dump \
  -h $OLD_RDS -U joiabagur_admin -d joiabagur_prod \
  --no-owner --no-acl \
  -f backup_prod.sql

echo "Backup: $(wc -l backup_prod.sql) líneas"
```

### 5.2 Abrir túnel SSM al nuevo RDS (desde el equipo local)

En una terminal aparte:

```bash
# Obtener EC2 instance ID e RDS endpoint de los outputs de terraform
EC2_ID=$(cd terraform && terraform output -raw ec2_instance_id)
NEW_RDS=$(cd terraform && terraform output -raw rds_endpoint)

aws ssm start-session \
  --target $EC2_ID \
  --document-name AWS-StartPortForwardingSessionToRemoteHost \
  --parameters "{\"host\":[\"$NEW_RDS\"],\"portNumber\":[\"5432\"],\"localPortNumber\":[\"5432\"]}"
# Dejar esta terminal abierta durante la importación
```

### 5.3 Importar datos al nuevo RDS (a través del túnel)

En otra terminal:

```bash
# Con el túnel activo, localhost:5432 apunta al nuevo RDS
PGPASSWORD=TU_PASSWORD_NUEVA psql \
  -h localhost -p 5432 \
  -U joiabagur_admin -d joiabagur_prod \
  -f backup_prod.sql

echo "Importación completada."
```

---

## 6. Paso 4 — Migrar ficheros S3

### 6.1 Añadir política cross-account en el bucket origen (cuenta antigua)

```bash
# Obtener Account ID de la nueva cuenta
NEW_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)

# En la cuenta antigua — añadir política temporal al bucket
aws s3api put-bucket-policy \
  --bucket jpv-files-prod \
  --profile cuenta-antigua \
  --policy "{
    \"Version\": \"2012-10-17\",
    \"Statement\": [{
      \"Effect\": \"Allow\",
      \"Principal\": { \"AWS\": \"arn:aws:iam::$NEW_ACCOUNT_ID:root\" },
      \"Action\": [\"s3:GetObject\", \"s3:ListBucket\"],
      \"Resource\": [
        \"arn:aws:s3:::jpv-files-prod\",
        \"arn:aws:s3:::jpv-files-prod/*\"
      ]
    }]
  }"
```

### 6.2 Sincronizar los ficheros

```bash
# Con credenciales de la nueva cuenta
aws s3 sync \
  s3://jpv-files-prod \
  s3://jpv-files-prod \
  --source-region eu-west-3 \
  --region eu-west-3 \
  --no-progress

echo "Sync completado."
```

### 6.3 Eliminar la política temporal (cuenta antigua)

```bash
aws s3api delete-bucket-policy \
  --bucket jpv-files-prod \
  --profile cuenta-antigua
```

---

## 7. Paso 5 — Configurar GitHub Actions

### 7.1 Actualizar GitHub Secrets del repositorio

Ir a **Settings → Secrets and variables → Actions** y crear/actualizar:

| Secret | Valor (de terraform output) |
|--------|-----------------------------|
| `DEPLOY_ROLE_ARN` | `terraform output -raw github_actions_role_arn` |
| `EC2_INSTANCE_ID` | `terraform output -raw ec2_instance_id` |
| `PRODUCTION_DOMAIN` | `pv.joiabagur.com` |

> Los secrets `AWS_ACCESS_KEY_ID` y `AWS_SECRET_ACCESS_KEY` **no se necesitan** para el nuevo workflow gracias a OIDC (Mejora 1). Los secrets actuales pueden dejarse para los workflows existentes hasta que se desactiven.

---

## 8. Paso 6 — Primer despliegue

Lanzar el workflow manualmente desde GitHub:

```
GitHub → Actions → "Deploy to AWS EC2 (nueva arquitectura)" → Run workflow
```

O desde la CLI:

```bash
gh workflow run deploy-aws-ec2.yml
```

Verificar que el contenedor está corriendo:

```bash
# Via SSM
aws ssm start-session --target $EC2_ID

# Dentro de la sesión SSM:
docker ps
docker logs jpv-api --tail 50
```

---

## 9. Paso 7 — Configurar SSL y cambiar DNS

### 9.1 Obtener el Elastic IP

```bash
EC2_IP=$(cd terraform && terraform output -raw ec2_public_ip)
echo "Elastic IP: $EC2_IP"
```

### 9.2 Actualizar el registro DNS

En el proveedor DNS actual, cambiar el registro A de `pv.joiabagur.com` de la IP antigua a `$EC2_IP`. Esperar propagación (entre 5 min y 1 hora dependiendo del TTL).

Verificar propagación:

```bash
dig pv.joiabagur.com +short
# Debe mostrar el Elastic IP nuevo
```

### 9.3 Obtener certificado SSL con Let's Encrypt

Una vez el DNS apunte al EC2:

```bash
# Via SSM
aws ssm start-session --target $EC2_ID

# Dentro de la sesión SSM:
sudo certbot --nginx -d pv.joiabagur.com
# Seguir las instrucciones. Certbot modifica nginx automáticamente.

# Verificar renovación automática
sudo certbot renew --dry-run
```

---

## 10. Paso 8 — Verificación y cierre del entorno antiguo

### 10.1 Checklist de verificación

```bash
# Health check
curl -sf https://pv.joiabagur.com/api/health | jq .

# Frontend carga correctamente
curl -sf -o /dev/null -w "%{http_code}" https://pv.joiabagur.com
# Debe retornar 200
```

Pruebas funcionales:
- [ ] Login con usuario admin
- [ ] Listar productos y fotos
- [ ] Subir una foto de producto
- [ ] Inventario por punto de venta
- [ ] Acceso desde móvil

### 10.2 Apagar entorno antiguo

Solo después de confirmar que todo funciona correctamente en la nueva cuenta:

```bash
# Con credenciales de la cuenta ANTIGUA
# Pausar App Runner (no eliminar — por si acaso)
aws apprunner pause-service \
  --service-arn $APP_RUNNER_SERVICE_ARN \
  --profile cuenta-antigua

# Tras unos días de margen, eliminar recursos:
# - App Runner service
# - S3 frontend bucket (jpv-frontend-prod)
# - CloudFront distribution
# - ECR repository (antigua cuenta)
# Conservar: RDS (hasta confirmar backup migrado), usuario IAM antigua
```

---

## 11. Costes estimados (nueva arquitectura, post free tier)

| Servicio | €/mes |
|----------|-------|
| EC2 t3.micro + EBS 20GB | ~9–10 |
| RDS PostgreSQL db.t3.micro | ~15–18 |
| S3 jpv-files-prod | ~0.5–2 |
| ECR jpv-backend | ~0 |
| SSM Parameter Store (7 params) | ~0.35 |
| Data transfer | ~1–3 |
| **Total** | **~26–33 €/mes** |

> Durante los primeros 12 meses de la nueva cuenta, EC2 t3.micro y RDS db.t3.micro son free tier: **~1–3 €/mes** (solo S3 + SSM + data transfer).

---

## 12. Troubleshooting

### Contenedor no arranca

```bash
# Via SSM
aws ssm start-session --target $EC2_ID
docker logs jpv-api
# Error común: SSM parameter no encontrado → verificar /jpv/prod/* en AWS Console
```

### Error en SSM send-command

```bash
# Ver output del comando
aws ssm get-command-invocation \
  --command-id $COMMAND_ID \
  --instance-id $EC2_INSTANCE_ID \
  --query 'StandardErrorContent' \
  --output text
```

### nginx devuelve 502 Bad Gateway

El contenedor no está corriendo en el puerto 8080:

```bash
# Via SSM
docker ps -a
docker start jpv-api  # Si está parado
```

### Certbot falla (DNS no propagado)

```bash
# Verificar que el DNS apunta al EC2
dig pv.joiabagur.com +short
# Debe coincidir con: terraform output -raw ec2_public_ip
```

### Importación de datos falla (acceso denegado)

Verificar que el túnel SSM está activo y que las credenciales de la nueva cuenta tienen acceso a RDS:

```bash
# El túnel SSM debe estar corriendo en otra terminal
# Verificar conectividad:
psql -h localhost -p 5432 -U joiabagur_admin -d joiabagur_prod -c "SELECT 1;"
```

---

*Última actualización: Marzo 2026*
