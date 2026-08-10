import { useState } from 'react';
import Modal from '../../../shared/components/atoms/Modal';
import Button from '../../../shared/components/atoms/Button';
import Alert from '../../../shared/components/atoms/Alert';
import Input from '../../../shared/components/atoms/Input';
import FormField from '../../../shared/components/molecules/FormField';
import { useToast } from '../../../shared/context/ToastProvider';
import { useValidarCuit } from '../hooks/useValidarCuit';
import { comerciosService } from '../services/comerciosService';

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
  const toast = useToast();
  const [form, setForm] = useState(
    esNuevo ? { ...VACIO } : { ...VACIO, ...pickCampos(comercio), estado: comercio.estado }
  );
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState(null);
  const { feedback, bloqueado } = useValidarCuit(form.cuit, esNuevo);

  const set = (campo) => (e) => setForm((f) => ({ ...f, [campo]: e.target.value }));

  const guardar = async (e) => {
    e.preventDefault();
    if (bloqueado) return;
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
        await comerciosService.create(cuerpo);
      } else {
        cuerpo.estado = form.estado;
        await comerciosService.update(comercio.id, cuerpo);
      }
      toast.success(esNuevo ? 'Comercio creado.' : 'Comercio actualizado.');
      onGuardado();
    } catch (err) {
      setError(err.message);
    } finally {
      setGuardando(false);
    }
  };

  return (
    <Modal title={esNuevo ? 'Nuevo comercio' : `Editar: ${comercio.razonSocial}`} onClose={onClose}>
      <form onSubmit={guardar} className="space-y-4">
        <FormField label="Razón social" required>
          <Input required value={form.razonSocial} onChange={set('razonSocial')} maxLength={200} />
        </FormField>

        {esNuevo && (
          <FormField
            label="CUIT (11 dígitos)"
            required
            feedback={feedback?.texto}
            feedbackTono={feedback?.tono ?? 'neutral'}
          >
            <Input
              required
              invalid={feedback?.tono === 'error'}
              value={form.cuit}
              onChange={set('cuit')}
              pattern="\d{11}"
              title="El CUIT debe tener exactamente 11 dígitos"
              maxLength={11}
            />
          </FormField>
        )}

        {!esNuevo && (
          <FormField
            label="Estado"
            feedback="Las transiciones inválidas son rechazadas por el backend (409)."
          >
            <select
              value={form.estado}
              onChange={set('estado')}
              className="w-full rounded-md border border-border bg-surface px-3 py-2 text-sm text-text focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary"
            >
              {ESTADOS.map((e) => (
                <option key={e} value={e}>
                  {e}
                </option>
              ))}
            </select>
          </FormField>
        )}

        <FormField label="Nombre del contacto">
          <Input value={form.nombreDelContacto} onChange={set('nombreDelContacto')} maxLength={150} />
        </FormField>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <FormField label="Teléfono">
            <Input value={form.telefono} onChange={set('telefono')} maxLength={50} />
          </FormField>
          <FormField label="Email">
            <Input type="email" value={form.email} onChange={set('email')} maxLength={150} />
          </FormField>
          <FormField label="Dirección">
            <Input value={form.direccion} onChange={set('direccion')} maxLength={150} />
          </FormField>
          <FormField label="Rubro">
            <Input value={form.rubro} onChange={set('rubro')} maxLength={100} />
          </FormField>
        </div>

        <FormField label="Notas">
          <textarea
            value={form.notas}
            onChange={set('notas')}
            rows={3}
            maxLength={2000}
            className="w-full rounded-md border border-border bg-surface px-3 py-2 text-sm text-text placeholder:text-muted focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary"
          />
        </FormField>

        {error && <Alert tono="error">{error}</Alert>}

        <div className="flex justify-end gap-2 border-t border-gray-200 pt-4">
          <Button onClick={onClose} disabled={guardando}>
            Cancelar
          </Button>
          <Button type="submit" variant="primary" disabled={guardando || bloqueado}>
            {guardando ? 'Guardando…' : 'Guardar'}
          </Button>
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
