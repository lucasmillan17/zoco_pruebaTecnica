import { Navigate, Outlet, useLocation } from 'react-router-dom';
import Spinner from '../../../shared/components/atoms/Spinner';
import { useAuth } from '../context/AuthProvider';

/**
 * Bloquea el acceso a rutas que requieren sesión. Si se indican `roles`,
 * además valida que el usuario tenga uno de ellos. Si el usuario debe cambiar
 * su contraseña en el primer inicio, solo se permite la ruta /cambiar-password.
 */
export default function ProtectedRoute({ roles = null }) {
  const { user, isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-100">
        <Spinner />
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (user.debeCambiarPassword && location.pathname !== '/cambiar-password') {
    return <Navigate to="/cambiar-password" replace />;
  }

  if (roles && !roles.includes(user.rol)) {
    return <Navigate to="/comercios" replace />;
  }

  return <Outlet />;
}
