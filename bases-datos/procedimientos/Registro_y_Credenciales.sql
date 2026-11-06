
USE TrabajaDecente;

DELIMITER $$

CREATE PROCEDURE sp_RegistrarEstudiante(
    IN p_TipoDocumento VARCHAR(20),
    IN p_NumeroDocumento VARCHAR(20),
    IN p_Nombres VARCHAR(100),
    IN p_PrimerApellido VARCHAR(50),
    IN p_SegundoApellido VARCHAR(50),
    IN p_FechaNacimiento DATE,
    IN p_CorreoElectronico VARCHAR(254),
    IN p_Telefono VARCHAR(20),
    IN p_ContrasenaHash VARCHAR(255)
)
BEGIN
    IF p_FechaNacimiento >= CURDATE() THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La fecha de nacimiento no es valida';
    END IF;

    IF p_ContrasenaHash IS NULL OR CHAR_LENGTH(p_ContrasenaHash) < 20 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Debe enviarse un hash de contrasena valido';
    END IF;

    INSERT INTO Estudiantes (
        TipoDocumento, NumeroDocumento, Nombres, PrimerApellido,
        SegundoApellido, FechaNacimiento, CorreoElectronico,
        Telefono, ContrasenaHash
    )
    VALUES (
        p_TipoDocumento, p_NumeroDocumento, p_Nombres, p_PrimerApellido,
        p_SegundoApellido, p_FechaNacimiento, p_CorreoElectronico,
        p_Telefono, p_ContrasenaHash
    );

    SELECT LAST_INSERT_ID() AS IdEstudiante;
END$$

CREATE PROCEDURE sp_RegistrarEmpresa(
    IN p_NombreEmpresa VARCHAR(150),
    IN p_NIT VARCHAR(20),
    IN p_CorreoElectronico VARCHAR(254),
    IN p_Telefono VARCHAR(20),
    IN p_Direccion VARCHAR(200),
    IN p_ContrasenaHash VARCHAR(255)
)
BEGIN
    IF p_ContrasenaHash IS NULL OR CHAR_LENGTH(p_ContrasenaHash) < 20 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Debe enviarse un hash de contrasena valido';
    END IF;

    INSERT INTO Empresas (
        NombreEmpresa, NIT, CorreoElectronico, Telefono,
        Direccion, ContrasenaHash, Estado, EstadoAprobacion
    )
    VALUES (
        p_NombreEmpresa, p_NIT, p_CorreoElectronico, p_Telefono,
        p_Direccion, p_ContrasenaHash, 1, 'Pendiente'
    );

    SELECT LAST_INSERT_ID() AS IdEmpresa;
END$$

CREATE PROCEDURE sp_ObtenerCredencialesEstudiante(IN p_Correo VARCHAR(254))
BEGIN
    SELECT IdEstudiante AS IdUsuario, Nombres AS Nombre,
           CorreoElectronico, ContrasenaHash, Estado
    FROM Estudiantes
    WHERE CorreoElectronico = p_Correo;
END$$

CREATE PROCEDURE sp_ObtenerCredencialesEmpresa(IN p_Correo VARCHAR(254))
BEGIN
    SELECT IdEmpresa AS IdUsuario, NombreEmpresa AS Nombre,
           CorreoElectronico, ContrasenaHash, Estado,
           EstadoAprobacion
    FROM Empresas
    WHERE CorreoElectronico = p_Correo;
END$$

CREATE PROCEDURE sp_ObtenerCredencialesAdministrador(IN p_Correo VARCHAR(254))
BEGIN
    SELECT IdAdministrador AS IdUsuario, Nombres AS Nombre,
           CorreoElectronico, ContrasenaHash, Estado
    FROM Administradores
    WHERE CorreoElectronico = p_Correo;
END$$


DELIMITER ;
