
USE TrabajaDecente;

DELIMITER $$

CREATE PROCEDURE sp_GuardarFavorita(
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
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Oferta no disponible';
    END IF;

    INSERT INTO OfertasFavoritas (IdEstudiante, IdOferta)
    VALUES (p_IdEstudiante, p_IdOferta);
END$$

CREATE PROCEDURE sp_EliminarFavorita(
    IN p_IdEstudiante INT,
    IN p_IdOferta INT
)
BEGIN
    DELETE FROM OfertasFavoritas
    WHERE IdEstudiante = p_IdEstudiante AND IdOferta = p_IdOferta;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Favorito no encontrado';
    END IF;
END$$

CREATE PROCEDURE sp_VerFavoritas(IN p_IdEstudiante INT)
BEGIN
    SELECT F.FechaGuardado, V.*
    FROM OfertasFavoritas F
    INNER JOIN vw_OfertasPublicadas V ON V.IdOferta = F.IdOferta
    WHERE F.IdEstudiante = p_IdEstudiante
    ORDER BY F.FechaGuardado DESC;
END$$


DELIMITER ;
