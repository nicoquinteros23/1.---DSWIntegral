# PRUEBAS.md

Este documento recoge los pasos y ejemplos para probar cada funcionalidad implementada en la API **DSWIntegral**.

---

## 🚀 1. Preparación

1. Configura el entorno en **Desarrollo**:

   ```bash
   export ASPNETCORE_ENVIRONMENT=Development
   ```
2. Arranca la API:

   ```bash
   dotnet run
   ```
3. Asegúrate de que la base de datos está migrada y lista:

   ```bash
   dotnet ef database update
   ```
4. Swagger UI disponible en:

    * `http://localhost:5000/` (si `RoutePrefix = ""`) o
    * `http://localhost:5000/swagger`

---

## 📦 2. Endpoints de Productos

### 2.1 GET `/api/Products`

* **Descripción**: Lista todos los productos con `IsActive = true`.
* **Comando CURL**:

  ```bash
  curl -i http://localhost:5000/api/Products
  ```
* **Respuesta esperada**:

    * `200 OK`
    * JSON array de productos activos:

      ```json
      [
        {
          "id": "...",
          "sku": "ABC123",
          "name": "Producto A",
          "description": "...",
          "currentUnitPrice": 9.99,
          "stockQuantity": 100,
          "isActive": true
        },
        // ... más elementos
      ]
      ```

### 2.2 GET `/api/Products/{id}`

* **Descripción**: Obtiene un producto por su `id`.
* **CURL**:

  ```bash
  curl -i http://localhost:5000/api/Products/{id}
  ```
* **Escenarios**:

    * **Existente**: `200 OK` + objeto JSON.
    * **No encontrado**: `404 Not Found`.

### 2.3 POST `/api/Products`

* **Descripción**: Crea un nuevo producto.

#### 2.3.1 Payload válido

```bash
curl -i -X POST http://localhost:5000/api/Products \
  -H "Content-Type: application/json" \
  -d '{
      "sku": "PRU001",
      "name": "Producto Prueba",
      "description": "Desc",
      "currentUnitPrice": 12.50,
      "stockQuantity": 20,
      "isActive": true
    }'
```

* **Respuesta**:

    * `201 Created`
    * Cabecera `Location: /api/Products/{newId}`
    * Body: JSON del producto con `id` asignado.

#### 2.3.2 SKU duplicado

```bash
curl -i -X POST http://localhost:5000/api/Products \
  -H "Content-Type: application/json" \
  -d '{
      "sku": "PRU001",
      ...
    }'
```

* **Respuesta**:

    * `400 Bad Request`
    * Body:

      ```json
      { "message": "El SKU 'PRU001' ya está en uso." }
      ```

#### 2.3.3 Payload inválido (DataAnnotations)

```bash
curl -i -X POST http://localhost:5000/api/Products \
  -H "Content-Type: application/json" \
  -d '{
      "sku": "",
      "name": null,
      "currentUnitPrice": -5,
      "stockQuantity": -1
    }'
```

* **Respuesta**:

    * `400 Bad Request`
    * Body:

      ```json
      {
        "title": "One or more validation errors occurred.",
        "errors": {
          "Sku": ["El campo SKU es obligatorio."],
          "Name": ["El campo Name es obligatorio."],
          "CurrentUnitPrice": ["El campo CurrentUnitPrice debe estar entre 0.01 y ..."],
          "StockQuantity": ["El campo StockQuantity debe estar entre 0 y ..."]
        }
      }
      ```

### 2.4 PUT `/api/Products/{id}`

* **Descripción**: Actualiza todos los campos de un producto existente.

#### 2.4.1 Actualización exitosa

```bash
curl -i -X PUT http://localhost:5000/api/Products/{id} \
  -H "Content-Type: application/json" \
  -d '{
      "id": "{id}",
      "sku": "PRU002",
      "name": "Prod Modificado",
      "description": "Desc nueva",
      "currentUnitPrice": 15.00,
      "stockQuantity": 25,
      "isActive": true
    }'
```

* **Respuesta**:

    * `204 No Content`

#### 2.4.2 ID de URL vs Body distinto

* **Mismo comando** con `"id"` diferente que `{id}` de la ruta.
* **Respuesta**: `400 Bad Request` + mensaje de mismatch.

#### 2.4.3 SKU en uso por otro producto

* Intentar cambiar `sku` a uno que ya exista en otro registro.
* **Respuesta**: `400 Bad Request` + `{ message: "El SKU '...' ya está en uso..." }`.

### 2.5 PATCH `/api/Products/{id}`

* **Descripción**: Inhabilita (`IsActive = false`) el producto.

```bash
curl -i -X PATCH http://localhost:5000/api/Products/{id}
```

* **Respuesta**: `204 No Content`

* **Verificación**:

    * `GET /api/Products/{id}` → `200 OK` y `"isActive": false`.
    * `GET /api/Products` → el producto ya **no** aparece.

### 2.6 DELETE `/api/Products/{id}`

* **Descripción**: Elimina físicamente el registro.

```bash
curl -i -X DELETE http://localhost:5000/api/Products/{id}
```

* **Respuesta**: `204 No Content`

* **Verificación**:

    * `GET /api/Products/{id}` → `404 Not Found`.

---

## 🔒 3. Seguridad y errores (M2)

### 3.1 Middleware global de excepciones

* En un endpoint (por ej. `GET /api/Products`) inserta:

  ```csharp
  throw new Exception("Error de prueba");
  ```
* Llama vía CURL/SWAGGER:

  ```bash
  curl -i http://localhost:5000/api/Products
  ```
* **Respuesta**:

    * `500 Internal Server Error`
    * Body:

      ```json
      {
        "message": "Ocurrió un error interno.",
        "details": "Error de prueba"
      }
      ```

### 3.2 Pruebas de CORS

* **Origen permitido**:

  ```bash
  curl -i http://localhost:5000/api/Products \
    -H "Origin: http://localhost:3000"
  ```

    * Respuesta contiene header:

      ```
      Access-Control-Allow-Origin: http://localhost:3000
      ```
* **Origen denegado**:

  ```bash
  curl -i http://localhost:5000/api/Products \
    -H "Origin: http://evil.com"
  ```

    * Respuesta **no** contiene `Access-Control-Allow-Origin`.
    * En un navegador, la consola JS mostrará un error CORS.

---

> Con estos ejemplos tienes cubiertas todas las rutas y mecanismos de error y seguridad. Ajusta los `{id}` según tu base y reutiliza estos casos para pruebas manuales o automatizadas.
