/**
 * Inventory Assignment Page
 * Allows administrators to assign products to points of sale
 */
import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { ArrowLeft, Package, Check, X, Loader2 } from 'lucide-react';
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
import { Checkbox } from '@/components/ui/checkbox';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';

import { inventoryService } from '@/services/inventory.service';
import { pointOfSaleService } from '@/services/point-of-sale.service';
import { productService } from '@/services/product.service';
import { PointOfSale } from '@/types/point-of-sale.types';
import { Product } from '@/types/product.types';
import { Inventory } from '@/types/inventory.types';
import { ROUTES } from '@/routing/routes';

export function InventoryAssignPage() {
  const [pointsOfSale, setPointsOfSale] = useState<PointOfSale[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [selectedPosId, setSelectedPosId] = useState<string>('');
  const [assignedProducts, setAssignedProducts] = useState<Set<string>>(new Set());
  const [selectedProducts, setSelectedProducts] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(true);
  const [assigning, setAssigning] = useState(false);
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  const [productToUnassign, setProductToUnassign] = useState<Inventory | null>(null);
  const [assignedInventories, setAssignedInventories] = useState<Inventory[]>([]);

  // Load initial data
  useEffect(() => {
    const loadData = async () => {
      try {
        const [posData, productsData] = await Promise.all([
          pointOfSaleService.getPointsOfSale(false),
          productService.getProducts(false), // Only active products
        ]);
        setPointsOfSale(posData);
        setProducts(productsData);
        if (posData.length > 0) {
          setSelectedPosId(posData[0].id);
        }
      } catch (error) {
        toast.error('Error al cargar los datos');
        console.error(error);
      } finally {
        setLoading(false);
      }
    };
    loadData();
  }, []);

  // Load assigned products when POS changes
  const loadAssignedProducts = useCallback(async () => {
    if (!selectedPosId) return;
    
    try {
      const result = await inventoryService.getAssignedProducts(selectedPosId);
      const assignedIds = new Set(result.items.map((i) => i.productId));
      setAssignedProducts(assignedIds);
      setAssignedInventories(result.items);
      setSelectedProducts(new Set()); // Reset selection
    } catch (error) {
      toast.error('Error al cargar productos asignados');
      console.error(error);
    }
  }, [selectedPosId]);

  useEffect(() => {
    loadAssignedProducts();
  }, [loadAssignedProducts]);

  const handleProductSelect = (productId: string, checked: boolean) => {
    const newSelected = new Set(selectedProducts);
    if (checked) {
      newSelected.add(productId);
    } else {
      newSelected.delete(productId);
    }
    setSelectedProducts(newSelected);
  };

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      const unassignedIds = products
        .filter((p) => !assignedProducts.has(p.id))
        .map((p) => p.id);
      setSelectedProducts(new Set(unassignedIds));
    } else {
      setSelectedProducts(new Set());
    }
  };

  const handleAssign = async () => {
    if (selectedProducts.size === 0 || !selectedPosId) return;

    setAssigning(true);
    try {
      const result = await inventoryService.assignProducts({
        productIds: Array.from(selectedProducts),
        pointOfSaleId: selectedPosId,
      });

      if (result.success) {
        toast.success(result.message);
        await loadAssignedProducts();
      } else {
        toast.error(result.message);
      }
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Error desconocido';
      toast.error(`Error al asignar productos: ${errorMessage}`);
      console.error(error);
    } finally {
      setAssigning(false);
    }
  };

  const handleUnassignClick = (inventory: Inventory) => {
    if (inventory.quantity > 0) {
      toast.error(
        `No se puede desasignar un producto con stock (${inventory.quantity} unidades). Primero ajuste el stock a 0.`
      );
      return;
    }
    setProductToUnassign(inventory);
    setShowConfirmDialog(true);
  };

  const handleUnassignConfirm = async () => {
    if (!productToUnassign) return;

    try {
      const result = await inventoryService.unassignProduct({
        productId: productToUnassign.productId,
        pointOfSaleId: selectedPosId,
      });

      if (result.success) {
        toast.success('Producto desasignado correctamente');
        await loadAssignedProducts();
      } else {
        toast.error(result.errorMessage || 'Error al desasignar');
      }
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Error desconocido';
      toast.error(`Error al desasignar: ${errorMessage}`);
      console.error(error);
    } finally {
      setShowConfirmDialog(false);
      setProductToUnassign(null);
    }
  };

  const unassignedProducts = products.filter((p) => !assignedProducts.has(p.id));

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
          <h1 className="text-2xl font-bold tracking-tight">Asignar Productos</h1>
          <p className="text-muted-foreground">
            Asignar productos a un punto de venta
          </p>
        </div>
      </div>

      {/* Point of Sale Selector */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Punto de Venta</CardTitle>
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

      {/* Products to Assign */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <CardTitle className="text-lg">Productos Disponibles</CardTitle>
            <CardDescription>
              Seleccione los productos a asignar ({selectedProducts.size} seleccionados)
            </CardDescription>
          </div>
          <Button
            onClick={handleAssign}
            disabled={selectedProducts.size === 0 || assigning}
          >
            {assigning ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Asignando...
              </>
            ) : (
              <>
                <Check className="mr-2 h-4 w-4" />
                Asignar ({selectedProducts.size})
              </>
            )}
          </Button>
        </CardHeader>
        <CardContent>
          {unassignedProducts.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-8 text-center">
              <Package className="mb-4 h-12 w-12 text-muted-foreground" />
              <p className="text-muted-foreground">
                Todos los productos están asignados a este punto de venta
              </p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-12">
                    <Checkbox
                      checked={
                        unassignedProducts.length > 0 &&
                        selectedProducts.size === unassignedProducts.length
                      }
                      onCheckedChange={handleSelectAll}
                    />
                  </TableHead>
                  <TableHead>SKU</TableHead>
                  <TableHead>Nombre</TableHead>
                  <TableHead>Precio</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {unassignedProducts.map((product) => (
                  <TableRow key={product.id}>
                    <TableCell>
                      <Checkbox
                        checked={selectedProducts.has(product.id)}
                        onCheckedChange={(checked) =>
                          handleProductSelect(product.id, checked as boolean)
                        }
                      />
                    </TableCell>
                    <TableCell className="font-mono text-sm">
                      {product.sku}
                    </TableCell>
                    <TableCell>{product.name}</TableCell>
                    <TableCell>€{product.price.toFixed(2)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Assigned Products */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Productos Asignados</CardTitle>
          <CardDescription>
            {assignedInventories.length} productos asignados a este punto de venta
          </CardDescription>
        </CardHeader>
        <CardContent>
          {assignedInventories.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-8 text-center">
              <Package className="mb-4 h-12 w-12 text-muted-foreground" />
              <p className="text-muted-foreground">
                No hay productos asignados
              </p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>SKU</TableHead>
                  <TableHead>Nombre</TableHead>
                  <TableHead className="text-right">Cantidad</TableHead>
                  <TableHead className="w-24"></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {assignedInventories.map((inventory) => (
                  <TableRow key={inventory.id}>
                    <TableCell className="font-mono text-sm">
                      {inventory.productSku}
                    </TableCell>
                    <TableCell>{inventory.productName}</TableCell>
                    <TableCell className="text-right font-mono">
                      {inventory.quantity}
                    </TableCell>
                    <TableCell>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleUnassignClick(inventory)}
                        disabled={inventory.quantity > 0}
                        title={
                          inventory.quantity > 0
                            ? 'Ajuste el stock a 0 antes de desasignar'
                            : 'Desasignar producto'
                        }
                      >
                        <X className="h-4 w-4" />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Unassign Confirmation Dialog */}
      <AlertDialog open={showConfirmDialog} onOpenChange={setShowConfirmDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>¿Desasignar producto?</AlertDialogTitle>
            <AlertDialogDescription>
              ¿Está seguro de que desea desasignar{' '}
              <strong>{productToUnassign?.productName}</strong> de este punto de venta?
              Esta acción se puede revertir asignando el producto nuevamente.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleUnassignConfirm}>
              Desasignar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

export default InventoryAssignPage;

