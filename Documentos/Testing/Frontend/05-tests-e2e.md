# 05 - Tests End-to-End con Playwright

Tests de extremo a extremo que simulan la interacción real del usuario con la aplicación.

---

## ¿Por qué Playwright?

| Característica | Playwright | Cypress |
|----------------|------------|---------|
| Multi-navegador | ✅ Chromium, Firefox, WebKit | ⚠️ Solo Chromium |
| Ejecución paralela | ✅ Nativo | ⚠️ Requiere Dashboard (pago) |
| Auto-waiting | ✅ Inteligente | ✅ Bueno |
| Network mocking | ✅ Completo | ✅ Bueno |
| Mobile emulation | ✅ Excelente | ⚠️ Limitado |
| Mantenedor | Microsoft | Cypress.io |
| Licencia | Apache 2.0 (gratis) | MIT (gratis) |

---

## Configuración

### playwright.config.ts

```typescript
import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  // Directorio de tests
  testDir: './e2e',
  
  // Timeout por test
  timeout: 30 * 1000,
  
  // Timeout para expects
  expect: {
    timeout: 5000,
  },
  
  // Ejecutar en paralelo
  fullyParallel: true,
  
  // Prohibir .only en CI
  forbidOnly: !!process.env.CI,
  
  // Reintentos
  retries: process.env.CI ? 2 : 0,
  
  // Workers
  workers: process.env.CI ? 1 : undefined,
  
  // Reporters
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['list'],
    ['json', { outputFile: 'test-results/results.json' }],
  ],
  
  // Configuración global
  use: {
    // URL base
    baseURL: 'http://localhost:5173',
    
    // Trace en caso de fallo
    trace: 'on-first-retry',
    
    // Screenshot en caso de fallo
    screenshot: 'only-on-failure',
    
    // Video en caso de fallo
    video: 'on-first-retry',
    
    // Viewport por defecto
    viewport: { width: 1280, height: 720 },
    
    // Ignorar errores HTTPS
    ignoreHTTPSErrors: true,
  },

  // Proyectos (navegadores)
  projects: [
    // Setup de autenticación
    {
      name: 'setup',
      testMatch: /.*\.setup\.ts/,
    },
    
    // Desktop browsers
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
      dependencies: ['setup'],
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
      dependencies: ['setup'],
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
      dependencies: ['setup'],
    },
    
    // Mobile browsers
    {
      name: 'mobile-chrome',
      use: { ...devices['Pixel 5'] },
      dependencies: ['setup'],
    },
    {
      name: 'mobile-safari',
      use: { ...devices['iPhone 12'] },
      dependencies: ['setup'],
    },
  ],

  // Servidor de desarrollo
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
})
```

---

## Estructura de Tests E2E

```
e2e/
├── auth.setup.ts           # Setup de autenticación
├── auth.spec.ts            # Tests de login/logout
├── products.spec.ts        # Tests de productos
├── sales.spec.ts           # Tests de ventas
├── inventory.spec.ts       # Tests de inventario
├── users.spec.ts           # Tests de usuarios
├── fixtures/
│   ├── test-user.ts        # Datos de usuario de prueba
│   └── test-products.ts    # Datos de productos
├── pages/
│   ├── login.page.ts       # Page Object de Login
│   ├── products.page.ts    # Page Object de Productos
│   └── base.page.ts        # Page Object base
└── utils/
    └── helpers.ts          # Utilidades
```

---

## Setup de Autenticación

### e2e/auth.setup.ts

```typescript
import { test as setup, expect } from '@playwright/test'
import path from 'path'

const authFile = path.join(__dirname, '../playwright/.auth/user.json')

setup('authenticate', async ({ page }) => {
  // Ir a página de login
  await page.goto('/login')
  
  // Completar formulario
  await page.getByLabel(/email/i).fill('admin@joyeria.com')
  await page.getByLabel(/contraseña/i).fill('admin123')
  
  // Enviar formulario
  await page.getByRole('button', { name: /iniciar sesión/i }).click()
  
  // Esperar redirección al dashboard
  await page.waitForURL('/dashboard')
  
  // Verificar que estamos logueados
  await expect(page.getByText(/bienvenido/i)).toBeVisible()
  
  // Guardar estado de autenticación
  await page.context().storageState({ path: authFile })
})
```

