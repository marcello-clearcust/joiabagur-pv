/**
 * Component Templates Page - EP10 HU-EP10-006
 * CRUD management for component templates (admin-only)
 */

import { useState, useEffect, useMemo } from 'react';
import {
  useReactTable,
  getCoreRowModel,
  ColumnDef,
  createColumnHelper,
} from '@tanstack/react-table';
import { Plus, Pencil, Trash2, MoreHorizontal } from 'lucide-react';
import { componentService } from '@/services/component.service';
import type { ComponentTemplate } from '@/types/component.types';
import { DataGrid, DataGridContainer } from '@/components/ui/data-grid';
import { DataGridTable } from '@/components/ui/data-grid-table';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { toast } from 'sonner';
import { TemplateFormDialog } from './template-form-dialog';

const columnHelper = createColumnHelper<ComponentTemplate>();

export function ComponentTemplatesPage() {
  const [templates, setTemplates] = useState<ComponentTemplate[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [selectedTemplate, setSelectedTemplate] = useState<ComponentTemplate | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<ComponentTemplate | null>(null);

  const fetchTemplates = async () => {
    setIsLoading(true);
    try {
      const data = await componentService.getTemplates();
      setTemplates(data);
    } catch (error) {
      toast.error('Error al cargar las plantillas');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchTemplates();
  }, []);

  const handleCreate = () => {
    setSelectedTemplate(null);
    setIsDialogOpen(true);
  };

  const handleEdit = (template: ComponentTemplate) => {
    setSelectedTemplate(template);
    setIsDialogOpen(true);
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await componentService.deleteTemplate(deleteTarget.id);
      toast.success('Plantilla eliminada correctamente');
      fetchTemplates();
    } catch (error: any) {
      toast.error(error?.message || 'Error al eliminar la plantilla');
    } finally {
      setDeleteTarget(null);
    }
  };

  const handleDialogClose = (success?: boolean) => {
    setIsDialogOpen(false);
    setSelectedTemplate(null);
    if (success) fetchTemplates();
  };

  const columns = useMemo<ColumnDef<ComponentTemplate, unknown>[]>(
    () => [
      columnHelper.accessor('name', {
        header: () => 'Nombre',
        cell: (info) => <span className="font-medium">{info.getValue()}</span>,
        size: 200,
        meta: { skeleton: <Skeleton className="h-4 w-32" /> },
      }),
      columnHelper.accessor('description', {
        header: () => 'Descripción',
        cell: (info) => (
          <span className="text-muted-foreground">{info.getValue() || '-'}</span>
        ),
        size: 300,
        meta: { skeleton: <Skeleton className="h-4 w-48" /> },
      }),
      columnHelper.accessor('items', {
        header: () => 'Componentes',
        cell: (info) => `${info.getValue().length} componente${info.getValue().length !== 1 ? 's' : ''}`,
        size: 120,
        meta: { skeleton: <Skeleton className="h-4 w-20" /> },
      }),
      columnHelper.display({
        id: 'actions',
        header: () => 'Acciones',
        cell: ({ row }) => {
          const template = row.original;
          return (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon">
                  <MoreHorizontal className="size-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={() => handleEdit(template)}>
                  <Pencil className="mr-2 size-4" />
                  Editar
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  className="text-destructive"
                  onClick={() => setDeleteTarget(template)}
                >
                  <Trash2 className="mr-2 size-4" />
                  Eliminar
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          );
        },
        size: 70,
        meta: { skeleton: <Skeleton className="h-8 w-8 ml-auto" /> },
      }),
    ],
    []
  );

  const table = useReactTable({
    data: templates,
    columns,
    getCoreRowModel: getCoreRowModel(),
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Plantillas de Componentes</h1>
          <p className="text-muted-foreground">
            Plantillas reutilizables para asignar componentes a productos
          </p>
        </div>
        <Button onClick={handleCreate}>
          <Plus className="mr-2 size-4" />
          Nueva Plantilla
        </Button>
      </div>

      <DataGridContainer>
        <DataGrid
          table={table}
          recordCount={templates.length}
          isLoading={isLoading}
          emptyMessage="No hay plantillas registradas"
        >
          <DataGridTable />
        </DataGrid>
      </DataGridContainer>

      <TemplateFormDialog
        open={isDialogOpen}
        onClose={handleDialogClose}
        template={selectedTemplate}
      />

      <AlertDialog open={!!deleteTarget} onOpenChange={() => setDeleteTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Eliminar plantilla</AlertDialogTitle>
            <AlertDialogDescription>
              ¿Está seguro de eliminar la plantilla "{deleteTarget?.name}"? Los productos que
              ya tienen componentes asignados no se verán afectados.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete}>Eliminar</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

export default ComponentTemplatesPage;
