import { useState } from 'react';
import Modal from '../../../shared/components/atoms/Modal';
import Button from '../../../shared/components/atoms/Button';
import Alert from '../../../shared/components/atoms/Alert';
import Input from '../../../shared/components/atoms/Input';
import FormField from '../../../shared/components/molecules/FormField';
import { tiposInteraccionService } from '../services/tiposInteraccionService';

const CODIGO_REGEX = /^[a-z][a-z0-9_]*$/;

export default function TipoInteraccionModal({ tipo, onGuardado, onClose }) {
  const esEdicion = Boolean(tipo);

  const [codigo, setCodigo] = useState(tipo?.codigo ?? '');
  const [nombre, setNombre] = useState(tipo?.nombre ?? '');
  const [descripcion, setDescripcion] = useState(tipo?.descripcion ?? '');
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState(null);

  const validar = () => {
    if (!esEdicion && !CODIGO_REGEX.test(codigo.trim())) {
      return 'El código solo puede contener letras minúsculas, números y guion bajo, y debe empezar con una letra.';
    }
    if (!nombre.trim()) {
      return 'El nombre es obligatorio.';
    }
    return null;
  };

  const guardar = async (e) => {
    e.preventDefault();
    const errorValidacion = validar();
    if (errorValidacion) {
      setError(errorValidacion);
      return;
    }
    setEnviando(true);
    setError(null);
    try {
      if (esEdicion) {
        await tiposInteraccionService.update(tipo.id, {
          nombre: nombre.trim(),
          descripcion: descripcion.trim() || null,
        });
      } else {
        await tiposInteraccionService.create({
          codigo: codigo.trim().toLowerCase(),
          nombre: nombre.trim(),
          descripcion: descripcion.trim() || null,
        });
      }
      onGuardado();
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setEnviando(false);
    }
  };

  return (
    <Modal title={esEdicion ? 'Editar tipo de interacción' : 'Nuevo tipo de interacción'} onClose={onClose}>
      {error && <Alert tono="error">{error}</Alert>}

      <form onSubmit={guardar} className="flex flex-col gap-4">
        <FormField
          label="Código"
          required
          feedback={!esEdicion ? 'Minúsculas, números y guion bajo (ej: llamada, nota_interna)' : 'No se puede modificar'}
        >
          <Input
            value={codigo}
            onChange={(e) => setCodigo(e.target.value)}
            maxLength={50}
            disabled={esEdicion}
            placeholder="llamada"
            className={esEdicion ? 'bg-gray-50 text-gray-500' : ''}
          />
        </FormField>

        <FormField label="Nombre" required>
          <Input value={nombre} onChange={(e) => setNombre(e.target.value)} maxLength={100} placeholder="Llamada telefónica" />
        </FormField>

        <FormField label="Descripción">
          <textarea
            value={descripcion}
            onChange={(e) => setDescripcion(e.target.value)}
            maxLength={300}
            rows={3}
            className="w-full rounded-md border border-border bg-surface px-3 py-2 text-sm text-text placeholder:text-muted focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary"
            placeholder="Opcional"
          />
        </FormField>

        <div className="flex justify-end gap-2 border-t border-gray-100 pt-4">
          <Button variant="secondary" onClick={onClose} disabled={enviando}>
            Cancelar
          </Button>
          <Button type="submit" variant="primary" disabled={enviando}>
            {enviando ? 'Guardando…' : esEdicion ? 'Guardar cambios' : 'Crear tipo'}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
