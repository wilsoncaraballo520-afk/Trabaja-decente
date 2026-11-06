
USE TrabajaDecente;

DELIMITER $$

CREATE PROCEDURE sp_GuardarHojaVida(
    IN p_IdEstudiante INT,
    IN p_NombreArchivo VARCHAR(255),
    IN p_TipoArchivo VARCHAR(100),
    IN p_TamanoBytes BIGINT,
    IN p_ArchivoPDF LONGBLOB
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM Estudiantes
        WHERE IdEstudiante = p_IdEstudiante AND Estado = 1
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Estudiante no valido o inactivo';
    END IF;

    IF p_TipoArchivo <> 'application/pdf'
       OR p_TamanoBytes <= 0 OR p_TamanoBytes > 5242880
       OR OCTET_LENGTH(p_ArchivoPDF) <> p_TamanoBytes THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Archivo PDF no valido o supera 5 MB';
    END IF;

    INSERT INTO HojasDeVida (
        IdEstudiante, NombreArchivo, TipoArchivo, TamanoBytes, ArchivoPDF
    )
    VALUES (
        p_IdEstudiante, p_NombreArchivo, p_TipoArchivo, p_TamanoBytes, p_ArchivoPDF
    )
    ON DUPLICATE KEY UPDATE
        NombreArchivo = VALUES(NombreArchivo),
        TipoArchivo = VALUES(TipoArchivo),
        TamanoBytes = VALUES(TamanoBytes),
        ArchivoPDF = VALUES(ArchivoPDF),
        FechaCarga = CURRENT_TIMESTAMP;

    SELECT IdHojaDeVida, NombreArchivo, TamanoBytes, FechaCarga
    FROM HojasDeVida
    WHERE IdEstudiante = p_IdEstudiante;
END$$

CREATE PROCEDURE sp_ObtenerHojaVidaEstudiante(IN p_IdEstudiante INT)
BEGIN
    SELECT IdHojaDeVida, NombreArchivo, TipoArchivo,
           TamanoBytes, FechaCarga
    FROM HojasDeVida
    WHERE IdEstudiante = p_IdEstudiante;
END$$

CREATE PROCEDURE sp_DescargarHojaVidaPorEmpresa(
    IN p_IdEmpresa INT,
    IN p_IdEstudiante INT,
    IN p_IdOferta INT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM Postulaciones P
        INNER JOIN OfertasLaborales O ON O.IdOferta = P.IdOferta
        WHERE P.IdEstudiante = p_IdEstudiante
          AND O.IdOferta = p_IdOferta
          AND O.IdEmpresa = p_IdEmpresa
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No autorizado para consultar esta hoja de vida';
    END IF;

    SELECT H.NombreArchivo, H.TipoArchivo, H.TamanoBytes,
           H.ArchivoPDF, H.FechaCarga
    FROM HojasDeVida H
    WHERE H.IdEstudiante = p_IdEstudiante;
END$$

CREATE PROCEDURE sp_EliminarHojaVida(IN p_IdEstudiante INT)
BEGIN
    DELETE FROM HojasDeVida WHERE IdEstudiante = p_IdEstudiante;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Hoja de vida no encontrada';
    END IF;
END$$


DELIMITER ;
