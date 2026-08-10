import { useState } from 'react';
import {
  createBrowserRouter,
  Link,
  Navigate,
  NavLink,
  Outlet,
  RouterProvider,
} from 'react-router-dom';
import { Building2, CircleUserRound, History, KeyRound, LogOut, Menu, Tags, Users, X } from 'lucide-react';
import ComerciosPage from './modules/comercios/pages/ComerciosPage';
import TiposInteraccionPage from './modules/interacciones/pages/TiposInteraccionPage';
import CuentasPage from './modules/cuentas/pages/CuentasPage';
import AuditoriaPage from './modules/auditoria/pages/AuditoriaPage';
import LoginPage from './modules/auth/pages/LoginPage';
import CambiarPasswordPage from './modules/auth/pages/CambiarPasswordPage';
import ProtectedRoute from './modules/auth/components/ProtectedRoute';
import { AuthProvider, useAuth } from './modules/auth/context/AuthProvider';

const NAV_GRUPOS = [
  {
    titulo: 'Facturación',
    roles: ['Administrador', 'Ventas'],
    items: [{ to: '/comercios', label: 'Comercios', icon: Building2 }],
  },
  {
    titulo: 'Administración',
    roles: ['Administrador'],
    items: [
      { to: '/tipos-interaccion', label: 'Tipos de interacción', icon: Tags },
      { to: '/cuentas', label: 'Cuentas', icon: Users },
      { to: '/auditoria', label: 'Auditoría', icon: History },
    ],
  },
];

function Logo() {
  return (
    <div className="flex items-center gap-2 px-5 py-5">
      <span className="flex h-8 w-8 items-center justify-center rounded-md bg-primary text-sm font-bold text-white">
        C
      </span>
      <span className="text-base font-semibold text-gray-900">CMS Zoco</span>
    </div>
  );
}

function SidebarContenido({ onNavegar }) {
  const { user, logout } = useAuth();
  const gruposVisibles = NAV_GRUPOS.filter((grupo) =>
    grupo.roles.includes(user.rol)
  );

  return (
    <div className="flex h-full flex-col">
      <Logo />
      <nav className="flex-1 space-y-5 px-3">
        {gruposVisibles.map((grupo) => (
          <div key={grupo.titulo}>
            <p className="px-2 pb-1.5 text-xs font-semibold uppercase tracking-wide text-muted">
              {grupo.titulo}
            </p>
            <ul className="space-y-1">
              {grupo.items.map((item) => {
                const Icono = item.icon;
                return (
                  <li key={item.to}>
                    <NavLink
                      to={item.to}
                      onClick={onNavegar}
                      className={({ isActive }) =>
                        `flex items-center gap-2.5 rounded-md px-2.5 py-1.5 text-sm font-medium transition-colors ${
                          isActive
                            ? 'bg-primary/10 text-primary'
                            : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                        }`
                      }
                    >
                      <Icono className="h-4 w-4 shrink-0" />
                      {item.label}
                    </NavLink>
                  </li>
                );
              })}
            </ul>
          </div>
        ))}
      </nav>
      <div className="border-t border-gray-200 p-3">
        <div className="flex items-center gap-2 rounded-md bg-gray-50 px-2.5 py-2">
          <CircleUserRound className="h-5 w-5 text-gray-400" />
          <div className="min-w-0 flex-1">
            <p className="truncate text-sm font-medium text-gray-900">{user.nombre}</p>
            <p className="text-xs capitalize text-gray-500">{user.rol}</p>
          </div>
          <Link
            to="/cambiar-password"
            title="Cambiar contraseña"
            className="rounded-md p-1.5 text-gray-400 transition-colors hover:bg-gray-200 hover:text-gray-700"
          >
            <KeyRound className="h-4 w-4" />
          </Link>
          <button
            type="button"
            onClick={logout}
            title="Cerrar sesión"
            className="rounded-md p-1.5 text-gray-400 transition-colors hover:bg-gray-200 hover:text-gray-700"
          >
            <LogOut className="h-4 w-4" />
          </button>
        </div>
      </div>
    </div>
  );
}

function AppLayout() {
  const [menuAbierto, setMenuAbierto] = useState(false);

  return (
    <div className="flex min-h-screen bg-gray-100">
      {/* Overlay móvil */}
      <div
        className={`fixed inset-0 z-40 bg-gray-900/50 transition-opacity lg:hidden ${
          menuAbierto ? 'opacity-100' : 'pointer-events-none opacity-0'
        }`}
        onClick={() => setMenuAbierto(false)}
      />
      <aside
        className={`fixed inset-y-0 left-0 z-50 w-64 transform bg-white transition-transform lg:static lg:translate-x-0 ${
          menuAbierto ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <div className="flex h-full flex-col">
          <button
            type="button"
            onClick={() => setMenuAbierto(false)}
            className="absolute right-3 top-3 rounded-md p-1 text-gray-400 hover:bg-gray-100 lg:hidden"
          >
            <X className="h-5 w-5" />
          </button>
          <SidebarContenido onNavegar={() => setMenuAbierto(false)} />
        </div>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex items-center justify-between border-b border-gray-200 bg-white px-4 py-3 lg:hidden">
          <button
            type="button"
            className="rounded-md p-2 text-gray-600 hover:bg-gray-100"
            onClick={() => setMenuAbierto(true)}
          >
            <Menu className="h-5 w-5" />
          </button>
          <span className="text-sm font-medium text-gray-600">CMS Zoco</span>
          <span className="w-9" />
        </header>
        <main className="flex-1 overflow-y-auto p-4 lg:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  {
    element: <ProtectedRoute />,
    children: [
      { path: 'cambiar-password', element: <CambiarPasswordPage /> },
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <Navigate to="/comercios" replace /> },
          { path: 'comercios', element: <ComerciosPage /> },
          {
            element: <ProtectedRoute roles={['Administrador']} />,
            children: [
              { path: 'tipos-interaccion', element: <TiposInteraccionPage /> },
              { path: 'cuentas', element: <CuentasPage /> },
              { path: 'auditoria', element: <AuditoriaPage /> },
            ],
          },
        ],
      },
    ],
  },
  { path: '*', element: <Navigate to="/comercios" replace /> },
]);

export default function App() {
  return (
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  );
}
