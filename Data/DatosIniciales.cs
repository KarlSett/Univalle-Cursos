using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proyector_Web_3.Models;

namespace Proyector_Web_3.Data;

public static class DatosIniciales
{
    public static async Task CrearAsync(IServiceProvider services, IConfiguration config, IWebHostEnvironment entorno)
    {
        var db = services.GetRequiredService<PortalContext>();
        await db.Database.EnsureCreatedAsync();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in new[] { "Administrador", "Suscriptor" })
            if (!await roles.RoleExistsAsync(rol))
                Comprobar(await roles.CreateAsync(new IdentityRole(rol)));

        var usuarios = services.GetRequiredService<UserManager<Usuario>>();
        var correo = config["Administrador:Correo"];
        var clave = config["Administrador:Clave"];
        if (!string.IsNullOrWhiteSpace(correo) && !string.IsNullOrWhiteSpace(clave))
        {
            var admin = await usuarios.FindByEmailAsync(correo);
            if (admin == null)
            {
                admin = new Usuario { UserName = correo, Email = correo, Nombre = "Administración", EmailConfirmed = true };
                Comprobar(await usuarios.CreateAsync(admin, clave));
            }
            if (!await usuarios.IsInRoleAsync(admin, "Administrador"))
                Comprobar(await usuarios.AddToRoleAsync(admin, "Administrador"));
        }

