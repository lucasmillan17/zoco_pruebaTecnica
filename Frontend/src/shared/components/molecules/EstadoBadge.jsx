import Badge from '../atoms/Badge';

const VARIANTE_POR_ESTADO = {
  Nuevo: 'gray',
  Contactado: 'blue',
  Interesado: 'indigo',
  Documentacion: 'amber',
  Aprobado: 'green',
  Rechazado: 'red',
};

export default function EstadoBadge({ estado }) {
  return <Badge variant={VARIANTE_POR_ESTADO[estado] ?? 'gray'}>{estado}</Badge>;
}
