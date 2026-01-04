/**
 * Payment Method Form Dialog Component
 * Modal dialog for creating and editing payment methods
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
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import { paymentMethodService } from '@/services/payment-method.service';
import type { PaymentMethod } from '@/types/payment-method.types';

// Validation schema
const paymentMethodSchema = z.object({
  code: z
    .string()
    .min(2, 'El código debe tener al menos 2 caracteres')
    .max(20, 'El código no puede exceder 20 caracteres')
    .regex(/^[A-Z0-9_]+$/, 'El código debe ser mayúsculas, números y guiones bajos'),
  name: z
    .string()
    .min(2, 'El nombre debe tener al menos 2 caracteres')
    .max(100, 'El nombre no puede exceder 100 caracteres'),
  description: z.string().max(500, 'La descripción no puede exceder 500 caracteres').optional(),
});

type PaymentMethodFormData = z.infer<typeof paymentMethodSchema>;

interface PaymentMethodFormDialogProps {
  open: boolean;
  onClose: (success?: boolean) => void;
  paymentMethod: PaymentMethod | null;
}

export function PaymentMethodFormDialog({
  open,
  onClose,
  paymentMethod,
}: PaymentMethodFormDialogProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const isEditMode = !!paymentMethod;

  const form = useForm<PaymentMethodFormData>({
    resolver: zodResolver(paymentMethodSchema),
    defaultValues: {
      code: '',
      name: '',
      description: '',
    },
  });

  // Load payment method data when editing
  useEffect(() => {
    if (paymentMethod) {
      form.reset({
        code: paymentMethod.code,
        name: paymentMethod.name,
        description: paymentMethod.description || '',
      });
    } else {
      form.reset({
        code: '',
        name: '',
        description: '',
      });
    }
  }, [paymentMethod, form]);

  // Handle form submission
  const onSubmit = async (data: PaymentMethodFormData) => {
    setIsSubmitting(true);
    try {
      if (isEditMode) {
        // Update existing payment method
        await paymentMethodService.updatePaymentMethod(paymentMethod.id, {
          name: data.name,
          description: data.description || '',
        });
        toast.success('Método de pago actualizado correctamente');
      } else {
        // Create new payment method
        await paymentMethodService.createPaymentMethod({
          code: data.code,
          name: data.name,
          description: data.description || '',
        });
        toast.success('Método de pago creado correctamente');
      }
      onClose(true);
    } catch (error: any) {
      const errorMessage =
        error?.message ||
        error?.response?.data?.errors?.[0] ||
        'Error al guardar el método de pago';
      toast.error(errorMessage);
      console.error('Failed to save payment method:', error);
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
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>
            {isEditMode ? 'Editar Método de Pago' : 'Crear Método de Pago'}
          </DialogTitle>
          <DialogDescription>
            {isEditMode
              ? 'Actualiza la información del método de pago. El código no puede modificarse.'
              : 'Completa la información para crear un nuevo método de pago.'}
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            {/* Code Field */}
            <FormField
              control={form.control}
              name="code"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Código *</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="CASH, CARD_POS, etc."
                      {...field}
                      disabled={isEditMode}
                      className="uppercase"
                      onChange={(e) => field.onChange(e.target.value.toUpperCase())}
                    />
                  </FormControl>
                  <FormDescription>
                    Identificador único en mayúsculas (solo letras, números y guiones bajos)
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Name Field */}
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Nombre *</FormLabel>
                  <FormControl>
                    <Input placeholder="Efectivo, Tarjeta, etc." {...field} />
                  </FormControl>
                  <FormDescription>Nombre descriptivo del método de pago</FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Description Field */}
            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Descripción</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Descripción opcional del método de pago"
                      className="resize-none"
                      rows={3}
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

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
