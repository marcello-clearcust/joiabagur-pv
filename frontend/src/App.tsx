import { AppRouting } from '@/routing/app-routing';
import { ThemeProvider } from 'next-themes';
import { BrowserRouter } from 'react-router-dom';
import { Toaster } from '@/components/ui/sonner';
import { AuthProvider } from '@/providers';
import { ModelHealthAlert } from '@/components/admin/model-health-alert';

const { BASE_URL } = import.meta.env;

export function App() {
  return (
    <ThemeProvider
      attribute="class"
      defaultTheme="light"
      storageKey="vite-theme"
      enableSystem
      disableTransitionOnChange
      enableColorScheme
    >
      <BrowserRouter basename={BASE_URL}>
        <AuthProvider>
          <ModelHealthAlert />
          <Toaster />
          <AppRouting />
        </AuthProvider>
      </BrowserRouter>
    </ThemeProvider>
  );
}
