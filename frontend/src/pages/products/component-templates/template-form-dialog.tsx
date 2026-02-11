/**
 * Template Form Dialog - EP10 HU-EP10-006
 * Create/Edit template dialog with component list management
 */

import { useState, useEffect } from 'react';
import { Search, Trash2 } from 'lucide-react';
import { componentService } from '@/services/component.service';
import type { ComponentTemplate, ProductComponent } from '@/types/component.types';
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
import { Textarea } from '@/components/ui/textarea';
import { toast } from 'sonner';

interface TemplateItem {
  componentId: string;
  componentDescription: string;
  quantity: number;
}

interface TemplateFormDialogProps {
  open: boolean;
  onClose: (success?: boolean) => void;
  template: ComponentTemplate | null;
}

export function TemplateFormDialog({ open, onClose, template }: TemplateFormDialogProps) {
  const isEditing = !!template;
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [items, setItems] = useState<TemplateItem[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<ProductComponent[]>([]);
  const [showDropdown, setShowDropdown] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (open) {
      setName(template?.name ?? '');
      setDescription(template?.description ?? '');
      setItems(
        template?.items.map((i) => ({
          componentId: i.componentId,
          componentDescription: i.componentDescription,
          quantity: i.quantity,
        })) ?? []
      );
    }
  }, [open, template]);

  // Debounced search
  useEffect(() => {
    const timer = setTimeout(async () => {
      if (searchQuery.length >= 2) {
        try {
          const results = await componentService.searchComponents(searchQuery);
          const existingIds = new Set(items.map((i) => i.componentId));
          setSearchResults(results.filter((r) => !existingIds.has(r.id)));
          setShowDropdown(true);
        } catch {
          setSearchResults([]);
        }
      } else {
        setSearchResults([]);
        setShowDropdown(false);
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [searchQuery, items]);

  const addComponent = (component: ProductComponent) => {
    setItems((prev) => [
      ...prev,
      { componentId: component.id, componentDescription: component.description, quantity: 1 },
    ]);
    setSearchQuery('');
    setShowDropdown(false);
  };

  const removeItem = (index: number) => {
    setItems((prev) => prev.filter((_, i) => i !== index));
  };

  const updateQuantity = (index: number, quantity: number) => {
    setItems((prev) =>
      prev.map((item, i) => (i === index ? { ...item, quantity } : item))
    );
  };

  const handleSubmit = async () => {
    if (!name.trim()) {
      toast.error('El nombre es obligatorio');
      return;
    }

    setIsSubmitting(true);
    try {
      const data = {
        name: name.trim(),
        description: description.trim() || null,
        items: items.map((i) => ({
          componentId: i.componentId,
          quantity: i.quantity,
        })),
      };

      if (isEditing) {
        await componentService.updateTemplate(template!.id, data);
        toast.success('Plantilla actualizada correctamente');
      } else {
        await componentService.createTemplate(data);
        toast.success('Plantilla creada correctamente');
      }
      onClose(true);
    } catch (error: any) {
      toast.error(error?.message || 'Error al guardar la plantilla');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={() => onClose()}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{isEditing ? 'Editar Plantilla' : 'Nueva Plantilla'}</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="template-name">Nombre *</Label>
            <Input
              id="template-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Ej: Anillo oro"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="template-description">Descripción</Label>
            <Textarea
              id="template-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Descripción opcional..."
              rows={2}
            />
          </div>

          <div className="space-y-2">
            <Label>Componentes</Label>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Buscar componente..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onFocus={() => searchResults.length > 0 && setShowDropdown(true)}
                onBlur={() => setTimeout(() => setShowDropdown(false), 200)}
                className="pl-9"
              />
              {showDropdown && searchResults.length > 0 && (
                <div className="absolute z-10 mt-1 w-full rounded-md border bg-popover shadow-md max-h-40 overflow-auto">
                  {searchResults.map((comp) => (
                    <button
                      key={comp.id}
                      type="button"
                      className="flex w-full items-center px-3 py-2 text-sm hover:bg-accent"
                      onMouseDown={(e) => {
                        e.preventDefault();
                        addComponent(comp);
                      }}
                    >
                      {comp.description}
                    </button>
                  ))}
                </div>
              )}
            </div>

            {(items ?? []).length > 0 ? (
              <div className="space-y-1 mt-2">
                {(items ?? []).map((item, index) => (
                  <div key={item.componentId} className="flex items-center gap-2 rounded border px-2 py-1">
                    <span className="flex-1 text-sm">{item.componentDescription}</span>
                    <Input
                      type="number"
                      step="0.0001"
                      min="0.0001"
                      value={item.quantity}
                      onChange={(e) => updateQuantity(index, parseFloat(e.target.value) || 0)}
                      className="h-7 w-24 text-sm"
                    />
                    <Button variant="ghost" size="icon" className="size-7" onClick={() => removeItem(index)}>
                      <Trash2 className="size-3.5 text-destructive" />
                    </Button>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground text-center py-2">
                Sin componentes. Use la búsqueda para añadir.
              </p>
            )}
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onClose()}>
            Cancelar
          </Button>
          <Button onClick={handleSubmit} disabled={isSubmitting}>
            {isSubmitting ? 'Guardando...' : isEditing ? 'Actualizar' : 'Crear'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
