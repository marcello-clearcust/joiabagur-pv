/**
 * Inventory Stock Page
 * Displays stock for a selected point of sale
 */
import { useState, useEffect, useCallback } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Package, FileSpreadsheet, Plus, PenLine, History, Building2 } from 'lucide-react';
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

import { useAuth } from '@/providers/auth-provider';
import { inventoryService } from '@/services/inventory.service';
import { pointOfSaleService } from '@/services/point-of-sale.service';
import { Inventory, PaginatedInventoryResult } from '@/types/inventory.types';
import { PointOfSale } from '@/types/point-of-sale.types';
import { ROUTES } from '@/routing/routes';

export function InventoryPage() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const isAdmin = user?.role === 'Administrator';

  const [pointsOfSale, setPointsOfSale] = useState<PointOfSale[]>([]);
  const [selectedPosId, setSelectedPosId] = useState<string>('');
  const [inventoryResult, setInventoryResult] = useState<PaginatedInventoryResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [stockLoading, setStockLoading] = useState(false);

  // Load points of sale
  useEffect(() => {
    const loadPointsOfSale = async () => {
      try {
        const pos = await pointOfSaleService.getPointsOfSale(false); // Only active
        setPointsOfSale(pos);
        if (pos.length > 0) {
          setSelectedPosId(pos[0].id);
        }
      } catch (error) {
        toast.error('Error al cargar los puntos de venta');
        console.error(error);
      } finally {
        setLoading(false);
      }
    };
    loadPointsOfSale();
  }, []);

  // Load stock when POS changes
  const loadStock = useCallback(async () => {
    if (!selectedPosId) return;
    
    setStockLoading(true);
    try {
      const result = await inventoryService.getStock(selectedPosId);
      setInventoryResult(result);
    } catch (error) {
      toast.error('Error al cargar el stock');
      console.error(error);
    } finally {
      setStockLoading(false);
    }
  }, [selectedPosId]);

  useEffect(() => {
    loadStock();
  }, [loadStock]);

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
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Inventario</h1>
          <p className="text-muted-foreground">
            Stock por punto de venta
          </p>
        </div>
        
        {isAdmin && (
          <div className="flex flex-wrap gap-2">
            <Button variant="outline" size="sm" asChild>
              <Link to={ROUTES.INVENTORY.ASSIGN}>
                <Plus className="mr-2 h-4 w-4" />
                Asignar
              </Link>
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link to={ROUTES.INVENTORY.IMPORT}>
                <FileSpreadsheet className="mr-2 h-4 w-4" />
                Importar
              </Link>
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link to={ROUTES.INVENTORY.ADJUST}>
                <PenLine className="mr-2 h-4 w-4" />
                Ajustar
              </Link>
            </Button>
            <Button variant="outline" size="sm" asChild>
              <Link to={ROUTES.INVENTORY.CENTRALIZED}>
                <Building2 className="mr-2 h-4 w-4" />
                Centralizado
              </Link>
            </Button>
          </div>
        )}
      </div>

      {/* History link for all users */}
      <div className="flex gap-2">
        <Button variant="ghost" size="sm" asChild>
          <Link to={ROUTES.INVENTORY.MOVEMENTS}>
            <History className="mr-2 h-4 w-4" />
            Historial de movimientos
          </Link>
        </Button>
      </div>

      {/* Point of Sale Selector */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Seleccionar Punto de Venta</CardTitle>
          <CardDescription>
            Seleccione un punto de venta para ver su stock
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Select value={selectedPosId} onValueChange={setSelectedPosId}>
            <SelectTrigger className="w-full max-w-sm">
              <SelectValue placeholder="Seleccionar punto de venta" />
            </SelectTrigger>
            <SelectContent>
              {pointsOfSale.map((pos) => (
                <SelectItem key={pos.id} value={pos.id}>
                  {pos.name} ({pos.code})
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </CardContent>
      </Card>

      {/* Stock Table */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-lg">
            <Package className="h-5 w-5" />
            Stock
          </CardTitle>
          {inventoryResult && (
            <CardDescription>
              {inventoryResult.totalCount} productos asignados
            </CardDescription>
          )}
        </CardHeader>
        <CardContent>
          {stockLoading ? (
            <div className="space-y-2">
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : !inventoryResult || inventoryResult.items.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Package className="mb-4 h-12 w-12 text-muted-foreground" />
              <p className="text-muted-foreground">
                No hay productos asignados a este punto de venta
              </p>
              {isAdmin && (
                <Button className="mt-4" variant="outline" asChild>
                  <Link to={ROUTES.INVENTORY.ASSIGN}>
                    <Plus className="mr-2 h-4 w-4" />
                    Asignar productos
                  </Link>
                </Button>
              )}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>SKU</TableHead>
                  <TableHead>Producto</TableHead>
                  <TableHead className="text-right">Cantidad</TableHead>
                  <TableHead>Última Actualización</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {inventoryResult.items.map((item: Inventory) => (
                  <TableRow key={item.id}>
                    <TableCell className="font-mono text-sm">
                      {item.productSku}
                    </TableCell>
                    <TableCell>{item.productName}</TableCell>
                    <TableCell className="text-right">
                      {item.quantity === 0 ? (
                        <Badge variant="secondary" className="font-mono">
                          0
                        </Badge>
                      ) : (
                        <span className="font-mono font-medium">
                          {item.quantity}
                        </span>
                      )}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {new Date(item.lastUpdatedAt).toLocaleDateString('es-ES', {
                        day: '2-digit',
                        month: '2-digit',
                        year: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

export default InventoryPage;
