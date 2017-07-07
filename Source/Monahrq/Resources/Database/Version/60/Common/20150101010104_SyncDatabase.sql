IF NOT EXISTS(SELECT TOP 1 [Id] FROM [Base_Sexes] WHERE UPPER([Name]) = 'MISSING')
BEGIN
	SET IDENTITY_INSERT [Base_Sexes] ON;

	INSERT INTO [Base_Sexes] ([Id], [Name])
	VALUES (0, 'Missing');

	SET IDENTITY_INSERT [Base_Sexes] OFF;
END;

-- Insert missing target scope values
INSERT	INTO [dbo].[Wings_ScopeValues] ([Name],[Description],[Value],[Scope_Id])
SELECT	'Missing', 'Missing', 0, [ID]
FROM	[Wings_Scopes] WHERE UPPER([Name]) = 'SEX';

-- update wings dataset data if null then should be missing.
UPDATE [Targets_InpatientTargets]
SET [Sex] = 0
WHERE [Sex] = null;

UPDATE [Targets_TreatAndReleaseTargets]
SET [Sex] = 0
WHERE [Sex] = null;