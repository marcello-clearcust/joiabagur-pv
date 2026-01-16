/**
 * Product Catalog Page
 * Displays a paginated, searchable list of products with role-based filtering.
 * - Administrators see all products
 * - Operators see only products with inventory at their assigned POS
 */
import { useState, useEffect, useCallback, useMemo } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import {
  Package,
  Search,
  ChevronLeft,
  ChevronRight,
  SortAsc,
  SortDesc,
  Plus,
  FileSpreadsheet,
  X,
  AlertTriangle,
} from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { cn } from '@/lib/utils';

import { useAuth } from '@/providers/auth-provider';
import { productService } from '@/services/product.service';
import { ProductListItem, PaginatedResult, CatalogQueryParams } from '@/types/product.types';
import { ROUTES } from '@/routing/routes';
import { useDebouncedCallback } from '@/hooks/use-debounced-callback';
import { getImageUrl } from '@/lib/image-url';

const PAGE_SIZE = 50;

type SortField = 'name' | 'createdAt' | 'price';
type SortDirection = 'asc' | 'desc';

interface ProductCardProps {
  product: ProductListItem;
  isAdmin: boolean;
}

/**
 * Product Card Component
 */
function ProductCard({ product, isAdmin }: ProductCardProps) {
  const photoUrl = getImageUrl(product.primaryPhotoUrl);

  return (
    <Card className="group overflow-hidden transition-shadow hover:shadow-md">
      <div className="relative aspect-square bg-muted">
        {photoUrl ? (
          <img
            src={photoUrl}
            alt={product.name}
            className="h-full w-full object-cover"
          />
        ) : (
          <div className="flex h-full items-center justify-center">
            <Package className="h-12 w-12 text-muted-foreground/50" />
          </div>
        )}
        
        {/* Stock badge */}
        {product.availableQuantity === 0 && (
          <Badge
            variant="secondary"
            className="absolute left-2 top-2 bg-destructive/90 text-destructive-foreground"
          >
            <AlertTriangle className="mr-1 h-3 w-3" />
            Sin stock
          </Badge>
        )}
        
        {/* Inactive badge */}
        {!product.isActive && (
          <Badge
            variant="secondary"
            className="absolute right-2 top-2 bg-muted/90"
          >
            Inactivo
          </Badge>
        )}
      </div>
      
      <CardContent className="p-4">
        <div className="mb-1 font-mono text-xs text-muted-foreground">
          {product.sku}
        </div>
        <h3 className="line-clamp-2 font-medium leading-tight">
          {product.name}
        </h3>
        
        {product.collectionName && (
          <div className="mt-1 text-xs text-muted-foreground">
            {product.collectionName}
          </div>
        )}
        
        <div className="mt-3 flex items-center justify-between">
          <span className="text-lg font-semibold">
            €{product.price.toLocaleString('es-ES', { minimumFractionDigits: 2 })}
          </span>
          <span className={cn(
            "text-sm font-mono",
            product.availableQuantity === 0 
              ? "text-destructive" 
              : "text-muted-foreground"
          )}>
            {product.availableQuantity} uds
          </span>
        </div>
        
        {isAdmin && (
          <Button
            variant="outline"
            size="sm"
            className="mt-3 w-full"
            asChild
          >
            <Link to={ROUTES.PRODUCTS.EDIT(product.id)}>
              Editar
            </Link>
          </Button>
        )}
      </CardContent>
    </Card>
  );
}

/**
 * Product Catalog Page Component
 */
