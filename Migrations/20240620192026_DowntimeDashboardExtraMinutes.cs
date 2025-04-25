using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class DowntimeDashboardExtraMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlVmDowntime = @"ALTER VIEW [dbo].[vwDowntime] 
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
                ,   c.ExtraMinutes
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
                ,   c.ExtraMinutes
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
            string sqlVwDowntimeXSubCat = @"ALTER VIEW [dbo].[vwDowntimeXSubCat] 
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
,   c.ExtraMinutes
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
,   c.ExtraMinutes
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
            string sqlVwDowntimeTrend = @"ALTER VIEW [dbo].[vwDowntimeTrend] 
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
,   c.ExtraMinutes
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
,   c.ExtraMinutes
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
GO";
            string sqlVwDowntimePerSku = @"ALTER VIEW [dbo].[vwDowntimePerSku] 
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
                        dr.ExtraMinutes,
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
                        dr.ExtraMinutes,
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
                GO";

            migrationBuilder.Sql(sqlVmDowntime);
            migrationBuilder.Sql(sqlVwDowntimeXSubCat);
            migrationBuilder.Sql(sqlVwDowntimeTrend);
            migrationBuilder.Sql(sqlVwDowntimePerSku);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
