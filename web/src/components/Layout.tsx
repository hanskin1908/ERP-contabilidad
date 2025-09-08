import { Link, useLocation, useNavigate } from 'react-router-dom'
import { getToken, clearToken } from '../auth'

export default function Layout({ children }: { children: React.ReactNode }) {
  const navigate = useNavigate()
  const loc = useLocation()
  const authed = !!getToken()

  const logout = () => { clearToken(); navigate('/login') }
  const NavItem = ({ to, label }: { to: string; label: string }) => (
    <Link to={to} className={`block rounded-md px-3 py-2 hover:bg-slate-100 ${loc.pathname.startsWith(to) ? 'bg-slate-100 font-medium' : ''}`}>{label}</Link>
  )

  return (
    <div className="min-h-screen grid grid-cols-1 lg:grid-cols-[240px_1fr]">
      <aside className="hidden lg:block border-r border-slate-200 bg-white">
        <div className="px-4 py-4 text-brand-700 font-bold text-lg">ERP Contable</div>
        <nav className="px-2 space-y-1">
          {authed ? (
            <>
              <NavItem to="/accounts" label="Cuentas" />
              <NavItem to="/third-parties" label="Terceros" />
              <NavItem to="/journal" label="Asientos" />
              <NavItem to="/invoices" label="Facturas" />
              <NavItem to="/reports" label="Reportes" />
              <NavItem to="/settings" label="Configuración" />
            </>
          ) : (
            <NavItem to="/login" label="Login" />
          )}
        </nav>
      </aside>
      <div className="flex flex-col min-h-screen">
        <header className="sticky top-0 z-10 bg-white border-b border-slate-200">
          <div className="h-14 flex items-center justify-between px-4">
            <div className="lg:hidden font-semibold text-brand-700">ERP Contable</div>
            <div className="flex items-center gap-2">
              {authed ? (
                <button className="btn btn-secondary" onClick={logout}>Salir</button>
              ) : (
                <Link to="/login" className="btn btn-primary">Entrar</Link>
              )}
            </div>
          </div>
        </header>
        <main className="p-4">
          {children}
        </main>
      </div>
    </div>
  )
}
