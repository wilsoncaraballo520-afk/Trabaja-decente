
USE TrabajaDecente;

DELIMITER $$

CREATE PROCEDURE sp_PostularOferta(
    IN p_IdEstudiante INT,
    IN p_IdOferta INT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM Estudiantes
        WHERE IdEstudiante = p_IdEstudiante AND Estado = 1
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Estudiante no valido o inactivo';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM vw_OfertasPublicadas
        WHERE IdOferta = p_IdOferta
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La oferta no esta disponible';
    END IF;

    IF EXISTS (
        SELECT 1 FROM Postulaciones
        WHERE IdEstudiante = p_IdEstudiante AND IdOferta = p_IdOferta
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya existe una postulacion para esta oferta';
    END IF;

    INSERT INTO Postulaciones (IdEstudiante, IdOferta)
    VALUES (p_IdEstudiante, p_IdOferta);

    SELECT LAST_INSERT_ID() AS IdPostulacion;
END$$

CREATE PROCEDURE sp_VerPostulacionesEstudiante(IN p_IdEstudiante INT)
BEGIN
    SELECT P.IdPostulacion, P.FechaPostulacion,
           P.Estado AS EstadoPostulacion,
           O.IdOferta, O.Titulo, O.Salario, O.Modalidad,
           O.TipoContrato, E.NombreEmpresa, CI.NombreCiudad
    FROM Postulaciones P
    INNER JOIN OfertasLaborales O ON O.IdOferta = P.IdOferta
    INNER JOIN Empresas E ON E.IdEmpresa = O.IdEmpresa
    INNER JOIN Ciudades CI ON CI.IdCiudad = O.IdCiudad
    WHERE P.IdEstudiante = p_IdEstudiante
    ORDER BY P.FechaPostulacion DESC;
END$$

CREATE PROCEDURE sp_VerPostulacionesEmpresa(
    IN p_IdEmpresa INT,
    IN p_IdOferta INT
)
BEGIN
    SELECT P.IdPostulacion, P.FechaPostulacion, P.Estado,
           E.IdEstudiante, E.Nombres, E.PrimerApellido,
           E.SegundoApellido, E.CorreoElectronico, E.Telefono,
           O.Titulo,
           IF(H.IdHojaDeVida IS NULL, 0, 1) AS TieneHojaDeVida
    FROM Postulaciones P
    INNER JOIN Estudiantes E ON E.IdEstudiante = P.IdEstudiante
    INNER JOIN OfertasLaborales O ON O.IdOferta = P.IdOferta
    LEFT JOIN HojasDeVida H ON H.IdEstudiante = E.IdEstudiante
    WHERE O.IdEmpresa = p_IdEmpresa
      AND (p_IdOferta IS NULL OR O.IdOferta = p_IdOferta)
    ORDER BY P.FechaPostulacion DESC;
END$$

CREATE PROCEDURE sp_ActualizarEstadoPostulacion(
    IN p_IdEmpresa INT,
    IN p_IdPostulacion INT,
    IN p_NuevoEstado VARCHAR(30)
)
BEGIN
    IF p_NuevoEstado NOT IN ('Aplicado','En proceso','Proceso finalizado') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Estado de postulacion no valido';
    END IF;

    UPDATE Postulaciones P
    INNER JOIN OfertasLaborales O ON O.IdOferta = P.IdOferta
    SET P.Estado = p_NuevoEstado
    WHERE P.IdPostulacion = p_IdPostulacion
      AND O.IdEmpresa = p_IdEmpresa;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Postulacion no encontrada o sin cambios';
    END IF;
END$$


DELIMITER ;
