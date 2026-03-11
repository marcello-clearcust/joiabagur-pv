## 1. Backend — Dashboard Stats Endpoint

- [x] 1.1 Create `DashboardStatsDto` in Application/DTOs with fields: salesTodayCount, salesTodayTotal, monthlyRevenue, previousYearMonthlyRevenue (nullable), monthlyReturnsCount, monthlyReturnsTotal, weeklyRevenue (nullable), returnsTodayCount (nullable), paymentMethodDistribution (nullable array), returnCategoryDistribution (nullable array)
- [x] 1.2 Create `PaymentMethodDistributionDto` (method name, amount, count) and `ReturnCategoryDistributionDto` (category, count)
- [x] 1.3 Create `IDashboardService` interface and `DashboardService` implementation in Application/Services
- [x] 1.4 Implement global KPI aggregation queries (today's sales, monthly revenue, YoY comparison, monthly returns)
- [x] 1.5 Implement POS-scoped KPI aggregation queries (today's sales, weekly revenue Mon-Sun, today's returns)
- [x] 1.6 Implement IMemoryCache for payment method distribution and return category distribution (24h TTL)
- [x] 1.7 Add POS access validation for operator requests (reuse UserPointOfSaleService)
- [x] 1.8 Create `DashboardController` with GET /api/dashboard/stats?posId={optional}, [Authorize] attribute
- [x] 1.9 Register DashboardService in DI container (Program.cs or service registration)
- [x] 1.10 Write unit tests for DashboardService (global KPIs, POS-scoped KPIs, cache hit/miss, access validation)

## 2. Frontend — Dashboard Service and Types

- [x] 2.1 Create `dashboard.types.ts` in frontend/src/types with DashboardStats, PaymentMethodDistribution, ReturnCategoryDistribution interfaces
- [x] 2.2 Create `dashboard.service.ts` in frontend/src/services consuming GET /api/dashboard/stats

## 3. Frontend — Admin Dashboard

- [x] 3.1 Install Recharts dependency (`npm install recharts`)
- [x] 3.2 Create `AdminDashboard.tsx` component in pages/dashboard/ with KPI cards row (Ventas hoy, Ingresos del mes, Devoluciones del mes)
- [x] 3.3 Add 30-day sales trend multi-line chart (fetch from /api/sales with date filter, group by day/POS client-side)
- [x] 3.4 Add revenue-by-POS horizontal bar chart (current month, fetch from /api/sales with date filter)
- [x] 3.5 Add critical stock table (fetch from /api/inventory/centralized, filter stock <= 2 client-side)
- [x] 3.6 Add recent 8 sales table (fetch from /api/sales with pageSize=8)
- [x] 3.7 Add payment method donut chart (data from dashboard stats response)
- [x] 3.8 Add return category donut chart (data from dashboard stats response)

## 4. Frontend — Operator Dashboard

- [x] 4.1 Create `OperatorDashboard.tsx` component in pages/dashboard/ with KPI cards (Mis ventas hoy, Mis ventas esta semana, Devoluciones hoy, Artículos en stock)
- [x] 4.2 Add weekly sales line chart (fetch from /api/sales with week date filter for assigned POS)
- [x] 4.3 Add recent 8 sales table (fetch from /api/sales with pageSize=8 and posId filter)
- [x] 4.4 Add low stock table (fetch from /api/inventory with posId, filter stock <= 2)

## 5. Frontend — Operator Layout and Action Bar

- [x] 5.1 Create `OperatorLayout` wrapper component that hides Layout8 sidebar on operator dashboard
- [x] 5.2 Create sticky bottom bar component with 3 FAB buttons: Nueva venta manual (/sales/new), Venta por imagen (/sales/new/image), Registrar devolución (/returns/new)
- [x] 5.3 Ensure sidebar restores on navigation away from dashboard

## 6. Frontend — Dashboard Page Integration

- [x] 6.1 Rewrite `pages/dashboard/page.tsx` to read user role from auth context and render AdminDashboard or OperatorDashboard
- [x] 6.2 Verify lazy loading still works for the dashboard page (no bundle size regression)

## 7. Validation and Testing

- [x] 7.1 Write frontend tests for dashboard role bifurcation (admin sees AdminDashboard, operator sees OperatorDashboard)
- [x] 7.2 Write frontend tests for OperatorLayout sidebar hiding behavior
- [x] 7.3 Verify EUR formatting on all chart tooltips and KPI cards
- [x] 7.4 Manual smoke test: admin dashboard loads with chart data, operator dashboard loads with POS-scoped data
