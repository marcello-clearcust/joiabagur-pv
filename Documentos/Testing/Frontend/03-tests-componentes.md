# 03 - Tests de Componentes

Testing de componentes React con React Testing Library y Vitest.

---

## Principios de React Testing Library

### Filosofía

> "The more your tests resemble the way your software is used, the more confidence they can give you."
> — Kent C. Dodds

**Principios clave:**
1. Testear comportamiento, no implementación
2. Usar queries accesibles (como lo haría un usuario)
3. Evitar testear detalles internos del componente
4. Preferir `userEvent` sobre `fireEvent`

---

## Custom Render con Providers

### src/test/utils/render.tsx

```typescript
import { ReactElement, ReactNode } from 'react'
import { render, RenderOptions } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { ThemeProvider } from '@/components/theme-provider'
import { AuthProvider } from '@/providers/auth-provider'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

// Crear QueryClient para tests
function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: 0,
      },
    },
  })
}

interface AllProvidersProps {
  children: ReactNode
}

function AllProviders({ children }: AllProvidersProps) {
  const queryClient = createTestQueryClient()

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <ThemeProvider defaultTheme="light" storageKey="test-theme">
          <AuthProvider>
            {children}
          </AuthProvider>
        </ThemeProvider>
      </BrowserRouter>
    </QueryClientProvider>
  )
}

// Custom render que incluye todos los providers
function customRender(
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) {
  return render(ui, { wrapper: AllProviders, ...options })
}

// Re-exportar todo de testing-library
export * from '@testing-library/react'
export { customRender as render }
```

### Uso del custom render

```typescript
import { describe, it, expect } from 'vitest'
import { render, screen } from '@/test/utils/render'
import { ProductCard } from './product-card'

describe('ProductCard', () => {
  it('should render with all providers', () => {
    render(<ProductCard product={mockProduct} />)
    // El componente tiene acceso a Router, Theme, Auth, etc.
  })
})
```

---

## Queries de Testing Library

### Prioridad de Queries (de mayor a menor)

| Prioridad | Query | Cuándo usar |
|-----------|-------|-------------|
| 1️⃣ | `getByRole` | Cualquier elemento con rol ARIA |
| 2️⃣ | `getByLabelText` | Campos de formulario |
| 3️⃣ | `getByPlaceholderText` | Inputs sin label visible |
| 4️⃣ | `getByText` | Texto visible para el usuario |
| 5️⃣ | `getByDisplayValue` | Valor actual de input |
| 6️⃣ | `getByAltText` | Imágenes |
| 7️⃣ | `getByTitle` | Elementos con title |
| 8️⃣ | `getByTestId` | Último recurso |

### Variantes de Queries

| Variante | No encuentra | Encuentra 1 | Encuentra >1 | Async |
|----------|-------------|-------------|--------------|-------|
| `getBy` | ❌ Error | ✅ Retorna | ❌ Error | No |
| `queryBy` | ✅ null | ✅ Retorna | ❌ Error | No |
| `findBy` | ❌ Error | ✅ Retorna | ❌ Error | Sí |
| `getAllBy` | ❌ Error | ✅ Array | ✅ Array | No |
| `queryAllBy` | ✅ [] | ✅ Array | ✅ Array | No |
| `findAllBy` | ❌ Error | ✅ Array | ✅ Array | Sí |

### Ejemplos de Queries

```typescript
import { render, screen } from '@testing-library/react'

// ✅ getByRole - Preferido
screen.getByRole('button', { name: /guardar/i })
screen.getByRole('heading', { level: 1 })
screen.getByRole('textbox', { name: /email/i })
screen.getByRole('combobox', { name: /colección/i })
screen.getByRole('checkbox', { name: /activo/i })
screen.getByRole('link', { name: /ver detalles/i })

// ✅ getByLabelText - Para formularios
screen.getByLabelText(/nombre del producto/i)
screen.getByLabelText('Email')

// ✅ getByText - Para texto visible
screen.getByText('Guardar cambios')
screen.getByText(/no hay productos/i)

// ✅ findBy - Para elementos async
await screen.findByText('Producto guardado')
await screen.findByRole('alert')

// ✅ queryBy - Para verificar que NO existe
expect(screen.queryByText('Error')).not.toBeInTheDocument()

// ⚠️ getByTestId - Solo cuando no hay otra opción
screen.getByTestId('product-grid')
```

