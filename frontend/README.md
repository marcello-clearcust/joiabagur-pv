# JoiaBagur PV - Frontend

Sistema de Gestión de Puntos de Venta para Joyería - Frontend Application

## Tech Stack

- **React 19** with TypeScript
- **Vite** for build tooling
- **Tailwind CSS** for styling
- **Radix UI** for accessible components
- **React Router v7** for routing
- **React Hook Form + Zod** for forms

## Getting Started

### Prerequisites

- Node.js 20+
- npm 10+

### Installation

```bash
cd frontend
npm install --legacy-peer-deps
```

### Development

```bash
npm run dev
```

The app will be available at [http://localhost:3000](http://localhost:3000)

### Build

```bash
npm run build
```

## Testing

### Unit & Component Tests (Vitest + React Testing Library)

```bash
# Run all tests
npm run test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage
npm run test:coverage

# Run tests with UI
npm run test:ui
```

### E2E Tests (Playwright)

```bash
# Install browsers (first time only)
npx playwright install

# Run E2E tests
npm run test:e2e

# Run E2E tests with UI mode
npm run test:e2e:ui

# Run E2E tests in headed mode
npm run test:e2e:headed
```

### Test Structure

```
frontend/
├── src/
│   ├── components/
│   │   └── ui/
│   │       ├── button.tsx
│   │       └── button.test.tsx    # Colocated test
│   ├── hooks/
│   │   ├── use-copy-to-clipboard.ts
│   │   └── use-copy-to-clipboard.test.ts
│   ├── lib/
│   │   ├── utils.ts
│   │   └── utils.test.ts
│   └── test/
│       ├── setup.ts               # Global test setup
│       ├── mocks/
│       │   ├── handlers.ts        # MSW request handlers
│       │   └── server.ts          # MSW server config
│       └── utils/
│           ├── render.tsx         # Custom render with providers
│           └── test-data.ts       # Test data factories
├── e2e/
│   ├── app.spec.ts                # E2E tests
│   └── fixtures/
│       └── test-user.json
├── playwright.config.ts
└── vite.config.ts                 # Vitest config included
```

### Test Conventions

- **Naming**: `should [behavior] when [condition]`
- **Structure**: AAA (Arrange, Act, Assert)
- **Queries**: Prefer accessible queries (`getByRole`, `getByLabelText`)
- **Coverage Target**: 70%

### Example Test

```typescript
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@/test/utils';
import userEvent from '@testing-library/user-event';
import { Button } from './button';

describe('Button', () => {
  it('should call onClick when clicked', async () => {
    const user = userEvent.setup();
    const handleClick = vi.fn();
    
    render(<Button onClick={handleClick}>Click me</Button>);
    
    await user.click(screen.getByRole('button'));
    
    expect(handleClick).toHaveBeenCalledTimes(1);
  });
});
```

## Scripts

| Script | Description |
|--------|-------------|
| `npm run dev` | Start development server |
| `npm run build` | Build for production |
| `npm run preview` | Preview production build |
| `npm run lint` | Run ESLint |
| `npm run test` | Run unit/component tests |
| `npm run test:watch` | Run tests in watch mode |
| `npm run test:coverage` | Run tests with coverage |
| `npm run test:ui` | Run tests with Vitest UI |
| `npm run test:e2e` | Run E2E tests |
| `npm run test:e2e:ui` | Run E2E tests with Playwright UI |
| `npm run test:e2e:headed` | Run E2E tests in headed browser |

## License

Private - JoiaBagur
