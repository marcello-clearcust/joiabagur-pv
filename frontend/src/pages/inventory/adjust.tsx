/**
 * Stock Adjustment Page
 * Allows administrators to adjust stock quantities
 */
import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { ArrowLeft, PenLine, Loader2, Minus, Plus } from 'lucide-react';
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
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Skeleton } from '@/components/ui/skeleton';
import { Badge } from '@/components/ui/badge';
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
import { PointOfSale } from '@/types/point-of-sale.types';
import { Inventory, PaginatedInventoryResult } from '@/types/inventory.types';
import { ROUTES } from '@/routing/routes';

export function InventoryAdjustPage() {
  const [pointsOfSale, setPointsOfSale] = useState<PointOfSale[]>([]);
  const [selectedPosId, setSelectedPosId] = useState<string>('');
  const [inventoryResult, setInventoryResult] = useState<PaginatedInventoryResult | null>(null);
  const [selectedInventory, setSelectedInventory] = useState<Inventory | null>(null);
  const [quantityChange, setQuantityChange] = useState<number>(0);
  const [reason, setReason] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [adjusting, setAdjusting] = useState(false);
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);

  // Load initial data
  useEffect(() => {
    const loadData = async () => {
      try {
        const pos = await pointOfSaleService.getPointsOfSale(false);
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
    loadData();
  }, []);

  // Load inventory when POS changes
  const loadInventory = useCallback(async () => {
    if (!selectedPosId) return;
    
    try {
      const result = await inventoryService.getStock(selectedPosId);
      setInventoryResult(result);
      setSelectedInventory(null);
      setQuantityChange(0);
      setReason('');
    } catch (error) {
      toast.error('Error al cargar el inventario');
      console.error(error);
    }
  }, [selectedPosId]);

  useEffect(() => {
    loadInventory();
  }, [loadInventory]);

  const handleProductSelect = (productId: string) => {
    const inventory = inventoryResult?.items.find((i) => i.productId === productId);
    setSelectedInventory(inventory || null);
    setQuantityChange(0);
    setReason('');
  };

  const handleAdjustment = () => {
    if (!selectedInventory || quantityChange === 0 || !reason.trim()) {
      toast.error('Complete todos los campos requeridos');
      return;
    }

    const newQuantity = selectedInventory.quantity + quantityChange;
    if (newQuantity < 0) {
      toast.error(`El ajuste resultaría en stock negativo (${newQuantity})`);
      return;
    }

    setShowConfirmDialog(true);
  };

  const handleConfirmAdjustment = async () => {
    if (!selectedInventory) return;

    setAdjusting(true);
    try {
      const result = await inventoryService.adjustStock({
        productId: selectedInventory.productId,
        pointOfSaleId: selectedPosId,
        quantityChange,
        reason: reason.trim(),
      });

      if (result.success) {
        toast.success(
          `Stock ajustado: ${result.quantityBefore} → ${result.quantityAfter}`
        );
        await loadInventory();
      } else {
        toast.error(result.errorMessage || 'Error al ajustar el stock');
      }
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Error desconocido';
      toast.error(`Error al ajustar: ${errorMessage}`);
      console.error(error);
    } finally {
      setAdjusting(false);
      setShowConfirmDialog(false);
    }
  };

  const newQuantity = selectedInventory
    ? selectedInventory.quantity + quantityChange
    : 0;

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
          <h1 className="text-2xl font-bold tracking-tight">Ajustar Stock</h1>
          <p className="text-muted-foreground">
            Ajuste manual de cantidades de stock
          </p>
        </div>
      </div>

      {/* Adjustment Form */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-lg">
            <PenLine className="h-5 w-5" />
            Ajuste de Stock
          </CardTitle>
          <CardDescription>
            Seleccione un producto y especifique el ajuste
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Point of Sale Selector */}
          <div className="space-y-2">
            <Label>Punto de Venta</Label>
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
          </div>

          {/* Product Selector */}
          <div className="space-y-2">
            <Label>Producto</Label>
            <Select
              value={selectedInventory?.productId || ''}
              onValueChange={handleProductSelect}
              disabled={!inventoryResult || inventoryResult.items.length === 0}
            >
              <SelectTrigger className="w-full max-w-md">
                <SelectValue placeholder="Seleccionar producto" />
              </SelectTrigger>
              <SelectContent>
                {inventoryResult?.items.map((item) => (
                  <SelectItem key={item.productId} value={item.productId}>
                    {item.productSku} - {item.productName} (Stock: {item.quantity})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {inventoryResult && inventoryResult.items.length === 0 && (
              <p className="text-sm text-muted-foreground">
                No hay productos asignados a este punto de venta
              </p>
            )}
          </div>

          {selectedInventory && (
            <>
              {/* Current Stock Display */}
              <div className="rounded-lg border bg-muted/50 p-4">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">{selectedInventory.productName}</p>
                    <p className="text-sm text-muted-foreground">
                      SKU: {selectedInventory.productSku}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm text-muted-foreground">Stock actual</p>
                    <p className="text-2xl font-bold">{selectedInventory.quantity}</p>
                  </div>
                </div>
              </div>

              {/* Quantity Change */}
              <div className="space-y-2">
                <Label>Cambio de cantidad</Label>
                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    size="icon"
                    onClick={() => setQuantityChange((q) => q - 1)}
                  >
                    <Minus className="h-4 w-4" />
                  </Button>
                  <Input
                    type="number"
                    value={quantityChange}
                    onChange={(e) => setQuantityChange(parseInt(e.target.value) || 0)}
                    className="w-24 text-center"
                  />
                  <Button
                    variant="outline"
                    size="icon"
                    onClick={() => setQuantityChange((q) => q + 1)}
                  >
                    <Plus className="h-4 w-4" />
                  </Button>
                </div>
                <p className="text-sm text-muted-foreground">
                  Use valores positivos para aumentar, negativos para disminuir
                </p>
              </div>

              {/* New Quantity Preview */}
              <div className="flex items-center gap-4">
                <span className="text-muted-foreground">Nuevo stock:</span>
                <Badge
                  variant={newQuantity < 0 ? 'destructive' : 'default'}
                  className="text-lg"
                >
                  {newQuantity}
                </Badge>
                {newQuantity < 0 && (
                  <span className="text-sm text-destructive">
                    No se permite stock negativo
                  </span>
                )}
              </div>

              {/* Reason */}
              <div className="space-y-2">
                <Label>Motivo del ajuste *</Label>
                <Textarea
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  placeholder="Describa el motivo del ajuste (máximo 500 caracteres)"
                  maxLength={500}
                  rows={3}
                />
                <p className="text-sm text-muted-foreground">
                  {reason.length}/500 caracteres
                </p>
              </div>

              {/* Submit Button */}
              <Button
                onClick={handleAdjustment}
                disabled={
                  quantityChange === 0 ||
                  !reason.trim() ||
                  newQuantity < 0 ||
                  adjusting
                }
              >
                {adjusting ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Ajustando...
                  </>
                ) : (
                  <>
                    <PenLine className="mr-2 h-4 w-4" />
                    Aplicar Ajuste
                  </>
                )}
              </Button>
            </>
          )}
        </CardContent>
      </Card>

      {/* Confirmation Dialog */}
      <AlertDialog open={showConfirmDialog} onOpenChange={setShowConfirmDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Confirmar Ajuste</AlertDialogTitle>
            <AlertDialogDescription asChild>
              <div className="space-y-2">
                <p>
                  ¿Está seguro de que desea ajustar el stock de{' '}
                  <strong>{selectedInventory?.productName}</strong>?
                </p>
                <p>
                  Stock actual: <strong>{selectedInventory?.quantity}</strong> →
                  Nuevo stock: <strong>{newQuantity}</strong>
                </p>
                <p>
                  Cambio: <strong>{quantityChange > 0 ? '+' : ''}{quantityChange}</strong>
                </p>
                <p>Motivo: {reason}</p>
              </div>
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleConfirmAdjustment}>
              Confirmar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

export default InventoryAdjustPage;

