IF OBJECT_ID('[NursingHomeMeasureCodeMapping]') IS NOT NULL
    DROP TABLE NursingHomeMeasureCodeMapping
		  
/****** Object:  Table [dbo].[NursingHomeMeasureCodeMapping]    Script Date: 1/14/2015 3:34:45 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[NursingHomeMeasureCodeMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AccessCode] [nvarchar](50) NOT NULL,
	[MonahrqCode] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_NursingHomeMeasureCodeMapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


SET IDENTITY_INSERT [dbo].[NursingHomeMeasureCodeMapping] ON 


INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (1, N'QM401', N'NH-QM-LS-10')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (2, N'QM402', N'NH-QM-LS-02')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (4, N'QM403', N'NH-QM-LS-03')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (5, N'QM404', N'NH-QM-LS-11')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (6, N'QM405', N'NH-QM-LS-07')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (7, N'QM406', N'NH-QM-LS-08')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (8, N'QM407', N'NH-QM-LS-06')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (9, N'QM408', N'NH-QM-LS-12')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (10, N'QM409', N'NH-QM-LS-09')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (11, N'QM410', N'NH-QM-LS-01')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (12, N'QM411', N'NH-QM-LS-04')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (13, N'QM415', N'NH-QM-LS-05')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (14, N'QM419', N'NH-QM-LS-13')


INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (15, N'QM451', N'NH-QM-LS-14')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (16, N'QM452', N'NH-QM-LS-15')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (17, N'QM424', N'NH-QM-SS-01')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (18, N'QM425', N'NH-QM-SS-02')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (19, N'QM426', N'NH-QM-SS-03')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (20, N'QM430', N'NH-QM-SS-04')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (21, N'QM434', N'NH-QM-SS-05')


INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (22, N'QM471', N'NH-QM-SS-06')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (23, N'QM521', N'NH-QM-SS-07')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (24, N'QM522', N'NH-QM-SS-08')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (25, N'QM523', N'NH-QM-SS-09')



INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (26, N'ReportedCNAStaffingHoursperResidentperDay', N'NH-SD-02')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (27, N'ReportedLPNStaffingHoursperResidentperDay', N'NH-SD-03')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (28, N'ReportedRNStaffingHoursperResidentperDay', N'NH-SD-04')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (29, N'LicensedStaffingHoursperResidentperDay', N'NH-SD-05')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (30, N'TotalNurseStaffingHoursperResidentperDay', N'NH-SD-06')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (31, N'PhysicalTherapistStaffingHoursperResidentPerDay', N'NH-SD-07')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (32, N'HealthSurveyDate', N'NH-HI-02')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (33, N'FireSafetySurveyDate', N'NH-HI-03')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (34, N'TotalHealthDeficiencies', N'NH-HI-04')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (35, N'TotalFireSafetyDeficiencies', N'NH-HI-05')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (36, N'OverallRating', N'NH-OA-01')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (37, N'qualityrating', N'NH-QM-01')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (38, N'staffingrating', N'NH-SD-01')

INSERT [dbo].[NursingHomeMeasureCodeMapping] ([Id], [AccessCode], [MonahrqCode]) VALUES (39, N'surveyrating', N'NH-HI-01')




SET IDENTITY_INSERT [dbo].[NursingHomeMeasureCodeMapping] OFF

