import { Link, useLocation } from 'react-router-dom';
import { MenuConfig } from '@/config/types';
import { cn } from '@/lib/utils';
import { useMenu } from '@/hooks/use-menu';
import { useRoleMenu } from '@/hooks/use-role-menu';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuSub,
  DropdownMenuSubContent,
  DropdownMenuSubTrigger,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

export function SidebarMenu() {
  const { pathname } = useLocation();
  const { isActive, hasActiveChild } = useMenu(pathname);

  // Use the role-based menu configuration from auth context
  const menuConfig: MenuConfig = useRoleMenu();

  const buildMenu = (items: MenuConfig) => {
    return items.map((item, index) => {
      // Skip headings and separators for icon menu
      if (item.heading || item.separator) return null;

      return (
        <div key={index} className="flex flex-col items-center">
          {item.children ? (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  data-here={hasActiveChild(item.children) || undefined}
                  className={cn(
                    'flex flex-col items-center justify-center w-[60px] h-[60px] gap-1 rounded-lg shadow-none',
                    'text-xs font-medium text-secondary-foreground bg-transparent',
                    'hover:text-primary hover:bg-background hover:border-border',
                    'data-[state=open]:text-primary data-[state=open]:bg-background data-[state=open]:border-border',
                    'data-[here=true]:text-primary data-[here=true]:bg-background data-[here=true]:border-border',
                  )}
                >
                  {item.icon && <item.icon className="size-5!" />}
                  {item.title}
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent
                align="start"
                side="right"
                className="w-[200px]"
              >
                <DropdownMenuLabel>{item.title}</DropdownMenuLabel>
                {buildMenuChildren(item.children)}
              </DropdownMenuContent>
            </DropdownMenu>
          ) : (
            <Link
              data-active={isActive(item.path) || undefined}
              to={item.path || '#'}
              className={cn(
                'flex flex-col items-center justify-center w-[62px] h-[60px] gap-1 p-2 rounded-lg',
                'text-xs font-medium text-muted-foreground bg-transparent',
                'hover:text-primary hover:bg-background hover:border-border',
                'data-[active=true]:text-primary data-[active=true]:bg-background data-[active=true]:border-border',
              )}
            >
              {item.icon && <item.icon className="size-5!" />}
              {item.title}
            </Link>
          )}
        </div>
      );
    });
  };

  const buildMenuChildren = (items: MenuConfig) => {
    return items.map((item, index) => {
      if (item.disabled) return null;
      if (item.separator) return <DropdownMenuSeparator key={index} />;
      if (item.children) {
        return (
          <DropdownMenuSub key={index}>
            <DropdownMenuSubTrigger
              data-here={hasActiveChild(item.children) || undefined}
            >
              {item.icon && <item.icon className="size-4 mr-2" />}
              {item.title}
            </DropdownMenuSubTrigger>
            <DropdownMenuSubContent className="w-[200px]">
              <DropdownMenuLabel>{item.title}</DropdownMenuLabel>
              {buildMenuChildren(item.children)}
            </DropdownMenuSubContent>
          </DropdownMenuSub>
        );
      }
      return (
        <DropdownMenuItem key={index} asChild>
          <Link to={item.path || '#'} className="flex items-center">
            {item.icon && <item.icon className="size-4 mr-2" />}
            {item.title}
          </Link>
        </DropdownMenuItem>
      );
    });
  };

  return (
    <div className="flex flex-col gap-2.5 grow kt-scrollable-y-auto max-h-[calc(100vh-5rem)] lg:max-h-[calc(100vh-6rem)]">
      {buildMenu(menuConfig)}
    </div>
  );
}
