# AWS — Plantilla de recursos (ejemplo en repo)

Copia este fichero a **`aws-production-credentials.md`** en la misma carpeta (ese nombre está en `.gitignore` para no subir secretos).

---

# AWS — Recursos y credenciales (referencia local)

> **CONFIDENCIAL** — No subir a repositorios públicos. Este fichero suele estar en `.gitignore`.  
> **No pegues access keys ni contraseñas en chat ni en código.** Usa un gestor de secretos.

**Arquitectura de referencia:** producción actual = **EC2 + Docker bundlado + RDS + S3 + SSM + OIDC** ([deploy-aws-production.md](deploy-aws-production.md)).  
Datos de la **cuenta legada** (App Runner, CloudFront, `jpv-files-prod`, IAM `jpv-cicd`, etc.) conservalos aparte si aún los necesitas para rollback.

---

## URLs de producción (actual)

| Uso | URL típica |
|-----|------------|
| **App (SPA + API mismo origen)** | `https://pv.joiabagur.com` |
| **Health** | `https://pv.joiabagur.com/api/health` |

Rellena aquí si difiere:

| Campo | Valor |
|-------|-------|
| Dominio público | |
| Elastic IP (EC2) | `terraform output -raw ec2_public_ip` |

### Legado (cuenta antigua — opcional)

| Uso | Notas |
|-----|--------|
| CloudFront / App Runner | Solo si siguen activos; ver [deploy-aws-app-runner-legacy.md](deploy-aws-app-runner-legacy.md) |

---

## Cuenta AWS (nueva / actual)

| Campo | Valor |
|-------|--------|
| Región | `eu-west-3` |
| Account ID | _(pegar desde `aws sts get-caller-identity`)_ |

---

## Terraform

| Campo | Cómo obtenerlo |
|-------|----------------|
| `ec2_instance_id` | `terraform output -raw ec2_instance_id` |
| `rds_endpoint` | `terraform output -raw rds_endpoint` |
| `github_actions_role_arn` | `terraform output -raw github_actions_role_arn` |
| `ecr_repository_url` | `terraform output -raw ecr_repository_url` |

---

## RDS PostgreSQL (nueva cuenta)

| Campo | Valor |
|-------|--------|
| Identificador | _(p. ej. `jpv-db-prod`)_ |
| Endpoint | _(salida Terraform o consola RDS)_ |
| Puerto | `5432` |
| Base de datos | `jpv` |
| Usuario | `postgres` |
| Contraseña | _(solo en `terraform.tfvars` / gestor; no duplicar aquí en claro si el repo no es privado)_ |

### Cadena de conexión (rellenar tú)

```
Host=...;Port=5432;Database=jpv;Username=postgres;Password=...
```

En runtime la app usa **SSM** (`ConnectionStrings__DefaultConnection`); no hace falta pegar la contraseña en el contenedor a mano si ya está en parámetros.

---

## S3

| Bucket | Uso |
|--------|-----|
| **`prod-jpv-files`** | Ficheros de negocio (fotos, modelos ML) — pila Terraform actual |
| `jpv-files-prod` | Legado (cuenta antigua), solo referencia migración |

---

## ECR

| Campo | Valor |
|-------|--------|
| Repositorio | `jpv-backend` |
| URI | `terraform output` o consola ECR |

---

## GitHub Actions

### Producción (EC2)

| Secret | Descripción |
|--------|-------------|
| `DEPLOY_ROLE_ARN` | ARN rol OIDC (`terraform output -raw github_actions_role_arn`) |
| `EC2_INSTANCE_ID` | ID instancia EC2 |
| `PRODUCTION_DOMAIN` | Sin `https://`, p. ej. `pv.joiabagur.com` |

### Legado (solo si ejecutáis workflows deprecated a mano)

| Secret | Uso |
|--------|-----|
| `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` | Cuenta antigua / usuario `jpv-cicd` — **rotar** si hubo exposición |
| `APP_RUNNER_SERVICE_ARN` | App Runner legado |
| `CLOUDFRONT_DISTRIBUTION_ID` | CDN legado |

---

## DNS (OVH u otro proveedor)

Registro típico **actual:** tipo **A** de `pv` → Elastic IP de la EC2.

| Nombre | Tipo | Valor | Notas |
|--------|------|-------|--------|
| `pv` | A | _(Elastic IP)_ | Producción actual |
| _(legado)_ | CNAME | CloudFront / App Runner | Solo si aún aplica |

---

## Aplicación

| Campo | Notas |
|-------|--------|
| Usuario admin por defecto | `admin` / contraseña inicial según seeder o datos migrados — **cambiar** en producción |

---

## Workflows (repo)

| Workflow | Estado |
|----------|--------|
| `deploy-aws-ec2.yml` | **Activo** — push `main`/`master` + manual |
| `deploy-backend-aws.yml` | **Deprecated** — solo `workflow_dispatch` |
| `deploy-frontend-aws.yml` | **Deprecated** — solo `workflow_dispatch` |

---

## Recordatorios

1. Rotar credenciales si alguna vez se filtraron.
2. Tras migración, cerrar acceso público temporal al RDS si lo abriste.
3. Vigilar facturación AWS y alarmas.
4. Backups RDS según ventana Terraform.

---

*Plantilla actualizada abril 2026 — rellenar valores sensibles solo en copia local segura.*
