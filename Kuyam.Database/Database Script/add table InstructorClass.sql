USE [Kuyam]
GO

/****** Object:  Table [dbo].[EmployeeService]    Script Date: 06/20/2014 15:18:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[InstructorClass](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ServiceCompanyID] [int] NOT NULL,
	[CompanyEmployeeID] [int] NOT NULL,
 CONSTRAINT [PK_InstructorClass] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[InstructorClass]  WITH CHECK ADD  CONSTRAINT [FK_InstructorClass_CompanyEmployee] FOREIGN KEY([CompanyEmployeeID])
REFERENCES [dbo].[CompanyEmployee] ([EmployeeID])
GO

ALTER TABLE [dbo].[InstructorClass] CHECK CONSTRAINT [FK_InstructorClass_CompanyEmployee]
GO

ALTER TABLE [dbo].[InstructorClass]  WITH CHECK ADD  CONSTRAINT [FK_InstructorClass_ServiceCompany] FOREIGN KEY([ServiceCompanyID])
REFERENCES [dbo].[ServiceCompany] ([ServiceCompanyID])
GO

ALTER TABLE [dbo].[InstructorClass] CHECK CONSTRAINT [FK_InstructorClass_ServiceCompany]
GO


