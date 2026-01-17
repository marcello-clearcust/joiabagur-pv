/**
 * Manual Sales Registration Page (EP3)
 * Allows operators to register sales by searching for products
 */
import { useState, useEffect, useCallback } from 'react';
import { useNavigate, Link, useLocation } from 'react-router-dom';
import { ArrowLeft, Search, ShoppingCart, AlertTriangle, CheckCircle2 } from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
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
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Skeleton } from '@/components/ui/skeleton';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';

import { useAuth } from '@/providers/auth-provider';
import { getImageUrl } from '@/lib/image-url';
import { salesService } from '@/services/sales.service';
import { productService } from '@/services/product.service';
import { pointOfSaleService } from '@/services/point-of-sale.service';
import { paymentMethodService } from '@/services/payment-method.service';
import { inventoryService } from '@/services/inventory.service';
import { ROUTES } from '@/routing/routes';
import type { Product } from '@/types/product.types';
import type { PointOfSale } from '@/types/point-of-sale.types';
import type { PaymentMethod } from '@/types/payment-method.types';

// State passed from image recognition page
interface LocationState {
  productId?: string;
  photoDataUrl?: string;
}

export function ManualSalesPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useAuth();
  
  // Get pre-selected product from image recognition
  const locationState = location.state as LocationState | null;

  // State
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [pointsOfSale, setPointsOfSale] = useState<PointOfSale[]>([]);
  const [paymentMethods, setPaymentMethods] = useState<PaymentMethod[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [productSearch, setProductSearch] = useState('');
  const [searchResults, setSearchResults] = useState<Product[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);
  
  // Form state
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [selectedPosId, setSelectedPosId] = useState<string>('');
  const [selectedPaymentMethodId, setSelectedPaymentMethodId] = useState<string>('');
  const [quantity, setQuantity] = useState(1);
  const [notes, setNotes] = useState('');
  const [availableStock, setAvailableStock] = useState<number | null>(null);
  const [stockLoading, setStockLoading] = useState(false);

  // Confirmation dialog
  const [confirmDialogOpen, setConfirmDialogOpen] = useState(false);

  // Load initial data
  useEffect(() => {
    const loadData = async () => {
      try {
        const [posData, productsData] = await Promise.all([
          pointOfSaleService.getPointsOfSale(),
          productService.getProducts(),
        ]);
        setPointsOfSale(posData);
        setProducts(productsData || []);
        if (posData.length > 0) {
          setSelectedPosId(posData[0].id);
        }
        
        // If a productId was passed from image recognition, pre-select that product
        // Note: productId could be empty string if image recognition enrichment failed
        if (locationState?.productId && locationState.productId.trim() !== '') {
          // First try to find in already loaded products
          let preSelectedProduct = productsData?.find(
            (p) => p.id === locationState.productId
          );
          
          // If not found in local list, fetch directly from API
          // This handles cases where the product list loading is incomplete
          // or the admin is accessing a product not in the initial page
          if (!preSelectedProduct) {
            try {
              preSelectedProduct = await productService.getProduct(locationState.productId);
              // Add to local list for future searches
              if (preSelectedProduct) {
                setProducts(prev => [...prev, preSelectedProduct!]);
              }
            } catch (fetchError) {
              console.warn('Could not fetch product by ID:', fetchError);
            }
          }
          
          if (preSelectedProduct) {
            setSelectedProduct(preSelectedProduct);
            setProductSearch(preSelectedProduct.sku);
            toast.success(`Producto "${preSelectedProduct.name}" seleccionado automáticamente`);
          } else {
            // Product truly not found or not accessible
            toast.warning('El producto seleccionado no está disponible en tus puntos de venta');
          }
        }
      } catch (error) {
        toast.error('Error al cargar datos');
        console.error(error);
      } finally {
        setLoading(false);
      }
    };
    loadData();
  }, [locationState?.productId]);

  // Load payment methods when POS changes
  useEffect(() => {
    const loadPaymentMethods = async () => {
      if (!selectedPosId) return;
      
      let methods: PaymentMethod[] = [];
      
      try {
        // First try to get payment methods assigned to this point of sale
        const posPaymentMethods = await paymentMethodService.getPointOfSalePaymentMethods(selectedPosId);
        // Filter to active assignments only and extract the payment method
        methods = posPaymentMethods
          .filter(pm => pm.isActive && pm.paymentMethod)
          .map(pm => pm.paymentMethod);
      } catch (error) {
        console.error('Error loading payment methods for POS:', error);
      }
      
      // If no POS-specific methods, fall back to all payment methods
      if (methods.length === 0) {
        try {
          const allMethods = await paymentMethodService.getPaymentMethods();
          methods = allMethods.filter(m => m.isActive);
        } catch (fallbackError) {
          console.error('Error loading fallback payment methods:', fallbackError);
        }
      }
      
      setPaymentMethods(methods);
      if (methods.length > 0) {
        setSelectedPaymentMethodId(methods[0].id);
      } else {
        setSelectedPaymentMethodId('');
      }
    };
    loadPaymentMethods();
  }, [selectedPosId]);

  // Check stock when product and POS are selected
  useEffect(() => {
    const checkStock = async () => {
      if (!selectedProduct || !selectedPosId) {
        setAvailableStock(null);
        return;
      }
      
      setStockLoading(true);
      try {
        // Use getProductStockBreakdown to get stock across all POSes for this product
        // This is more reliable than getStock which returns paginated results
        const stockBreakdown = await inventoryService.getProductStockBreakdown(selectedProduct.id);
        
        if (stockBreakdown) {
          // Find stock for the selected POS
          const posStock = stockBreakdown.breakdown.find(
            (b) => b.pointOfSaleId === selectedPosId
          );
          setAvailableStock(posStock?.quantity ?? 0);
        } else {
          // Product has no inventory assignments
          setAvailableStock(0);
        }
      } catch (error) {
        console.error('Error checking stock:', error);
        setAvailableStock(null);
      } finally {
        setStockLoading(false);
      }
    };
    checkStock();
  }, [selectedProduct, selectedPosId]);

  // Product search
  const handleProductSearch = useCallback((query: string) => {
    setProductSearch(query);
    if (!query.trim()) {
      setSearchResults([]);
      return;
    }
    
    setSearchLoading(true);
    const filtered = products.filter(
      (p) =>
        p.sku.toLowerCase().includes(query.toLowerCase()) ||
        p.name.toLowerCase().includes(query.toLowerCase())
    );
    setSearchResults(filtered.slice(0, 10));
    setSearchLoading(false);
  }, [products]);

  // Select product from search
  const handleSelectProduct = (product: Product) => {
    setSelectedProduct(product);
    setProductSearch(product.sku);
    setSearchResults([]);
    setQuantity(1);
  };

  // Calculate total
  const total = selectedProduct ? selectedProduct.price * quantity : 0;

  // Validation
  const isFormValid = 
    selectedProduct && 
    selectedPosId && 
    selectedPaymentMethodId && 
    quantity > 0 &&
    availableStock !== null &&
    availableStock >= quantity;

  // Handle sale submission
  const handleSubmitSale = async () => {
    if (!selectedProduct || !isFormValid) return;

    setSubmitting(true);
    try {
      const result = await salesService.createSale({
        productId: selectedProduct.id,
        pointOfSaleId: selectedPosId,
        paymentMethodId: selectedPaymentMethodId,
        quantity,
        notes: notes || undefined,
      });

      toast.success('Venta registrada exitosamente');
      
      if (result.isLowStock) {
        toast.warning(`⚠️ Stock bajo: quedan ${result.remainingStock} unidades`, {
          duration: 5000,
        });
      }

      setConfirmDialogOpen(false);
      navigate(ROUTES.SALES.ROOT);
    } catch (error: unknown) {
      const errorMessage = (error as { message?: string }).message || 'Error al registrar la venta';
      toast.error(errorMessage);
    } finally {
      setSubmitting(false);
    }
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
          <Link to={ROUTES.SALES.ROOT}>
            <ArrowLeft className="h-5 w-5" />
          </Link>
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Nueva Venta</h1>
          <p className="text-muted-foreground">
            Registro manual de venta
          </p>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Product Selection */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-lg">
              <Search className="h-5 w-5" />
              Seleccionar Producto
            </CardTitle>
            <CardDescription>
              Busca por SKU o nombre del producto
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {/* Search Input */}
            <div className="relative">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Buscar producto..."
                className="pl-10"
                value={productSearch}
                onChange={(e) => handleProductSearch(e.target.value)}
              />
              
              {/* Search Results Dropdown */}
              {searchResults.length > 0 && (
                <div className="absolute z-10 mt-1 w-full rounded-md border bg-background shadow-lg">
                  {searchResults.map((product) => {
                    const primaryPhoto = product.photos?.find((p) => p.isPrimary) || product.photos?.[0];
                    const photoUrl = getImageUrl(primaryPhoto?.url);
                    return (
                      <button
                        key={product.id}
                        type="button"
                        className="flex w-full items-center gap-3 px-4 py-3 text-left hover:bg-muted"
                        onClick={() => handleSelectProduct(product)}
                      >
                        {photoUrl ? (
                          <img 
                            src={photoUrl} 
                            alt={product.name}
                            className="h-10 w-10 rounded-md object-cover flex-shrink-0"
                          />
                        ) : (
                          <div className="flex h-10 w-10 items-center justify-center rounded-md bg-muted flex-shrink-0">
                            <ShoppingCart className="h-5 w-5 text-muted-foreground" />
                          </div>
                        )}
                        <div className="flex-1 min-w-0">
                          <div className="font-medium truncate">{product.name}</div>
                          <div className="text-sm text-muted-foreground">
                            SKU: {product.sku} | €{product.price.toFixed(2)}
                          </div>
                        </div>
                      </button>
                    );
                  })}
                </div>
              )}
            </div>

            {/* Selected Product */}
            {selectedProduct && (
              <div className="rounded-lg border p-4">
                <div className="flex items-start gap-4">
                  {/* Product Photo */}
                  {(() => {
                    const primaryPhoto = selectedProduct.photos?.find((p) => p.isPrimary) || selectedProduct.photos?.[0];
                    const photoUrl = getImageUrl(primaryPhoto?.url);
                    return photoUrl ? (
                      <img 
                        src={photoUrl} 
                        alt={selectedProduct.name}
                        className="h-20 w-20 rounded-lg object-cover flex-shrink-0"
                      />
                    ) : (
                      <div className="flex h-20 w-20 items-center justify-center rounded-lg bg-muted flex-shrink-0">
                        <ShoppingCart className="h-8 w-8 text-muted-foreground" />
                      </div>
                    );
                  })()}
                  <div className="flex-1 flex items-start justify-between">
                    <div>
                      <h3 className="font-semibold">{selectedProduct.name}</h3>
                      <p className="text-sm text-muted-foreground">SKU: {selectedProduct.sku}</p>
                      <p className="mt-1 text-lg font-bold text-primary">
                        €{selectedProduct.price.toFixed(2)}
                      </p>
                    </div>
                    <Badge variant={selectedProduct.isActive ? 'primary' : 'secondary'}>
                      {selectedProduct.isActive ? 'Activo' : 'Inactivo'}
                    </Badge>
                  </div>
                </div>

                {/* Stock Info */}
                <div className="mt-4 flex items-center gap-2">
                  {stockLoading ? (
                    <Skeleton className="h-5 w-32" />
                  ) : availableStock !== null ? (
                    <>
                      <span className="text-sm text-muted-foreground">
                        Stock disponible:
                      </span>
                      <Badge variant={availableStock > 0 ? 'primary' : 'destructive'}>
                        {availableStock} unidades
                      </Badge>
                    </>
                  ) : null}
                </div>

                {/* Quantity Input */}
                <div className="mt-4 space-y-2">
                  <Label htmlFor="quantity">Cantidad</Label>
                  <Input
                    id="quantity"
                    type="number"
                    min={1}
                    max={availableStock || 1}
                    value={quantity}
                    onChange={(e) => setQuantity(Math.max(1, parseInt(e.target.value) || 1))}
                    className="w-24"
                  />
                  {availableStock !== null && quantity > availableStock && (
                    <p className="text-sm text-destructive">
                      Stock insuficiente
                    </p>
                  )}
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Sale Details */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-lg">
              <ShoppingCart className="h-5 w-5" />
              Detalles de Venta
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {/* Point of Sale */}
            <div className="space-y-2">
              <Label>Punto de Venta</Label>
              <Select value={selectedPosId} onValueChange={setSelectedPosId}>
                <SelectTrigger>
                  <SelectValue placeholder="Seleccionar..." />
                </SelectTrigger>
                <SelectContent>
                  {pointsOfSale.map((pos) => (
                    <SelectItem key={pos.id} value={pos.id}>
                      {pos.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Payment Method */}
            <div className="space-y-2">
              <Label>Método de Pago</Label>
              <Select 
                value={selectedPaymentMethodId} 
                onValueChange={setSelectedPaymentMethodId}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Seleccionar..." />
                </SelectTrigger>
                <SelectContent>
                  {paymentMethods.map((method) => (
                    <SelectItem key={method.id} value={method.id}>
                      {method.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Notes */}
            <div className="space-y-2">
              <Label htmlFor="notes">Notas (opcional)</Label>
              <Textarea
                id="notes"
                placeholder="Añadir notas..."
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                maxLength={500}
              />
            </div>

            {/* Total */}
            <div className="rounded-lg bg-muted p-4">
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Subtotal</span>
                <span>€{(selectedProduct?.price || 0).toFixed(2)}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-muted-foreground">Cantidad</span>
                <span>×{quantity}</span>
              </div>
              <div className="mt-2 flex items-center justify-between border-t pt-2">
                <span className="font-semibold">Total</span>
                <span className="text-xl font-bold text-primary">
                  €{total.toFixed(2)}
                </span>
              </div>
            </div>

            {/* Submit Button */}
            <Button 
              className="w-full" 
              size="lg"
              disabled={!isFormValid}
              onClick={() => setConfirmDialogOpen(true)}
            >
              <ShoppingCart className="mr-2 h-5 w-5" />
              Registrar Venta
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* Confirmation Dialog */}
      <Dialog open={confirmDialogOpen} onOpenChange={setConfirmDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirmar Venta</DialogTitle>
            <DialogDescription>
              ¿Estás seguro de que deseas registrar esta venta?
            </DialogDescription>
          </DialogHeader>
          
          {selectedProduct && (
            <div className="space-y-4">
              <Alert>
                <CheckCircle2 className="h-4 w-4" />
                <AlertTitle>Resumen de Venta</AlertTitle>
                <AlertDescription>
                  <div className="mt-2 space-y-1 text-sm">
                    <p><strong>Producto:</strong> {selectedProduct.name}</p>
                    <p><strong>SKU:</strong> {selectedProduct.sku}</p>
                    <p><strong>Cantidad:</strong> {quantity}</p>
                    <p><strong>Total:</strong> €{total.toFixed(2)}</p>
                  </div>
                </AlertDescription>
              </Alert>

              {availableStock !== null && availableStock - quantity <= 5 && (
                <Alert variant="destructive">
                  <AlertTriangle className="h-4 w-4" />
                  <AlertTitle>Advertencia de Stock</AlertTitle>
                  <AlertDescription>
                    Después de esta venta quedarán solo {availableStock - quantity} unidades.
                  </AlertDescription>
                </Alert>
              )}
            </div>
          )}

          <DialogFooter>
            <Button 
              variant="outline" 
              onClick={() => setConfirmDialogOpen(false)}
              disabled={submitting}
            >
              Cancelar
            </Button>
            <Button 
              onClick={handleSubmitSale}
              disabled={submitting}
            >
              {submitting ? 'Procesando...' : 'Confirmar Venta'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default ManualSalesPage;
