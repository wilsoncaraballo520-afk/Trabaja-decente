USE TrabajaDecente;

INSERT INTO Categorias (NombreCategoria) VALUES
('Tecnologia y sistemas'), ('Administracion'),
('Ventas y comercio'), ('Atencion al cliente'),
('Diseno y comunicacion'), ('Educacion'),
('Ingenieria'), ('Logistica'),
('Servicios generales'), ('Contabilidad y finanzas'),
('Marketing'), ('Otras areas');

INSERT INTO Departamentos (NombreDepartamento)
VALUES ('Antioquia');

INSERT INTO Ciudades (IdDepartamento, NombreCiudad) VALUES
(1, 'Medellin'), (1, 'Bello'),
(1, 'Itagui'), (1, 'Envigado'),
(1, 'Sabaneta'), (1, 'La Estrella'),
(1, 'Caldas'), (1, 'Copacabana'),
(1, 'Girardota'), (1, 'Barbosa');

INSERT INTO Administradores (Nombres, CorreoElectronico, ContrasenaHash)
VALUES ('Administrador', 'admin@tudominio.com', '$12$uMU6BjmutaGnU8sGJgH4NepN8.0jrr58RR0GMOWTIB1u2n2sZdlCW');
