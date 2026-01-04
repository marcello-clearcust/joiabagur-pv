/**
 * Operator Assignments Dialog Component
 * Modal dialog for managing operator assignments to a point of sale
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
import { userService } from '@/services/user.service';
import type { PointOfSale, UserPointOfSale } from '@/types/point-of-sale.types';
import type { UserListItem } from '@/types/user.types';

interface OperatorAssignmentsDialogProps {
  open: boolean;
  onClose: () => void;
  pointOfSale: PointOfSale | null;
}

export function OperatorAssignmentsDialog({
  open,
  onClose,
  pointOfSale,
}: OperatorAssignmentsDialogProps) {
  const [allUsers, setAllUsers] = useState<UserListItem[]>([]);
  const [assignments, setAssignments] = useState<UserPointOfSale[]>([]);
  const [selectedUserIds, setSelectedUserIds] = useState<Set<string>>(new Set());
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  // Load users and assignments
  useEffect(() => {
    if (open && pointOfSale) {
      loadData();
    }
  }, [open, pointOfSale]);

  const loadData = async () => {
    if (!pointOfSale) return;

    setIsLoading(true);
    try {
      // Load all users and current assignments in parallel
      const [users, currentAssignments] = await Promise.all([
        userService.getUsers(),
        pointOfSaleService.getPointOfSaleOperators(pointOfSale.id),
      ]);

      // Filter to show only operators
      const operators = users.filter((u) => u.role === 'Operator');
      setAllUsers(operators);
      setAssignments(currentAssignments);

      // Set initially selected users (active assignments)
      const assignedUserIds = new Set(
        currentAssignments.filter((a) => a.isActive).map((a) => a.userId)
      );
      setSelectedUserIds(assignedUserIds);
    } catch (error) {
      toast.error('Error al cargar los datos');
      console.error('Failed to load data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  // Toggle user selection
  const handleToggleUser = (userId: string) => {
    setSelectedUserIds((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(userId)) {
        newSet.delete(userId);
      } else {
        newSet.add(userId);
      }
      return newSet;
    });
  };

  // Save assignments
  const handleSave = async () => {
    if (!pointOfSale) return;

    setIsSaving(true);
    try {
      // Determine which users to assign and unassign
      const currentlyAssigned = new Set(
        assignments.filter((a) => a.isActive).map((a) => a.userId)
      );

      const toAssign = Array.from(selectedUserIds).filter((id) => !currentlyAssigned.has(id));
      const toUnassign = Array.from(currentlyAssigned).filter((id) => !selectedUserIds.has(id));

      // Perform assignments and unassignments
      const operations = [
        ...toAssign.map((userId) =>
          pointOfSaleService.assignOperatorToPointOfSale(pointOfSale.id, userId)
        ),
        ...toUnassign.map((userId) =>
          pointOfSaleService.unassignOperatorFromPointOfSale(pointOfSale.id, userId)
        ),
      ];

      await Promise.all(operations);

      toast.success('Asignaciones actualizadas correctamente');
      onClose();
    } catch (error: any) {
      const errorMessage =
        error?.message || 'Error al actualizar las asignaciones';
      toast.error(errorMessage);
      console.error('Failed to save assignments:', error);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Asignar Operadores</DialogTitle>
          <DialogDescription>
            Selecciona los operadores que tendrán acceso a{' '}
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
          ) : allUsers.length === 0 ? (
            // No operators message
            <div className="rounded-lg border border-dashed p-8 text-center">
              <p className="text-sm text-muted-foreground">
                No hay operadores disponibles. Crea usuarios con rol "Operador" primero.
              </p>
            </div>
          ) : (
            // Operators list
            <div className="max-h-[400px] space-y-3 overflow-y-auto pr-2">
              {allUsers.map((user) => {
                const isSelected = selectedUserIds.has(user.id);
                const assignment = assignments.find((a) => a.userId === user.id);

                return (
                  <div
                    key={user.id}
                    className="flex items-center gap-3 rounded-lg border p-3 hover:bg-muted/50"
                  >
                    <Checkbox
                      id={`user-${user.id}`}
                      checked={isSelected}
                      onCheckedChange={() => handleToggleUser(user.id)}
                    />
                    <label
                      htmlFor={`user-${user.id}`}
                      className="flex flex-1 cursor-pointer items-center justify-between"
                    >
                      <div className="space-y-0.5">
                        <p className="text-sm font-medium">
                          {user.firstName} {user.lastName}
                        </p>
                        <p className="text-xs text-muted-foreground">@{user.username}</p>
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
          {!isLoading && allUsers.length > 0 && (
            <div className="rounded-lg bg-muted p-3 text-sm text-muted-foreground">
              {selectedUserIds.size} operador{selectedUserIds.size !== 1 ? 'es' : ''}{' '}
              seleccionado{selectedUserIds.size !== 1 ? 's' : ''}
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
