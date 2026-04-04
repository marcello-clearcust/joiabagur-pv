# Arquitectura legada: App Runner + CloudFront + S3 frontend

Esta pila **ya no es la de producción** tras la migración a **EC2 + Docker bundlado + Terraform** (véase [deploy-aws-production.md](deploy-aws-production.md)).

Se documenta aquí solo como **referencia histórica** para la cuenta o recursos que aún puedan existir hasta su baja.

## Componentes típicos (legado)

| Recurso | Uso |
|---------|-----|
| **AWS App Runner** | Servicio `jpv-backend` — contenedor solo API .NET |
| **Amazon ECR** | Imagen API (sin SPA en contenedor) |
| **S3 `jpv-frontend-prod`** | Hosting estático del build Vite/React |
| **CloudFront** | CDN delante del bucket frontend |
| **RDS** | PostgreSQL (endpoint propio de esa cuenta) |
| **S3 `jpv-files-prod`** | Ficheros de negocio en cuenta antigua |
| **Secrets / env** | Variables en App Runner o Secrets Manager |

## CI/CD legado (repositorio)

Los workflows **Deploy Backend to AWS** y **Deploy Frontend to AWS** siguen en `.github/workflows/` con nombre **`(deprecated)`** y disparador **solo manual** (`workflow_dispatch`), para no publicar en push accidentalmente.

## DNS típico (legado)

- `pv.joiabagur.com` → CloudFront (frontend).
- `api.joiabagur.com` → App Runner (API).

En la arquitectura nueva, **`pv.joiabagur.com`** suele apuntar al **Elastic IP** de la EC2 y sirve **SPA + API** bajo el mismo dominio (`/api/...`).

## Cierre recomendado

Cuando la migración esté validada: pausar o eliminar App Runner, distribución CloudFront y bucket frontend; conservar snapshot RDS y copia S3 hasta periodo de retención acordado. Lista orientativa en [deploy-aws-ec2-migration.md](deploy-aws-ec2-migration.md) §10.

*Referencia — abril 2026.*
