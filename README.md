# Univalle-Cursos

Portal de microcursos hecho con ASP.NET Core MVC (.NET 8), Razor, ASP.NET Core Identity, EF Core y SQL Server. El diseño usa blanco, burdeos, ilustraciones de los temas y una navegación adaptable a teléfonos. El estilo y el menú están en archivos locales CSS y JavaScript.

## Ejecutar

Necesitas el SDK de .NET 8 o superior y SQL Server con autenticación de Windows.

1. Abre la solución en Visual Studio o una terminal en esta carpeta.
2. Revisa `ConnectionStrings:Portal` en `appsettings.json`. La conexión configurada es:

   ```text
   Server=localhost;Database=UnivalleMicrocursos;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False
   ```

   Si usas una instancia Express, cambia `localhost` por `localhost\SQLEXPRESS`. En JSON, la barra se escribe doble: `localhost\\SQLEXPRESS`.

3. Restaura los paquetes y ejecuta:

   ```powershell
   dotnet restore --configfile NuGet.Config
   dotnet run --project "Proyector Web 3.csproj" --launch-profile http
   ```

4. Abre **http://localhost:5278**.

La primera ejecución crea la base y las tablas si la base es nueva. El usuario de Windows necesita permiso para crear esa base. Después crea los roles, las cuentas de desarrollo y seis cursos de ejemplo con dos lecciones cada uno.

También puedes crear primero la estructura ejecutando **Database/UnivalleMicrocursos.sql** en SQL Server Management Studio. Usa una base nueva y ejecuta el script una sola vez. Luego inicia la aplicación para cargar las cuentas y el contenido de ejemplo. No insertes contraseñas directamente en SQL: Identity calcula sus hashes.

Para volver a obtener un script desde los modelos:

```powershell
dotnet run --project "Proyector Web 3.csproj" --no-launch-profile -- --exportar-sql
```

Se usa `EnsureCreated` para mantener sencillo el arranque de este proyecto. No modifica tablas existentes ni utiliza migraciones. Si se amplía el modelo después de crear la base, habrá que preparar el cambio de esquema correspondiente.

## Cuentas para desarrollo

| Rol | Correo | Contraseña |
| --- | --- | --- |
| Administrador | admin@univalle.edu | Admin12345 |
| Suscriptor | estudiante@univalle.edu | Estudiante123 |

La configuración del administrador está en `appsettings.Development.json`. La cuenta de estudiante solo se crea en Development. Fuera de desarrollo no hay credenciales predeterminadas: el administrador se configura mediante `Administrador__Correo` y `Administrador__Clave` en el entorno.

## Qué puede hacer cada persona

- **Invitado:** explorar, buscar y filtrar cursos publicados; ver descripción, nivel, duración y temario.
- **Suscriptor:** registrarse, ingresar, inscribirse gratis, consultar sus cursos, leer sus lecciones y guardar progreso.
- **Administrador:** crear, editar, eliminar, publicar y retirar cursos; agregar, editar, eliminar y ordenar lecciones; consultar inscritos y progreso.

Si un curso se retira, sus lecciones dejan de estar disponibles. Se conservan las inscripciones y el progreso. Eliminar un curso sí borra sus lecciones, inscripciones y progreso. Si se elimina su última lección, el curso vuelve a borrador.

El contenido de las lecciones es texto. Se conservan párrafos y saltos de línea, y Razor escapa las etiquetas HTML para poder explicar código sin ejecutarlo. No se alojan videos, pagos, certificados ni evaluaciones.

## Cómo está organizado

| Carpeta | Responsabilidad |
| --- | --- |
| Models | Entidades y modelos de los formularios y vistas |
| Data | Contexto EF Core y datos iniciales |
| Controllers | Peticiones, consultas, validación y permisos |
| Views | Pantallas Razor |
| wwwroot | CSS, JavaScript e ilustraciones SVG locales |
| Database | Script SQL de la estructura |

No se agregaron repositorios genéricos ni capas de servicios. Los controladores usan directamente el contexto de EF Core para que el recorrido de cada operación sea fácil de seguir.

## Puntos para explicar en la defensa

1. **MVC:** la ruta llama a una acción del controlador, el controlador consulta el modelo y entrega datos a la vista Razor.
2. **Identity:** guarda usuarios y roles en `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` y las demás tablas de Identity. La aplicación no compara contraseñas por su cuenta.
3. **Autorización:** `[Authorize]` exige sesión y rol. Esto no basta para una lección: `LeccionesController` también consulta la inscripción del usuario actual y verifica que el curso esté publicado, antes de obtener el contenido.
4. **Identidad del usuario:** el identificador sale de la sesión (`ClaimTypes.NameIdentifier`), no de un campo que el visitante pueda cambiar.
5. **Progreso:** lecciones completadas divididas entre total de lecciones, por 100. La duración del curso suma los minutos de sus lecciones.
6. **Integridad:** un índice único evita dos inscripciones de la misma persona en un curso. La clave compuesta evita completar dos veces la misma lección.
7. **Formularios:** las operaciones que modifican datos usan POST y protección antifalsificación. El registro siempre asigna Suscriptor desde el servidor.
8. **Borrado:** las transacciones agrupan los cambios relacionados. Se borran primero las marcas de completado para respetar las claves foráneas de SQL Server.

## Comprobación manual

Prueba estas acciones con el servidor en ejecución:

| Caso | Resultado esperado |
| --- | --- |
| Abrir el catálogo sin sesión | Solo cursos publicados |
| Buscar algo que no existe | Mensaje vacío y opción de quitar filtros |
| Abrir `/Lecciones/Leer/1` sin sesión | Redirección a iniciar sesión |
| Abrir esa URL con un suscriptor sin inscripción | Acceso restringido, sin contenido |
| Enviar POST a completar sin inscripción | Acceso restringido |
| Inscribirse y abrir la lección | Contenido disponible |
| Marcar completada dos veces | Una sola marca en la base |
| Entrar a `/Admin` como suscriptor | Acceso restringido |
| Publicar un curso sin lecciones | Mensaje que pide agregar una lección |
| Retirar un curso inscrito | Sin acceso al contenido; progreso conservado |
| Crear, ordenar y borrar lecciones | El orden se guarda y la duración se recalcula |
| Enviar un formulario POST sin token | HTTP 400 |

Revisa además el portal a 390, 768 y 1440 píxeles de ancho y navega por los formularios con el teclado.

Referencias: [Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity?view=aspnetcore-8.0), [autorización por roles](https://learn.microsoft.com/aspnet/core/security/authorization/roles?view=aspnetcore-8.0) y [creación de bases con EF Core](https://learn.microsoft.com/ef/core/managing-schemas/ensure-created).
