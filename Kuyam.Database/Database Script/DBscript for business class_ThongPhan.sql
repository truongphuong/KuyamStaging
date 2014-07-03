GO
/****** Object:  Table [dbo].[InstructorClassScheduler]    Script Date: 06/25/2014 16:45:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[InstructorClassScheduler](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[InstructorClassID] [int] NOT NULL,
	[FromHour] [time](0) NOT NULL,
	[ToHour] [time](0) NOT NULL,
	[DayOfWeek] [int] NOT NULL,
	[IsPreview] [bit] NOT NULL,
 CONSTRAINT [PK_InstructorClassScheduler] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[InstructorClassScheduler]  WITH CHECK ADD  CONSTRAINT [FK_InstructorClassScheduler_EmployeeService] FOREIGN KEY([InstructorClassID])
REFERENCES [dbo].[EmployeeService] ([ID])
GO

ALTER TABLE [dbo].[InstructorClassScheduler] CHECK CONSTRAINT [FK_InstructorClassScheduler_EmployeeService]
GO


ALTER TABLE CompanyEmployee
ADD EmployeeTypeId int

 ALTER TABLE [ServiceCompany]
 ADD [ServiceTypeId] int NOT NULL DEFAULT(0)
 
 ALTER TABLE [ProfileCompany]
 ADD [IsClass] [bit]
