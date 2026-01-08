/**
 * Stock Import Page
 * Allows administrators to import stock from Excel files
 */
import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import {
  ArrowLeft,
  Upload,
  FileSpreadsheet,
  Download,
  AlertCircle,
  CheckCircle2,
  Loader2,
} from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';

import { inventoryService } from '@/services/inventory.service';
import { pointOfSaleService } from '@/services/point-of-sale.service';
import { PointOfSale } from '@/types/point-of-sale.types';
import { StockImportResult } from '@/types/inventory.types';
import { ROUTES } from '@/routing/routes';

export function InventoryImportPage() {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [pointsOfSale, setPointsOfSale] = useState<PointOfSale[]>([]);
  const [selectedPosId, setSelectedPosId] = useState<string>('');
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [validationResult, setValidationResult] = useState<StockImportResult | null>(null);
  const [importResult, setImportResult] = useState<StockImportResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [validating, setValidating] = useState(false);
  const [importing, setImporting] = useState(false);

  // Load points of sale
  useEffect(() => {
    const loadData = async () => {
      try {
        const pos = await pointOfSaleService.getPointsOfSale(false);
        setPointsOfSale(pos);
        if (pos.length > 0) {
          setSelectedPosId(pos[0].id);
        }
      } catch (error) {
        toast.error('Error al cargar los puntos de venta');
        console.error(error);
      } finally {
        setLoading(false);
      }
    };
    loadData();
  }, []);

  const handleDownloadTemplate = async () => {
    try {
      const blob = await inventoryService.downloadImportTemplate();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'stock-import-template.xlsx';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      toast.success('Plantilla descargada');
    } catch (error) {
      toast.error('Error al descargar la plantilla');
      console.error(error);
    }
  };

  const handleFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file type
    const allowedTypes = ['.xlsx', '.xls'];
    const extension = file.name.toLowerCase().slice(file.name.lastIndexOf('.'));
    if (!allowedTypes.includes(extension)) {
      toast.error('Tipo de archivo no válido. Use archivos .xlsx o .xls');
      return;
    }

    // Validate file size (10 MB)
    if (file.size > 10 * 1024 * 1024) {
      toast.error('El archivo excede el tamaño máximo de 10 MB');
      return;
    }

    setSelectedFile(file);
    setValidationResult(null);
    setImportResult(null);

    // Auto-validate
    if (selectedPosId) {
      setValidating(true);
      try {
        const result = await inventoryService.validateImport(file, selectedPosId);
        setValidationResult(result);
      } catch (error) {
        toast.error('Error al validar el archivo');
        console.error(error);
      } finally {
        setValidating(false);
      }
    }
  };

  const handleImport = async () => {
    if (!selectedFile || !selectedPosId) return;

    setImporting(true);
    try {
      const result = await inventoryService.importStock(selectedFile, selectedPosId);
      setImportResult(result);
      if (result.success) {
        toast.success(result.message);
        setSelectedFile(null);
        setValidationResult(null);
        if (fileInputRef.current) {
          fileInputRef.current.value = '';
        }
      } else {
        toast.error(result.message);
      }
    } catch (error) {
      toast.error('Error al importar el stock');
      console.error(error);
    } finally {
      setImporting(false);
    }
  };

  const handleReset = () => {
    setSelectedFile(null);
    setValidationResult(null);
    setImportResult(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link to={ROUTES.INVENTORY.ROOT}>
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Importar Stock</h1>
          <p className="text-muted-foreground">
            Importar stock desde archivo Excel
          </p>
        </div>
      </div>

      {/* Instructions */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-lg">
            <FileSpreadsheet className="h-5 w-5" />
            Instrucciones
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">
            El archivo Excel debe contener las siguientes columnas con nombres exactos:
          </p>
          <ul className="ml-4 list-disc space-y-1 text-sm text-muted-foreground">
            <li><strong>SKU</strong> - Código del producto (debe existir en el catálogo)</li>
            <li><strong>Quantity</strong> - Cantidad a agregar (número entero ≥ 0)</li>
          </ul>
          <p className="text-sm text-muted-foreground">
            <strong>Nota:</strong> Las cantidades se <em>agregan</em> al stock existente.
            Los productos no asignados serán asignados automáticamente.
          </p>
          <Button variant="outline" onClick={handleDownloadTemplate}>
            <Download className="mr-2 h-4 w-4" />
            Descargar plantilla
          </Button>
        </CardContent>
      </Card>

      {/* Point of Sale Selector */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Punto de Venta</CardTitle>
          <CardDescription>
            Seleccione el punto de venta donde se importará el stock
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Select value={selectedPosId} onValueChange={setSelectedPosId}>
            <SelectTrigger className="w-full max-w-sm">
              <SelectValue placeholder="Seleccionar punto de venta" />
            </SelectTrigger>
            <SelectContent>
              {pointsOfSale.map((pos) => (
                <SelectItem key={pos.id} value={pos.id}>
                  {pos.name} ({pos.code})
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </CardContent>
      </Card>

      {/* File Upload */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Archivo Excel</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center gap-4">
            <input
              ref={fileInputRef}
              type="file"
              accept=".xlsx,.xls"
              onChange={handleFileChange}
              className="hidden"
              id="file-upload"
            />
            <Button
              variant="outline"
              onClick={() => fileInputRef.current?.click()}
              disabled={!selectedPosId}
            >
              <Upload className="mr-2 h-4 w-4" />
              Seleccionar archivo
            </Button>
            {selectedFile && (
              <span className="text-sm text-muted-foreground">
                {selectedFile.name}
              </span>
            )}
          </div>

          {validating && (
            <div className="flex items-center gap-2 text-muted-foreground">
              <Loader2 className="h-4 w-4 animate-spin" />
              Validando archivo...
            </div>
          )}
        </CardContent>
      </Card>

      {/* Validation Result */}
      {validationResult && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-lg">
              {validationResult.success ? (
                <CheckCircle2 className="h-5 w-5 text-green-500" />
              ) : (
                <AlertCircle className="h-5 w-5 text-destructive" />
              )}
              Resultado de Validación
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className={validationResult.success ? 'text-green-600' : 'text-destructive'}>
              {validationResult.message}
            </p>

            {validationResult.errors.length > 0 && (
              <div className="space-y-2">
                <p className="font-medium">Errores encontrados:</p>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Fila</TableHead>
                      <TableHead>Campo</TableHead>
                      <TableHead>Error</TableHead>
                      <TableHead>Valor</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {validationResult.errors.map((error, index) => (
                      <TableRow key={index}>
                        <TableCell>{error.rowNumber}</TableCell>
                        <TableCell>{error.field}</TableCell>
                        <TableCell>{error.message}</TableCell>
                        <TableCell className="font-mono text-sm">
                          {error.value || '-'}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            )}

            {validationResult.success && (
              <div className="flex gap-2">
                <Button onClick={handleImport} disabled={importing}>
                  {importing ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Importando...
                    </>
                  ) : (
                    <>
                      <Upload className="mr-2 h-4 w-4" />
                      Importar ({validationResult.totalRows} filas)
                    </>
                  )}
                </Button>
                <Button variant="outline" onClick={handleReset}>
                  Cancelar
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Import Result */}
      {importResult && importResult.success && (
        <Alert>
          <CheckCircle2 className="h-4 w-4" />
          <AlertTitle>Importación Exitosa</AlertTitle>
          <AlertDescription className="space-y-2">
            <p>{importResult.message}</p>
            <ul className="ml-4 list-disc">
              <li>Productos con stock actualizado: {importResult.stockUpdatedCount}</li>
              <li>Asignaciones creadas: {importResult.assignmentsCreatedCount}</li>
            </ul>
            {importResult.warnings.length > 0 && (
              <div className="mt-2">
                <p className="font-medium">Avisos:</p>
                <ul className="ml-4 list-disc text-sm">
                  {importResult.warnings.map((warning, index) => (
                    <li key={index}>{warning}</li>
                  ))}
                </ul>
              </div>
            )}
          </AlertDescription>
        </Alert>
      )}
    </div>
  );
}

export default InventoryImportPage;

