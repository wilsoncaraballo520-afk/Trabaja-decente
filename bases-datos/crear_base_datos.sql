DROP DATABASE IF EXISTS TrabajaDecente;
CREATE DATABASE TrabajaDecente
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
USE TrabajaDecente;

CREATE TABLE Administradores (
    IdAdministrador INT AUTO_INCREMENT PRIMARY KEY,
    Nombres VARCHAR(100) NOT NULL,
    CorreoElectronico VARCHAR(254) NOT NULL UNIQUE,
    ContrasenaHash VARCHAR(255) NOT NULL,
    Estado TINYINT(1) NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE Estudiantes (
    IdEstudiante INT AUTO_INCREMENT PRIMARY KEY,
    TipoDocumento VARCHAR(20) NOT NULL,
    NumeroDocumento VARCHAR(20) NOT NULL UNIQUE,
    Nombres VARCHAR(100) NOT NULL,
    PrimerApellido VARCHAR(50) NOT NULL,
    SegundoApellido VARCHAR(50) NULL,
    FechaNacimiento DATE NOT NULL,
    CorreoElectronico VARCHAR(254) NOT NULL UNIQUE,
    Telefono VARCHAR(20) NOT NULL,
    ContrasenaHash VARCHAR(255) NOT NULL,
    Estado TINYINT(1) NOT NULL DEFAULT 1,
    FechaRegistro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX IDX_Estudiantes_Estado (Estado)
) ENGINE=InnoDB;

CREATE TABLE Empresas (
    IdEmpresa INT AUTO_INCREMENT PRIMARY KEY,
    IdAdministrador INT NULL,
    NombreEmpresa VARCHAR(150) NOT NULL,
    NIT VARCHAR(20) NOT NULL UNIQUE,
    CorreoElectronico VARCHAR(254) NOT NULL UNIQUE,
    Telefono VARCHAR(20) NOT NULL,
    Direccion VARCHAR(200) NULL,
    ContrasenaHash VARCHAR(255) NOT NULL,
    Estado TINYINT(1) NOT NULL DEFAULT 1,
    EstadoAprobacion ENUM('Pendiente','Aprobada','Rechazada')
        NOT NULL DEFAULT 'Pendiente',
    FechaRegistro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Empresas_Administradores
        FOREIGN KEY (IdAdministrador)
        REFERENCES Administradores(IdAdministrador)
        ON DELETE SET NULL ON UPDATE CASCADE,
    INDEX IDX_Empresas_Aprobacion (EstadoAprobacion, Estado)
) ENGINE=InnoDB;

CREATE TABLE Categorias (
    IdCategoria INT AUTO_INCREMENT PRIMARY KEY,
    NombreCategoria VARCHAR(100) NOT NULL UNIQUE,
    Estado TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB;

CREATE TABLE Departamentos (
    IdDepartamento INT AUTO_INCREMENT PRIMARY KEY,
    NombreDepartamento VARCHAR(100) NOT NULL UNIQUE
) ENGINE=InnoDB;

CREATE TABLE Ciudades (
    IdCiudad INT AUTO_INCREMENT PRIMARY KEY,
    IdDepartamento INT NOT NULL,
    NombreCiudad VARCHAR(100) NOT NULL,
    CONSTRAINT FK_Ciudades_Departamentos
        FOREIGN KEY (IdDepartamento)
        REFERENCES Departamentos(IdDepartamento)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT UQ_Ciudad_Departamento
        UNIQUE (IdDepartamento, NombreCiudad)
) ENGINE=InnoDB;

CREATE TABLE HojasDeVida (
    IdHojaDeVida INT AUTO_INCREMENT PRIMARY KEY,
    IdEstudiante INT NOT NULL UNIQUE,
    NombreArchivo VARCHAR(255) NOT NULL,
    TipoArchivo VARCHAR(100) NOT NULL DEFAULT 'application/pdf',
    TamanoBytes BIGINT NOT NULL,
    ArchivoPDF LONGBLOB NOT NULL,
    FechaCarga DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_HojasDeVida_Estudiantes
        FOREIGN KEY (IdEstudiante)
        REFERENCES Estudiantes(IdEstudiante)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT CK_HojaVida_Tamano CHECK (TamanoBytes > 0 AND TamanoBytes <= 5242880),
    CONSTRAINT CK_HojaVida_Tipo CHECK (TipoArchivo = 'application/pdf')
) ENGINE=InnoDB;

CREATE TABLE OfertasLaborales (
    IdOferta INT AUTO_INCREMENT PRIMARY KEY,
    IdEmpresa INT NOT NULL,
    IdCategoria INT NOT NULL,
    IdCiudad INT NOT NULL,
    Titulo VARCHAR(150) NOT NULL,
    Descripcion TEXT NOT NULL,
    Salario DECIMAL(12,2) NULL,
    TipoContrato ENUM('Laboral','Practicas','Prestacion de servicios','Aprendizaje','Temporal') NOT NULL,
    Modalidad ENUM('Presencial','Remota','Hibrida') NOT NULL,
    Horario VARCHAR(100) NOT NULL,
    Vacantes INT NOT NULL,
    ExperienciaMinimaMeses INT NOT NULL DEFAULT 0,
    FechaPublicacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaCierre DATETIME NOT NULL,
    Estado ENUM('Abierta','Cerrada') NOT NULL DEFAULT 'Abierta',
    CONSTRAINT FK_Ofertas_Empresas
        FOREIGN KEY (IdEmpresa) REFERENCES Empresas(IdEmpresa)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT FK_Ofertas_Categorias
        FOREIGN KEY (IdCategoria) REFERENCES Categorias(IdCategoria)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT FK_Ofertas_Ciudades
        FOREIGN KEY (IdCiudad) REFERENCES Ciudades(IdCiudad)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT CK_Ofertas_Salario CHECK (Salario IS NULL OR Salario >= 0),
    CONSTRAINT CK_Ofertas_Vacantes CHECK (Vacantes > 0),
    CONSTRAINT CK_Ofertas_Experiencia CHECK (ExperienciaMinimaMeses >= 0),
    CONSTRAINT CK_Ofertas_Fechas CHECK (FechaCierre > FechaPublicacion),
    INDEX IDX_Ofertas_Empresa (IdEmpresa),
    INDEX IDX_Ofertas_Categoria (IdCategoria),
    INDEX IDX_Ofertas_Ciudad (IdCiudad),
    INDEX IDX_Ofertas_Estado_Fecha (Estado, FechaCierre, FechaPublicacion),
    FULLTEXT INDEX FTX_Ofertas_Busqueda (Titulo, Descripcion)
) ENGINE=InnoDB;

CREATE TABLE Postulaciones (
    IdPostulacion INT AUTO_INCREMENT PRIMARY KEY,
    IdEstudiante INT NOT NULL,
    IdOferta INT NOT NULL,
    FechaPostulacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Estado ENUM('Aplicado','En proceso','Proceso finalizado')
        NOT NULL DEFAULT 'Aplicado',
    CONSTRAINT FK_Postulaciones_Estudiantes
        FOREIGN KEY (IdEstudiante) REFERENCES Estudiantes(IdEstudiante)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT FK_Postulaciones_Ofertas
        FOREIGN KEY (IdOferta) REFERENCES OfertasLaborales(IdOferta)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT UQ_Postulacion UNIQUE (IdEstudiante, IdOferta),
    INDEX IDX_Postulaciones_Oferta_Estado (IdOferta, Estado),
    INDEX IDX_Postulaciones_Estudiante_Fecha (IdEstudiante, FechaPostulacion)
) ENGINE=InnoDB;

CREATE TABLE OfertasFavoritas (
    IdFavorito INT AUTO_INCREMENT PRIMARY KEY,
    IdEstudiante INT NOT NULL,
    IdOferta INT NOT NULL,
    FechaGuardado DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Favoritas_Estudiantes
        FOREIGN KEY (IdEstudiante) REFERENCES Estudiantes(IdEstudiante)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_Favoritas_Ofertas
        FOREIGN KEY (IdOferta) REFERENCES OfertasLaborales(IdOferta)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT UQ_OfertaFavorita UNIQUE (IdEstudiante, IdOferta),
    INDEX IDX_Favoritas_Estudiante (IdEstudiante)
) ENGINE=InnoDB;