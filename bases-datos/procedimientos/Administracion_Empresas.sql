
USE TrabajaDecente;

DELIMITER $$

CREATE PROCEDURE sp_AprobarEmpresa(
    IN p_IdEmpresa INT,
    IN p_IdAdministrador INT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM Administradores
        WHERE IdAdministrador = p_IdAdministrador AND Estado = 1
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Administrador no valido o inactivo';
    END IF;

    UPDATE Empresas
    SET EstadoAprobacion = 'Aprobada',
        IdAdministrador = p_IdAdministrador
    WHERE IdEmpresa = p_IdEmpresa AND Estado = 1;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Empresa no encontrada o inactiva';
    END IF;
END$$

CREATE PROCEDURE sp_RechazarEmpresa(
    IN p_IdEmpresa INT,
    IN p_IdAdministrador INT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM Administradores
        WHERE IdAdministrador = p_IdAdministrador AND Estado = 1
    ) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Administrador no valido o inactivo';
    END IF;

    UPDATE Empresas
    SET EstadoAprobacion = 'Rechazada',
        IdAdministrador = p_IdAdministrador
    WHERE IdEmpresa = p_IdEmpresa AND Estado = 1;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Empresa no encontrada o inactiva';
    END IF;
END$$

CREATE PROCEDURE sp_ListarEmpresasPendientes()
BEGIN
    SELECT * FROM vw_EmpresasPendientes ORDER BY FechaRegistro ASC;
END$$

CREATE PROCEDURE sp_ListarEmpresas()
BEGIN
    SELECT IdEmpresa, NombreEmpresa, NIT, CorreoElectronico,
           Telefono, Estado, EstadoAprobacion, FechaRegistro
    FROM Empresas
    ORDER BY FechaRegistro DESC;
END$$

CREATE PROCEDURE sp_CambiarEstadoEmpresa(
    IN p_IdEmpresa INT,
    IN p_Estado TINYINT
)
BEGIN
    IF p_Estado NOT IN (0,1) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Estado no valido';
    END IF;

    UPDATE Empresas SET Estado = p_Estado WHERE IdEmpresa = p_IdEmpresa;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Empresa no encontrada o sin cambios';
    END IF;
END$$


DELIMITER ;
