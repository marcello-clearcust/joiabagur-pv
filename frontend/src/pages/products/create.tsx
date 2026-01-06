/**
 * Product Create Page - EP1 HU-EP1-002
 * Form for creating individual products manually
 */

import { useState, useEffect, useCallback } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Loader2, CheckCircle2, Plus, Package } from 'lucide-react';
import { productService } from '@/services/product.service';
import { Collection } from '@/types/product.types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { toast } from 'sonner';
import { ROUTES } from '@/routing/routes';
import { useDebouncedCallback } from '@/hooks/use-debounced-callback';

// Validation schema
const createProductSchema = z.object({
  sku: z
    .string()
    .min(1, 'El SKU es requerido')
    .max(50, 'El SKU no puede exceder 50 caracteres')
    .regex(/^[A-Za-z0-9-_]+$/, 'El SKU solo puede contener letras, números, guiones y guiones bajos'),
  name: z
    .string()
    .min(1, 'El nombre es requerido')
    .max(200, 'El nombre no puede exceder 200 caracteres'),
  description: z
    .string()
    .max(1000, 'La descripción no puede exceder 1000 caracteres')
    .optional()
    .or(z.literal('')),
  price: z
    .string()
    .min(1, 'El precio es requerido')
    .refine((val) => {
      const num = parseFloat(val);
      return !isNaN(num) && num > 0;
    }, 'El precio debe ser mayor que 0'),
  collectionId: z.string().optional(),
});

type CreateProductFormValues = z.infer<typeof createProductSchema>;

// No collection value for the select
const NO_COLLECTION_VALUE = '__none__';

