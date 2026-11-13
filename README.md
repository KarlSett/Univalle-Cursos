# Univalle Cursos - Portal de Microcursos y Tutoriales

## Integrantes

- David Fortun
- Carlos Peralta

## Descripción

Portal web de la Universidad Privada del Valle para publicar microcursos y tutoriales técnicos.

- **Invitado:** ve el catálogo de cursos publicados y la página de presentación de cada curso (título, descripción, nivel y duración). No puede leer las lecciones.
- **Suscriptor:** se registra, se inscribe gratis en un curso, ve sus cursos, lee las lecciones, las marca como completadas y ve su progreso.
- **Administrador:** crea, edita y elimina cursos; agrega, edita, elimina y ordena lecciones; publica o despublica cursos; y ve los suscriptores inscritos en cada curso.

## Tecnologías

- ASP.NET Core MVC (.NET 8)
- ASP.NET Core Identity (usuarios y roles)
- Entity Framework Core (persistencia)
- SQL Server (base de datos)

## ¿Por qué usamos MVC y no Razor Pages?

Elegimos **MVC** porque el proyecto tiene tres tipos de usuario con funciones distintas, y con MVC podemos separar cada parte en su propio controlador:

- `CursosController`: catálogo, detalle, inscripción y "mis cursos".
- `LeccionesController`: leer y completar lecciones (solo inscritos).
- `AdminController`: todo lo del administrador.
- `CuentaController`: registro, ingreso y salida.

Así, con un solo atributo `[Authorize(Roles = "...")]` arriba del controlador protegemos todas sus acciones. Además, MVC separa claramente los datos (Model), las pantallas (View) y la lógica (Controller), y es el patrón que vimos en clase, por lo que nos resulta más fácil de explicar y de mantener. Razor Pages está pensado para páginas más independientes; aquí muchas pantallas comparten la misma lógica, por eso MVC nos ordena mejor el código.

## Modelo de datos

| Tabla | Para qué sirve |
| --- | --- |
| `Cursos` | Título, descripción, categoría, nivel y si está publicado |
| `Lecciones` | Contenido de cada lección, su duración y su orden dentro del curso |
| `Inscripciones` | Qué usuario está inscrito en qué curso |
| `LeccionesCompletadas` | Qué lecciones completó cada inscripción (para el progreso) |
| `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`... | Tablas de Identity: usuarios y roles |

Reglas importantes:
- Un usuario no puede inscribirse dos veces en el mismo curso (índice único `UsuarioId + CursoId`).
- Una lección no se puede marcar dos veces como completada (clave compuesta `InscripcionId + LeccionId`).
- La duración del curso es la suma de los minutos de sus lecciones.
- El progreso es: lecciones completadas / total de lecciones x 100.

## Seguridad: verificación de la inscripción en el servidor

Ocultar el botón en la vista no es suficiente. Por eso en `LeccionesController`, antes de mostrar una lección, el servidor busca en la base de datos si el usuario actual está inscrito en ese curso y si el curso está publicado (método `BuscarInscripcion`). Si no lo está, devuelve `Forbid()` y el usuario ve "Acceso denegado", **aunque escriba la URL directamente** (por ejemplo `/Lecciones/Leer/1`).

El id del usuario se toma de la sesión (`ClaimTypes.NameIdentifier`), nunca de un dato del formulario. Al registrarse, el rol "Suscriptor" lo asigna el servidor.

## Cómo ejecutar

1. Tener instalado el SDK de .NET 8 y SQL Server (Express sirve).
2. Revisar la cadena de conexión `Portal` en `appsettings.json` (cambiar el nombre del servidor si hace falta, por ejemplo `localhost\\SQLEXPRESS`).
3. Ejecutar:

   ```
   dotnet run --launch-profile http
   ```

4. Abrir http://localhost:5278

La primera vez, la aplicación crea sola la base `UnivalleMicrocursos` con sus tablas (`EnsureCreated`), los roles, el administrador y 6 cursos de ejemplo (`Data/DatosIniciales.cs`). También está el script `Database/UnivalleMicrocursos.sql` con la estructura.

## Cuentas de prueba

| Rol | Correo | Contraseña |
| --- | --- | --- |
| Administrador | admin@univalle.edu | Admin12345 |
| Suscriptor | estudiante@univalle.edu | Estudiante123 |

## Estructura del proyecto

| Carpeta | Contenido |
| --- | --- |
| `Models` | Clases de las tablas y modelos de los formularios |
| `Data` | `PortalContext` (EF Core) y datos iniciales |
| `Controllers` | Lógica de cada parte del sistema |
| `Views` | Pantallas Razor |
| `wwwroot` | CSS y logo |
| `Database` | Script SQL de la base |
