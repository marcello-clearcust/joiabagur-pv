export interface InventoryMovementReportFilter {
  startDate?: string;
  endDate?: string;
  pointOfSaleId?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: string;
}

export interface InventoryMovementSummaryRow {
  productId: string;
  productName: string;
  productSku: string;
  additions: number;
  subtractions: number;
  difference: number;
}

export interface InventoryMovementReportResponse {
  items: InventoryMovementSummaryRow[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
