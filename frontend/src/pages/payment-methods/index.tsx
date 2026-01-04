/**
 * Payment Methods Module - EP6
 * Payment methods management page with CRUD operations
 */

import { useState, useEffect, useMemo } from 'react';
import {
  useReactTable,
  getCoreRowModel,
  ColumnDef,
  createColumnHelper,
} from '@tanstack/react-table';
import { Plus, Pencil, MoreHorizontal, CheckCircle2, XCircle, Power } from 'lucide-react';
import { paymentMethodService } from '@/services/payment-method.service';
import { PaymentMethod } from '@/types/payment-method.types';
import { DataGrid, DataGridContainer } from '@/components/ui/data-grid';
import { DataGridTable } from '@/components/ui/data-grid-table';
import { DataGridPagination } from '@/components/ui/data-grid-pagination';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
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
import { PaymentMethodFormDialog } from './components/payment-method-form-dialog';

const columnHelper = createColumnHelper<PaymentMethod>();

export function PaymentMethodsPage() {
  const [paymentMethods, setPaymentMethods] = useState<PaymentMethod[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [selectedPaymentMethod, setSelectedPaymentMethod] = useState<PaymentMethod | null>(null);
  const [showInactive, setShowInactive] = useState(false);

  // Fetch payment methods
  const fetchPaymentMethods = async () => {
    setIsLoading(true);
    try {
      const data = await paymentMethodService.getPaymentMethods(showInactive);
      setPaymentMethods(data);
    } catch (error) {
      toast.error('Error al cargar los métodos de pago');
      console.error('Failed to fetch payment methods:', error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchPaymentMethods();
  }, [showInactive]);

  // Handle create payment method
  const handleCreate = () => {
    setSelectedPaymentMethod(null);
    setIsDialogOpen(true);
  };

  // Handle edit payment method
  const handleEdit = (paymentMethod: PaymentMethod) => {
    setSelectedPaymentMethod(paymentMethod);
    setIsDialogOpen(true);
  };

  // Handle status toggle
  const handleToggleStatus = async (paymentMethod: PaymentMethod) => {
    try {
      await paymentMethodService.changePaymentMethodStatus(
        paymentMethod.id,
        !paymentMethod.isActive
      );
      toast.success(
        paymentMethod.isActive
          ? 'Método de pago desactivado correctamente'
          : 'Método de pago activado correctamente'
      );
      fetchPaymentMethods();
    } catch (error: any) {
      const errorMessage =
        error?.message || 'Error al cambiar el estado del método de pago';
      toast.error(errorMessage);
      console.error('Failed to toggle payment method status:', error);
    }
  };

  // Handle dialog close
  const handleDialogClose = (success?: boolean) => {
    setIsDialogOpen(false);
    setSelectedPaymentMethod(null);
    if (success) {
      fetchPaymentMethods();
    }
  };

  // Table columns
  const columns = useMemo<ColumnDef<PaymentMethod, unknown>[]>(
    () => [
      columnHelper.accessor('code', {
        header: () => 'Código',
        cell: (info) => (
          <span className="font-mono font-medium">{info.getValue()}</span>
        ),
        size: 150,
        meta: {
          skeleton: <Skeleton className="h-4 w-24" />,
        },
      }),
      columnHelper.accessor('name', {
        header: () => 'Nombre',
        cell: (info) => info.getValue(),
        size: 200,
        meta: {
          skeleton: <Skeleton className="h-4 w-32" />,
        },
      }),
      columnHelper.accessor('description', {
        header: () => 'Descripción',
        cell: (info) => (
          <span className="text-muted-foreground">
            {info.getValue() || '-'}
          </span>
        ),
        size: 300,
        meta: {
          skeleton: <Skeleton className="h-4 w-48" />,
        },
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
        meta: {
          skeleton: <Skeleton className="h-5 w-16" />,
        },
      }),
      columnHelper.accessor('createdAt', {
        header: () => 'Fecha de Creación',
        cell: (info) => new Date(info.getValue()).toLocaleDateString('es-ES'),
        size: 150,
        meta: {
          skeleton: <Skeleton className="h-4 w-20" />,
        },
      }),
      columnHelper.display({
        id: 'actions',
        header: () => 'Acciones',
        cell: ({ row }) => {
          const paymentMethod = row.original;
          return (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon">
                  <MoreHorizontal className="size-4" />
                  <span className="sr-only">Abrir menú</span>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={() => handleEdit(paymentMethod)}>
                  <Pencil className="mr-2 size-4" />
                  Editar
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={() => handleToggleStatus(paymentMethod)}>
                  <Power className="mr-2 size-4" />
                  {paymentMethod.isActive ? 'Desactivar' : 'Activar'}
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          );
        },
        size: 70,
        meta: {
          skeleton: <Skeleton className="h-8 w-8 ml-auto" />,
        },
      }),
    ],
    []
  );

  const table = useReactTable({
    data: paymentMethods,
    columns,
    getCoreRowModel: getCoreRowModel(),
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Métodos de Pago</h1>
          <p className="text-muted-foreground">
            Configuración de métodos de pago disponibles en el sistema
          </p>
        </div>
        <Button onClick={handleCreate}>
          <Plus className="mr-2 size-4" />
          Nuevo Método de Pago
        </Button>
      </div>

      {/* Filter Controls */}
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-2">
          <Switch
            id="show-inactive"
            checked={showInactive}
            onCheckedChange={setShowInactive}
          />
          <label
            htmlFor="show-inactive"
            className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
          >
            Mostrar inactivos
          </label>
        </div>

        <div className="text-sm text-muted-foreground">
          {paymentMethods.length} método{paymentMethods.length !== 1 ? 's' : ''} de pago
        </div>
      </div>

      {/* Predefined Methods Info */}
      <div className="rounded-lg border bg-muted/50 p-4">
        <div className="flex items-start gap-3">
          <Badge variant="outline" className="mt-0.5">
            Info
          </Badge>
          <div className="flex-1 space-y-1">
            <p className="text-sm font-medium">Métodos de pago predefinidos</p>
            <p className="text-sm text-muted-foreground">
              El sistema incluye 6 métodos de pago predefinidos: Efectivo (CASH), Bizum (BIZUM),
              Transferencia (TRANSFER), Tarjeta TPV propio (CARD_OWN), Tarjeta TPV punto de venta
              (CARD_POS) y PayPal (PAYPAL). Puedes crear métodos adicionales según las necesidades
              del negocio.
            </p>
          </div>
        </div>
      </div>

      {/* Data Table */}
      <DataGridContainer>
        <DataGrid
          table={table}
          recordCount={paymentMethods.length}
          isLoading={isLoading}
          emptyMessage="No hay métodos de pago registrados"
        >
          <DataGridTable />
          <DataGridPagination />
        </DataGrid>
      </DataGridContainer>

      {/* Form Dialog */}
      <PaymentMethodFormDialog
        open={isDialogOpen}
        onClose={handleDialogClose}
        paymentMethod={selectedPaymentMethod}
      />
    </div>
  );
}

export default PaymentMethodsPage;
