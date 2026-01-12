/**
 * Products Module - EP1
 * Product management page with links to sub-modules
 */

import { Link } from 'react-router-dom';
import { FileSpreadsheet, Package, Plus, Download } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { ROUTES } from '@/routing/routes';
import { productService } from '@/services/product.service';
import { toast } from 'sonner';

export function ProductsPage() {
  /**
   * Download Excel template for product import
   */
  const handleDownloadTemplate = async () => {
    try {
      const blob = await productService.downloadImportTemplate();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'product-import-template.xlsx';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      toast.success('Plantilla descargada en formato Excel (.xlsx)');
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Error al descargar la plantilla';
      toast.error(errorMessage);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Productos</h1>
          <p className="text-muted-foreground">
            Gestión del catálogo de productos
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" asChild>
            <Link to={ROUTES.PRODUCTS.IMPORT}>
              <FileSpreadsheet className="mr-2 size-4" />
              Importar Excel
            </Link>
          </Button>
          <Button asChild>
            <Link to={ROUTES.PRODUCTS.CREATE}>
              <Plus className="mr-2 size-4" />
              Nuevo Producto
            </Link>
          </Button>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <Card className="border-primary/50">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-primary">
              <Package className="size-5" />
              Catálogo de Productos
            </CardTitle>
            <CardDescription>
              Ver y gestionar todos los productos del sistema
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground mb-4">
              Lista completa de productos con opciones de búsqueda, filtrado y edición.
            </p>
            <Button asChild className="w-full">
              <Link to={ROUTES.PRODUCTS.CATALOG}>
                Ver Catálogo
              </Link>
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Plus className="size-5" />
              Crear Producto
            </CardTitle>
            <CardDescription>
              Añadir un producto individual
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground mb-4">
              Crea un nuevo producto manualmente con todos sus detalles.
            </p>
            <Button variant="secondary" asChild className="w-full">
              <Link to={ROUTES.PRODUCTS.CREATE}>
                Crear Producto
              </Link>
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FileSpreadsheet className="size-5" />
              Importar Productos
            </CardTitle>
            <CardDescription>
              Carga masiva de productos desde Excel
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground mb-4">
              Importa o actualiza múltiples productos a la vez usando un archivo Excel.
            </p>
            <div className="space-y-2">
              <Button variant="secondary" asChild className="w-full">
                <Link to={ROUTES.PRODUCTS.IMPORT}>
                  Ir a Importación
                </Link>
              </Button>
              <Button 
                variant="outline" 
                className="w-full" 
                onClick={handleDownloadTemplate}
              >
                <Download className="mr-2 size-4" />
                Descargar Plantilla Excel
              </Button>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Package className="size-5" />
              Colecciones
            </CardTitle>
            <CardDescription>
              Organiza productos por colecciones
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground mb-4">
              Gestiona las colecciones para agrupar y categorizar productos.
            </p>
            <Button variant="secondary" disabled className="w-full">
              Ver Colecciones (Próximamente)
            </Button>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export default ProductsPage;
