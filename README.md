# Plataforma de Créditos — Gestión de Solicitudes y Evaluación

Plataforma web interna desarrollada para una entidad financiera. Permite a los usuarios autenticados registrar solicitudes de crédito, asegurando reglas de negocio como evitar que el cliente exceda su capacidad de pago y garantizando que solo exista una solicitud activa por cliente. Los analistas de riesgo utilizan la plataforma para evaluar, aprobar o rechazar dichas solicitudes.

## 🚀 Tecnologías (Stack)

- **Framework**: ASP.NET Core MVC (.NET 8)
- **Autenticación**: ASP.NET Core Identity
- **ORM & Base de Datos**: Entity Framework Core con SQLite (Local) / PostgreSQL (Opcional en Prod)
- **Caché y Sesiones**: Redis (Gestionado en RedisLabs)
- **Vistas**: Razor Views
- **Despliegue**: Render.com (Web Service)

## ⚙️ Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Una instancia de [Redis](https://app.redislabs.com/) en la nube o instalada localmente para la gestión de sesiones.
- Herramientas de CLI de Entity Framework Core (`dotnet tool install --global dotnet-ef`).
- Git.

## 🛠️ Variables de Entorno

Para ejecutar la aplicación correctamente, se deben configurar las siguientes variables de entorno. 

En un entorno local de desarrollo, puedes añadirlas en tu archivo `appsettings.Development.json` o usando `dotnet user-secrets`.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db",
    "RedisConnection": "TU_CADENA_DE_CONEXION_REDIS_AQUI"
  }
}
```

*Nota: Asegúrate de no subir nunca contraseñas ni cadenas de conexión reales de producción al repositorio de GitHub.*

## 💻 Pasos Locales (Instalación y Ejecución)

1. **Clonar el repositorio**:
   ```bash
   git clone <URL_DEL_REPOSITORIO>
   cd "Examen Parcial"
   ```

2. **Restaurar las dependencias**:
   ```bash
   dotnet restore
   ```

3. **Aplicar las migraciones (Base de Datos)**:
   Asegúrate de estar en la carpeta donde se encuentra el `.csproj`.
   ```bash
   dotnet ef database update
   ```

4. **Ejecutar la aplicación**:
   ```bash
   dotnet run
   ```
   La aplicación se levantará y estará disponible en el navegador (usualmente en `http://localhost:5000` o `https://localhost:5001`).

## 🗄️ Migraciones de Base de Datos

El proyecto utiliza **Entity Framework Core Code-First**. Para gestionar cambios en la estructura de la base de datos:

- **Crear una nueva migración** (después de modificar los modelos):
  ```bash
  dotnet ef migrations add NombreDescriptivoDeLaMigracion
  ```
- **Aplicar migraciones pendientes**:
  ```bash
  dotnet ef database update
  ```

## ☁️ Despliegue en Render

El proyecto está diseñado para ser desplegado en [Render.com](https://render.com/) como un Web Service.

**URL de Producción:**  
🔗 `[AÑADIR_URL_DE_RENDER_AQUI]` *(Se actualizará una vez completado el despliegue)*

**Configuración en Render:**
- **Entorno:** `.NET`
- **Build Command:** `dotnet build -c Release` (o un script de bash personalizado si es necesario compilar vistas o ejecutar migraciones).
- **Start Command:** `dotnet run -c Release` (o especificar el dll generado).
- **Variables de Entorno (Environment Variables):**
  - Es mandatorio agregar la variable `ConnectionStrings__RedisConnection` con la URL de tu instancia en RedisLabs.
  - Para persistir SQLite en Render, asegúrate de configurar un [Disk](https://render.com/docs/disks) apuntando a la ruta del `app.db`.

## 🌳 Flujo de Trabajo (Git / Control de Versiones)

El desarrollo se gestiona a través de GitHub siguiendo reglas estrictas:
- La rama `main` contiene el código base. **No se permiten commits directos a `main`**.
- Cada pregunta/funcionalidad del examen se desarrolla en una rama independiente (ejemplo: `feature/pregunta-1`, `feature/evaluacion-riesgo`).
- Al finalizar el trabajo en una rama, se debe crear un **Pull Request (PR)** hacia `main` para fusionar los cambios.
