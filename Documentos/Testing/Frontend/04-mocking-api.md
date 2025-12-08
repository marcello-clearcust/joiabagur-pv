# 04 - Mocking de API con MSW

Mock Service Worker (MSW) para interceptar requests HTTP en tests y desarrollo.

---

## ¿Qué es MSW?

**MSW (Mock Service Worker)** intercepta requests a nivel de red, no a nivel de código. Esto significa que:

- ✅ Tu código hace requests reales (fetch, axios)
- ✅ MSW los intercepta antes de salir del navegador/Node
- ✅ No necesitas mockear fetch ni axios
- ✅ Funciona igual en tests y en desarrollo

---

## Configuración Inicial

### Instalación

```bash
npm install -D msw
```

### Estructura de Archivos

```
src/test/mocks/
├── handlers.ts      # Request handlers (rutas y respuestas)
├── server.ts        # Servidor para Node (tests)
├── browser.ts       # Worker para navegador (desarrollo)
└── data/
    ├── products.ts  # Datos mock de productos
    ├── users.ts     # Datos mock de usuarios
    └── sales.ts     # Datos mock de ventas
```

---

## Handlers Base

### src/test/mocks/handlers.ts

```typescript
import { http, HttpResponse, delay } from 'msw'

const API_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api'

// ==================== DATOS MOCK ====================

export const mockProducts = [
  {
    id: '1',
    sku: 'ANI-00001',
    name: 'Anillo Oro',
    description: 'Anillo de oro 18k',
    price: 1500,
    collectionId: '1',
    photos: ['https://example.com/anillo1.jpg'],
    createdAt: '2024-01-15T10:00:00Z',
  },
  {
    id: '2',
    sku: 'COL-00001',
    name: 'Collar Plata',
    description: 'Collar de plata 925',
    price: 800,
    collectionId: '2',
    photos: ['https://example.com/collar1.jpg'],
    createdAt: '2024-01-16T10:00:00Z',
  },
]

export const mockUsers = [
  {
    id: '1',
    email: 'admin@joyeria.com',
    name: 'Admin Principal',
    role: 'admin',
    active: true,
  },
  {
    id: '2',
    email: 'operador@joyeria.com',
    name: 'Operador Tienda',
    role: 'operator',
    pointOfSaleId: '1',
    active: true,
  },
]

export const mockCollections = [
  { id: '1', name: 'Anillos' },
  { id: '2', name: 'Collares' },
  { id: '3', name: 'Pulseras' },
]

// ==================== HANDLERS ====================

export const handlers = [
  // ---------- AUTH ----------
  http.post(`${API_URL}/auth/login`, async ({ request }) => {
    const { email, password } = await request.json() as { email: string; password: string }
    
    // Simular delay de red
    await delay(100)

    // Credenciales de prueba
    if (email === 'admin@joyeria.com' && password === 'admin123') {
      return HttpResponse.json({
        token: 'mock-jwt-token-admin',
        refreshToken: 'mock-refresh-token',
        user: mockUsers[0],
      })
    }

    if (email === 'operador@joyeria.com' && password === 'operador123') {
      return HttpResponse.json({
        token: 'mock-jwt-token-operator',
        refreshToken: 'mock-refresh-token',
        user: mockUsers[1],
      })
    }

    return HttpResponse.json(
      { message: 'Credenciales inválidas' },
      { status: 401 }
    )
  }),

  http.get(`${API_URL}/auth/me`, ({ request }) => {
    const authHeader = request.headers.get('Authorization')
    
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return HttpResponse.json(
        { message: 'No autorizado' },
        { status: 401 }
      )
    }

    const token = authHeader.replace('Bearer ', '')
    
    if (token === 'mock-jwt-token-admin') {
      return HttpResponse.json(mockUsers[0])
    }
    
    if (token === 'mock-jwt-token-operator') {
      return HttpResponse.json(mockUsers[1])
    }

    return HttpResponse.json(
      { message: 'Token inválido' },
      { status: 401 }
    )
  }),

  http.post(`${API_URL}/auth/refresh`, async () => {
    await delay(50)
    return HttpResponse.json({
      token: 'mock-jwt-token-refreshed',
      refreshToken: 'mock-refresh-token-new',
    })
  }),

  // ---------- PRODUCTS ----------
  http.get(`${API_URL}/products`, async ({ request }) => {
    await delay(100)
    
    const url = new URL(request.url)
    const search = url.searchParams.get('search')?.toLowerCase()
    const collectionId = url.searchParams.get('collectionId')
    
    let filtered = [...mockProducts]
    
    if (search) {
      filtered = filtered.filter(
        (p) =>
          p.name.toLowerCase().includes(search) ||
          p.sku.toLowerCase().includes(search)
      )
    }
    
    if (collectionId) {
      filtered = filtered.filter((p) => p.collectionId === collectionId)
    }

    return HttpResponse.json(filtered)
  }),

  http.get(`${API_URL}/products/:id`, async ({ params }) => {
    await delay(50)
    const product = mockProducts.find((p) => p.id === params.id)
    
    if (!product) {
      return HttpResponse.json(
        { message: 'Producto no encontrado' },
        { status: 404 }
      )
    }

    return HttpResponse.json(product)
  }),

  http.get(`${API_URL}/products/sku/:sku`, async ({ params }) => {
    await delay(50)
    const product = mockProducts.find((p) => p.sku === params.sku)
    
    if (!product) {
      return HttpResponse.json(
        { message: 'Producto no encontrado' },
        { status: 404 }
      )
    }

    return HttpResponse.json(product)
  }),

  http.post(`${API_URL}/products`, async ({ request }) => {
    await delay(100)
    const body = await request.json() as Record<string, unknown>
    
    // Validación básica
    if (!body.sku || !body.name) {
      return HttpResponse.json(
        { message: 'SKU y nombre son requeridos' },
        { status: 400 }
      )
    }

    // Verificar SKU duplicado
    if (mockProducts.some((p) => p.sku === body.sku)) {
      return HttpResponse.json(
        { message: 'El SKU ya existe' },
        { status: 409 }
      )
    }

    const newProduct = {
      id: String(mockProducts.length + 1),
      ...body,
      createdAt: new Date().toISOString(),
    }

    return HttpResponse.json(newProduct, { status: 201 })
  }),

  http.put(`${API_URL}/products/:id`, async ({ params, request }) => {
    await delay(100)
    const body = await request.json() as Record<string, unknown>
    const product = mockProducts.find((p) => p.id === params.id)
    
    if (!product) {
      return HttpResponse.json(
        { message: 'Producto no encontrado' },
        { status: 404 }
      )
    }

    const updated = { ...product, ...body }
    return HttpResponse.json(updated)
  }),

  http.delete(`${API_URL}/products/:id`, async ({ params }) => {
    await delay(50)
    const product = mockProducts.find((p) => p.id === params.id)
    
    if (!product) {
      return HttpResponse.json(
        { message: 'Producto no encontrado' },
        { status: 404 }
      )
    }

    return new HttpResponse(null, { status: 204 })
  }),

  // ---------- COLLECTIONS ----------
  http.get(`${API_URL}/collections`, async () => {
    await delay(50)
    return HttpResponse.json(mockCollections)
  }),

  // ---------- USERS ----------
  http.get(`${API_URL}/users`, async () => {
    await delay(100)
    return HttpResponse.json(mockUsers)
  }),

  http.get(`${API_URL}/users/:id`, async ({ params }) => {
    const user = mockUsers.find((u) => u.id === params.id)
    
    if (!user) {
      return HttpResponse.json(
        { message: 'Usuario no encontrado' },
        { status: 404 }
      )
    }

    return HttpResponse.json(user)
  }),
]
```

