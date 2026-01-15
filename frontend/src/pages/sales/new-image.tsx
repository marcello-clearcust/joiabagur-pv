/**
 * Sales with Image Recognition Page (EP4 - Section 12)
 * Allows operators to take a photo and get AI-powered product suggestions.
 */
import { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { ArrowLeft, Camera, X, RefreshCw, AlertTriangle, CheckCircle2, ShoppingCart } from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Progress } from '@/components/ui/progress';

import { useAuth } from '@/providers/auth-provider';
import { imageRecognitionInferenceService } from '@/services/image-recognition.service';
import { ROUTES } from '@/routing/routes';
import type { ProductSuggestion } from '@/types/sales.types';

export function ImageRecognitionSalesPage() {
  const navigate = useNavigate();
  useAuth();
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const capturedImageObjectUrlRef = useRef<string | null>(null);

  // State
  const [loading, setLoading] = useState(true);
  const [deviceCompatible, setDeviceCompatible] = useState(true);
  const [cameraActive, setCameraActive] = useState(false);
  const [capturedImage, setCapturedImage] = useState<string | null>(null);
  const [processing, setProcessing] = useState(false);
  const [processingProgress, setProcessingProgress] = useState(0);
  const [suggestions, setSuggestions] = useState<ProductSuggestion[]>([]);

  // Check device compatibility
  useEffect(() => {
    const checkCompatibility = async () => {
      try {
        const capabilities = await imageRecognitionInferenceService.checkDeviceCompatibility();
        
        if (!capabilities.hasTensorFlowJS || !capabilities.hasWebGL) {
          setDeviceCompatible(false);
          toast.error('Tu dispositivo no es compatible con reconocimiento de imágenes');
        }
      } catch (error) {
        console.error('Compatibility check failed:', error);
        setDeviceCompatible(false);
      } finally {
        setLoading(false);
      }
    };
    
    checkCompatibility();
  }, []);

  // Start camera
  const startCamera = useCallback(async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { 
          facingMode: 'environment', // Prefer back camera on mobile
          width: { ideal: 1280 },
          height: { ideal: 720 },
        },
      });
      
      if (videoRef.current) {
        videoRef.current.srcObject = stream;
        videoRef.current.play();
        setCameraActive(true);
      }
    } catch (error) {
      console.error('Camera access error:', error);
      toast.error('No se pudo acceder a la cámara');
    }
  }, []);

  // Stop camera
  const stopCamera = useCallback(() => {
    if (videoRef.current && videoRef.current.srcObject) {
      const stream = videoRef.current.srcObject as MediaStream;
      stream.getTracks().forEach((track) => track.stop());
      videoRef.current.srcObject = null;
      setCameraActive(false);
    }
  }, []);

  // Capture photo from camera
  const capturePhoto = useCallback(() => {
    if (!videoRef.current || !canvasRef.current) return;

    const video = videoRef.current;
    const canvas = canvasRef.current;
    const context = canvas.getContext('2d');

    if (!context) return;

    // Set canvas dimensions to match video
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;

    // Draw video frame to canvas
    context.drawImage(video, 0, 0, canvas.width, canvas.height);

    // Get image as data URL
    const imageDataUrl = canvas.toDataURL('image/jpeg', 0.9);
    setCapturedImage(imageDataUrl);
    stopCamera();

    // Convert to file and process
    canvas.toBlob((blob) => {
      if (blob) {
        const file = new File([blob], 'captured-photo.jpg', { type: 'image/jpeg' });
        processImage(file, imageDataUrl);
      }
    }, 'image/jpeg', 0.9);
  }, [stopCamera]);

  // Handle file upload
  const handleFileUpload = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file
    const validation = imageRecognitionInferenceService.validateImage(file);
    if (!validation.isValid) {
      toast.error(validation.error);
      // Allow selecting the same file again
      event.target.value = '';
      return;
    }

    // Show preview immediately (FileReader can be slow / fail silently on some devices)
    // Revoke previous object URL if any
    if (capturedImageObjectUrlRef.current) {
      URL.revokeObjectURL(capturedImageObjectUrlRef.current);
      capturedImageObjectUrlRef.current = null;
    }
    const objectUrl = URL.createObjectURL(file);
    capturedImageObjectUrlRef.current = objectUrl;
    setCapturedImage(objectUrl);

    // Process image
    processImage(file, objectUrl);

    // Allow selecting the same file again
    event.target.value = '';
  }, []);

  // Process image with AI
  const processImage = async (file: File, previewUrl?: string | null) => {
    setProcessing(true);
    setProcessingProgress(0);
    setSuggestions([]);

    let progressInterval: number | undefined;
    try {
      // Simulate progress updates
      progressInterval = window.setInterval(() => {
        setProcessingProgress((prev) => Math.min(prev + 10, 90));
      }, 200);

      // Run inference
      const productSuggestions = await imageRecognitionInferenceService.recognizeProduct(file);
      setProcessingProgress(100);

      if (productSuggestions.length === 0) {
        // No suggestions above threshold - redirect to manual entry
        toast.warning('No se encontraron coincidencias fiables. Redirigiendo a registro manual...');
        setTimeout(() => {
          navigate(ROUTES.SALES.NEW, { 
            state: { photoDataUrl: previewUrl ?? capturedImage } 
          });
        }, 2000);
        return;
      }

      setSuggestions(productSuggestions);
      toast.success(`Se encontraron ${productSuggestions.length} sugerencias`);
    } catch (error: unknown) {
      console.error('Image processing error:', error);
      const errorMessage = (error as { message?: string }).message || 'Error al procesar la imagen';
      // Single toast with action to avoid overlapping notifications
      toast.error(errorMessage, {
        action: {
          label: 'Registrar Manual',
          onClick: () => navigate(ROUTES.SALES.NEW),
        },
      });
    } finally {
      if (progressInterval) {
        window.clearInterval(progressInterval);
      }
      setProcessing(false);
    }
  };

  // Handle suggestion selection
  const handleSelectSuggestion = (suggestion: ProductSuggestion) => {
    // Navigate to manual entry with pre-selected product
    navigate(ROUTES.SALES.NEW, {
      state: {
        productId: suggestion.productId,
        photoDataUrl: capturedImage,
      },
    });
  };

  // Reset flow
  const resetFlow = () => {
    if (capturedImageObjectUrlRef.current) {
      URL.revokeObjectURL(capturedImageObjectUrlRef.current);
      capturedImageObjectUrlRef.current = null;
    }
    setCapturedImage(null);
    setSuggestions([]);
    setProcessing(false);
    setProcessingProgress(0);
  };

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      stopCamera();
    };
  }, [stopCamera]);

  if (loading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-96 w-full" />
      </div>
    );
  }

  if (!deviceCompatible) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link to={ROUTES.SALES.ROOT}>
              <ArrowLeft className="h-5 w-5" />
            </Link>
          </Button>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Reconocimiento de Imagen</h1>
            <p className="text-muted-foreground">
              Tu dispositivo no es compatible
            </p>
          </div>
        </div>

        <Alert variant="destructive">
          <AlertTriangle className="h-4 w-4" />
          <AlertTitle>Dispositivo No Compatible</AlertTitle>
          <AlertDescription>
            Tu dispositivo no es compatible con reconocimiento de imágenes.
            <br />
            <br />
            <strong>Requisitos:</strong>
            <ul className="mt-2 list-disc list-inside space-y-1">
              <li>iOS 12+ o Android 8.0+</li>
              <li>Navegador moderno (Chrome 90+, Safari 14+, Edge 90+)</li>
              <li>Soporte WebGL 2.0</li>
            </ul>
          </AlertDescription>
        </Alert>

        <Button asChild>
          <Link to={ROUTES.SALES.NEW}>
            Registrar Venta Manual
          </Link>
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link to={ROUTES.SALES.ROOT}>
            <ArrowLeft className="h-5 w-5" />
          </Link>
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Reconocimiento de Imagen</h1>
          <p className="text-muted-foreground">
            Toma una foto del producto para identificarlo
          </p>
        </div>
      </div>

      {/* Camera / Photo Capture */}
      {!capturedImage && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Camera className="h-5 w-5" />
              Capturar Foto
            </CardTitle>
            <CardDescription>
              Usa la cámara o sube una foto del producto
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {!cameraActive ? (
              <div className="flex flex-col gap-3">
                <Button onClick={startCamera} size="lg" className="w-full">
                  <Camera className="mr-2 h-5 w-5" />
                  Abrir Cámara
                </Button>
                <div className="relative">
                  <div className="absolute inset-0 flex items-center">
                    <span className="w-full border-t" />
                  </div>
                  <div className="relative flex justify-center text-xs uppercase">
                    <span className="bg-background px-2 text-muted-foreground">O</span>
                  </div>
                </div>
                <Button 
                  variant="outline" 
                  size="lg" 
                  className="w-full"
                  onClick={() => fileInputRef.current?.click()}
                >
                  Subir Foto
                </Button>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/*"
                  className="hidden"
                  onChange={handleFileUpload}
                />
              </div>
            ) : (
              <div className="space-y-3">
                <video
                  ref={videoRef}
                  className="w-full rounded-lg bg-black"
                  autoPlay
                  playsInline
                />
                <div className="flex gap-2">
                  <Button onClick={capturePhoto} size="lg" className="flex-1">
                    <Camera className="mr-2 h-5 w-5" />
                    Capturar
                  </Button>
                  <Button 
                    variant="outline" 
                    onClick={stopCamera}
                  >
                    <X className="h-5 w-5" />
                  </Button>
                </div>
              </div>
            )}
            <canvas ref={canvasRef} className="hidden" />
          </CardContent>
        </Card>
      )}

      {/* Processing / Results */}
      {capturedImage && (
        <>
          {/* Processing Indicator */}
          {processing && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <RefreshCw className="h-5 w-5 animate-spin" />
                  Procesando Imagen...
                </CardTitle>
                <CardDescription>
                  Analizando la foto con inteligencia artificial
                </CardDescription>
              </CardHeader>
              <CardContent>
                <Progress value={processingProgress} className="h-2" />
                <p className="mt-2 text-center text-sm text-muted-foreground">
                  {processingProgress}%
                </p>
              </CardContent>
            </Card>
          )}

          {/* Suggestions - shown before the image */}
          {!processing && suggestions.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Sugerencias de Productos</CardTitle>
                <CardDescription>
                  Selecciona el producto correcto o registra manualmente
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-3">
                {suggestions.map((suggestion) => (
                  <button
                    key={suggestion.productId}
                    type="button"
                    className="flex w-full items-center justify-between rounded-lg border p-4 text-left transition-colors hover:bg-muted"
                    onClick={() => handleSelectSuggestion(suggestion)}
                  >
                    <div className="flex items-center gap-3">
                      <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-primary/10">
                        <ShoppingCart className="h-6 w-6 text-primary" />
                      </div>
                      <div>
                        <div className="flex items-center gap-2">
                          <h4 className="font-semibold">{suggestion.productName}</h4>
                          <p className="text-sm text-muted-foreground">
                            SKU: {suggestion.productSku}
                          </p>
                        </div>
                        <Badge 
                          variant={suggestion.confidence >= 70 ? 'primary' : 'secondary'}
                          className="mt-1"
                        >
                          {suggestion.confidence.toFixed(0)}% confianza
                        </Badge>
                      </div>
                    </div>
                    <ShoppingCart className="h-5 w-5 text-muted-foreground" />
                  </button>
                ))}

                <Button 
                  variant="outline" 
                  className="w-full mt-4"
                  onClick={() => navigate(ROUTES.SALES.NEW, { state: { photoDataUrl: capturedImage } })}
                >
                  Ninguna es correcta - Registrar Manual
                </Button>
              </CardContent>
            </Card>
          )}

          {/* Captured Photo Preview - reduced size and centered */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="flex items-center gap-2">
                  <CheckCircle2 className="h-5 w-5 text-green-500" />
                  Foto Capturada
                </CardTitle>
                <Button variant="ghost" size="sm" onClick={resetFlow}>
                  <X className="mr-2 h-4 w-4" />
                  Tomar Otra
                </Button>
              </div>
            </CardHeader>
            <CardContent className="flex justify-center">
              <img 
                src={capturedImage} 
                alt="Captured product" 
                className="w-1/3 rounded-lg"
              />
            </CardContent>
          </Card>
        </>
      )}
    </div>
  );
}

export default ImageRecognitionSalesPage;
