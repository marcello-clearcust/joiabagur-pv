/**
 * Products Without Components Report - EP10 HU-EP10-008
 * Lists products without any component assignments
 */

import { useState, useEffect, useMemo } from 'react';
import {
  useReactTable,
  getCoreRowModel,
  ColumnDef,
  createColumnHelper,
} from '@tanstack/react-table';
import { Search, Pencil } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { componentService } from '@/services/component.service';
import type { ProductWithoutComponents } from '@/types/component.types';
import { DataGrid, DataGridContainer } from '@/components/ui/data-grid';
import { DataGridTable } from '@/components/ui/data-grid-table';
import { DataGridPagination } from '@/components/ui/data-grid-pagination';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Skeleton } from '@/components/ui/skeleton';
import { toast } from 'sonner';

const columnHelper = createColumnHelper<ProductWithoutComponents>();

export function ProductsWithoutComponentsPage() {
  const navigate = useNavigate();
  const [items, setItems] = useState<ProductWithoutComponents[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 50 });
  const [totalPages, setTotalPages] = useState(0);

  const fetchReport = async () => {
    setIsLoading(true);
    try {
      const result = await componentService.getProductsWithoutComponents({
        page: pagination.pageIndex + 1,
        pageSize: pagination.pageSize,
        search: search.length >= 2 ? search : undefined,
      });
      setItems(result.items);
      setTotalCount(result.totalCount);
      setTotalPages(result.totalPages);
    } catch (error) {
      toast.error('Error al cargar el reporte');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchReport();
  }, [search, pagination.pageIndex, pagination.pageSize]);

  const columns = useMemo<ColumnDef<ProductWithoutComponents, unknown>[]>(
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
        size: 250,
        meta: { skeleton: <Skeleton className="h-4 w-32" /> },
      }),
      columnHelper.accessor('price', {
        header: () => 'Precio (€)',
        cell: (info) => `€${info.getValue().toFixed(2)}`,
        size: 120,
        meta: { skeleton: <Skeleton className="h-4 w-20" /> },
      }),
      columnHelper.accessor('collectionName', {
        header: () => 'Colección',
        cell: (info) => (
          <span className="text-muted-foreground">{info.getValue() || '-'}</span>
        ),
        size: 150,
        meta: { skeleton: <Skeleton className="h-4 w-24" /> },
      }),
      columnHelper.display({
        id: 'actions',
        header: () => '',
        cell: ({ row }) => (
          <Button
            variant="outline"
            size="sm"
            onClick={() => navigate(`/products/${row.original.productId}/edit`)}
          >
            <Pencil className="mr-2 size-3.5" />
            Editar
          </Button>
        ),
        size: 100,
        meta: { skeleton: <Skeleton className="h-8 w-20" /> },
      }),
    ],
    [navigate]
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
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Productos sin Componentes</h1>
        <p className="text-muted-foreground">
          Productos que no tienen componentes asignados
        </p>
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
        <span className="text-sm text-muted-foreground">
          {totalCount} producto{totalCount !== 1 ? 's' : ''}
        </span>
      </div>

      <DataGridContainer>
        <DataGrid
          table={table}
          recordCount={totalCount}
          isLoading={isLoading}
          emptyMessage="No hay productos sin componentes"
        >
          <DataGridTable />
          <DataGridPagination />
        </DataGrid>
      </DataGridContainer>
    </div>
  );
}

export default ProductsWithoutComponentsPage;
