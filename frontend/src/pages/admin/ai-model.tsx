/**
 * AI Model Management Page (EP4)
 * Admin dashboard for managing ML model training and health metrics
 */
import { useState, useEffect } from 'react';
import { 
  Brain, 
  AlertTriangle, 
  CheckCircle2, 
  AlertCircle, 
  Info,
  RefreshCw,
  Upload,
  Image,
  Package,
  Clock,
  TrendingUp,
  Sparkles,
} from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Progress } from '@/components/ui/progress';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';

import { imageRecognitionService } from '@/services/sales.service';
import { modelTrainingService } from '@/services/model-training.service';
import type { ModelHealth, ModelMetadata, EmbeddingsStatusResponse } from '@/types/sales.types';
import type { TrainingProgress, EmbeddingGenerationProgress } from '@/services/model-training.service';

type AlertLevel = 'OK' | 'RECOMMENDED' | 'HIGH' | 'CRITICAL';

export function AIModelPage() {
  const [loading, setLoading] = useState(true);
  const [modelHealth, setModelHealth] = useState<ModelHealth | null>(null);
  const [modelVersions, setModelVersions] = useState<ModelMetadata[]>([]);
  const [trainingInProgress, setTrainingInProgress] = useState(false);
  const [trainingProgress, setTrainingProgress] = useState<TrainingProgress | null>(null);
  const [trainingError, setTrainingError] = useState<string | null>(null);
  const [embeddingsStatus, setEmbeddingsStatus] = useState<EmbeddingsStatusResponse | null>(null);
  const [embeddingGenerationInProgress, setEmbeddingGenerationInProgress] = useState(false);
  const [embeddingGenerationProgress, setEmbeddingGenerationProgress] = useState<EmbeddingGenerationProgress | null>(null);
  const [embeddingGenerationError, setEmbeddingGenerationError] = useState<string | null>(null);

  // Load model health, versions, and embeddings status
  useEffect(() => {
    const loadData = async () => {
      try {
        const [health, versions] = await Promise.all([
          imageRecognitionService.getModelHealth(),
          imageRecognitionService.getModelVersions(),
        ]);
        setModelHealth(health);
        setModelVersions(versions);
      } catch (error) {
        console.error('Error loading model data:', error);
        // Set default health if endpoint fails (no model yet)
        setModelHealth({
          alertLevel: 'CRITICAL',
          alertMessage: 'No hay modelo de IA disponible. Entrene el primer modelo.',
          catalogMetrics: {
            totalProducts: 0,
            productsWithPhotos: 0,
            productsWithoutPhotos: 0,
            newProductsSinceTraining: 0,
            newProductsPercentage: 0,
          },
          photoMetrics: {
            totalPhotos: 0,
            photosAddedSinceTraining: 0,
            photosDeletedSinceTraining: 0,
            netChangePercentage: 0,
          },
        });
      } finally {
        setLoading(false);
      }
    };

    const loadEmbeddingsStatus = async () => {
      try {
        const status = await imageRecognitionService.getEmbeddingsStatus();
        setEmbeddingsStatus(status);
      } catch {
        // Non-critical — just leave null
      }
    };

    loadData();
    loadEmbeddingsStatus();
  }, []);

  // Prevent accidental tab close during training
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (trainingInProgress) {
        e.preventDefault();
        e.returnValue = 'El entrenamiento está en progreso. ¿Estás seguro de que deseas cerrar esta pestaña? Se perderá todo el progreso.';
        return e.returnValue;
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    
    return () => {
      window.removeEventListener('beforeunload', handleBeforeUnload);
    };
  }, [trainingInProgress]);

  // Get alert level badge variant
  const getAlertBadge = (level: AlertLevel) => {
    switch (level) {
      case 'CRITICAL':
        return <Badge variant="destructive" className="gap-1"><AlertTriangle className="h-3 w-3" /> Crítico</Badge>;
      case 'HIGH':
        return <Badge variant="destructive" className="gap-1 bg-orange-500"><AlertCircle className="h-3 w-3" /> Alto</Badge>;
      case 'RECOMMENDED':
        return <Badge variant="secondary" className="gap-1 bg-yellow-500 text-white"><Info className="h-3 w-3" /> Recomendado</Badge>;
      case 'OK':
        return <Badge variant="default" className="gap-1 bg-green-500"><CheckCircle2 className="h-3 w-3" /> OK</Badge>;
    }
  };

  // Format date
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-ES', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  // Handle training start
  const handleStartTraining = async () => {
    try {
      // Check capabilities first
      const capabilities = await modelTrainingService.checkTrainingCapabilities();
      
      if (!capabilities.hasWebGL2) {
        toast.error('Tu dispositivo no soporta WebGL 2.0. El entrenamiento puede ser muy lento o fallar.');
        return;
      }

      if (capabilities.memoryMB < 2048) {
        toast.warning('Tu dispositivo tiene poca memoria. Se recomienda cerrar otras pestañas.');
      }

      // Confirm training start
      const confirmed = window.confirm(
        `Duración estimada: ${capabilities.estimatedDuration}\n\n` +
        `⚠️ IMPORTANTE: Mantén esta pestaña abierta durante el entrenamiento.\n\n` +
        `¿Deseas continuar?`
      );

      if (!confirmed) return;

      setTrainingInProgress(true);
      setTrainingError(null);
      setTrainingProgress(null);

      // Start training
      await modelTrainingService.executeClientSideTraining((progress) => {
        setTrainingProgress(progress);
      });

      // Training complete
      toast.success('¡Modelo entrenado exitosamente!');
      
      // Reload model health and versions
      const [health, versions] = await Promise.all([
        imageRecognitionService.getModelHealth(),
        imageRecognitionService.getModelVersions(),
      ]);
      setModelHealth(health);
      setModelVersions(versions);
    } catch (error: unknown) {
      console.error('Training error:', error);
      const errorMessage = (error as { message?: string }).message || 'Error durante el entrenamiento';
      setTrainingError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setTrainingInProgress(false);
    }
  };

  const handleGenerateEmbeddings = async () => {
    const confirmed = window.confirm(
      'Se generarán embeddings para todas las fotos del catálogo (~30-60 segundos).\n\n' +
      '⚠️ Los embeddings existentes serán reemplazados.\n\n' +
      '¿Deseas continuar?'
    );
    if (!confirmed) return;

    setEmbeddingGenerationInProgress(true);
    setEmbeddingGenerationError(null);
    setEmbeddingGenerationProgress(null);

    try {
      await modelTrainingService.executeEmbeddingGeneration((progress) => {
        setEmbeddingGenerationProgress(progress);
      });

      toast.success('¡Embeddings generados exitosamente!');

      const status = await imageRecognitionService.getEmbeddingsStatus();
      setEmbeddingsStatus(status);
    } catch (error: unknown) {
      const errorMessage = (error as { message?: string }).message || 'Error al generar embeddings';
      setEmbeddingGenerationError(errorMessage);
      toast.error(errorMessage);
    } finally {
      setEmbeddingGenerationInProgress(false);
    }
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-48" />
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4].map((i) => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
        <Skeleton className="h-64" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2">
            <Brain className="h-6 w-6" />
            Modelo de IA
          </h1>
          <p className="text-muted-foreground">
            Gestión del modelo de reconocimiento de imágenes
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={handleGenerateEmbeddings}
            disabled={trainingInProgress || embeddingGenerationInProgress}
          >
            {embeddingGenerationInProgress ? (
              <>
                <RefreshCw className="mr-2 h-4 w-4 animate-spin" />
                Generando...
              </>
            ) : (
              <>
                <Sparkles className="mr-2 h-4 w-4" />
                Generar Embeddings
              </>
            )}
          </Button>
          <Button 
            onClick={handleStartTraining}
            disabled={trainingInProgress || embeddingGenerationInProgress}
          >
            {trainingInProgress ? (
              <>
                <RefreshCw className="mr-2 h-4 w-4 animate-spin" />
                Entrenando...
              </>
            ) : (
              <>
                <Upload className="mr-2 h-4 w-4" />
                Entrenar Modelo
              </>
            )}
          </Button>
        </div>
      </div>

      {/* Alert Banner */}
      {modelHealth && modelHealth.alertLevel !== 'OK' && (
        <Alert variant={modelHealth.alertLevel === 'CRITICAL' ? 'destructive' : 'default'}>
          <AlertTriangle className="h-4 w-4" />
          <AlertTitle>
            {modelHealth.alertLevel === 'CRITICAL' && 'Acción Requerida'}
            {modelHealth.alertLevel === 'HIGH' && 'Alta Prioridad'}
            {modelHealth.alertLevel === 'RECOMMENDED' && 'Recomendación'}
          </AlertTitle>
          <AlertDescription>
            {modelHealth.alertMessage}
          </AlertDescription>
        </Alert>
      )}

      {/* Metrics Cards */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-5">
        {/* Model Status */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Estado del Modelo</CardTitle>
            <Brain className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {modelHealth?.currentVersion && modelHealth?.lastTrainedAt ? (
              <>
                <div className="text-2xl font-bold">{modelHealth.currentVersion}</div>
                <p className="text-xs text-muted-foreground">
                  Entrenado {formatDate(modelHealth.lastTrainedAt)}
                </p>
                <div className="mt-2">
                  {getAlertBadge(modelHealth.alertLevel)}
                </div>
              </>
            ) : (
              <>
                <div className="text-2xl font-bold text-muted-foreground">Sin modelo</div>
                <p className="text-xs text-muted-foreground">
                  Entrene el primer modelo
                </p>
                <div className="mt-2">
                  {getAlertBadge('CRITICAL')}
                </div>
              </>
            )}
          </CardContent>
        </Card>

        {/* Products */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Productos</CardTitle>
            <Package className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {modelHealth?.catalogMetrics.totalProducts || 0}
            </div>
            <p className="text-xs text-muted-foreground">
              {modelHealth?.catalogMetrics.productsWithPhotos || 0} con fotos
            </p>
            {modelHealth && modelHealth.catalogMetrics.newProductsSinceTraining > 0 && (
              <Badge variant="secondary" className="mt-2">
                +{modelHealth.catalogMetrics.newProductsSinceTraining} nuevos
              </Badge>
            )}
          </CardContent>
        </Card>

        {/* Photos */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Fotos</CardTitle>
            <Image className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {modelHealth?.photoMetrics.totalPhotos || 0}
            </div>
            <p className="text-xs text-muted-foreground">
              {modelHealth?.photoMetrics.photosAddedSinceTraining || 0} añadidas desde entrenamiento
            </p>
            {modelHealth && modelHealth.photoMetrics.netChangePercentage !== 0 && (
              <Badge variant={modelHealth.photoMetrics.netChangePercentage > 0 ? 'default' : 'secondary'} className="mt-2">
                {modelHealth.photoMetrics.netChangePercentage > 0 ? '+' : ''}
                {modelHealth.photoMetrics.netChangePercentage.toFixed(1)}%
              </Badge>
            )}
          </CardContent>
        </Card>

        {/* Model Age */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Antigüedad</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {modelHealth?.daysSinceTraining !== null && modelHealth?.daysSinceTraining !== undefined ? (
              <>
                <div className="text-2xl font-bold">
                  {modelHealth.daysSinceTraining} días
                </div>
                <p className="text-xs text-muted-foreground">
                  desde último entrenamiento
                </p>
                {modelHealth.daysSinceTraining > 60 && (
                  <Badge variant="destructive" className="mt-2">
                    Modelo desactualizado
                  </Badge>
                )}
              </>
            ) : (
              <>
                <div className="text-2xl font-bold text-muted-foreground">N/A</div>
                <p className="text-xs text-muted-foreground">
                  Sin modelo entrenado
                </p>
              </>
            )}
          </CardContent>
        </Card>

        {/* Embeddings Index */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Embeddings</CardTitle>
            <Sparkles className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {embeddingsStatus && embeddingsStatus.count > 0 ? (
              <>
                <div className="text-2xl font-bold">{embeddingsStatus.count}</div>
                <p className="text-xs text-muted-foreground">
                  {embeddingsStatus.lastUpdated
                    ? `Actualizado ${formatDate(embeddingsStatus.lastUpdated)}`
                    : 'Generados'}
                </p>
                <Badge variant="default" className="mt-2 bg-green-500 gap-1">
                  <CheckCircle2 className="h-3 w-3" /> Activos
                </Badge>
              </>
            ) : (
              <>
                <div className="text-2xl font-bold text-muted-foreground">0</div>
                <p className="text-xs text-muted-foreground">
                  No hay embeddings generados
                </p>
                <Badge variant="secondary" className="mt-2">Sin embeddings</Badge>
              </>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Embedding Generation Progress (shown when generating) */}
      {embeddingGenerationInProgress && embeddingGenerationProgress && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <RefreshCw className="h-5 w-5 animate-spin" />
              Generando Embeddings
            </CardTitle>
            <CardDescription>
              Mantenga esta pestaña abierta durante la generación
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <div className="flex items-center justify-between text-sm">
                <span>{embeddingGenerationProgress.message}</span>
                <span>
                  {embeddingGenerationProgress.total > 0
                    ? `${embeddingGenerationProgress.current}/${embeddingGenerationProgress.total}`
                    : ''}
                </span>
              </div>
              <Progress
                value={
                  embeddingGenerationProgress.total > 0
                    ? Math.round((embeddingGenerationProgress.current / embeddingGenerationProgress.total) * 100)
                    : 0
                }
              />
            </div>
            <Alert>
              <AlertTriangle className="h-4 w-4" />
              <AlertTitle>⚠️ No cierre esta pestaña</AlertTitle>
              <AlertDescription>
                La generación de embeddings se ejecuta en su navegador. Cerrar la pestaña cancelará el proceso.
              </AlertDescription>
            </Alert>
          </CardContent>
        </Card>
      )}

      {/* Embedding Generation Error */}
      {embeddingGenerationError && (
        <Alert variant="destructive">
          <AlertTriangle className="h-4 w-4" />
          <AlertTitle>Error al Generar Embeddings</AlertTitle>
          <AlertDescription>
            {embeddingGenerationError}
          </AlertDescription>
        </Alert>
      )}

      {/* Training Progress (shown when training) */}
      {trainingInProgress && trainingProgress && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <RefreshCw className="h-5 w-5 animate-spin" />
              Entrenamiento en Progreso
            </CardTitle>
            <CardDescription>
              Mantenga esta pestaña abierta durante el entrenamiento
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <div className="flex items-center justify-between text-sm">
                <span>{trainingProgress.message}</span>
                <span>{trainingProgress.progress}%</span>
              </div>
              <Progress value={trainingProgress.progress} />
            </div>
            
            {trainingProgress.currentEpoch && trainingProgress.totalEpochs && (
              <div className="rounded-lg bg-muted p-4 space-y-2">
                <div className="flex items-center justify-between text-sm">
                  <span>Época:</span>
                  <span className="font-mono">{trainingProgress.currentEpoch}/{trainingProgress.totalEpochs}</span>
                </div>
                {trainingProgress.accuracy && (
                  <div className="flex items-center justify-between text-sm">
                    <span>Precisión:</span>
                    <span className="font-mono">{trainingProgress.accuracy.toFixed(1)}%</span>
                  </div>
                )}
                {trainingProgress.loss && (
                  <div className="flex items-center justify-between text-sm">
                    <span>Pérdida:</span>
                    <span className="font-mono">{trainingProgress.loss.toFixed(4)}</span>
                  </div>
                )}
              </div>
            )}

            <Alert>
              <AlertTriangle className="h-4 w-4" />
              <AlertTitle>⚠️ No cierre esta pestaña</AlertTitle>
              <AlertDescription>
                El entrenamiento se ejecuta en su navegador. Cerrar la pestaña cancelará el proceso.
              </AlertDescription>
            </Alert>
          </CardContent>
        </Card>
      )}

      {/* Training Error */}
      {trainingError && (
        <Alert variant="destructive">
          <AlertTriangle className="h-4 w-4" />
          <AlertTitle>Error de Entrenamiento</AlertTitle>
          <AlertDescription>
            {trainingError}
          </AlertDescription>
        </Alert>
      )}

      {/* Model Versions */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="h-5 w-5" />
            Historial de Versiones
          </CardTitle>
          <CardDescription>
            Versiones anteriores del modelo de IA
          </CardDescription>
        </CardHeader>
        <CardContent>
          {modelVersions.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Brain className="mb-4 h-12 w-12 text-muted-foreground" />
              <p className="text-muted-foreground">
                No hay versiones de modelo disponibles
              </p>
              <Button className="mt-4" onClick={handleStartTraining}>
                Entrenar Primer Modelo
              </Button>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Versión</TableHead>
                  <TableHead>Fecha</TableHead>
                  <TableHead>Productos</TableHead>
                  <TableHead>Fotos</TableHead>
                  <TableHead>Estado</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {modelVersions.map((version) => (
                  <TableRow key={version.version}>
                    <TableCell className="font-mono font-medium">
                      {version.version}
                    </TableCell>
                    <TableCell>{formatDate(version.trainedAt)}</TableCell>
                    <TableCell>{version.totalProductsUsed}</TableCell>
                    <TableCell>{version.totalPhotosUsed}</TableCell>
                    <TableCell>
                      {version.isActive ? (
                        <Badge variant="default" className="bg-green-500">Activo</Badge>
                      ) : (
                        <Badge variant="secondary">Anterior</Badge>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Browser Training Info */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Info className="h-5 w-5" />
            Sobre el Entrenamiento
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">
            El entrenamiento del modelo se ejecuta directamente en su navegador usando TensorFlow.js.
            No se requiere Python ni configuración adicional.
          </p>
          
          <div className="grid gap-4 md:grid-cols-2">
            <div className="rounded-lg border p-4">
              <h4 className="font-semibold mb-2">Requisitos</h4>
              <ul className="text-sm text-muted-foreground space-y-1">
                <li>✅ Navegador moderno (Chrome 90+, Edge 90+, Safari 14+)</li>
                <li>✅ WebGL 2.0 (aceleración GPU)</li>
                <li>✅ 8GB+ RAM recomendado</li>
                <li>✅ Conexión a internet estable</li>
              </ul>
            </div>
            
            <div className="rounded-lg border p-4">
              <h4 className="font-semibold mb-2">Duración Estimada</h4>
              <ul className="text-sm text-muted-foreground space-y-1">
                <li>🚀 Con GPU (WebGL): 15-20 minutos</li>
                <li>🐢 Sin GPU (WASM): 45-60 minutos</li>
                <li>📦 Descarga de fotos: 2-5 minutos</li>
                <li>☁️ Subida del modelo: 1-2 minutos</li>
              </ul>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export default AIModelPage;
