import { Outlet } from 'react-router-dom';
import { useBodyClass } from '@/hooks/use-body-class';
import { useIsMobile } from '@/hooks/use-mobile';
import { Footer } from './footer';
import { Header } from './header';
import { Sidebar } from './sidebar';

export function Main() {
  const isMobile = useIsMobile();

  // Using the custom hook to set classes on the body
  useBodyClass(`
    [--header-height:60px]
    [--sidebar-width:90px]
    bg-muted!
  `);

  return (
    <div className="flex min-h-0 min-w-0 w-full grow">
      {isMobile && <Header />}

      <div className="flex min-h-0 min-w-0 w-full grow flex-col pt-(--header-height) lg:flex-row lg:pt-0">
        {!isMobile && <Sidebar />}

        <div className="flex min-h-0 min-w-0 w-full grow flex-col rounded-xl border border-input bg-background m-4 mt-0 lg:m-5 lg:ms-(--sidebar-width)">
          <div className="kt-scrollable-y-auto flex min-h-0 min-w-0 w-full grow flex-col pt-5 lg:[scrollbar-width:auto]">
            <main className="min-w-0 max-w-full grow px-5 pb-5 lg:px-8" role="content">
              <Outlet />
            </main>
          </div>

          <Footer />
        </div>
      </div>
    </div>
  );
}
