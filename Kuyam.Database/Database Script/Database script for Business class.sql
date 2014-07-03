USE [Kuyam]
GO

ALTER TABLE dbo.ProfileCompany
ADD IsClass bit

ALTER TABLE dbo.CompanyEmployee
ADD EmployeeTypeId int

GO

/****** Object:  Table [dbo].[EmployeeHour]    Script Date: 05/20/2014 17:19:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

GO

/****** Object:  Table [dbo].[EmployeeHour]    Script Date: 05/20/2014 17:19:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ClassScheduler](
	[ClassSchedulerID] [int] IDENTITY(1,1) NOT NULL,
	[ServiceCompanyID] [int] NOT NULL,
	[CompanyEmployeeID] [int] NOT NULL,
	[FromHour] [time](0) NOT NULL,
	[ToHour] [time](0) NOT NULL,	
	[DayOfWeek] [int] NOT NULL,
	[IsPreview] [bit] NOT NULL,
	[LastedUpdate] [datetime] NOT NULL,
 CONSTRAINT [PK_ClassScheduler] PRIMARY KEY CLUSTERED 
(
	[ClassSchedulerID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ClassScheduler]  WITH CHECK ADD  CONSTRAINT [FK_ClassScheduler_ServiceCompany] FOREIGN KEY([ServiceCompanyID])
REFERENCES [dbo].[ServiceCompany] ([ServiceCompanyID])
GO

ALTER TABLE [dbo].[ClassScheduler]  WITH CHECK ADD  CONSTRAINT [FK_ClassScheduler_CompanyEmployee] FOREIGN KEY([CompanyEmployeeID])
REFERENCES [dbo].[CompanyEmployee] ([EmployeeID])
GO
