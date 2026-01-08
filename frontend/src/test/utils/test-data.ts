/**
 * Test Data Factories
 * Helper functions to create consistent test data for entities
 * 
 * Each factory function:
 * - Accepts optional overrides to customize generated data
 * - Generates realistic test data with all required fields populated
 * - Supports builder patterns for complex test scenarios
 * 
 * @example
 * ```ts
 * // Basic usage
 * const user = createMockUser();
 * 
 * // With overrides
 * const admin = createMockUser({ role: 'Admin', name: 'Admin User' });
 * 
 * // Create multiple items
 * const products = createMockProductList(5);
 * ```
 */

import type { AuthUser } from '@/types/auth.types';
import type { UserListItem } from '@/types/user.types';
import type { Product, ProductListItem } from '@/types/product.types';
import type { PaymentMethod } from '@/types/payment-method.types';

// ============================================================================
// User Factories
// ============================================================================

/**
 * Create a mock authenticated user
 */
export function createMockUser(overrides: Partial<AuthUser> = {}): AuthUser {
  return {
    id: '1',
    email: 'admin@test.com',
    name: 'Test Admin',
    role: 'Admin',
    ...overrides,
  };
}

/**
 * Create a mock user list item for user management
 */
export function createMockUserListItem(overrides: Partial<UserListItem> = {}): UserListItem {
  return {
    id: '1',
    email: 'user@test.com',
    name: 'Test User',
    role: 'Operator',
    isActive: true,
    createdAt: new Date().toISOString(),
    ...overrides,
  };
}

/**
 * Create multiple mock users
 */
export function createMockUserList(count: number = 3): UserListItem[] {
  return Array.from({ length: count }, (_, i) => 
    createMockUserListItem({
      id: String(i + 1),
      email: `user${i + 1}@test.com`,
      name: `Test User ${i + 1}`,
      role: i === 0 ? 'Admin' : 'Operator',
    })
  );
}

// ============================================================================
// Product Factories
// ============================================================================

/**
 * Create a mock product with full details
 */
export function createMockProduct(overrides: Partial<Product> = {}): Product {
  const baseDate = new Date().toISOString();
  return {
    id: '1',
    sku: 'JOY-001',
    name: 'Gold Ring',
    description: 'Beautiful 18k gold ring',
    price: 299.99,
    collectionId: 'col-1',
    collectionName: 'Gold Collection',
    isActive: true,
    createdAt: baseDate,
    updatedAt: baseDate,
    photos: [],
    ...overrides,
  };
}

/**
 * Create a mock product list item for catalog display
 */
export function createMockProductListItem(overrides: Partial<ProductListItem> = {}): ProductListItem {
  return {
    id: '1',
    sku: 'JOY-001',
    name: 'Gold Ring',
    price: 299.99,
    primaryPhotoUrl: undefined,
    collectionName: 'Gold Collection',
    isActive: true,
    availableQuantity: 10,
    createdAt: new Date().toISOString(),
    ...overrides,
  };
}

/**
 * Create multiple mock products for catalog
 */
export function createMockProductList(count: number = 3): ProductListItem[] {
  return Array.from({ length: count }, (_, i) => 
    createMockProductListItem({
      id: String(i + 1),
      sku: `JOY-${String(i + 1).padStart(3, '0')}`,
      name: `Product ${i + 1}`,
      price: 99.99 + (i * 50),
      availableQuantity: 10 + i,
    })
  );
}

// ============================================================================
// Payment Method Factories
// ============================================================================

/**
 * Create a mock payment method
 */
export function createMockPaymentMethod(overrides: Partial<PaymentMethod> = {}): PaymentMethod {
  const baseDate = new Date().toISOString();
  return {
    id: '1',
    code: 'CASH',
    name: 'Cash',
    description: 'Cash payment',
    isActive: true,
    createdAt: baseDate,
    updatedAt: baseDate,
    ...overrides,
  };
}

/**
 * Create multiple mock payment methods
 */
export function createMockPaymentMethodList(count: number = 3): PaymentMethod[] {
  const methods = [
    { code: 'CASH', name: 'Cash', description: 'Cash payment' },
    { code: 'CARD', name: 'Card', description: 'Credit/Debit card payment' },
    { code: 'TRANSFER', name: 'Transfer', description: 'Bank transfer' },
    { code: 'DIGITAL', name: 'Digital Wallet', description: 'Digital wallet payment' },
  ];
  
  return Array.from({ length: Math.min(count, methods.length) }, (_, i) => 
    createMockPaymentMethod({
      id: String(i + 1),
      ...methods[i],
    })
  );
}

// ============================================================================
// Factory Builder Patterns
// ============================================================================

/**
 * Product builder for complex test scenarios
 * 
 * @example
 * ```ts
 * const product = new ProductBuilder()
 *   .withSku('CUSTOM-001')
 *   .withPrice(499.99)
 *   .inactive()
 *   .build();
 * ```
 */
export class ProductBuilder {
  private product: Partial<Product>;

  constructor() {
    this.product = {};
  }

  withId(id: string): ProductBuilder {
    this.product.id = id;
    return this;
  }

  withSku(sku: string): ProductBuilder {
    this.product.sku = sku;
    return this;
  }

  withName(name: string): ProductBuilder {
    this.product.name = name;
    return this;
  }

  withPrice(price: number): ProductBuilder {
    this.product.price = price;
    return this;
  }

  withCollection(collectionId: string, collectionName: string): ProductBuilder {
    this.product.collectionId = collectionId;
    this.product.collectionName = collectionName;
    return this;
  }

  inactive(): ProductBuilder {
    this.product.isActive = false;
    return this;
  }

  build(): Product {
    return createMockProduct(this.product);
  }
}

/**
 * User builder for complex test scenarios
 * 
 * @example
 * ```ts
 * const operator = new UserBuilder()
 *   .withRole('Operator')
 *   .withEmail('operator@test.com')
 *   .inactive()
 *   .build();
 * ```
 */
export class UserBuilder {
  private user: Partial<UserListItem>;

  constructor() {
    this.user = {};
  }

  withId(id: string): UserBuilder {
    this.user.id = id;
    return this;
  }

  withEmail(email: string): UserBuilder {
    this.user.email = email;
    return this;
  }

  withName(name: string): UserBuilder {
    this.user.name = name;
    return this;
  }

  withRole(role: 'Admin' | 'Operator'): UserBuilder {
    this.user.role = role;
    return this;
  }

  inactive(): UserBuilder {
    this.user.isActive = false;
    return this;
  }

  build(): UserListItem {
    return createMockUserListItem(this.user);
  }
}
