# Trabaja Decente

Sistema web de bolsa de empleo dirigido a estudiantes de la Institución Universitaria Pascual Bravo, que permite conectar estudiantes con empresas y facilitar la búsqueda de oportunidades laborales y prácticas en el Área Metropolitana del Valle de Aburrá.

## Tecnologías utilizadas

- **Frontend:** HTML, CSS y JavaScript.
- **Backend:** C# y ASP.NET Core (.NET 8).
- **Base de datos:** MySQL.
- **Seguridad:** JWT y BCrypt.
- **Control de versiones:** Git y GitHub.

## Requisitos

Para ejecutar el proyecto necesitas:

- Visual Studio Code.
- .NET 8 SDK.
- MySQL Server.
- MySQL Workbench.
- Git.
- Navegador web.

## Estructura del proyecto

```text
Trabaja-decente/
│
├── TrabajaDecente/
│   ├── Backend/
│   │   └── TrabajaDecente.Api/
│   └── Frontend/
│
├── bases-datos/
│   ├── crear_base_datos.sql
│   ├── Insertar_datos.sql
│   ├── Crear_vistas.sql
│   └── procedimientos/
│
└── README.md
```

## Instalación y ejecución

### 1. Descargar el proyecto

```bash
git clone https://github.com/wilsoncaraballo520-afk/Trabaja-decente.git
```

### 2. Crear la base de datos

Abre MySQL Workbench y ejecuta los scripts de la carpeta `bases-datos` en el siguiente orden:

1. `crear_base_datos.sql`
2. Los procedimientos almacenados de la carpeta `procedimientos`, excepto `Procedimientos_Backend.sql`.
3. `Insertar_datos.sql`
4. `Crear_vistas.sql`
5. `Procedimientos_Backend.sql`

**Importante:** el primer script elimina y vuelve a crear la base de datos. Si ya tienes información, realiza una copia de seguridad antes de ejecutarlo.

### 3. Configurar el backend

Abre una terminal en la carpeta:

```text
cd TrabajaDecente/Backend/TrabajaDecente.Api
```

Restaura las dependencias:

```bash
dotnet restore
```

Configura la conexión a MySQL mediante User Secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=TrabajaDecente;User=root;Password=TU_CONTRASEÑA;"
```

Configura la clave JWT:

```bash
dotnet user-secrets set "Jwt:Key" "TU_CLAVE_SEGURA_DE_AL_MENOS_32_CARACTERES"
```

Reemplaza los valores de ejemplo por tus propios datos. No publiques las credenciales en GitHub.

### 4. Ejecutar el backend

Desde la carpeta del proyecto API, ejecuta:

```bash
dotnet run
```

### 5. Ejecutar el frontend

Abre la carpeta `TrabajaDecente/Frontend` en Visual Studio Code y ejecuta el archivo `index.html` utilizando la extensión Live Server.

El backend debe estar funcionando para que el frontend pueda consultar y enviar información.

## Funcionalidades principales

- Registro e inicio de sesión de estudiantes y empresas.
- Administración de perfiles.
- Publicación y búsqueda de ofertas laborales.
- Gestión de hojas de vida.
- Postulación a ofertas.
- Administración de empresas y publicaciones.
- Gestión de postulaciones y sus estados.

## Autores

- Wilson Caraballo Salgado
- Nicolás Álvarez
- Samuel Pérez

**Institución Universitaria Pascual Bravo**

**Estado:** En desarrollo.