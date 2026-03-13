/**
 * Reports Module - EP9/EP10
 * Reports hub page with links to available reports
 */

import { Link } from 'react-router-dom';
import { TrendingUp, PackageX, ShoppingCart, ArrowLeftRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { ROUTES } from '@/routing/routes';

export function ReportsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Reportes</h1>
        <p className="text-muted-foreground">
          Consultas y reportes del sistema
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <ShoppingCart className="size-5" />
              Reporte de Ventas
            </CardTitle>
            <CardDescription>
              Análisis detallado de ventas con exportación a Excel
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground mb-4">
              Consulta ventas con filtros avanzados, totales globales por filtro y exportación
              a Excel con resumen por punto de venta.
            </p>
            <Button variant="secondary" asChild className="w-full">
              <Link to={ROUTES.REPORTS.SALES}>
                Ver Reporte
              </Link>
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <TrendingUp className="size-5" />
              Márgenes por Producto
            </CardTitle>
            <CardDescription>
              Análisis de rentabilidad basado en componentes
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground mb-4">
              Visualiza costes, precios y márgenes de cada producto calculados desde sus componentes.
              Incluye exportación a Excel.
            </p>
            <Button variant="secondary" asChild className="w-full">
              <Link to={ROUTES.REPORTS.PRODUCT_MARGINS}>
                Ver Reporte
              </Link>
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <PackageX className="size-5" />
              Productos sin Componentes
            </CardTitle>
            <CardDescription>
              Identificar productos sin estructura de costes
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground mb-4">
              Lista de productos que no tienen componentes asignados, con acceso directo a edición.
            </p>
            <Button variant="secondary" asChild className="w-full">
              <Link to={ROUTES.REPORTS.PRODUCTS_WITHOUT_COMPONENTS}>
                Ver Reporte
              </Link>
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <ArrowLeftRight className="size-5" />
              Resumen de movimientos de inventario
            </CardTitle>
            <CardDescription>
              Adiciones y sustracciones agregadas por producto
            </CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground mb-4">
              Vista agregada de entradas y salidas de inventario por producto en un rango de fechas,
              con filtro opcional por punto de venta y exportación a Excel.
            </p>
            <Button variant="secondary" asChild className="w-full">
              <Link to={ROUTES.REPORTS.INVENTORY_MOVEMENT_SUMMARY}>
                Ver Reporte
              </Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export default ReportsPage;
