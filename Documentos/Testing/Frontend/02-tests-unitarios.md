# 02 - Tests Unitarios

Tests de funciones puras, hooks personalizados y utilities del frontend.

---

## Tests de Funciones Puras

### Ejemplo: Testing de Utilities

**src/lib/utils.ts**
```typescript
import { clsx, type ClassValue } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function formatCurrency(amount: number, currency = 'EUR'): string {
  return new Intl.NumberFormat('es-ES', {
    style: 'currency',
    currency,
  }).format(amount)
}

export function formatDate(date: Date | string): string {
  const d = typeof date === 'string' ? new Date(date) : date
  return new Intl.DateTimeFormat('es-ES', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(d)
}

export function generateSku(prefix: string, id: number): string {
  return `${prefix}-${String(id).padStart(5, '0')}`
}

export function slugify(text: string): string {
  return text
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '')
}
```

**src/lib/utils.test.ts**
```typescript
import { describe, it, expect } from 'vitest'
import { cn, formatCurrency, formatDate, generateSku, slugify } from './utils'

describe('cn (className merger)', () => {
  it('should merge class names', () => {
    expect(cn('foo', 'bar')).toBe('foo bar')
  })

  it('should handle conditional classes', () => {
    expect(cn('base', false && 'hidden', true && 'visible')).toBe('base visible')
  })

  it('should merge Tailwind classes correctly', () => {
    expect(cn('px-2 py-1', 'px-4')).toBe('py-1 px-4')
  })

  it('should handle undefined and null', () => {
    expect(cn('foo', undefined, null, 'bar')).toBe('foo bar')
  })
})

describe('formatCurrency', () => {
  it('should format currency in EUR by default', () => {
    expect(formatCurrency(1234.56)).toBe('1.234,56 €')
  })

  it('should format currency in USD', () => {
    expect(formatCurrency(1234.56, 'USD')).toBe('1.234,56 US$')
  })

  it('should handle zero', () => {
    expect(formatCurrency(0)).toBe('0,00 €')
  })

  it('should handle negative numbers', () => {
    expect(formatCurrency(-50)).toBe('-50,00 €')
  })
})

describe('formatDate', () => {
  it('should format Date object', () => {
    const date = new Date('2024-03-15')
    expect(formatDate(date)).toBe('15 de marzo de 2024')
  })

  it('should format ISO date string', () => {
    expect(formatDate('2024-12-25')).toBe('25 de diciembre de 2024')
  })
})

describe('generateSku', () => {
  it('should generate SKU with padded number', () => {
    expect(generateSku('ANI', 1)).toBe('ANI-00001')
    expect(generateSku('COL', 123)).toBe('COL-00123')
    expect(generateSku('PUL', 99999)).toBe('PUL-99999')
  })
})

describe('slugify', () => {
  it('should convert text to slug', () => {
    expect(slugify('Anillo de Oro')).toBe('anillo-de-oro')
  })

  it('should remove accents', () => {
    expect(slugify('Joyería España')).toBe('joyeria-espana')
  })

  it('should remove special characters', () => {
    expect(slugify('Collar (Premium)!')).toBe('collar-premium')
  })

  it('should handle multiple spaces', () => {
    expect(slugify('Pulsera   Plata')).toBe('pulsera-plata')
  })
})
```

---

## Tests de Hooks Personalizados

### Hook de Autenticación

**src/hooks/use-auth.ts**
```typescript
import { useState, useCallback, useEffect } from 'react'

interface User {
  id: string
  email: string
  name: string
  role: 'admin' | 'operator'
}

interface AuthState {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
}

interface UseAuthReturn extends AuthState {
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  clearError: () => void
}

const TOKEN_KEY = 'auth_token'

export function useAuth(): UseAuthReturn {
  const [state, setState] = useState<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
    error: null,
  })

  useEffect(() => {
    const token = localStorage.getItem(TOKEN_KEY)
    if (token) {
      // Validar token y obtener usuario
      validateToken(token)
        .then((user) => {
          setState({ user, isAuthenticated: true, isLoading: false, error: null })
        })
        .catch(() => {
          localStorage.removeItem(TOKEN_KEY)
          setState({ user: null, isAuthenticated: false, isLoading: false, error: null })
        })
    } else {
      setState((prev) => ({ ...prev, isLoading: false }))
    }
  }, [])

  const login = useCallback(async (email: string, password: string) => {
    setState((prev) => ({ ...prev, isLoading: true, error: null }))
    
    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      })

      if (!response.ok) {
        throw new Error('Credenciales inválidas')
      }

      const { token, user } = await response.json()
      localStorage.setItem(TOKEN_KEY, token)
      setState({ user, isAuthenticated: true, isLoading: false, error: null })
    } catch (error) {
      setState((prev) => ({
        ...prev,
        isLoading: false,
        error: error instanceof Error ? error.message : 'Error de autenticación',
      }))
      throw error
    }
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY)
    setState({ user: null, isAuthenticated: false, isLoading: false, error: null })
  }, [])

  const clearError = useCallback(() => {
    setState((prev) => ({ ...prev, error: null }))
  }, [])

  return { ...state, login, logout, clearError }
}

async function validateToken(token: string): Promise<User> {
  const response = await fetch('/api/auth/me', {
    headers: { Authorization: `Bearer ${token}` },
  })
  if (!response.ok) throw new Error('Token inválido')
  return response.json()
}
```

