# API REST — CMS Zoco

API de gestión de comercios para el equipo de ventas. Permite administrar comercios,
registrar interacciones, definir tipos de interacción y analizar oportunidades con IA.

- **Base URL (producción):** `https://cms-api.onrender.com`
- **Base URL (local):** `http://localhost:5000`
- **Formato:** JSON
- **Enums:** se serializan como strings (ej: `"Nuevo"`, `"Administrador"`). En query
  strings y body se aceptan indistintamente mayúsculas/minúsculas.

---

## Autenticación

Toda la API (salvo `login` y `health`) requiere un token JWT en el header:

```
Authorization: Bearer <token>
```

### Obtener un token

```
POST /api/auth/login
```

Body:

```json
{
  "usuario": "admin",
  "password": "Admin123!"
}
```

Response `200 OK`:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "usuario": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nombreUsuario": "admin",
    "nombre": "Administrador",
    "rol": "Administrador",
    "activo": true,
    "debeCambiarPassword": true,
    "email": "admin@cmszoco.local",
    "telefono": null,
    "createdBy": null,
    "updatedBy": null
  }
}
```

### Usuarios iniciales (seed)

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| `admin` | `Admin123!` | Administrador |
| `ventas` | `Ventas123!` | Ventas |

Ambos arrancan con `debeCambiarPassword: true`, por lo que en el primer inicio de
sesión la API obliga a cambiarla mediante `PUT /api/auth/password`.

### Roles

| Rol | Permisos |
|-----|----------|
| `Administrador` | Acceso total: administra usuarios, tipos de interacción y auditoría |
| `Ventas` | Gestiona comercios e interacciones |

El token expira a las **8 horas**. Ante un token inválido o vencido la API responde
`401` y el frontend redirige al login.

---

## Convenciones comunes

### Paginación

Los listados devuelven un objeto paginado:

```json
{
  "items": [],
  "totalCount": 42,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `pageNumber` | int | `1` | Página actual (inicia en 1) |
| `pageSize` | int | `10` (auditoría: `20`) | Elementos por página |

### Fechas

Se envían y devuelven en **UTC** (ISO 8601). Ejemplos: `2026-08-10T14:30:00Z`,
`2026-08-10`.

---

## Endpoints

### Health check

#### `GET /api/health`

Sin autenticación. Devuelve el estado del servicio (lo usa Render para los health checks).

```json
{ "status": "ok" }
```

---

### Auth — `/api/auth`

#### `POST /api/auth/login`

Público. Autentica y devuelve el token. Ver [Autenticación](#autenticación).

Errores:
- `400` si falta `usuario` o `password`.
- `401` si las credenciales son incorrectas o el usuario está inactivo.

#### `GET /api/auth/me`

Requiere token. Devuelve el usuario autenticado.

Response `200 OK`: objeto `usuario` (ver login).

#### `PUT /api/auth/password`

Requiere token. Cambia la contraseña del usuario autenticado.

Body:

```json
{
  "passwordActual": "Admin123!",
  "passwordNueva": "Nueva123!"
}
```

Response `200 OK`: el `usuario` actualizado (con `debeCambiarPassword: false`).

Errores:
- `401` si `passwordActual` es incorrecta.
- `400` si `passwordNueva` tiene menos de 6 caracteres.

#### `GET /api/auth/usuarios`

Rol **Administrador**. Lista todos los usuarios (activos primero).

#### `POST /api/auth/usuarios`

Rol **Administrador**. Crea un usuario.

Body:

```json
{
  "nombreUsuario": "soporte",
  "nombre": "Usuario de Soporte",
  "password": "Soporte123!",
  "rol": "Ventas",
  "email": "soporte@cmszoco.local",
  "telefono": "11-5555-1234"
}
```

`rol` puede ser `Administrador` o `Ventas`. `email` y `telefono` son opcionales.

Response `201 Created`.

Errores:
- `409` si ya existe un usuario con ese `nombreUsuario`.
- `400` si falta un campo obligatorio o `password` tiene menos de 6 caracteres.

#### `POST /api/auth/usuarios/{id}/desactivar`

Rol **Administrador**. Desactiva un usuario (no se elimina; no podrá loguearse).

Response `200 OK`: el `usuario` desactivado.

Errores:
- `404` si el usuario no existe.
- `409` si intentás desactivar tu propia cuenta o la de otro administrador.

---

### Comercios — `/api/comercios`

#### `GET /api/comercios`

Lista comercios con filtros combinables y paginación.

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `busqueda` | string | Filtra por razón social, CUIT, contacto o email (contiene, case-insensitive) |
| `estado` | string | `Nuevo`, `Contactado`, `Interesado`, `Documentacion`, `Aprobado`, `Rechazado` |
| `rubro` | string | Filtra por rubro (contiene) |
| `ordenarPor` | string | `RazonSocial`, `Rubro`, `Cuit`, `Estado`, `FechaCreacion`, `UltimoContacto` |
| `orden` | string | `Asc` / `Desc` (default `Desc`) |
| `estadoActivo` | string | `Activos` (default), `Inactivos`, `Todos` |
| `pageNumber`, `pageSize` | int | Paginación |

Ejemplo:

```
GET /api/comercios?busqueda=gastronomia&estado=Interesado&ordenarPor=RazonSocial&orden=Asc&pageNumber=1&pageSize=10
```

Response `200 OK` (paginado). Cada item:

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "razonSocial": "Panadería Don Pedro",
  "cuit": "20345678901",
  "nombreDelContacto": "Pedro Gómez",
  "telefono": "11-5555-0001",
  "direccion": "Av. Siempreviva 742",
  "email": "contacto@donpedro.com",
  "rubro": "Gastronomía",
  "fechaDeCreacionEmpresa": "2026-08-10T14:30:00Z",
  "notas": "Prefieren cobrar con QR.",
  "estado": "Interesado",
  "activo": true,
  "createdAt": "2026-08-10T14:30:00Z",
  "updatedAt": "2026-08-10T14:30:00Z",
  "createdBy": "ventas",
  "updatedBy": "ventas"
}
```

#### `GET /api/comercios/{id}`

Devuelve un comercio por ID. `404` si no existe (o está inactivo).

#### `GET /api/comercios/validar-cuit?cuit={cuit}`

Valida el checksum del CUIT y devuelve si ya existe un comercio activo con ese CUIT.
Útil para validación en tiempo real en formularios.

Response `200 OK`:

```json
{ "esValido": true, "existe": false }
```

#### `POST /api/comercios`

Crea un comercio con estado inicial `Nuevo`.

Body:

```json
{
  "razonSocial": "Panadería Don Pedro",
  "cuit": "20345678901",
  "nombreDelContacto": "Pedro Gómez",
  "telefono": "11-5555-0001",
  "direccion": "Av. Siempreviva 742",
  "email": "contacto@donpedro.com",
  "rubro": "Gastronomía",
  "notas": "Prefieren cobrar con QR."
}
```

Todos los campos excepto `razonSocial` y `cuit` son opcionales. El CUIT se valida
(11 dígitos + dígito verificador) y no se puede modificar después.

Response `201 Created` con el comercio completo.

Errores:
- `400` si `razonSocial` o `cuit` faltan, o `cuit` no tiene 11 dígitos / email inválido.
- `409` si el CUIT es inválido (checksum) o ya existe un comercio activo con ese CUIT.

#### `PUT /api/comercios/{id}`

Actualiza un comercio. El CUIT es inmutable. Permite cambiar el `estado` según las
transiciones válidas (ver [Reglas de negocio](#reglas-de-negocio)).

Body:

```json
{
  "razonSocial": "Panadería Don Pedro SA",
  "nombreDelContacto": "Pedro Gómez",
  "telefono": "11-5555-0001",
  "direccion": "Av. Siempreviva 742",
  "email": "contacto@donpedro.com",
  "rubro": "Gastronomía",
  "notas": "Prefieren cobrar con QR.",
  "estado": "Documentacion"
}
```

Response `200 OK` con el comercio actualizado.

Errores:
- `404` si el comercio no existe.
- `409` si la transición de estado es inválida (ej: pasar de `Nuevo` a `Aprobado`).
- `400` si falta `razonSocial` o el email es inválido.

#### `DELETE /api/comercios/{id}`

**Soft delete**: marca el comercio como inactivo (`activo: false`). Las interacciones
se conservan y el CUIT queda disponible para re-crear. Si el comercio está inactivo,
la mayoría de las operaciones responden `404`.

Response `204 No Content`. `404` si no existe.

#### `POST /api/comercios/{id}/reactivar`

Re-activa un comercio inactivo o rechazado, volviéndolo a estado `Nuevo`.

Response `200 OK` con el comercio reactivado.

Errores:
- `404` si no existe.
- `409` si ya está activo y no está en estado `Rechazado`.

#### `POST /api/comercios/{id}/oportunidad`

Analiza la oportunidad comercial del comercio usando sus datos e interacciones.
Intenta con el proveedor de IA (Gemini) y, si no está configurado o falla, usa un
analizador heurístico determinista (funciona siempre).

Response `200 OK`:

```json
{
  "resumen": "Comercio de rubro Gastronomía, en estado Interesado, con 3 interacciones registradas. Registró 2 interacciones en el último mes.",
  "nivelInteres": "alto",
  "proximoPaso": "Coordinar demo de POS + QR y detallar la solución de conciliación.",
  "preguntas": [
    "¿Cuál es su volumen mensual de ventas aproximado (Gastronomía)?",
    "¿Cuántas cajas o terminales de cobro necesita operar?",
    "¿Con qué métodos de pago cobra hoy y tiene problemas de conciliación?"
  ],
  "datosFaltantes": ["Volumen mensual aproximado", "Cantidad de cajas / terminales"]
}
```

`nivelInteres` puede ser `alto`, `medio` o `bajo`.

`404` si el comercio no existe o está inactivo.

---

### Interacciones — `/api/interacciones`

#### `GET /api/interacciones`

Lista interacciones de un comercio, ordenadas por fecha descendente.

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `comercioId` | uuid | **Obligatorio.** ID del comercio |
| `tipoInteraccionId` | uuid | Filtra por tipo |
| `desde` | fecha | Filtra interacciones desde (incluida) |
| `hasta` | fecha | Filtra interacciones hasta (incluida) |
| `pageNumber`, `pageSize` | int | Paginación |

Ejemplo:

```
GET /api/interacciones?comercioId=3fa85f64-5717-4562-b3fc-2c963f66afa6&tipoInteraccionId=11111111-1111-1111-1111-111111111101&pageSize=20
```

Response `200 OK` (paginado). Cada item:

```json
{
  "id": "4fa85f64-5717-4562-b3fc-2c963f66afa6",
  "comercioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tipoInteraccionId": "11111111-1111-1111-1111-111111111101",
  "tipoNombre": "Llamada telefónica",
  "fechaInteraccion": "2026-08-10T14:30:00Z",
  "notas": "Quiere saber costos de comisión.",
  "createdAt": "2026-08-10T14:30:00Z",
  "createdBy": "ventas",
  "updatedBy": null
}
```

#### `GET /api/interacciones/{id}`

Devuelve una interacción. `404` si no existe.

#### `POST /api/interacciones`

Registra una interacción.

Body:

```json
{
  "comercioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tipoInteraccionId": "11111111-1111-1111-1111-111111111101",
  "fechaInteraccion": "2026-08-10T14:30:00Z",
  "notas": "Quiere saber costos de comisión."
}
```

`fechaInteraccion` es opcional (si no se envía, se usa la fecha actual). `notas` es
opcional (máx. 2000 caracteres).

Response `201 Created`.

Errores:
- `404` si el comercio o el tipo de interacción no existen o están inactivos.
- `400` si `comercioId`/`tipoInteraccionId` faltan o `notas` supera los 2000 caracteres.

#### `PUT /api/interacciones/{id}`

Actualiza una interacción. Todos los campos son opcionales.

Body:

```json
{
  "tipoInteraccionId": "11111111-1111-1111-1111-111111111102",
  "fechaInteraccion": "2026-08-11T10:00:00Z",
  "notas": "Actualizada tras la demo."
}
```

Response `200 OK`. `404` si la interacción o el nuevo tipo no existen.

#### `DELETE /api/interacciones/{id}`

Elimina una interacción (borrado real).

Response `204 No Content`. `404` si no existe.

---

### Tipos de interacción — `/api/tipos-interaccion`

Los tipos se usan para clasificar las interacciones (llamada, demo, reunión, etc.).
El `codigo` se asigna al crear y **no se puede modificar**.

#### `GET /api/tipos-interaccion`

Lista tipos. Cualquier usuario autenticado.

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `estadoActivo` | string | `Activos` (default), `Inactivos`, `Todos` |
| `pageNumber`, `pageSize` | int | Paginación |

Response `200 OK` (paginado). Cada item:

```json
{
  "id": "11111111-1111-1111-1111-111111111101",
  "codigo": "llamada",
  "nombre": "Llamada telefónica",
  "descripcion": "Contacto telefónico con el comercio.",
  "activo": true,
  "createdBy": "admin",
  "updatedBy": null
}
```

#### `GET /api/tipos-interaccion/{id}`

Devuelve un tipo. `404` si no existe.

#### `POST /api/tipos-interaccion`

Rol **Administrador**. Crea un tipo.

Body:

```json
{
  "codigo": "demo",
  "nombre": "Demo",
  "descripcion": "Demostración de la solución."
}
```

El código debe cumplir `^[a-z][a-z0-9_]*$` (minúsculas, números y guion bajo,
empezando con letra). Se normaliza a minúsculas.

Response `201 Created`.

Errores:
- `409` si el código ya existe o no cumple el formato.
- `400` si `codigo` o `nombre` faltan.

#### `PUT /api/tipos-interaccion/{id}`

Rol **Administrador**. Edita `nombre` y `descripcion` (el código es inmutable).

Body:

```json
{
  "nombre": "Demo en sucursal",
  "descripcion": "Demostración presencial de la solución."
}
```

Response `200 OK`. `404` si no existe.

#### `DELETE /api/tipos-interaccion/{id}`

Rol **Administrador**. **Soft delete**: marca `activo: false`. Las interacciones
existentes conservan el tipo.

Response `204 No Content`. `404` si no existe.

#### `POST /api/tipos-interaccion/{id}/reactivar`

Rol **Administrador**. Re-activa el tipo (`activo: true`).

Response `200 OK`. `404` si no existe.

---

### Auditoría — `/api/auditoria`

Rol **Administrador**. Historial de cambios registrado automáticamente por la API
(una fila por campo modificado en cada alta/baja/modificación de comercios,
interacciones, tipos y usuarios).

#### `GET /api/auditoria`

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `entidad` | string | `Comercio`, `Interaccion`, `TipoInteraccion`, `Usuario` |
| `usuario` | string | Filtra por nombre de usuario (contiene) |
| `operacion` | string | `Crear`, `Actualizar`, `Eliminar` |
| `desde`, `hasta` | fecha | Rango de fechas |
| `pageNumber`, `pageSize` | int | Paginación (default `pageSize=20`) |

Response `200 OK` (paginado). Cada item:

```json
{
  "id": "5fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fecha": "2026-08-10T14:30:00Z",
  "usuario": "ventas",
  "rol": "Ventas",
  "entidad": "Comercio",
  "entidadId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "operacion": "Actualizar",
  "campo": "Estado",
  "valorAnterior": "Nuevo",
  "valorNuevo": "Interesado"
}
```

---

## Errores

Todos los errores usan el estándar **RFC 7807 (`ProblemDetails`)**:

```json
{
  "status": 409,
  "title": "Conflicto",
  "detail": "Ya existe un comercio con el CUIT 20345678901.",
  "instance": "/api/comercios"
}
```

| Código | Title | Cuándo ocurre |
|--------|-------|---------------|
| `400` | — | Validación de modelo (campos obligatorios, formato de CUIT/email, enum inválido, fecha mal formateada). Incluye `errors` con los detalles por campo |
| `401` | No autorizado | Token faltante, inválido o vencido; credenciales incorrectas; usuario desactivado |
| `403` | — | El token es válido pero el rol no tiene permiso para el recurso |
| `404` | No encontrado | El recurso no existe (o está inactivo y no es visible) |
| `409` | Conflicto | Regla de negocio violada: CUIT duplicado/inválido, transición de estado inválida, código duplicado, reactivar algo activo, desactivar propia cuenta o admin |
| `502` | Servicio externo no disponible | Fallo del servicio externo de IA (en oportunidad degrada a heurística) |
| `500` | Error interno del servidor | Error no controlado |

### Ejemplo de error `400` con validación de campos

```json
{
  "status": 400,
  "title": "One or more validation errors occurred.",
  "errors": {
    "Cuit": ["El CUIT debe tener 11 dígitos."],
    "RazonSocial": ["La razón social es obligatoria."]
  }
}
```

---

## Reglas de negocio

### CUIT

- Debe tener exactamente **11 dígitos** (`^\d{11}$`).
- Se valida el **dígito verificador** (checksum).
- Es **inmutable** una vez creado y **único entre comercios activos**.
- Al hacer soft delete, el CUIT queda disponible para re-crear.

### Transiciones de estado del pipeline

El estado de un comercio solo puede cambiar a las siguientes transiciones:

| Desde | Hacia |
|-------|-------|
| `Nuevo` | `Contactado`, `Rechazado` |
| `Contactado` | `Interesado`, `Rechazado` |
| `Interesado` | `Documentacion`, `Rechazado` |
| `Documentacion` | `Aprobado`, `Rechazado` |
| `Aprobado` | *(terminal)* |
| `Rechazado` | *(terminal; solo se re-activa con `reactivar`)* |

### Soft delete

- **Comercios** y **tipos de interacción**: `DELETE` marca `activo: false` (soft delete).
- **Interacciones**: `DELETE` borra el registro realmente.
- Un comercio/tipo inactivo no aparece en los listados por defecto y responde `404`
  en las operaciones sobre él.

### Auditoría automática

Cada `Crear`/`Actualizar`/`Eliminar` registra una entrada por campo modificado en la
tabla de auditoría (quién, cuándo, valor anterior y nuevo). El campo `createdBy`/
`updatedBy` de cada entidad también se completa con el usuario autenticado.

---

## Ejemplo de flujo completo

Walkthrough usando `curl` (base de producción). Los IDs de tipo de interacción de
ejemplo corresponden al seed inicial.

**1. Login**

```bash
curl -X POST https://cms-api.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usuario":"admin","password":"Admin123!"}'
```

Guardá el `token` de la respuesta.

**2. Crear un comercio**

```bash
curl -X POST https://cms-api.onrender.com/api/comercios \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "razonSocial": "Farmacia San Jorge",
    "cuit": "20345678901",
    "rubro": "Salud",
    "nombreDelContacto": "María López",
    "email": "sanjorge@mail.com",
    "notas": "Cobra con efectivo hoy."
  }'
```

La respuesta incluye el `id` del comercio.

**3. Registrar una interacción**

```bash
curl -X POST https://cms-api.onrender.com/api/interacciones \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "comercioId": "<id-del-comercio>",
    "tipoInteraccionId": "11111111-1111-1111-1111-111111111101",
    "notas": "Quiere conocer la comisión y si puede operar con QR."
  }'
```

**4. Avanzar el estado a Interesado**

```bash
curl -X PUT https://cms-api.onrender.com/api/comercios/<id-del-comercio> \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"razonSocial":"Farmacia San Jorge","rubro":"Salud","estado":"Contactado"}'
```

Luego repetir el PUT con `"estado":"Interesado"`.

**5. Analizar la oportunidad**

```bash
curl -X POST https://cms-api.onrender.com/api/comercios/<id-del-comercio>/oportunidad \
  -H "Authorization: Bearer <token>"
```

Devuelve `resumen`, `nivelInteres`, `proximoPaso`, `preguntas` y `datosFaltantes`.

**6. Consultar la auditoría (Admin)**

```bash
curl -X GET "https://cms-api.onrender.com/api/auditoria?entidad=Comercio&operacion=Actualizar" \
  -H "Authorization: Bearer <token>"
```