---

## Tests de Componentes Básicos

### Componente Button

**src/components/ui/button.tsx**
```typescript
import { ButtonHTMLAttributes, forwardRef } from 'react'
import { cn } from '@/lib/utils'
import { Loader2 } from 'lucide-react'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost'
  size?: 'sm' | 'md' | 'lg'
  isLoading?: boolean
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant = 'primary', size = 'md', isLoading, children, disabled, ...props }, ref) => {
    return (
      <button
        ref={ref}
        className={cn(
          'inline-flex items-center justify-center rounded-md font-medium transition-colors',
          'focus-visible:outline-none focus-visible:ring-2',
          {
            'bg-blue-600 text-white hover:bg-blue-700': variant === 'primary',
            'bg-gray-200 text-gray-900 hover:bg-gray-300': variant === 'secondary',
            'bg-red-600 text-white hover:bg-red-700': variant === 'danger',
            'hover:bg-gray-100': variant === 'ghost',
            'h-8 px-3 text-sm': size === 'sm',
            'h-10 px-4': size === 'md',
            'h-12 px-6 text-lg': size === 'lg',
            'opacity-50 cursor-not-allowed': disabled || isLoading,
          },
          className
        )}
        disabled={disabled || isLoading}
        {...props}
      >
        {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
        {children}
      </button>
    )
  }
)
Button.displayName = 'Button'
```

**src/components/ui/button.test.tsx**
```typescript
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Button } from './button'

describe('Button', () => {
  describe('rendering', () => {
    it('should render children text', () => {
      render(<Button>Click me</Button>)
      expect(screen.getByRole('button', { name: /click me/i })).toBeInTheDocument()
    })

    it('should apply variant classes', () => {
      const { rerender } = render(<Button variant="primary">Primary</Button>)
      expect(screen.getByRole('button')).toHaveClass('bg-blue-600')

      rerender(<Button variant="danger">Danger</Button>)
      expect(screen.getByRole('button')).toHaveClass('bg-red-600')
    })

    it('should apply size classes', () => {
      const { rerender } = render(<Button size="sm">Small</Button>)
      expect(screen.getByRole('button')).toHaveClass('h-8')

      rerender(<Button size="lg">Large</Button>)
      expect(screen.getByRole('button')).toHaveClass('h-12')
    })
  })

  describe('interactions', () => {
    it('should call onClick when clicked', async () => {
      const user = userEvent.setup()
      const handleClick = vi.fn()
      render(<Button onClick={handleClick}>Click me</Button>)

      await user.click(screen.getByRole('button'))

      expect(handleClick).toHaveBeenCalledTimes(1)
    })

    it('should not call onClick when disabled', async () => {
      const user = userEvent.setup()
      const handleClick = vi.fn()
      render(<Button onClick={handleClick} disabled>Click me</Button>)

      await user.click(screen.getByRole('button'))

      expect(handleClick).not.toHaveBeenCalled()
    })
  })

  describe('loading state', () => {
    it('should show loading spinner when isLoading', () => {
      render(<Button isLoading>Loading</Button>)
      
      expect(screen.getByRole('button')).toBeDisabled()
      // El spinner tiene la clase animate-spin
      expect(document.querySelector('.animate-spin')).toBeInTheDocument()
    })

    it('should not call onClick when loading', async () => {
      const user = userEvent.setup()
      const handleClick = vi.fn()
      render(<Button onClick={handleClick} isLoading>Loading</Button>)

      await user.click(screen.getByRole('button'))

      expect(handleClick).not.toHaveBeenCalled()
    })
  })

  describe('accessibility', () => {
    it('should be focusable', async () => {
      const user = userEvent.setup()
      render(<Button>Focus me</Button>)

      await user.tab()

      expect(screen.getByRole('button')).toHaveFocus()
    })

    it('should support aria-label', () => {
      render(<Button aria-label="Close modal">×</Button>)
      expect(screen.getByRole('button', { name: /close modal/i })).toBeInTheDocument()
    })
  })
})
```

---

## Tests de Formularios

