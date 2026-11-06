
USE TrabajaDecente;

DELIMITER $$

CREATE PROCEDURE sp_VerPerfilEstudiante(IN p_IdEstudiante INT)
BEGIN
    SELECT IdEstudiante, TipoDocumento, NumeroDocumento, Nombres,
           PrimerApellido, SegundoApellido, FechaNacimiento,
           CorreoElectronico, Telefono, FechaRegistro
    FROM Estudiantes
    WHERE IdEstudiante = p_IdEstudiante AND Estado = 1;
END$$

CREATE PROCEDURE sp_ActualizarPerfilEstudiante(
    IN p_IdEstudiante INT,
    IN p_Nombres VARCHAR(100),
    IN p_PrimerApellido VARCHAR(50),
    IN p_SegundoApellido VARCHAR(50),
    IN p_Telefono VARCHAR(20)
)
BEGIN
    UPDATE Estudiantes
    SET Nombres = p_Nombres,
        PrimerApellido = p_PrimerApellido,
        SegundoApellido = p_SegundoApellido,
        Telefono = p_Telefono
    WHERE IdEstudiante = p_IdEstudiante AND Estado = 1;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Estudiante no encontrado o sin cambios';
    END IF;
END$$

CREATE PROCEDURE sp_ResumenEstudiante(IN p_IdEstudiante INT)
BEGIN
    SELECT
        (SELECT COUNT(*) FROM Postulaciones
         WHERE IdEstudiante = p_IdEstudiante) AS TotalPostulaciones,
        (SELECT COUNT(*) FROM Postulaciones
         WHERE IdEstudiante = p_IdEstudiante AND Estado = 'En proceso') AS EnProceso,
        (SELECT COUNT(*) FROM OfertasFavoritas
         WHERE IdEstudiante = p_IdEstudiante) AS TotalFavoritas,
        (SELECT COUNT(*) FROM HojasDeVida
         WHERE IdEstudiante = p_IdEstudiante) AS TieneHojaDeVida;
END$$

CREATE PROCEDURE sp_ResumenEmpresa(IN p_IdEmpresa INT)
BEGIN
    SELECT
        (SELECT COUNT(*) FROM OfertasLaborales
         WHERE IdEmpresa = p_IdEmpresa AND Estado = 'Abierta'
           AND FechaCierre >= NOW()) AS OfertasActivas,
        (SELECT COUNT(*) FROM Postulaciones P
         INNER JOIN OfertasLaborales O ON O.IdOferta = P.IdOferta
         WHERE O.IdEmpresa = p_IdEmpresa) AS TotalPostulaciones;
END$$

CREATE PROCEDURE sp_ResumenAdministrador()
BEGIN
    SELECT
        (SELECT COUNT(*) FROM Empresas
         WHERE EstadoAprobacion = 'Pendiente' AND Estado = 1) AS EmpresasPendientes,
        (SELECT COUNT(*) FROM Empresas
         WHERE EstadoAprobacion = 'Aprobada' AND Estado = 1) AS EmpresasAprobadas,
        (SELECT COUNT(*) FROM OfertasLaborales
         WHERE Estado = 'Abierta' AND FechaCierre >= NOW()) AS OfertasActivas,
        (SELECT COUNT(*) FROM Estudiantes
         WHERE Estado = 1) AS EstudiantesActivos;
END$$


DELIMITER ;
