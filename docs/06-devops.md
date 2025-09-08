# DevOps y Entornos

## Variables de entorno (ejemplo)

- Backend
  - `ASPNETCORE_ENVIRONMENT=Development`
  - `DB__CONNECTION=Host=postgres;Port=5432;Database=erp;Username=erp;Password=erp`
  - `JWT__ISSUER=erp-local`
  - `JWT__AUDIENCE=erp-web`
  - `JWT__KEY=<clave-secreta-32+>`
- Frontend
  - `VITE_API_BASE=http://localhost:5000/api`

## Docker Compose (local)

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: erp
      POSTGRES_USER: erp
      POSTGRES_PASSWORD: erp
    ports:
      - "5432:5432"
    volumes:
      - db_data:/var/lib/postgresql/data
  adminer:
    image: adminer
    ports:
      - "8080:8080"
volumes:
  db_data:
```

(Agregar servicios `api` y `web` al integrar el código.)

## CI/CD (líneas base)

- Backend (.NET):
  - `dotnet restore`
  - `dotnet build --configuration Release`
  - `dotnet test --no-build`
- Frontend (Vite):
  - `npm ci`
  - `npm run build`
- Seguridad:
  - Análisis SAST/Dependabot/Whitesource (según plataforma)
- Despliegue:
  - Contenedores por servicio; variables en secretos del runner.
