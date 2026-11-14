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

Elegimos **ASP.NET Core MVC** porque el portal maneja flujos compartidos entre tres perfiles distintos (invitado, suscriptor y administrador) y el patrón Modelo–Vista–Controlador nos permite organizar cada módulo por responsabilidad:

- `CursosController`: catálogo público con búsqueda y filtro por categoría, detalle, inscripción y "Mis cursos".
- `LeccionesController`: lectura de lecciones y registro de progreso (validando inscripción activa en el servidor).
- `AdminController`: gestión completa de cursos, lecciones e inscritos.
- `CuentaController`: registro, inicio y cierre de sesión con Identity.

Al agrupar las acciones relacionadas en cada controlador, podemos proteger áreas enteras con un único atributo `[Authorize(Roles = "...")]`, reutilizar modelos de vista y mantener la lógica de negocio separada de las pantallas Razor. En cambio, Razor Pages funciona mejor para pantallas aisladas con lógica propia por archivo; en este proyecto nos resultaba más claro y mantenible centralizar las rutas y permisos por controlador, además de ser la arquitectura trabajada en clase.

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

## Cómo probar

1. **Como invitado (sin iniciar sesión):**
   - Entra a `/` para ver los cursos publicados, usar el buscador por texto y filtrar por categoría.
   - Abre el detalle de un curso para revisar su descripción y temario.
   - Si intentas abrir una lección directamente (por ejemplo `/Lecciones/Leer/1`), el sistema redirige a `/Cuenta/Ingresar`.
2. **Como suscriptor (`estudiante@univalle.edu` / `Estudiante123` o creando una cuenta nueva):**
   - Inicia sesión en **Ingresar**, entra a un curso y haz clic en **Inscribirme gratis**.
   - Abre una lección, léela y presiona **Marcar como completada**.
   - Entra a **Mis cursos** en el menú superior para verificar que el porcentaje de avance sube correctamente.
   - Si ingresas manualmente la URL `/Lecciones/Leer/{id}` de un curso en el que no estás inscrito, el servidor responde `Acceso denegado`.
3. **Como administrador (`admin@univalle.edu` / `Admin12345`):**
   - Entra a **Administración** en el menú superior.
   - Crea un curso nuevo (queda en estado **Borrador**), agrégale lecciones, ordénalas con las flechas `↑` / `↓` y publícalo desde la tabla.
   - Consulta **Inscritos** en cualquier curso para ver la lista de estudiantes y cuántas lecciones completaron.

## Estructura del proyecto

| Carpeta | Contenido |
| --- | --- |
| `Models` | Clases de las tablas y modelos de los formularios |
| `Data` | `PortalContext` (EF Core) y datos iniciales |
| `Controllers` | Lógica de cada parte del sistema |
| `Views` | Pantallas Razor |
| `wwwroot` | CSS y logo |
| `Database` | Script SQL de la base |
