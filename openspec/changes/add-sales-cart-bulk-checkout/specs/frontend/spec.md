## ADDED Requirements
### Requirement: Persistent Sales Cart State
The frontend SHALL provide a sales cart state container that persists cart lines in browser storage and restores them across page reloads.

#### Scenario: Cart state survives reload
- **WHEN** the operator adds one or more lines to the sales cart
- **AND** reloads the browser tab
- **THEN** the cart state is restored from persisted storage
- **AND** line metadata needed for UI rendering remains available

### Requirement: Cart Expiration Policy
The frontend SHALL apply a cart time-to-live policy of 10 hours of inactivity so stale carts are automatically invalidated.

#### Scenario: Expire stale cart on next access
- **WHEN** cart data exceeds 10 hours since last cart activity
- **THEN** frontend clears stale cart data before checkout interactions
- **AND** informs the user that the previous cart expired

### Requirement: Cart Composition Constraints
The frontend SHALL enforce single point-of-sale and single payment-method composition rules while adding new lines to cart.

#### Scenario: Reject line with different point of sale
- **WHEN** cart already contains lines from one point of sale
- **AND** operator attempts to add a line with a different point of sale
- **THEN** frontend blocks the addition
- **AND** displays a validation message explaining the mismatch

#### Scenario: Reject line with different payment method when fixed
- **WHEN** cart already has an established payment method context
- **AND** operator attempts to add a line with a different payment method
- **THEN** frontend blocks the addition
- **AND** keeps existing cart unchanged

### Requirement: Sales Cart Page and Checkout Confirmation
The frontend SHALL provide route `/sales/cart` with line listing, line removal, totals, payment method selection, and a confirmation dialog before bulk submission.

#### Scenario: Remove line from cart
- **WHEN** operator removes a line in `/sales/cart`
- **THEN** the line is deleted from cart state
- **AND** totals and badge count update immediately

#### Scenario: Confirm bulk checkout from cart
- **WHEN** operator opens checkout confirmation in `/sales/cart`
- **THEN** dialog shows line count, total amount, selected payment method, and global note input
- **AND** confirming sends a bulk request to the backend

### Requirement: Per-Line Stock Revalidation in Cart
The frontend SHALL revalidate stock for each cart line against the selected point of sale before allowing checkout.

#### Scenario: Stock insufficient for one or more lines
- **WHEN** revalidation detects `quantity > available stock` for any line
- **THEN** frontend marks affected lines as insufficient stock
- **AND** disables the checkout action
- **AND** communicates which lines must be removed before retrying

### Requirement: Sales Entry Points and Cart Visibility
The frontend SHALL expose cart access from sales entry pages and sales navigation, including a line-count badge.

#### Scenario: Add to cart from manual sales page
- **WHEN** operator completes valid form data in `/sales/new`
- **THEN** frontend provides action "Añadir al carrito"
- **AND** added line appears in cart count immediately

#### Scenario: View cart from image sales page
- **WHEN** the cart has at least one line
- **THEN** `/sales/new/image` shows action "Ver carrito"
- **AND** action navigates to `/sales/cart`

#### Scenario: Cart badge in sales layout
- **WHEN** user navigates inside `/sales/*`
- **THEN** sales layout/nav displays a cart badge with current line count
- **AND** clicking badge navigates to `/sales/cart`
