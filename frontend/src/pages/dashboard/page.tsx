import { useAuth } from '@/providers/auth-provider';
import { AdminDashboard } from './AdminDashboard';
import { OperatorDashboard } from './OperatorDashboard';

export function DashboardPage() {
  const { user } = useAuth();

  if (!user) return null;

  if (user.role === 'Administrator') {
    return <AdminDashboard />;
  }

  return <OperatorDashboard />;
}

export default DashboardPage;