---

## Servidor para Tests (Node)

### src/test/mocks/server.ts

```typescript
import { setupServer } from 'msw/node'
import { handlers } from './handlers'

// Crear servidor con handlers por defecto
export const server = setupServer(...handlers)
```

### Integración con Setup de Vitest

**src/test/setup.ts**
```typescript
import '@testing-library/jest-dom'
import { cleanup } from '@testing-library/react'
import { afterEach, beforeAll, afterAll } from 'vitest'
import { server } from './mocks/server'

// Limpieza automática de React Testing Library
afterEach(() => {
  cleanup()
})

// MSW Setup
beforeAll(() => {
  // Iniciar servidor con modo estricto
  // 'error' lanza error si hay request no manejado
  server.listen({ onUnhandledRequest: 'error' })
})

afterEach(() => {
  // Resetear handlers a los defaults después de cada test
  server.resetHandlers()
})

afterAll(() => {
  // Cerrar servidor al terminar
  server.close()
})
```

---

## Worker para Desarrollo (Browser)

### src/test/mocks/browser.ts

```typescript
import { setupWorker } from 'msw/browser'
import { handlers } from './handlers'

export const worker = setupWorker(...handlers)
```

### Inicializar en Desarrollo

**src/main.tsx**
```typescript
import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'

async function enableMocking() {
  // Solo habilitar MSW en desarrollo si está configurado
  if (import.meta.env.DEV && import.meta.env.VITE_ENABLE_MSW === 'true') {
    const { worker } = await import('./test/mocks/browser')
    return worker.start({
      onUnhandledRequest: 'bypass', // No bloquear requests no mockeados
    })
  }
}

enableMocking().then(() => {
  ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
      <App />
    </React.StrictMode>
  )
})
```

