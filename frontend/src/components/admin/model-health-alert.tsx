/**
 * Model Health Alert Component
 * Shows toast notification on admin login if model training is needed.
 */
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { Brain } from 'lucide-react';

import { useAuth } from '@/providers/auth-provider';
import { imageRecognitionService } from '@/services/sales.service';
import { ROUTES } from '@/routing/routes';

export function ModelHealthAlert() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [hasChecked, setHasChecked] = useState(false);

  useEffect(() => {
    const checkModelHealth = async () => {
      // Only check once per session and only for admins
      if (hasChecked || !user || user.role !== 'Administrator') {
        return;
      }

      try {
        const health = await imageRecognitionService.getModelHealth();
        
        // Show toast for CRITICAL or HIGH alert levels
        if (health.alertLevel === 'CRITICAL' || health.alertLevel === 'HIGH') {
          const alertIcon = health.alertLevel === 'CRITICAL' ? '🔴' : '🟠';
          const alertTitle = health.alertLevel === 'CRITICAL' 
            ? 'Modelo de IA: Acción Requerida'
            : 'Modelo de IA: Alta Prioridad';

          toast.warning(alertTitle, {
            icon: <Brain className="h-4 w-4" />,
            description: health.alertMessage,
            duration: 10000, // 10 seconds
            action: {
              label: 'Ver Dashboard',
              onClick: () => navigate(ROUTES.AI_MODEL),
            },
          });
        }
        
        setHasChecked(true);
      } catch (error) {
        // Silently fail - model health check is not critical
        console.debug('Model health check failed:', error);
        setHasChecked(true);
      }
    };

    // Check after a short delay to avoid blocking UI
    const timer = setTimeout(checkModelHealth, 2000);
    
    return () => clearTimeout(timer);
  }, [user, hasChecked, navigate]);

  // This component doesn't render anything
  return null;
}