### Uso del Estado de Auth

```typescript
import { test } from '@playwright/test'

// Los tests que usan 'chromium' project heredan la autenticación
test('should access protected page', async ({ page }) => {
  await page.goto('/products')
  // Ya estamos autenticados
})
```

---

## Page Objects

### e2e/pages/base.page.ts

```typescript
import { Page, Locator, expect } from '@playwright/test'

export class BasePage {
  readonly page: Page
  readonly sidebar: Locator
  readonly header: Locator
  readonly toast: Locator

  constructor(page: Page) {
    this.page = page
    this.sidebar = page.locator('[data-testid="sidebar"]')
    this.header = page.locator('header')
    this.toast = page.locator('[data-sonner-toast]')
  }

  async navigateTo(path: string) {
    await this.page.goto(path)
  }

  async waitForToast(message: string | RegExp) {
    await expect(this.toast.filter({ hasText: message })).toBeVisible()
  }

  async clickSidebarLink(name: string) {
    await this.sidebar.getByRole('link', { name }).click()
  }

  async expectPageTitle(title: string) {
    await expect(this.page.getByRole('heading', { level: 1 })).toHaveText(title)
  }
}
```

### e2e/pages/login.page.ts

```typescript
import { Page, Locator, expect } from '@playwright/test'

export class LoginPage {
  readonly page: Page
  readonly emailInput: Locator
  readonly passwordInput: Locator
  readonly submitButton: Locator
  readonly errorAlert: Locator

  constructor(page: Page) {
    this.page = page
    this.emailInput = page.getByLabel(/email/i)
    this.passwordInput = page.getByLabel(/contraseña/i)
    this.submitButton = page.getByRole('button', { name: /iniciar sesión/i })
    this.errorAlert = page.getByRole('alert')
  }

  async goto() {
    await this.page.goto('/login')
  }

  async login(email: string, password: string) {
    await this.emailInput.fill(email)
    await this.passwordInput.fill(password)
    await this.submitButton.click()
  }

  async expectError(message: string | RegExp) {
    await expect(this.errorAlert).toContainText(message)
  }

  async expectRedirectToDashboard() {
    await this.page.waitForURL('/dashboard')
  }
}
```

### e2e/pages/products.page.ts

```typescript
import { Page, Locator, expect } from '@playwright/test'
import { BasePage } from './base.page'

export class ProductsPage extends BasePage {
  readonly createButton: Locator
  readonly searchInput: Locator
  readonly productTable: Locator
  readonly deleteDialog: Locator

  constructor(page: Page) {
    super(page)
    this.createButton = page.getByRole('button', { name: /nuevo producto/i })
    this.searchInput = page.getByPlaceholder(/buscar/i)
    this.productTable = page.getByRole('table')
    this.deleteDialog = page.getByRole('alertdialog')
  }

  async goto() {
    await this.page.goto('/products')
  }

  async createProduct(data: {
    sku: string
    name: string
    price: number
    collectionId: string
  }) {
    await this.createButton.click()
    await this.page.getByLabel(/sku/i).fill(data.sku)
    await this.page.getByLabel(/nombre/i).fill(data.name)
    await this.page.getByLabel(/precio/i).fill(String(data.price))
    await this.page.getByLabel(/colección/i).selectOption(data.collectionId)
    await this.page.getByRole('button', { name: /guardar/i }).click()
  }

  async searchProduct(query: string) {
    await this.searchInput.fill(query)
    // Esperar debounce
    await this.page.waitForTimeout(300)
  }

  async deleteProduct(sku: string) {
    // Encontrar fila del producto
    const row = this.productTable.getByRole('row').filter({ hasText: sku })
    await row.getByRole('button', { name: /eliminar/i }).click()
    
    // Confirmar en diálogo
    await this.deleteDialog.getByRole('button', { name: /confirmar/i }).click()
  }

  async expectProductInTable(name: string) {
    await expect(this.productTable.getByText(name)).toBeVisible()
  }

  async expectProductNotInTable(name: string) {
    await expect(this.productTable.getByText(name)).not.toBeVisible()
  }

  async expectProductCount(count: number) {
    const rows = this.productTable.getByRole('row')
    // -1 por el header
    await expect(rows).toHaveCount(count + 1)
  }
}
```

