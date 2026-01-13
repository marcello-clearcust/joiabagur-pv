/**
 * Sales Module - EP3
 * Main sales page with options for registration methods
 */
import { Link } from 'react-router-dom';
import { Camera, PenLine, History, ShoppingCart } from 'lucide-react';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { ROUTES } from '@/routing/routes';

export function SalesPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Ventas</h1>
          <p className="text-muted-foreground">
            Registra ventas en punto de venta
          </p>
        </div>
        <Button variant="outline" size="sm" asChild>
          <Link to={ROUTES.SALES.HISTORY}>
            <History className="mr-2 h-4 w-4" />
            Historial
          </Link>
        </Button>
      </div>

      {/* Registration Options */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* Manual Registration */}
        <Card className="cursor-pointer transition-shadow hover:shadow-lg">
          <Link to={ROUTES.SALES.NEW}>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <PenLine className="h-5 w-5 text-primary" />
                Registro Manual
              </CardTitle>
              <CardDescription>
                Busca productos por SKU o nombre y registra la venta manualmente
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex items-center justify-between">
                <div className="space-y-1 text-sm text-muted-foreground">
                  <p>• Búsqueda por SKU o nombre</p>
                  <p>• Selección de método de pago</p>
                  <p>• Cantidad editable</p>
                </div>
                <ShoppingCart className="h-12 w-12 text-muted-foreground/50" />
              </div>
              <Button className="mt-4 w-full">
                Registrar Venta Manual
              </Button>
            </CardContent>
          </Link>
        </Card>

        {/* Image Recognition */}
        <Card className="cursor-pointer transition-shadow hover:shadow-lg">
          <Link to={ROUTES.SALES.NEW_IMAGE}>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Camera className="h-5 w-5 text-primary" />
                Reconocimiento de Imagen
              </CardTitle>
              <CardDescription>
                Toma una foto del producto y la IA sugerirá las opciones más probables
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex items-center justify-between">
                <div className="space-y-1 text-sm text-muted-foreground">
                  <p>• Captura de foto desde cámara</p>
                  <p>• Sugerencias con IA</p>
                  <p>• Selección rápida</p>
                </div>
                <Camera className="h-12 w-12 text-muted-foreground/50" />
              </div>
              <Button className="mt-4 w-full" variant="secondary">
                Usar Reconocimiento de Imagen
              </Button>
            </CardContent>
          </Link>
        </Card>
      </div>
    </div>
  );
}

export default SalesPage;
