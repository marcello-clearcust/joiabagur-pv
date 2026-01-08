/**
 * Movement History Page
 * Displays inventory movement history with filtering
 */
import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { ArrowLeft, History, Filter, ChevronLeft, ChevronRight } from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';

import { inventoryService } from '@/services/inventory.service';
import { pointOfSaleService } from '@/services/point-of-sale.service';
import { productService } from '@/services/product.service';
import {
  InventoryMovement,
  MovementType,
  PaginatedMovementResult,
} from '@/types/inventory.types';
import { PointOfSale } from '@/types/point-of-sale.types';
import { Product } from '@/types/product.types';
import { ROUTES } from '@/routing/routes';

const MOVEMENT_TYPE_LABELS: Record<MovementType, string> = {
  [MovementType.Sale]: 'Venta',
  [MovementType.Return]: 'Devolución',
  [MovementType.Adjustment]: 'Ajuste',
  [MovementType.Import]: 'Importación',
};

const MOVEMENT_TYPE_VARIANTS: Record<MovementType, 'default' | 'secondary' | 'destructive' | 'outline'> = {
  [MovementType.Sale]: 'destructive',
  [MovementType.Return]: 'default',
  [MovementType.Adjustment]: 'secondary',
  [MovementType.Import]: 'outline',
};

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('es-ES', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function InventoryMovementsPage() {
  const [pointsOfSale, setPointsOfSale] = useState<PointOfSale[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [movementResult, setMovementResult] = useState<PaginatedMovementResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadingMovements, setLoadingMovements] = useState(false);

  // Filters
  const [selectedPosId, setSelectedPosId] = useState<string>('');
  const [selectedProductId, setSelectedProductId] = useState<string>('');
  const [startDate, setStartDate] = useState<string>(() => {
    const date = new Date();
    date.setDate(date.getDate() - 30);
    return date.toISOString().split('T')[0];
  });
  const [endDate, setEndDate] = useState<string>(() => {
    return new Date().toISOString().split('T')[0];
  });
  const [page, setPage] = useState(1);

  // Load initial data
  useEffect(() => {
    const loadData = async () => {
      try {
        const [posData, productsData] = await Promise.all([
          pointOfSaleService.getPointsOfSale(false),
          productService.getProducts(false),
        ]);
        setPointsOfSale(posData);
        setProducts(productsData);
      } catch (error) {
        toast.error('Error al cargar los datos');
        console.error(error);
      } finally {
        setLoading(false);
      }
    };
    loadData();
  }, []);

  // Load movements
  const loadMovements = useCallback(async () => {
    setLoadingMovements(true);
    try {
      const result = await inventoryService.getMovementHistory({
        pointOfSaleId: selectedPosId || undefined,
        productId: selectedProductId || undefined,
        startDate: startDate || undefined,
        endDate: endDate ? `${endDate}T23:59:59` : undefined,
        page,
        pageSize: 50,
      });
      setMovementResult(result);
    } catch (error) {
      toast.error('Error al cargar los movimientos');
      console.error(error);
    } finally {
      setLoadingMovements(false);
    }
  }, [selectedPosId, selectedProductId, startDate, endDate, page]);

  useEffect(() => {
    if (!loading) {
      loadMovements();
    }
  }, [loadMovements, loading]);

  const handleClearFilters = () => {
    setSelectedPosId('');
    setSelectedProductId('');
    const date = new Date();
    date.setDate(date.getDate() - 30);
    setStartDate(date.toISOString().split('T')[0]);
    setEndDate(new Date().toISOString().split('T')[0]);
    setPage(1);
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link to={ROUTES.INVENTORY.ROOT}>
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Historial de Movimientos</h1>
          <p className="text-muted-foreground">
            Registro de todos los cambios de inventario
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
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <div className="space-y-2">
              <Label>Punto de Venta</Label>
              <Select value={selectedPosId} onValueChange={(value) => { setSelectedPosId(value); setPage(1); }}>
                <SelectTrigger>
                  <SelectValue placeholder="Todos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Todos</SelectItem>
                  {pointsOfSale.map((pos) => (
                    <SelectItem key={pos.id} value={pos.id}>
                      {pos.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Producto</Label>
              <Select value={selectedProductId} onValueChange={(value) => { setSelectedProductId(value); setPage(1); }}>
                <SelectTrigger>
                  <SelectValue placeholder="Todos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="">Todos</SelectItem>
                  {products.map((product) => (
                    <SelectItem key={product.id} value={product.id}>
                      {product.sku} - {product.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Desde</Label>
              <Input
                type="date"
                value={startDate}
                onChange={(e) => { setStartDate(e.target.value); setPage(1); }}
              />
            </div>

            <div className="space-y-2">
              <Label>Hasta</Label>
              <Input
                type="date"
                value={endDate}
                onChange={(e) => { setEndDate(e.target.value); setPage(1); }}
              />
            </div>
          </div>

          <div className="mt-4">
            <Button variant="outline" size="sm" onClick={handleClearFilters}>
              Limpiar filtros
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Movements Table */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-lg">
            <History className="h-5 w-5" />
            Movimientos
          </CardTitle>
          {movementResult && (
            <CardDescription>
              {movementResult.totalCount} movimientos encontrados
            </CardDescription>
          )}
        </CardHeader>
        <CardContent>
          {loadingMovements ? (
            <div className="space-y-2">
              {[1, 2, 3, 4, 5].map((i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : !movementResult || movementResult.items.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <History className="mb-4 h-12 w-12 text-muted-foreground" />
              <p className="text-muted-foreground">
                No se encontraron movimientos con los filtros seleccionados
              </p>
            </div>
          ) : (
            <>
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Fecha</TableHead>
                      <TableHead>Tipo</TableHead>
                      <TableHead>Producto</TableHead>
                      <TableHead>Punto de Venta</TableHead>
                      <TableHead className="text-right">Cambio</TableHead>
                      <TableHead className="text-right">Antes</TableHead>
                      <TableHead className="text-right">Después</TableHead>
                      <TableHead>Usuario</TableHead>
                      <TableHead>Motivo</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {movementResult.items.map((movement: InventoryMovement) => (
                      <TableRow key={movement.id}>
                        <TableCell className="whitespace-nowrap text-sm">
                          {formatDate(movement.movementDate)}
                        </TableCell>
                        <TableCell>
                          <Badge variant={MOVEMENT_TYPE_VARIANTS[movement.movementType]}>
                            {MOVEMENT_TYPE_LABELS[movement.movementType]}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <div>
                            <p className="font-medium">{movement.productName}</p>
                            <p className="text-sm text-muted-foreground">
                              {movement.productSku}
                            </p>
                          </div>
                        </TableCell>
                        <TableCell>{movement.pointOfSaleName}</TableCell>
                        <TableCell className="text-right font-mono">
                          <span
                            className={
                              movement.quantityChange > 0
                                ? 'text-green-600'
                                : 'text-destructive'
                            }
                          >
                            {movement.quantityChange > 0 ? '+' : ''}
                            {movement.quantityChange}
                          </span>
                        </TableCell>
                        <TableCell className="text-right font-mono">
                          {movement.quantityBefore}
                        </TableCell>
                        <TableCell className="text-right font-mono">
                          {movement.quantityAfter}
                        </TableCell>
                        <TableCell>{movement.userName}</TableCell>
                        <TableCell className="max-w-[200px] truncate">
                          {movement.reason || '-'}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>

              {/* Pagination */}
              {movementResult.totalPages > 1 && (
                <div className="mt-4 flex items-center justify-between">
                  <p className="text-sm text-muted-foreground">
                    Página {movementResult.page} de {movementResult.totalPages}
                  </p>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                      disabled={page === 1}
                    >
                      <ChevronLeft className="h-4 w-4" />
                      Anterior
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setPage((p) => Math.min(movementResult.totalPages, p + 1))}
                      disabled={page === movementResult.totalPages}
                    >
                      Siguiente
                      <ChevronRight className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

export default InventoryMovementsPage;

