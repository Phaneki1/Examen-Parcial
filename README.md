# 🏦 FinCredit — Plataforma de Evaluación Crediticia

Plataforma web desarrollada con arquitectura empresarial para la gestión, evaluación y aprobación de solicitudes de crédito. Permite a los clientes enviar solicitudes bajo estrictas reglas de negocio (validación de capacidad de pago) y provee a los analistas de riesgo un dashboard profesional para su evaluación.

## 🚀 Tecnologías y Stack

- **Framework Core**: ASP.NET Core MVC (.NET 10.0)
- **Seguridad**: ASP.NET Core Identity (Autenticación y Autorización basada en Roles)
- **Base de Datos & ORM**: Entity Framework Core 10 con SQLite
- **Caché y Alto Rendimiento**: Redis (Caché Distribuida y Almacenamiento de Sesiones)
- **Diseño UI/UX**: Bootstrap 5 + Iconografía (Diseño limpio y corporativo)
- **Despliegue y DevOps**: Docker, Render.com (Como Web Service)

## ✨ Características Principales

- **Registro de Solicitudes**: Límite estricto de crédito basado en los ingresos declarados (hasta 10 veces el salario).
- **Control de Flujo**: Restricción de una sola solicitud "Pendiente" por usuario a la vez.
- **Panel de Analista**: Acceso restringido por rol (`Analista`). Permite aprobar o rechazar solicitudes. El rechazo exige un motivo obligatorio y detallado.
- **Rendimiento Acelerado**: Uso intensivo de `IDistributedCache` con Redis para mantener en memoria las listas de solicitudes por usuario, minimizando las llamadas a la base de datos.
- **Auditoría de Sesión**: Registro del último inicio de sesión utilizando `HttpContext.Session` respaldado por Redis.
- **Seguridad CSRF**: Implementación de `[ValidateAntiForgeryToken]` en todos los envíos de formularios.

## ⚙️ Requisitos Previos (Desarrollo Local)

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Instancia de [Redis](https://redis.io/) (Local o Cloud como RedisLabs)
- Docker (Opcional, para pruebas de contenedor local)
- Herramientas de CLI de Entity Framework Core (`dotnet tool install --global dotnet-ef`)

## 🛠️ Configuración del Entorno Local

1. **Clonar y restaurar**:
   ```bash
   git clone <URL_DEL_REPOSITORIO>
   cd "Examen Parcial"
   dotnet restore
   ```

2. **Configurar Secretos (Variables de Entorno)**:
   Añadir la cadena de conexión de Redis en `appsettings.Development.json` o usando User Secrets:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=app.db"
     },
     "Redis": {
       "ConnectionString": "TU_CADENA_REDIS_AQUI"
     }
   }
   ```

3. **Aplicar Migraciones**:
   ```bash
   dotnet ef database update
   ```

4. **Ejecutar la Plataforma**:
   ```bash
   dotnet run
   ```

## ☁️ Despliegue en Producción (Render & Docker)

La aplicación está completamente dockerizada para garantizar la consistencia entre el entorno de desarrollo y producción.

### Configuración en Render.com
1. Crear un nuevo **Web Service**.
2. Conectar el repositorio de GitHub y la rama `deploy/render`.
3. Seleccionar el entorno **Docker**.
4. Añadir las siguientes Variables de Entorno (Environment Variables):
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `ASPNETCORE_URLS` = `http://0.0.0.0:${PORT}`
   - `Redis__ConnectionString` = `<URL_DE_REDIS>` (Usar doble guion bajo `__` para inyección de dependencias en Linux).
   - `ConnectionStrings__DefaultConnection` = `Data Source=app.db`
5. (Opcional) Configurar un **Disk** montado en el directorio de la aplicación para preservar la base de datos SQLite `app.db` entre despliegues.

## 🌳 Arquitectura de Control de Versiones

El proyecto sigue un esquema estricto de Git:
- **`main`**: Código estable y verificado.
- **`deploy/render`**: Rama destinada exclusivamente a los despliegues automatizados hacia producción.
- **`feature/*`**: Ramas de desarrollo temporal donde se trabajan las nuevas características antes de hacer un Pull Request.
