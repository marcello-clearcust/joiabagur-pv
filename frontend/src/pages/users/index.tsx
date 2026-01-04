/**
 * Users Module - EP7
 * User management page with CRUD operations
 */

import { useState, useEffect, useMemo } from 'react';
import {
  useReactTable,
  getCoreRowModel,
  ColumnDef,
  createColumnHelper,
} from '@tanstack/react-table';
import { Plus, Pencil, MoreHorizontal, UserCheck, UserX, Store } from 'lucide-react';
import { userService } from '@/services/user.service';
import { UserListItem } from '@/types/user.types';
import { DataGrid, DataGridContainer } from '@/components/ui/data-grid';
import { DataGridTable } from '@/components/ui/data-grid-table';
import { DataGridPagination } from '@/components/ui/data-grid-pagination';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { toast } from 'sonner';
import { UserFormDialog } from './components/user-form-dialog';
import { UserAssignmentsDialog } from './components/user-assignments-dialog';

const columnHelper = createColumnHelper<UserListItem>();

export function UsersPage() {
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isAssignmentsOpen, setIsAssignmentsOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserListItem | null>(null);

  // Fetch users
  const fetchUsers = async () => {
    setIsLoading(true);
    try {
      const data = await userService.getUsers();
      setUsers(data);
    } catch (error) {
      toast.error('Error al cargar los usuarios');
      console.error('Failed to fetch users:', error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  // Handle create user
  const handleCreate = () => {
    setSelectedUser(null);
    setIsDialogOpen(true);
  };

  // Handle edit user
  const handleEdit = (user: UserListItem) => {
    setSelectedUser(user);
    setIsDialogOpen(true);
  };

  // Handle assignments
  const handleAssignments = (user: UserListItem) => {
    setSelectedUser(user);
    setIsAssignmentsOpen(true);
  };

  // Handle dialog close
  const handleDialogClose = (success?: boolean) => {
    setIsDialogOpen(false);
    setSelectedUser(null);
    if (success) {
      fetchUsers();
    }
  };

  // Handle assignments dialog close
  const handleAssignmentsClose = () => {
    setIsAssignmentsOpen(false);
    setSelectedUser(null);
  };

  // Table columns
  const columns = useMemo<ColumnDef<UserListItem, unknown>[]>(
    () => [
      columnHelper.accessor('username', {
        header: () => 'Usuario',
        cell: (info) => (
          <span className="font-medium">{info.getValue()}</span>
        ),
        size: 150,
        meta: {
          skeleton: <Skeleton className="h-4 w-24" />,
        },
      }),
      columnHelper.accessor((row) => `${row.firstName} ${row.lastName}`, {
        id: 'fullName',
        header: () => 'Nombre Completo',
        cell: (info) => info.getValue(),
        size: 200,
        meta: {
          skeleton: <Skeleton className="h-4 w-32" />,
        },
      }),
      columnHelper.accessor('email', {
        header: () => 'Email',
        cell: (info) => info.getValue() || '-',
        size: 200,
        meta: {
          skeleton: <Skeleton className="h-4 w-40" />,
        },
      }),
      columnHelper.accessor('role', {
        header: () => 'Rol',
        cell: (info) => (
          <Badge
            variant={info.getValue() === 'Administrator' ? 'default' : 'secondary'}
          >
            {info.getValue() === 'Administrator' ? 'Administrador' : 'Operador'}
          </Badge>
        ),
        size: 130,
        meta: {
          skeleton: <Skeleton className="h-5 w-20" />,
        },
      }),
      columnHelper.accessor('isActive', {
        header: () => 'Estado',
        cell: (info) => (
          <div className="flex items-center gap-1.5">
            {info.getValue() ? (
              <>
                <UserCheck className="size-4 text-green-600" />
                <span className="text-green-600">Activo</span>
              </>
            ) : (
              <>
                <UserX className="size-4 text-muted-foreground" />
                <span className="text-muted-foreground">Inactivo</span>
              </>
            )}
          </div>
        ),
        size: 100,
        meta: {
          skeleton: <Skeleton className="h-4 w-16" />,
        },
      }),
      columnHelper.display({
        id: 'actions',
        header: () => '',
        cell: ({ row }) => (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" mode="icon" size="sm">
                <MoreHorizontal className="size-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => handleEdit(row.original)}>
                <Pencil className="size-4 mr-2" />
                Editar
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={() => handleAssignments(row.original)}>
                <Store className="size-4 mr-2" />
                Puntos de Venta
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        ),
        size: 60,
        meta: {
          skeleton: <Skeleton className="h-7 w-7" />,
        },
      }),
    ],
    []
  );

  // Table instance
  const table = useReactTable({
    data: users,
    columns,
    getCoreRowModel: getCoreRowModel(),
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Usuarios</h1>
          <p className="text-muted-foreground">
            Gestión de usuarios del sistema
          </p>
        </div>
        <Button onClick={handleCreate}>
          <Plus className="size-4 mr-2" />
          Nuevo Usuario
        </Button>
      </div>

      {/* Data Grid */}
      <DataGridContainer>
        <DataGrid
          table={table}
          recordCount={users.length}
          isLoading={isLoading}
          emptyMessage="No hay usuarios registrados"
        >
          <DataGridTable />
          <DataGridPagination />
        </DataGrid>
      </DataGridContainer>

      {/* User Form Dialog */}
      <UserFormDialog
        open={isDialogOpen}
        user={selectedUser}
        onClose={handleDialogClose}
      />

      {/* User Assignments Dialog */}
      <UserAssignmentsDialog
        open={isAssignmentsOpen}
        user={selectedUser}
        onClose={handleAssignmentsClose}
      />
    </div>
  );
}

export default UsersPage;
