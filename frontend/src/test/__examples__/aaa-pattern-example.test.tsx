/**
 * Example Test File Demonstrating AAA Pattern
 * 
 * This file serves as a reference for writing well-structured tests
 * following the Arrange-Act-Assert (AAA) pattern.
 * 
 * AAA Pattern Structure:
 * - ARRANGE: Set up test data, mocks, and render components
 * - ACT: Execute user interactions or function calls
 * - ASSERT: Verify expected outcomes
 */

import React from 'react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@/test/utils';
import { userEvent } from '@testing-library/user-event';
import { 
  createMockProduct, 
  createMockUser,
  ProductBuilder 
} from '@/test/utils/test-data';

/**
 * Example 1: Simple Component Test with AAA Pattern
 * Testing a button click interaction
 */
describe('AAA Pattern Example - Button Component', () => {
  it('should increment counter when button is clicked', async () => {
    // ARRANGE - Prepare test data and render component
    const user = userEvent.setup();
    const CounterComponent = () => {
      const [count, setCount] = React.useState(0);
      return (
        <div>
          <span data-testid="count">{count}</span>
          <button onClick={() => setCount(count + 1)}>
            Increment
          </button>
        </div>
      );
    };
    render(<CounterComponent />);

    // ACT - Execute user interaction
    const button = screen.getByRole('button', { name: /increment/i });
    await user.click(button);

    // ASSERT - Verify expected outcome
    expect(screen.getByTestId('count')).toHaveTextContent('1');
  });
});

/**
 * Example 2: Form Validation Test with AAA Pattern
 * Testing form validation and error handling
 */
describe('AAA Pattern Example - Form Validation', () => {
  const mockOnSubmit = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should display validation error when email is invalid', async () => {
    // ARRANGE
    const user = userEvent.setup();
    const EmailForm = ({ onSubmit }: { onSubmit: (email: string) => void }) => {
      const [email, setEmail] = React.useState('');
      const [error, setError] = React.useState('');

      const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!email.includes('@')) {
          setError('Email inválido');
          return;
        }
        onSubmit(email);
      };

      return (
        <form onSubmit={handleSubmit}>
          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="text"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          {error && <p role="alert">{error}</p>}
          <button type="submit">Submit</button>
        </form>
      );
    };
    
    render(<EmailForm onSubmit={mockOnSubmit} />);

    // ACT
    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /submit/i });
    
    await user.type(emailInput, 'invalid-email');
    await user.click(submitButton);

    // ASSERT
    expect(screen.getByRole('alert')).toHaveTextContent('Email inválido');
    expect(mockOnSubmit).not.toHaveBeenCalled();
  });

  it('should submit form when all fields are valid', async () => {
    // ARRANGE
    const user = userEvent.setup();
    const validEmail = 'test@example.com';
    const EmailForm = ({ onSubmit }: { onSubmit: (email: string) => void }) => {
      const [email, setEmail] = React.useState('');

      const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (email.includes('@')) {
          onSubmit(email);
        }
      };

      return (
        <form onSubmit={handleSubmit}>
          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="text"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <button type="submit">Submit</button>
        </form>
      );
    };
    
    render(<EmailForm onSubmit={mockOnSubmit} />);

    // ACT
    await user.type(screen.getByLabelText(/email/i), validEmail);
    await user.click(screen.getByRole('button', { name: /submit/i }));

    // ASSERT
    expect(mockOnSubmit).toHaveBeenCalledWith(validEmail);
    expect(mockOnSubmit).toHaveBeenCalledTimes(1);
  });
});

/**
 * Example 3: Async Data Loading with AAA Pattern
 * Testing component that fetches data
 */
describe('AAA Pattern Example - Async Data Loading', () => {
  it('should display loading state then show products', async () => {
    // ARRANGE
    const mockProducts = [
      createMockProduct({ id: '1', name: 'Product 1' }),
      createMockProduct({ id: '2', name: 'Product 2' }),
    ];

    const ProductList = () => {
      const [products, setProducts] = React.useState<any[]>([]);
      const [loading, setLoading] = React.useState(true);

      React.useEffect(() => {
        // Simulate API call
        setTimeout(() => {
          setProducts(mockProducts);
          setLoading(false);
        }, 100);
      }, []);

      if (loading) {
        return <div>Loading...</div>;
      }

      return (
        <ul>
          {products.map((product) => (
            <li key={product.id}>{product.name}</li>
          ))}
        </ul>
      );
    };

    render(<ProductList />);

    // ACT - Wait for loading to complete
    await waitFor(() => {
      expect(screen.queryByText(/loading/i)).not.toBeInTheDocument();
    });

    // ASSERT
    expect(screen.getByText('Product 1')).toBeInTheDocument();
    expect(screen.getByText('Product 2')).toBeInTheDocument();
  });
});

