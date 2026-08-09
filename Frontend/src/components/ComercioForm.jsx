import { useState } from 'react';
import { api } from '../api';
import Modal from './Modal';

const ESTADOS = ['Nuevo', 'Contactado', 'Interesado', 'Documentacion', 'Aprobado', 'Rechazado'];

const VACIO = {
  razonSocial: '',
  cuit: '',
  nombreDelContacto: '',
  telefono: '',
  direccion: '',
  email: '',
  rubro: '',
  notas: '',
};

function aNull(valor) {
  return valor?.trim() ? valor.trim() : null;
}

export default function ComercioForm({ comercio, onClose, onGuardado }) {
  const esNuevo = !comercio;
  const [form, setForm] = useState(
    esNuevo ? { ...VACIO } : { ...VACIO, ...pickCampos(comercio), estado: comercio.estado }
  );
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState(null);

  const set = (campo) => (e) => setForm((f) => ({ ...f, [campo]: e.target.value }));

  const guardar = async (e) => {
    e.preventDefault();
    setGuardando(true);
    setError(null);
    const cuerpo = {
      razonSocial: form.razonSocial.trim(),
      nombreDelContacto: aNull(form.nombreDelContacto),
      telefono: aNull(form.telefono),
      direccion: aNull(form.direccion),
      email: aNull(form.email),
      rubro: aNull(form.rubro),
      notas: aNull(form.notas),
    };
    try {
      if (esNuevo) {
        cuerpo.cuit = form.cuit.trim();
        await api.post('/api/comercios', cuerpo);
      } else {
        cuerpo.estado = form.estado;
        await api.put(`/api/comercios/${comercio.id}`, cuerpo);
      }
      onGuardado();
    } catch (err) {
      setError(err.message);
    } finally {
      setGuardando(false);
    }
  };

  return (
    <Modal title={esNuevo ? 'Nuevo comercio' : `Editar: ${comercio.razonSocial}`} onClose={onClose}>
      <form onSubmit={guardar} className="form">
        <label>
          Razón social *
          <input required value={form.razonSocial} onChange={set('razonSocial')} maxLength={200} />
        </label>

        {esNuevo && (
          <label>
            CUIT (11 dígitos) *
            <input
              required
              value={form.cuit}
              onChange={set('cuit')}
              pattern="\d{11}"
              title="El CUIT debe tener exactamente 11 dígitos"
              maxLength={11}
            />
          </label>
        )}

        {!esNuevo && (
          <label>
            Estado
            <select value={form.estado} onChange={set('estado')}>
              {ESTADOS.map((e) => (
                <option key={e} value={e}>
                  {e}
                </option>
              ))}
            </select>
            <small>Las transiciones inválidas son rechazadas por el backend (409).</small>
          </label>
        )}

        <label>
          Nombre del contacto
          <input value={form.nombreDelContacto} onChange={set('nombreDelContacto')} maxLength={150} />
        </label>

        <div className="form-dos-col">
          <label>
            Teléfono
            <input value={form.telefono} onChange={set('telefono')} maxLength={50} />
          </label>
          <label>
            Email
            <input type="email" value={form.email} onChange={set('email')} maxLength={150} />
          </label>
        </div>

        <div className="form-dos-col">
          <label>
            Dirección
            <input value={form.direccion} onChange={set('direccion')} maxLength={150} />
          </label>
          <label>
            Rubro
            <input value={form.rubro} onChange={set('rubro')} maxLength={100} />
          </label>
        </div>

        <label>
          Notas
          <textarea value={form.notas} onChange={set('notas')} rows={3} maxLength={2000} />
        </label>

        {error && <div className="alert alert-error">{error}</div>}

        <div className="form-acciones">
          <button type="button" className="btn" onClick={onClose} disabled={guardando}>
            Cancelar
          </button>
          <button type="submit" className="btn btn-primary" disabled={guardando}>
            {guardando ? 'Guardando…' : 'Guardar'}
          </button>
        </div>
      </form>
    </Modal>
  );
}

function pickCampos(c) {
  return {
    razonSocial: c.razonSocial ?? '',
    cuit: c.cuit ?? '',
    nombreDelContacto: c.nombreDelContacto ?? '',
    telefono: c.telefono ?? '',
    direccion: c.direccion ?? '',
    email: c.email ?? '',
    rubro: c.rubro ?? '',
    notas: c.notas ?? '',
  };
}
