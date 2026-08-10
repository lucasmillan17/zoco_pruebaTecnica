import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { authService } from '../services/authService';
import { guardarSesion, leerToken, leerUsuario, limpiarSesion } from '../storage';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => leerUsuario());
  const [isLoading, setIsLoading] = useState(() => Boolean(leerToken()));

  // Al arrancar, si hay token, validarlo contra /auth/me para recuperar la sesión
  useEffect(() => {
    let activo = true;

    async function validarSesion() {
      if (!leerToken()) {
        setIsLoading(false);
        return;
      }
      try {
        const usuario = await authService.me();
        if (activo) {
          setUser(usuario);
          localStorage.setItem('usuario', JSON.stringify(usuario));
        }
      } catch {
        if (activo) {
          limpiarSesion();
          setUser(null);
        }
      } finally {
        if (activo) setIsLoading(false);
      }
    }

    validarSesion();
    return () => {
      activo = false;
    };
  }, []);

  const login = useCallback(async (credentials) => {
    const data = await authService.login(credentials);
    guardarSesion(data);
    setUser(data.usuario);
    return data.usuario;
  }, []);

  const logout = useCallback(() => {
    limpiarSesion();
    setUser(null);
  }, []);

  const actualizarUsuario = useCallback((usuario) => {
    setUser(usuario);
    localStorage.setItem('usuario', JSON.stringify(usuario));
  }, []);

  const value = useMemo(
    () => ({ user, isAuthenticated: Boolean(user), isLoading, login, logout, actualizarUsuario }),
    [user, isLoading, login, logout, actualizarUsuario]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth debe usarse dentro de un AuthProvider');
  return ctx;
}
