# Trabaja Decente — aplicación web

Bolsa de empleo para estudiantes de la Institución Universitaria Pascual Bravo (Área Metropolitana del Valle de Aburrá).

## 1. Base de datos
1. Ejecuta la base de dadtos, sus vistas, procedimientos execto uno que se tiene que ejecutar al final.
2. Ejecuta **`Procedimientos_Backend.sql`** al final. No modifica nada existente: agrega la tabla `RefreshTokens` y procedimientos auxiliares (sesión, descarga de hoja de vida, categorías de administración).

## 2. Configurar secretos (nunca en el repositorio)
```bash
cd Backend/TrabajaDecente.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Port=3306;Database=TrabajaDecente;User=TU_USUARIO;Password=TU_CLAVE;"
dotnet user-secrets set "Jwt:Key" "una-clave-larga-de-al-menos-32-caracteres-123456"
```

## 3. Crear el primer administrador
```bash
dotnet run -- --hash "Admind2355"    
```
Luego en MySQL 
```sql
INSERT INTO Administradores (Nombres, CorreoElectronico, ContrasenaHash)
VALUES ('Administrador', 'admin@tudominio.com', 'PEGA_AQUI_EL_HASH');
```

## 4. Ejecutar
```bash
dotnet dev-certs https --trust
dotnet run --launch-profile https
```
La API sirve también el frontend (mismo origen): abre `https://localhost:7080`.