**src/hooks/use-auth.test.ts**
```typescript
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { renderHook, act, waitFor } from '@testing-library/react'
import { useAuth } from './use-auth'

// Mock de fetch
const mockFetch = vi.fn()
global.fetch = mockFetch

// Mock de localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
}
Object.defineProperty(window, 'localStorage', { value: localStorageMock })

describe('useAuth', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorageMock.getItem.mockReturnValue(null)
  })

  afterEach(() => {
    vi.resetAllMocks()
  })

  describe('initial state', () => {
    it('should start with loading state', () => {
      const { result } = renderHook(() => useAuth())

      expect(result.current.isLoading).toBe(true)
      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
    })

    it('should finish loading when no token exists', async () => {
      const { result } = renderHook(() => useAuth())

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false)
      })

      expect(result.current.isAuthenticated).toBe(false)
    })

    it('should validate existing token on mount', async () => {
      const mockUser = { id: '1', email: 'test@test.com', name: 'Test', role: 'admin' as const }
      localStorageMock.getItem.mockReturnValue('valid-token')
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockUser),
      })

      const { result } = renderHook(() => useAuth())

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false)
      })

      expect(result.current.isAuthenticated).toBe(true)
      expect(result.current.user).toEqual(mockUser)
    })
  })

  describe('login', () => {
    it('should login successfully with valid credentials', async () => {
      const mockUser = { id: '1', email: 'test@test.com', name: 'Test', role: 'admin' as const }
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({ token: 'new-token', user: mockUser }),
      })

      const { result } = renderHook(() => useAuth())

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false)
      })

      await act(async () => {
        await result.current.login('test@test.com', 'password123')
      })

      expect(result.current.isAuthenticated).toBe(true)
      expect(result.current.user).toEqual(mockUser)
      expect(localStorageMock.setItem).toHaveBeenCalledWith('auth_token', 'new-token')
    })

    it('should handle login error', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
      })

      const { result } = renderHook(() => useAuth())

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false)
      })

      await expect(
        act(async () => {
          await result.current.login('test@test.com', 'wrong-password')
        })
      ).rejects.toThrow('Credenciales inválidas')

      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.error).toBe('Credenciales inválidas')
    })
  })

  describe('logout', () => {
    it('should clear auth state and token', async () => {
      const mockUser = { id: '1', email: 'test@test.com', name: 'Test', role: 'admin' as const }
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({ token: 'token', user: mockUser }),
      })

      const { result } = renderHook(() => useAuth())

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false)
      })

      await act(async () => {
        await result.current.login('test@test.com', 'password123')
      })

      expect(result.current.isAuthenticated).toBe(true)

      act(() => {
        result.current.logout()
      })

      expect(result.current.isAuthenticated).toBe(false)
      expect(result.current.user).toBeNull()
      expect(localStorageMock.removeItem).toHaveBeenCalledWith('auth_token')
    })
  })

  describe('clearError', () => {
    it('should clear error state', async () => {
      mockFetch.mockResolvedValueOnce({ ok: false, status: 401 })

      const { result } = renderHook(() => useAuth())

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false)
      })

      try {
        await act(async () => {
          await result.current.login('test@test.com', 'wrong')
        })
      } catch {
        // Expected error
      }

      expect(result.current.error).toBe('Credenciales inválidas')

      act(() => {
        result.current.clearError()
      })

      expect(result.current.error).toBeNull()
    })
  })
})
```

---

## Tests con Datos Parametrizados

### Usando it.each

