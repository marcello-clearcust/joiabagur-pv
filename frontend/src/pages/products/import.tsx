/**
 * Product Import Page - EP1
 * Allows administrators to import products from Excel files
 */

import { useState, useCallback, useEffect } from 'react';
import { useDropzone } from 'react-dropzone';
import { 
  Upload, 
  FileSpreadsheet, 
  Download, 
  AlertCircle, 
  CheckCircle2, 
  X,
  Loader2,
  AlertTriangle,
  Eye,
  Plus,
  RefreshCw
} from 'lucide-react';
import * as XLSX from 'xlsx';
import { productService } from '@/services/product.service';
import { ImportResult } from '@/types/product.types';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { toast } from 'sonner';
import { cn } from '@/lib/utils';

/**
 * Preview row data from Excel
 */
interface PreviewRow {
  sku: string;
  name: string;
  description?: string;
  price: string | number;
  collection?: string;
}

// Excel template columns
const TEMPLATE_COLUMNS = ['SKU', 'Name', 'Description', 'Price', 'Collection'];
const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB
const ACCEPTED_EXTENSIONS = ['.xlsx', '.xls'];

export function ProductImportPage() {
  const [file, setFile] = useState<File | null>(null);
  const [previewData, setPreviewData] = useState<PreviewRow[]>([]);
  const [validationResult, setValidationResult] = useState<ImportResult | null>(null);
  const [importResult, setImportResult] = useState<ImportResult | null>(null);
  const [isLoadingPreview, setIsLoadingPreview] = useState(false);
  const [isValidating, setIsValidating] = useState(false);
  const [isImporting, setIsImporting] = useState(false);
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);

  /**
   * Parse Excel file to get preview data (first 5 rows)
   */
  const parseExcelPreview = useCallback(async (selectedFile: File) => {
    setIsLoadingPreview(true);
    try {
      const arrayBuffer = await selectedFile.arrayBuffer();
      const workbook = XLSX.read(arrayBuffer, { type: 'array' });
      const firstSheet = workbook.Sheets[workbook.SheetNames[0]];
      
      // Convert to JSON, including all rows
      const jsonData = XLSX.utils.sheet_to_json<Record<string, unknown>>(firstSheet, {
        defval: '',
      });

      // Map to preview format (first 5 rows)
      const preview: PreviewRow[] = jsonData.slice(0, 5).map((row) => ({
        sku: String(row['SKU'] || row['sku'] || ''),
        name: String(row['Name'] || row['name'] || row['Nombre'] || ''),
        description: String(row['Description'] || row['description'] || row['Descripción'] || ''),
        price: row['Price'] || row['price'] || row['Precio'] || '',
        collection: String(row['Collection'] || row['collection'] || row['Colección'] || ''),
      }));

      setPreviewData(preview);
    } catch (error) {
      console.error('Error parsing Excel for preview:', error);
      setPreviewData([]);
    } finally {
      setIsLoadingPreview(false);
    }
  }, []);

  // Handle file drop
  const onDrop = useCallback(async (acceptedFiles: File[]) => {
    if (acceptedFiles.length === 0) return;

    const selectedFile = acceptedFiles[0];
    
    // Reset state
    setValidationResult(null);
    setImportResult(null);
    setPreviewData([]);
    setFile(selectedFile);

    // Parse Excel for preview first
    await parseExcelPreview(selectedFile);

    // Validate file on server
    setIsValidating(true);
    try {
      const result = await productService.validateImport(selectedFile);
      setValidationResult(result);
      
      if (result.success) {
        toast.success(`Archivo válido: ${result.totalRows} productos encontrados`);
      } else {
        toast.warning(`Se encontraron ${result.errors.length} errores de validación`);
      }
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Error al validar el archivo';
      toast.error(errorMessage);
      setValidationResult({
        success: false,
        totalRows: 0,
        createdCount: 0,
        updatedCount: 0,
        collectionsCreatedCount: 0,
        errors: [{ rowNumber: 0, field: 'File', message: errorMessage }],
        warnings: [],
        message: errorMessage,
      });
    } finally {
      setIsValidating(false);
    }
  }, [parseExcelPreview]);

  const { getRootProps, getInputProps, isDragActive, isDragReject } = useDropzone({
    onDrop,
    accept: {
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': ['.xlsx'],
      'application/vnd.ms-excel': ['.xls'],
    },
    maxSize: MAX_FILE_SIZE,
    multiple: false,
    disabled: isValidating || isImporting,
  });

  // Download template
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

  // Handle import confirmation
  const handleImportClick = () => {
    setShowConfirmDialog(true);
  };

  // Execute import
  const handleConfirmImport = async () => {
    if (!file) return;

    setShowConfirmDialog(false);
    setIsImporting(true);

    try {
      const result = await productService.importProducts(file);
      setImportResult(result);

      if (result.success) {
        toast.success(
          `Importación exitosa: ${result.createdCount} creados, ${result.updatedCount} actualizados`
        );
        // Reset for new import
        setFile(null);
        setValidationResult(null);
      } else {
        toast.error(`Error en la importación: ${result.message}`);
      }
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Error al importar productos';
      toast.error(errorMessage);
    } finally {
      setIsImporting(false);
    }
  };

  // Clear current file
  const handleClearFile = () => {
    setFile(null);
    setPreviewData([]);
    setValidationResult(null);
    setImportResult(null);
  };

  // Check if can import
  const canImport = file && validationResult?.success && !isImporting && !isValidating;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Importar Productos</h1>
          <p className="text-muted-foreground">
            Carga productos de forma masiva desde un archivo Excel
          </p>
        </div>
        <Button variant="outline" onClick={handleDownloadTemplate}>
          <Download className="mr-2 size-4" />
          Descargar Plantilla
        </Button>
      </div>

      {/* Drop Zone */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileSpreadsheet className="size-5" />
            Archivo Excel
          </CardTitle>
          <CardDescription>
            Arrastra y suelta un archivo Excel (.xlsx, .xls) o haz clic para seleccionar
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div
            {...getRootProps()}
            className={cn(
              'relative flex min-h-[200px] cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed transition-colors',
              isDragActive && !isDragReject && 'border-primary bg-primary/5',
              isDragReject && 'border-destructive bg-destructive/5',
              !isDragActive && 'border-muted-foreground/25 hover:border-primary/50',
              (isValidating || isImporting) && 'pointer-events-none opacity-50'
            )}
          >
            <input {...getInputProps()} />
            
            {isValidating ? (
              <div className="flex flex-col items-center gap-3">
                <Loader2 className="size-10 animate-spin text-primary" />
                <p className="text-sm text-muted-foreground">Validando archivo...</p>
              </div>
            ) : isImporting ? (
              <div className="flex flex-col items-center gap-3">
                <Loader2 className="size-10 animate-spin text-primary" />
                <p className="text-sm text-muted-foreground">Importando productos...</p>
                <Progress value={33} className="w-48" />
              </div>
            ) : file ? (
              <div className="flex flex-col items-center gap-3">
                <FileSpreadsheet className="size-10 text-green-600" />
                <div className="text-center">
                  <p className="font-medium">{file.name}</p>
                  <p className="text-sm text-muted-foreground">
                    {(file.size / 1024).toFixed(1)} KB
                  </p>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleClearFile();
                  }}
                >
                  <X className="mr-1 size-4" />
                  Quitar archivo
                </Button>
              </div>
            ) : isDragReject ? (
              <div className="flex flex-col items-center gap-3 text-destructive">
                <AlertCircle className="size-10" />
                <p className="text-sm">Formato de archivo no válido</p>
                <p className="text-xs">Solo se aceptan archivos .xlsx y .xls</p>
              </div>
            ) : (
              <div className="flex flex-col items-center gap-3">
                <Upload className="size-10 text-muted-foreground" />
                <div className="text-center">
                  <p className="text-sm font-medium">
                    {isDragActive ? 'Suelta el archivo aquí' : 'Arrastra y suelta un archivo Excel'}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    o haz clic para seleccionar (máx. 10 MB)
                  </p>
                </div>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Preview Section */}
      {file && previewData.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Eye className="size-5" />
              Vista Previa
            </CardTitle>
            <CardDescription>
              Primeras {previewData.length} filas del archivo
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>SKU</TableHead>
                    <TableHead>Nombre</TableHead>
                    <TableHead>Descripción</TableHead>
                    <TableHead className="text-right">Precio</TableHead>
                    <TableHead>Colección</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {previewData.map((row, index) => (
                    <TableRow key={index}>
                      <TableCell className="font-mono text-sm">{row.sku || '-'}</TableCell>
                      <TableCell>{row.name || '-'}</TableCell>
                      <TableCell className="max-w-[200px] truncate text-muted-foreground">
                        {row.description || '-'}
                      </TableCell>
                      <TableCell className="text-right font-mono">
                        {typeof row.price === 'number' 
                          ? `$${row.price.toFixed(2)}`
                          : row.price || '-'}
                      </TableCell>
                      <TableCell>{row.collection || '-'}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
            {isLoadingPreview && (
              <div className="mt-2 flex items-center gap-2 text-sm text-muted-foreground">
                <Loader2 className="size-4 animate-spin" />
                Cargando vista previa...
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Validation Results */}
      {validationResult && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              {validationResult.success ? (
                <>
                  <CheckCircle2 className="size-5 text-green-600" />
                  <span className="text-green-600">Validación Exitosa</span>
                </>
              ) : (
                <>
                  <AlertCircle className="size-5 text-destructive" />
                  <span className="text-destructive">Errores de Validación</span>
                </>
              )}
            </CardTitle>
            <CardDescription>
              {validationResult.totalRows} filas encontradas en el archivo
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {/* Summary badges */}
            <div className="flex flex-wrap gap-2">
              <Badge variant="outline">
                Total: {validationResult.totalRows} productos
              </Badge>
              {validationResult.success && (
                <Badge variant="secondary" className="bg-green-100 text-green-800">
                  Listo para importar
                </Badge>
              )}
              {validationResult.errors.length > 0 && (
                <Badge variant="destructive">
                  {validationResult.errors.length} error(es)
                </Badge>
              )}
            </div>

            {/* Error table */}
            {validationResult.errors.length > 0 && (
              <div className="rounded-md border">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead className="w-20">Fila</TableHead>
                      <TableHead className="w-32">Campo</TableHead>
                      <TableHead>Error</TableHead>
                      <TableHead className="w-32">Valor</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {validationResult.errors.slice(0, 20).map((error, index) => (
                      <TableRow key={index}>
                        <TableCell className="font-mono">{error.rowNumber}</TableCell>
                        <TableCell>
                          <Badge variant="outline">{error.field}</Badge>
                        </TableCell>
                        <TableCell className="text-destructive">{error.message}</TableCell>
                        <TableCell className="font-mono text-xs">
                          {error.value || '-'}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
                {validationResult.errors.length > 20 && (
                  <div className="border-t bg-muted/50 p-2 text-center text-sm text-muted-foreground">
                    ... y {validationResult.errors.length - 20} errores más
                  </div>
                )}
              </div>
            )}

            {/* Import button */}
            {validationResult.success && (
              <div className="flex justify-end">
                <Button 
                  onClick={handleImportClick} 
                  disabled={!canImport}
                  size="lg"
                >
                  <Upload className="mr-2 size-4" />
                  Importar Productos
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Import Results */}
      {importResult && importResult.success && (
        <Card className="border-green-200 bg-green-50">
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-green-700">
              <CheckCircle2 className="size-5" />
              Importación Completada
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 sm:grid-cols-3">
              <div className="rounded-lg bg-white p-4 text-center shadow-sm">
                <p className="text-3xl font-bold text-green-600">{importResult.createdCount}</p>
                <p className="text-sm text-muted-foreground">Productos creados</p>
              </div>
              <div className="rounded-lg bg-white p-4 text-center shadow-sm">
                <p className="text-3xl font-bold text-blue-600">{importResult.updatedCount}</p>
                <p className="text-sm text-muted-foreground">Productos actualizados</p>
              </div>
              <div className="rounded-lg bg-white p-4 text-center shadow-sm">
                <p className="text-3xl font-bold text-purple-600">{importResult.collectionsCreatedCount}</p>
                <p className="text-sm text-muted-foreground">Colecciones creadas</p>
              </div>
            </div>

            {importResult.warnings.length > 0 && (
              <div className="mt-4 rounded-md border border-yellow-200 bg-yellow-50 p-4">
                <div className="flex items-center gap-2 text-yellow-800">
                  <AlertTriangle className="size-4" />
                  <span className="font-medium">Advertencias:</span>
                </div>
                <ul className="mt-2 list-inside list-disc text-sm text-yellow-700">
                  {importResult.warnings.map((warning, index) => (
                    <li key={index}>{warning}</li>
                  ))}
                </ul>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Confirmation Dialog */}
      <Dialog open={showConfirmDialog} onOpenChange={setShowConfirmDialog}>
        <DialogContent className="sm:max-w-[500px]">
          <DialogHeader>
            <DialogTitle>Confirmar Importación</DialogTitle>
            <DialogDescription>
              ¿Estás seguro de que deseas importar los productos del archivo{' '}
              <strong>{file?.name}</strong>?
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            {/* Summary breakdown */}
            <div className="rounded-lg border bg-muted/30 p-4">
              <h4 className="mb-3 font-medium">Resumen de la importación</h4>
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <span className="flex items-center gap-2 text-sm">
                    <Plus className="size-4 text-green-600" />
                    Productos nuevos a crear
                  </span>
                  <Badge variant="secondary" className="bg-green-100 text-green-800">
                    {validationResult?.createdCount ?? 0}
                  </Badge>
                </div>
                <div className="flex items-center justify-between">
                  <span className="flex items-center gap-2 text-sm">
                    <RefreshCw className="size-4 text-blue-600" />
                    Productos existentes a actualizar
                  </span>
                  <Badge variant="secondary" className="bg-blue-100 text-blue-800">
                    {validationResult?.updatedCount ?? 0}
                  </Badge>
                </div>
                <hr className="my-2" />
                <div className="flex items-center justify-between font-medium">
                  <span className="text-sm">Total de productos</span>
                  <Badge variant="outline">
                    {validationResult?.totalRows ?? 0}
                  </Badge>
                </div>
              </div>
            </div>

            {/* Warnings if any collections will be created */}
            {validationResult && validationResult.collectionsCreatedCount > 0 && (
              <div className="flex items-start gap-2 rounded-md border border-yellow-200 bg-yellow-50 p-3">
                <AlertTriangle className="mt-0.5 size-4 text-yellow-600" />
                <div className="text-sm text-yellow-800">
                  Se crearán <strong>{validationResult.collectionsCreatedCount}</strong> colecciones nuevas.
                </div>
              </div>
            )}

            <p className="text-xs text-muted-foreground">
              Los productos con SKU existente serán actualizados con los nuevos valores.
              Los productos con SKU nuevo serán creados.
            </p>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowConfirmDialog(false)}>
              Cancelar
            </Button>
            <Button onClick={handleConfirmImport}>
              <Upload className="mr-2 size-4" />
              Confirmar Importación
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

export default ProductImportPage;




