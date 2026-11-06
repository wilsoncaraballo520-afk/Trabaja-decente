
USE TrabajaDecente;

CREATE VIEW vw_OfertasPublicadas AS
SELECT
    O.IdOferta, O.Titulo, O.Descripcion, O.Salario,
    O.TipoContrato, O.Modalidad, O.Horario, O.Vacantes,
    O.ExperienciaMinimaMeses, O.FechaPublicacion, O.FechaCierre,
    E.IdEmpresa, E.NombreEmpresa,
    C.IdCategoria, C.NombreCategoria,
    CI.IdCiudad, CI.NombreCiudad, D.NombreDepartamento
FROM OfertasLaborales O
INNER JOIN Empresas E ON E.IdEmpresa = O.IdEmpresa
INNER JOIN Categorias C ON C.IdCategoria = O.IdCategoria
INNER JOIN Ciudades CI ON CI.IdCiudad = O.IdCiudad
INNER JOIN Departamentos D ON D.IdDepartamento = CI.IdDepartamento
WHERE O.Estado = 'Abierta'
  AND O.FechaCierre >= NOW()
  AND E.Estado = 1
  AND E.EstadoAprobacion = 'Aprobada'
  AND C.Estado = 1;

CREATE VIEW vw_EmpresasPendientes AS
SELECT IdEmpresa, NombreEmpresa, NIT, CorreoElectronico,
       Telefono, FechaRegistro, EstadoAprobacion
FROM Empresas
WHERE EstadoAprobacion = 'Pendiente' AND Estado = 1;

CREATE VIEW vw_PostulacionesDetalle AS
SELECT
    P.IdPostulacion, P.FechaPostulacion, P.Estado,
    E.IdEstudiante, E.Nombres, E.PrimerApellido, E.SegundoApellido,
    E.CorreoElectronico AS CorreoEstudiante,
    O.IdOferta, O.Titulo,
    EM.IdEmpresa, EM.NombreEmpresa
FROM Postulaciones P
INNER JOIN Estudiantes E ON E.IdEstudiante = P.IdEstudiante
INNER JOIN OfertasLaborales O ON O.IdOferta = P.IdOferta
INNER JOIN Empresas EM ON EM.IdEmpresa = O.IdEmpresa;


