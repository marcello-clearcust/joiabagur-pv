import { Instagram, Globe } from 'lucide-react';

export function Footer() {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="footer">
      <div className="container">
        <div className="flex flex-col md:flex-row justify-center md:justify-between items-center gap-3 py-5">
          <div className="flex order-2 md:order-1 gap-2 font-normal text-sm">
            <span className="text-muted-foreground">{currentYear} © Joia Bagur</span>
          </div>
          <nav className="flex order-1 md:order-2 gap-4 font-normal text-sm text-muted-foreground items-center">
            <a
              href="https://www.joiabagur.com"
              target="_blank"
              rel="noopener noreferrer"
              className="hover:text-primary flex items-center gap-1.5"
            >
              <Globe className="size-4" />
              www.joiabagur.com
            </a>
            <a
              href="https://instagram.com/joiabagur"
              target="_blank"
              rel="noopener noreferrer"
              className="hover:text-primary flex items-center gap-1.5"
            >
              <Instagram className="size-4" />
              @joiabagur
            </a>
          </nav>
        </div>
      </div>
    </footer>
  );
}
