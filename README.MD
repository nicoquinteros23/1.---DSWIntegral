DSWIntegral API (Backend)

Este repositorio contiene la implementación de la API RESTful DSWIntegral, desarrollada en ASP.NET Core.

⸻

Integrantes

• 53049; Nicolas Quinteros; nicoquinteros.nq@gmail.com

• 58000; Josue Limo; Josue.LimoMunoz@alu.frt.utn.edu.ar

• 58122 ; Ignacio Hillen ; Ignacio.Hillen@alu.frt.utn.edu.ar

⸻

1. Prerequisitos

Antes de comenzar, asegúrate de tener instalados:
•	.NET 9.0 SDK o superior
•	SQL Server (puede ser local, Docker o servidor remoto)
•	Herramienta de línea de comandos: dotnet, sqlcmd (opcional para verificar BD)
•	IDE o editor (Visual Studio, Rider, VS Code, etc.)

⸻

2. Configuración y ejecución local
    1.	Clonar el repositorio

git clone https://github.com/tu-organizacion/DSWIntegral.git
cd DSWIntegral


	2.	Configurar cadena de conexión
	•	Abre appsettings.json y edita la sección ConnectionStrings:DefaultConnection:

"DefaultConnection": "Server=localhost,1433;Database=DSW2025;User Id=sa;Password=TuP4ssw0rd!;TrustServerCertificate=True;"


	•	Asegúrate de que tu instancia de SQL Server esté corriendo y accesible.

	3.	Restaurar paquetes y migraciones

dotnet restore
dotnet ef database update

Esto crea la base de datos y las tablas según las migraciones en Migrations/.

	4.	Ejecutar la API

dotnet run --project DSWIntegral

Por defecto escucha en http://localhost:5000.

	5.	Explorar documentación Swagger
Abre en tu navegador:
•	http://localhost:5000/ si RoutePrefix = "", o
•	http://localhost:5000/swagger

⸻

3. Endpoints implementados

A continuación un resumen de los principales recursos y cómo probarlos.

3.1 Autenticación (JWT)

Ruta	Método	Descripción	Payload de ejemplo
/api/Auth/register	POST	Registrar un nuevo cliente (rol Customer)	{ "name":"Juan", "email":"juan@ejemplo.com", "address":"Calle 1", "password":"Secret123" }
/api/Auth/login	POST	Iniciar sesión y obtener token JWT	{ "email":"juan@ejemplo.com", "password":"Secret123" }

	•	Respuesta: { "token": "<JWT>", "expires": "2025-07-24T..." }

Usa el JWT en el header Authorization: Bearer <token> para rutas protegidas.

3.2 Productos

Ruta	Método	Roles	Descripción
/api/Products	GET	Público	Listar productos activos
/api/Products/{id}	GET	Público	Obtener producto por ID
/api/Products	POST	AdminOnly	Crear nuevo producto
/api/Products/{id}	PUT	AdminOnly	Actualizar todos los campos
/api/Products/{id}	PATCH	AdminOnly	Inactivar (IsActive=false)
/api/Products/{id}	DELETE	AdminOnly	Eliminar físicamente

Payload POST/PUT ejemplo:

{
"sku":"SKU-1000",
"name":"Producto X",
"description":"Desc opcional",
"currentUnitPrice":19.99,
"stockQuantity":50,
"isActive":true
}



3.3 Clientes

Ruta	Método	Roles	Descripción
/api/Customers	GET	Público	Listar todos los clientes
/api/Customers/{id}	GET	Público	Obtener cliente por ID
/api/Customers	POST	Público	Crear nuevo cliente
/api/Customers/{id}	PUT	Público	Actualizar datos de cliente
/api/Customers/{id}	DELETE	Público/Admin	Eliminar cliente

Payload POST/PUT ejemplo:

{
"name":"María",
"email":"maria@ejemplo.com",
"address":"Av. Siempre Viva 123"
}



3.4 Órdenes

Ruta	Método	Roles	Descripción
/api/Orders	GET	CustomerOnly	Listar órdenes del cliente autenticado
/api/Orders/{id}	GET	Cliente/Admin	Obtener orden por ID (dueño o admin)
/api/Orders	POST	CustomerOnly	Crear nueva orden (se actualiza stock automáticamente)
/api/Orders/{id}	DELETE	AdminOnly	Eliminar orden (opcionalmente restaura stock)
/api/Orders/{id}/status	PUT	AdminOnly	Actualizar estado de la orden (Pending, Processing, Completed, Cancelled)

Payload POST ejemplo:

{
"customerId":"<GUID_cliente>",
"shippingAddress":"Calle A 123",
"billingAddress":"Calle B 456",
"items":[
{ "productId":"<GUID_producto>", "quantity":2 }
]
}



⸻

4. Notas finales
   •	CORS: configurado para permitir http://localhost:3000.
   •	Middleware de errores: respuestas uniformes con { message, details }.
   •	Rate limiting, CSRF, XSS: implementados según Módulo 7.

¡Listo! Con esto tienes todo lo necesario para ejecutar y probar la parte backend de DSWIntegral hasta el módulo 7. Si un requisito no aparece aquí, revisa las secciones anteriores y los endpoints cargados.