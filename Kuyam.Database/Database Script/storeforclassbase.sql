SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[GetProfileCompaniesWebSite] 
	 @ServiceID INT = NULL,
	 @FromPrice DECIMAL(18,0) = NULL,
	 @ToPrice DECIMAL(18,0) = NULL,
	 @FromDate DateTime = NULL,
	 @ToDate DateTime = NULL,
	 @IsToday BIT = NULL,
	 @KeySearch NVARCHAR(100),
	 @CurrentLat FLOAT = NULL,
	 @CurrentLong FLOAT = NULL,
	 @Distance FLOAT = NULL,
	 @CustID INT = NULL,
	 @Skip INT = NULL,
	 @Take INT = NULL,
	 @TotalItems INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	IF(@CustID IS NULL) SET @CustID = 0
	IF(@Skip IS NULL) SET @Skip = 0
	IF(@Take IS NULL) SET @Take = 10
	
	IF(@Distance IS NULL) SET @Distance = 0.0
	
	IF(@KeySearch IS NOT NULL)
	BEGIN
		SET @KeySearch = '%' + LOWER(@KeySearch) + '%'
	END
	
	DECLARE @LimitTime Time
	SET @LimitTime = '00:00:10'
	
	DECLARE @FromHour INT
	DECLARE @ToHour INT
	DECLARE @DateOfWeek INT
	SET @DateOfWeek = (select datepart(dw,getdate()) - 1)
	
	DECLARE @NeedSearchHour BIT
	SET @NeedSearchHour = 1
	
	DECLARE @DateTimeUTCNow DATETIME
	SET @DateTimeUTCNow = GETUTCDATE()
	
	DECLARE @DateUTCNow DATETIME
	SET @DateUTCNow = CONVERT(DATE, @DateTimeUTCNow)
	
	IF(@FromDate IS NULL OR @ToDate IS NULL) SET @NeedSearchHour = 0
	IF(CONVERT(DATE, @FromDate) = @DateUTCNow AND CONVERT(DATE, @ToDate) = @DateUTCNow) SET @NeedSearchHour = 0
	
	IF(@NeedSearchHour = 1)
	BEGIN
		SELECT @FromHour = DATEPART(hh, @FromDate)
		SELECT @ToHour = DATEPART(hh, @ToDate)
	END
	
	SET @DateUTCNow = @DateUTCNow + @LimitTime
	
	IF(@ServiceID IS NULL) SET @ServiceID = 0
	IF(@IsToday IS NULL) SET @IsToday = 0
	IF(@FromPrice IS NULL) SET @FromPrice = 0
	IF(@ToPrice IS NULL) SET @ToPrice = 0
	
	IF OBJECT_ID('tempdb..#TempProfileCompanies') IS NOT NULL DROP TABLE #TempProfileCompanies
	CREATE TABLE #TempProfileCompanies (RowID INT, ProfileID INT, IsFeature BIT, Distance FLOAT)
	INSERT INTO #TempProfileCompanies(RowID , ProfileID, IsFeature, Distance)
	SELECT DISTINCT 0 AS RowID, pc.ProfileID, 
		CASE WHEN EXISTS(SELECT 1 FROM FeaturedCompany fc WHERE fc.ProfileID = pc.ProfileID) THEN 1 ELSE 0 END AS IsFeature,
		(ROUND(dbo.fnGetDistance(@CurrentLat, @CurrentLong, pc.Latitude, pc.Longitude, 'Kilometers'), 1)) AS Distance
	FROM ProfileCompany pc
		LEFT OUTER JOIN ServiceCompany sc ON pc.ProfileID = sc.ProfileID
	WHERE pc.CompanyStatusID = 7
		AND (@ServiceID = 0 OR sc.ServiceID = @ServiceID)
		AND (@ToPrice = 0 OR EXISTS(SELECT 1 FROM [Service] s WHERE s.ServiceID = sc.ServiceID AND sc.Price BETWEEN @FromPrice AND @ToPrice))
		AND (@NeedSearchHour = 0 OR EXISTS(SELECT 1 FROM EmployeeService es
											JOIN CompanyEmployee ce ON ce.EmployeeID = es.CompanyEmployeeID
											JOIN EmployeeHour eh ON eh.CompanyEmployeeID = ce.EmployeeID
											WHERE es.ServiceCompanyID = sc.ServiceCompanyID
												AND ( 
														(cast(cast(eh.[FromHour] AS varchar(2)) AS INT) >= @FromHour AND cast(cast(eh.[FromHour] AS varchar(2)) AS INT) < @ToHour )
													OR  (cast(cast(eh.[ToHour] AS varchar(2)) AS INT) > @FromHour AND cast(cast(eh.[ToHour] AS varchar(2)) AS INT) <= @ToHour )
													OR  (cast(cast(eh.[FromHour] AS varchar(2)) AS INT) <= @FromHour AND cast(cast(eh.[ToHour] AS varchar(2)) AS INT) >= @ToHour )
												)
											))
		AND (@IsToday = 0 OR EXISTS(SELECT 1 FROM EmployeeService es
											JOIN CompanyEmployee ce ON ce.EmployeeID = es.CompanyEmployeeID
											JOIN EmployeeHour eh ON eh.CompanyEmployeeID = ce.EmployeeID
											WHERE es.ServiceCompanyID = sc.ServiceCompanyID
												AND eh.IsPreview = 0 AND eh.[DayOfWeek] = @DateOfWeek
												AND eh.ToHour + @DateUTCNow > @DateTimeUTCNow
											))
		AND (@KeySearch IS NULL OR (pc.Name IS NOT NULL AND LOWER(pc.Name) LIKE @KeySearch))
		AND (@Distance = 0.0 OR (ROUND(dbo.fnGetDistance(@CurrentLat, @CurrentLong, pc.Latitude, pc.Longitude, 'Kilometers'), 1)) <= @Distance)
		
	SET @TotalItems = (SELECT COUNT(*) FROM #TempProfileCompanies)
		
	IF OBJECT_ID('tempdb..#TempProfileCompaniesRowID') IS NOT NULL DROP TABLE #TempProfileCompaniesRowID
	CREATE TABLE #TempProfileCompaniesRowID (RowID INT, ProfileID INT, IsFeature BIT, Distance FLOAT)
	CREATE CLUSTERED INDEX TempProfileCompanieIndex ON #TempProfileCompanies(ProfileID)
	INSERT INTO #TempProfileCompaniesRowID(RowID, ProfileID, IsFeature, Distance)
	SELECT (ROW_NUMBER() OVER (Order By IsFeature DESC, Distance)) RowID, ProfileID, IsFeature, Distance
	FROM #TempProfileCompanies
	
	DELETE #TempProfileCompanies
	
	SET @Skip = @Skip + 1
	SET @Take = @Skip + @Take - 1
	
	INSERT INTO #TempProfileCompanies(RowID , ProfileID, IsFeature, Distance)
	SELECT RowID, ProfileID, IsFeature, Distance
	FROM #TempProfileCompaniesRowID
	WHERE RowID BETWEEN @Skip AND @Take
	
	IF OBJECT_ID('tempdb..#EmployeeHours') IS NOT NULL DROP TABLE #EmployeeHours
	CREATE TABLE #EmployeeHours (ProfileID INT, ID INT, FromHourString Time, ToHourString Time, [DayOfWeek] INT, IsDayly BIT, EmployeeID INT, EmployeeName NVARCHAR(500))
	INSERT INTO #EmployeeHours(ProfileID, ID, FromHourString, ToHourString, [DayOfWeek], IsDayly, EmployeeID, EmployeeName)
	SELECT DISTINCT pc.ProfileID,
	eh.ID,
	eh.FromHour FromHourString,
	eh.ToHour ToHourString,
	eh.[DayOfWeek],
	eh.IsPreview,
	ce.EmployeeID,
	ce.EmployeeName
	FROM #TempProfileCompanies pc JOIN CompanyEmployee ce ON pc.ProfileID = ce.ProfileCompanyID
	JOIN EmployeeHour eh ON eh.CompanyEmployeeID = ce.EmployeeID
	WHERE EXISTS(SELECT top 1 es.CompanyEmployeeID FROM EmployeeService es WHERE es.CompanyEmployeeID = ce.EmployeeID)
		
	IF OBJECT_ID('tempdb..#InstructorClassSchedulers') IS NOT NULL DROP TABLE #InstructorClassSchedulers
	CREATE TABLE #InstructorClassSchedulers (ProfileID INT, ID INT, FromHourString Time, ToHourString Time, [DayOfWeek] INT, IsDayly BIT, EmployeeID INT, EmployeeName NVARCHAR(500))
	INSERT INTO #InstructorClassSchedulers(ProfileID, ID, FromHourString, ToHourString, [DayOfWeek], IsDayly, EmployeeID, EmployeeName)
	SELECT DISTINCT pc.ProfileID,
	ic.ServiceCompanyID ID,
	ics.FromHour FromHourString,
	ics.ToHour ToHourString,
	ics.[DayOfWeek],
	ics.IsPreview,
	ce.EmployeeID,
	ce.EmployeeName
	FROM #TempProfileCompanies pc 
	JOIN CompanyEmployee ce ON pc.ProfileID = ce.ProfileCompanyID
	JOIN EmployeeService  ic on ce.EmployeeID = ic.CompanyEmployeeID
	JOIN InstructorClassScheduler ics on ic.ID = ics.InstructorClassID	
	WHERE EXISTS(SELECT top 1 es.CompanyEmployeeID FROM EmployeeService es WHERE es.CompanyEmployeeID = ce.EmployeeID)
	
	SELECT pc.*, t.IsFeature IsFeature, t.Distance Distance,
		CASE WHEN @CustID <> 0 AND EXISTS(SELECT 1 FROM   favorites f WHERE  f.profileid = t.profileid 
                                     AND f.custid = @CustID) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsUserFavorite,
		(
				SELECT(
					SELECT ch.CompanyHourID,
						ch.FromHour FromHourString,
						ch.ToHour ToHourString,
						ch.[DayOfWeek],
						ch.IsDaily
					FROM CompanyHour AS ch
					WHERE ch.ProfileCompanyID = t.ProfileID
					FOR XML PATH('CompanyHour'), type)
					FOR XML PATH(''),
					ROOT('CompanyHours')
				) AS CompanyHoursStr,
		(
			SELECT(
				SELECT * FROM #EmployeeHours eh WHERE eh.ProfileID = t.ProfileID
				FOR XML PATH('EmployeeHour'), type)
				FOR XML PATH(''),
				ROOT('EmployeeHours')
			) AS EmployeeHoursStr,
			(
			SELECT(
				SELECT * FROM #InstructorClassSchedulers eh WHERE eh.ProfileID = t.ProfileID
				FOR XML PATH('EmployeeHour'), type)
				FOR XML PATH(''),
				ROOT('EmployeeHours')
			) AS InstructorClassSchedulerStr,
			Stuff((SELECT ',' + s.servicename 
                       FROM   [Service] s 
                       WHERE  EXISTS (SELECT 1 
                                      FROM   servicecompany subsc 
                                      WHERE  subsc.profileid = t.profileid 
                                             AND s.serviceid = subsc.serviceid
                                             AND s.ParentServiceID is null) 
                       ORDER  BY s.servicename 
                       FOR xml path('')), 1, 1, '') 
                ListServices,
       CASE WHEN EXISTS (SELECT 1 FROM CompanyMedia cm WHERE cm.ProfileID = t.ProfileID AND cm.IsVideo IS NOT NULL AND cm.IsVideo = 1)
		THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsVideo,
       (SELECT CASE 
                          WHEN Avg(r.ratingvalue) IS NULL THEN 0
                          ELSE Avg(r.ratingvalue) 
                        END 
                 FROM   rating r 
                 WHERE  EXISTS(SELECT 1 
                               FROM   servicecompany subsc2 
                               WHERE  subsc2.profileid = t.profileid 
                                      AND subsc2.servicecompanyid = 
                                          r.servicecompanyid)) Rate,
        (SELECT CASE 
                          WHEN COUNT(*) IS NULL THEN 0
                          ELSE COUNT(*) 
                        END 
                 FROM   rating r 
                 WHERE  EXISTS(SELECT 1 
                               FROM   servicecompany subsc2 
                               WHERE  subsc2.profileid = t.profileid 
                                      AND subsc2.servicecompanyid = 
                                          r.servicecompanyid)) TotalReviews,
        (
				SELECT(
					SELECT DISTINCT e.EventID,
						e.Name,
						e.StartDate,
						e.EndDate,
						cevent.CompanyEventID
					FROM CompanyEvent AS cevent
					JOIN [Event] e ON cevent.EventID = e.EventID
					JOIN [CompanyServiceEvent] cse on  cevent.CompanyEventID = cse.CompanyEventID
					WHERE cevent.ProfileCompanyID = t.ProfileID
					FOR XML PATH('Event'), type)
					FOR XML PATH(''),
					ROOT('Events')
				) AS CompanyEventsStr
	FROM #TempProfileCompanies t JOIN ProfileCompany pc ON t.ProfileID = pc.ProfileID
	ORDER BY t.RowID
	
END

GO

ALTER PROCEDURE [dbo].[GetProfileCompanies] 
	 @MinLat FLOAT = NULL,
	 @MaxLat FLOAT = NULL,
	 @MinLong FLOAT = NULL,
	 @MaxLong FLOAT = NULL,
	 @KeySearch NVARCHAR(100) = NULL,
	 @CategoryID INT = NULL,
	 @CustID INT = NULL,
	 @CurrentLat FLOAT = NULL,
	 @CurrentLong FLOAT = NULL,
	 @Skip INT = NULL,
	 @Take INT = NULL,
	 @TotalItems INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @TrueVal BIT = 1
	DECLARE @FalseVal BIT = 0
	
	IF(@Skip IS NULL) SET @Skip = 0
	IF(@Take IS NULL) SET @Take = 10
	
	DECLARE @TempSQLString AS NVARCHAR(MAX) = ''
	DECLARE @FilterString AS NVARCHAR(MAX)
	DECLARE @OrderBySQLString AS NVARCHAR(MAX)
	DECLARE @IsFeatureSQLString NVARCHAR(500) = '(CASE WHEN EXISTS(SELECT 1 FROM FeaturedCompany fc WHERE fc.ProfileID = pc.ProfileID) THEN @TrueVal ELSE @FalseVal END)'
	DECLARE @DistanceSQLString NVARCHAR(500) = '(ROUND(dbo.fnGetDistance(@CurrentLat, @CurrentLong, pc.Latitude, pc.Longitude, ''Kilometers''), 1))'
	
	SET @FilterString = ' WHERE pc.CompanyStatusID = 7'
	IF(@MinLat IS NOT NULL) SET @FilterString = @FilterString + ' AND pc.Latitude >= @MinLat'
	IF(@MaxLat IS NOT NULL) SET @FilterString = @FilterString + ' AND pc.Latitude <= @MaxLat'
	IF(@MinLong IS NOT NULL) SET @FilterString = @FilterString + ' AND pc.Longitude >= @MinLong'
	IF(@MaxLong IS NOT NULL) SET @FilterString = @FilterString + ' AND pc.Longitude <= @MaxLong'
	IF(@KeySearch IS NOT NULL)
	BEGIN
		SET @KeySearch = '%' + LOWER(@KeySearch) + '%'
		SET @FilterString = @FilterString + ' AND ('
		SET @FilterString = @FilterString + ' EXISTS (SELECT 1 FROM Service s JOIN ServiceCompany sc ON s.ServiceID = sc.ServiceID AND sc.ProfileID = pc.ProfileID AND s.ServiceName IS NOT NULL Where LOWER(s.ServiceName) LIKE @KeySearch)'
		SET @FilterString = @FilterString + ' OR (pc.Name IS NOT NULL AND LOWER(pc.Name) LIKE @KeySearch)'
		SET @FilterString = @FilterString + ')'
	END
	IF(@CategoryID IS NOT NULL AND @CategoryID <> 0) SET @FilterString = @FilterString + ' AND EXISTS (SELECT 1 FROM ServiceCompany sc2 WHERE sc2.ServiceID = @CategoryID AND sc2.ProfileID = pc.ProfileID)'	
		
	SET @TempSQLString = 'SELECT Distinct 0 RowID,pc.ProfileID'
	SET @TempSQLString = @TempSQLString + ',' + @IsFeatureSQLString + ' AS IsFeature'
	SET @TempSQLString = @TempSQLString + ',' + @DistanceSQLString + '  Distance'
	SET @TempSQLString = @TempSQLString + ' FROM ProfileCompany pc ' + @FilterString
	
	DECLARE @TempParams AS NVARCHAR(MAX) = '@MinLat FLOAT,@MaxLat FLOAT,@MinLong FLOAT,@MaxLong FLOAT,@KeySearch NVARCHAR(100),@CategoryID INT'
	SET @TempParams = @TempParams + ',@CustID INT,@CurrentLat FLOAT,@CurrentLong FLOAT'
	SET @TempParams = @TempParams + ',@TrueVal BIT,@FalseVal BIT'
	
	IF OBJECT_ID('tempdb..#TempProfileCompanies') IS NOT NULL DROP TABLE #TempProfileCompanies
	CREATE TABLE #TempProfileCompanies (RowID INT, ProfileID INT, IsFeature BIT, Distance FLOAT)
	CREATE CLUSTERED INDEX TempProfileCompanieIndex ON #TempProfileCompanies(ProfileID)
	INSERT INTO #TempProfileCompanies(RowID, ProfileID, IsFeature, Distance)
	EXECUTE sp_executesql @TempSQLString, @TempParams, @MinLat=@MinLat,@MaxLat=@MaxLat,@MinLong=@MinLong,@MaxLong=@MaxLong,@KeySearch=@KeySearch
		,@CategoryID=@CategoryID,@CustID=@CustID,@CurrentLat=@CurrentLat,@CurrentLong=@CurrentLong,@TrueVal=@TrueVal,@FalseVal=@FalseVal	
	
	SET @TotalItems = (SELECT COUNT(*) FROM #TempProfileCompanies)
	
	SET @Skip = @Skip + 1
	SET @Take = @Skip + @Take - 1
	
	IF OBJECT_ID('tempdb..#Result') IS NOT NULL DROP TABLE #Result
	CREATE TABLE #Result (RowID INT, ProfileID INT, IsFeature BIT, Distance FLOAT)
	CREATE CLUSTERED INDEX ResultIndex ON #Result(ProfileID)
	INSERT INTO #Result(RowID, ProfileID, IsFeature, Distance)
	SELECT (ROW_NUMBER() OVER (Order By IsFeature, Distance)) RowID, ProfileID, IsFeature, Distance
	FROM #TempProfileCompanies
	
	DELETE #TempProfileCompanies
	INSERT INTO #TempProfileCompanies(RowID, ProfileID, IsFeature, Distance)
	SELECT RowID, ProfileID, IsFeature, Distance
	FROM #Result
	WHERE RowID BETWEEN @Skip AND @Take
	
	IF OBJECT_ID('tempdb..#EmployeeHours') IS NOT NULL DROP TABLE #EmployeeHours
	CREATE TABLE #EmployeeHours (ProfileID INT, ID INT, FromHourString Time, ToHourString Time, [DayOfWeek] INT, IsDayly BIT, EmployeeID INT, EmployeeName NVARCHAR(500))
	INSERT INTO #EmployeeHours(ProfileID, ID, FromHourString, ToHourString, [DayOfWeek], IsDayly, EmployeeID, EmployeeName)
	SELECT DISTINCT pc.ProfileID,
	eh.ID,
	eh.FromHour FromHourString,
	eh.ToHour ToHourString,
	eh.[DayOfWeek],
	eh.IsPreview,
	ce.EmployeeID,
	ce.EmployeeName
	FROM #TempProfileCompanies pc JOIN CompanyEmployee ce ON pc.ProfileID = ce.ProfileCompanyID
	JOIN EmployeeHour eh ON eh.CompanyEmployeeID = ce.EmployeeID
	WHERE EXISTS(SELECT top 1 es.CompanyEmployeeID FROM EmployeeService es WHERE es.CompanyEmployeeID = ce.EmployeeID)
	
	SELECT DISTINCT pc.profileid, 
                ct.IsFeature AS IsFeature, 
                CASE 
                  WHEN EXISTS(SELECT 1 
                              FROM   favorites f 
                              WHERE  f.profileid = ct.profileid 
                                     AND f.custid = @CustID) THEN @TrueVal 
                  ELSE @FalseVal 
                END 
                AS IsUserFavorite, 
                Stuff((SELECT ',' + s.servicename 
                       FROM   [Service] s 
                       WHERE  EXISTS (SELECT 1 
                                      FROM   servicecompany subsc 
                                      WHERE  subsc.profileid = ct.profileid 
                                             AND s.serviceid = subsc.serviceid) 
                       ORDER  BY s.servicename 
                       FOR xml path('')), 1, 1, '') 
                ListServices, 
                Stuff((SELECT ',' + m.locationdata 
                       FROM   media m 
                       WHERE  EXISTS (SELECT 1 
                                      FROM   companymedia cm 
                                      WHERE  cm.profileid = ct.profileid 
                                             AND cm.mediaid = m.mediaid) 
                       FOR xml path('')), 1, 1, '') 
                LocationData, 
                (SELECT CASE 
                          WHEN Avg(r.ratingvalue) IS NULL THEN 0.0 
                          ELSE Avg(r.ratingvalue) 
                        END 
                 FROM   rating r 
                 WHERE  EXISTS(SELECT 1 
                               FROM   servicecompany subsc2 
                               WHERE  subsc2.profileid = ct.profileid 
                                      AND subsc2.servicecompanyid = 
                                          r.servicecompanyid)) Rate, 
                pc.latitude, 
                pc.longitude,
                ct.Distance AS Distance, 
                pc.isshowcatagory, 
                pc.companytypeid, 
                pc.companystatusid, 
                pc.name, 
                pc.[Desc], 
                pc.contactname, 
                pc.contacttitle, 
                pc.street1, 
                pc.street2, 
                pc.city, 
                pc.state, 
                pc.zip, 
                pc.phone, 
                pc.fax, 
                pc.email, 
                pc.url, 
                pc.youtubelink, 
                pc.preferredcontact, 
                pc.paymentoptions, 
                pc.paymentmethod, 
                pc.payafter, 
                pc.mapurl, 
                pc.publictransportation, 
                pc.notes, 
                pc.apptautoconfirm, 
                pc.apptdefaultslotduration, 
                pc.apptdefaultpeopleperslot, 
                pc.created, 
                pc.modified, 
                pc.expireddate, 
                pc.contactfirstname, 
                pc.contactlastname, 
                pc.cancelpolicy, 
                pc.cancelhour, 
                pc.cancelrefundpercent, 
                pc.mobilecarrier, 
                Stuff((SELECT ',' + m.locationdata 
                       FROM   media m 
                       WHERE  EXISTS (SELECT 1 
                                      FROM   companymedia cm 
                                      WHERE  cm.profileid = ct.profileid 
                                             AND cm.mediaid = m.mediaid) 
                       FOR xml path('')), 1, 1, '') 
                ImageUrlStr,
                (
				SELECT(
					SELECT * FROM #EmployeeHours eh WHERE eh.ProfileID = ct.ProfileID
					FOR XML PATH('EmployeeHour'), type)
					FOR XML PATH(''),
					ROOT('EmployeeHours')
				) AS EmployeeHoursStr,
				(
				SELECT(
					SELECT ch.CompanyHourID,
						ch.FromHour FromHourString,
						ch.ToHour ToHourString,
						ch.[DayOfWeek],
						ch.IsDaily
					FROM CompanyHour AS ch
					WHERE ch.ProfileCompanyID = ct.ProfileID
					FOR XML PATH('CompanyHour'), type)
					FOR XML PATH(''),
					ROOT('CompanyHours')
				) AS CompanyHoursStr
	FROM #TempProfileCompanies ct JOIN ProfileCompany pc ON ct.ProfileID = pc.ProfileID
	
END


GO

alter PROCEDURE [dbo].[GetDataCheckOutOfClass]
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
	
END



GO
SET QUOTED_IDENTIFIER ON
GO

alter PROCEDURE [dbo].[GetDataCheckOutForServiceIdAndEmployeeId]
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


GO
SET QUOTED_IDENTIFIER ON
GO

alter PROCEDURE [dbo].[GetSchedulerAvailabilityOfClass]
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
svc.ProfileID =@ProfileId
and (svc.FromDateTime <@FromDateTime and  svc.ToDateTime>=@ToDateTime 
    OR (svc.FromDateTime <=@FromDateTime and @FromDateTime < svc.ToDateTime)
    OR (svc.FromDateTime < @ToDateTime  and @ToDateTime   <= svc.ToDateTime)
    )
and svc.ServiceTypeId=1 
	
END

