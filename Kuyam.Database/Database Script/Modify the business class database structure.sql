USE [Kuyam]
GO

-- drop table 
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassScheduler_CompanyEmployee]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassScheduler]'))
ALTER TABLE [dbo].[ClassScheduler] DROP CONSTRAINT [FK_ClassScheduler_CompanyEmployee]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassScheduler_ServiceCompany]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassScheduler]'))
ALTER TABLE [dbo].[ClassScheduler] DROP CONSTRAINT [FK_ClassScheduler_ServiceCompany]
GO

USE [Kuyam]
GO

/****** Object:  Table [dbo].[ClassScheduler]    Script Date: 06/11/2014 10:54:20 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClassScheduler]') AND type in (N'U'))
DROP TABLE [dbo].[ClassScheduler]
GO


-- 

/****** Object:  Table [dbo].[ClassScheduler]    Script Date: 06/11/2014 10:54:11 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ClassScheduler](
	[ClassSchedulerID] [int] IDENTITY(1,1) NOT NULL,
	[ServiceCompanyID] [int] NOT NULL,	
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

ALTER TABLE [dbo].[ClassScheduler] CHECK CONSTRAINT [FK_ClassScheduler_ServiceCompany]
GO


CREATE TABLE [dbo].[ClassInstructorScheduler](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ClassSchedulerID] [int] NOT NULL,
	[EmployeeID] [int] NOT NULL
	
 CONSTRAINT [PK_ClassInstructorScheduler] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ClassInstructorScheduler]  WITH CHECK ADD  CONSTRAINT [FK_ClassInstructorScheduler_CompanyEmployee] FOREIGN KEY([EmployeeID])
REFERENCES [dbo].[CompanyEmployee] ([EmployeeID])
GO

ALTER TABLE [dbo].[ClassInstructorScheduler] CHECK CONSTRAINT [FK_ClassInstructorScheduler_CompanyEmployee]
GO

ALTER TABLE [dbo].[ClassInstructorScheduler]  WITH CHECK ADD  CONSTRAINT [FK_ClassInstructorScheduler_ClassScheduler] FOREIGN KEY([ClassSchedulerID])
REFERENCES [dbo].[ClassScheduler] ([ClassSchedulerID])
GO

ALTER TABLE [dbo].[ClassInstructorScheduler] CHECK CONSTRAINT [FK_ClassInstructorScheduler_ClassScheduler]
GO


ALTER TABLE dbo.ServiceCompany
ADD ServiceTypeId int not null Default 0
