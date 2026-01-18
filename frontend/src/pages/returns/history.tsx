/**
 * Returns History Page (EP5)
 * Displays paginated returns history with filters
 */
import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { ArrowLeft, Calendar, Filter, Image, Eye } from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';

import { useAuth } from '@/providers/auth-provider';
import { returnsService } from '@/services/returns.service';
import { pointOfSaleService } from '@/services/point-of-sale.service';
import { ROUTES } from '@/routing/routes';
import type { Return, ReturnsHistoryFilterRequest, ReturnCategoryLabels } from '@/types/returns.types';
import { ReturnCategory } from '@/types/returns.types';
import type { PointOfSale } from '@/types/point-of-sale.types';

const CategoryLabels: Record<ReturnCategory, string> = {
  [ReturnCategory.Defectuoso]: 'Defectuoso',
  [ReturnCategory.TamañoIncorrecto]: 'Tamaño Incorrecto',
  [ReturnCategory.NoSatisfecho]: 'No Satisfecho',
  [ReturnCategory.Otro]: 'Otro',
};

export function ReturnsHistoryPage() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Administrator';

  // State
  const [loading, setLoading] = useState(true);
  const [returns, setReturns] = useState<Return[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [pointsOfSale, setPointsOfSale] = useState<PointOfSale[]>([]);
  const [selectedReturn, setSelectedReturn] = useState<Return | null>(null);
  const [photoDialogOpen, setPhotoDialogOpen] = useState(false);

  // Filters
  const [filters, setFilters] = useState<ReturnsHistoryFilterRequest>({
    page: 1,
    pageSize: 20,
  });

  // Calculate default date range (last 30 days)
  const getDefaultDateRange = () => {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - 30);
    return {
      startDate: startDate.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0],
    };
  };

  // Load initial data
  useEffect(() => {
    const loadData = async () => {
      try {
        const posData = await pointOfSaleService.getPointsOfSale();
        setPointsOfSale(posData);
        
        // Set default date range
        const { startDate, endDate } = getDefaultDateRange();
        setFilters(prev => ({ ...prev, startDate, endDate }));
      } catch (error) {
        toast.error('Error al cargar datos');
        console.error(error);
      }
    };
    loadData();
  }, []);

  // Load returns when filters change
  const loadReturns = useCallback(async () => {
    setLoading(true);
    try {
      const result = await returnsService.getReturnsHistory(filters);
      setReturns(result.returns);
      setTotalCount(result.totalCount);
      setTotalPages(result.totalPages);
    } catch (error) {
      toast.error('Error al cargar historial de devoluciones');
      console.error(error);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    if (filters.startDate && filters.endDate) {
      loadReturns();
    }
  }, [loadReturns, filters.startDate, filters.endDate]);

  // Handle filter changes
  const handleFilterChange = (key: keyof ReturnsHistoryFilterRequest, value: string | undefined) => {
    setFilters(prev => ({
      ...prev,
      [key]: value === 'all' ? undefined : value,
      page: 1, // Reset to first page on filter change
    }));
  };

  // Format currency
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('es-ES', {
      style: 'currency',
      currency: 'EUR',
    }).format(amount);
  };

  // Format date
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-ES', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  // Get category badge variant
  const getCategoryVariant = (category: ReturnCategory) => {
    switch (category) {
      case ReturnCategory.Defectuoso:
        return 'destructive';
      case ReturnCategory.TamañoIncorrecto:
        return 'secondary';
      case ReturnCategory.NoSatisfecho:
        return 'outline';
      default:
        return 'default';
    }
  };

  // View return details
  const handleViewReturn = async (returnItem: Return) => {
    setSelectedReturn(returnItem);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link to={ROUTES.RETURNS.ROOT}>
            <ArrowLeft className="h-5 w-5" />
          </Link>
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Historial de Devoluciones</h1>
          <p className="text-muted-foreground">
            {totalCount} devoluciones encontradas
          </p>
        </div>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-lg">
            <Filter className="h-5 w-5" />
            Filtros
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-4">
            {/* Date Range */}
            <div className="space-y-2">
              <Label>Fecha Inicio</Label>
              <Input
                type="date"
                value={filters.startDate || ''}
                onChange={(e) => handleFilterChange('startDate', e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>Fecha Fin</Label>
              <Input
                type="date"
                value={filters.endDate || ''}
                onChange={(e) => handleFilterChange('endDate', e.target.value)}
              />
            </div>

            {/* Point of Sale */}
            <div className="space-y-2">
              <Label>Punto de Venta</Label>
              <Select
                value={filters.pointOfSaleId || 'all'}
                onValueChange={(value) => handleFilterChange('pointOfSaleId', value)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos</SelectItem>
                  {pointsOfSale.map((pos) => (
                    <SelectItem key={pos.id} value={pos.id}>
                      {pos.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Page Size */}
            <div className="space-y-2">
              <Label>Resultados por página</Label>
              <Select
                value={filters.pageSize?.toString() || '20'}
                onValueChange={(value) => handleFilterChange('pageSize', value)}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="10">10</SelectItem>
                  <SelectItem value="20">20</SelectItem>
                  <SelectItem value="50">50</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Returns Table */}
      <Card>
        <CardContent className="pt-6">
          {loading ? (
            <div className="space-y-2">
              {[1, 2, 3, 4, 5].map((i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : returns.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Calendar className="mb-4 h-12 w-12 text-muted-foreground" />
              <p className="text-muted-foreground">
                No se encontraron devoluciones para los filtros seleccionados
              </p>
            </div>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Fecha</TableHead>
                    <TableHead>Producto</TableHead>
                    <TableHead>Punto de Venta</TableHead>
                    <TableHead className="text-center">Cantidad</TableHead>
                    <TableHead className="text-right">Valor</TableHead>
                    <TableHead>Categoría</TableHead>
                    {isAdmin && <TableHead>Operador</TableHead>}
                    <TableHead className="text-center">Foto</TableHead>
                    <TableHead></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {returns.map((returnItem) => (
                    <TableRow key={returnItem.id}>
                      <TableCell className="whitespace-nowrap">
                        {formatDate(returnItem.returnDate)}
                      </TableCell>
                      <TableCell>
                        <div>
                          <div className="font-medium">{returnItem.productName}</div>
                          <div className="text-sm text-muted-foreground">
                            SKU: {returnItem.productSku}
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>{returnItem.pointOfSaleName}</TableCell>
                      <TableCell className="text-center">{returnItem.quantity}</TableCell>
                      <TableCell className="text-right font-medium">
                        {formatCurrency(returnItem.totalValue)}
                      </TableCell>
                      <TableCell>
                        <Badge variant={getCategoryVariant(returnItem.category) as 'default' | 'secondary' | 'destructive' | 'outline'}>
                          {CategoryLabels[returnItem.category] || returnItem.categoryName}
                        </Badge>
                      </TableCell>
                      {isAdmin && <TableCell>{returnItem.userName}</TableCell>}
                      <TableCell className="text-center">
                        {returnItem.hasPhoto && (
                          <Badge variant="secondary" className="gap-1">
                            <Image className="h-3 w-3" />
                            Sí
                          </Badge>
                        )}
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleViewReturn(returnItem)}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {/* Pagination */}
              <div className="mt-4 flex items-center justify-between">
                <p className="text-sm text-muted-foreground">
                  Página {filters.page} de {totalPages}
                </p>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={filters.page === 1}
                    onClick={() => setFilters(prev => ({ ...prev, page: (prev.page || 1) - 1 }))}
                  >
                    Anterior
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={filters.page === totalPages}
                    onClick={() => setFilters(prev => ({ ...prev, page: (prev.page || 1) + 1 }))}
                  >
                    Siguiente
                  </Button>
                </div>
              </div>
            </>
          )}
        </CardContent>
      </Card>

      {/* Photo Dialog */}
      <Dialog open={photoDialogOpen} onOpenChange={setPhotoDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Foto de Devolución</DialogTitle>
          </DialogHeader>
          {selectedReturn && selectedReturn.hasPhoto && (
            <div className="flex items-center justify-center">
              <img
                src={returnsService.getReturnPhotoUrl(selectedReturn.id)}
                alt="Foto de devolución"
                className="max-h-[60vh] rounded-lg object-contain"
              />
            </div>
          )}
        </DialogContent>
      </Dialog>

      {/* Return Details Dialog */}
      <Dialog open={!!selectedReturn && !photoDialogOpen} onOpenChange={() => setSelectedReturn(null)}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Detalles de Devolución</DialogTitle>
          </DialogHeader>
          {selectedReturn && (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-muted-foreground">Fecha:</span>
                  <p className="font-medium">{formatDate(selectedReturn.returnDate)}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Producto:</span>
                  <p className="font-medium">{selectedReturn.productName}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">SKU:</span>
                  <p className="font-medium">{selectedReturn.productSku}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Cantidad:</span>
                  <p className="font-medium">{selectedReturn.quantity}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Valor Total:</span>
                  <p className="font-medium text-primary">{formatCurrency(selectedReturn.totalValue)}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Categoría:</span>
                  <p className="font-medium">
                    <Badge variant={getCategoryVariant(selectedReturn.category) as 'default' | 'secondary' | 'destructive' | 'outline'}>
                      {CategoryLabels[selectedReturn.category] || selectedReturn.categoryName}
                    </Badge>
                  </p>
                </div>
                <div>
                  <span className="text-muted-foreground">Punto de Venta:</span>
                  <p className="font-medium">{selectedReturn.pointOfSaleName}</p>
                </div>
                {isAdmin && (
                  <div>
                    <span className="text-muted-foreground">Operador:</span>
                    <p className="font-medium">{selectedReturn.userName}</p>
                  </div>
                )}
                {selectedReturn.reason && (
                  <div className="col-span-2">
                    <span className="text-muted-foreground">Motivo:</span>
                    <p className="font-medium">{selectedReturn.reason}</p>
                  </div>
                )}
              </div>

              {/* Associated Sales */}
              {selectedReturn.associatedSales && selectedReturn.associatedSales.length > 0 && (
                <div className="border-t pt-4">
                  <h4 className="mb-2 font-medium">Ventas Asociadas</h4>
                  <div className="space-y-2">
                    {selectedReturn.associatedSales.map((sale) => (
                      <div key={sale.saleId} className="rounded border p-2 text-sm">
                        <div className="flex justify-between">
                          <span>Venta del {formatDate(sale.saleDate)}</span>
                          <span className="font-medium">{formatCurrency(sale.subtotal)}</span>
                        </div>
                        <div className="text-muted-foreground">
                          Cantidad: {sale.quantity} × {formatCurrency(sale.unitPrice)} | {sale.paymentMethodName}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {selectedReturn.hasPhoto && (
                <Button
                  variant="outline"
                  className="w-full"
                  onClick={() => setPhotoDialogOpen(true)}
                >
                  <Image className="mr-2 h-4 w-4" />
                  Ver Foto
                </Button>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default ReturnsHistoryPage;