        // La cuenta de práctica solo se crea en desarrollo.
        if (entorno.IsDevelopment() && await usuarios.FindByEmailAsync("estudiante@univalle.edu") == null)
        {
            var estudiante = new Usuario { UserName = "estudiante@univalle.edu", Email = "estudiante@univalle.edu", Nombre = "Estudiante", EmailConfirmed = true };
            Comprobar(await usuarios.CreateAsync(estudiante, "Estudiante123"));
            Comprobar(await usuarios.AddToRoleAsync(estudiante, "Suscriptor"));
        }
        if (!await db.Cursos.AnyAsync())
        {
            db.Cursos.AddRange(CrearCursos());
            await db.SaveChangesAsync();
        }
    }

    private static void Comprobar(IdentityResult resultado)
    {
        if (!resultado.Succeeded)
            throw new InvalidOperationException(string.Join("; ", resultado.Errors.Select(e => e.Description)));
    }

    private static List<Curso> CrearCursos()
    {
        return [
            Nuevo("Tu primera página web", "Construye una página desde cero. Organiza el contenido con HTML, agrega enlaces y prepara un formulario accesible.", "Desarrollo", "Inicial", "web",
                ("La estructura de una página", 15, "HTML describe el significado del contenido. Un documento empieza con <!DOCTYPE html> y contiene las etiquetas html, head y body. En head se escribe el título de la pestaña y en body el contenido visible.\n\nCrea un archivo llamado index.html. Agrega un encabezado h1 con tu nombre y un párrafo p con una presentación. Ábrelo en el navegador y observa el resultado.\n\nUsa un solo h1 para el título principal y h2 para las secciones. Las etiquetas header, main y footer ayudan a organizar la página y a los lectores de pantalla.\n\nPráctica: organiza una presentación personal con un encabezado, dos secciones y un pie de página."),
                ("Enlaces y formularios", 20, "La etiqueta a crea enlaces mediante href. Una imagen usa img y necesita un texto alternativo útil en alt. Las listas sin orden usan ul y las secuencias usan ol.\n\nLa etiqueta form agrupa campos. Cada input necesita un label cuyo atributo for coincida con el id del campo. Usa type=\"email\" para un correo y required para solicitar un valor.\n\nLos datos que modifican información deben enviarse con POST. La validación del navegador ayuda, pero el servidor también debe comprobar todos los datos.\n\nPráctica: agrega a tu página un enlace y un formulario con nombre y correo. Recorre los campos con Tab y comprueba que sus etiquetas sean claras.")),
            Nuevo("SQL para empezar", "Consulta y relaciona información en SQL Server. Practica SELECT, filtros y combinaciones de tablas con ejemplos pequeños.", "Datos", "Inicial", "sql",
                ("Consultar, filtrar y ordenar", 20, "Una tabla organiza registros en filas y características en columnas. La clave primaria identifica una fila y la clave foránea conecta tablas.\n\nSELECT Titulo, Nivel FROM Cursos; devuelve dos columnas. WHERE Publicado = 1 permite seleccionar solo los cursos publicados. ORDER BY Titulo ASC ordena alfabéticamente.\n\nCombina condiciones con AND y OR. Usa paréntesis para que la intención sea clara. Es preferible solicitar las columnas que necesitas en vez de utilizar siempre SELECT *.\n\nPráctica: escribe una consulta para obtener los cursos de nivel Inicial ordenados por título."),
                ("Relacionar datos con JOIN", 25, "Inscripciones.CursoId apunta a Cursos.Id. Así se puede relacionar una inscripción con su curso sin repetir el título en cada fila.\n\nSELECT c.Titulo, i.FechaInscripcion FROM Cursos c INNER JOIN Inscripciones i ON c.Id = i.CursoId; reúne los registros relacionados. INNER JOIN devuelve coincidencias. LEFT JOIN conserva todas las filas de la tabla izquierda.\n\nCOUNT cuenta registros y GROUP BY los agrupa. En una aplicación utiliza parámetros: concatenar datos del usuario dentro de SQL permite inyecciones. Las consultas LINQ habituales de EF Core son parametrizadas.\n\nPráctica: plantea una consulta que incluya todos los cursos, incluso los que no tienen inscripciones. Explica qué JOIN usarías.")),
            Nuevo("Git en equipo", "Lleva un historial claro de tu proyecto. Crea commits, trabaja con ramas y resuelve conflictos junto a tu compañero.", "Herramientas", "Inicial", "git",
                ("Un historial que se entiende", 15, "Git registra versiones de un proyecto. Git es una herramienta local; GitHub es uno de los servicios donde puedes alojar repositorios.\n\nConfigura tu nombre y correo antes de empezar. git status muestra archivos modificados y git diff permite revisar sus cambios. Agrega archivos específicos con git add y guarda una tarea concreta con git commit.\n\nUn mensaje como 'Valida la inscripción al abrir una lección' explica mejor la intención que 'Cambios'. El archivo .gitignore excluye carpetas generadas como bin y obj, y los secretos nunca deben subirse.\n\nPráctica: identifica qué archivos son código fuente y cuáles se generan al compilar."),
                ("Ramas y conflictos", 20, "Una rama permite desarrollar una funcionalidad antes de integrarla en la rama principal. Cada integrante debe configurar su identidad real y registrar sus propias contribuciones.\n\nUn conflicto aparece cuando Git no puede combinar automáticamente dos cambios. Abre el archivo, acuerda el resultado con tu compañero y elimina los marcadores de conflicto.\n\nCompila y prueba después de resolverlo. Revisar juntos un cambio ayuda a detectar errores y permite que ambos entiendan el proyecto.\n\nPráctica: propón cómo dividirían el catálogo y el área de suscriptor en dos tareas, e indica qué revisarían juntos antes de integrarlas.")),
            Nuevo("Layouts con CSS", "Diseña una interfaz que se adapte a cualquier pantalla. Aprende a organizar contenido con Flexbox, Grid y media queries.", "Diseño web", "Intermedio", "css",
                ("Cajas, Flexbox y Grid", 20, "Todo elemento tiene contenido, padding, border y margin. box-sizing: border-box hace que el ancho incluya el relleno y el borde.\n\nFlexbox distribuye elementos en una fila o una columna. display: flex, gap y align-items son útiles para una barra de navegación. flex-wrap permite pasar a otra línea.\n\nGrid organiza filas y columnas. grid-template-columns: repeat(3, minmax(0, 1fr)) crea tres columnas iguales y evita que textos largos fuercen su ancho.\n\nPráctica: crea un catálogo de tres columnas con Grid y una barra de acciones con Flexbox."),
                ("Una página para cada pantalla", 20, "Una media query como @media (max-width: 640px) adapta la disposición al espacio disponible. Un catálogo de tres columnas puede pasar a una sola columna en pantallas pequeñas.\n\nEvita alturas fijas en bloques con texto variable. Usa rem para tamaños relacionados con la fuente y max-width para limitar el ancho de lectura.\n\nComprueba que no exista desplazamiento horizontal. Prueba textos largos, zoom y navegación por teclado. Si agregas movimiento, respeta prefers-reduced-motion.\n\nPráctica: revisa tu página a 390, 768 y 1440 píxeles. Anota qué cambia de posición y por qué.")),
            Nuevo("C# paso a paso", "Entiende variables, condiciones y clases con ejemplos concretos. Una base práctica para comenzar con ASP.NET Core MVC.", "Desarrollo", "Inicial", "csharp",
                ("Variables, condiciones y clases", 20, "Una variable guarda un valor con un tipo. int se usa para enteros, string para texto y bool para verdadero o falso. Elige nombres que describan qué representa el dato.\n\nUna condición if ejecuta un bloque cuando su expresión es verdadera. foreach recorre una colección sin repetir manualmente las instrucciones para cada elemento.\n\nUna clase agrupa propiedades y comportamiento. Curso puede tener Titulo, Nivel y Publicado. Un objeto es una instancia concreta de la clase.\n\nPráctica: dibuja las clases Curso y Leccion. Explica por qué una lección necesita identificar a su curso."),
                ("El recorrido de MVC", 25, "MVC separa tres responsabilidades: el modelo representa los datos, la vista los presenta y el controlador recibe una petición y decide la respuesta.\n\nAl visitar /Cursos/Detalle/1, el enrutamiento busca el controlador Cursos y la acción Detalle. El controlador consulta la base de datos y entrega un modelo a una vista Razor.\n\nLa vista no decide si alguien puede leer datos protegidos. El controlador comprueba autenticación e inscripción antes de consultar y enviar el contenido de la lección.\n\nPráctica: explica el recorrido de una petición para abrir una lección y señala dónde debe verificarse la inscripción.")),
            Nuevo("Excel para tu día a día", "Organiza tablas, aplica fórmulas sencillas y filtra información. Herramientas prácticas para estudiar y trabajar con datos.", "Herramientas", "Inicial", "excel",
                ("Tablas y fórmulas útiles", 15, "Una tabla necesita encabezados claros y una fila por registro. Evita combinar celdas dentro del conjunto de datos y usa tipos consistentes: fechas como fechas e importes como números.\n\nUna fórmula comienza con igual. SUMA suma un rango y PROMEDIO calcula su media. Los nombres de funciones dependen del idioma de Excel.\n\nUna referencia relativa como A2 cambia al copiar una fórmula. Una referencia absoluta como $A$2 conserva fila y columna. Revisa siempre qué rango incluye un total.\n\nPráctica: organiza una lista de cursos y calcula la suma de sus minutos. Comprueba el resultado con una suma manual."),
                ("Filtrar y ordenar", 15, "Un filtro oculta temporalmente filas: no las elimina. Ordenar cambia su posición, por eso debes ordenar la tabla completa en vez de una columna aislada.\n\nPuedes filtrar por texto, número o fecha. Al terminar, quita los filtros para no confundir una selección parcial con todos los datos.\n\nUsa nombres de hojas claros y escribe las unidades en los encabezados. 'Duración (min)' evita que alguien interprete los valores como horas.\n\nPráctica: filtra los cursos de nivel Inicial y ordénalos por duración, de menor a mayor."))
        ];
    }

    private static Curso Nuevo(string titulo, string descripcion, string categoria, string nivel, string portada,
        params (string Titulo, int Minutos, string Contenido)[] lecciones)
    {
        var curso = new Curso { Titulo = titulo, Descripcion = descripcion, Categoria = categoria, Nivel = nivel, Portada = portada, Publicado = true };
        for (int i = 0; i < lecciones.Length; i++)
            curso.Lecciones.Add(new Leccion { Titulo = lecciones[i].Titulo, DuracionMinutos = lecciones[i].Minutos, Contenido = lecciones[i].Contenido, Orden = i + 1 });
        return curso;
    }
}
