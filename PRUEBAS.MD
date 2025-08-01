PRUEBAS.md

Este documento describe cómo probar todos los endpoints de la API DSWIntegral, incluyendo autenticación, roles y restricciones.

⸻

🚀 1. Preparación
1.	Clonar y situarse en la carpeta del proyecto.
2.	(Opcional) Establecer entorno en modo Desarrollo:

export ASPNETCORE_ENVIRONMENT=Development


	3.	Restaurar y compilar:

dotnet restore
dotnet build


	4.	Aplicar migraciones:

dotnet ef database update


	5.	Ejecutar la API:

dotnet run


	6.	Swagger UI disponible en:
	•	http://localhost:5000/ (si RoutePrefix = "")
	•	http://localhost:5000/swagger

Tip: En Swagger pulsa el botón Authorize para ingresar el token y probar rutas protegidas.

⸻

🔑 2. Autenticación y Registro (Módulo M6)

2.1 Registro de Usuario
•	Endpoint: POST /api/Auth/register
•	Rol: Anónimo
•	Payload de ejemplo:

{
"name": "Test Customer",
"email": "customer@example.com",
"address": "Calle Falsa 123",
"password": "P@ssw0rd!"
}


	•	Respuesta esperada:
	•	200 OK
	•	Body:

{
"token": "<JWT_TOKEN>",
"expires": "2025-07-23T...Z"
}



2.2 Login de Usuario
•	Endpoint: POST /api/Auth/login
•	Rol: Anónimo
•	Payload de ejemplo:

{
"email": "customer@example.com",
"password": "P@ssw0rd!"
}


	•	Respuesta esperada:
	•	200 OK
	•	Body igual a register (token + expires).

Con este token podrás acceder a rutas protegidas. Guarda <JWT_TOKEN>.

⸻

👨‍💼 3. Roles y Restricciones
•	Admin: puede gestionar Productos y actualizar Estado de Órdenes.
•	Customer: puede crear y consultar sus Órdenes.
•	Invitado: solo puede consultar Productos y Clientes.

Cabecera común para rutas protegidas:

-H "Authorization: Bearer <JWT_TOKEN>"


⸻

🛠️ 4. M1: CRUD Productos

4.1 GET /api/Products (Público)

curl -i http://localhost:5000/api/Products

	•	200 OK: lista de productos activos.

4.2 GET /api/Products/{id} (Público)

curl -i http://localhost:5000/api/Products/<PRODUCT_ID>

	•	200 OK o 404 Not Found.

4.3 POST /api/Products (AdminOnly)

curl -i -X POST http://localhost:5000/api/Products \
-H "Content-Type: application/json" \
-H "Authorization: Bearer <ADMIN_TOKEN>" \
-d '{
"sku": "NEW1001",
"name": "Producto Nuevo",
"description": "Descripción opcional",
"currentUnitPrice": 19.99,
"stockQuantity": 50,
"isActive": true
}'

	•	201 Created + Location header.
	•	403 Forbidden si no es Admin.
	•	400 Bad Request en caso de SKU duplicado o validación.

4.4 PUT /api/Products/{id} (AdminOnly)

curl -i -X PUT http://localhost:5000/api/Products/<PRODUCT_ID> \
-H "Content-Type: application/json" \
-H "Authorization: Bearer <ADMIN_TOKEN>" \
-d '{
"id": "<PRODUCT_ID>",
"sku": "UPDATED1001",
"name": "Prod Modificado",
"description": "Nueva descripción",
"currentUnitPrice": 29.99,
"stockQuantity": 75,
"isActive": true
}'

	•	204 No Content | 400/403/404 según el caso.

4.5 PATCH /api/Products/{id} (AdminOnly)

curl -i -X PATCH http://localhost:5000/api/Products/<PRODUCT_ID> \
-H "Authorization: Bearer <ADMIN_TOKEN>"

	•	204 No Content|403/404.

4.6 DELETE /api/Products/{id} (AdminOnly)

curl -i -X DELETE http://localhost:5000/api/Products/<PRODUCT_ID> \
-H "Authorization: Bearer <ADMIN_TOKEN>"

	•	204 No Content|403/404.

⸻

👥 5. M3: CRUD Clientes

Nota: Estos endpoints son de uso público (sin autenticación).

5.1 GET /api/Customers

curl -i http://localhost:5000/api/Customers

	•	200 OK: lista de todos los clientes.

5.2 GET /api/Customers/{id}

curl -i http://localhost:5000/api/Customers/<CUSTOMER_ID>

	•	200 OK o 404 Not Found.

5.3 POST /api/Customers

curl -i -X POST http://localhost:5000/api/Customers \
-H "Content-Type: application/json" \
-d '{
"name":"Ana López",
"email":"ana@example.com",
"address":"Av. Siempre Viva 742"
}'

	•	201 Created|400 Bad Request.

⸻

📝 6. M4/M5: CRUD Órdenes

Importante: Solo usuarios autenticados (Customer o Admin).

6.1 GET /api/Orders (CustomerOnly)

Lista órdenes del cliente autenticado.

curl -i http://localhost:5000/api/Orders \
-H "Authorization: Bearer <CUSTOMER_TOKEN>"

	•	200 OK + array de OrderResponseDto.
	•	401/403 si no autorizado.

6.2 GET /api/Orders/{id} (CustomerOnly or AdminOnly)

curl -i http://localhost:5000/api/Orders/<ORDER_ID> \
-H "Authorization: Bearer <TOKEN>"

	•	200 OK si el order.CustomerId coincide con el JWT o si es Admin.
	•	404 Not Found si no existe.
	•	403 Forbidden si cliente intenta ver orden ajena.

6.3 POST /api/Orders (CustomerOnly)

curl -i -X POST http://localhost:5000/api/Orders \
-H "Content-Type: application/json" \
-H "Authorization: Bearer <CUSTOMER_TOKEN>" \
-d '{
"shippingAddress":"Calle 123",
"billingAddress":"Av. 456",
"items":[
{"productId":"<PRODUCT_ID>","quantity":2}
]
}'

	•	201 Created + Body OrderResponseDto.
	•	400 Bad Request por validaciones.

6.4 DELETE /api/Orders/{id} (AdminOnly)

curl -i -X DELETE http://localhost:5000/api/Orders/<ORDER_ID> \
-H "Authorization: Bearer <ADMIN_TOKEN>"

	•	204 No Content|404/403.

6.5 PUT /api/Orders/{id}/status (AdminOnly)

curl -i -X PUT http://localhost:5000/api/Orders/<ORDER_ID>/status \
-H "Content-Type: application/json" \
-H "Authorization: Bearer <ADMIN_TOKEN>" \
-d '{"newStatus":"Processing"}'

	•	204 No Content.
	•	400 Bad Request si estado no válido.
	•	404 si no existe.

⸻

🔒 7. M2 Avanzado: CORS, Rate Limiting y CSRF/XSS
•	Rate Limiting: 20 requests/minuto. Ver headers x-rate-limit-*.
•	CSRF/XSS: Asegurado por políticas (Token, SameSite, etc).

⸻

Con esta guía puedes probar exhaustivamente la API según roles y módulos. ¡Éxitos!