import { ReactNode } from 'react';
import { LogOut, Moon, User } from 'lucide-react';
import { useTheme } from 'next-themes';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Switch } from '@/components/ui/switch';
import { useAuth } from '@/providers';

interface UserMenuProps {
  trigger: ReactNode;
}

/**
 * Simplified User Menu for JoiaBagur PV
 * Shows: User name, role badge, dark mode toggle, and logout button
 */
export function UserMenu({ trigger }: UserMenuProps) {
  const { theme, setTheme } = useTheme();
  const { user, logout } = useAuth();

  // Get user info from auth context
  const userName = user ? `${user.firstName} ${user.lastName}` : 'Usuario';
  const userRole = user?.role === 'Administrator' ? 'Administrador' : 'Operador';

  const handleThemeToggle = (checked: boolean) => {
    setTheme(checked ? 'dark' : 'light');
  };

  const handleLogout = async () => {
    await logout();
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>{trigger}</DropdownMenuTrigger>
      <DropdownMenuContent className="w-56" side="right" align="end">
        {/* Header - User Info */}
        <div className="flex items-center justify-between p-3">
          <div className="flex items-center gap-3">
            <div className="flex items-center justify-center size-9 rounded-full bg-primary/10 text-primary">
              <User className="size-5" />
            </div>
            <div className="flex flex-col">
              <span className="text-sm font-semibold text-foreground">
                {userName}
              </span>
            </div>
          </div>
          <Badge variant="secondary" size="sm">
            {userRole}
          </Badge>
        </div>

        <DropdownMenuSeparator />

        {/* Dark Mode Toggle */}
        <DropdownMenuItem
          className="flex items-center gap-2"
          onSelect={(event) => event.preventDefault()}
        >
          <Moon className="size-4" />
          <div className="flex items-center gap-2 justify-between grow">
            Modo Oscuro
            <Switch
              size="sm"
              checked={theme === 'dark'}
              onCheckedChange={handleThemeToggle}
            />
          </div>
        </DropdownMenuItem>

        <DropdownMenuSeparator />

        {/* Logout Button */}
        <div className="p-2">
          <Button 
            variant="outline" 
            size="sm" 
            className="w-full"
            onClick={handleLogout}
          >
            <LogOut className="size-4 mr-2" />
            Cerrar Sesión
          </Button>
        </div>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