### Componente ProductForm

**src/components/products/product-form.test.tsx**
```typescript
import { describe, it, expect, vi } from 'vitest'
import { render, screen, waitFor } from '@/test/utils/render'
import userEvent from '@testing-library/user-event'
import { ProductForm } from './product-form'

const mockCollections = [
  { id: '1', name: 'Anillos' },
  { id: '2', name: 'Collares' },
]

describe('ProductForm', () => {
  const defaultProps = {
    collections: mockCollections,
    onSubmit: vi.fn(),
    onCancel: vi.fn(),
  }

  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('rendering', () => {
    it('should render all form fields', () => {
      render(<ProductForm {...defaultProps} />)

      expect(screen.getByLabelText(/sku/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/nombre/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/descripción/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/precio/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/colección/i)).toBeInTheDocument()
    })

    it('should render with initial values when editing', () => {
      const product = {
        id: '1',
        sku: 'ANI-00001',
        name: 'Anillo Oro',
        description: 'Anillo de oro 18k',
        price: 1500,
        collectionId: '1',
      }
      render(<ProductForm {...defaultProps} initialData={product} />)

      expect(screen.getByLabelText(/sku/i)).toHaveValue('ANI-00001')
      expect(screen.getByLabelText(/nombre/i)).toHaveValue('Anillo Oro')
      expect(screen.getByLabelText(/precio/i)).toHaveValue(1500)
    })
  })

  describe('validation', () => {
    it('should show error when SKU is empty', async () => {
      const user = userEvent.setup()
      render(<ProductForm {...defaultProps} />)

      await user.click(screen.getByRole('button', { name: /guardar/i }))

      expect(await screen.findByText(/el sku es requerido/i)).toBeInTheDocument()
    })

    it('should show error when price is invalid', async () => {
      const user = userEvent.setup()
      render(<ProductForm {...defaultProps} />)

      await user.type(screen.getByLabelText(/sku/i), 'ANI-00001')
      await user.type(screen.getByLabelText(/nombre/i), 'Anillo')
      await user.type(screen.getByLabelText(/precio/i), '-100')
      await user.click(screen.getByRole('button', { name: /guardar/i }))

      expect(await screen.findByText(/el precio debe ser mayor a 0/i)).toBeInTheDocument()
    })

    it('should validate SKU format', async () => {
      const user = userEvent.setup()
      render(<ProductForm {...defaultProps} />)

      await user.type(screen.getByLabelText(/sku/i), 'invalid-sku')
      await user.click(screen.getByRole('button', { name: /guardar/i }))

      expect(await screen.findByText(/formato de sku inválido/i)).toBeInTheDocument()
    })
  })

  describe('submission', () => {
    it('should call onSubmit with form data when valid', async () => {
      const user = userEvent.setup()
      const onSubmit = vi.fn()
      render(<ProductForm {...defaultProps} onSubmit={onSubmit} />)

      await user.type(screen.getByLabelText(/sku/i), 'ANI-00001')
      await user.type(screen.getByLabelText(/nombre/i), 'Anillo Oro')
      await user.type(screen.getByLabelText(/precio/i), '1500')
      await user.selectOptions(screen.getByLabelText(/colección/i), '1')
      await user.click(screen.getByRole('button', { name: /guardar/i }))

      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalledWith({
          sku: 'ANI-00001',
          name: 'Anillo Oro',
          description: '',
          price: 1500,
          collectionId: '1',
        })
      })
    })

    it('should disable submit button while submitting', async () => {
      const user = userEvent.setup()
      const onSubmit = vi.fn(() => new Promise((resolve) => setTimeout(resolve, 1000)))
      render(<ProductForm {...defaultProps} onSubmit={onSubmit} />)

      await user.type(screen.getByLabelText(/sku/i), 'ANI-00001')
      await user.type(screen.getByLabelText(/nombre/i), 'Anillo')
      await user.type(screen.getByLabelText(/precio/i), '100')
      await user.selectOptions(screen.getByLabelText(/colección/i), '1')
      await user.click(screen.getByRole('button', { name: /guardar/i }))

      expect(screen.getByRole('button', { name: /guardando/i })).toBeDisabled()
    })
  })

  describe('cancel', () => {
    it('should call onCancel when cancel button is clicked', async () => {
      const user = userEvent.setup()
      const onCancel = vi.fn()
      render(<ProductForm {...defaultProps} onCancel={onCancel} />)

      await user.click(screen.getByRole('button', { name: /cancelar/i }))

      expect(onCancel).toHaveBeenCalledTimes(1)
    })
  })
})
```

