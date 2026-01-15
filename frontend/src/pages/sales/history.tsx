/**
 * Sales History Page (EP3)
 * Displays paginated sales history with filters
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
import { salesService } from '@/services/sales.service';
import { pointOfSaleService } from '@/services/point-of-sale.service';
import { ROUTES } from '@/routing/routes';
import type { Sale, SalesHistoryFilterRequest } from '@/types/sales.types';
import type { PointOfSale } from '@/types/point-of-sale.types';

export function SalesHistoryPage() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Administrator';

  // State
  const [loading, setLoading] = useState(true);
  const [sales, setSales] = useState<Sale[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [pointsOfSale, setPointsOfSale] = useState<PointOfSale[]>([]);
  const [selectedSale, setSelectedSale] = useState<Sale | null>(null);
  const [photoDialogOpen, setPhotoDialogOpen] = useState(false);

  // Filters
  const [filters, setFilters] = useState<SalesHistoryFilterRequest>({
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

  // Load sales when filters change
  const loadSales = useCallback(async () => {
    setLoading(true);
    try {
      const result = await salesService.getSalesHistory(filters);
      setSales(result.sales);
      setTotalCount(result.totalCount);
      setTotalPages(result.totalPages);
    } catch (error) {
      toast.error('Error al cargar historial de ventas');
      console.error(error);
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    if (filters.startDate && filters.endDate) {
      loadSales();
    }
  }, [loadSales, filters.startDate, filters.endDate]);

  // Handle filter changes
  const handleFilterChange = (key: keyof SalesHistoryFilterRequest, value: string | undefined) => {
    setFilters(prev => ({
      ...prev,
      [key]: value === 'all' ? undefined : value,
      page: 1, // Reset to first page on filter change
    }));
  };

  // Format currency
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN',
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

  // View sale details
  const handleViewSale = async (sale: Sale) => {
    setSelectedSale(sale);
    if (sale.hasPhoto) {
      setPhotoDialogOpen(true);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link to={ROUTES.SALES.ROOT}>
            <ArrowLeft className="h-5 w-5" />
          </Link>
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Historial de Ventas</h1>
          <p className="text-muted-foreground">
            {totalCount} ventas encontradas
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

      {/* Sales Table */}
      <Card>
        <CardContent className="pt-6">
          {loading ? (
            <div className="space-y-2">
              {[1, 2, 3, 4, 5].map((i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : sales.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Calendar className="mb-4 h-12 w-12 text-muted-foreground" />
              <p className="text-muted-foreground">
                No se encontraron ventas para los filtros seleccionados
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
                    <TableHead className="text-right">Total</TableHead>
                    <TableHead>Método de Pago</TableHead>
                    {isAdmin && <TableHead>Operador</TableHead>}
                    <TableHead className="text-center">Foto</TableHead>
                    <TableHead></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {sales.map((sale) => (
                    <TableRow key={sale.id}>
                      <TableCell className="whitespace-nowrap">
                        {formatDate(sale.saleDate)}
                      </TableCell>
                      <TableCell>
                        <div>
                          <div className="font-medium">{sale.productName}</div>
                          <div className="text-sm text-muted-foreground">
                            SKU: {sale.productSku}
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>{sale.pointOfSaleName}</TableCell>
                      <TableCell className="text-center">{sale.quantity}</TableCell>
                      <TableCell className="text-right font-medium">
                        {formatCurrency(sale.total)}
                      </TableCell>
                      <TableCell>{sale.paymentMethodName}</TableCell>
                      {isAdmin && <TableCell>{sale.userName}</TableCell>}
                      <TableCell className="text-center">
                        {sale.hasPhoto && (
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
                          onClick={() => handleViewSale(sale)}
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
            <DialogTitle>Foto de Venta</DialogTitle>
          </DialogHeader>
          {selectedSale && selectedSale.hasPhoto && (
            <div className="flex items-center justify-center">
              <img
                src={salesService.getSalePhotoUrl(selectedSale.id)}
                alt="Foto de venta"
                className="max-h-[60vh] rounded-lg object-contain"
              />
            </div>
          )}
        </DialogContent>
      </Dialog>

      {/* Sale Details Dialog */}
      <Dialog open={!!selectedSale && !photoDialogOpen} onOpenChange={() => setSelectedSale(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Detalles de Venta</DialogTitle>
          </DialogHeader>
          {selectedSale && (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-muted-foreground">Fecha:</span>
                  <p className="font-medium">{formatDate(selectedSale.saleDate)}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Producto:</span>
                  <p className="font-medium">{selectedSale.productName}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">SKU:</span>
                  <p className="font-medium">{selectedSale.productSku}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Cantidad:</span>
                  <p className="font-medium">{selectedSale.quantity}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Precio Unitario:</span>
                  <p className="font-medium">{formatCurrency(selectedSale.price)}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Total:</span>
                  <p className="font-medium text-primary">{formatCurrency(selectedSale.total)}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Punto de Venta:</span>
                  <p className="font-medium">{selectedSale.pointOfSaleName}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Método de Pago:</span>
                  <p className="font-medium">{selectedSale.paymentMethodName}</p>
                </div>
                {isAdmin && (
                  <div>
                    <span className="text-muted-foreground">Operador:</span>
                    <p className="font-medium">{selectedSale.userName}</p>
                  </div>
                )}
                {selectedSale.notes && (
                  <div className="col-span-2">
                    <span className="text-muted-foreground">Notas:</span>
                    <p className="font-medium">{selectedSale.notes}</p>
                  </div>
                )}
              </div>
              {selectedSale.hasPhoto && (
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

export default SalesHistoryPage;
