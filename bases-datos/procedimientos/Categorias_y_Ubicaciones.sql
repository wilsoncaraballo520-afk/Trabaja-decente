
USE TrabajaDecente;

DELIMITER $$

CREATE PROCEDURE sp_ListarCategorias()
BEGIN
    SELECT IdCategoria, NombreCategoria
    FROM Categorias
    WHERE Estado = 1
    ORDER BY NombreCategoria;
END$$

CREATE PROCEDURE sp_CrearCategoria(IN p_NombreCategoria VARCHAR(100))
BEGIN
    INSERT INTO Categorias (NombreCategoria)
    VALUES (TRIM(p_NombreCategoria));
    SELECT LAST_INSERT_ID() AS IdCategoria;
END$$

CREATE PROCEDURE sp_CambiarEstadoCategoria(
    IN p_IdCategoria INT,
    IN p_Estado TINYINT
)
BEGIN
    IF p_Estado NOT IN (0,1) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Estado no valido';
    END IF;

    UPDATE Categorias SET Estado = p_Estado
    WHERE IdCategoria = p_IdCategoria;

    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Categoria no encontrada o sin cambios';
    END IF;
END$$

CREATE PROCEDURE sp_ListarCiudades()
BEGIN
    SELECT CI.IdCiudad, CI.NombreCiudad, D.NombreDepartamento
    FROM Ciudades CI
    INNER JOIN Departamentos D ON D.IdDepartamento = CI.IdDepartamento
    WHERE D.NombreDepartamento = 'Antioquia'
    ORDER BY CI.NombreCiudad;
END$$



DELIMITER ;
