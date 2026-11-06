USE TrabajaDecente;

CREATE TABLE IF NOT EXISTS RefreshTokens (
    IdRefreshToken BIGINT AUTO_INCREMENT PRIMARY KEY,
    Rol ENUM('Estudiante','Empresa','Administrador') NOT NULL,
    IdUsuario INT NOT NULL,
    TokenHash CHAR(64) NOT NULL UNIQUE,      
    FechaCreacion DATETIME NOT NULL,         
    FechaExpiracion DATETIME NOT NULL,       
    FechaRevocacion DATETIME NULL,          
    INDEX IDX_RefreshTokens_Usuario (Rol, IdUsuario)
) ENGINE=InnoDB;

DELIMITER $$

CREATE PROCEDURE sp_GuardarRefreshToken(
    IN p_Rol VARCHAR(20), IN p_IdUsuario INT,
    IN p_TokenHash CHAR(64), IN p_FechaExpiracion DATETIME)
BEGIN
    INSERT INTO RefreshTokens (Rol, IdUsuario, TokenHash, FechaCreacion, FechaExpiracion)
    VALUES (p_Rol, p_IdUsuario, p_TokenHash, UTC_TIMESTAMP(), p_FechaExpiracion);
END$$

CREATE PROCEDURE sp_ObtenerRefreshToken(IN p_TokenHash CHAR(64))
BEGIN
    SELECT IdRefreshToken, Rol, IdUsuario, FechaExpiracion,
           CAST(FechaRevocacion IS NOT NULL AS SIGNED) AS Revocado,
           TIMESTAMPDIFF(SECOND, FechaRevocacion, UTC_TIMESTAMP()) AS SegundosDesdeRevocacion
    FROM RefreshTokens
    WHERE TokenHash = p_TokenHash;
END$$

CREATE PROCEDURE sp_RevocarRefreshToken(IN p_TokenHash CHAR(64))
BEGIN
    UPDATE RefreshTokens SET FechaRevocacion = UTC_TIMESTAMP()
    WHERE TokenHash = p_TokenHash AND FechaRevocacion IS NULL;
END$$

CREATE PROCEDURE sp_RevocarRefreshTokensUsuario(IN p_Rol VARCHAR(20), IN p_IdUsuario INT)
BEGIN
    UPDATE RefreshTokens SET FechaRevocacion = UTC_TIMESTAMP()
    WHERE Rol = p_Rol AND IdUsuario = p_IdUsuario AND FechaRevocacion IS NULL;
    DELETE FROM RefreshTokens WHERE FechaExpiracion < UTC_TIMESTAMP() - INTERVAL 30 DAY;
END$$

CREATE PROCEDURE sp_ObtenerEstudiantePorId(IN p_IdEstudiante INT)
BEGIN
    SELECT IdEstudiante AS IdUsuario, Nombres AS Nombre, Estado
    FROM Estudiantes WHERE IdEstudiante = p_IdEstudiante;
END$$

CREATE PROCEDURE sp_ObtenerEmpresaPorId(IN p_IdEmpresa INT)
BEGIN
    SELECT IdEmpresa AS IdUsuario, NombreEmpresa AS Nombre, Estado, EstadoAprobacion
    FROM Empresas WHERE IdEmpresa = p_IdEmpresa;
END$$

CREATE PROCEDURE sp_ObtenerAdministradorPorId(IN p_IdAdministrador INT)
BEGIN
    SELECT IdAdministrador AS IdUsuario, Nombres AS Nombre, Estado
    FROM Administradores WHERE IdAdministrador = p_IdAdministrador;
END$$

CREATE PROCEDURE sp_DescargarMiHojaVida(IN p_IdEstudiante INT)
BEGIN
    SELECT NombreArchivo, TipoArchivo, TamanoBytes, ArchivoPDF, FechaCarga
    FROM HojasDeVida WHERE IdEstudiante = p_IdEstudiante;
END$$

CREATE PROCEDURE sp_DescargarHojaVidaPorPostulacion(
    IN p_IdEmpresa INT, IN p_IdPostulacion INT)
BEGIN
    DECLARE v_IdEstudiante INT DEFAULT NULL;
    DECLARE v_IdOferta INT DEFAULT NULL;

    SELECT P.IdEstudiante, P.IdOferta INTO v_IdEstudiante, v_IdOferta
    FROM Postulaciones P
    INNER JOIN OfertasLaborales O ON O.IdOferta = P.IdOferta
    WHERE P.IdPostulacion = p_IdPostulacion AND O.IdEmpresa = p_IdEmpresa;

    IF v_IdEstudiante IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No autorizado para consultar esta hoja de vida';
    END IF;

    CALL sp_DescargarHojaVidaPorEmpresa(p_IdEmpresa, v_IdEstudiante, v_IdOferta);
END$$

CREATE PROCEDURE sp_ListarCategoriasAdmin()
BEGIN
    SELECT IdCategoria, NombreCategoria, Estado
    FROM Categorias ORDER BY NombreCategoria;
END$$

CREATE PROCEDURE sp_ContarOfertasPorCategoria()
BEGIN
    SELECT C.IdCategoria, C.NombreCategoria, COUNT(V.IdOferta) AS TotalOfertas
    FROM Categorias C
    LEFT JOIN vw_OfertasPublicadas V ON V.IdCategoria = C.IdCategoria
    WHERE C.Estado = 1
    GROUP BY C.IdCategoria, C.NombreCategoria
    ORDER BY TotalOfertas DESC, C.NombreCategoria;
END$$

DELIMITER ;
