import { useEffect, useRef, useState } from 'react';
import { comerciosService } from '../services/comerciosService';
import { cuitValido } from '../utils/cuit';

export function useValidarCuit(cuit, enabled = true) {
  const [existe, setExiste] = useState(null);
  const [revisando, setRevisando] = useState(false);
  const consultadoRef = useRef(null);

  const digitos = cuit.replace(/\D/g, '');
  const completo = digitos.length === 11;
  const checksumOk = cuitValido(cuit);

  useEffect(() => {
    if (!enabled) {
      setRevisando(false);
      setExiste(null);
      consultadoRef.current = null;
      return;
    }

    if (!completo || !checksumOk) {
      setRevisando(false);
      setExiste(null);
      consultadoRef.current = null;
      return;
    }

    setRevisando(true);
    const timeout = setTimeout(async () => {
      try {
        const res = await comerciosService.validarCuit(cuit);
        if (consultadoRef.current === cuit) {
          setExiste(res.existe);
          setRevisando(false);
        }
      } catch {
        if (consultadoRef.current === cuit) {
          setExiste(null);
          setRevisando(false);
        }
      }
    }, 400);

    consultadoRef.current = cuit;
    return () => clearTimeout(timeout);
  }, [enabled, cuit, completo, checksumOk]);

  let feedback = null;
  if (digitos.length > 0) {
    if (!completo) {
      feedback = { texto: `Debe tener 11 dígitos (van ${digitos.length}).`, tono: 'neutral' };
    } else if (!checksumOk) {
      feedback = { texto: 'El CUIT no es válido (dígito verificador incorrecto).', tono: 'error' };
    } else if (existe === true) {
      feedback = { texto: 'Ya existe un comercio con este CUIT.', tono: 'error' };
    } else if (revisando) {
      feedback = { texto: 'Verificando…', tono: 'neutral' };
    } else if (existe === false) {
      feedback = { texto: 'CUIT válido.', tono: 'ok' };
    }
  }

  return {
    feedback,
    bloqueado: completo && (!checksumOk || existe === true),
  };
}