export function ProductCreatePage() {
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isCheckingSku, setIsCheckingSku] = useState(false);
  const [skuExists, setSkuExists] = useState(false);
  const [collections, setCollections] = useState<Collection[]>([]);
  const [isLoadingCollections, setIsLoadingCollections] = useState(true);
  const [showSuccess, setShowSuccess] = useState(false);
  const [createdProductName, setCreatedProductName] = useState('');

  const form = useForm<CreateProductFormValues>({
    resolver: zodResolver(createProductSchema),
    defaultValues: {
      sku: '',
      name: '',
      description: '',
      price: '',
      collectionId: undefined,
    },
    mode: 'onTouched',
    reValidateMode: 'onChange',
  });

  // Load collections on mount
  useEffect(() => {
    const loadCollections = async () => {
      try {
        const data = await productService.getCollections();
        setCollections(data);
      } catch (error) {
        console.error('Failed to load collections:', error);
        // Non-blocking error - collections are optional
      } finally {
        setIsLoadingCollections(false);
      }
    };
    loadCollections();
  }, []);

  // Debounced SKU check
  const checkSkuUniqueness = useDebouncedCallback(
    useCallback(async (sku: string) => {
      if (!sku || sku.length < 1) {
        setSkuExists(false);
        setIsCheckingSku(false);
        return;
      }

      setIsCheckingSku(true);
      try {
        const exists = await productService.checkSkuExists(sku);
        setSkuExists(exists);
        if (exists) {
          form.setError('sku', {
            type: 'manual',
            message: 'El SKU ya está en uso',
          });
        } else {
          // Clear manual error if SKU is now available
          const currentError = form.formState.errors.sku;
          if (currentError?.type === 'manual') {
            form.clearErrors('sku');
          }
        }
      } catch {
        // If check fails, allow submission (server will validate)
        setSkuExists(false);
      } finally {
        setIsCheckingSku(false);
      }
    }, [form]),
    500
  );

  // Watch SKU field for uniqueness validation
  const skuValue = form.watch('sku');
  useEffect(() => {
    if (skuValue) {
      checkSkuUniqueness(skuValue);
    } else {
      setSkuExists(false);
    }
  }, [skuValue, checkSkuUniqueness]);

  // Handle form submission
  const onSubmit = async (data: CreateProductFormValues) => {
    // Double-check SKU uniqueness before submit
    if (skuExists) {
      form.setError('sku', {
        type: 'manual',
        message: 'El SKU ya está en uso',
      });
      return;
    }

    setIsSubmitting(true);
    try {
      await productService.createProduct({
        sku: data.sku,
        name: data.name,
        description: data.description || undefined,
        price: parseFloat(data.price),
        collectionId: data.collectionId === NO_COLLECTION_VALUE ? undefined : data.collectionId,
      });

      setCreatedProductName(data.name);
      setShowSuccess(true);
      toast.success('Producto creado exitosamente');
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Error al crear el producto';
      
      // Check if it's a SKU conflict error
      if (message.toLowerCase().includes('sku') || message.toLowerCase().includes('already exists')) {
        form.setError('sku', {
          type: 'manual',
          message: 'El SKU ya está en uso',
        });
        setSkuExists(true);
      } else {
        toast.error(message);
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle creating another product
  const handleCreateAnother = () => {
    form.reset();
    setShowSuccess(false);
    setSkuExists(false);
    setCreatedProductName('');
  };

  // Check if form can be submitted
  const canSubmit = !isSubmitting && !isCheckingSku && !skuExists;

  // Success view
  if (showSuccess) {
    return (
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link to={ROUTES.PRODUCTS.ROOT} aria-label="Volver a productos">
              <ArrowLeft className="size-5" />
            </Link>
          </Button>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Crear Producto</h1>
            <p className="text-muted-foreground">
              Añade un nuevo producto al catálogo
            </p>
          </div>
        </div>

        {/* Success Card */}
        <Card className="border-green-200 bg-green-50 dark:border-green-900 dark:bg-green-950">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-green-700 dark:text-green-400">
              <CheckCircle2 className="size-6" />
              Producto Creado Exitosamente
            </CardTitle>
            <CardDescription className="text-green-600 dark:text-green-500">
              El producto &quot;{createdProductName}&quot; ha sido añadido al catálogo
            </CardDescription>
          </CardHeader>
          <CardContent className="flex flex-col gap-4 sm:flex-row">
            <Button onClick={handleCreateAnother} variant="outline" className="gap-2">
              <Plus className="size-4" />
              Crear Otro Producto
            </Button>
            <Button asChild className="gap-2">
              <Link to={ROUTES.PRODUCTS.ROOT}>
                <Package className="size-4" />
                Ver Productos
              </Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Form view
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link to={ROUTES.PRODUCTS} aria-label="Volver a productos">
            <ArrowLeft className="size-5" />
          </Link>
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Crear Producto</h1>
          <p className="text-muted-foreground">
            Añade un nuevo producto al catálogo
          </p>
        </div>
      </div>

      {/* Form Card */}
      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle>Información del Producto</CardTitle>
          <CardDescription>
            Ingresa los datos del nuevo producto. Los campos marcados con * son obligatorios.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              {/* SKU Field */}
              <FormField
                control={form.control}
                name="sku"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>SKU *</FormLabel>
                    <div className="relative">
                      <FormControl>
                        <Input
                          placeholder="JOY-001"
                          autoComplete="off"
                          disabled={isSubmitting}
                          className={skuExists ? 'border-destructive pr-10' : 'pr-10'}
                          {...field}
                        />
                      </FormControl>
                      {isCheckingSku && (
                        <div className="absolute right-3 top-1/2 -translate-y-1/2">
                          <Loader2 className="size-4 animate-spin text-muted-foreground" />
                        </div>
                      )}
                    </div>
                    <FormDescription>
                      Identificador único del producto (ej: JOY-001)
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
                      <Input
                        placeholder="Anillo de Oro 18k"
                        disabled={isSubmitting}
                        {...field}
                      />
                    </FormControl>
                    <FormDescription>
                      Nombre descriptivo del producto
                    </FormDescription>
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
                        placeholder="Anillo de oro de 18 kilates con acabado brillante..."
                        disabled={isSubmitting}
                        rows={3}
                        {...field}
                      />
                    </FormControl>
                    <FormDescription>
                      Descripción detallada del producto (opcional)
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Price Field */}
              <FormField
                control={form.control}
                name="price"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Precio *</FormLabel>
                    <div className="relative">
                      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">
                        €
                      </span>
                      <FormControl>
                        <Input
                          type="number"
                          step="0.01"
                          min="0.01"
                          placeholder="299.99"
                          disabled={isSubmitting}
                          className="pl-7"
                          {...field}
                        />
                      </FormControl>
                    </div>
                    <FormDescription>
                      Precio de venta del producto (debe ser mayor que 0)
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Collection Field */}
              <FormField
                control={form.control}
                name="collectionId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Colección</FormLabel>
                    <Select
                      onValueChange={(value) =>
                        field.onChange(value === NO_COLLECTION_VALUE ? undefined : value)
                      }
                      value={field.value ?? NO_COLLECTION_VALUE}
                      disabled={isSubmitting || isLoadingCollections}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={
                            isLoadingCollections 
                              ? 'Cargando colecciones...' 
                              : 'Selecciona una colección (opcional)'
                          } />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value={NO_COLLECTION_VALUE}>
                          Sin colección
                        </SelectItem>
                        {collections.map((collection) => (
                          <SelectItem key={collection.id} value={collection.id}>
                            {collection.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormDescription>
                      Asigna el producto a una colección existente (opcional)
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Form Actions */}
              <div className="flex flex-col-reverse gap-3 pt-4 sm:flex-row sm:justify-end">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => navigate(ROUTES.PRODUCTS.ROOT)}
                  disabled={isSubmitting}
                >
                  Cancelar
                </Button>
                <Button type="submit" disabled={!canSubmit}>
                  {isSubmitting ? (
                    <>
                      <Loader2 className="mr-2 size-4 animate-spin" />
                      Creando...
                    </>
                  ) : (
                    <>
                      <Plus className="mr-2 size-4" />
                      Crear Producto
                    </>
                  )}
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  );
}

export default ProductCreatePage;