---

## Tests de Componentes con Estado Async

### Lista de Productos con Loading y Error

**src/components/products/product-list.test.tsx**
```typescript
import { describe, it, expect, vi } from 'vitest'
import { render, screen, waitFor } from '@/test/utils/render'
import userEvent from '@testing-library/user-event'
import { ProductList } from './product-list'
import { server } from '@/test/mocks/server'
import { http, HttpResponse } from 'msw'

const API_URL = import.meta.env.VITE_API_BASE_URL

describe('ProductList', () => {
  describe('loading state', () => {
    it('should show loading skeleton while fetching', () => {
      render(<ProductList />)
      expect(screen.getByTestId('loading-skeleton')).toBeInTheDocument()
    })
  })

  describe('success state', () => {
    it('should render products when loaded', async () => {
      render(<ProductList />)

      await waitFor(() => {
        expect(screen.getByText('Anillo Oro')).toBeInTheDocument()
        expect(screen.getByText('Collar Plata')).toBeInTheDocument()
      })
    })

    it('should show empty state when no products', async () => {
      server.use(
        http.get(`${API_URL}/products`, () => {
          return HttpResponse.json([])
        })
      )

      render(<ProductList />)

      await waitFor(() => {
        expect(screen.getByText(/no hay productos/i)).toBeInTheDocument()
      })
    })
  })

  describe('error state', () => {
    it('should show error message when fetch fails', async () => {
      server.use(
        http.get(`${API_URL}/products`, () => {
          return HttpResponse.json(
            { message: 'Server error' },
            { status: 500 }
          )
        })
      )

      render(<ProductList />)

      await waitFor(() => {
        expect(screen.getByRole('alert')).toHaveTextContent(/error al cargar/i)
      })
    })

    it('should allow retry on error', async () => {
      const user = userEvent.setup()
      let callCount = 0
      
      server.use(
        http.get(`${API_URL}/products`, () => {
          callCount++
          if (callCount === 1) {
            return HttpResponse.json({ message: 'Error' }, { status: 500 })
          }
          return HttpResponse.json([
            { id: '1', sku: 'ANI-001', name: 'Anillo', price: 100 }
          ])
        })
      )

      render(<ProductList />)

      await waitFor(() => {
        expect(screen.getByRole('alert')).toBeInTheDocument()
      })

      await user.click(screen.getByRole('button', { name: /reintentar/i }))

      await waitFor(() => {
        expect(screen.getByText('Anillo')).toBeInTheDocument()
      })
    })
  })

  describe('interactions', () => {
    it('should filter products by search', async () => {
      const user = userEvent.setup()
      render(<ProductList />)

      await waitFor(() => {
        expect(screen.getByText('Anillo Oro')).toBeInTheDocument()
      })

      await user.type(screen.getByPlaceholderText(/buscar/i), 'Collar')

      await waitFor(() => {
        expect(screen.queryByText('Anillo Oro')).not.toBeInTheDocument()
        expect(screen.getByText('Collar Plata')).toBeInTheDocument()
      })
    })

    it('should navigate to edit page on row click', async () => {
      const user = userEvent.setup()
      render(<ProductList />)

      await waitFor(() => {
        expect(screen.getByText('Anillo Oro')).toBeInTheDocument()
      })

      await user.click(screen.getByText('Anillo Oro'))

      // Verificar navegación (dependiendo de tu implementación)
      expect(window.location.pathname).toBe('/products/1/edit')
    })
  })
})
```

---

## Tests de Accesibilidad

### Usando jest-axe

```bash
npm install -D jest-axe @types/jest-axe
```