export function ProductCatalogPage() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Administrator';
  const [searchParams, setSearchParams] = useSearchParams();

  // State
  const [catalogResult, setCatalogResult] = useState<PaginatedResult<ProductListItem> | null>(null);
  const [searchResults, setSearchResults] = useState<ProductListItem[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [searchLoading, setSearchLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState(searchParams.get('q') || '');

  // Query parameters
  const currentPage = parseInt(searchParams.get('page') || '1', 10);
  const sortBy = (searchParams.get('sortBy') as SortField) || 'name';
  const sortDirection = (searchParams.get('sortDir') as SortDirection) || 'asc';
  const includeInactive = searchParams.get('inactive') === 'true';

  // Update URL params
  const updateParams = useCallback((updates: Record<string, string | null>) => {
    const newParams = new URLSearchParams(searchParams);
    Object.entries(updates).forEach(([key, value]) => {
      if (value === null) {
        newParams.delete(key);
      } else {
        newParams.set(key, value);
      }
    });
    setSearchParams(newParams, { replace: true });
  }, [searchParams, setSearchParams]);

  // Load catalog
  const loadCatalog = useCallback(async () => {
    setLoading(true);
    try {
      const params: CatalogQueryParams = {
        page: currentPage,
        pageSize: PAGE_SIZE,
        sortBy,
        sortDirection,
        includeInactive,
      };
      const result = await productService.getCatalog(params);
      setCatalogResult(result);
    } catch (error) {
      toast.error('Error al cargar el catálogo');
      console.error(error);
    } finally {
      setLoading(false);
    }
  }, [currentPage, sortBy, sortDirection, includeInactive]);

  // Search products (debounced)
  const performSearch = useDebouncedCallback(async (query: string) => {
    if (!query || query.length < 2) {
      setSearchResults(null);
      setSearchLoading(false);
      return;
    }

    setSearchLoading(true);
    try {
      const results = await productService.searchProducts(query);
      setSearchResults(results);
    } catch (error) {
      toast.error('Error en la búsqueda');
      console.error(error);
    } finally {
      setSearchLoading(false);
    }
  }, 300);

  // Handle search input change
  const handleSearchChange = (value: string) => {
    setSearchQuery(value);
    if (value.length >= 2) {
      setSearchLoading(true);
      performSearch(value);
    } else {
      setSearchResults(null);
    }
    updateParams({ q: value || null, page: '1' });
  };

  // Clear search
  const clearSearch = () => {
    setSearchQuery('');
    setSearchResults(null);
    updateParams({ q: null });
  };

  // Load catalog on mount and param changes
  useEffect(() => {
    if (!searchQuery || searchQuery.length < 2) {
      loadCatalog();
    }
  }, [loadCatalog, searchQuery]);

  // Perform initial search if query param exists
  useEffect(() => {
    const q = searchParams.get('q');
    if (q && q.length >= 2) {
      setSearchQuery(q);
      performSearch(q);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Products to display
  const displayProducts = useMemo(() => {
    if (searchResults !== null) {
      return searchResults;
    }
    return catalogResult?.items || [];
  }, [searchResults, catalogResult]);

  const isSearchMode = searchResults !== null;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Catálogo de Productos</h1>
          <p className="text-muted-foreground">
            {isSearchMode
              ? `${displayProducts.length} resultado${displayProducts.length !== 1 ? 's' : ''}`
              : catalogResult
                ? `${catalogResult.totalCount} producto${catalogResult.totalCount !== 1 ? 's' : ''}`
                : 'Cargando...'}
          </p>
        </div>

        {isAdmin && (
          <div className="flex flex-wrap gap-2">
            <Button variant="outline" size="sm" asChild>
              <Link to={ROUTES.PRODUCTS.IMPORT}>
                <FileSpreadsheet className="mr-2 h-4 w-4" />
                Importar Excel
              </Link>
            </Button>
            <Button size="sm" asChild>
              <Link to={ROUTES.PRODUCTS.CREATE}>
                <Plus className="mr-2 h-4 w-4" />
                Nuevo Producto
              </Link>
            </Button>
          </div>
        )}
      </div>

      {/* Search and Filters */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        {/* Search Input */}
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Buscar por SKU o nombre..."
            value={searchQuery}
            onChange={(e) => handleSearchChange(e.target.value)}
            className="pl-10 pr-10"
          />
          {searchQuery && (
            <button
              onClick={clearSearch}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
            >
              <X className="h-4 w-4" />
            </button>
          )}
          {searchLoading && (
            <div className="absolute right-10 top-1/2 -translate-y-1/2">
              <div className="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent" />
            </div>
          )}
        </div>

        {/* Sort Controls (hidden during search) */}
        {!isSearchMode && (
          <>
            <Select
              value={sortBy}
              onValueChange={(value: SortField) => updateParams({ sortBy: value, page: '1' })}
            >
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Ordenar por" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="name">Nombre</SelectItem>
                <SelectItem value="price">Precio</SelectItem>
                <SelectItem value="createdAt">Fecha creación</SelectItem>
              </SelectContent>
            </Select>

            <Button
              variant="outline"
              size="icon"
              onClick={() => updateParams({ 
                sortDir: sortDirection === 'asc' ? 'desc' : 'asc',
                page: '1'
              })}
            >
              {sortDirection === 'asc' ? (
                <SortAsc className="h-4 w-4" />
              ) : (
                <SortDesc className="h-4 w-4" />
              )}
            </Button>

            {isAdmin && (
              <Button
                variant={includeInactive ? 'secondary' : 'ghost'}
                size="sm"
                onClick={() => updateParams({ 
                  inactive: includeInactive ? null : 'true',
                  page: '1'
                })}
              >
                {includeInactive ? 'Mostrando inactivos' : 'Incluir inactivos'}
              </Button>
            )}
          </>
        )}
      </div>

      {/* Product Grid */}
      {loading ? (
        <div className="grid gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
          {Array.from({ length: 10 }).map((_, i) => (
            <Card key={i} className="overflow-hidden">
              <Skeleton className="aspect-square" />
              <CardContent className="p-4">
                <Skeleton className="mb-2 h-3 w-16" />
                <Skeleton className="h-4 w-full" />
                <Skeleton className="mt-1 h-3 w-24" />
                <div className="mt-3 flex justify-between">
                  <Skeleton className="h-6 w-20" />
                  <Skeleton className="h-4 w-12" />
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : displayProducts.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-16 text-center">
          <Package className="mb-4 h-16 w-16 text-muted-foreground/50" />
          {isSearchMode ? (
            <>
              <h3 className="text-lg font-medium">No se encontraron resultados</h3>
              <p className="text-muted-foreground">
                No hay productos que coincidan con "{searchQuery}"
              </p>
              <Button variant="outline" className="mt-4" onClick={clearSearch}>
                Limpiar búsqueda
              </Button>
            </>
          ) : (
            <>
              <h3 className="text-lg font-medium">No hay productos</h3>
              <p className="text-muted-foreground">
                {isAdmin
                  ? 'Comienza importando productos desde un archivo Excel o créalos manualmente.'
                  : 'No tienes productos asignados a tus puntos de venta.'}
              </p>
              {isAdmin && (
                <div className="mt-4 flex gap-2">
                  <Button variant="outline" asChild>
                    <Link to={ROUTES.PRODUCTS.IMPORT}>
                      <FileSpreadsheet className="mr-2 h-4 w-4" />
                      Importar Excel
                    </Link>
                  </Button>
                  <Button asChild>
                    <Link to={ROUTES.PRODUCTS.CREATE}>
                      <Plus className="mr-2 h-4 w-4" />
                      Crear Producto
                    </Link>
                  </Button>
                </div>
              )}
            </>
          )}
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
          {displayProducts.map((product) => (
            <ProductCard
              key={product.id}
              product={product}
              isAdmin={isAdmin}
            />
          ))}
        </div>
      )}

      {/* Pagination (hidden during search) */}
      {!isSearchMode && catalogResult && catalogResult.totalPages > 1 && (
        <div className="flex items-center justify-center gap-2">
          <Button
            variant="outline"
            size="sm"
            disabled={!catalogResult.hasPreviousPage}
            onClick={() => updateParams({ page: String(currentPage - 1) })}
          >
            <ChevronLeft className="mr-1 h-4 w-4" />
            Anterior
          </Button>
          
          <span className="px-4 text-sm text-muted-foreground">
            Página {catalogResult.currentPage} de {catalogResult.totalPages}
          </span>
          
          <Button
            variant="outline"
            size="sm"
            disabled={!catalogResult.hasNextPage}
            onClick={() => updateParams({ page: String(currentPage + 1) })}
          >
            Siguiente
            <ChevronRight className="ml-1 h-4 w-4" />
          </Button>
        </div>
      )}
    </div>
  );
}

export default ProductCatalogPage;