```typescript
import { describe, it, expect } from 'vitest'
import { validateEmail, validatePassword, validateSku } from './validators'

describe('validateEmail', () => {
  it.each([
    ['test@example.com', true],
    ['user.name@domain.co', true],
    ['invalid-email', false],
    ['@nodomain.com', false],
    ['spaces in@email.com', false],
    ['', false],
  ])('validateEmail("%s") should return %s', (email, expected) => {
    expect(validateEmail(email)).toBe(expected)
  })
})

describe('validatePassword', () => {
  const validPasswords = [
    'Password123!',
    'Secure@Pass1',
    'MyP@ssw0rd',
  ]

  const invalidPasswords = [
    { password: 'short', reason: 'too short' },
    { password: 'nouppercase1!', reason: 'no uppercase' },
    { password: 'NOLOWERCASE1!', reason: 'no lowercase' },
    { password: 'NoNumbers!', reason: 'no numbers' },
    { password: 'NoSpecial123', reason: 'no special characters' },
  ]

  it.each(validPasswords)('should accept valid password: %s', (password) => {
    expect(validatePassword(password).isValid).toBe(true)
  })

  it.each(invalidPasswords)(
    'should reject password with $reason',
    ({ password }) => {
      expect(validatePassword(password).isValid).toBe(false)
    }
  )
})

describe('validateSku', () => {
  it.each([
    ['ANI-00001', true, 'valid format'],
    ['COL-12345', true, 'valid format'],
    ['ani-00001', false, 'lowercase prefix'],
    ['ANI00001', false, 'missing dash'],
    ['ANI-0001', false, 'only 4 digits'],
    ['ANI-000001', false, '6 digits'],
    ['ANILLO-00001', false, 'prefix too long'],
  ])('validateSku("%s") should be %s (%s)', (sku, expected) => {
    expect(validateSku(sku)).toBe(expected)
  })
})
```

---

## Tests de Funciones Async

### Servicios de API

**src/services/products.service.ts**
```typescript
const API_BASE = import.meta.env.VITE_API_BASE_URL

export interface Product {
  id: string
  sku: string
  name: string
  description?: string
  price: number
  collectionId: string
}

export interface CreateProductDto {
  sku: string
  name: string
  description?: string
  price: number
  collectionId: string
}

export async function getProducts(): Promise<Product[]> {
  const response = await fetch(`${API_BASE}/products`)
  if (!response.ok) throw new Error('Error fetching products')
  return response.json()
}

export async function getProductBySku(sku: string): Promise<Product | null> {
  const response = await fetch(`${API_BASE}/products/sku/${sku}`)
  if (response.status === 404) return null
  if (!response.ok) throw new Error('Error fetching product')
  return response.json()
}

export async function createProduct(data: CreateProductDto): Promise<Product> {
  const response = await fetch(`${API_BASE}/products`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  })
  if (!response.ok) {
    const error = await response.json()
    throw new Error(error.message || 'Error creating product')
  }
  return response.json()
}
```

**src/services/products.service.test.ts**
```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { getProducts, getProductBySku, createProduct } from './products.service'

const mockFetch = vi.fn()
global.fetch = mockFetch

describe('ProductsService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('getProducts', () => {
    it('should fetch all products', async () => {
      const mockProducts = [
        { id: '1', sku: 'ANI-001', name: 'Anillo', price: 100, collectionId: '1' },
        { id: '2', sku: 'COL-001', name: 'Collar', price: 200, collectionId: '1' },
      ]
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockProducts),
      })

      const result = await getProducts()

      expect(result).toEqual(mockProducts)
      expect(mockFetch).toHaveBeenCalledWith(expect.stringContaining('/products'))
    })

    it('should throw error on failure', async () => {
      mockFetch.mockResolvedValueOnce({ ok: false, status: 500 })

      await expect(getProducts()).rejects.toThrow('Error fetching products')
    })
  })

  describe('getProductBySku', () => {
    it('should return product when found', async () => {
      const mockProduct = { id: '1', sku: 'ANI-001', name: 'Anillo', price: 100, collectionId: '1' }
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockProduct),
      })

      const result = await getProductBySku('ANI-001')

      expect(result).toEqual(mockProduct)
    })

    it('should return null when product not found', async () => {
      mockFetch.mockResolvedValueOnce({ ok: false, status: 404 })

      const result = await getProductBySku('INVALID')

      expect(result).toBeNull()
    })
  })

  describe('createProduct', () => {
    it('should create product successfully', async () => {
      const newProduct = { sku: 'ANI-002', name: 'Anillo Plata', price: 150, collectionId: '1' }
      const createdProduct = { id: '3', ...newProduct }
      
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(createdProduct),
      })

      const result = await createProduct(newProduct)

      expect(result).toEqual(createdProduct)
      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/products'),
        expect.objectContaining({
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(newProduct),
        })
      )
    })

    it('should throw error with message from API', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        json: () => Promise.resolve({ message: 'SKU already exists' }),
      })

      await expect(
        createProduct({ sku: 'ANI-001', name: 'Duplicate', price: 100, collectionId: '1' })
      ).rejects.toThrow('SKU already exists')
    })
  })
})
```