---

## Tests de Autenticación

### e2e/auth.spec.ts

```typescript
import { test, expect } from '@playwright/test'
import { LoginPage } from './pages/login.page'

// Estos tests NO usan la autenticación del setup
test.describe('Authentication', () => {
  test.use({ storageState: { cookies: [], origins: [] } }) // Sin auth

  test('should login with valid credentials', async ({ page }) => {
    const loginPage = new LoginPage(page)
    
    await loginPage.goto()
    await loginPage.login('admin@joyeria.com', 'admin123')
    await loginPage.expectRedirectToDashboard()
    
    // Verificar que se muestra el nombre del usuario
    await expect(page.getByText('Admin Principal')).toBeVisible()
  })

  test('should show error with invalid credentials', async ({ page }) => {
    const loginPage = new LoginPage(page)
    
    await loginPage.goto()
    await loginPage.login('wrong@email.com', 'wrongpassword')
    await loginPage.expectError(/credenciales inválidas/i)
    
    // Verificar que seguimos en login
    await expect(page).toHaveURL('/login')
  })

  test('should validate required fields', async ({ page }) => {
    const loginPage = new LoginPage(page)
    
    await loginPage.goto()
    await loginPage.submitButton.click()
    
    await expect(page.getByText(/el email es requerido/i)).toBeVisible()
    await expect(page.getByText(/la contraseña es requerida/i)).toBeVisible()
  })

  test('should redirect to login when accessing protected route', async ({ page }) => {
    await page.goto('/products')
    await expect(page).toHaveURL(/\/login/)
  })

  test('should logout correctly', async ({ page }) => {
    const loginPage = new LoginPage(page)
    
    // Login
    await loginPage.goto()
    await loginPage.login('admin@joyeria.com', 'admin123')
    await loginPage.expectRedirectToDashboard()
    
    // Logout
    await page.getByRole('button', { name: /cerrar sesión/i }).click()
    
    // Verificar redirección a login
    await expect(page).toHaveURL('/login')
    
    // Verificar que no podemos acceder a rutas protegidas
    await page.goto('/products')
    await expect(page).toHaveURL(/\/login/)
  })
})
```

---

## Tests de Productos (CRUD)

### e2e/products.spec.ts

