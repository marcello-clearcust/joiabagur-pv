/**
 * Payment Method Assignments Dialog Component
 * Modal dialog for managing payment method assignments to a point of sale
 */

import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { toast } from 'sonner';
import { pointOfSaleService } from '@/services/point-of-sale.service';
import { paymentMethodService } from '@/services/payment-method.service';
import type { PointOfSale } from '@/types/point-of-sale.types';
import type { PaymentMethod, PointOfSalePaymentMethod } from '@/types/payment-method.types';

interface PaymentMethodAssignmentsDialogProps {
  open: boolean;
  onClose: () => void;
  pointOfSale: PointOfSale | null;
}

export function PaymentMethodAssignmentsDialog({
  open,
  onClose,
  pointOfSale,
}: PaymentMethodAssignmentsDialogProps) {
  const [allPaymentMethods, setAllPaymentMethods] = useState<PaymentMethod[]>([]);
  const [assignments, setAssignments] = useState<PointOfSalePaymentMethod[]>([]);
  const [selectedMethodIds, setSelectedMethodIds] = useState<Set<string>>(new Set());
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  // Load payment methods and assignments
  useEffect(() => {
    if (open && pointOfSale) {
      loadData();
    }
  }, [open, pointOfSale]);

  const loadData = async () => {
    if (!pointOfSale) return;

    setIsLoading(true);
    try {
      // Load all payment methods and current assignments in parallel
      const [methods, currentAssignments] = await Promise.all([
        paymentMethodService.getPaymentMethods(false), // Only active payment methods
        pointOfSaleService.getPointOfSalePaymentMethods(pointOfSale.id),
      ]);

      setAllPaymentMethods(methods);
      setAssignments(currentAssignments);

      // Set initially selected payment methods (active assignments)
      const assignedMethodIds = new Set(
        currentAssignments.filter((a) => a.isActive).map((a) => a.paymentMethodId)
      );
      setSelectedMethodIds(assignedMethodIds);
    } catch (error) {
      toast.error('Error al cargar los datos');
      console.error('Failed to load data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  // Toggle payment method selection
  const handleToggleMethod = (methodId: string) => {
    setSelectedMethodIds((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(methodId)) {
        newSet.delete(methodId);
      } else {
        newSet.add(methodId);
      }
      return newSet;
    });
  };

  // Save assignments
  const handleSave = async () => {
    if (!pointOfSale) return;

    setIsSaving(true);
    try {
      // Determine which payment methods to assign and unassign
      const currentlyAssigned = new Set(
        assignments.filter((a) => a.isActive).map((a) => a.paymentMethodId)
      );

      const toAssign = Array.from(selectedMethodIds).filter(
        (id) => !currentlyAssigned.has(id)
      );
      const toUnassign = Array.from(currentlyAssigned).filter(
        (id) => !selectedMethodIds.has(id)
      );

      // Perform assignments and unassignments
      const operations = [
        ...toAssign.map((methodId) =>
          pointOfSaleService.assignPaymentMethodToPointOfSale(pointOfSale.id, methodId)
        ),
        ...toUnassign.map((methodId) =>
          pointOfSaleService.unassignPaymentMethodFromPointOfSale(pointOfSale.id, methodId)
        ),
      ];

      await Promise.all(operations);

      toast.success('Métodos de pago actualizados correctamente');
      onClose();
    } catch (error: any) {
      const errorMessage =
        error?.message || 'Error al actualizar los métodos de pago';
      toast.error(errorMessage);
      console.error('Failed to save payment method assignments:', error);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Asignar Métodos de Pago</DialogTitle>
          <DialogDescription>
            Selecciona los métodos de pago disponibles en{' '}
            <span className="font-medium">{pointOfSale?.name}</span>
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          {isLoading ? (
            // Loading skeleton
            <div className="space-y-3">
              {[1, 2, 3, 4].map((i) => (
                <div key={i} className="flex items-center gap-3">
                  <Skeleton className="h-4 w-4" />
                  <Skeleton className="h-4 flex-1" />
                </div>
              ))}
            </div>
          ) : allPaymentMethods.length === 0 ? (
            // No payment methods message
            <div className="rounded-lg border border-dashed p-8 text-center">
              <p className="text-sm text-muted-foreground">
                No hay métodos de pago disponibles. Verifica que existan métodos de pago activos.
              </p>
            </div>
          ) : (
            // Payment methods list
            <div className="max-h-[400px] space-y-3 overflow-y-auto pr-2">
              {allPaymentMethods.map((method) => {
                const isSelected = selectedMethodIds.has(method.id);
                const assignment = assignments.find((a) => a.paymentMethodId === method.id);

                return (
                  <div
                    key={method.id}
                    className="flex items-center gap-3 rounded-lg border p-3 hover:bg-muted/50"
                  >
                    <Checkbox
                      id={`method-${method.id}`}
                      checked={isSelected}
                      onCheckedChange={() => handleToggleMethod(method.id)}
                    />
                    <label
                      htmlFor={`method-${method.id}`}
                      className="flex flex-1 cursor-pointer items-center justify-between"
                    >
                      <div className="space-y-0.5">
                        <div className="flex items-center gap-2">
                          <p className="text-sm font-medium">{method.name}</p>
                          <Badge variant="outline" className="text-xs font-mono">
                            {method.code}
                          </Badge>
                        </div>
                        {method.description && (
                          <p className="text-xs text-muted-foreground">{method.description}</p>
                        )}
                      </div>
                      {assignment && !assignment.isActive && (
                        <Badge variant="outline" className="text-xs">
                          Previamente asignado
                        </Badge>
                      )}
                    </label>
                  </div>
                );
              })}
            </div>
          )}

          {/* Selection summary */}
          {!isLoading && allPaymentMethods.length > 0 && (
            <div className="rounded-lg bg-muted p-3 text-sm text-muted-foreground">
              {selectedMethodIds.size} método{selectedMethodIds.size !== 1 ? 's' : ''} de pago{' '}
              seleccionado{selectedMethodIds.size !== 1 ? 's' : ''}
            </div>
          )}
        </div>

        <div className="flex justify-end gap-2">
          <Button variant="outline" onClick={onClose} disabled={isSaving}>
            Cancelar
          </Button>
          <Button onClick={handleSave} disabled={isSaving || isLoading}>
            {isSaving ? 'Guardando...' : 'Guardar Asignaciones'}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
