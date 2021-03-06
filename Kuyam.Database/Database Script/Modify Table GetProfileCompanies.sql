USE [Kuyam]
GO
/****** Object:  StoredProcedure [dbo].[GetProfileCompanies]    Script Date: 06/30/2014 16:31:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