---

## Override de Handlers en Tests

### Escenarios de Error

```typescript
import { describe, it, expect } from 'vitest'
import { render, screen, waitFor } from '@/test/utils/render'
import { server } from '@/test/mocks/server'
import { http, HttpResponse } from 'msw'
import { ProductList } from './product-list'

const API_URL = import.meta.env.VITE_API_BASE_URL

describe('ProductList - Error Scenarios', () => {
  it('should show error when API returns 500', async () => {
    // Override del handler solo para este test
    server.use(
      http.get(`${API_URL}/products`, () => {
        return HttpResponse.json(
          { message: 'Internal Server Error' },
          { status: 500 }
        )
      })
    )

    render(<ProductList />)

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent(/error/i)
    })
  })

  it('should show network error message', async () => {
    server.use(
      http.get(`${API_URL}/products`, () => {
        return HttpResponse.error() // Simula error de red
      })
    )

    render(<ProductList />)

    await waitFor(() => {
      expect(screen.getByText(/error de conexión/i)).toBeInTheDocument()
    })
  })

  it('should show unauthorized message when 401', async () => {
    server.use(
      http.get(`${API_URL}/products`, () => {
        return HttpResponse.json(
          { message: 'No autorizado' },
          { status: 401 }
        )
      })
    )

    render(<ProductList />)

    await waitFor(() => {
      expect(screen.getByText(/sesión expirada/i)).toBeInTheDocument()
    })
  })
})
```

### Simular Latencia

```typescript
import { http, HttpResponse, delay } from 'msw'

describe('ProductList - Loading States', () => {
  it('should show loading state for slow requests', async () => {
    server.use(
      http.get(`${API_URL}/products`, async () => {
        // Simular request lento (2 segundos)
        await delay(2000)
        return HttpResponse.json([])
      })
    )

    render(<ProductList />)

    // Verificar que muestra loading
    expect(screen.getByTestId('loading-skeleton')).toBeInTheDocument()

    // El loading debe mantenerse durante el delay
    await new Promise((r) => setTimeout(r, 500))
    expect(screen.getByTestId('loading-skeleton')).toBeInTheDocument()
  })
})
```

### Respuestas Dinámicas

```typescript
describe('ProductList - Dynamic Responses', () => {
  it('should handle pagination', async () => {
    let page = 1
    
    server.use(
      http.get(`${API_URL}/products`, ({ request }) => {
        const url = new URL(request.url)
        page = parseInt(url.searchParams.get('page') || '1')
        
        const products = page === 1
          ? [{ id: '1', name: 'Product 1' }, { id: '2', name: 'Product 2' }]
          : [{ id: '3', name: 'Product 3' }, { id: '4', name: 'Product 4' }]
        
        return HttpResponse.json({
          data: products,
          pagination: {
            page,
            totalPages: 2,
            totalItems: 4,
          },
        })
      })
    )

    render(<ProductList />)

    // Verificar página 1
    await waitFor(() => {
      expect(screen.getByText('Product 1')).toBeInTheDocument()
    })

    // Cambiar a página 2
    await user.click(screen.getByRole('button', { name: /siguiente/i }))

    await waitFor(() => {
      expect(screen.getByText('Product 3')).toBeInTheDocument()
    })
  })
})
```

---

## Handlers para Formularios

### File Upload