```typescript
import { test, expect } from '@playwright/test'
import { ProductsPage } from './pages/products.page'

test.describe('Products Management', () => {
  test('should display products list', async ({ page }) => {
    const productsPage = new ProductsPage(page)
    
    await productsPage.goto()
    await productsPage.expectPageTitle('Productos')
    
    // Verificar que hay productos
    await expect(productsPage.productTable).toBeVisible()
  })

  test('should create a new product', async ({ page }) => {
    const productsPage = new ProductsPage(page)
    const newProduct = {
      sku: `TEST-${Date.now()}`,
      name: 'Producto de Prueba E2E',
      price: 999,
      collectionId: '1',
    }
    
    await productsPage.goto()
    await productsPage.createProduct(newProduct)
    
    // Verificar toast de éxito
    await productsPage.waitForToast(/producto creado/i)
    
    // Verificar que aparece en la tabla
    await productsPage.expectProductInTable(newProduct.name)
  })

  test('should search products', async ({ page }) => {
    const productsPage = new ProductsPage(page)
    
    await productsPage.goto()
    await productsPage.searchProduct('Anillo')
    
    // Solo deberían mostrarse productos con "Anillo"
    await productsPage.expectProductInTable('Anillo Oro')
    await productsPage.expectProductNotInTable('Collar Plata')
  })

  test('should edit a product', async ({ page }) => {
    const productsPage = new ProductsPage(page)
    
    await productsPage.goto()
    
    // Click en el producto para editar
    await page.getByText('Anillo Oro').click()
    
    // Verificar que estamos en la página de edición
    await expect(page).toHaveURL(/\/products\/\d+\/edit/)
    
    // Cambiar el nombre
    const nameInput = page.getByLabel(/nombre/i)
    await nameInput.clear()
    await nameInput.fill('Anillo Oro Modificado')
    
    // Guardar
    await page.getByRole('button', { name: /guardar/i }).click()
    
    // Verificar toast de éxito
    await productsPage.waitForToast(/producto actualizado/i)
    
    // Verificar el cambio en la lista
    await productsPage.goto()
    await productsPage.expectProductInTable('Anillo Oro Modificado')
  })

  test('should delete a product', async ({ page }) => {
    const productsPage = new ProductsPage(page)
    
    // Primero crear un producto para eliminar
    const productToDelete = {
      sku: `DEL-${Date.now()}`,
      name: 'Producto Para Eliminar',
      price: 100,
      collectionId: '1',
    }
    
    await productsPage.goto()
    await productsPage.createProduct(productToDelete)
    await productsPage.waitForToast(/producto creado/i)
    
    // Eliminar el producto
    await productsPage.deleteProduct(productToDelete.sku)
    
    // Verificar toast de éxito
    await productsPage.waitForToast(/producto eliminado/i)
    
    // Verificar que ya no está en la tabla
    await productsPage.expectProductNotInTable(productToDelete.name)
  })

  test('should validate product form', async ({ page }) => {
    const productsPage = new ProductsPage(page)
    
    await productsPage.goto()
    await productsPage.createButton.click()
    
    // Intentar guardar sin datos
    await page.getByRole('button', { name: /guardar/i }).click()
    
    // Verificar errores de validación
    await expect(page.getByText(/el sku es requerido/i)).toBeVisible()
    await expect(page.getByText(/el nombre es requerido/i)).toBeVisible()
  })
})
```

---

## Tests de Ventas

### e2e/sales.spec.ts

```typescript
import { test, expect } from '@playwright/test'

test.describe('Sales Registration', () => {
  test('should register a sale manually', async ({ page }) => {
    await page.goto('/sales/register')
    
    // Seleccionar producto
    await page.getByLabel(/producto/i).click()
    await page.getByRole('option', { name: /anillo oro/i }).click()
    
    // Seleccionar método de pago
    await page.getByLabel(/método de pago/i).selectOption('1') // Efectivo
    
    // Registrar venta
    await page.getByRole('button', { name: /registrar venta/i }).click()
    
    // Verificar éxito
    await expect(page.getByText(/venta registrada/i)).toBeVisible()
    
    // Verificar que se muestra el resumen
    await expect(page.getByText(/anillo oro/i)).toBeVisible()
    await expect(page.getByText(/efectivo/i)).toBeVisible()
  })

  test('should show product suggestions from photo', async ({ page }) => {
    await page.goto('/sales/register')
    
    // Simular captura de foto (usando archivo de test)
    const fileInput = page.locator('input[type="file"]')
    await fileInput.setInputFiles('e2e/fixtures/test-product-image.jpg')
    
    // Esperar procesamiento
    await expect(page.getByText(/analizando imagen/i)).toBeVisible()
    
    // Verificar que muestra sugerencias
    await expect(page.getByText(/sugerencias/i)).toBeVisible()
    await expect(page.getByRole('button', { name: /seleccionar/i })).toHaveCount(3)
  })
})
```

---

## Tests Responsive (Mobile)

### e2e/mobile.spec.ts