/**
 * Example 4: Using Test Data Factories with AAA Pattern
 * Demonstrating the use of factory functions and builders
 */
describe('AAA Pattern Example - Test Data Factories', () => {
  it('should display product details correctly', () => {
    // ARRANGE - Using factory function with overrides
    const product = createMockProduct({
      name: 'Luxury Watch',
      price: 1299.99,
      sku: 'WATCH-001',
    });

    const ProductCard = ({ product }: { product: any }) => (
      <article>
        <h2>{product.name}</h2>
        <p>SKU: {product.sku}</p>
        <p>Price: €{product.price}</p>
      </article>
    );

    render(<ProductCard product={product} />);

    // ACT - No user interaction needed for this test

    // ASSERT
    expect(screen.getByRole('heading', { name: 'Luxury Watch' })).toBeInTheDocument();
    expect(screen.getByText('SKU: WATCH-001')).toBeInTheDocument();
    expect(screen.getByText('Price: €1299.99')).toBeInTheDocument();
  });

  it('should build complex product using builder pattern', () => {
    // ARRANGE - Using builder pattern for complex scenarios
    const customProduct = new ProductBuilder()
      .withSku('CUSTOM-001')
      .withName('Custom Jewelry')
      .withPrice(499.99)
      .withCollection('col-1', 'Premium Collection')
      .inactive()
      .build();

    const ProductStatus = ({ product }: { product: any }) => (
      <div>
        <h3>{product.name}</h3>
        <span>{product.isActive ? 'Active' : 'Inactive'}</span>
        <p>Collection: {product.collectionName}</p>
      </div>
    );

    render(<ProductStatus product={customProduct} />);

    // ACT - No user interaction needed

    // ASSERT
    expect(screen.getByText('Custom Jewelry')).toBeInTheDocument();
    expect(screen.getByText('Inactive')).toBeInTheDocument();
    expect(screen.getByText('Collection: Premium Collection')).toBeInTheDocument();
  });
});

/**
 * Example 5: Testing with Multiple User Interactions (AAA Pattern)
 * Demonstrating complex user flows
 */
describe('AAA Pattern Example - Multi-Step User Flow', () => {
  it('should complete a multi-step form', async () => {
    // ARRANGE
    const user = userEvent.setup();
    const mockOnComplete = vi.fn();

    const MultiStepForm = ({ onComplete }: { onComplete: (data: any) => void }) => {
      const [step, setStep] = React.useState(1);
      const [formData, setFormData] = React.useState({ name: '', email: '' });

      const handleNext = () => setStep(2);
      const handleSubmit = () => onComplete(formData);

      if (step === 1) {
        return (
          <div>
            <label htmlFor="name">Name</label>
            <input
              id="name"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            />
            <button onClick={handleNext}>Next</button>
          </div>
        );
      }

      return (
        <div>
          <label htmlFor="email">Email</label>
          <input
            id="email"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          />
          <button onClick={handleSubmit}>Submit</button>
        </div>
      );
    };

    render(<MultiStepForm onComplete={mockOnComplete} />);

    // ACT - Step 1: Fill name and click next
    await user.type(screen.getByLabelText(/name/i), 'John Doe');
    await user.click(screen.getByRole('button', { name: /next/i }));

    // ACT - Step 2: Wait for email field and fill it
    await waitFor(() => {
      expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    });
    await user.type(screen.getByLabelText(/email/i), 'john@example.com');
    await user.click(screen.getByRole('button', { name: /submit/i }));

    // ASSERT
    expect(mockOnComplete).toHaveBeenCalledWith({
      name: 'John Doe',
      email: 'john@example.com',
    });
  });
});

/**
 * Best Practices Summary:
 * 
 * 1. Always structure tests with clear AAA sections
 * 2. Add comments to separate sections when needed for readability
 * 3. Use descriptive test names: "should [behavior] when [condition]"
 * 4. Initialize userEvent with setup() before interactions
 * 5. Use accessible queries (getByRole, getByLabelText) over getByTestId
 * 6. Use factory functions to create test data consistently
 * 7. Use waitFor for async operations
 * 8. Clear mocks in beforeEach to ensure test isolation
 */

