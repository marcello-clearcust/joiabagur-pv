/**
 * Returns Module - EP5
 * Main returns page with options for registration and history
 */
import { Link } from 'react-router-dom';
import { RotateCcw, History, PlusCircle } from 'lucide-react';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { ROUTES } from '@/routing/routes';

export function ReturnsPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Devoluciones</h1>
          <p className="text-muted-foreground">
            Gestiona devoluciones de productos
          </p>
        </div>
        <Button variant="outline" size="sm" asChild>
          <Link to={ROUTES.RETURNS.HISTORY}>
            <History className="mr-2 h-4 w-4" />
            Historial
          </Link>
        </Button>
      </div>

      {/* Registration Option */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* New Return */}
        <Card className="cursor-pointer transition-shadow hover:shadow-lg">
          <Link to={ROUTES.RETURNS.NEW}>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <PlusCircle className="h-5 w-5 text-primary" />
                Nueva Devolución
              </CardTitle>
              <CardDescription>
                Registra una devolución de producto seleccionando la venta original
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex items-center justify-between">
                <div className="space-y-1 text-sm text-muted-foreground">
                  <p>• Búsqueda de producto</p>
                  <p>• Selección de venta(s) original(es)</p>
                  <p>• Período de devolución: 30 días</p>
                  <p>• Actualización automática de stock</p>
                </div>
                <RotateCcw className="h-12 w-12 text-muted-foreground/50" />
              </div>
              <Button className="mt-4 w-full">
                Registrar Devolución
              </Button>
            </CardContent>
          </Link>
        </Card>

        {/* View History */}
        <Card className="cursor-pointer transition-shadow hover:shadow-lg">
          <Link to={ROUTES.RETURNS.HISTORY}>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <History className="h-5 w-5 text-primary" />
                Historial de Devoluciones
              </CardTitle>
              <CardDescription>
                Consulta el historial de devoluciones con filtros y detalles
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex items-center justify-between">
                <div className="space-y-1 text-sm text-muted-foreground">
                  <p>• Filtros por fecha</p>
                  <p>• Filtros por punto de venta</p>
                  <p>• Detalles de ventas asociadas</p>
                  <p>• Visualización de fotos</p>
                </div>
                <History className="h-12 w-12 text-muted-foreground/50" />
              </div>
              <Button className="mt-4 w-full" variant="secondary">
                Ver Historial
              </Button>
            </CardContent>
          </Link>
        </Card>
      </div>
    </div>
  );
}

export default ReturnsPage;
