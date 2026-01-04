import { Link } from 'react-router-dom';
import logoUrl from '@/assets/logo.svg';

export function SidebarHeader() {
  return (
    <div className="hidden lg:flex items-center justify-center shrink-0 pt-8 pb-3.5">
      <Link to="/dashboard" className="flex items-center">
        <img src={logoUrl} alt="Joia Bagur" className="h-12 w-auto" />
      </Link>
    </div>
  );
}
