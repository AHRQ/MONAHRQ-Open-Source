IF  NOT EXISTS (SELECT * FROM sys.objects  WHERE object_id = OBJECT_ID(N'[dbo].[SchemaVersions]') AND type in (N'U'))
	BEGIN
		-- PRINT 'Inside creation';
		CREATE TABLE [dbo].[SchemaVersions](
			[Id] [int] IDENTITY(1,1) NOT NULL,
			[Name] [nvarchar](255) NOT NULL,
			[Version] [nvarchar](50) NOT NULL,
			[ActiveDate] [datetime] NOT NULL DEFAULT(GETDATE()),
			[Month] [int] NULL,
			[Year] [int] NULL,
			[VersionType] [int] NULL,
			[FileName] [nvarchar](255) NULL,
			[Major] [int] NULL,
			[Minor] [int] NULL,
			[Milestone] [int] NULL
		) ON [PRIMARY];

		INSERT INTO [dbo].[SchemaVersions]([Name],[Version],[ActiveDate],[Month],[Year],[VersionType],[FileName],[Major],[Minor],[Milestone]) 
		VALUES ('{0}', '{1}', GETDATE(), NULL, NULL, NULL, NULL, NULL, NULL, NULL);
	END
ELSE
	BEGIN
		-- PRINT 'Inside update';
		IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaVersions]') AND [name] = 'Name')
		BEGIN
			ALTER TABLE [dbo].[SchemaVersions] 
			ADD [Name] [nvarchar](255) NULL;
		END;

		IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaVersions]') AND [name] = 'Month')
		BEGIN
			ALTER TABLE [dbo].[SchemaVersions] 
			ADD [Month] INT NULL;
		END;

		IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaVersions]') AND [name] = 'Year')
		BEGIN
			ALTER TABLE [dbo].[SchemaVersions] 
			ADD [Year] INT NULL;
		END;

		IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaVersions]') AND [name] = 'VersionType')
		BEGIN
			ALTER TABLE [dbo].[SchemaVersions] 
			ADD [VersionType] INT NULL;
		END;

		IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaVersions]') AND [name] = 'FileName')
		BEGIN
			ALTER TABLE [dbo].[SchemaVersions] 
			ADD [FileName] [nvarchar](255) NULL;
		END;

		IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaVersions]') AND [name] = 'Major')
		BEGIN
			ALTER TABLE [dbo].[SchemaVersions] 
			ADD [Major] INT NULL;
		END;

		IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaVersions]') AND [name] = 'Minor')
		BEGIN
			ALTER TABLE [dbo].[SchemaVersions] 
			ADD [Minor] INT NULL;
		END;

		IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SchemaVersions]') AND [name] = 'Milestone')
		BEGIN
			ALTER TABLE [dbo].[SchemaVersions] 
			ADD [Milestone] INT NULL;
		END;
	END;