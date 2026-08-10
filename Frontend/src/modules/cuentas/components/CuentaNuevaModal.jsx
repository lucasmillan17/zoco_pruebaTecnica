import { useState } from 'react';
import Alert from '../../../shared/components/atoms/Alert';
import Button from '../../../shared/components/atoms/Button';
import FormField from '../../../shared/components/molecules/FormField';
import Input from '../../../shared/components/atoms/Input';
import PasswordInput from '../../../shared/components/atoms/PasswordInput';
import Select from '../../../shared/components/atoms/Select';
import Modal from '../../../shared/components/atoms/Modal';
import { authService } from '../../auth/services/authService';

const ROLES = [
  { value: 'Administrador', label: 'Administrador' },
  { value: 'Ventas', label: 'Ventas' },
];

export default function CuentaNuevaModal({ onGuardado, onClose }) {
  const [form, setForm] = useState({
    nombreUsuario: '',
    nombre: '',
    password: '',
    rol: 'Ventas',
    email: '',
    telefono: '',
  });
  const [error, setError] = useState(null);
  const [enviando, setEnviando] = useState(false);

  function cambiar(campo) {
    return (e) => setForm((f) => ({ ...f, [campo]: e.target.value }));
  }

  async function onSubmit(e) {
    e.preventDefault();
    setError(null);
    setEnviando(true);
    try {
      const datos = {
        nombreUsuario: form.nombreUsuario,
        nombre: form.nombre,
        password: form.password,
        rol: form.rol,
        email: form.email.trim() || null,
        telefono: form.telefono.trim() || null,
      };
      await authService.crearUsuario(datos);
      onGuardado();
    } catch (err) {
      setError(err.message);
    } finally {
      setEnviando(false);
    }
  }

  return (
    <Modal title="Nueva cuenta" onClose={onClose}>
      <form onSubmit={onSubmit} className="space-y-4" noValidate>
        {error && <Alert tono="error">{error}</Alert>}

        <div className="grid gap-4 sm:grid-cols-2">
          <FormField label="Nombre de usuario" required>
            <Input value={form.nombreUsuario} onChange={cambiar('nombreUsuario')} placeholder="ej. jperez" required />
          </FormField>
          <FormField label="Nombre de la persona" required>
            <Input value={form.nombre} onChange={cambiar('nombre')} placeholder="ej. Juan Pérez" required />
          </FormField>
          <FormField label="Contraseña inicial" required>
            <PasswordInput
              value={form.password}
              onChange={cambiar('password')}
              placeholder="Mínimo 6 caracteres"
              required
            />
          </FormField>
          <FormField label="Rol" required>
            <Select value={form.rol} onChange={cambiar('rol')}>
              {ROLES.map((r) => (
                <option key={r.value} value={r.value}>
                  {r.label}
                </option>
              ))}
            </Select>
          </FormField>
          <FormField label="Email">
            <Input type="email" value={form.email} onChange={cambiar('email')} placeholder="opcional" />
          </FormField>
          <FormField label="Teléfono">
            <Input value={form.telefono} onChange={cambiar('telefono')} placeholder="opcional" />
          </FormField>
        </div>

        <p className="text-xs text-muted">
          La persona deberá cambiar la contraseña en su primer inicio de sesión.
        </p>

        <div className="flex justify-end gap-2 pt-1">
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" variant="primary" disabled={enviando}>
            {enviando ? 'Creando…' : 'Crear cuenta'}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
