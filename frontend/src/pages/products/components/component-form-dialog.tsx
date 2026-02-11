/**
 * Component Form Dialog - EP10 HU-EP10-001
 * Create/Edit component dialog with validation
 */

import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { componentService } from '@/services/component.service';
import type { ProductComponent } from '@/types/component.types';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { toast } from 'sonner';

const componentSchema = z.object({
  description: z
    .string()
    .min(1, 'La descripción es obligatoria')
    .max(35, 'Máximo 35 caracteres'),
  costPrice: z
    .union([z.number().min(0, 'Debe ser >= 0'), z.nan(), z.null()])
    .optional()
    .nullable()
    .transform((val) => (val === undefined || Number.isNaN(val) ? null : val)),
  salePrice: z
    .union([z.number().min(0, 'Debe ser >= 0'), z.nan(), z.null()])
    .optional()
    .nullable()
    .transform((val) => (val === undefined || Number.isNaN(val) ? null : val)),
});

type ComponentFormValues = z.infer<typeof componentSchema>;

interface ComponentFormDialogProps {
  open: boolean;
  onClose: (success?: boolean) => void;
  component: ProductComponent | null;
}

export function ComponentFormDialog({ open, onClose, component }: ComponentFormDialogProps) {
  const isEditing = !!component;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ComponentFormValues>({
    resolver: zodResolver(componentSchema),
    defaultValues: {
      description: '',
      costPrice: null,
      salePrice: null,
    },
  });

  useEffect(() => {
    if (open) {
      reset({
        description: component?.description ?? '',
        costPrice: component?.costPrice ?? null,
        salePrice: component?.salePrice ?? null,
      });
    }
  }, [open, component, reset]);

  const onSubmit = async (data: ComponentFormValues) => {
    try {
      if (isEditing) {
        await componentService.updateComponent(component!.id, {
          description: data.description,
          costPrice: data.costPrice,
          salePrice: data.salePrice,
          isActive: component!.isActive,
        });
        toast.success('Componente actualizado correctamente');
      } else {
        await componentService.createComponent({
          description: data.description,
          costPrice: data.costPrice,
          salePrice: data.salePrice,
        });
        toast.success('Componente creado correctamente');
      }
      onClose(true);
    } catch (error: any) {
      toast.error(error?.message || 'Error al guardar el componente');
    }
  };

  return (
    <Dialog open={open} onOpenChange={(open) => onClose(open)}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? 'Editar Componente' : 'Nuevo Componente'}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="description">Descripción *</Label>
            <Input
              id="description"
              placeholder="Ej: Oro 18k"
              maxLength={35}
              {...register('description')}
            />
            {errors.description && (
              <p className="text-sm text-destructive">{errors.description.message}</p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="costPrice">Precio Coste (€)</Label>
              <Input
                id="costPrice"
                type="number"
                step="0.0001"
                min="0"
                placeholder="Opcional"
                {...register('costPrice', { valueAsNumber: true })}
              />
              {errors.costPrice && (
                <p className="text-sm text-destructive">{errors.costPrice.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="salePrice">Precio Venta (€)</Label>
              <Input
                id="salePrice"
                type="number"
                step="0.0001"
                min="0"
                placeholder="Opcional"
                {...register('salePrice', { valueAsNumber: true })}
              />
              {errors.salePrice && (
                <p className="text-sm text-destructive">{errors.salePrice.message}</p>
              )}
            </div>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onClose()}>
              Cancelar
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? 'Guardando...' : isEditing ? 'Actualizar' : 'Crear'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
