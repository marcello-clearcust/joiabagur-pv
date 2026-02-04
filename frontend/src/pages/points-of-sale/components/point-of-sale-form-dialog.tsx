/**
 * Point of Sale Form Dialog Component
 * Modal dialog for creating and editing points of sale
 */

import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
  FormDescription,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Switch } from '@/components/ui/switch';
import { toast } from 'sonner';
import { pointOfSaleService } from '@/services/point-of-sale.service';
import type { PointOfSale } from '@/types/point-of-sale.types';

// Validation schema
const pointOfSaleSchema = z.object({
  name: z
    .string()
    .min(2, 'El nombre debe tener al menos 2 caracteres')
    .max(100, 'El nombre no puede exceder 100 caracteres'),
  code: z
    .string()
    .min(2, 'El código debe tener al menos 2 caracteres')
    .max(50, 'El código no puede exceder 50 caracteres')
    .regex(/^[A-Z0-9_-]+$/i, 'El código debe ser alfanumérico (puede incluir _ y -)'),
  address: z.string().max(200, 'La dirección no puede exceder 200 caracteres').optional(),
  phone: z.string().max(20, 'El teléfono no puede exceder 20 caracteres').optional(),
  email: z.string().email('Email inválido').optional().or(z.literal('')),
  isActive: z.boolean(),
});

type PointOfSaleFormData = z.infer<typeof pointOfSaleSchema>;

interface PointOfSaleFormDialogProps {
  open: boolean;
  onClose: (success?: boolean) => void;
  pointOfSale: PointOfSale | null;
}

export function PointOfSaleFormDialog({
  open,
  onClose,
  pointOfSale,
}: PointOfSaleFormDialogProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const isEditMode = !!pointOfSale;

  const form = useForm<PointOfSaleFormData>({
    resolver: zodResolver(pointOfSaleSchema),
    defaultValues: {
      name: '',
      code: '',
      address: '',
      phone: '',
      email: '',
      isActive: true,
    },
  });

  // Load point of sale data when editing
  useEffect(() => {
    if (pointOfSale) {
      form.reset({
        name: pointOfSale.name,
        code: pointOfSale.code,
        address: pointOfSale.address || '',
        phone: pointOfSale.phone || '',
        email: pointOfSale.email || '',
        isActive: pointOfSale.isActive,
      });
    } else {
      form.reset({
        name: '',
        code: '',
        address: '',
        phone: '',
        email: '',
        isActive: true,
      });
    }
  }, [pointOfSale, form]);

  // Handle form submission
  const onSubmit = async (data: PointOfSaleFormData) => {
    setIsSubmitting(true);
    try {
      if (isEditMode) {
        // Update existing point of sale (code is immutable, not sent)
        await pointOfSaleService.updatePointOfSale(pointOfSale.id, {
          name: data.name,
          address: data.address || undefined,
          phone: data.phone || undefined,
          email: data.email || undefined,
          isActive: data.isActive,
        });
        toast.success('Punto de venta actualizado correctamente');
      } else {
        // Create new point of sale
        await pointOfSaleService.createPointOfSale({
          name: data.name,
          code: data.code,
          address: data.address || undefined,
          phone: data.phone || undefined,
          email: data.email || undefined,
        });
        toast.success('Punto de venta creado correctamente');
      }
      onClose(true);
    } catch (error: any) {
      const errorMessage =
        error?.message ||
        error?.response?.data?.errors?.[0] ||
        'Error al guardar el punto de venta';
      toast.error(errorMessage);
      console.error('Failed to save point of sale:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle dialog close
  const handleClose = () => {
    if (!isSubmitting) {
      form.reset();
      onClose();
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>
            {isEditMode ? 'Editar Punto de Venta' : 'Crear Punto de Venta'}
          </DialogTitle>
          <DialogDescription>
            {isEditMode
              ? 'Actualiza la información del punto de venta.'
              : 'Completa la información para crear un nuevo punto de venta.'}
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            {/* Name Field */}
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Nombre *</FormLabel>
                  <FormControl>
                    <Input placeholder="Tienda Centro, Hotel Plaza, etc." {...field} />
                  </FormControl>
                  <FormDescription>Nombre descriptivo del punto de venta</FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Code Field */}
            <FormField
              control={form.control}
              name="code"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Código *</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="CENTRO-001, HOTEL-PLZ, etc."
                      {...field}
                      className="uppercase"
                      onChange={(e) => field.onChange(e.target.value.toUpperCase())}
                      disabled={isEditMode}
                    />
                  </FormControl>
                  <FormDescription>
                    {isEditMode
                      ? 'El código no puede modificarse después de la creación'
                      : 'Código único del punto de venta (alfanumérico, _ y - permitidos)'}
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Address Field */}
            <FormField
              control={form.control}
              name="address"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Dirección</FormLabel>
                  <FormControl>
                    <Input placeholder="Calle Principal 123, Madrid" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid grid-cols-2 gap-4">
              {/* Phone Field */}
              <FormField
                control={form.control}
                name="phone"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Teléfono</FormLabel>
                    <FormControl>
                      <Input placeholder="+34 600 123 456" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Email Field */}
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Email</FormLabel>
                    <FormControl>
                      <Input placeholder="contacto@ejemplo.com" type="email" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* IsActive Field - Only show in edit mode */}
            {isEditMode && (
              <FormField
                control={form.control}
                name="isActive"
                render={({ field }) => (
                  <FormItem className="flex items-center justify-between rounded-lg border p-3">
                    <div className="space-y-0.5">
                      <FormLabel>Punto de Venta Activo</FormLabel>
                      <FormDescription>
                        Los puntos de venta inactivos no aparecerán en las listas para nuevas operaciones
                      </FormDescription>
                    </div>
                    <FormControl>
                      <Switch
                        checked={field.value}
                        onCheckedChange={field.onChange}
                        disabled={isSubmitting}
                      />
                    </FormControl>
                  </FormItem>
                )}
              />
            )}

            <DialogFooter>
              <Button type="button" variant="outline" onClick={handleClose} disabled={isSubmitting}>
                Cancelar
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Guardando...' : isEditMode ? 'Actualizar' : 'Crear'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
