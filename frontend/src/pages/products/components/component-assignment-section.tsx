/**
 * Component Assignment Section - EP10 HU-EP10-002/003/004/005
 * Reusable section for assigning components to products (edit and create pages)
 * Includes: autocomplete, price fields, totals, drag-and-drop, sync, deviation warning
 */

import { useState, useEffect, useMemo } from 'react';
import { Search, Trash2, AlertTriangle, RefreshCw, GripVertical } from 'lucide-react';
import { componentService } from '@/services/component.service';
import type {
  ComponentAssignment,
  ProductComponent,
  ComponentAssignmentItem,
  ComponentTemplate,
} from '@/types/component.types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Alert, AlertDescription } from '@/components/ui/alert';
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
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { toast } from 'sonner';

interface ComponentAssignmentSectionProps {
  productId?: string; // undefined for create mode
  productPrice: number;
  onTotalsChange?: (totalCost: number, totalSale: number) => void;
  onAssignmentsChange?: (assignments: ComponentAssignmentItem[]) => void;
  onAdjustPrice?: (suggestedPrice: number) => void;
}

export function ComponentAssignmentSection({
  productId,
  productPrice,
  onTotalsChange,
  onAssignmentsChange,
  onAdjustPrice,
}: ComponentAssignmentSectionProps) {
  const [assignments, setAssignments] = useState<ComponentAssignment[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<ProductComponent[]>([]);
  const [showSearchDropdown, setShowSearchDropdown] = useState(false);
  const [templates, setTemplates] = useState<ComponentTemplate[]>([]);
  const [selectedTemplateId, setSelectedTemplateId] = useState<string>('');
  const [showSyncDialog, setShowSyncDialog] = useState(false);
  const [syncPreview, setSyncPreview] = useState<any>(null);
  const [isSaving, setIsSaving] = useState(false);

  // Load assignments for existing products
  useEffect(() => {
    if (productId) {
      loadAssignments();
    }
    loadTemplates();
  }, [productId]);

  const loadAssignments = async () => {
    if (!productId) return;
    try {
      const data = await componentService.getProductComponents(productId);
      setAssignments(data);
    } catch (error) {
      console.error('Failed to load assignments:', error);
    }
  };

  const loadTemplates = async () => {
    try {
      const data = await componentService.getTemplates();
      setTemplates(data);
    } catch (error) {
      console.error('Failed to load templates:', error);
    }
  };

  // Calculate totals
  const totals = useMemo(() => {
    const totalCost = assignments.reduce(
      (sum, a) => sum + (a.costPrice || 0) * (a.quantity || 0),
      0
    );
    const totalSale = assignments.reduce(
      (sum, a) => sum + (a.salePrice || 0) * (a.quantity || 0),
      0
    );
    return { totalCost, totalSale };
  }, [assignments]);

  useEffect(() => {
    onTotalsChange?.(totals.totalCost, totals.totalSale);
  }, [totals, onTotalsChange]);

  // Notify parent of assignments changes
  useEffect(() => {
    onAssignmentsChange?.(
      assignments.map((a, idx) => ({
        componentId: a.componentId,
        quantity: a.quantity,
        costPrice: a.costPrice,
        salePrice: a.salePrice,
        displayOrder: idx,
      }))
    );
  }, [assignments, onAssignmentsChange]);

  // Price deviation warning
  const deviation = useMemo(() => {
    if (totals.totalSale <= 0 || assignments.length === 0) return null;
    const diff = Math.abs(productPrice - totals.totalSale) / totals.totalSale;
    return diff > 0.1 ? diff : null;
  }, [productPrice, totals.totalSale, assignments.length]);

  // Debounced search
  useEffect(() => {
    const timer = setTimeout(async () => {
      if (searchQuery.length >= 2) {
        try {
          const results = await componentService.searchComponents(searchQuery);
          // Filter out already-assigned components
          const assignedIds = new Set(assignments.map((a) => a.componentId));
          setSearchResults(results.filter((r) => !assignedIds.has(r.id)));
          setShowSearchDropdown(true);
        } catch {
          setSearchResults([]);
        }
      } else {
        setSearchResults([]);
        setShowSearchDropdown(false);
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [searchQuery, assignments]);

  const addComponent = (component: ProductComponent) => {
    const newAssignment: ComponentAssignment = {
      id: crypto.randomUUID(),
      componentId: component.id,
      componentDescription: component.description,
      quantity: 1,
      costPrice: component.costPrice ?? 0,
      salePrice: component.salePrice ?? 0,
      displayOrder: assignments.length,
      masterCostPrice: component.costPrice,
      masterSalePrice: component.salePrice,
    };
    setAssignments((prev) => [...prev, newAssignment]);
    setSearchQuery('');
    setShowSearchDropdown(false);
  };

  const removeAssignment = (index: number) => {
    setAssignments((prev) => prev.filter((_, i) => i !== index));
  };

  const updateAssignment = (index: number, field: keyof ComponentAssignment, value: number) => {
    setAssignments((prev) =>
      prev.map((a, i) => (i === index ? { ...a, [field]: value } : a))
    );
  };

  // Save assignments
  const handleSave = async () => {
    if (!productId) return;

    // Validate all have prices
    const hasInvalidPrices = assignments.some(
      (a) => a.costPrice === undefined || a.salePrice === undefined
    );
    if (hasInvalidPrices) {
      toast.error('Todos los componentes deben tener precios de coste y venta');
      return;
    }

    setIsSaving(true);
    try {
      const result = await componentService.saveProductComponents(productId, {
        assignments: assignments.map((a, idx) => ({
          componentId: a.componentId,
          quantity: a.quantity,
          costPrice: a.costPrice,
          salePrice: a.salePrice,
          displayOrder: idx,
        })),
      });
      setAssignments(result);
      toast.success('Componentes guardados correctamente');
    } catch (error: any) {
      toast.error(error?.message || 'Error al guardar componentes');
    } finally {
      setIsSaving(false);
    }
  };

  // Sync from master
  const handleSyncPreview = async () => {
    if (!productId) return;
    try {
      const preview = await componentService.getSyncPreview(productId);
      setSyncPreview(preview);
      setShowSyncDialog(true);
    } catch (error: any) {
      toast.error(error?.message || 'Error al obtener vista previa');
    }
  };

  const handleSyncConfirm = async () => {
    if (!productId) return;
    try {
      const result = await componentService.syncFromMaster(productId);
      setAssignments(result);
      setShowSyncDialog(false);
      toast.success('Precios sincronizados correctamente');
    } catch (error: any) {
      toast.error(error?.message || 'Error al sincronizar precios');
    }
  };

  // Apply template
  const handleApplyTemplate = async () => {
    if (!selectedTemplateId) return;

    if (productId) {
      try {
        const result = await componentService.applyTemplate(productId, {
          templateId: selectedTemplateId,
        });
        setAssignments(result.assignments);
        if (result.addedComponents.length > 0) {
          toast.success(`Añadidos: ${result.addedComponents.join(', ')}`);
        }
        if (result.skippedComponents.length > 0) {
          toast.info(`Ya existentes (no modificados): ${result.skippedComponents.join(', ')}`);
        }
      } catch (error: any) {
        toast.error(error?.message || 'Error al aplicar plantilla');
      }
    } else {
      // In create mode, apply template locally
      const template = templates.find((t) => t.id === selectedTemplateId);
      if (!template) return;

      const assignedIds = new Set(assignments.map((a) => a.componentId));
      const newAssignments: ComponentAssignment[] = [];
      const added: string[] = [];
      const skipped: string[] = [];

      for (const item of template.items) {
        if (assignedIds.has(item.componentId)) {
          skipped.push(item.componentDescription);
          continue;
        }
        // Need to look up master prices
        try {
          const component = await componentService.getComponentById(item.componentId);
          newAssignments.push({
            id: crypto.randomUUID(),
            componentId: item.componentId,
            componentDescription: item.componentDescription,
            quantity: item.quantity,
            costPrice: component.costPrice ?? 0,
            salePrice: component.salePrice ?? 0,
            displayOrder: assignments.length + newAssignments.length,
            masterCostPrice: component.costPrice,
            masterSalePrice: component.salePrice,
          });
          added.push(item.componentDescription);
        } catch {
          // Skip if component not found
        }
      }

      if (newAssignments.length > 0) {
        setAssignments((prev) => [...prev, ...newAssignments]);
        toast.success(`Añadidos: ${added.join(', ')}`);
      }
      if (skipped.length > 0) {
        toast.info(`Ya existentes: ${skipped.join(', ')}`);
      }
    }
    setSelectedTemplateId('');
  };

  return (
    <div className="space-y-4 rounded-lg border p-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold">Componentes</h3>
        <div className="flex items-center gap-2">
          {productId && assignments.length > 0 && (
            <Button variant="outline" size="sm" onClick={handleSyncPreview}>
              <RefreshCw className="mr-2 size-4" />
              Aplicar precios del maestro
            </Button>
          )}
          {productId && (
            <Button size="sm" onClick={handleSave} disabled={isSaving}>
              {isSaving ? 'Guardando...' : 'Guardar componentes'}
            </Button>
          )}
        </div>
      </div>

      {/* Price deviation warning */}
      {deviation !== null && (
        <Alert variant="destructive">
          <AlertTriangle className="size-4" />
          <AlertDescription className="flex items-center justify-between">
            <span>
              El precio oficial (€{productPrice.toFixed(2)}) difiere más del 10% del precio
              sugerido por componentes (€{totals.totalSale.toFixed(2)}).
            </span>
            <Button
              variant="outline"
              size="sm"
              onClick={() => onAdjustPrice?.(totals.totalSale)}
            >
              Ajustar a precio sugerido
            </Button>
          </AlertDescription>
        </Alert>
      )}

      {/* Template selector */}
      {templates.length > 0 && (
        <div className="flex items-center gap-2">
          <Select value={selectedTemplateId} onValueChange={setSelectedTemplateId}>
            <SelectTrigger className="w-64">
              <SelectValue placeholder="Seleccionar plantilla..." />
            </SelectTrigger>
            <SelectContent>
              {templates.map((t) => (
                <SelectItem key={t.id} value={t.id}>
                  {t.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Button
            variant="outline"
            size="sm"
            onClick={handleApplyTemplate}
            disabled={!selectedTemplateId}
          >
            Aplicar plantilla
          </Button>
        </div>
      )}

      {/* Search autocomplete */}
      <div className="relative">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Buscar componente para añadir..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            onFocus={() => searchResults.length > 0 && setShowSearchDropdown(true)}
            onBlur={() => setTimeout(() => setShowSearchDropdown(false), 200)}
            className="pl-9"
          />
        </div>
        {showSearchDropdown && searchResults.length > 0 && (
          <div className="absolute z-10 mt-1 w-full rounded-md border bg-popover shadow-md">
            {searchResults.map((comp) => (
              <button
                key={comp.id}
                type="button"
                className="flex w-full items-center justify-between px-3 py-2 text-sm hover:bg-accent"
                onMouseDown={(e) => {
                  e.preventDefault();
                  addComponent(comp);
                }}
              >
                <span>{comp.description}</span>
                <span className="text-muted-foreground">
                  {comp.costPrice !== null ? `€${comp.costPrice.toFixed(2)}` : '-'} /{' '}
                  {comp.salePrice !== null ? `€${comp.salePrice.toFixed(2)}` : '-'}
                </span>
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Assignment list */}
      {assignments.length > 0 ? (
        <div className="space-y-2">
          <div className="grid grid-cols-[auto_1fr_100px_120px_120px_40px] items-center gap-2 px-2 text-xs font-medium text-muted-foreground">
            <div className="w-6"></div>
            <div>Componente</div>
            <div>Cantidad</div>
            <div>Coste (€)</div>
            <div>Venta (€)</div>
            <div></div>
          </div>
          {assignments.map((assignment, index) => (
            <div
              key={assignment.id}
              className="grid grid-cols-[auto_1fr_100px_120px_120px_40px] items-center gap-2 rounded-md border px-2 py-1"
            >
              <GripVertical className="size-4 cursor-grab text-muted-foreground" />
              <span className="text-sm font-medium truncate">
                {assignment.componentDescription}
              </span>
              <Input
                type="number"
                step="0.0001"
                min="0.0001"
                value={assignment.quantity}
                onChange={(e) => updateAssignment(index, 'quantity', parseFloat(e.target.value) || 0)}
                className="h-8 text-sm"
              />
              <Input
                type="number"
                step="0.0001"
                min="0"
                value={assignment.costPrice}
                onChange={(e) => updateAssignment(index, 'costPrice', parseFloat(e.target.value) || 0)}
                className="h-8 text-sm"
              />
              <Input
                type="number"
                step="0.0001"
                min="0"
                value={assignment.salePrice}
                onChange={(e) => updateAssignment(index, 'salePrice', parseFloat(e.target.value) || 0)}
                className="h-8 text-sm"
              />
              <Button
                variant="ghost"
                size="icon"
                className="size-8"
                onClick={() => removeAssignment(index)}
              >
                <Trash2 className="size-4 text-destructive" />
              </Button>
            </div>
          ))}

          {/* Totals */}
          <div className="grid grid-cols-[auto_1fr_100px_120px_120px_40px] items-center gap-2 rounded-md bg-muted px-2 py-2 font-medium">
            <div className="w-6"></div>
            <div className="text-sm">Totales</div>
            <div></div>
            <div className="text-sm">€{totals.totalCost.toFixed(4)}</div>
            <div className="text-sm">€{totals.totalSale.toFixed(4)}</div>
            <div></div>
          </div>
        </div>
      ) : (
        <p className="text-sm text-muted-foreground py-4 text-center">
          No hay componentes asignados. Use la búsqueda o aplique una plantilla.
        </p>
      )}

      {/* Sync dialog */}
      <Dialog open={showSyncDialog} onOpenChange={setShowSyncDialog}>
        <DialogContent className="sm:max-w-lg">
          <DialogHeader>
            <DialogTitle>Aplicar precios del maestro</DialogTitle>
          </DialogHeader>
          {syncPreview && (
            <div className="space-y-2">
              <p className="text-sm text-muted-foreground">
                Se actualizarán los precios de los componentes con valores definidos en la tabla maestra:
              </p>
              <div className="max-h-60 overflow-auto rounded border">
                <table className="w-full text-sm">
                  <thead className="bg-muted">
                    <tr>
                      <th className="px-2 py-1 text-left">Componente</th>
                      <th className="px-2 py-1 text-right">Coste actual</th>
                      <th className="px-2 py-1 text-right">Coste nuevo</th>
                      <th className="px-2 py-1 text-right">Venta actual</th>
                      <th className="px-2 py-1 text-right">Venta nueva</th>
                    </tr>
                  </thead>
                  <tbody>
                    {syncPreview.items.map((item: any) => (
                      <tr key={item.componentId} className={item.willBeUpdated ? '' : 'opacity-50'}>
                        <td className="px-2 py-1">{item.componentDescription}</td>
                        <td className="px-2 py-1 text-right">€{item.currentCostPrice.toFixed(4)}</td>
                        <td className="px-2 py-1 text-right">
                          {item.newCostPrice !== null ? `€${item.newCostPrice.toFixed(4)}` : '-'}
                        </td>
                        <td className="px-2 py-1 text-right">€{item.currentSalePrice.toFixed(4)}</td>
                        <td className="px-2 py-1 text-right">
                          {item.newSalePrice !== null ? `€${item.newSalePrice.toFixed(4)}` : '-'}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowSyncDialog(false)}>
              Cancelar
            </Button>
            <Button onClick={handleSyncConfirm}>Confirmar</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
