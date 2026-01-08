/**
 * User Assignments Dialog Component
 * Manage user's point-of-sale assignments with checkboxes
 */

import { useState, useEffect } from 'react';
import { Loader2, Store } from 'lucide-react';
import { userService } from '@/services/user.service';
import { pointOfSaleService } from '@/services/point-of-sale.service';
import { UserListItem } from '@/types/user.types';
import { UserPointOfSale } from '@/types/auth.types';
import { PointOfSale } from '@/types/point-of-sale.types';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { toast } from 'sonner';

interface UserAssignmentsDialogProps {
  open: boolean;
  user: UserListItem | null;
  onClose: (success?: boolean) => void;
}

export function UserAssignmentsDialog({
  open,
  user,
  onClose,
}: UserAssignmentsDialogProps) {
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [pointsOfSale, setPointsOfSale] = useState<PointOfSale[]>([]);
  const [assignments, setAssignments] = useState<UserPointOfSale[]>([]);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [initialSelectedIds, setInitialSelectedIds] = useState<Set<string>>(new Set());

  // Fetch data when dialog opens
  useEffect(() => {
    if (open && user) {
      fetchData();
    }
  }, [open, user]);

  const fetchData = async () => {
    if (!user) return;

    setIsLoading(true);
    try {
      // Fetch all available points of sale (admin sees all, including inactive)
      const allPointsOfSale = await pointOfSaleService.getPointsOfSale();
      
      // Fetch user's current assignments
      const userAssignments = await userService.getUserPointOfSales(user.id);
      setAssignments(userAssignments);

      // Create set of assigned POS IDs
      const assignedIds = new Set(
        userAssignments.filter((a) => a.isActive).map((a) => a.pointOfSaleId)
      );
      setSelectedIds(assignedIds);
      setInitialSelectedIds(new Set(assignedIds));

      // Set all available points of sale
      setPointsOfSale(allPointsOfSale);
    } catch (error) {
      toast.error('Error al cargar los puntos de venta');
      console.error('Failed to fetch assignments:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleToggle = (posId: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(posId)) {
        next.delete(posId);
      } else {
        next.add(posId);
      }
      return next;
    });
  };

  const handleSave = async () => {
    if (!user) return;

    setIsSaving(true);
    try {
      // Determine changes
      const toAssign = [...selectedIds].filter(
        (id) => !initialSelectedIds.has(id)
      );
      const toUnassign = [...initialSelectedIds].filter(
        (id) => !selectedIds.has(id)
      );

      // Apply changes
      for (const posId of toAssign) {
        await userService.assignUserToPointOfSale(user.id, posId);
      }
      for (const posId of toUnassign) {
        await userService.unassignUserFromPointOfSale(user.id, posId);
      }

      toast.success('Asignaciones actualizadas');
      onClose(true);
    } catch (error) {
      toast.error('Error al actualizar las asignaciones');
      console.error('Failed to save assignments:', error);
    } finally {
      setIsSaving(false);
    }
  };

  const hasChanges =
    selectedIds.size !== initialSelectedIds.size ||
    [...selectedIds].some((id) => !initialSelectedIds.has(id));

  // Only show for Operators
  if (user?.role === 'Administrator') {
    return (
      <Dialog open={open} onOpenChange={() => onClose()}>
        <DialogContent className="sm:max-w-[450px]">
          <DialogHeader>
            <DialogTitle>Asignaciones de Puntos de Venta</DialogTitle>
          </DialogHeader>
          <div className="py-6 text-center text-muted-foreground">
            <Store className="size-12 mx-auto mb-3 opacity-50" />
            <p>
              Los administradores tienen acceso a todos los puntos de venta.
            </p>
            <p className="text-sm mt-1">
              No es necesario asignar puntos de venta específicos.
            </p>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => onClose()}>
              Cerrar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Dialog open={open} onOpenChange={() => onClose()}>
      <DialogContent className="sm:max-w-[450px]">
        <DialogHeader>
          <DialogTitle>Asignaciones de Puntos de Venta</DialogTitle>
          <DialogDescription>
            {user
              ? `Gestiona los puntos de venta asignados a ${user.firstName} ${user.lastName}`
              : 'Selecciona los puntos de venta a asignar'}
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          {isLoading ? (
            <div className="flex items-center justify-center py-8">
              <Loader2 className="size-6 animate-spin text-muted-foreground" />
            </div>
          ) : pointsOfSale.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              <Store className="size-12 mx-auto mb-3 opacity-50" />
              <p>No hay puntos de venta disponibles.</p>
              <p className="text-sm mt-1">
                Los puntos de venta se configuran en el módulo de Configuración.
              </p>
            </div>
          ) : (
            <div className="space-y-3 max-h-[300px] overflow-y-auto">
              {pointsOfSale.map((pos) => (
                <label
                  key={pos.id}
                  className="flex items-center gap-3 p-3 rounded-lg border cursor-pointer hover:bg-muted/50 transition-colors"
                >
                  <Checkbox
                    checked={selectedIds.has(pos.id)}
                    onCheckedChange={() => handleToggle(pos.id)}
                    disabled={isSaving}
                  />
                  <div className="flex-1 min-w-0">
                    <div className="font-medium">{pos.name}</div>
                    <div className="text-sm text-muted-foreground">
                      Código: {pos.code}
                    </div>
                  </div>
                </label>
              ))}
            </div>
          )}
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={() => onClose()}
            disabled={isSaving}
          >
            Cancelar
          </Button>
          <Button
            onClick={handleSave}
            disabled={isSaving || !hasChanges || pointsOfSale.length === 0}
          >
            {isSaving ? (
              <>
                <Loader2 className="mr-2 size-4 animate-spin" />
                Guardando...
              </>
            ) : (
              'Guardar'
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
