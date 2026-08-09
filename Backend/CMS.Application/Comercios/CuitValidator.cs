namespace CMS.Application.Comercios;

/// <summary>
/// Valida un CUIT/CUIL argentino: 11 dígitos + dígito verificador (módulo 11).
/// </summary>
public static class CuitValidator
{
    private static readonly int[] Pesos = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

    public static bool EsValido(string? cuit)
    {
        if (string.IsNullOrWhiteSpace(cuit))
        {
            return false;
        }

        var digitos = new string(cuit.Where(char.IsDigit).ToArray());
        if (digitos.Length != 11)
        {
            return false;
        }

        var suma = 0;
        for (var i = 0; i < 10; i++)
        {
            suma += (digitos[i] - '0') * Pesos[i];
        }

        var resto = suma % 11;
        int verificador;

        if (resto == 1)
        {
            // Caso especial AFIP: el verificador es 4 para ciertos prefijos, 9 para el resto.
            var prefijo = int.Parse(digitos.Substring(0, 2));
            verificador = prefijo is 20 or 23 or 24 or 27 or 30 or 33 or 34 ? 4 : 9;
        }
        else
        {
            verificador = 11 - resto;
            if (verificador == 11)
            {
                verificador = 0;
            }
        }

        return verificador == digitos[10] - '0';
    }
}
