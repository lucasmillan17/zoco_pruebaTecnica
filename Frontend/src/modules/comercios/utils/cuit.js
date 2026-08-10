const PESOS = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];

export function cuitValido(cuit) {
  if (!cuit || !cuit.trim()) return false;

  const digitos = cuit.replace(/\D/g, '');
  if (digitos.length !== 11) return false;

  let suma = 0;
  for (let i = 0; i < 10; i++) {
    suma += Number(digitos[i]) * PESOS[i];
  }

  const resto = suma % 11;
  let verificador;

  if (resto === 1) {
    const prefijo = Number(digitos.slice(0, 2));
    verificador = [20, 23, 24, 27, 30, 33, 34].includes(prefijo) ? 4 : 9;
  } else {
    verificador = 11 - resto;
    if (verificador === 11) verificador = 0;
  }

  return verificador === Number(digitos[10]);
}
