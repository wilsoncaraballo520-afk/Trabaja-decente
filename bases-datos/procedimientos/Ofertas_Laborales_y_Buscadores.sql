
USE TrabajaDecente;

DELIMITER $$

CREATE PROCEDURE sp_PublicarOferta(
    IN p_IdEmpresa INT,
    IN p_IdCategoria INT,
    IN p_IdCiudad INT,
    IN p_Titulo VARCHAR(150),
    IN p_Descripcion TEXT,
    IN p_Salario DECIMAL(12,2),
    IN p_TipoContrato VARCHAR(50),
    IN p_Modalidad VARCHAR(20),
    IN p_Horario VARCHAR(100),
    IN p_Vacantes INT,
    IN p_ExperienciaMinimaMeses INT,
    IN p_FechaCierre DATETIME
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM Empresas
        WHERE IdEmpresa = p_IdEmpresa
          AND Estado = 1 AND EstadoAprobacion = 'Aprobada'
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La empresa no esta aprobada o activa';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM Categorias
        WHERE IdCategoria = p_IdCategoria AND Estado = 1
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Categoria no valida';
    END IF;

    -- Solo se permiten los municipios cargados para el alcance inicial.
    IF NOT EXISTS (
        SELECT 1
        FROM Ciudades CI
        INNER JOIN Departamentos D ON D.IdDepartamento = CI.IdDepartamento
        WHERE CI.IdCiudad = p_IdCiudad
          AND D.NombreDepartamento = 'Antioquia'
          AND CI.NombreCiudad IN (
              'Medellin','Bello','Itagui','Envigado','Sabaneta',
              'La Estrella','Caldas','Copacabana','Girardota','Barbosa'
          )
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La ubicacion esta fuera de la zona habilitada';
    END IF;

    IF p_FechaCierre <= NOW() THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La fecha de cierre debe ser futura';
    END IF;

    IF p_Vacantes <= 0 OR p_ExperienciaMinimaMeses < 0
       OR (p_Salario IS NOT NULL AND p_Salario < 0) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Datos numericos de la oferta no validos';
    END IF;

    INSERT INTO OfertasLaborales (
        IdEmpresa, IdCategoria, IdCiudad, Titulo, Descripcion,
        Salario, TipoContrato, Modalidad, Horario, Vacantes,
        ExperienciaMinimaMeses, FechaCierre
    )
    VALUES (
        p_IdEmpresa, p_IdCategoria, p_IdCiudad, p_Titulo, p_Descripcion,
        p_Salario, p_TipoContrato, p_Modalidad, p_Horario, p_Vacantes,
        p_ExperienciaMinimaMeses, p_FechaCierre
    );

    SELECT LAST_INSERT_ID() AS IdOferta;
END$$

CREATE PROCEDURE sp_EditarOferta(
    IN p_IdOferta INT,
    IN p_IdEmpresa INT,
    IN p_IdCategoria INT,
    IN p_IdCiudad INT,
    IN p_Titulo VARCHAR(150),
    IN p_Descripcion TEXT,
    IN p_Salario DECIMAL(12,2),
    IN p_TipoContrato VARCHAR(50),
    IN p_Modalidad VARCHAR(20),
    IN p_Horario VARCHAR(100),
    IN p_Vacantes INT,
    IN p_ExperienciaMinimaMeses INT,
    IN p_FechaCierre DATETIME
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM Empresas
        WHERE IdEmpresa = p_IdEmpresa AND Estado = 1
          AND EstadoAprobacion = 'Aprobada'
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Empresa no autorizada';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM Ciudades
        WHERE IdCiudad = p_IdCiudad
          AND NombreCiudad IN (
              'Medellin','Bello','Itagui','Envigado','Sabaneta',
              'La Estrella','Caldas','Copacabana','Girardota','Barbosa'
          )
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ubicacion fuera de la zona habilitada';
    END IF;

    IF p_FechaCierre <= NOW() OR p_Vacantes <= 0
       OR p_ExperienciaMinimaMeses < 0
       OR (p_Salario IS NOT NULL AND p_Salario < 0) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Datos de la oferta no validos';
    END IF;

    UPDATE OfertasLaborales
    SET IdCategoria = p_IdCategoria,
        IdCiudad = p_IdCiudad,
        Titulo = p_Titulo,
        Descripcion = p_Descripcion,
        Salario = p_Salario,
        TipoContrato = p_TipoContrato,
        Modalidad = p_Modalidad,
        Horario = p_Horario,
        Vacantes = p_Vacantes,
        ExperienciaMinimaMeses = p_ExperienciaMinimaMeses,
        FechaCierre = p_FechaCierre,
        Estado = 'Abierta'
    WHERE IdOferta = p_IdOferta AND IdEmpresa = p_IdEmpresa;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Oferta no encontrada o sin cambios';
    END IF;
END$$

CREATE PROCEDURE sp_CerrarOferta(
    IN p_IdOferta INT,
    IN p_IdEmpresa INT
)
BEGIN
    UPDATE OfertasLaborales
    SET Estado = 'Cerrada'
    WHERE IdOferta = p_IdOferta AND IdEmpresa = p_IdEmpresa;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Oferta no encontrada o ya cerrada';
    END IF;
END$$

CREATE PROCEDURE sp_VerOfertasEmpresa(IN p_IdEmpresa INT)
BEGIN
    SELECT O.*, C.NombreCategoria, CI.NombreCiudad,
           (SELECT COUNT(*) FROM Postulaciones P
            WHERE P.IdOferta = O.IdOferta) AS TotalPostulaciones
    FROM OfertasLaborales O
    INNER JOIN Categorias C ON C.IdCategoria = O.IdCategoria
    INNER JOIN Ciudades CI ON CI.IdCiudad = O.IdCiudad
    WHERE O.IdEmpresa = p_IdEmpresa
    ORDER BY O.FechaPublicacion DESC;
END$$

CREATE PROCEDURE sp_VerDetalleOferta(IN p_IdOferta INT)
BEGIN
    SELECT * FROM vw_OfertasPublicadas
    WHERE IdOferta = p_IdOferta;
END$$

CREATE PROCEDURE sp_FiltrarOfertas(
    IN p_PalabraClave VARCHAR(150),
    IN p_IdCategoria INT,
    IN p_IdCiudad INT,
    IN p_Modalidad VARCHAR(20),
    IN p_TipoContrato VARCHAR(50),
    IN p_SalarioMinimo DECIMAL(12,2),
    IN p_SalarioMaximo DECIMAL(12,2)
)
BEGIN
    SELECT *
    FROM vw_OfertasPublicadas V
    WHERE (p_PalabraClave IS NULL OR
           V.Titulo LIKE CONCAT('%', p_PalabraClave, '%') OR
           V.Descripcion LIKE CONCAT('%', p_PalabraClave, '%') OR
           V.NombreEmpresa LIKE CONCAT('%', p_PalabraClave, '%'))
      AND (p_IdCategoria IS NULL OR V.IdCategoria = p_IdCategoria)
      AND (p_IdCiudad IS NULL OR V.IdCiudad = p_IdCiudad)
      AND (p_Modalidad IS NULL OR V.Modalidad = p_Modalidad)
      AND (p_TipoContrato IS NULL OR V.TipoContrato = p_TipoContrato)
      AND (p_SalarioMinimo IS NULL OR V.Salario >= p_SalarioMinimo)
      AND (p_SalarioMaximo IS NULL OR V.Salario <= p_SalarioMaximo)
    ORDER BY V.FechaPublicacion DESC;
END$$


DELIMITER ;