```typescript
import { test, expect, devices } from '@playwright/test'

test.describe('Mobile Experience', () => {
  test.use({ ...devices['iPhone 12'] })

  test('should show mobile menu', async ({ page }) => {
    await page.goto('/products')
    
    // El sidebar debería estar oculto
    await expect(page.locator('[data-testid="sidebar"]')).not.toBeVisible()
    
    // Debería haber un botón de menú
    const menuButton = page.getByRole('button', { name: /menú/i })
    await expect(menuButton).toBeVisible()
    
    // Click en menú
    await menuButton.click()
    
    // Sidebar debería aparecer como drawer
    await expect(page.locator('[data-testid="sidebar"]')).toBeVisible()
  })

  test('should navigate using mobile menu', async ({ page }) => {
    await page.goto('/dashboard')
    
    // Abrir menú
    await page.getByRole('button', { name: /menú/i }).click()
    
    // Click en productos
    await page.getByRole('link', { name: /productos/i }).click()
    
    // Verificar navegación
    await expect(page).toHaveURL('/products')
    
    // El menú debería cerrarse
    await expect(page.locator('[data-testid="sidebar"]')).not.toBeVisible()
  })

  test('should have touch-friendly buttons', async ({ page }) => {
    await page.goto('/products')
    
    // Verificar que los botones tienen tamaño mínimo para touch (44px)
    const buttons = page.getByRole('button')
    const count = await buttons.count()
    
    for (let i = 0; i < count; i++) {
      const button = buttons.nth(i)
      const box = await button.boundingBox()
      if (box) {
        expect(box.height).toBeGreaterThanOrEqual(44)
      }
    }
  })
})
```

---

## Network Mocking en Playwright

```typescript
import { test, expect } from '@playwright/test'

test.describe('API Mocking', () => {
  test('should handle API errors gracefully', async ({ page }) => {
    // Interceptar requests a productos
    await page.route('**/api/products', (route) => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Internal Server Error' }),
      })
    })
    
    await page.goto('/products')
    
    // Verificar mensaje de error
    await expect(page.getByRole('alert')).toContainText(/error/i)
    await expect(page.getByRole('button', { name: /reintentar/i })).toBeVisible()
  })

  test('should show loading state during slow requests', async ({ page }) => {
    // Simular request lento
    await page.route('**/api/products', async (route) => {
      await new Promise((r) => setTimeout(r, 2000))
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      })
    })
    
    await page.goto('/products')
    
    // Verificar skeleton/loading
    await expect(page.getByTestId('loading-skeleton')).toBeVisible()
  })
})
```

---

## Comandos Útiles

```bash
# Ejecutar todos los tests E2E
npx playwright test

# Ejecutar en modo UI (interactivo)
npx playwright test --ui

# Ejecutar en modo headed (ver navegador)
npx playwright test --headed

# Ejecutar un archivo específico
npx playwright test e2e/auth.spec.ts

# Ejecutar solo un navegador
npx playwright test --project=chromium

# Ejecutar en modo debug
npx playwright test --debug

# Generar tests grabando acciones
npx playwright codegen http://localhost:5173

# Ver reporte HTML
npx playwright show-report

# Actualizar snapshots
npx playwright test --update-snapshots
```

---

## Buenas Prácticas

### ✅ Hacer

```typescript
// ✅ Usar locators semánticos
page.getByRole('button', { name: /guardar/i })
page.getByLabel(/email/i)

// ✅ Usar auto-waiting (no usar sleeps)
await expect(element).toBeVisible()

// ✅ Usar Page Objects para código reutilizable
const productsPage = new ProductsPage(page)

// ✅ Limpiar datos entre tests
test.beforeEach(async ({ page }) => {
  await page.goto('/api/test/reset-db')
})

// ✅ Usar fixtures para datos de test
test.use({ testProduct: { sku: 'TEST-001', name: 'Test' } })
```

### ❌ Evitar

```typescript
// ❌ No usar waits fijos
await page.waitForTimeout(5000) // Frágil

// ❌ No usar selectores frágiles
page.locator('.btn-primary.mt-4.flex-1') // Muy específico

// ❌ No hardcodear datos que pueden cambiar
expect(page.getByText('1234.56€')) // Precio puede cambiar

// ❌ No depender del orden de tests
// Cada test debe ser independiente
```

---

## Próximos Pasos

1. → [06 - GitHub Actions](06-github-actions.md) - CI/CD
2. → [07 - Cobertura de Código](07-cobertura-codigo.md) - Coverage

