# Change: Add role-differentiated dashboards for Administrator and Operator

## Why
The current dashboard is a static placeholder with no live data. Administrators lack a consolidated view of business performance (sales, revenue, returns, stock alerts), and operators have no quick-start operational view for their assigned POS. Both roles need actionable insights surfaced immediately after login.

## What Changes
- **New backend endpoint** `GET /api/dashboard/stats?posId={optional}` returning pre-aggregated KPIs (today's sales, monthly revenue, monthly returns, year-over-year comparison)
- **Server-side 24h in-memory cache** (`IMemoryCache`) for the two donut chart aggregations (payment method distribution and return category distribution) to avoid repeated heavy queries
- **New frontend `dashboard.service.ts`** consuming the stats endpoint and existing endpoints for charts/tables
- **New `AdminDashboard.tsx`** component with 4 rows: KPI cards, trend + POS revenue charts, alerts/tables, donut charts
- **New `OperatorDashboard.tsx`** component with KPI cards, weekly trend chart, recent sales/low stock tables, and sticky bottom action bar
- **New `OperatorLayout`** wrapper that hides the Metronic sidebar and adds a sticky bottom bar with 3 quick-action FABs
- **Dashboard page (`/dashboard`)** updated to bifurcate by user role and render the appropriate dashboard
- Charts implemented with a lightweight library (Recharts or Chart.js via react-chartjs-2)

## Impact
- Affected specs: `dashboard-analytics` (new), `frontend`, `backend`
- Affected code:
  - Backend: new `DashboardController`, `DashboardService`, DTOs, cache configuration
  - Frontend: `pages/dashboard/page.tsx` (rewrite), new `AdminDashboard.tsx`, `OperatorDashboard.tsx`, `OperatorLayout`, `dashboard.service.ts`, chart components, routing updates
- No breaking changes to existing endpoints or database schema
- No conflict with `add-sales-cart-bulk-checkout` (that change affects `/sales/cart` and bulk checkout; dashboard only reads existing data)
