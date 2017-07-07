

--  Delete table if needed.
if(Object_id(N'Temp_CG_Child_Working_03')) is not null 
	drop table [Temp_CG_Child_Working_03]
go

/****** Object:  Table [dbo].[Temp_CG_Child_Working_03]    Script Date: 5/4/2016 11:52:09 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Temp_CG_Child_Working_03](
	[CGPracticeId] [varchar](50) NULL,
	[Child_Samp] [varchar](50) NULL,
	[CD_COMP_01] [decimal](38, 23) NULL,
	[CD_COMP_02] [decimal](38, 24) NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

