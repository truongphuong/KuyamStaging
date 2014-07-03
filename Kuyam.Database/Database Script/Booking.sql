
/****** Object:  StoredProcedure [dbo].[GetDataCheckOutForServiceIdAndEmployeeId]    Script Date: 06/27/2014 10:02:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [dbo].[GetDataCheckOutForServiceIdAndEmployeeId]
(
	@ServiceId int = 0,
	@EmployeeId int =0 
)
AS 
BEGIN
select
svc.ServiceCompanyID,
svc.ProfileID,
svc.ServiceID,
svc.FromDateTime,
svc.ToDateTime,
svc.Price,
svc.Duration,
svc.AttendeesNumber,
svc.[Description],
sv.ServiceName,
svc.IsPerDay,
emp.EmployeeID,
emp.EmployeeName,
emp.Email,
emp.Phone,
pf.Name,
pf.Street1,
pf.Street2,
pf.City,
pf.[State],
pf.Zip,
pf.PaymentMethod 
From ProfileCompany pf 
inner join ServiceCompany svc on pf.ProfileID=svc.ProfileID
inner join EmployeeService epsv on svc.ServiceCompanyID =epsv.ServiceCompanyID
inner join dbo.CompanyEmployee emp on epsv.CompanyEmployeeID = emp.EmployeeID
inner join dbo.[Service] sv on svc.ServiceID =sv.ServiceID
where
svc.ServiceCompanyID=@ServiceId
and emp.EmployeeID=@EmployeeId
and svc.ServiceTypeId=0 
and emp.EmployeeTypeId is null		
	
END

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [dbo].[GetSchedulerAvailabilityOfClass]
(
	@ProfileId int = 0,
	@FromDateTime varchar(10),
	@ToDateTime varchar(10),
	@Today int =0			
)
AS 
BEGIN
select
svc.ServiceCompanyID,
svc.ProfileID,
svc.ServiceID,
inscs.ID as ClassSchedulerID,
svc.FromDateTime,
svc.ToDateTime,
svc.Price,
svc.Duration,
svc.AttendeesNumber,
svc.[Description],
svc.ServiceName,
CASE  WHEN (select COUNT(1) From dbo.Appointment tmp where tmp.ClassSchedulerID=inscs.ID) <svc.AttendeesNumber
THEN CAST(1 as bit) ELSE CAST(0 as bit) END as IsAvailability ,
svc.IsPerDay,
inscs.[DayOfWeek],
inscs.FromHour,
inscs.ToHour,
emp.EmployeeID,
emp.EmployeeName,
emp.Email,
emp.Phone 
From ServiceCompany svc
inner join dbo.EmployeeService insc on svc.ServiceCompanyID = insc.ServiceCompanyID
inner join InstructorClassScheduler  inscs on insc.ID=inscs.InstructorClassID
inner join CompanyEmployee emp on insc.CompanyEmployeeID = emp.EmployeeID
where  
svc.ProfileID =5140 
and (svc.FromDateTime <@FromDateTime and  svc.ToDateTime>@ToDateTime 
    OR (svc.FromDateTime<=@FromDateTime and svc.ToDateTime <=@FromDateTime)
    OR (svc.FromDateTime <= @ToDateTime and svc.ToDateTime>=@ToDateTime)
    )
and svc.ServiceTypeId=1 
and emp.EmployeeTypeId is null		
	
END

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [dbo].[GetDataCheckOutOfClass]
(
	@ClassSchedulerID int = 0	
)
AS 
BEGIN
select
svc.ServiceCompanyID,
svc.ProfileID,
svc.ServiceID,
inscs.ID as ClassSchedulerID,
svc.FromDateTime,
svc.ToDateTime,
svc.Price,
svc.Duration,
svc.AttendeesNumber,
svc.[Description],
svc.ServiceName,
svc.IsPerDay,
inscs.[DayOfWeek],
inscs.FromHour,
inscs.ToHour,
emp.EmployeeID,
emp.EmployeeName,
emp.Email,
emp.Phone,
pf.Name,
pf.Street1,
pf.Street2,
pf.City,
pf.[State],
pf.Zip,
pf.PaymentMethod 
From ProfileCompany pf 
inner join ServiceCompany svc on pf.ProfileID=svc.ProfileID
inner join dbo.EmployeeService insc on svc.ServiceCompanyID = insc.ServiceCompanyID
inner join InstructorClassScheduler  inscs on insc.ID=inscs.InstructorClassID
inner join CompanyEmployee emp on insc.CompanyEmployeeID = emp.EmployeeID
where  
inscs.ID= @ClassSchedulerID
and svc.ServiceTypeId=1 
and emp.EmployeeTypeId is null		
	
END
