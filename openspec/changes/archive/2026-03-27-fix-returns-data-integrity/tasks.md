## 1. Backend — ReturnRepository: eager-load Sale in history queries

- [x] 1.1 Add `.ThenInclude(rs => rs.Sale)` to `GetByPointOfSaleAsync` in `ReturnRepository.cs`
- [x] 1.2 Add `.ThenInclude(rs => rs.Sale)` to `GetAllReturnsAsync` in `ReturnRepository.cs`
- [x] 1.3 Add `.ThenInclude(rs => rs.Sale)` to `GetByPointOfSalesAsync` in `ReturnRepository.cs`
- [x] 1.4 Verify: GET /api/returns list response no longer returns `saleDate: "0001-01-01T00:00:00"` for any associated sale

## 2. Backend — SaleDto: add HasReturn field

- [x] 2.1 Add `bool HasReturn { get; set; }` to `SaleDto.cs`
- [x] 2.2 Include `ReturnSales` in `GetByPointOfSaleAsync` in `SaleRepository.cs`
- [x] 2.3 Include `ReturnSales` in `GetAllSalesAsync` in `SaleRepository.cs`
- [x] 2.4 Include `ReturnSales` in `GetByPointOfSalesAsync` in `SaleRepository.cs`
- [x] 2.5 Set `HasReturn = sale.ReturnSales.Count > 0` in `SalesService.MapToDto`
- [x] 2.6 Verify: GET /api/sales response includes `hasReturn: true` for a sale that has a return, `hasReturn: false` otherwise

## 3. Backend — DashboardService: net revenue and payment distribution

- [x] 3.1 Rename intermediate variable `monthlyRevenue` to `monthlySalesRevenue` in `GetGlobalStatsAsync`
- [x] 3.2 Extract `monthlyReturnsTotal = monthlyReturns?.Total ?? 0m` and compute `monthlyRevenue = monthlySalesRevenue - monthlyReturnsTotal`
- [x] 3.3 Update `MonthlyReturnsTotal` assignment in the return DTO to use the extracted variable
- [x] 3.4 Add `.Where(s => !s.ReturnSales.Any())` filter to `GetPaymentMethodDistributionAsync` before the `GroupBy`
- [x] 3.5 Verify: GET /api/dashboard/stats `monthlyRevenue` equals gross sales minus monthly returns total
- [x] 3.6 Verify: `paymentMethodDistribution` amounts exclude returned sales (restart API to bypass 24h cache if testing immediately)

## 4. Frontend — Type and dashboard components

- [x] 4.1 Add `hasReturn: boolean` to `Sale` interface in `frontend/src/types/sales.types.ts`
- [x] 4.2 In `AdminDashboard.tsx`: derive `salesWithoutReturns = salesHistory.sales.filter(s => !s.hasReturn)` after API response
- [x] 4.3 In `AdminDashboard.tsx`: use `salesWithoutReturns` for recent sales slice (`.slice(0, 8)`)
- [x] 4.4 In `AdminDashboard.tsx`: use `salesWithoutReturns` for 30-day trend `for` loop
- [x] 4.5 In `AdminDashboard.tsx`: use `salesWithoutReturns` for POS revenue `for` loop
- [x] 4.6 In `OperatorDashboard.tsx`: derive `salesWithoutReturns` and use for recent sales slice
- [x] 4.7 In `OperatorDashboard.tsx`: use `salesWithoutReturns` for weekly trend `for` loop
- [x] 4.8 Verify: TypeScript strict compilation passes with no new errors (`tsc --noEmit`)
