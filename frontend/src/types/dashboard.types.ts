export interface PaymentMethodDistribution {
  methodName: string;
  amount: number;
  count: number;
}

export interface ReturnCategoryDistribution {
  category: string;
  count: number;
}

export interface DashboardStats {
  salesTodayCount: number;
  salesTodayTotal: number;
  monthlyRevenue: number;
  previousYearMonthlyRevenue: number | null;
  monthlyReturnsCount: number;
  monthlyReturnsTotal: number;
  weeklyRevenue: number | null;
  returnsTodayCount: number | null;
  paymentMethodDistribution: PaymentMethodDistribution[] | null;
  returnCategoryDistribution: ReturnCategoryDistribution[] | null;
}
