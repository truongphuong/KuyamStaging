--Appointment
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Appointment_ServiceCompany]') AND parent_object_id = OBJECT_ID(N'[dbo].[Appointment]'))
ALTER TABLE [dbo].[Appointment] DROP CONSTRAINT [FK_Appointment_ServiceCompany]
GO
ALTER TABLE [dbo].[Appointment]  WITH CHECK ADD  CONSTRAINT [FK_Appointment_ServiceCompany] FOREIGN KEY([ServiceCompanyID])
REFERENCES [dbo].[ServiceCompany] ([ServiceCompanyID]) ON DELETE CASCADE

-- ProposedAppointment
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ProposedAppointment_ServiceCompany]') AND parent_object_id = OBJECT_ID(N'[dbo].[ProposedAppointment]'))
ALTER TABLE [dbo].[ProposedAppointment] DROP CONSTRAINT [FK_ProposedAppointment_ServiceCompany]
GO
ALTER TABLE [dbo].[ProposedAppointment]  WITH CHECK ADD  CONSTRAINT [FK_ProposedAppointment_ServiceCompany] FOREIGN KEY([ServiceCompanyId])
REFERENCES [dbo].[ServiceCompany] ([ServiceCompanyID]) ON DELETE CASCADE
GO

--RequestAppointment
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RequestAppointment_ServiceCompany]') AND parent_object_id = OBJECT_ID(N'[dbo].[RequestAppointment]'))
ALTER TABLE [dbo].[RequestAppointment] DROP CONSTRAINT [FK_RequestAppointment_ServiceCompany]
GO
ALTER TABLE [dbo].[RequestAppointment]  WITH CHECK ADD  CONSTRAINT [FK_RequestAppointment_ServiceCompany] FOREIGN KEY([ServiceCompanyId])
REFERENCES [dbo].[ServiceCompany] ([ServiceCompanyID])ON DELETE CASCADE
GO

--Rating
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Rating_ServiceCompany]') AND parent_object_id = OBJECT_ID(N'[dbo].[Rating]'))
ALTER TABLE [dbo].[Rating] DROP CONSTRAINT [FK_Rating_ServiceCompany]
GO
ALTER TABLE [dbo].[Rating]  WITH CHECK ADD  CONSTRAINT [FK_Rating_ServiceCompany] FOREIGN KEY([ServiceCompanyID])
REFERENCES [dbo].[ServiceCompany] ([ServiceCompanyID])ON DELETE CASCADE
GO

--CompanyPackageService
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CompanyPackageService_ServiceCompany]') AND parent_object_id = OBJECT_ID(N'[dbo].[CompanyPackageService]'))
ALTER TABLE [dbo].[CompanyPackageService] DROP CONSTRAINT [FK_CompanyPackageService_ServiceCompany]
GO
ALTER TABLE [dbo].[CompanyPackageService]  WITH CHECK ADD  CONSTRAINT [FK_CompanyPackageService_ServiceCompany] FOREIGN KEY([ServiceCompanyId])
REFERENCES [dbo].[ServiceCompany] ([ServiceCompanyID])ON DELETE CASCADE
GO

--EmployeeService
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EmployeeService_ServiceCompany]') AND parent_object_id = OBJECT_ID(N'[dbo].[EmployeeService]'))
ALTER TABLE [dbo].[EmployeeService] DROP CONSTRAINT [FK_EmployeeService_ServiceCompany]
GO
ALTER TABLE [dbo].[EmployeeService]  WITH CHECK ADD  CONSTRAINT [FK_EmployeeService_ServiceCompany] FOREIGN KEY([ServiceCompanyID])
REFERENCES [dbo].[ServiceCompany] ([ServiceCompanyID])ON DELETE CASCADE
GO

--CompanyServiceEvent
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CompanyServiceEvent_ServiceCompany]') AND parent_object_id = OBJECT_ID(N'[dbo].[CompanyServiceEvent]'))
ALTER TABLE [dbo].[CompanyServiceEvent] DROP CONSTRAINT [FK_CompanyServiceEvent_ServiceCompany]
GO
ALTER TABLE [dbo].[CompanyServiceEvent]  WITH CHECK ADD  CONSTRAINT [FK_CompanyServiceEvent_ServiceCompany] FOREIGN KEY([ServiceCompanyID])
REFERENCES [dbo].[ServiceCompany] ([ServiceCompanyID])ON DELETE CASCADE
GO

-- DiscountService
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DiscountService_ServiceCompany]') AND parent_object_id = OBJECT_ID(N'[dbo].[DiscountService]'))
ALTER TABLE [dbo].[DiscountService] DROP CONSTRAINT [FK_DiscountService_ServiceCompany]
GO
ALTER TABLE [dbo].[DiscountService]  WITH CHECK ADD  CONSTRAINT [FK_DiscountService_ServiceCompany] FOREIGN KEY([ServiceCompanyId])
REFERENCES [dbo].[ServiceCompany] ([ServiceCompanyID])ON DELETE CASCADE
GO

--ClassScheduler
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassScheduler_ServiceCompany]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassScheduler]'))
ALTER TABLE [dbo].[ClassScheduler] DROP CONSTRAINT [FK_ClassScheduler_ServiceCompany]
GO
ALTER TABLE [dbo].[ClassScheduler]  WITH CHECK ADD  CONSTRAINT [FK_ClassScheduler_ServiceCompany] FOREIGN KEY([ServiceCompanyID])
REFERENCES [dbo].[ServiceCompany] ([ServiceCompanyID])ON DELETE CASCADE
GO

--ClassInstructorScheduler
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ClassInstructorScheduler_ClassScheduler]') AND parent_object_id = OBJECT_ID(N'[dbo].[ClassInstructorScheduler]'))
ALTER TABLE [dbo].[ClassInstructorScheduler] DROP CONSTRAINT [FK_ClassInstructorScheduler_ClassScheduler]
GO
ALTER TABLE [dbo].[ClassInstructorScheduler]  WITH CHECK ADD  CONSTRAINT [FK_ClassInstructorScheduler_ClassScheduler] FOREIGN KEY([ClassSchedulerID])
REFERENCES [dbo].[ClassScheduler] ([ClassSchedulerID])
GO

alter table dbo.AppointmentTemp
add AttendeesNumber int null

alter table dbo.AppointmentTemp
add Duration int null

alter table dbo.AppointmentTemp
add HotelID int null

alter table dbo.AppointmentTemp
add ProfileId int null

alter table dbo.AppointmentTemp
add StaffID int null

alter table dbo.AppointmentTemp
add EmployeeName nvarchar(100) null

alter table dbo.AppointmentTemp
add ServiceName nvarchar(150) null

alter table dbo.ProposedAppointment
add ServiceName nvarchar(150) null

alter table dbo.NonKuyamAppointment
add ServiceName nvarchar(150) null


