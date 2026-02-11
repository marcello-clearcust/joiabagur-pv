/**
 * Product Margin Report Page - EP10 HU-EP10-007
 * Margin analysis report with filtering, totals, and Excel export
 */

import { useState, useEffect, useMemo } from 'react';
import {
  useReactTable,
  getCoreRowModel,
  ColumnDef,
  createColumnHelper,
} from '@tanstack/react-table';
import { Download, Search } from 'lucide-react';
import { componentService } from '@/services/component.service';
import type { ProductMargin, MarginReportQueryParams } from '@/types/component.types';
import { DataGrid, DataGridContainer } from '@/components/ui/data-grid';
import { DataGridTable } from '@/components/ui/data-grid-table';
import { DataGridPagination } from '@/components/ui/data-grid-pagination';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Skeleton } from '@/components/ui/skeleton';
import { toast } from 'sonner';

const columnHelper = createColumnHelper<ProductMargin>();
const fmt = (v: number) => `€${v.toFixed(2)}`;
const fmtPct = (v: number) => `${v.toFixed(2)}%`;

export function ProductMarginsPage() {
  const [items, setItems] = useState<ProductMargin[]>([]);
  const [totals, setTotals] = useState({ sumCostPrice: 0, sumSalePrice: 0, sumMargin: 0 });
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 50 });
  const [totalPages, setTotalPages] = useState(0);

  const fetchReport = async () => {
    setIsLoading(true);
    try {
      const params: MarginReportQueryParams = {
        page: pagination.pageIndex + 1,
        pageSize: pagination.pageSize,
        search: search.length >= 2 ? search : undefined,
      };
      const report = await componentService.getMarginReport(params);
      setItems(report.items);
      setTotalCount(report.totalCount);
      setTotals({
        sumCostPrice: report.sumCostPrice,
        sumSalePrice: report.sumSalePrice,
        sumMargin: report.sumMargin,
      });
      setTotalPages(report.totalPages);
    } catch (error) {
      toast.error('Error al cargar el reporte de márgenes');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchReport();
  }, [search, pagination.pageIndex, pagination.pageSize]);

  const handleExport = async () => {
    try {
      const blob = await componentService.exportMarginReport({
        search: search.length >= 2 ? search : undefined,
      });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `margenes-productos-${new Date().toISOString().split('T')[0]}.xlsx`;
      a.click();
      URL.revokeObjectURL(url);
      toast.success('Reporte exportado correctamente');
    } catch {
      toast.error('Error al exportar el reporte');
    }
  };

  const columns = useMemo<ColumnDef<ProductMargin, unknown>[]>(
    () => [
      columnHelper.accessor('sku', {
        header: () => 'SKU',
        cell: (info) => <span className="font-mono text-sm">{info.getValue()}</span>,
        size: 120,
        meta: { skeleton: <Skeleton className="h-4 w-20" /> },
      }),
      columnHelper.accessor('productName', {
        header: () => 'Producto',
        cell: (info) => info.getValue(),
        size: 200,
        meta: { skeleton: <Skeleton className="h-4 w-32" /> },
      }),
      columnHelper.accessor('totalCostPrice', {
        header: () => 'Coste Total',
        cell: (info) => fmt(info.getValue()),
        size: 120,
        meta: { skeleton: <Skeleton className="h-4 w-20" /> },
      }),
      columnHelper.accessor('totalSalePrice', {
        header: () => 'Venta Total',
        cell: (info) => fmt(info.getValue()),
        size: 120,
        meta: { skeleton: <Skeleton className="h-4 w-20" /> },
      }),
      columnHelper.accessor('marginAmount', {
        header: () => 'Margen (€)',
        cell: (info) => (
          <span className={info.getValue() < 0 ? 'text-destructive' : 'text-green-600'}>
            {fmt(info.getValue())}
          </span>
        ),
        size: 120,
        meta: { skeleton: <Skeleton className="h-4 w-20" /> },
      }),
      columnHelper.accessor('marginPercent', {
        header: () => 'Margen (%)',
        cell: (info) => (
          <span className={info.getValue() < 20 ? 'text-amber-600' : 'text-green-600'}>
            {fmtPct(info.getValue())}
          </span>
        ),
        size: 100,
        meta: { skeleton: <Skeleton className="h-4 w-16" /> },
      }),
    ],
    []
  );

  const table = useReactTable({
    data: items,
    columns,
    getCoreRowModel: getCoreRowModel(),
    manualPagination: true,
    pageCount: totalPages,
    state: { pagination },
    onPaginationChange: setPagination,
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Reporte de Márgenes</h1>
          <p className="text-muted-foreground">
            Análisis de márgenes por producto basado en componentes
          </p>
        </div>
        <Button variant="outline" onClick={handleExport}>
          <Download className="mr-2 size-4" />
          Exportar Excel
        </Button>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Buscar por producto o SKU..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPagination(prev => ({ ...prev, pageIndex: 0 }));
            }}
            className="pl-9"
          />
        </div>
      </div>

      {/* Aggregated totals */}
      {!isLoading && items.length > 0 && (
        <div className="grid grid-cols-3 gap-4">
          <div className="rounded-lg border p-4">
            <p className="text-sm text-muted-foreground">Coste Total</p>
            <p className="text-2xl font-bold">{fmt(totals.sumCostPrice)}</p>
          </div>
          <div className="rounded-lg border p-4">
            <p className="text-sm text-muted-foreground">Venta Total</p>
            <p className="text-2xl font-bold">{fmt(totals.sumSalePrice)}</p>
          </div>
          <div className="rounded-lg border p-4">
            <p className="text-sm text-muted-foreground">Margen Total</p>
            <p className="text-2xl font-bold text-green-600">{fmt(totals.sumMargin)}</p>
          </div>
        </div>
      )}

      <DataGridContainer>
        <DataGrid
          table={table}
          recordCount={totalCount}
          isLoading={isLoading}
          emptyMessage="No hay productos con componentes asignados"
        >
          <DataGridTable />
          <DataGridPagination />
        </DataGrid>
      </DataGridContainer>
    </div>
  );
}

export default ProductMarginsPage;
