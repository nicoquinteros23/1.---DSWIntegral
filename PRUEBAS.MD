# PRUEBAS.md

Este documento muestra cómo probar cada módulo del proyecto **DSWIntegral** con ejemplos de payloads y respuestas esperadas.

---

## 🚀 Preparación General

1. Asegúrate de tener el entorno en **Development** (opcional):

   ```bash
   export ASPNETCORE_ENVIRONMENT=Development
   ```
2. Ejecuta la API:

   ```bash
   dotnet run
   ```
3. Actualiza la base de datos:

   ```bash
   dotnet ef database update
   ```
4. Swagger UI:

    * [http://localhost:5000/](http://localhost:5000/) (ruta raíz)
    * o [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## 🛠️ M1: CRUD Productos

### GET `/api/Products`

```bash
curl -i http://localhost:5000/api/Products
```

* **200 OK**
* JSON array de productos con `isActive = true`.

### GET `/api/Products/{id}`

```bash
curl -i http://localhost:5000/api/Products/00000000-0000-0000-0000-000000000001
```

* **200 OK** si existe
* **404 Not Found** si no

### POST `/api/Products`

```bash
curl -i -X POST http://localhost:5000/api/Products \
  -H "Content-Type: application/json" \
  -d '{
        "sku": "PRU001",
        "name": "Producto Prueba",
        "description": "Descripción",
        "currentUnitPrice": 9.99,
        "stockQuantity": 100,
        "isActive": true
      }'
```

* **201 Created** + Location header + JSON creado.

#### SKU duplicado

* Repetir con `sku: "PRU001"`
* **400 Bad Request**

  ```json
  { "message": "El SKU 'PRU001' ya está en uso." }
  ```

#### Validación DataAnnotations

* Campos vacíos o inválidos:

```bash
curl -i -X POST http://localhost:5000/api/Products \
  -H "Content-Type: application/json" \
  -d '{
        "sku": "",
        "name": null,
        "currentUnitPrice": -1,
        "stockQuantity": -5
      }'
```

* **400 Bad Request** con objeto de validación (message + details).

### PUT `/api/Products/{id}`

```bash
curl -i -X PUT http://localhost:5000/api/Products/00000000-0000-0000-0000-000000000001 \
  -H "Content-Type: application/json" \
  -d '{
        "id": "00000000-0000-0000-0000-000000000001",
        "sku": "PRU002",
        "name": "Modificado",
        "description": "Nuevo",
        "currentUnitPrice": 15.00,
        "stockQuantity": 50,
        "isActive": true
      }'
```

* **204 No Content**

### PATCH `/api/Products/{id}`

```bash
curl -i -X PATCH http://localhost:5000/api/Products/00000000-0000-0000-0000-000000000001
```

* **204 No Content**
* Luego GET, debe mostrar `isActive: false`.

### DELETE `/api/Products/{id}`

```bash
curl -i -X DELETE http://localhost:5000/api/Products/00000000-0000-0000-0000-000000000001
```

* **204 No Content**
* Luego GET → **404 Not Found**

---

## 🔒 M2: Middleware de Errores, CORS y Validaciones

### 1) Middleware Global de Excepciones

* Inserta en un controlador:

```csharp
throw new Exception("Error de prueba");
```

* Lanza:

```bash
curl -i http://localhost:5000/api/Products
```

* **500 Internal Server Error**

  ```json
  {"message":"Ocurrió un error interno.","details":"Error de prueba"}
  ```

### 2) Validación de ModelState

* Enviar JSON con errores (ver M1. DataAnnotations).
* Respuesta **400 Bad Request** con objeto `{ message, details }`.

### 3) Pruebas CORS

* **Origen permitido**:

  ```bash
  curl -i http://localhost:5000/api/Products -H "Origin: http://localhost:3000"
  ```

    * Header `Access-Control-Allow-Origin: http://localhost:3000`
* **Origen denegado**:

  ```bash
  curl -i http://localhost:5000/api/Products -H "Origin: http://evil.com"
  ```

    * No incluye `Access-Control-Allow-Origin`

---

## 👥 M3: CRUD Clientes

### GET `/api/Customers`

```bash
curl -i http://localhost:5000/api/Customers
```

* **200 OK** + array de clientes.

### GET `/api/Customers/{id}`

```bash
curl -i http://localhost:5000/api/Customers/11111111-1111-1111-1111-111111111111
```

* **200 OK** o **404 Not Found**.

### POST `/api/Customers`

```bash
curl -i -X POST http://localhost:5000/api/Customers \
  -H "Content-Type: application/json" \
  -d '{
        "name":"Juan Pérez",
        "email":"juan@correo.com",
        "address":"Calle Falsa 123"
      }'
```

* **201 Created** + JSON cliente.

---

## 📝 M4/M5: CRUD Órdenes

### GET `/api/Orders`

```bash
curl -i http://localhost:5000/api/Orders
```

* **200 OK** + array de órdenes (con items).

### GET `/api/Orders/{id}`

```bash
curl -i http://localhost:5000/api/Orders/22222222-2222-2222-2222-222222222222
```

* **200 OK** o **404 Not Found**.

### POST `/api/Orders`

```bash
curl -i -X POST http://localhost:5000/api/Orders \
  -H "Content-Type: application/json" \
  -d '{
        "customerId":"11111111-1111-1111-1111-111111111111",
        "items":[{ "productId":"00000000-0000-0000-0000-000000000001", "quantity":2 }]
      }'
```

* **200 OK** + JSON de la orden con `totalAmount` calculado.
* **400 Bad Request** si cliente/prod no existe o stock insuficiente.

### DELETE `/api/Orders/{id}`

```bash
curl -i -X DELETE http://localhost:5000/api/Orders/22222222-2222-2222-2222-222222222222
```

* **204 No Content**

---

## 🔑 M6: Autenticación y Rutas Protegidas

### POST `/api/Auth/register`

```bash
curl -i -X POST http://localhost:5000/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
        "name":"Test User",
        "email":"test@example.com",
        "address":"Addr",
        "password":"P@ssw0rd"
      }'
```

* **200 OK** + `{ token, expires }`.

### POST `/api/Auth/login`

```bash
curl -i -X POST http://localhost:5000/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
        "email":"test@example.com",
        "password":"P@ssw0rd"
      }'
```

* **200 OK** + `{ token, expires }`.

### Probar endpoints protegidos

```bash
curl -i http://localhost:5000/api/Products \
  -H "Authorization: Bearer <TU_TOKEN>"
```

* **200 OK** con token válido.
* **401 Unauthorized** sin token o token inválido.

---

> **Tip**: En Swagger UI pulsa "Authorize" e ingresa `Bearer <token>` para probar sin escribir manualmente el header.

---

```"
}

```