```typescript
import { describe, it, expect } from 'vitest'
import { render } from '@/test/utils/render'
import { axe, toHaveNoViolations } from 'jest-axe'
import { ProductForm } from './product-form'
import { Button } from '@/components/ui/button'
import { LoginForm } from './login-form'

expect.extend(toHaveNoViolations)

describe('Accessibility', () => {
  it('ProductForm should have no accessibility violations', async () => {
    const { container } = render(
      <ProductForm 
        collections={[{ id: '1', name: 'Test' }]}
        onSubmit={() => {}}
        onCancel={() => {}}
      />
    )
    
    const results = await axe(container)
    expect(results).toHaveNoViolations()
  })

  it('Button should have no accessibility violations', async () => {
    const { container } = render(
      <div>
        <Button>Normal Button</Button>
        <Button disabled>Disabled Button</Button>
        <Button isLoading>Loading Button</Button>
      </div>
    )
    
    const results = await axe(container)
    expect(results).toHaveNoViolations()
  })

  it('LoginForm should have no accessibility violations', async () => {
    const { container } = render(<LoginForm onSubmit={() => {}} />)
    
    const results = await axe(container)
    expect(results).toHaveNoViolations()
  })
})
```

---

## Tests de Componentes con Context

### Testing de Theme Toggle

```typescript
import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ThemeProvider, useTheme } from './theme-provider'

// Componente de test para verificar el context
function ThemeConsumer() {
  const { theme, setTheme } = useTheme()
  return (
    <div>
      <span data-testid="current-theme">{theme}</span>
      <button onClick={() => setTheme('dark')}>Set Dark</button>
      <button onClick={() => setTheme('light')}>Set Light</button>
    </div>
  )
}

describe('ThemeProvider', () => {
  it('should provide default theme', () => {
    render(
      <ThemeProvider defaultTheme="light" storageKey="test-theme">
        <ThemeConsumer />
      </ThemeProvider>
    )

    expect(screen.getByTestId('current-theme')).toHaveTextContent('light')
  })

  it('should update theme when setTheme is called', async () => {
    const user = userEvent.setup()
    render(
      <ThemeProvider defaultTheme="light" storageKey="test-theme">
        <ThemeConsumer />
      </ThemeProvider>
    )

    await user.click(screen.getByRole('button', { name: /set dark/i }))

    expect(screen.getByTestId('current-theme')).toHaveTextContent('dark')
  })
})
```

---

## Buenas Prácticas

### ✅ Hacer

```typescript
// ✅ Usar queries accesibles
screen.getByRole('button', { name: /guardar/i })
screen.getByLabelText(/email/i)

// ✅ Usar userEvent para interacciones
const user = userEvent.setup()
await user.click(button)
await user.type(input, 'text')

// ✅ Usar waitFor para assertions async
await waitFor(() => {
  expect(screen.getByText('Success')).toBeInTheDocument()
})

// ✅ Testear comportamiento, no implementación
expect(screen.getByRole('alert')).toHaveTextContent(/error/i)
```

### ❌ Evitar

```typescript
// ❌ Testear detalles de implementación
expect(component.state.isOpen).toBe(true)

// ❌ Usar getByTestId como primera opción
screen.getByTestId('submit-button')

// ❌ Usar fireEvent en lugar de userEvent
fireEvent.click(button)

// ❌ Assertions sin waitFor para contenido async
expect(screen.getByText('Loaded')).toBeInTheDocument()
```

---

## Tests de Formularios con React Hook Form + Zod

### Configuración

El proyecto usa **React Hook Form** con **Zod** para validaciones.

```bash
npm install react-hook-form @hookform/resolvers zod
```

### Ejemplo: Formulario con Validación Zod

**src/components/product-form-zod.tsx**
```typescript
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

const productSchema = z.object({
  sku: z.string()
    .min(1, 'SKU es requerido')
    .regex(/^[A-Z]{3}-\d{4}$/, 'Formato: ABC-1234'),
  name: z.string()
    .min(1, 'Nombre es requerido')
    .max(100, 'Máximo 100 caracteres'),
  price: z.number({ invalid_type_error: 'Precio es requerido' })
    .positive('El precio debe ser mayor a 0'),
  description: z.string().optional(),
})

type ProductFormData = z.infer<typeof productSchema>
```

### Test Completo de Formulario con Zod

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@/test/utils/render'
import userEvent from '@testing-library/user-event'
import { ProductForm } from './product-form'

