-- Ejecutar en SQL Server Management Studio sobre una base nueva.
-- Al iniciar la aplicación, Identity crea los roles y las cuentas de desarrollo.
IF DB_ID(N'UnivalleMicrocursos') IS NULL
    CREATE DATABASE [UnivalleMicrocursos];
GO
USE [UnivalleMicrocursos];
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO
CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [Nombre] nvarchar(80) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO
CREATE TABLE [AspNetRoleClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO
CREATE TABLE [AspNetUserClaims] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO
CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO
CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO
CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Cursos] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Titulo] nvarchar(80) NOT NULL,
    [Descripcion] nvarchar(1200) NOT NULL,
    [Categoria] nvarchar(30) NOT NULL,
    [Nivel] nvarchar(20) NOT NULL,
    [Portada] nvarchar(20) NOT NULL,
    [Publicado] bit NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    CONSTRAINT [PK_Cursos] PRIMARY KEY ([Id])
);
GO
CREATE TABLE [Lecciones] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CursoId] int NOT NULL,
    [Titulo] nvarchar(100) NOT NULL,
    [Contenido] nvarchar(max) NOT NULL,
    [DuracionMinutos] int NOT NULL,
    [Orden] int NOT NULL,
    CONSTRAINT [PK_Lecciones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Lecciones_Cursos_CursoId] FOREIGN KEY ([CursoId]) REFERENCES [Cursos] ([Id]) ON DELETE CASCADE
);
GO
CREATE TABLE [Inscripciones] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CursoId] int NOT NULL,
    [UsuarioId] nvarchar(450) NOT NULL,
    [FechaInscripcion] datetime2 NOT NULL,
    CONSTRAINT [PK_Inscripciones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Inscripciones_Cursos_CursoId] FOREIGN KEY ([CursoId]) REFERENCES [Cursos] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Inscripciones_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO
CREATE TABLE [LeccionesCompletadas] (
    [InscripcionId] int NOT NULL,
    [LeccionId] int NOT NULL,
    [FechaCompletada] datetime2 NOT NULL,
    CONSTRAINT [PK_LeccionesCompletadas] PRIMARY KEY ([InscripcionId], [LeccionId]),
    CONSTRAINT [FK_LeccionesCompletadas_Inscripciones_InscripcionId] FOREIGN KEY ([InscripcionId]) REFERENCES [Inscripciones] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LeccionesCompletadas_Lecciones_LeccionId] FOREIGN KEY ([LeccionId]) REFERENCES [Lecciones] ([Id])
);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
CREATE INDEX [IX_Lecciones_CursoId_Orden] ON [Lecciones] ([CursoId], [Orden]);
CREATE INDEX [IX_Inscripciones_CursoId] ON [Inscripciones] ([CursoId]);
CREATE UNIQUE INDEX [IX_Inscripciones_UsuarioId_CursoId] ON [Inscripciones] ([UsuarioId], [CursoId]);
CREATE INDEX [IX_LeccionesCompletadas_LeccionId] ON [LeccionesCompletadas] ([LeccionId]);
GO