---

## Mocking con Vitest

### Mocking de Módulos

```typescript
import { vi, describe, it, expect, beforeEach } from 'vitest'

// Mock de un módulo completo
vi.mock('./api-client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}))

// Importar después del mock
import { apiClient } from './api-client'
import { fetchUserProfile } from './user.service'

describe('UserService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should fetch user profile', async () => {
    const mockProfile = { id: '1', name: 'John' }
    vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockProfile })

    const result = await fetchUserProfile('1')

    expect(apiClient.get).toHaveBeenCalledWith('/users/1')
    expect(result).toEqual(mockProfile)
  })
})
```

### Spies

```typescript
import { vi, describe, it, expect } from 'vitest'

describe('Event tracking', () => {
  it('should track button click', () => {
    const trackEvent = vi.fn()
    
    // Simular clic
    trackEvent('button_click', { buttonId: 'submit' })
    
    expect(trackEvent).toHaveBeenCalledTimes(1)
    expect(trackEvent).toHaveBeenCalledWith('button_click', { buttonId: 'submit' })
  })
})
```

### Mock de Timers

```typescript
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest'
import { debounce } from './debounce'

describe('debounce', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('should debounce function calls', () => {
    const fn = vi.fn()
    const debouncedFn = debounce(fn, 300)

    debouncedFn()
    debouncedFn()
    debouncedFn()

    expect(fn).not.toHaveBeenCalled()

      vi.advanceTimersByTime(300)

    expect(fn).toHaveBeenCalledTimes(1)
  })
})
```

---

## Generación de Datos de Test

### Factory Functions

**src/test/utils/test-data.ts**
```typescript
import { faker } from '@faker-js/faker/locale/es'

export function createProduct(overrides = {}) {
  return {
    id: faker.string.uuid(),
    sku: `${faker.string.alpha({ length: 3, casing: 'upper' })}-${faker.string.numeric(5)}`,
    name: faker.commerce.productName(),
    description: faker.commerce.productDescription(),
    price: parseFloat(faker.commerce.price({ min: 10, max: 5000 })),
    collectionId: faker.string.uuid(),
    createdAt: faker.date.past().toISOString(),
    updatedAt: faker.date.recent().toISOString(),
    ...overrides,
  }
}

export function createUser(overrides = {}) {
  return {
    id: faker.string.uuid(),
    email: faker.internet.email(),
    name: faker.person.fullName(),
    role: faker.helpers.arrayElement(['admin', 'operator'] as const),
    pointOfSaleId: faker.string.uuid(),
    active: true,
    createdAt: faker.date.past().toISOString(),
    ...overrides,
  }
}

export function createSale(overrides = {}) {
  return {
    id: faker.string.uuid(),
    productId: faker.string.uuid(),
    pointOfSaleId: faker.string.uuid(),
    operatorId: faker.string.uuid(),
    paymentMethodId: faker.string.uuid(),
    quantity: faker.number.int({ min: 1, max: 5 }),
    unitPrice: parseFloat(faker.commerce.price({ min: 10, max: 5000 })),
    totalPrice: 0, // Se calcula
    saleDate: faker.date.recent().toISOString(),
    ...overrides,
  }
}

// Builder pattern para casos complejos
export class ProductBuilder {
  private product = createProduct()

  withSku(sku: string) {
    this.product.sku = sku
    return this
  }

  withPrice(price: number) {
    this.product.price = price
    return this
  }

  withCollection(collectionId: string) {
    this.product.collectionId = collectionId
    return this
  }

  build() {
    return { ...this.product }
  }
}
```

### Uso en tests

```typescript
import { describe, it, expect } from 'vitest'
import { createProduct, createUser, ProductBuilder } from '@/test/utils/test-data'

describe('Product calculations', () => {
  it('should calculate discount correctly', () => {
    const product = createProduct({ price: 100 })
    const discountedPrice = applyDiscount(product, 0.2)
    expect(discountedPrice).toBe(80)
  })

  it('should work with builder pattern', () => {
    const expensiveProduct = new ProductBuilder()
      .withSku('LUX-00001')
      .withPrice(10000)
      .build()

    expect(expensiveProduct.sku).toBe('LUX-00001')
    expect(expensiveProduct.price).toBe(10000)
  })
})
```

---

## Próximos Pasos

1. → [03 - Tests de Componentes](03-tests-componentes.md) - React Testing Library
2. → [04 - Mocking de API](04-mocking-api.md) - MSW
