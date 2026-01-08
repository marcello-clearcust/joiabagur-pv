/**
 * Centralized Stock Page
 * Displays aggregated stock view across all points of sale (admin only)
 */
import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import {
  ArrowLeft,
  Building2,
  ChevronDown,
  ChevronRight,
  ChevronLeft,
  Package,
} from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
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
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';

import { inventoryService } from '@/services/inventory.service';
import {
  CentralizedStock,
  PaginatedCentralizedStockResult,
} from '@/types/inventory.types';
import { ROUTES } from '@/routing/routes';

function ProductRow({ stock }: { stock: CentralizedStock }) {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <Collapsible open={isOpen} onOpenChange={setIsOpen}>
      <TableRow className="cursor-pointer hover:bg-muted/50">
        <TableCell>
          <CollapsibleTrigger asChild>
            <Button variant="ghost" size="sm" className="h-8 w-8 p-0">
              {isOpen ? (
                <ChevronDown className="h-4 w-4" />
              ) : (
                <ChevronRight className="h-4 w-4" />
              )}
            </Button>
          </CollapsibleTrigger>
        </TableCell>
        <TableCell className="font-mono text-sm">{stock.productSku}</TableCell>
        <TableCell>{stock.productName}</TableCell>
        <TableCell className="text-right">
          <Badge variant="secondary" className="font-mono">
            {stock.totalQuantity}
          </Badge>
        </TableCell>
        <TableCell className="text-right text-muted-foreground">
          {stock.breakdown.length} punto(s)
        </TableCell>
      </TableRow>
      <CollapsibleContent asChild>
        <TableRow>
          <TableCell colSpan={5} className="p-0">
            <div className="bg-muted/30 px-8 py-2">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Punto de Venta</TableHead>
                    <TableHead className="text-right">Cantidad</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {stock.breakdown.map((pos) => (
                    <TableRow key={pos.pointOfSaleId}>
                      <TableCell>{pos.pointOfSaleName}</TableCell>
                      <TableCell className="text-right font-mono">
                        {pos.quantity === 0 ? (
                          <span className="text-muted-foreground">0</span>
                        ) : (
                          pos.quantity
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </TableCell>
        </TableRow>
      </CollapsibleContent>
    </Collapsible>
  );
}

export function InventoryCentralizedPage() {
  const [stockResult, setStockResult] = useState<PaginatedCentralizedStockResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);

  const loadStock = useCallback(async () => {
    setLoading(true);
    try {
      const result = await inventoryService.getCentralizedStock(page, 50);
      setStockResult(result);
    } catch (error) {
      toast.error('Error al cargar el stock centralizado');
      console.error(error);
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    loadStock();
  }, [loadStock]);

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
          <h1 className="text-2xl font-bold tracking-tight">Stock Centralizado</h1>
          <p className="text-muted-foreground">
            Vista agregada del stock por producto
          </p>
        </div>
      </div>

      {/* Stock Table */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-lg">
            <Building2 className="h-5 w-5" />
            Stock por Producto
          </CardTitle>
          {stockResult && (
            <CardDescription>
              {stockResult.totalCount} productos con inventario
            </CardDescription>
          )}
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="space-y-2">
              {[1, 2, 3, 4, 5].map((i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : !stockResult || stockResult.items.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Package className="mb-4 h-12 w-12 text-muted-foreground" />
              <p className="text-muted-foreground">
                No hay productos con inventario asignado
              </p>
            </div>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-12"></TableHead>
                    <TableHead>SKU</TableHead>
                    <TableHead>Producto</TableHead>
                    <TableHead className="text-right">Stock Total</TableHead>
                    <TableHead className="text-right">Distribución</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {stockResult.items.map((stock: CentralizedStock) => (
                    <ProductRow key={stock.productId} stock={stock} />
                  ))}
                </TableBody>
              </Table>

              {/* Pagination */}
              {stockResult.totalPages > 1 && (
                <div className="mt-4 flex items-center justify-between">
                  <p className="text-sm text-muted-foreground">
                    Página {stockResult.page} de {stockResult.totalPages}
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
                      onClick={() => setPage((p) => Math.min(stockResult.totalPages, p + 1))}
                      disabled={page === stockResult.totalPages}
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

export default InventoryCentralizedPage;

