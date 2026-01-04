import { User } from 'lucide-react';
import { UserMenu } from './user-menu';

export function SidebarFooter() {
  return (
    <div className="flex flex-col gap-5 items-center shrink-0 pb-5">
      <UserMenu
        trigger={
          <button className="flex items-center justify-center size-10 rounded-lg border-2 border-muted-foreground/30 bg-muted hover:bg-background hover:border-primary/50 transition-colors cursor-pointer">
            <User className="size-5 text-muted-foreground" />
          </button>
        }
      />
    </div>
  );
}
