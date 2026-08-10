# zoco_pruebaTecnica

Prueba técnica Zoco: **CMS de comercios**. Gestión del pipeline comercial de comercios:
registro de comercios, interacciones, tipos de interacción, análisis de oportunidad
con IA y auditoría de cambios.

## Estructura del repo

| Carpeta | Descripción |
|---------|-------------|
| `Backend/` | API REST en **.NET 10** (clean architecture) con EF Core + PostgreSQL |
| `Frontend/` | SPA en **React 19 + Vite** + Tailwind CSS |
| `docs/` | Documentación de la API y notas de desarrollo |

## Deploy

- **API (producción):** https://cms-api.onrender.com (Render web service, PostgreSQL en Render)
- **Frontend:** Render static site (el build apunta a la API de producción vía `VITE_API_URL`)

## Levantar en local

### API

```bash
cd Backend
dotnet run --project CMS.Api
```

- Corre en `http://localhost:5000`
- Swagger disponible solo en Development en `/swagger`
- Requiere PostgreSQL y la sección `Jwt` en `appsettings.json`

### Frontend

```bash
cd Frontend
npm install
npm run dev
```

- Corre en `http://localhost:5173` (Vite)

## Usuarios de prueba (seed)

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| `admin` | `Admin123!` | Administrador |
| `ventas` | `Ventas123!` | Ventas |

Ambos arrancan con `debeCambiarPassword: true`: en el primer login la API obliga a
cambiar la contraseña.

## API REST

La API cubre: autenticación (JWT), comercios (con filtros, validación de CUIT y
transiciones de estado), interacciones, tipos de interacción, análisis de oportunidad
con IA (con fallback heurístico) y auditoría.

📄 **[Documentación completa de la API → `docs/API.md`](docs/API.md)**

Resumen de recursos:

| Recurso | Endpoints principales |
|---------|----------------------|
| Auth | `POST /api/auth/login` (público), `GET /api/auth/me`, `PUT /api/auth/password`, gestión de usuarios (Admin) |
| Comercios | `GET/POST /api/comercios`, `GET/PUT/DELETE /api/comercios/{id}`, `GET /api/comercios/validar-cuit`, `POST /api/comercios/{id}/reactivar`, `POST /api/comercios/{id}/oportunidad` |
| Interacciones | `GET/POST /api/interacciones`, `GET/PUT/DELETE /api/interacciones/{id}` |
| Tipos de interacción | `GET/POST /api/tipos-interaccion`, `GET/PUT/DELETE /api/tipos-interaccion/{id}`, `POST /api/tipos-interaccion/{id}/reactivar` |
| Auditoría | `GET /api/auditoria` (Admin) |
| Health | `GET /api/health` (público) |

Autenticación: JWT Bearer — obtener token con `POST /api/auth/login` y enviarlo como
`Authorization: Bearer <token>`.
