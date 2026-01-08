/**
 * Product Photo Upload Component - EP1 HU-EP1-004
 * Component for uploading and managing product photos
 * Admin-only: Upload, delete, and set primary photo
 * Operator: Read-only view of photos
 */

import { useState, useRef } from 'react';
import { Upload, X, Star, Loader2, Image as ImageIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { toast } from 'sonner';
import { useAuth } from '@/providers/auth-provider';
import { productService } from '@/services/product.service';
import { ProductPhoto } from '@/types/product.types';
import { getImageUrl } from '@/lib/image-url';

interface ProductPhotoUploadProps {
  productId: string;
  photos: ProductPhoto[];
  onPhotosChange: () => void;
}

export function ProductPhotoUpload({ productId, photos, onPhotosChange }: ProductPhotoUploadProps) {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Administrator';
  
  const [isUploading, setIsUploading] = useState(false);
  const [isDragging, setIsDragging] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [photoToDelete, setPhotoToDelete] = useState<string | null>(null);
  const [deletingPhotoId, setDeletingPhotoId] = useState<string | null>(null);
  const [settingPrimaryPhotoId, setSettingPrimaryPhotoId] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const allowedFormats = ['image/jpeg', 'image/jpg', 'image/png'];
  const maxFileSize = 5 * 1024 * 1024; // 5 MB

  const validateFile = (file: File): string | null => {
    if (!allowedFormats.includes(file.type)) {
      return 'Solo se permiten archivos JPG y PNG';
    }
    if (file.size > maxFileSize) {
      return 'El archivo no puede superar 5 MB';
    }
    return null;
  };

  const handleFileSelect = async (files: FileList | null) => {
    if (!files || files.length === 0) return;

    const file = files[0];
    const error = validateFile(file);

    if (error) {
      toast.error('Archivo inválido', { description: error });
      return;
    }

    await uploadPhoto(file);
  };

  const uploadPhoto = async (file: File) => {
    setIsUploading(true);

    try {
      await productService.uploadPhoto(productId, file);
      toast.success('Foto subida exitosamente');
      onPhotosChange();
      
      // Clear file input
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    } catch (error: any) {
      console.error('Failed to upload photo:', error);
      const errorMessage = error.response?.data?.error || 'Error al subir la foto';
      toast.error('Error al subir la foto', { description: errorMessage });
    } finally {
      setIsUploading(false);
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    handleFileSelect(e.dataTransfer.files);
  };

  const handleSetPrimary = async (photoId: string) => {
    if (!isAdmin) return;
    
    setSettingPrimaryPhotoId(photoId);
    try {
      await productService.setPrimaryPhoto(productId, photoId);
      toast.success('Foto principal actualizada');
      onPhotosChange();
    } catch (error: any) {
      console.error('Failed to set primary photo:', error);
      const errorMessage = error.response?.data?.error || 'Error al establecer foto principal';
      toast.error('Error al establecer foto principal', { description: errorMessage });
    } finally {
      setSettingPrimaryPhotoId(null);
    }
  };

  const handleDeleteClick = (photoId: string) => {
    if (!isAdmin) return;
    setPhotoToDelete(photoId);
    setDeleteConfirmOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!photoToDelete) return;

    setDeletingPhotoId(photoToDelete);
    setDeleteConfirmOpen(false);
    
    try {
      await productService.deletePhoto(productId, photoToDelete);
      toast.success('Foto eliminada');
      onPhotosChange();
    } catch (error: any) {
      console.error('Failed to delete photo:', error);
      const errorMessage = error.response?.data?.error || 'Error al eliminar la foto';
      toast.error('Error al eliminar la foto', { description: errorMessage });
    } finally {
      setDeletingPhotoId(null);
      setPhotoToDelete(null);
    }
  };

  const handleDeleteCancel = () => {
    setDeleteConfirmOpen(false);
    setPhotoToDelete(null);
  };

  const sortedPhotos = [...photos].sort((a, b) => a.displayOrder - b.displayOrder);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Fotos del Producto</CardTitle>
        <CardDescription>
          Sube fotos de referencia para el reconocimiento de imágenes. Formatos: JPG, PNG (máx. 5 MB)
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Upload Zone - Admin Only */}
        {isAdmin && (
          <div
            className={`border-2 border-dashed rounded-lg p-8 text-center transition-colors ${
              isDragging
                ? 'border-primary bg-primary/5'
                : 'border-muted-foreground/25 hover:border-primary/50'
            } ${isUploading ? 'opacity-50 pointer-events-none' : ''}`}
            onDragOver={handleDragOver}
            onDragLeave={handleDragLeave}
            onDrop={handleDrop}
          >
            <input
              ref={fileInputRef}
              type="file"
              accept=".jpg,.jpeg,.png"
              onChange={(e) => handleFileSelect(e.target.files)}
              className="hidden"
              disabled={isUploading}
            />

            {isUploading ? (
              <div className="flex flex-col items-center gap-2">
                <Loader2 className="size-8 animate-spin text-primary" />
                <p className="text-sm text-muted-foreground">Subiendo foto...</p>
              </div>
            ) : (
              <div className="flex flex-col items-center gap-2">
                <Upload className="size-8 text-muted-foreground" />
                <div className="space-y-1">
                  <p className="text-sm font-medium">
                    Arrastra una foto aquí o{' '}
                    <button
                      type="button"
                      onClick={() => fileInputRef.current?.click()}
                      className="text-primary hover:underline"
                    >
                      selecciona un archivo
                    </button>
                  </p>
                  <p className="text-xs text-muted-foreground">
                    JPG, PNG hasta 5 MB
                  </p>
                </div>
              </div>
            )}
          </div>
        )}

        {/* Photo Gallery */}
        {sortedPhotos.length > 0 ? (
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {sortedPhotos.map((photo) => (
              <div key={photo.id} className="relative group">
                <div className="aspect-square rounded-lg overflow-hidden border bg-muted">
                  {photo.url ? (
                    <img
                      src={getImageUrl(photo.url)}
                      alt={`Foto ${photo.displayOrder + 1}`}
                      className="w-full h-full object-cover"
                    />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center">
                      <ImageIcon className="size-8 text-muted-foreground" />
                    </div>
                  )}
                </div>

                {/* Primary Badge */}
                {photo.isPrimary && (
                  <Badge className="absolute top-2 left-2 gap-1">
                    <Star className="size-3 fill-current" />
                    Principal
                  </Badge>
                )}

                {/* Actions - Admin Only */}
                {isAdmin && (
                  <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity rounded-lg flex items-center justify-center gap-2">
                    {!photo.isPrimary && (
                      <Button
                        size="sm"
                        variant="secondary"
                        onClick={() => handleSetPrimary(photo.id)}
                        disabled={settingPrimaryPhotoId === photo.id || deletingPhotoId === photo.id}
                      >
                        {settingPrimaryPhotoId === photo.id ? (
                          <Loader2 className="size-4 animate-spin" />
                        ) : (
                          <Star className="size-4" />
                        )}
                      </Button>
                    )}
                    <Button
                      size="sm"
                      variant="destructive"
                      onClick={() => handleDeleteClick(photo.id)}
                      disabled={deletingPhotoId === photo.id || settingPrimaryPhotoId === photo.id}
                    >
                      {deletingPhotoId === photo.id ? (
                        <Loader2 className="size-4 animate-spin" />
                      ) : (
                        <X className="size-4" />
                      )}
                    </Button>
                  </div>
                )}
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-8 text-muted-foreground">
            <ImageIcon className="size-12 mx-auto mb-2 opacity-50" />
            <p className="text-sm">No hay fotos subidas aún</p>
            <p className="text-xs">Sube la primera foto para comenzar</p>
          </div>
        )}

        {/* Delete Confirmation Dialog */}
        <Dialog open={deleteConfirmOpen} onOpenChange={setDeleteConfirmOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Eliminar foto</DialogTitle>
              <DialogDescription>
                ¿Estás seguro de que deseas eliminar esta foto? Esta acción no se puede deshacer.
              </DialogDescription>
            </DialogHeader>
            <DialogFooter>
              <Button variant="outline" onClick={handleDeleteCancel}>
                Cancelar
              </Button>
              <Button variant="destructive" onClick={handleDeleteConfirm}>
                Eliminar
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </CardContent>
    </Card>
  );
}

