DROP INDEX [IDX_Wings_Datasets] ON [dbo].[Wings_Datasets]

ALTER TABLE [Wings_Datasets] ALTER COLUMN [ProviderStates] nvarchar(200) NULL

CREATE NONCLUSTERED INDEX [IDX_Wings_Datasets] ON [dbo].[Wings_Datasets]
(
	[DateImported] ASC,
	[ProviderStates] ASC,
	[ProviderUseRealtime] ASC,
	[DRGMDCMappingStatus] ASC,
	[IsFinished] ASC
)