```typescript
http.post(`${API_URL}/products/:id/photos`, async ({ params, request }) => {
  const formData = await request.formData()
  const file = formData.get('photo') as File
  
  if (!file) {
    return HttpResponse.json(
      { message: 'No se proporcionó archivo' },
      { status: 400 }
    )
  }

  // Validar tipo de archivo
  if (!file.type.startsWith('image/')) {
    return HttpResponse.json(
      { message: 'El archivo debe ser una imagen' },
      { status: 400 }
    )
  }

  // Simular respuesta exitosa
  return HttpResponse.json({
    id: 'photo-123',
    url: `https://example.com/photos/${params.id}/${file.name}`,
    filename: file.name,
    size: file.size,
  })
})
```

### Validación de Formularios

```typescript
http.post(`${API_URL}/users`, async ({ request }) => {
  const body = await request.json() as {
    email?: string
    name?: string
    password?: string
    role?: string
  }
  
  const errors: Record<string, string> = {}

  if (!body.email) {
    errors.email = 'El email es requerido'
  } else if (!body.email.includes('@')) {
    errors.email = 'Email inválido'
  }

  if (!body.name || body.name.length < 2) {
    errors.name = 'El nombre debe tener al menos 2 caracteres'
  }

  if (!body.password || body.password.length < 8) {
    errors.password = 'La contraseña debe tener al menos 8 caracteres'
  }

  if (Object.keys(errors).length > 0) {
    return HttpResponse.json(
      { message: 'Errores de validación', errors },
      { status: 422 }
    )
  }

  return HttpResponse.json(
    {
      id: '999',
      email: body.email,
      name: body.name,
      role: body.role || 'operator',
      active: true,
    },
    { status: 201 }
  )
})
```

---

## Test de Handlers

### Verificar que Handlers Funcionan

```typescript
import { describe, it, expect } from 'vitest'
import { server } from './server'
import { handlers } from './handlers'

describe('MSW Handlers', () => {
  it('should have handlers for all main endpoints', () => {
    const handlerPaths = handlers.map((h) => h.info.path)
    
    expect(handlerPaths).toContain('http://localhost:5000/api/auth/login')
    expect(handlerPaths).toContain('http://localhost:5000/api/products')
    expect(handlerPaths).toContain('http://localhost:5000/api/users')
  })

  it('should return products from GET /products', async () => {
    const response = await fetch('http://localhost:5000/api/products')
    const data = await response.json()

    expect(response.status).toBe(200)
    expect(Array.isArray(data)).toBe(true)
    expect(data.length).toBeGreaterThan(0)
  })

  it('should return 401 for invalid login', async () => {
    const response = await fetch('http://localhost:5000/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: 'wrong@test.com', password: 'wrong' }),
    })

    expect(response.status).toBe(401)
  })
})
```

---

## Debugging de MSW

### Logging de Requests

```typescript
// En setup.ts para ver todos los requests
server.events.on('request:start', ({ request }) => {
  console.log('MSW intercepted:', request.method, request.url)
})

server.events.on('request:match', ({ request }) => {
  console.log('MSW matched:', request.method, request.url)
})

server.events.on('request:unhandled', ({ request }) => {
  console.warn('MSW unhandled:', request.method, request.url)
})
```

### Verificar Requests en Tests

```typescript
import { describe, it, expect, vi } from 'vitest'
import { server } from '@/test/mocks/server'
import { http, HttpResponse } from 'msw'

describe('API calls', () => {
  it('should send correct data to API', async () => {
    const capturedRequest = vi.fn()
    
    server.use(
      http.post(`${API_URL}/products`, async ({ request }) => {
        const body = await request.json()
        capturedRequest(body)
        return HttpResponse.json({ id: '1', ...body })
      })
    )

    // Ejecutar acción que hace el request
    await createProduct({ sku: 'TEST-001', name: 'Test', price: 100 })

    // Verificar que se envió correctamente
    expect(capturedRequest).toHaveBeenCalledWith({
      sku: 'TEST-001',
      name: 'Test',
      price: 100,
    })
  })
})
```

---

## Buenas Prácticas

### ✅ Hacer

```typescript
// ✅ Usar delays realistas
await delay(100) // Simula latencia de red

// ✅ Retornar códigos HTTP correctos
return HttpResponse.json(data, { status: 201 }) // Created
return HttpResponse.json(null, { status: 204 }) // No Content

// ✅ Manejar errores como lo haría el backend real
return HttpResponse.json(
  { message: 'Not found', code: 'RESOURCE_NOT_FOUND' },
  { status: 404 }
)

// ✅ Resetear handlers después de cada test
afterEach(() => server.resetHandlers())
```

### ❌ Evitar

```typescript
// ❌ No usar onUnhandledRequest: 'bypass' en tests
server.listen({ onUnhandledRequest: 'bypass' }) // Oculta errores

// ❌ No olvidar resetear handlers
// Si no reseteas, los overrides persisten entre tests

// ❌ No mockear fetch directamente cuando usas MSW
vi.mock('fetch') // Innecesario con MSW
```

---

## Próximos Pasos

1. → [05 - Tests E2E](05-tests-e2e.md) - Playwright
2. → [06 - GitHub Actions](06-github-actions.md) - CI/CD
