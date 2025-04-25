using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class SqlArtifacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlCodeViewProductionSummary = @"
CREATE VIEW [dbo].[ProductionSummary]
AS
	SELECT *
FROM (
	-- Line with one flow
	SELECT 
		pro.Date,
		l.Name Line,
		s.Name Shift,
		p.Sku SKU,
		pro.Sku ProductId,
		p.Flavour,
		p.Packing,
		p.NetContent,
		1 Flow,
		su.Description Supervisor, 
		su.Name SupervisorName,
		SUM(pro.Production) Production,
		SUM(ISNULL(pro.ScrapUnits,0)) Scrap,
		MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
		MIN(l.Efficiency) Efficiency,
		ISNULL(SUM(d.ChangeHrs),0) ChangeHrs,
		ISNULL(SUM(d.ChangeMins),0) ChangeMins,
		COUNT(pro.Id) Hrs,
		COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs),0) NetHrs,
		(COUNT(pro.Id) - (ISNULL(SUM(d.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed) EffDEN,
		(COUNT(pro.Id) - (ISNULL(SUM(d.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed) EffDENSKU
	FROM [dbo].[Productivity] pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Shift s ON s.Id = pro.ShiftID
	INNER JOIN Product p ON p.Id = pro.ProductID
	INNER JOIN Supervisor su ON su.Id = pro.SupervisorID
	LEFT JOIN (
		SELECT
			ProductivityID,
			SUM(dr.Minutes)/CAST(60 AS Decimal) ChangeHrs,
			SUM(dr.Minutes) ChangeMins
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 1
	GROUP BY 
		pro.Date,
		l.Name,
		s.Name,
		p.Sku,
		pro.Sku,
		p.Flavour,
		p.Packing,
		p.NetContent,
		su.Name,
		su.Description

	UNION ALL

	-- Line with two flow
	-- Same SKU
	SELECT 
		pro.Date,
		l.Name Line,
		s.Name Shift,
		p.Sku SKU,
		pro.Sku ProductId,
		p.Flavour,
		p.Packing,
		p.NetContent,
		1 Flow,
		su.Description Supervisor, 
		su.Name SupervisorName,
		SUM(ISNULL(pro.Production,0) + ISNULL(pro.Production2,0)) Production,
		SUM(ISNULL(pro.ScrapUnits,0) + ISNULL(pro.ScrapUnits2,0)) Scrap,
		MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
		MIN(l.Efficiency) Efficiency,
		ISNULL(SUM(d.ChangeHrs),0) ChangeHrs,
		ISNULL(SUM(d.ChangeMins),0) ChangeMins,
		COUNT(pro.Id) Hrs,
		COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs),0) NetHrs,
		(COUNT(pro.Id) - (ISNULL(SUM(d.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed) EffDEN,
		(COUNT(pro.Id) - (ISNULL(SUM(d.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed) EffDENSKU
	FROM [dbo].[Productivity] pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Shift s ON s.Id = pro.ShiftID
	INNER JOIN Product p ON p.Id = pro.ProductID
	INNER JOIN Supervisor su ON su.Id = pro.SupervisorID
	LEFT JOIN (
		SELECT
			ProductivityID,
			SUM(dr.Minutes)/CAST(60 AS Decimal) ChangeHrs,
			SUM(dr.Minutes) ChangeMins
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 2
		AND ((pro.ProductID = pro.ProductID2) OR (pro.ProductID = 0 OR pro.ProductID2 = 0))
	GROUP BY 
		pro.Date,
		l.Name,
		s.Name,
		p.Sku,
		pro.Sku,
		p.Flavour,
		p.Packing,
		p.NetContent,
		su.Name,
		su.Description


	UNION ALL
	-- Different SKU
	-- Flow 1
	SELECT 
		pro.Date,
		l.Name Line,
		s.Name Shift,
		p.Sku SKU,
		pro.Sku ProductId,
		p.Flavour,
		p.Packing,
		p.NetContent,
		1 Flow,
		su.Description Supervisor, 
		su.Name SupervisorName,
		SUM(ISNULL(pro.Production,0)) Production,
		SUM(ISNULL(pro.ScrapUnits,0)) Scrap,
		MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
		MIN(l.Efficiency) Efficiency,
		ISNULL(SUM(d.ChangeHrs),0) ChangeHrs,
		ISNULL(SUM(d.ChangeMins),0) ChangeMins,
		COUNT(pro.Id) Hrs,
		COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs),0) NetHrs,
		(COUNT(pro.Id) - (ISNULL(SUM(d.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed/2) EffDEN,
		(COUNT(pro.Id) - (ISNULL(SUM(d.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed/2) EffDENSKU
	FROM [dbo].[Productivity] pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Shift s ON s.Id = pro.ShiftID
	INNER JOIN Product p ON p.Id = pro.ProductID
	INNER JOIN Supervisor su ON su.Id = pro.SupervisorID
	LEFT JOIN (
		SELECT
			ProductivityID,
			SUM(dr.Minutes)/CAST(60 AS Decimal) ChangeHrs,
			SUM(dr.Minutes) ChangeMins
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1 AND dr.FlowIndex = 1
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 2
		AND ((pro.ProductID <> pro.ProductID2) AND (pro.ProductID <> 0 AND pro.ProductID2 <> 0))
	GROUP BY 
		pro.Date,
		l.Name,
		s.Name,
		p.Sku,
		pro.Sku,
		p.Flavour,
		p.Packing,
		p.NetContent,
		su.Name,
		su.Description

	UNION ALL
	-- Flow 2
	SELECT 
		pro.Date,
		l.Name Line,
		s.Name Shift,
		p.Sku SKU,
		pro.Sku ProductId,
		p.Flavour,
		p.Packing,
		p.NetContent,
		2 Flow,
		su.Description Supervisor, 
		su.Name SupervisorName,
		SUM(ISNULL(pro.Production2,0)) Production,
		SUM(ISNULL(pro.ScrapUnits2,0)) Scrap,
		MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
		MIN(l.Efficiency) Efficiency,
		ISNULL(SUM(d.ChangeHrs),0) ChangeHrs,
		ISNULL(SUM(d.ChangeMins),0) ChangeMins,
		COUNT(pro.Id) Hrs,
		COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs),0) NetHrs,
		(COUNT(pro.Id) - (ISNULL(SUM(d.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed/2) EffDEN,
		(COUNT(pro.Id) - (ISNULL(SUM(d.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed/2) EffDENSKU
	FROM [dbo].[Productivity] pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Shift s ON s.Id = pro.ShiftID
	INNER JOIN Product p ON p.Id = pro.ProductID2
	INNER JOIN Supervisor su ON su.Id = pro.SupervisorID
	LEFT JOIN (
		SELECT
			ProductivityID,
			SUM(dr.Minutes)/CAST(60 AS Decimal) ChangeHrs,
			SUM(dr.Minutes) ChangeMins
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1 AND dr.FlowIndex = 2
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 2
		AND ((pro.ProductID <> pro.ProductID2) AND (pro.ProductID <> 0 AND pro.ProductID2 <> 0))
	GROUP BY 
		pro.Date,
		l.Name,
		s.Name,
		p.Sku,
		pro.Sku,
		p.Flavour,
		p.Packing,
		p.NetContent,
		su.Name,
		su.Description
) as t
GO
";

            string sqlCodeViewProductivitySummary = @"
CREATE VIEW [dbo].[ProductivitySummary] 
AS
SELECT
	Date
,	m.Production
,	m.ScrapUnits
,   m.BottleScrap
,   m.CanScrap
,   m.PreformScrap
,   m.PouchScrap
,	p.StandardSpeed
,	ISNULL(p.UnitsPerPackage,0) AS UnitsPerPackage
,	l.Name AS Line
,	l.Id AS LineId
,	ISNULL(m.Production,0) * ISNULL(p.UnitsPerPackage,0) AS ScrapDEN
,	t.Name AS Shift
,	t.Id AS ShiftId
,	p.SKU
,	p.Id AS ProductId
,	p.Flavour
,	p.Packing
,	P.NetContent
,	s.Id AS SupervisorId
,	s.Description AS Supervisor
,	s.Name AS 'SupervisorName'
FROM Productivity m
LEFT JOIN Product p ON m.ProductID = p.Id
JOIN Line l ON p.LineID = l.Id
JOIN Shift t on m.ShiftID = t.Id
JOIN Supervisor s ON m.SupervisorID = s.Id

UNION ALL

SELECT
	Date
,	m.Production2 AS Produccion
,	m.ScrapUnits2 AS ScrapUnits
,   m.BottleScrap2 AS BottleScrap
,   m.CanScrap2 AS CanScrap
,   m.PreformScrap2 AS PreformScrap
,   m.PouchScrap2 AS PouchScrap
,	p.StandardSpeed
,	ISNULL(p.UnitsPerPackage,0) AS UnitsPerPackage
,	l.Name AS Line
,	l.Id AS LineId
,	ISNULL(m.Production2,0) * ISNULL(p.UnitsPerPackage,0) AS ScrapDEN
,	t.Name AS Shift
,	t.Id AS ShiftId
,	p.SKU
,	p.Id AS ProductId
,	p.Flavour
,	p.Packing
,	P.NetContent
,	s.Id AS SupervisorId
,	s.Description AS Supervisor
,	s.Name AS 'SupervisorName'
FROM Productivity m
LEFT JOIN Product p ON m.ProductID2 = p.Id
JOIN Line l ON p.LineID = l.Id
JOIN Shift t on m.ShiftID = t.Id
JOIN Supervisor s ON m.SupervisorID = s.Id
GO
";

            string sqlCodeViewvwCases = @"
CREATE VIEW [dbo].[vwCases]
AS
SELECT p.Date, l.Name AS Line, s.Name AS Shift, SUM(p.Production + p.Production2) AS Production
FROM dbo.Productivity AS p INNER JOIN
	dbo.Line AS l ON l.Id = p.LineID INNER JOIN
	dbo.Shift AS s ON s.Id = p.ShiftID
GROUP BY p.Date, l.Name, s.Name
GO
";

            string sqlCodeViewvwChange = @"
CREATE VIEW [dbo].[vwChange]
AS

SELECT
	D.Date
,	D.Supervisor
,	D.Line
,	D.SKU
,	D.Flavour
,	D.NetContent
,	D.Packing
,	D.shift_
,	D.Minutes
,	D.Minutes / CAST(60 AS decimal) AS 'Hours'
,	D.FlowIndex
,	D.Category
,	D.Code
,	D.Failure
,	D.DowntimeSubCategory2
,	D.ObjectiveMinutes
,	D.HourStart
,	D.Sort
,	D.Hora_Sort
FROM vwDowntime D
Inner Join DowntimeCategory C ON C.Name = D.Category
WHERE C.IsChangeOver = 1
GO
";

            string sqlCodeViewvwDailySummary = @"
CREATE VIEW [dbo].[vwDailySummary]
AS
SELECT        pro.Id, pro.Date, l.Name AS Line, p1.Sku, p1.Flavour AS Flavor, pro.Production, p1.Packing, s.Name AS Shift
FROM            dbo.Productivity AS pro INNER JOIN
                         dbo.Shift AS s ON pro.ShiftID = s.Id INNER JOIN
                         dbo.Product AS p1 ON pro.ProductID = p1.Id INNER JOIN
                         dbo.Line AS l ON pro.LineID = l.Id
UNION ALL
SELECT        pro.Id, pro.Date, l.Name AS Line, p1.Sku, p1.Flavour AS Flavor, pro.Production2 AS Production, p1.Packing, s.Name AS Shift
FROM            dbo.Productivity AS pro INNER JOIN
                         dbo.Shift AS s ON pro.ShiftID = s.Id INNER JOIN
                         dbo.Product AS p1 ON pro.ProductID2 = p1.Id INNER JOIN
                         dbo.Line AS l ON pro.LineID = l.Id
WHERE        (pro.ProductID2 IS NOT NULL)
GO
";

            string sqlCodeViewvwDowntime = @"
CREATE VIEW [dbo].[vwDowntime] 
AS
SELECT
	m.Date
,	s.Description AS 'Supervisor'
,	l.Name AS 'Line'
,	p.SKU
,	p.Flavour
,	p.NetContent
,	p.Packing
,	t.Name as 'shift_'
,	c.Minutes
,	c.Minutes / CAST(60 AS decimal) AS 'hours'
,	c.FlowIndex
,	d.Name as 'Category'
,	dc.Code
,	dc.Failure
,	ds.Name as 'DowntimeSubCategory2'
,	o.ObjectiveMinutes
,	m.HourStart
,	h.Sort
,	CONVERT(VARCHAR(10),m.Date,112) + '-' + RIGHT('00' + CAST(h.Sort AS VARCHAR(2)),2) AS Hora_Sort
FROM DowntimeReason c
JOIN Productivity m ON c.ProductivityID = m.Id
JOIN DowntimeCategory d ON c.DowntimeCategoryId = d.Id
JOIN DowntimeCode dc ON c.DowntimeCodeId = dc.Id
JOIN Supervisor s ON m.SupervisorID = s.Id
JOIN Shift t ON m.ShiftID = t.Id
JOIN Product p ON m.ProductId = p.Id
JOIN Line l ON p.LineID = l.Id
JOIN Hour h ON m.HourStart = h.[Time]
Join DowntimeSubCategory2 ds ON ds.id = c.DowntimeSubCategory2Id
LEFT JOIN DowntimeCode o ON dc.Code = o.Code
WHERE c.FlowIndex = 1

UNION ALL

SELECT
	m.Date
,	s.Description AS 'Supervisor'
,	l.Name AS 'Line'
,	p.SKU
,	p.Flavour
,	p.NetContent
,	p.Packing
,	t.Name as 'shift_'
,	c.Minutes
,	c.Minutes / CAST(60 AS decimal) AS 'hours'
,	c.FlowIndex
,	d.Name as 'Category'
,	dc.Code
,	dc.Failure
,	ds.Name as 'DowntimeSubCategory2'
,	o.ObjectiveMinutes
,	m.HourStart
,	h.Sort
,	CONVERT(VARCHAR(10),m.Date,112) + '-' + RIGHT('00' + CAST(h.Sort AS VARCHAR(2)),2) AS Hora_Sort
FROM DowntimeReason c
JOIN Productivity m ON c.ProductivityID = m.Id
JOIN DowntimeCategory d ON c.DowntimeCategoryId = d.Id
JOIN DowntimeCode dc ON c.DowntimeCodeId = dc.Id
JOIN Supervisor s ON m.SupervisorID = s.Id
JOIN Shift t ON m.ShiftID = t.Id
JOIN Product p ON m.ProductID2 = p.Id
JOIN Line l ON p.LineID = l.Id
JOIN Hour h ON m.HourStart = h.[Time]
Join DowntimeSubCategory2 ds ON ds.id =  c.DowntimeSubCategory2Id
LEFT JOIN DowntimeCode o ON dc.Code = o.Code
WHERE c.FlowIndex = 2
GO

";

            string sqlCodeViewvwDowntimePerSku = @"
CREATE VIEW [dbo].[vwDowntimePerSku] 
AS
SELECT *
FROM (
	SELECT 
		p.Date,
		l.Id LineID, 
		l.Name Line,
		dc.Id CategoryID,
		dc.Name Category,
		s.Id ShiftID, 
		s.Name Shift,
		pro.Packing,
		pro.Id SKUID,
		pro.Sku SKU,
		pro.Flavour Flavor,
		pro.NetContent,
		dr.Minutes,
		dc2.Name SubCategory2,
		dco.Code,
		dco.Failure
	FROM DowntimeReason dr
	INNER JOIN Productivity p ON p.Id = dr.ProductivityID
	INNER JOIN Line l ON l.Id = p.LineID
	INNER JOIN Shift s ON s.Id = p.ShiftID
	INNER JOIN Product pro ON pro.Id = p.ProductID
	INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
	INNER JOIN DowntimeSubCategory2 dc2 ON dc2.Id = dr.DowntimeSubCategory2Id
	LEFT JOIN DowntimeCode dco ON dco.Id = dr.DowntimeCodeId
	WHERE
		dr.FlowIndex = 1

	UNION ALL

	SELECT 
		p.Date,
		l.Id LineID, 
		l.Name Line,
		dc.Id CategoryID,
		dc.Name Category,
		s.Id ShiftID, 
		s.Name Shift,
		pro.Packing,
		pro.Id SKUID,
		pro.Sku SKU,
		pro.Flavour Flavor,
		pro.NetContent,
		dr.Minutes,
		dc2.Name SubCategory2,
		dco.Code,
		dco.Failure
	FROM DowntimeReason dr
	INNER JOIN Productivity p ON p.Id = dr.ProductivityID
	INNER JOIN Line l ON l.Id = p.LineID
	INNER JOIN Shift s ON s.Id = p.ShiftID
	INNER JOIN Product pro ON pro.Id = p.ProductID2
	INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
	INNER JOIN DowntimeSubCategory2 dc2 ON dc2.Id = dr.DowntimeSubCategory2Id
	LEFT JOIN DowntimeCode dco ON dco.Id = dr.DowntimeCodeId
	WHERE
		dr.FlowIndex = 2
) AS t
GO
";

            string sqlCodeViewvwDowntimeTrend = @"
CREATE VIEW [dbo].[vwDowntimeTrend] 
AS
SELECT
	m.Date
,	l.Id AS 'LineId'
,	l.Name AS 'Line'
,	d.Id AS 'CategoryId'
,	d.Name AS 'Category'
,	ds.Id AS 'SubCategory2Id'
,	ds.Name AS 'SubCategory2'
,	dc.Id AS 'CodeId'
,	dc.Code AS 'Code'
,	c.FlowIndex
,	c.Minutes
,	c.Minutes / CAST(60 AS decimal) AS 'hours'
,	dc.Failure
FROM DowntimeReason c
JOIN Productivity m ON c.ProductivityID = m.Id
JOIN DowntimeCategory d ON c.DowntimeCategoryId = d.Id
LEFT JOIN DowntimeCode dc ON c.DowntimeCodeId = dc.Id
JOIN Product p ON m.ProductId = p.Id
JOIN Line l ON p.LineID = l.Id
Join DowntimeSubCategory2 ds ON ds.id = c.DowntimeSubCategory2Id
WHERE c.FlowIndex = 1

UNION ALL

SELECT
	m.Date
,	l.Id AS 'LineId'
,	l.Name AS 'Line'
,	d.Id as 'CategoryId'
,	d.Name as 'Category'
,	ds.Id as 'SubCategory2Id'
,	ds.Name as 'SubCategory2'
,	dc.Id AS 'CodeId'
,	dc.Code AS 'Code'
,	c.FlowIndex
,	c.Minutes
,	c.Minutes / CAST(60 AS decimal) AS 'hours'
,	dc.Failure
FROM DowntimeReason c
JOIN Productivity m ON c.ProductivityID = m.Id
JOIN DowntimeCategory d ON c.DowntimeCategoryId = d.Id
LEFT JOIN DowntimeCode dc ON c.DowntimeCodeId = dc.Id
JOIN Product p ON m.ProductID2 = p.Id
JOIN Line l ON p.LineID = l.Id
Join DowntimeSubCategory2 ds ON ds.id =  c.DowntimeSubCategory2Id
WHERE c.FlowIndex = 2
GO
";

            string sqlCodeViewvwDowntimeXSubCat = @"
CREATE VIEW [dbo].[vwDowntimeXSubCat] 
AS
SELECT
	m.Date
,	l.Id AS 'LineId'
,	l.Name AS 'Line'
,	t.Id as 'shiftId'
,	t.Name as 'shift'
,	d.Id as 'CategoryId'
,	d.Name as 'Category'
,	ds.Id as 'SubCategory2Id'
,	ds.Name as 'SubCategory2'
,	c.FlowIndex
,	c.Minutes
,	c.Minutes / CAST(60 AS decimal) AS 'hours'
,	dc.Code
,	dc.Failure
FROM DowntimeReason c
JOIN Productivity m ON c.ProductivityID = m.Id
JOIN DowntimeCategory d ON c.DowntimeCategoryId = d.Id
LEFT JOIN DowntimeCode dc ON c.DowntimeCodeId = dc.Id
JOIN Shift t ON m.ShiftID = t.Id
JOIN Product p ON m.ProductId = p.Id
JOIN Line l ON p.LineID = l.Id
Join DowntimeSubCategory2 ds ON ds.id = c.DowntimeSubCategory2Id
WHERE c.FlowIndex = 1

UNION ALL

SELECT
	m.Date
,	l.Id AS 'LineId'
,	l.Name AS 'Line'
,	t.Id as 'shiftId'
,	t.Name as 'shift'
,	d.Id as 'CategoryId'
,	d.Name as 'Category'
,	ds.Id as 'SubCategory2Id'
,	ds.Name as 'SubCategory2'
,	c.FlowIndex
,	c.Minutes
,	c.Minutes / CAST(60 AS decimal) AS 'hours'
,	dc.Code
,	dc.Failure
FROM DowntimeReason c
JOIN Productivity m ON c.ProductivityID = m.Id
JOIN DowntimeCategory d ON c.DowntimeCategoryId = d.Id
LEFT JOIN DowntimeCode dc ON c.DowntimeCodeId = dc.Id
JOIN Shift t ON m.ShiftID = t.Id
JOIN Product p ON m.ProductID2 = p.Id
JOIN Line l ON p.LineID = l.Id
Join DowntimeSubCategory2 ds ON ds.id =  c.DowntimeSubCategory2Id
WHERE c.FlowIndex = 2
GO
";

            string sqlCodeViewvwGeneralEfficiency = @"
CREATE VIEW [dbo].[vwGeneralEfficiency]
AS
	SELECT *
FROM (
	-- Line with one flow
	SELECT 
		pro.Date,
		l.Id LineId,
		l.Name Line,
		s.Id ShiftId,
		s.Name Shift,
		pro.Sku,
		p.Id ProductId,
		p.Flavour,
		p.Packing,
		p.NetContent,
		1 Flow,
		su.Id SupervisorId,
		su.Name Supervisor,
		SUM(pro.Production) Production,
		MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
		ISNULL(SUM(d.ChangeHrs),0) ChangeHrs,
		ISNULL(SUM(d.ChangeMins),0) ChangeMins,
		COUNT(pro.Id) Hrs,
		COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs),0) NetHrs,
		(COUNT(pro.Id) - (ISNULL(SUM(d.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed) EffDEN
	FROM [dbo].[Productivity] pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Shift s ON s.Id = pro.ShiftID
	INNER JOIN Product p ON p.Id = pro.ProductID
	INNER JOIN Supervisor su ON su.Id = pro.SupervisorID
	LEFT JOIN (
		SELECT
			ProductivityID,
			SUM(dr.Minutes)/CAST(60 AS Decimal) ChangeHrs,
			SUM(dr.Minutes) ChangeMins
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 1
	GROUP BY 
		pro.Date,
		l.Name,
		l.Id,
		s.Id,
		s.Name,
		pro.Sku,
		p.Id,
		p.Flavour,
		p.Packing,
		p.NetContent,
		su.Id,
		su.Name

	UNION ALL

	-- Line with two flow
	-- Same SKU
	SELECT 
		pro.Date,
		l.Id LineId,
		l.Name Line,
		s.Id ShiftId,
		s.Name Shift,
		pro.Sku,
		p.Id ProductId,
		p.Flavour,
		p.Packing,
		p.NetContent,
		1 Flow,
		su.Id SupervisorId,
		su.Name Supervisor,
		SUM(ISNULL(pro.Production,0) + ISNULL(pro.Production2,0)) Production,
		MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
		ISNULL(SUM(d.ChangeHrs),0) ChangeHrs,
		ISNULL(SUM(d.ChangeMins),0) ChangeMins,
		COUNT(pro.Id) Hrs,
		COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs),0) NetHrs,
		(COUNT(pro.Id) - (ISNULL(SUM(d.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed) EffDEN
	FROM [dbo].[Productivity] pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Shift s ON s.Id = pro.ShiftID
	INNER JOIN Product p ON p.Id = pro.ProductID
	INNER JOIN Supervisor su ON su.Id = pro.SupervisorID
	LEFT JOIN (
		SELECT
			ProductivityID,
			SUM(dr.Minutes)/CAST(60 AS Decimal) ChangeHrs,
			SUM(dr.Minutes) ChangeMins
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 2
		AND ((pro.ProductID = pro.ProductID2) OR (pro.ProductID = 0 OR pro.ProductID2 = 0))
	GROUP BY 
		pro.Date,
		l.Name,
		l.Id,
		s.Id,
		s.Name,
		pro.Sku,
		p.Id,
		p.Flavour,
		p.Packing,
		p.NetContent,
		su.Id,
		su.Name


	UNION ALL
	-- Different SKU
	-- Flow 1
	SELECT 
		pro.Date,
		l.Id LineId,
		l.Name Line,
		s.Id ShiftId,
		s.Name Shift,
		pro.Sku,
		p.Id ProductId,
		p.Flavour,
		p.Packing,
		p.NetContent,
		1 Flow,
		su.Id SupervisorId,
		su.Name Supervisor,
		SUM(ISNULL(pro.Production,0)) Production,
		MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
		ISNULL(SUM(d.ChangeHrs),0) ChangeHrs,
		ISNULL(SUM(d.ChangeMins),0) ChangeMins,
		COUNT(pro.Id) Hrs,
		COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs),0) NetHrs,
		(COUNT(pro.Id) - (ISNULL(SUM(d.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed/2) EffDEN
	FROM [dbo].[Productivity] pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Shift s ON s.Id = pro.ShiftID
	INNER JOIN Product p ON p.Id = pro.ProductID
	INNER JOIN Supervisor su ON su.Id = pro.SupervisorID
	LEFT JOIN (
		SELECT
			ProductivityID,
			SUM(dr.Minutes)/CAST(60 AS Decimal) ChangeHrs,
			SUM(dr.Minutes) ChangeMins
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1 AND dr.FlowIndex = 1
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 2
		AND ((pro.ProductID <> pro.ProductID2) AND (pro.ProductID <> 0 AND pro.ProductID2 <> 0))
	GROUP BY 
		pro.Date,
		l.Name,
		l.Id,
		s.Id,
		s.Name,
		pro.Sku,
		p.Id,
		p.Flavour,
		p.Packing,
		p.NetContent,
		su.Id,
		su.Name

	UNION ALL
	-- Flow 2
	SELECT 
		pro.Date,
		l.Id LineId,
		l.Name Line,
		s.Id ShiftId,
		s.Name Shift,
		pro.Sku,
		p.Id ProductId,
		p.Flavour,
		p.Packing,
		p.NetContent,
		2 Flow,
		su.Id SupervisorId,
		su.Name Supervisor,
		SUM(ISNULL(pro.Production2,0)) Production,
		MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
		ISNULL(SUM(d.ChangeHrs),0) ChangeHrs,
		ISNULL(SUM(d.ChangeMins),0) ChangeMins,
		COUNT(pro.Id) Hrs,
		COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs),0) NetHrs,
		(COUNT(pro.Id) - (ISNULL(SUM(d.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed/2) EffDEN
	FROM [dbo].[Productivity] pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Shift s ON s.Id = pro.ShiftID
	INNER JOIN Product p ON p.Id = pro.ProductID2
	INNER JOIN Supervisor su ON su.Id = pro.SupervisorID
	LEFT JOIN (
		SELECT
			ProductivityID,
			SUM(dr.Minutes)/CAST(60 AS Decimal) ChangeHrs,
			SUM(dr.Minutes) ChangeMins
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1 AND dr.FlowIndex = 2
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 2
		AND ((pro.ProductID <> pro.ProductID2) AND (pro.ProductID <> 0 AND pro.ProductID2 <> 0))
	GROUP BY 
		pro.Date,
		l.Name,
		l.Id,
		s.Id,
		s.Name,
		pro.Sku,
		p.Id,
		p.Flavour,
		p.Packing,
		p.NetContent,
		su.Id,
		su.Name
) as t
GO
";

            string sqlCodeViewvwPackageSku = @"
CREATE VIEW [dbo].[vwPackageSku]
AS
	SELECT *
FROM (
	-- Can calculate:
	-- Cases - Scrap -Efficiency
	-- Per (Line, Sku, Packing, Sku, Supervisors)

	-- Line with one flow
	SELECT 
		pro.Date,
		l.Id LineID,
		l.Name Line,
		p.Packing,
		p.Sku,
		p.Flavour Flavor,
		p.NetContent,
		p.UnitsPerPackage,
		s.Id SupervisorId,
		s.Name Supervisor,
		SUM(pro.ScrapUnits) ScrapUnits,
		SUM(pro.CanScrap) CanScrap,
		SUM(pro.BottleScrap) BottleScrap,
		SUM(pro.PreformScrap) PreformScrap,
		SUM(pro.Production) Production,
		COUNT(pro.Id) Hrs,
		ISNULL(SUM(d.DownHrs),0) DownHrs,
		COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0) NetHrs,
		MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
		((COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0)) * (p.StandardSpeed)) AS MaxProduction
	FROM Productivity pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Product p ON p.Id = pro.ProductID
	INNER JOIN Supervisor s ON s.Id = pro.SupervisorID
	LEFT JOIN (
		SELECT
			ProductivityID,
			SUM(dr.Minutes)/CAST(60 AS Decimal) DownHrs
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 1
	GROUP BY
		pro.Date,
		l.Id,
		l.Name,
		p.Packing,
		p.Sku,
		p.Flavour,
		p.NetContent,
		p.UnitsPerPackage,
		s.Id,
		s.Name,
		p.StandardSpeed

	UNION ALL
	
	-- Line with two flow
	-- Same SKU
		SELECT 
		pro.Date,
		l.Id LineID,
		l.Name Line,
		p.Packing,
		p.Sku,
		p.Flavour Flavor,
		p.NetContent,
		p.UnitsPerPackage,
		s.Id SupervisorId,
		s.Name Supervisor,
		SUM(pro.ScrapUnits) + SUM(pro.ScrapUnits2) ScrapUnits,
		SUM(pro.CanScrap) + SUM(pro.CanScrap2) CanScrap,
		SUM(pro.BottleScrap) + SUM(pro.BottleScrap2) BottleScrap,
		SUM(pro.PreformScrap) + SUM(pro.PreformScrap2) PreformScrap,
		SUM(ISNULL(pro.Production,0) + ISNULL(pro.Production2,0)) Production,
		COUNT(pro.Id) Hrs,
		ISNULL(SUM(d.DownHrs),0) DownHrs,
		COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0) NetHrs,
		MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
		((COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0)) * (p.StandardSpeed)) AS MaxProduction
	FROM Productivity pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Product p ON p.Id = pro.ProductID
	INNER JOIN Supervisor s ON s.Id = pro.SupervisorID
	LEFT JOIN (
		SELECT
			ProductivityID,
			SUM(dr.Minutes)/CAST(60 AS Decimal) DownHrs
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 2
		AND ((pro.ProductID = pro.ProductID2) OR (pro.ProductID = 0 OR pro.ProductID2 = 0))
	GROUP BY
		pro.Date,
		l.Id,
		l.Name,
		p.Packing,
		p.Sku,
		p.Flavour,
		p.NetContent,
		p.UnitsPerPackage,
		s.Id,
		s.Name,
		p.StandardSpeed

	UNION ALL
	-- Different SKU
	-- Flow 1
		SELECT 
		pro.Date,
		l.Id LineID,
		l.Name Line,
		p.Packing,
		p.Sku,
		p.Flavour Flavor,
		p.NetContent,
		p.UnitsPerPackage,
		s.Id SupervisorId,
		s.Name Supervisor,
		SUM(pro.ScrapUnits) ScrapUnits,
		SUM(pro.CanScrap) CanScrap,
		SUM(pro.BottleScrap) BottleScrap,
		SUM(pro.PreformScrap) PreformScrap,
		SUM(ISNULL(pro.Production,0)) Production,
		COUNT(pro.Id) Hrs,
		ISNULL(SUM(d.DownHrs),0) DownHrs,
		COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0) NetHrs,
		MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
		((COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0)) * (p.StandardSpeed/2)) AS MaxProduction
	FROM Productivity pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Product p ON p.Id = pro.ProductID
	INNER JOIN Supervisor s ON s.Id = pro.SupervisorID
	LEFT JOIN (
		SELECT
			ProductivityID,
			SUM(dr.Minutes)/CAST(60 AS Decimal) DownHrs
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1 AND dr.FlowIndex = 1
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 2
		AND ((pro.ProductID <> pro.ProductID2) AND (pro.ProductID <> 0 AND pro.ProductID2 <> 0))
	GROUP BY
		pro.Date,
		l.Id,
		l.Name,
		p.Packing,
		p.Sku,
		p.Flavour,
		p.NetContent,
		p.UnitsPerPackage,
		s.Id,
		s.Name,
		p.StandardSpeed

	UNION ALL
	-- Flow 2
		SELECT 
		pro.Date,
		l.Id LineID,
		l.Name Line,
		p.Packing,
		p.Sku,
		p.Flavour Flavor,
		p.NetContent,
		p.UnitsPerPackage,
		s.Id SupervisorId,
		s.Name Supervisor,
		SUM(pro.ScrapUnits2) ScrapUnits,
		SUM(pro.CanScrap2) CanScrap,
		SUM(pro.BottleScrap2) BottleScrap,
		SUM(pro.PreformScrap2) PreformScrap,
		SUM(ISNULL(pro.Production2,0)) Production,
		COUNT(pro.Id) Hrs,
		ISNULL(SUM(d.DownHrs),0) DownHrs,
		COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0) NetHrs,
		MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
		((COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0)) * (p.StandardSpeed/2)) AS MaxProduction
	FROM Productivity pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Product p ON p.Id = pro.ProductID2
	INNER JOIN Supervisor s ON s.Id = pro.SupervisorID
	LEFT JOIN (
		SELECT
			ProductivityID,
			SUM(dr.Minutes)/CAST(60 AS Decimal) DownHrs
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1 AND dr.FlowIndex = 2
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 2
		AND ((pro.ProductID <> pro.ProductID2) AND (pro.ProductID <> 0 AND pro.ProductID2 <> 0))
	GROUP BY
		pro.Date,
		l.Id,
		l.Name,
		p.Packing,
		p.Sku,
		p.Flavour,
		p.NetContent,
		p.UnitsPerPackage,
		s.Id,
		s.Name,
		p.StandardSpeed
	) AS T
GO

";

            string sqlCodeViewvwProductivityReport = @"
CREATE VIEW [dbo].[vwProductivityReport]
AS
SELECT
	p.Id,
	p.Date,
	l.Name AS Line,
	l.Id AS LineID,
	p.Sku,
	prod.Flavour AS Flavor,
	s.Name AS Shift,
	s.Id AS ShiftID,
	CAST(p.Production AS INT) AS Production,
	p.StandardSpeed,
	p.HourStart,
	p.Name AS Supervisor,
	prod.NetContent, 
    prod.Packing, 
	l.Flow, 
	super.Id AS SupervisorID, 
    p.DowntimeMinutes AS Downtime_minutes, 
	p.HourEnd, 
	p.Sku2, 
	prod2.Flavour AS Flavor2, 
	CAST(p.Production2 AS INT) AS Production2, 
	prod2.NetContent AS NetContent2, 
	prod2.Packing AS Packing2, 
	prod.Id AS ProductID,
	prod2.Id AS ProductID2
FROM dbo.Productivity AS p 
  INNER JOIN Shift AS s ON p.ShiftID = s.Id 
  INNER JOIN Line AS l ON p.LineID = l.Id 
  LEFT JOIN Product AS prod ON p.ProductID = prod.Id
  LEFT JOIN Product AS prod2 ON p.ProductID2 = prod2.Id
  INNER JOIN Supervisor AS super ON p.SupervisorID = super.Id 
GO

";

            string sqlCodeViewvwStatisticalChangeOver = @"
CREATE VIEW [dbo].[vwStatisticalChangeOver] 
AS
SELECT
	m.Date
,	s.Id AS 'SupervisorId'
,	s.Description AS 'Supervisor'
,	l.Id AS 'LineId'
,	l.Name AS 'Line'
,	t.Id as 'shiftId'
,	t.Name as 'shift'
,	ds.Id as 'SubCategory2Id'
,	ds.Name as 'SubCategory2'
,	dc.Id as 'CodeId'
,	dc.Code
,	c.Minutes
,	c.Minutes / CAST(60 AS decimal) AS 'hours'
,	c.FlowIndex
,	dc.Failure
,	o.ObjectiveMinutes
FROM DowntimeReason c
JOIN Productivity m ON c.ProductivityID = m.Id
JOIN DowntimeCategory d ON c.DowntimeCategoryId = d.Id
JOIN DowntimeCode dc ON c.DowntimeCodeId = dc.Id
JOIN Supervisor s ON m.SupervisorID = s.Id
JOIN Shift t ON m.ShiftID = t.Id
JOIN Product p ON m.ProductId = p.Id
JOIN Line l ON p.LineID = l.Id
Join DowntimeSubCategory2 ds ON ds.id = c.DowntimeSubCategory2Id
LEFT JOIN DowntimeCode o ON dc.Code = o.Code
WHERE c.FlowIndex = 1 AND d.IsChangeOver = 1

UNION ALL

SELECT
	m.Date
,	s.Id AS 'SupervisorId'
,	s.Description AS 'Supervisor'
,	l.Id AS 'LineId'
,	l.Name AS 'Line'
,	t.Id as 'shiftId'
,	t.Name as 'shift'
,	ds.Id as 'SubCategory2Id'
,	ds.Name as 'SubCategory2'
,	dc.Id as 'CodeId'
,	dc.Code
,	c.Minutes
,	c.Minutes / CAST(60 AS decimal) AS 'hours'
,	c.FlowIndex
,	dc.Failure
,	o.ObjectiveMinutes
FROM DowntimeReason c
JOIN Productivity m ON c.ProductivityID = m.Id
JOIN DowntimeCategory d ON c.DowntimeCategoryId = d.Id
JOIN DowntimeCode dc ON c.DowntimeCodeId = dc.Id
JOIN Supervisor s ON m.SupervisorID = s.Id
JOIN Shift t ON m.ShiftID = t.Id
JOIN Product p ON m.ProductID2 = p.Id
JOIN Line l ON p.LineID = l.Id
Join DowntimeSubCategory2 ds ON ds.id =  c.DowntimeSubCategory2Id
LEFT JOIN DowntimeCode o ON dc.Code = o.Code
WHERE c.FlowIndex = 2 AND d.IsChangeOver = 1
GO

";

            string sqlCodeViewvwSupervisorMetrics = @"
CREATE VIEW [dbo].[vwSupervisorMetrics] 
AS
SELECT
	m.Date
,	l.Id AS 'LineId'
,	l.Name AS 'Line'
,	s.Id AS 'ShiftId'
,	s.Name AS 'Shift'
,	su.Id AS 'SupervisorId'
,	su.Name AS 'Supervisor'
,   SUM(m.Production) AS 'Production'
,   SUM(ISNULL(m.ScrapUnits,0)) AS 'ScrapUnits'
,   SUM(ISNULL(m.Production,0) * ISNULL(p.UnitsPerPackage,0)) AS 'ScrapDen'
FROM Productivity m
INNER JOIN Product p ON m.ProductID = p.Id
INNER JOIN Line l ON m.LineID = l.Id
INNER JOIN Shift s on m.ShiftID = s.Id
INNER JOIN Supervisor su ON m.SupervisorID = su.Id
GROUP BY 
		m.Date,
		l.Name,
		l.Id,
		s.Id,
		s.Name,
		su.Id,
		su.Name

UNION ALL

SELECT
	m.Date
,	l.Id AS 'LineId'
,	l.Name AS 'Line'
,	s.Id AS 'ShiftId'
,	s.Name AS 'Shift'
,	su.Id AS 'SupervisorId'
,	su.Name AS 'Supervisor'
,   SUM(m.Production2) AS 'Production'
,   SUM(ISNULL(m.ScrapUnits2,0)) AS 'ScrapUnits'
,	SUM(ISNULL(m.Production2,0) * ISNULL(p.UnitsPerPackage,0)) AS 'ScrapDen'
FROM Productivity m
INNER JOIN Product p ON m.ProductID2 = p.Id
INNER JOIN Line l ON m.LineID = l.Id
INNER JOIN Shift s on m.ShiftID = s.Id
INNER JOIN Supervisor su ON m.SupervisorID = su.Id
GROUP BY 
		m.Date,
		l.Name,
		l.Id,
		s.Id,
		s.Name,
		su.Id,
		su.Name
GO
";

            string sqlCodeViewvwUtilization = @"
CREATE VIEW [dbo].[vwUtilization]
AS

SELECT *
FROM (
	-- Line with one flow
	SELECT 
		pro.Date,
		l.Name,
		p.Sku SKU,
		pro.ProductID ProductId,
		1 flow,
		COUNT(pro.Id) Hrs,
		ISNULL(SUM(d.ChangeHrs), 0) ChangeHrs,
		(COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs), 0)) NetHrs,
		(COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs), 0)) NetPlantHrs
	FROM 
		Productivity pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Product p ON p.Id = pro.ProductID
	LEFT JOIN (
		SELECT 
			ProductivityID,
			SUM(dr.Minutes) / CAST(60 AS Decimal) ChangeHrs,
			SUM(dr.Minutes) ChangeMins
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 1
	GROUP BY
		pro.Date,
		l.Name,
		p.Sku,
		pro.ProductID

	UNION ALL
	-- Line with two flow
	-- Flow 1
		SELECT 
		pro.Date,
		l.Name,
		p.Sku SKU,
		pro.ProductID ProductId,
		1 flow,
		COUNT(pro.Id) Hrs,
		ISNULL(SUM(d.ChangeHrs), 0) ChangeHrs,
		(COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs), 0)) NetHrs,
		(COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs), 0)) NetPlantHrs
	FROM 
		Productivity pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Product p ON p.Id = pro.ProductID
	LEFT JOIN (
		SELECT 
			ProductivityID,
			SUM(dr.Minutes) / CAST(60 AS Decimal) ChangeHrs,
			SUM(dr.Minutes) ChangeMins
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1 AND dr.FlowIndex = 1
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 2
	GROUP BY
		pro.Date,
		l.Name,
		p.Sku,
		pro.ProductID

	UNION ALL
		-- Line with two flow
	-- Flow 1
		SELECT 
		pro.Date,
		l.Name + ' (2)' Name,
		p.Sku SKU,
		pro.ProductID2 ProductId,
		2 flow,
		COUNT(pro.Id) Hrs,
		ISNULL(SUM(d.ChangeHrs), 0) ChangeHrs,
		(COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs), 0)) NetHrs,
		(COUNT(pro.Id) - ISNULL(SUM(d.ChangeHrs), 0)) NetPlantHrs
	FROM 
		Productivity pro
	INNER JOIN Line l ON l.Id = pro.LineID
	INNER JOIN Product p ON p.Id = pro.ProductID2
	LEFT JOIN (
		SELECT 
			ProductivityID,
			SUM(dr.Minutes) / CAST(60 AS Decimal) ChangeHrs,
			SUM(dr.Minutes) ChangeMins
		FROM DowntimeReason dr
		INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
		WHERE dc.IsChangeOver = 1 AND dr.FlowIndex = 2
		GROUP BY ProductivityID
	) d ON d.ProductivityID = pro.Id
	WHERE 
		l.Flow = 2
	GROUP BY
		pro.Date,
		l.Name,
		p.Sku,
		pro.ProductID2
) as t
GO

";

            // Stored Procedures
            string sqlCodeSPEfficiencyAnalysis = @"
CREATE PROCEDURE [dbo].[EfficiencyAnalysis] ( 
	@categories NVARCHAR(MAX),
	@startDate DATE,
	@endDate DATE
	)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	-- Create table temporal categories
	CREATE TABLE #TempCategories (category NVARCHAR(255))

	-- Insert categories params
	INSERT INTO #TempCategories (category)
	SELECT value FROM string_split(@categories, ',')

	SELECT *
	FROM (
	-- Line with one flow
		SELECT
			pro.Date,
			l.Name AS Line,
			t.Name AS Shift,
			p.Sku,
			P.Flavour AS Flavor,
			p.Packing,
			p.NetContent,
			SUM(pro.Production) Production,
			MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
			SUM(ISNULL(pro.ScrapUnits,0)) Scrap,
			COUNT(pro.Id) Hrs,
			ISNULL(SUM(d.DownHrs),0) DownHrs,
			COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0) NetHrs,
			((COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0)) * (p.StandardSpeed)) AS MaxProduction
		FROM Productivity AS pro
		INNER JOIN Line AS l ON l.Id = pro.LineID
		INNER JOIN Shift AS t ON t.Id = pro.ShiftID
		INNER JOIN Product AS p ON p.Id = pro.ProductID
		LEFT JOIN (
			SELECT
				dr.ProductivityID,
				SUM(dr.Minutes)/CAST(60 AS Decimal) DownHrs
			FROM DowntimeReason AS dr
			INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
			WHERE 
				dc.Name IN (SELECT category FROM #TempCategories)
			GROUP BY ProductivityID
		) AS d ON d.ProductivityID = pro.Id
		WHERE
			l.Flow = 1
			AND pro.Date BETWEEN @startDate AND @endDate 
		GROUP BY
			pro.Date,
			l.Name,
			t.Name,
			p.Sku,
			pro.Sku,
			p.Flavour,
			p.Packing,
			p.NetContent,
			p.StandardSpeed

	UNION ALL
	-- Line with two flow
	-- Same SKU
		SELECT
			pro.Date,
			l.Name AS Line,
			t.Name AS Shift,
			p.Sku,
			P.Flavour AS Flavor,
			p.Packing,
			p.NetContent,
			SUM(ISNULL(pro.Production,0) + ISNULL(pro.Production2,0)) Production,
			MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
			SUM(ISNULL(pro.ScrapUnits,0) + ISNULL(pro.ScrapUnits2,0)) Scrap,
			COUNT(pro.Id) Hrs,
			ISNULL(SUM(d.DownHrs),0) DownHrs,
			COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0) NetHrs,
			((COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0)) * (p.StandardSpeed)) AS MaxProduction
		FROM Productivity AS pro
		INNER JOIN Line AS l ON l.Id = pro.LineID
		INNER JOIN Shift AS t ON t.Id = pro.ShiftID
		INNER JOIN Product AS p ON p.Id = pro.ProductID
		LEFT JOIN (
			SELECT
				dr.ProductivityID,
				SUM(dr.Minutes)/CAST(60 AS Decimal) DownHrs
			FROM DowntimeReason AS dr
			INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
			WHERE 
				dc.Name IN (SELECT category FROM #TempCategories)
			GROUP BY ProductivityID
		) AS d ON d.ProductivityID = pro.Id
		WHERE
			l.Flow = 2
			AND ((pro.ProductID = pro.ProductID2) OR (pro.ProductID = 0 OR pro.ProductID2 = 0))
			AND pro.Date BETWEEN @startDate AND @endDate 
		GROUP BY
			pro.Date,
			l.Name,
			t.Name,
			p.Sku,
			pro.Sku,
			p.Flavour,
			p.Packing,
			p.NetContent,
			p.StandardSpeed

	UNION ALL
	-- Different SKU
	-- Flow 1
		SELECT
			pro.Date,
			l.Name AS Line,
			t.Name AS Shift,
			p.Sku,
			P.Flavour AS Flavor,
			p.Packing,
			p.NetContent,
			SUM(ISNULL(pro.Production,0)) Production,
			MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
			SUM(ISNULL(pro.ScrapUnits,0)) Scrap,
			COUNT(pro.Id) Hrs,
			ISNULL(SUM(d.DownHrs),0) DownHrs,
			COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0) NetHrs,
			((COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0)) * (p.StandardSpeed/2)) AS MaxProduction
		FROM Productivity AS pro
		INNER JOIN Line AS l ON l.Id = pro.LineID
		INNER JOIN Shift AS t ON t.Id = pro.ShiftID
		INNER JOIN Product AS p ON p.Id = pro.ProductID
		LEFT JOIN (
			SELECT
				dr.ProductivityID,
				SUM(dr.Minutes)/CAST(60 AS Decimal) DownHrs
			FROM DowntimeReason AS dr
			INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
			WHERE 
				dc.Name IN (SELECT category FROM #TempCategories)
				AND dr.FlowIndex = 1
			GROUP BY ProductivityID
		) AS d ON d.ProductivityID = pro.Id
		WHERE
			l.Flow = 2
			AND ((pro.ProductID <> pro.ProductID2) AND (pro.ProductID <> 0 AND pro.ProductID2 <> 0))
			AND pro.Date BETWEEN @startDate AND @endDate 
		GROUP BY
			pro.Date,
			l.Name,
			t.Name,
			p.Sku,
			pro.Sku,
			p.Flavour,
			p.Packing,
			p.NetContent,
			p.StandardSpeed

	UNION ALL
	-- Flow 2
		SELECT
			pro.Date,
			l.Name AS Line,
			t.Name AS Shift,
			p.Sku,
			P.Flavour AS Flavor,
			p.Packing,
			p.NetContent,
			SUM(ISNULL(pro.Production2,0)) Production,
			MIN(ISNULL(p.StandardSpeed,0)) StandardSpeed,
			SUM(ISNULL(pro.ScrapUnits,0)) Scrap,
			COUNT(pro.Id) Hrs,
			ISNULL(SUM(d.DownHrs),0) DownHrs,
			COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0) NetHrs,
			((COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0)) * (p.StandardSpeed/2)) AS MaxProduction
		FROM Productivity AS pro
		INNER JOIN Line AS l ON l.Id = pro.LineID
		INNER JOIN Shift AS t ON t.Id = pro.ShiftID
		INNER JOIN Product AS p ON p.Id = pro.ProductID2
		LEFT JOIN (
			SELECT
				dr.ProductivityID,
				SUM(dr.Minutes)/CAST(60 AS Decimal) DownHrs
			FROM DowntimeReason AS dr
			INNER JOIN DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
			WHERE 
				dc.Name IN (SELECT category FROM #TempCategories)
				AND dr.FlowIndex = 2
			GROUP BY ProductivityID
		) AS d ON d.ProductivityID = pro.Id
		WHERE
			l.Flow = 2
			AND ((pro.ProductID <> pro.ProductID2) AND (pro.ProductID <> 0 AND pro.ProductID2 <> 0))
			AND pro.Date BETWEEN @startDate AND @endDate 
		GROUP BY
			pro.Date,
			l.Name,
			t.Name,
			p.Sku,
			pro.Sku,
			p.Flavour,
			p.Packing,
			p.NetContent,
			p.StandardSpeed
	)
	AS T

END
GO
";

            string sqlCodeSPGenerateChangeOverRecords = @"
CREATE PROCEDURE [dbo].[GenerateChangeOverRecords]
AS

    DECLARE @date AS DATE
    DECLARE @end AS DATE
    DECLARE @row AS INT
    DECLARE @rows AS INT
    DECLARE @rowLine AS INT
    DECLARE @rowsLine AS INT
    DECLARE @dateC AS DATETIME
    DECLARE @dateN AS DATETIME
    DECLARE @dateP AS DATETIME
    DECLARE @minutes AS DECIMAL(18,2)
    DECLARE @minutesP AS DECIMAL(18,2)
    DECLARE @minutesTotal AS DECIMAL(18,2)
    DECLARE @codeC AS VARCHAR(20)
    DECLARE @codeN AS VARCHAR(20)
    DECLARE @codeP AS VARCHAR(20)
    DECLARE @lineC AS VARCHAR(15)
    DECLARE @shiftC AS VARCHAR(10)
    DECLARE @supervisorC AS VARCHAR(20)
    DECLARE @category2C AS VARCHAR(30)
    DECLARE @failureC AS VARCHAR(100)
    DECLARE @hour_sortC AS INT
    DECLARE @hour_sortN AS INT
    DECLARE @hour_sortP AS INT
    DECLARE @objectiveC AS INT
	DECLARE @lastLineID AS INT
	DECLARE @firstLineID AS INT
    DECLARE @currentLine AS VARCHAR(15)


    IF OBJECT_ID('tempdb..#changerecords') IS NOT NULL DROP TABLE #changerecords

    CREATE TABLE #changerecords (
        Id INT IDENTITY(1,1)
    ,  Fecha DATETIME
    ,  Linea VARCHAR(20)
    ,  Turno VARCHAR(10)
    ,  Supervisor VARCHAR(20)
    ,  Subcategoria2 VARCHAR(30)
    ,  SKU VARCHAR(20)
    ,  Minutos DECIMAL(18,2)
    ,  Codigo VARCHAR(20)
    ,  Falla VARCHAR(100)
    ,  Hora_Start VARCHAR(10)
    ,  Hora_Sort INT
    ,  Objectivo INT
    )

    --SELECT @date = DATEADD(DAY,-1,CAST(GETDATE() AS DATE))
    SELECT @date = CAST(GETDATE() AS DATE)

    -- EXEC GenerateAnalisisDeCambioRecords
     --set @date = '2022-12-13'

    DELETE FROM ChangeOver WHERE Date BETWEEN DATEADD(DAY,-2,@date) AND @date


    SELECT top 1 @firstLineID = id from Line;
    SELECT @lastLineID  = id from Line;

    set @rowLine = @firstLineID;

    WHILE @rowLine <= @lastLineID
    BEGIN

        TRUNCATE TABLE #changerecords

        SELECT @currentLine = Name FROM Line WHERE Id = (@rowLine)

        INSERT INTO #changerecords
        SELECT
            Date
        ,  Line
        ,  shift_
        ,  Supervisor
        ,  DowntimeSubCategory2
        ,  SKU
        ,  SUM(Minutes) AS Minutes
        ,  Code
        ,  Failure
        ,  HourStart
        ,  Sort
        ,  ObjectiveMinutes
        FROM vwChange
        WHERE Date BETWEEN DATEADD(DAY,-2,@date) AND @date AND Line = @currentLine
        GROUP BY Date, Line, shift_, Supervisor, SKU, Code, Failure, DowntimeSubCategory2, HourStart, Sort, ObjectiveMinutes
        ORDER BY Date, Code, Sort

        SELECT @row = 1, @rows = COUNT(*) FROM #changerecords

        SET @minutesTotal = 0

        WHILE @row <= @rows
        BEGIN

            SET @minutesP = 0
            set @minutes = 0

            SELECT @dateC = Fecha, @shiftC = Turno, @supervisorC = Supervisor, @category2C = Subcategoria2, @codeC = Codigo,
            @failureC = Falla, @hour_sortC = Hora_Sort, @objectiveC = Objectivo, @minutes = Minutos
            FROM #changerecords
            WHERE Id = @row

            SELECT @dateN = Fecha, @codeN = Codigo, @hour_sortN = Hora_Sort
            FROM #changerecords
            WHERE Id = @row + 1

            IF @hour_sortC = 1
            BEGIN

                SELECT @minutesP = Minutes, @dateP = Date, @hour_sortP = Hour_Sort
                FROM ChangeOver
                WHERE Date = DATEADD(DAY,-1,@dateC) AND Line = @currentLine AND Code = @codeC AND Hour_Sort = 24

                IF @minutesP > 0
                BEGIN
                    PRINT 'Prior day shift B hour 24 is the same cambio occurance, add minutes to this record and delete prior day record ROW: ' + CAST(@row AS VARCHAR)

                    --SELECT @minutesP, @dateP, @minutes, @hour_sortC, @hour_sortP

                    DELETE FROM ChangeOver WHERE Date = @dateP AND Line = @currentLine AND Code = @codeC AND Hour_Sort = 24 AND Minutes = @minutesP
                END
            END

            SET @minutesTotal = @minutesTotal + ISNULL(@minutes,0) + ISNULL(@minutesP,0)

            IF @dateC = @dateN AND @codeC = @codeN AND @hour_sortC = (@hour_sortN - 1)
            BEGIN
                PRINT 'Next record is same cambio occurance, skip this record ROW: ' + CAST(@row AS VARCHAR)
                --SELECT @dateC, @dateN, @lineC, @codeC, @codeN, @hour_sortC, @hour_sortN, @minutes, @minutesTotal, 'SKIP ROW'
            END
            ELSE
            BEGIN
                --SELECT @dateC, @dateN, @lineC,@codeC, @codeN, @hour_sortC, @hour_sortN, @minutes, @minutesTotal, 'INSERT ROW'

                INSERT INTO ChangeOver
                SELECT @dateC, @currentLine, @shiftC, @supervisorC, @category2C, @codeC, @failureC, @objectiveC, @minutesTotal, @hour_sortC

                SET @minutesTotal = 0
            END

            SET @row = @row + 1

        END

        SET @rowLine = @rowLine + 1

    END

GO

";

            string sqlCodeSPGenerateChangeOverRecordsHelper = @"
CREATE PROCEDURE [dbo].[GenerateChangeOverRecordsHelper] @date DATE
AS	

    DECLARE @end AS DATE
    DECLARE @row AS INT
    DECLARE @rows AS INT
    DECLARE @rowLine AS INT
    DECLARE @rowsLine AS INT
    DECLARE @dateC AS DATETIME
    DECLARE @dateN AS DATETIME
    DECLARE @dateP AS DATETIME
    DECLARE @minutes AS DECIMAL(18,2)
    DECLARE @minutesP AS DECIMAL(18,2)
    DECLARE @minutesTotal AS DECIMAL(18,2)
    DECLARE @codeC AS VARCHAR(20)
    DECLARE @codeN AS VARCHAR(20)
    DECLARE @codeP AS VARCHAR(20)
    DECLARE @lineC AS VARCHAR(15)
    DECLARE @shiftC AS VARCHAR(10)
    DECLARE @supervisorC AS VARCHAR(20)
    DECLARE @category2C AS VARCHAR(30)
    DECLARE @failureC AS VARCHAR(100)
    DECLARE @hour_sortC AS INT
    DECLARE @hour_sortN AS INT
    DECLARE @hour_sortP AS INT
    DECLARE @objectiveC AS INT
	DECLARE @lastLineID AS INT
	DECLARE @firstLineID AS INT
    DECLARE @currentLine AS VARCHAR(15)


    IF OBJECT_ID('tempdb..#changerecords') IS NOT NULL DROP TABLE #changerecords

    CREATE TABLE #changerecords (
        Id INT IDENTITY(1,1)
    ,  Fecha DATETIME
    ,  Linea VARCHAR(20)
    ,  Turno VARCHAR(10)
    ,  Supervisor VARCHAR(20)
    ,  Subcategoria2 VARCHAR(30)
    ,  SKU VARCHAR(20)
    ,  Minutos DECIMAL(18,2)
    ,  Codigo VARCHAR(20)
    ,  Falla VARCHAR(100)
    ,  Hora_Start VARCHAR(10)
    ,  Hora_Sort INT
    ,  Objectivo INT
    )
    
    DELETE FROM ChangeOver WHERE Date BETWEEN DATEADD(DAY,-2,@date) AND @date


    SELECT top 1 @firstLineID = id from Line;
    SELECT @lastLineID  = id from Line;

    set @rowLine = @firstLineID;

    WHILE @rowLine <= @lastLineID
    BEGIN

        TRUNCATE TABLE #changerecords

        SELECT @currentLine = Name FROM Line WHERE Id = (@rowLine)

        INSERT INTO #changerecords
        SELECT
            Date
        ,  Line
        ,  shift_
        ,  Supervisor
        ,  DowntimeSubCategory2
        ,  SKU
        ,  SUM(Minutes) AS Minutes
        ,  Code
        ,  Failure
        ,  HourStart
        ,  Sort
        ,  ObjectiveMinutes
        FROM vwChange
        WHERE Date BETWEEN DATEADD(DAY,-2,@date) AND @date AND Line = @currentLine
        GROUP BY Date, Line, shift_, Supervisor, SKU, Code, Failure, DowntimeSubCategory2, HourStart, Sort, ObjectiveMinutes
        ORDER BY Date, Code, Sort

        SELECT @row = 1, @rows = COUNT(*) FROM #changerecords

        SET @minutesTotal = 0

        WHILE @row <= @rows
        BEGIN

            SET @minutesP = 0
            set @minutes = 0

            SELECT @dateC = Fecha, @shiftC = Turno, @supervisorC = Supervisor, @category2C = Subcategoria2, @codeC = Codigo,
            @failureC = Falla, @hour_sortC = Hora_Sort, @objectiveC = Objectivo, @minutes = Minutos
            FROM #changerecords
            WHERE Id = @row

            SELECT @dateN = Fecha, @codeN = Codigo, @hour_sortN = Hora_Sort
            FROM #changerecords
            WHERE Id = @row + 1

            IF @hour_sortC = 1
            BEGIN

                SELECT @minutesP = Minutes, @dateP = Date, @hour_sortP = Hour_Sort
                FROM ChangeOver
                WHERE Date = DATEADD(DAY,-1,@dateC) AND Line = @currentLine AND Code = @codeC AND Hour_Sort = 24

                IF @minutesP > 0
                BEGIN
                    PRINT 'Prior day shift B hour 24 is the same cambio occurance, add minutes to this record and delete prior day record ROW: ' + CAST(@row AS VARCHAR)

                    --SELECT @minutesP, @dateP, @minutes, @hour_sortC, @hour_sortP

                    DELETE FROM ChangeOver WHERE Date = @dateP AND Line = @currentLine AND Code = @codeC AND Hour_Sort = 24 AND Minutes = @minutesP
                END
            END

            SET @minutesTotal = @minutesTotal + ISNULL(@minutes,0) + ISNULL(@minutesP,0)

            IF @dateC = @dateN AND @codeC = @codeN AND @hour_sortC = (@hour_sortN - 1)
            BEGIN
                PRINT 'Next record is same cambio occurance, skip this record ROW: ' + CAST(@row AS VARCHAR)
                --SELECT @dateC, @dateN, @lineC, @codeC, @codeN, @hour_sortC, @hour_sortN, @minutes, @minutesTotal, 'SKIP ROW'
            END
            ELSE
            BEGIN
                --SELECT @dateC, @dateN, @lineC,@codeC, @codeN, @hour_sortC, @hour_sortN, @minutes, @minutesTotal, 'INSERT ROW'

                INSERT INTO ChangeOver
                SELECT @dateC, @currentLine, @shiftC, @supervisorC, @category2C, @codeC, @failureC, @objectiveC, @minutesTotal, @hour_sortC

                SET @minutesTotal = 0
            END

            SET @row = @row + 1

        END

        SET @rowLine = @rowLine + 1

    END
GO

";

            string sqlCodeSPGetEfficiency = @"
CREATE PROCEDURE [dbo].[GetEfficiency] ( 
	@categories NVARCHAR(MAX),
	@startDate DATE,
	@endDate DATE
	)
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON

	-- Create table temporal categories
	CREATE TABLE #TempCategories (category NVARCHAR(255))

	-- Insert categories params
	INSERT INTO #TempCategories (category)
	SELECT value FROM string_split(@categories, ',')

	-- Line with one flow	
	-- Productivity
	SELECT pro.Id, pro.Date, l.Name AS Line, t.Name AS Shift, p.Sku, P.Flavour AS Flavor,
		   p.Packing, p.NetContent, s.Name AS Supervisor, pro.Production, 
		   p.StandardSpeed, pro.ScrapUnits AS Scrap
	INTO #Temp1
	FROM Productivity AS pro
	INNER JOIN Line AS l ON l.Id = pro.LineID
	INNER JOIN Shift AS t ON t.Id = pro.ShiftID
	INNER JOIN Product AS p ON p.Id = pro.ProductID
	INNER JOIN Supervisor AS s on s.Id = pro.SupervisorID
	WHERE pro.Date BETWEEN @startDate AND @endDate AND l.Flow = 1

	-- Downtime
	SELECT pro.Id, (SUM(dr.Minutes)/CAST(60 AS Decimal)) AS DownHrs, l.Name AS Line, p.Sku
	INTO #Temp2
	FROM Productivity AS pro
	INNER JOIN DowntimeReason AS dr ON dr.ProductivityID = pro.Id
	INNER JOIN DowntimeCategory AS dc ON dr.DowntimeCategoryId = dc.Id
	INNER JOIN Product AS p ON p.Id = pro.ProductID
	INNER JOIN Line AS l ON l.Id = pro.LineID
	WHERE pro.Date BETWEEN @startDate AND @endDate AND l.Flow = 1
			AND dc.Name IN (SELECT category FROM #TempCategories)
	GROUP BY pro.Id, l.Name, p.Sku

	-- Merge
	SELECT t1.Date, t1.Line, t1.Shift, t1.Sku, t1.Packing,
		   t1.NetContent, t1.Supervisor, SUM(t1.Production) AS Production,
		   t1.StandardSpeed, SUM(t1.Scrap) AS Scrap,
		   COUNT(t1.Id) AS Hrs, ISNULL(SUM(t2.DownHrs),0) AS DownHrs,
		   ((COUNT(t1.Id) - ISNULL(SUM(t2.DownHrs),0)) * t1.StandardSpeed) AS MaxProduction
	INTO #Temp3
	FROM #Temp1 AS t1
	LEFT JOIN #Temp2 AS t2 ON t2.Id = t1.Id
	GROUP BY t1.Date, t1.Line, t1.Shift, t1.Sku, t1.Packing, t1.NetContent,
			 t1.Supervisor, t1.StandardSpeed

	-- Line with two flow
	-- Same SKU
	-- Productivity
	SELECT pro.Id, pro.Date, l.Name AS Line, t.Name AS Shift, p.Sku, P.Flavour AS Flavor,
		   p.Packing, p.NetContent, s.Name AS Supervisor, pro.Production + pro.Production2 AS Production, 
		   p.StandardSpeed, pro.ScrapUnits + pro.ScrapUnits2 AS Scrap
	INTO #ProductivitySameSku
	FROM Productivity AS pro
	INNER JOIN Line AS l ON l.Id = pro.LineID
	INNER JOIN Shift AS t ON t.Id = pro.ShiftID
	INNER JOIN Product AS p ON p.Id = pro.ProductID
	INNER JOIN Supervisor AS s on s.Id = pro.SupervisorID
	WHERE 
		pro.Date BETWEEN @startDate AND @endDate 
		AND l.Flow = 2 
		AND (pro.ProductID = pro.ProductID2 OR (pro.ProductID = 0 OR pro.ProductID2 = 0))

	-- Downtime
	SELECT pro.Id, (SUM(dr.Minutes)/CAST(60 AS Decimal)) AS DownHrs, l.Name AS Line, p.Sku
	INTO #DowntimeSameSku
	FROM Productivity AS pro
	INNER JOIN DowntimeReason AS dr ON dr.ProductivityID = pro.Id
	INNER JOIN DowntimeCategory AS dc ON dr.DowntimeCategoryId = dc.Id
	INNER JOIN Product AS p ON p.Id = pro.ProductID
	INNER JOIN Line AS l ON l.Id = pro.LineID
	WHERE pro.Date BETWEEN @startDate AND @endDate 
			AND l.Flow = 2
			AND (pro.ProductID = pro.ProductID2 OR (pro.ProductID = 0 OR pro.ProductID2 = 0))
			AND dc.Name IN (SELECT category FROM #TempCategories)
	GROUP BY pro.Id, l.Name, p.Sku

	-- Merge
	SELECT t1.Date, t1.Line, t1.Shift, t1.Sku, t1.Packing,
		   t1.NetContent, t1.Supervisor, SUM(t1.Production) AS Production,
		   t1.StandardSpeed, SUM(t1.Scrap) AS Scrap,
		   COUNT(t1.Id) AS Hrs, ISNULL(SUM(t2.DownHrs),0) AS DownHrs,
		   ((COUNT(t1.Id) - ISNULL(SUM(t2.DownHrs),0)) * t1.StandardSpeed) AS MaxProduction
	INTO #MergeSameSku
	FROM #ProductivitySameSku AS t1
	LEFT JOIN #DowntimeSameSku AS t2 ON t2.Id = t1.Id
	GROUP BY t1.Date, t1.Line, t1.Shift, t1.Sku, t1.Packing, t1.NetContent,
			 t1.Supervisor, t1.StandardSpeed


	-- Different SKU
	-- Productivity Flow 1
	SELECT pro.Id, pro.Date, l.Name AS Line, t.Name AS Shift, p.Sku, P.Flavour AS Flavor,
		   p.Packing, p.NetContent, s.Name AS Supervisor, pro.Production, 
		   p.StandardSpeed, pro.ScrapUnits AS Scrap
	INTO #ProductivityFlow1
	FROM Productivity AS pro
	INNER JOIN Line AS l ON l.Id = pro.LineID
	INNER JOIN Shift AS t ON t.Id = pro.ShiftID
	INNER JOIN Product AS p ON p.Id = pro.ProductID
	INNER JOIN Supervisor AS s on s.Id = pro.SupervisorID
	WHERE 
		pro.Date BETWEEN @startDate AND @endDate 
		AND l.Flow = 2 
		AND pro.ProductID <> pro.ProductID2

	-- Downtime Flow 1
	SELECT pro.Id, (SUM(dr.Minutes)/CAST(60 AS Decimal)) AS DownHrs, l.Name AS Line, p.Sku
	INTO #DowntimeFlow1
	FROM Productivity AS pro
	INNER JOIN DowntimeReason AS dr ON dr.ProductivityID = pro.Id
	INNER JOIN DowntimeCategory AS dc ON dr.DowntimeCategoryId = dc.Id
	INNER JOIN Product AS p ON p.Id = pro.ProductID
	INNER JOIN Line AS l ON l.Id = pro.LineID
	WHERE pro.Date BETWEEN @startDate AND @endDate 
			AND l.Flow = 2
			AND pro.ProductID <> pro.ProductID2
			AND dr.FlowIndex = 1
			AND dc.Name IN (SELECT category FROM #TempCategories)
	GROUP BY pro.Id, l.Name, p.Sku

	-- Merge Flow 1
	SELECT t1.Date, t1.Line, t1.Shift, t1.Sku, t1.Packing,
		   t1.NetContent, t1.Supervisor, SUM(t1.Production) AS Production,
		   t1.StandardSpeed, SUM(t1.Scrap) AS Scrap,
		   COUNT(t1.Id) AS Hrs, ISNULL(SUM(t2.DownHrs),0) AS DownHrs,
		   ((COUNT(t1.Id) - ISNULL(SUM(t2.DownHrs),0)) * (t1.StandardSpeed/2)) AS MaxProduction
	INTO #MergeFlow1
	FROM #ProductivityFlow1 AS t1
	LEFT JOIN #DowntimeFlow1 AS t2 ON t2.Id = t1.Id
	GROUP BY t1.Date, t1.Line, t1.Shift, t1.Sku, t1.Packing, t1.NetContent,
			 t1.Supervisor, t1.StandardSpeed

	-- Productivity Flow 2
	SELECT pro.Id, pro.Date, l.Name AS Line, t.Name AS Shift, p.Sku, P.Flavour AS Flavor,
		   p.Packing, p.NetContent, s.Name AS Supervisor, pro.Production2 AS Production, 
		   p.StandardSpeed, pro.ScrapUnits2 AS Scrap
	INTO #ProductivityFlow2
	FROM Productivity AS pro
	INNER JOIN Line AS l ON l.Id = pro.LineID
	INNER JOIN Shift AS t ON t.Id = pro.ShiftID
	INNER JOIN Product AS p ON p.Id = pro.ProductID2
	INNER JOIN Supervisor AS s on s.Id = pro.SupervisorID
	WHERE 
		pro.Date BETWEEN @startDate AND @endDate 
		AND l.Flow = 2 
		AND pro.ProductID <> pro.ProductID2

	-- Downtime Flow 2
	SELECT pro.Id, (SUM(dr.Minutes)/CAST(60 AS Decimal)) AS DownHrs, l.Name AS Line, p.Sku
	INTO #DowntimeFlow2
	FROM Productivity AS pro
	INNER JOIN DowntimeReason AS dr ON dr.ProductivityID = pro.Id
	INNER JOIN DowntimeCategory AS dc ON dr.DowntimeCategoryId = dc.Id
	INNER JOIN Product AS p ON p.Id = pro.ProductID2
	INNER JOIN Line AS l ON l.Id = pro.LineID
	WHERE pro.Date BETWEEN @startDate AND @endDate 
			AND l.Flow = 2
			AND pro.ProductID <> pro.ProductID2
			AND dr.FlowIndex = 2
			AND dc.Name IN (SELECT category FROM #TempCategories)
	GROUP BY pro.Id, l.Name, p.Sku

	-- Merge Flow 2
	SELECT t1.Date, t1.Line, t1.Shift, t1.Sku, t1.Packing,
		   t1.NetContent, t1.Supervisor, SUM(t1.Production) AS Production,
		   t1.StandardSpeed, SUM(t1.Scrap) AS Scrap,
		   COUNT(t1.Id) AS Hrs, ISNULL(SUM(t2.DownHrs),0) AS DownHrs,
		   ((COUNT(t1.Id) - ISNULL(SUM(t2.DownHrs),0)) * (t1.StandardSpeed/2)) AS MaxProduction
	INTO #MergeFlow2
	FROM #ProductivityFlow2 AS t1
	LEFT JOIN #DowntimeFlow2 AS t2 ON t2.Id = t1.Id
	GROUP BY t1.Date, t1.Line, t1.Shift, t1.Sku, t1.Packing, t1.NetContent,
			 t1.Supervisor, t1.StandardSpeed

	--SELECT * FROM #DowntimeFlow1
	--SELECT * FROM #DowntimeFlow2

	SELECT *
	FROM (
		SELECT * FROM #Temp3
		UNION ALL
		SELECT * FROM #MergeSameSku
		UNION ALL
		SELECT * FROM #MergeFlow1
		UNION ALL
		SELECT * FROM #MergeFlow2
	) as t
END
GO
";

            string sqlCodeSPGetLineById = @"
CREATE PROCEDURE [dbo].[GetLineById]
    @LineId INT
AS
BEGIN
    SELECT *
    FROM Shift
    WHERE Id = @LineId;
END
GO

";

            string sqlCodeSPGetProductivitybyId = @"
CREATE PROCEDURE [dbo].[GetProductivitybyId]     
	@ProductivityId int
,	@ProductSku nvarchar(100)
AS
BEGIN
      
	SELECT 
	PC.Id, PC.ProductivityID, PC.DowntimeCodeId, PC.FlowIndex AS 'Flow', PC.Minutes, PC.DowntimeCategoryId, 
	DS.Name AS 'SubCategory1', DSS.Name AS 'SubCategory2',  CD.Code, CD.Failure
	FROM DowntimeReason PC
	JOIN Productivity PM ON pc.ProductivityID = pm.Id
	JOIN Product p ON pm.ProductID = p.Id
	JOIN DowntimeSubCategory1 DS ON PC.DowntimeSubCategory1Id = DS.Id
	JOIN DowntimeSubCategory2 DSS ON PC.DowntimeSubCategory2Id = DSS.Id
	LEFT JOIN DowntimeCode CD ON CD.Id = PC.DowntimeCodeId
	WHERE PC.FlowIndex = 1 AND PC.ProductivityID = @ProductivityId AND p.Sku = @ProductSku

	UNION ALL 

	SELECT 
	PC.Id, PC.ProductivityID, PC.DowntimeCodeId, PC.FlowIndex AS 'Flow', PC.Minutes, PC.DowntimeCategoryId, 
	DS.Name AS 'SubCategory1', DSS.Name AS 'SubCategory2',  CD.Code, CD.Failure
	FROM DowntimeReason PC
	JOIN Productivity PM ON pc.ProductivityID = pm.Id
	JOIN Product p ON pm.ProductID = p.Id
	JOIN DowntimeSubCategory1 DS ON PC.DowntimeSubCategory1Id = DS.Id
	JOIN DowntimeSubCategory2 DSS ON PC.DowntimeSubCategory2Id = DSS.Id
	LEFT JOIN DowntimeCode CD ON CD.Id = PC.DowntimeCodeId
	WHERE PC.FlowIndex = 2  AND PC.ProductivityID = @ProductivityId AND p.Sku = @ProductSku;

END
GO

";

            string sqlCodeSPGetReportEfficiency = @"
CREATE PROCEDURE [dbo].[GetReportEfficiency]
	@Date datetime
,	@Line int
,	@Shift int
,	@Category varchar(100)
AS
BEGIN
	
	DECLARE @sql_xml XML = Cast('<root><U>'+ Replace(@Category, ',', '</U><U>')+ '</U></root>' AS XML)

	IF OBJECT_ID('tempdb..#categories') IS NOT NULL DROP TABLE #categories;

	SELECT f.x.value('.', 'INT') AS category
	INTO #categories
	FROM @sql_xml.nodes('/root/U') f(x)

	SELECT *
	FROM (
		-- Line with one flow
		SELECT
			m.Date,
			l.Name AS 'Line',
			t.Name AS 'Shift',
			p.SKU,
			m.ProductID AS 'ProductId',
			SUM(ISNULL(m.Production,0)) AS 'Production',
			MIN(ISNULL(p.StandardSpeed,0)) AS 'StandardSpeed',
			MIN(l.Efficiency) AS 'Efficiency',
			COUNT(m.Id) AS 'Hrs',
			ISNULL(SUM(c.ChangeHrs),0) AS 'ChangeHrs',
			ISNULL(SUM(c.ChangeMins),0) AS 'ChangeMins',
			COUNT(m.Id) - ISNULL(SUM(c.ChangeHrs),0) AS 'NetHrs',
			(COUNT(m.Id) - (ISNULL(SUM(c.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed)  AS 'EffDEN',
			(COUNT(m.Id) - (ISNULL(SUM(c.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed)  AS 'EffDENSKU',
			SUM(ISNULL(m.ScrapUnits,0)) AS 'Scrap'
		FROM Productivity m
		INNER JOIN Line l ON l.Id = m.LineID
		INNER JOIN Shift t ON t.Id = m.ShiftID
		INNER JOIN Product p ON p.Id = m.ProductID
		INNER JOIN Supervisor su ON su.Id = m.SupervisorID
		LEFT JOIN (
			SELECT
				ProductivityID,
				SUM(dr.Minutes)/CAST(60 AS Decimal) ChangeHrs,
				SUM(dr.Minutes) ChangeMins
			FROM DowntimeReason dr
			INNER JOIN #categories cat ON dr.DowntimeCategoryId = cat.category
			GROUP BY ProductivityID
		) c ON c.ProductivityID = m.Id
		WHERE 
			l.Flow = 1
			AND m.Date = @Date
			AND m.LineID = @Line
			AND m.ShiftID = @Shift
		GROUP BY
			m.Date,
			l.Name,
			t.Name,
			p.Sku,
			m.ProductID

		UNION ALL

		-- Line with two flow
		-- Same SKU
		SELECT
			Date,
			l.Name AS 'Line',
			t.Name AS 'Shift',
			p.SKU,
			m.ProductID AS 'ProductId',
			SUM(ISNULL(m.Production,0) + ISNULL(m.Production2,0)) AS 'Production',
			MIN(ISNULL(p.StandardSpeed,0)) AS 'StandardSpeed',
			MIN(l.Efficiency) AS 'Efficiency',
			COUNT(m.Id) AS Hrs,
			ISNULL(SUM(c.ChangeHrs),0) AS 'ChangeHrs',
			ISNULL(SUM(c.ChangeMins),0) AS 'ChangeMins',
			COUNT(m.Id) - ISNULL(SUM(c.ChangeHrs),0) AS 'NetHrs',
			(COUNT(m.Id) - (ISNULL(SUM(c.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed)  AS 'EffDEN',
			(COUNT(m.Id) - (ISNULL(SUM(c.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed)  AS 'EffDENSKU',
			SUM(ISNULL(m.ScrapUnits,0) + ISNULL(m.ScrapUnits2,0)) AS 'Scrap'
		FROM Productivity m
		INNER JOIN Line l ON l.Id = m.LineID
		INNER JOIN Shift t ON t.Id = m.ShiftID
		INNER JOIN Product p ON p.Id = m.ProductID
		INNER JOIN Supervisor su ON su.Id = m.SupervisorID
		LEFT JOIN (
			SELECT
				ProductivityID,
				SUM(dr.Minutes)/CAST(60 AS Decimal) ChangeHrs,
				SUM(dr.Minutes) ChangeMins
			FROM DowntimeReason dr
			JOIN #categories cat ON dr.DowntimeCategoryId = cat.category
			GROUP BY ProductivityID
		) c ON c.ProductivityID = m.Id
		WHERE 
			l.Flow = 2
			AND ((m.ProductID = m.ProductID2) OR (m.ProductID = 0 OR m.ProductID2 = 0))
			AND m.Date = @Date
			AND m.LineID = @Line
			AND m.ShiftID = @Shift
		GROUP BY
			m.Date,
			l.Name,
			t.Name,
			p.Sku,
			m.ProductID

		UNION ALL

		-- Different SKU
		-- Flow 1
		SELECT
			Date,
			l.Name AS 'Line',
			t.Name AS 'Shift',
			p.SKU,
			m.ProductID AS 'ProductId',
			SUM(ISNULL(m.Production,0)) AS 'Production',
			MIN(ISNULL(p.StandardSpeed,0)) AS 'StandardSpeed',
			MIN(l.Efficiency) AS 'Efficiency',
			COUNT(m.Id) AS Hrs,
			ISNULL(SUM(c.ChangeHrs),0) AS 'ChangeHrs',
			ISNULL(SUM(c.ChangeMins),0) AS 'ChangeMins',
			COUNT(m.Id) - ISNULL(SUM(c.ChangeHrs),0) AS 'NetHrs',
			(COUNT(m.Id) - (ISNULL(SUM(c.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed/2)  AS 'EffDEN',
			(COUNT(m.Id) - (ISNULL(SUM(c.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed/2)  AS 'EffDENSKU',
			SUM(ISNULL(m.ScrapUnits,0)) AS 'Scrap'
		FROM Productivity m
		INNER JOIN Line l ON l.Id = m.LineID
		INNER JOIN Shift t ON t.Id = m.ShiftID
		INNER JOIN Product p ON p.Id = m.ProductID
		INNER JOIN Supervisor su ON su.Id = m.SupervisorID
		LEFT JOIN (
			SELECT
				ProductivityID,
				SUM(dr.Minutes)/CAST(60 AS Decimal) ChangeHrs,
				SUM(dr.Minutes) ChangeMins
			FROM DowntimeReason dr
			JOIN #categories cat ON dr.DowntimeCategoryId = cat.category
			WHERE dr.FlowIndex = 1
			GROUP BY ProductivityID
		) c ON c.ProductivityID = m.Id
		WHERE 
			l.Flow = 2
			AND ((m.ProductID <> m.ProductID2) AND (m.ProductID <> 0 AND m.ProductID2 <> 0))
			AND m.Date = @Date
			AND m.LineID = @Line
			AND m.ShiftID = @Shift
		GROUP BY
			m.Date,
			l.Name,
			t.Name,
			p.Sku,
			m.ProductID

		UNION ALL

		-- Flow 2
		SELECT
			Date,
			l.Name AS 'Line',
			t.Name AS 'Shift',
			p.SKU,
			m.ProductID AS 'ProductId',
			SUM(ISNULL(m.Production2,0)) AS 'Production',
			MIN(ISNULL(p.StandardSpeed,0)) AS 'StandardSpeed',
			MIN(l.Efficiency) AS 'Efficiency',
			COUNT(m.Id) AS Hrs,
			ISNULL(SUM(c.ChangeHrs),0) AS 'ChangeHrs',
			ISNULL(SUM(c.ChangeMins),0) AS 'ChangeMins',
			COUNT(m.Id) - ISNULL(SUM(c.ChangeHrs),0) AS 'NetHrs',
			(COUNT(m.Id) - (ISNULL(SUM(c.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed/2)  AS 'EffDEN',
			(COUNT(m.Id) - (ISNULL(SUM(c.ChangeMins),0) / CAST(60.00 AS DECIMAL))) * MIN(p.StandardSpeed/2)  AS 'EffDENSKU',
			SUM(ISNULL(m.ScrapUnits2,0)) AS 'Scrap'
		FROM Productivity m
		INNER JOIN Line l ON l.Id = m.LineID
		INNER JOIN Shift t ON t.Id = m.ShiftID
		INNER JOIN Product p ON p.Id = m.ProductID2
		INNER JOIN Supervisor su ON su.Id = m.SupervisorID
		LEFT JOIN (
			SELECT
				ProductivityID,
				SUM(dr.Minutes)/CAST(60 AS Decimal) ChangeHrs,
				SUM(dr.Minutes) ChangeMins
			FROM DowntimeReason dr
			JOIN #categories cat ON dr.DowntimeCategoryId = cat.category
			WHERE dr.FlowIndex = 2
			GROUP BY ProductivityID
		) c ON c.ProductivityID = m.Id
		WHERE 
			l.Flow = 2
			AND ((m.ProductID <> m.ProductID2) AND (m.ProductID <> 0 AND m.ProductID2 <> 0))
			AND m.Date = @Date
			AND m.LineID = @Line
			AND m.ShiftID = @Shift
		GROUP BY
			m.Date,
			l.Name,
			t.Name,
			p.Sku,
			m.ProductID
	) AS t
END
GO

";

            string sqlCodeSPGetReportProductivityList = @"
CREATE PROCEDURE [dbo].[GetReportProductivityList]        
	@Date datetime
,	@Line int
,	@Shift int
AS                  
BEGIN                  
     
	declare @linename varchar(50)= (select Name from Line where id = @Line)  
     
	declare @FlowlineName varchar(10) = (select top 1 name from Line where flow = 2)
	-- select @FlowlineName;

	IF @linename = @FlowlineName  
	BEGIN
		
		-- Flujo1 SKU same as Flujo2
		SELECT 
			PM.Id, CONVERT(varchar,PM.Date,101) AS 'Date', L.Name AS 'Line', T.Name AS 'Shift', --S.Descripcion as Supervisor, PM.nombre,
			PM.HourStart, PM.HourEnd, hr.Id AS 'HoursId', hr.Sort AS 'HourSort', CONCAT(PM.HourStart,'-' ,PM.HourEnd) AS 'Schedule', pro.SKU, 
			pro.StandardSpeed, pro.Flavour, pro.NetContent, pro.Packing, PM.Production + PM.Production2 AS 'Production',
			PM.RemainingProduction, PM.RemainingMinutes, PM.DowntimeMinutes, 
			ISNULL(PM.ScrapUnits,0) + ISNULL(PM.ScrapUnits2,0) AS 'ScrapUnits' , 1 AS 'Flow', PM.Comment AS 'Comment'
		FROM Productivity PM
		INNER JOIN Line L ON L.Id = PM.LineID
		INNER JOIN Shift T ON T.Id = PM.ShiftID
		INNER JOIN Product pro ON pro.Id = PM.ProductID
		INNER join Hour hr ON hr.Time = PM.HourStart
		WHERE
			Date = (SELECT CONVERT(date, @Date)) and PM.LineID = @Line and PM.ShiftID = @Shift AND PM.SKU = PM.SKU2

		UNION ALL

		-- Flujo1 and Flujo2 have different SKU's so calculate efficiency seperately
		-- Flujo1
		SELECT 
			PM.Id, CONVERT(varchar,PM.Date,101) AS 'Date', L.Name AS 'Line', T.Name AS 'Shift', --S.Descripcion as Supervisor, PM.nombre, 
			PM.HourStart, PM.HourEnd, hr.Id AS 'HoursId', hr.Sort AS 'HourSort', CONCAT(PM.HourStart,'-' ,PM.HourEnd) AS 'Schedule', pro.SKU,
			pro.StandardSpeed, pro.Flavour, pro.NetContent, pro.Packing, PM.Production,
			PM.RemainingProduction, PM.RemainingMinutes, PM.DowntimeMinutes, PM.ScrapUnits, 2 AS 'Flow', PM.Comment AS 'Comment'
		FROM Productivity PM             
		INNER JOIN Line L ON L.Id = PM.LineID
		INNER JOIN Shift T ON T.Id = PM.ShiftID
		INNER JOIN Product pro ON pro.Id = PM.ProductID
		INNER join Hour hr ON hr.Time = PM.HourStart
		WHERE 
			Date = (SELECT CONVERT(date, @Date)) and PM.LineID = @Line and PM.ShiftID = @Shift AND pm.SKU <> pm.SKU2
  
		UNION ALL

		-- Flujo2
		SELECT 
			PM.Id, CONVERT(varchar,PM.Date,101) AS 'Date', L.Name AS 'Line', T.Name AS 'Shift', --S.Descripcion as Supervisor, PM.nombre, 
			PM.HourStart, PM.HourEnd, hr.Id AS 'HoursId', hr.Sort AS 'HourSort', CONCAT(PM.HourStart,'-' ,PM.HourEnd) AS 'Schedule', pro.SKU,
			pro.StandardSpeed, pro.Flavour, pro.NetContent, pro.Packing, PM.Production2 as 'Production', 
			PM.RemainingProduction, PM.RemainingMinutes, PM.DowntimeMinutes, PM.ScrapUnits2 AS 'ScrapUnits', 2 AS 'Flow', PM.Comment AS 'Comment'
		FROM Productivity PM             
		INNER JOIN Line L ON L.Id = PM.LineID
		INNER JOIN Shift T ON T.Id = PM.ShiftID
		INNER JOIN Product pro ON pro.Id = PM.ProductID2
		INNER join Hour hr ON hr.Time = PM.HourStart
		WHERE 
			Date = (SELECT CONVERT(date, @Date)) and PM.LineID = @Line and PM.ShiftID = @Shift AND pm.SKU <> pm.SKU2
	END
	ELSE
	BEGIN
		SELECT 
			PM.Id, CONVERT(varchar,PM.Date,101) AS 'Date', L.Name AS 'Line', T.Name AS 'Shift', --S.Descripcion as Supervisor, PM.nombre, 
			PM.HourStart, PM.HourEnd, hr.Id AS 'HoursId', hr.Sort AS 'HourSort', CONCAT(PM.HourStart,'-' ,PM.HourEnd) AS 'Schedule', pro.SKU,
			pro.StandardSpeed, pro.Flavour, pro.NetContent, pro.Packing, PM.Production,
			PM.RemainingProduction, PM.RemainingMinutes, PM.DowntimeMinutes, PM.ScrapUnits, 1 AS 'Flow', PM.Comment AS 'Comment'
		FROM Productivity PM             
		INNER JOIN Line L ON L.Id = PM.LineID
		INNER JOIN Shift T ON T.Id = PM.ShiftID
		INNER JOIN Product pro ON pro.Id = PM.ProductID
		INNER join Hour hr ON hr.Time = PM.HourStart
		WHERE 
			Date = (SELECT CONVERT(date, @Date)) and PM.LineID = @Line and PM.ShiftID = @Shift 
	END              
END

GO

";

            string sqlCodeSPGetStats = @"
CREATE procedure [dbo].[GetStats]
  AS
  BEGIN
	DECLARE @prods int
	DECLARE @flows int
	DECLARE @reasons int
	DECLARE @products int
	DECLARE @codes int
	DECLARE @lines int
	DECLARE @shifts int
	DECLARE @supervisors int
	DECLARE @ef_migrations int

	select @prods = count(*) from Productivity;
	select @flows = count(*) from ProductivityFlow;
	select @reasons = count(*) from DowntimeReason;
	select @products = count(*) from Product;
	select @codes = count(*) from DowntimeCode;
	select @lines = count(*) from Line;
	select @shifts = count(*) from [Shift];
	select @supervisors = count(*) from Supervisor;
	select @ef_migrations = count(*) from __EFMigrationsHistory;

	select 
		@prods as Productivities, 
		@flows as Flows, 
		@reasons as Reasons, 
		@products as Products,
		@codes as Codes,
		@lines as Lines,
		@shifts as Shifts,
		@supervisors as Supervisors,
		@ef_migrations as EF_Migrations

  END
GO

";

            //             string sqlCodeSP = @"
            // ";

            // Views
            migrationBuilder.Sql(sqlCodeViewProductionSummary);
            migrationBuilder.Sql(sqlCodeViewProductivitySummary);
            migrationBuilder.Sql(sqlCodeViewvwDowntime);
            migrationBuilder.Sql(sqlCodeViewvwCases);
            migrationBuilder.Sql(sqlCodeViewvwChange);
            migrationBuilder.Sql(sqlCodeViewvwDailySummary);
            migrationBuilder.Sql(sqlCodeViewvwDowntimePerSku);
            migrationBuilder.Sql(sqlCodeViewvwDowntimeTrend);
            migrationBuilder.Sql(sqlCodeViewvwDowntimeXSubCat);
            migrationBuilder.Sql(sqlCodeViewvwGeneralEfficiency);
            migrationBuilder.Sql(sqlCodeViewvwPackageSku);
            migrationBuilder.Sql(sqlCodeViewvwProductivityReport);
            migrationBuilder.Sql(sqlCodeViewvwStatisticalChangeOver);
            migrationBuilder.Sql(sqlCodeViewvwSupervisorMetrics);
            migrationBuilder.Sql(sqlCodeViewvwUtilization);

            // Stored Procedures
            migrationBuilder.Sql(sqlCodeSPEfficiencyAnalysis);
            migrationBuilder.Sql(sqlCodeSPGenerateChangeOverRecords);
            migrationBuilder.Sql(sqlCodeSPGenerateChangeOverRecordsHelper);
            migrationBuilder.Sql(sqlCodeSPGetEfficiency);
            migrationBuilder.Sql(sqlCodeSPGetLineById);
            migrationBuilder.Sql(sqlCodeSPGetProductivitybyId);
            migrationBuilder.Sql(sqlCodeSPGetReportEfficiency);
            migrationBuilder.Sql(sqlCodeSPGetReportProductivityList);
            migrationBuilder.Sql(sqlCodeSPGetStats);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