describe('ProductForm with Zod validation', () => {
  const mockOnSubmit = vi.fn()

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should submit form with valid data', async () => {
    const user = userEvent.setup()
    mockOnSubmit.mockResolvedValue(undefined)

    render(<ProductForm onSubmit={mockOnSubmit} />)

    await user.type(screen.getByLabelText('SKU'), 'ABC-1234')
    await user.type(screen.getByLabelText('Nombre'), 'Anillo de Oro')
    await user.type(screen.getByLabelText(/Precio/), '150')

    await user.click(screen.getByRole('button', { name: 'Guardar' }))

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith({
        sku: 'ABC-1234',
        name: 'Anillo de Oro',
        price: 150,
        description: '',
      })
    })
  })

  it('should validate SKU format with regex', async () => {
    const user = userEvent.setup()

    render(<ProductForm onSubmit={mockOnSubmit} />)

    await user.type(screen.getByLabelText('SKU'), 'invalid-sku')
    await user.type(screen.getByLabelText('Nombre'), 'Test')
    await user.type(screen.getByLabelText(/Precio/), '100')

    await user.click(screen.getByRole('button', { name: 'Guardar' }))

    await waitFor(() => {
      expect(screen.getByText('Formato: ABC-1234')).toBeInTheDocument()
    })
  })

  it('should validate price is positive', async () => {
    const user = userEvent.setup()

    render(<ProductForm onSubmit={mockOnSubmit} />)

    await user.type(screen.getByLabelText('SKU'), 'ABC-1234')
    await user.type(screen.getByLabelText('Nombre'), 'Test')
    await user.type(screen.getByLabelText(/Precio/), '-10')

    await user.click(screen.getByRole('button', { name: 'Guardar' }))

    await waitFor(() => {
      expect(screen.getByText('El precio debe ser mayor a 0')).toBeInTheDocument()
    })
  })

  it('should mark invalid fields with aria-invalid', async () => {
    const user = userEvent.setup()

    render(<ProductForm onSubmit={mockOnSubmit} />)

    await user.click(screen.getByRole('button', { name: 'Guardar' }))

    await waitFor(() => {
      expect(screen.getByLabelText('SKU')).toHaveAttribute('aria-invalid', 'true')
      expect(screen.getByLabelText('Nombre')).toHaveAttribute('aria-invalid', 'true')
    })
  })

  it('should populate form with defaultValues', () => {
    render(
      <ProductForm
        onSubmit={mockOnSubmit}
        defaultValues={{
          sku: 'DEF-5678',
          name: 'Producto Existente',
          price: 200,
        }}
      />
    )

    expect(screen.getByLabelText('SKU')).toHaveValue('DEF-5678')
    expect(screen.getByLabelText('Nombre')).toHaveValue('Producto Existente')
    expect(screen.getByLabelText(/Precio/)).toHaveValue(200)
  })
})
```

### Test de Select con React Hook Form

```typescript
describe('SaleForm - Payment Method Select', () => {
  const paymentMethods = [
    { id: '1', name: 'Efectivo' },
    { id: '2', name: 'Tarjeta' },
    { id: '3', name: 'Transferencia' },
  ]

  it('should allow selecting payment method', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()

    render(<SaleForm paymentMethods={paymentMethods} onSubmit={onSubmit} />)

    // Abrir el select
    await user.click(screen.getByRole('combobox', { name: /método de pago/i }))

    // Seleccionar opción
    await user.click(screen.getByRole('option', { name: 'Tarjeta' }))

    // Verificar selección
    expect(screen.getByRole('combobox')).toHaveTextContent('Tarjeta')
  })

  it('should show validation error if no payment method selected', async () => {
    const user = userEvent.setup()

    render(<SaleForm paymentMethods={paymentMethods} onSubmit={vi.fn()} />)

    await user.click(screen.getByRole('button', { name: 'Registrar venta' }))

    await waitFor(() => {
      expect(screen.getByText(/método de pago.*requerido/i)).toBeInTheDocument()
    })
  })
})
```

---

## Próximos Pasos

1. → [04 - Mocking de API](04-mocking-api.md) - MSW
2. → [05 - Tests E2E](05-tests-e2e.md) - Playwright
