import apiClient from './api.service';
import type { DashboardStats } from '@/types/dashboard.types';

const DASHBOARD_ENDPOINT = '/dashboard';

export const dashboardService = {
  getStats: async (posId?: string): Promise<DashboardStats> => {
    const params = new URLSearchParams();
    if (posId) params.append('posId', posId);

    const url = `${DASHBOARD_ENDPOINT}/stats${params.toString() ? `?${params.toString()}` : ''}`;
    const response = await apiClient.get<DashboardStats>(url);
    return response.data;
  },
};

export default dashboardService;
