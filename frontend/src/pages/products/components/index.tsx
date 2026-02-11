/**
 * Component Master Table Page - EP10 HU-EP10-001
 * CRUD management for product components (admin-only)
 */

import { useState, useEffect, useMemo } from 'react';
import {
  useReactTable,
  getCoreRowModel,
  ColumnDef,
  createColumnHelper,
} from '@tanstack/react-table';
import { Plus, Pencil, MoreHorizontal, CheckCircle2, XCircle, Power } from 'lucide-react';
import { componentService } from '@/services/component.service';
import type { ProductComponent, ComponentQueryParams } from '@/types/component.types';
import { DataGrid, DataGridContainer } from '@/components/ui/data-grid';
import { DataGridTable } from '@/components/ui/data-grid-table';
import { DataGridPagination } from '@/components/ui/data-grid-pagination';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Skeleton } from '@/components/ui/skeleton';
import { Switch } from '@/components/ui/switch';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { toast } from 'sonner';
import { ComponentFormDialog } from './component-form-dialog';

const columnHelper = createColumnHelper<ProductComponent>();

const formatPrice = (value: number | null) => {
  if (value === null || value === undefined) return '-';
  return `€${value.toFixed(4)}`;
};

export function ComponentListPage() {
  const [components, setComponents] = useState<ProductComponent[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [selectedComponent, setSelectedComponent] = useState<ProductComponent | null>(null);
  const [showInactive, setShowInactive] = useState(false);
  const [search, setSearch] = useState('');
  const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 50 });
  const [totalPages, setTotalPages] = useState(0);

  const fetchComponents = async () => {
    setIsLoading(true);
    try {
      const params: ComponentQueryParams = {
        page: pagination.pageIndex + 1,
        pageSize: pagination.pageSize,
        search: search.length >= 2 ? search : undefined,
        isActive: showInactive ? undefined : true,
      };
      const result = await componentService.getComponents(params);
      setComponents(result.items);
      setTotalCount(result.totalCount);
      setTotalPages(result.totalPages);
    } catch (error) {
      toast.error('Error al cargar los componentes');
      console.error('Failed to fetch components:', error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchComponents();
  }, [showInactive, search, pagination.pageIndex, pagination.pageSize]);

  const handleCreate = () => {
    setSelectedComponent(null);
    setIsDialogOpen(true);
  };

  const handleEdit = (component: ProductComponent) => {
    setSelectedComponent(component);
    setIsDialogOpen(true);
  };

  const handleToggleStatus = async (component: ProductComponent) => {
    try {
      await componentService.updateComponent(component.id, {
        description: component.description,
        costPrice: component.costPrice,
        salePrice: component.salePrice,
        isActive: !component.isActive,
      });
      toast.success(
        component.isActive
          ? 'Componente desactivado correctamente'
          : 'Componente activado correctamente'
      );
      fetchComponents();
    } catch (error: any) {
      toast.error(error?.message || 'Error al cambiar el estado del componente');
    }
  };

  const handleDialogClose = (success?: boolean) => {
    setIsDialogOpen(false);
    setSelectedComponent(null);
    if (success) {
      fetchComponents();
    }
  };

  const columns = useMemo<ColumnDef<ProductComponent, unknown>[]>(
() => [
      columnHelper.accessor('description', {
        header: () => 'Descripción',
        cell: (info) => (
          <span className="font-medium">{info.getValue()}</span>
        ),
        size: 250,
        meta: { skeleton: <Skeleton className="h-4 w-32" /> },
      }),
      columnHelper.accessor('costPrice', {
        header: () => 'Precio Coste',
        cell: (info) => formatPrice(info.getValue()),
        size: 150,
        meta: { skeleton: <Skeleton className="h-4 w-20" /> },
      }),
      columnHelper.accessor('salePrice', {
        header: () => 'Precio Venta',
        cell: (info) => formatPrice(info.getValue()),
        size: 150,
        meta: { skeleton: <Skeleton className="h-4 w-20" /> },
      }),
      columnHelper.accessor('isActive', {
        header: () => 'Estado',
        cell: (info) => (
          <div className="flex items-center gap-1.5">
            {info.getValue() ? (
              <>
                <CheckCircle2 className="size-4 text-green-600" />
                <span className="text-green-600">Activo</span>
              </>
            ) : (
              <>
                <XCircle className="size-4 text-muted-foreground" />
                <span className="text-muted-foreground">Inactivo</span>
              </>
            )}
          </div>
        ),
        size: 120,
        meta: { skeleton: <Skeleton className="h-5 w-16" /> },
      }),
      columnHelper.display({
        id: 'actions',
        header: () => 'Acciones',
        cell: ({ row }) => {
          const component = row.original;
          return (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon">
                  <MoreHorizontal className="size-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={() => handleEdit(component)}>
                  <Pencil className="mr-2 size-4" />
                  Editar
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={() => handleToggleStatus(component)}>
                  <Power className="mr-2 size-4" />
                  {component.isActive ? 'Desactivar' : 'Activar'}
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          );
        },
        size: 70,
        meta: { skeleton: <Skeleton className="h-8 w-8 ml-auto" /> },
      }),
    ],
    []
  );

  const table = useReactTable({
    data: components,
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
          <h1 className="text-2xl font-bold tracking-tight">Componentes</h1>
          <p className="text-muted-foreground">
            Tabla maestra de componentes de joyas (materiales, mano de obra, etc.)
          </p>
        </div>
        <Button onClick={handleCreate}>
          <Plus className="mr-2 size-4" />
          Nuevo Componente
        </Button>
      </div>

      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-4">
          <Input
            placeholder="Buscar por descripción..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPagination(prev => ({ ...prev, pageIndex: 0 }));
            }}
            className="w-64"
          />
          <div className="flex items-center gap-2">
            <Switch
              id="show-inactive"
              checked={showInactive}
              onCheckedChange={(checked) => {
                setShowInactive(checked);
                setPagination(prev => ({ ...prev, pageIndex: 0 }));
              }}
            />
            <label htmlFor="show-inactive" className="text-sm font-medium">
              Mostrar inactivos
            </label>
          </div>
        </div>
        <div className="text-sm text-muted-foreground">
          {totalCount} componente{totalCount !== 1 ? 's' : ''}
        </div>
      </div>

      <DataGridContainer>
        <DataGrid
          table={table}
          recordCount={totalCount}
          isLoading={isLoading}
          emptyMessage="No hay componentes registrados"
        >
          <DataGridTable />
          <DataGridPagination />
        </DataGrid>
      </DataGridContainer>

      <ComponentFormDialog
        open={isDialogOpen}
        onClose={handleDialogClose}
        component={selectedComponent}
      />
    </div>
  );
}

export default ComponentListPage;
