using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class AddSPGetDataEfficiency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlCodeSPGetDataEfficiency = @"
CREATE PROCEDURE [dbo].[GetDataEfficiency]
    (
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
    CREATE TABLE #TempCategories
    (
        category INT
    )

    -- Insert categories params
    INSERT INTO #TempCategories
        (category)
    SELECT value
    FROM string_split(@categories, ',')

    SELECT *
    FROM (
	-- Line with one flow
            SELECT
                pro.Date,
                l.Name AS Line,
                l.Id AS LineId,
                s.Id AS SupervisorId,
                s.Name AS Supervisor,
                t.Name AS Shift,
                t.id AS ShiftId,
                p.Id ProductId,
                p.Sku,
                P.Flavour AS Flavor,
                p.Packing,
                p.NetContent,
                1 Flow,
                ISNULL(SUM(pro.Production),0) AS Production,
                MIN(ISNULL(p.StandardSpeed,0)) AS StandardSpeed,
                SUM(ISNULL(pro.ScrapUnits,0)) AS Scrap,
                COUNT(pro.Id) Hrs,
                ISNULL(SUM(d.DownHrs),0) AS DownHrs,
                COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0) AS NetHrs,
                ((COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0)) * (p.StandardSpeed)) AS MaxProduction
            FROM Productivity AS pro
                INNER JOIN Line AS l ON l.Id = pro.LineID
                INNER JOIN Shift AS t ON t.Id = pro.ShiftID
                INNER JOIN Product AS p ON p.Id = pro.ProductID
                INNER JOIN Supervisor AS s ON s.Id = pro.SupervisorID
                LEFT JOIN (
			SELECT
                    dr.ProductivityID,
                    SUM(ISNULL(dr.Minutes,0))/CAST(60 AS Decimal) DownHrs
                FROM DowntimeReason AS dr
                WHERE dr.DowntimeCategoryId IN (SELECT category
                FROM #TempCategories)
                GROUP BY ProductivityID
		) AS d ON d.ProductivityID = pro.Id
            WHERE
			l.Flow = 1
                AND pro.Date BETWEEN @startDate AND @endDate
            GROUP BY
			pro.Date,
			l.Name,
            s.Id,
            s.Name,
			t.Name,
            l.Id,
            t.Id,
			p.Sku,
            p.Id,
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
                l.Id AS LineId,
                s.Id AS SupervisorId,
                s.Name AS Supervisor,
                t.Name AS Shift,
                t.id AS ShiftId,
                p.Id ProductId,
                p.Sku,
                P.Flavour AS Flavor,
                p.Packing,
                p.NetContent,
                1 Flow,
                SUM(ISNULL(pro.Production,0) + ISNULL(pro.Production2,0)) AS Production,
                MIN(ISNULL(p.StandardSpeed,0)) AS StandardSpeed,
                SUM(ISNULL(pro.ScrapUnits,0) + ISNULL(pro.ScrapUnits2,0)) AS Scrap,
                COUNT(pro.Id) Hrs,
                ISNULL(SUM(d.DownHrs),0) AS DownHrs,
                COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0) AS NetHrs,
                ((COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0)) * (p.StandardSpeed)) AS MaxProduction
            FROM Productivity AS pro
                INNER JOIN Line AS l ON l.Id = pro.LineID
                INNER JOIN Shift AS t ON t.Id = pro.ShiftID
                INNER JOIN Product AS p ON p.Id = pro.ProductID
                INNER JOIN Supervisor AS s ON s.Id = pro.SupervisorID
                LEFT JOIN (
			SELECT
                    dr.ProductivityID,
                    SUM(ISNULL(dr.Minutes,0))/CAST(60 AS Decimal) AS DownHrs
                FROM DowntimeReason AS dr
                WHERE dr.DowntimeCategoryId IN (SELECT category
                FROM #TempCategories)
                GROUP BY ProductivityID
		) AS d ON d.ProductivityID = pro.Id
            WHERE
			l.Flow = 2
                AND ((pro.ProductID = pro.ProductID2) OR (pro.ProductID = 0 OR pro.ProductID2 = 0))
                AND pro.Date BETWEEN @startDate AND @endDate
            GROUP BY
			pro.Date,
			l.Name,
            s.Id,
            s.Name,
			t.Name,
            l.Id,
            t.Id,
            p.Id,
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
                l.Id AS LineId,
                s.Id AS SupervisorId,
                s.Name AS Supervisor,
                t.Name AS Shift,
                t.id AS ShiftId,
                p.Id ProductId,
                p.Sku,
                P.Flavour AS Flavor,
                p.Packing,
                p.NetContent,
                1 Flow,
                SUM(ISNULL(pro.Production,0)) AS Production,
                MIN(ISNULL(p.StandardSpeed,0)) AS StandardSpeed,
                SUM(ISNULL(pro.ScrapUnits,0)) AS Scrap,
                COUNT(pro.Id) AS Hrs,
                ISNULL(SUM(d.DownHrs),0) AS DownHrs,
                COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0) AS NetHrs,
                ((COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0)) * (p.StandardSpeed/2)) AS MaxProduction
            FROM Productivity AS pro
                INNER JOIN Line AS l ON l.Id = pro.LineID
                INNER JOIN Shift AS t ON t.Id = pro.ShiftID
                INNER JOIN Product AS p ON p.Id = pro.ProductID
                INNER JOIN Supervisor AS s ON s.Id = pro.SupervisorID
                LEFT JOIN (
			SELECT
                    dr.ProductivityID,
                    SUM(ISNULL(dr.Minutes,0))/CAST(60 AS Decimal) AS DownHrs
                FROM DowntimeReason AS dr
                WHERE dr.DowntimeCategoryId IN (SELECT category
                    FROM #TempCategories)
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
            s.Id,
            s.Name,
			t.Name,
            l.Id,
            t.Id,
            p.Id,
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
                l.Id AS LineId,
                s.Id AS SupervisorId,
                s.Name AS Supervisor,
                t.Name AS Shift,
                t.id AS ShiftId,
                p.Id ProductId,
                p.Sku,
                P.Flavour AS Flavor,
                p.Packing,
                p.NetContent,
                2 Flow,
                SUM(ISNULL(pro.Production2,0)) AS Production,
                MIN(ISNULL(p.StandardSpeed,0)) AS StandardSpeed,
                SUM(ISNULL(pro.ScrapUnits,0)) AS Scrap,
                COUNT(pro.Id) AS Hrs,
                ISNULL(SUM(d.DownHrs),0) AS DownHrs,
                COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0) AS NetHrs,
                ((COUNT(pro.Id) - ISNULL(SUM(d.DownHrs),0)) * (p.StandardSpeed/2)) AS MaxProduction
            FROM Productivity AS pro
                INNER JOIN Line AS l ON l.Id = pro.LineID
                INNER JOIN Shift AS t ON t.Id = pro.ShiftID
                INNER JOIN Product AS p ON p.Id = pro.ProductID2
                INNER JOIN Supervisor AS s ON s.Id = pro.SupervisorID
                LEFT JOIN (
			SELECT
                    dr.ProductivityID,
                    SUM(ISNULL(dr.Minutes,0))/CAST(60 AS Decimal) DownHrs
                FROM DowntimeReason AS dr
                WHERE dr.DowntimeCategoryId IN (SELECT category
                    FROM #TempCategories)
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
            s.Id,
            s.Name,
			t.Name,
            l.Id,
            t.Id,
            p.Id,
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

            migrationBuilder.Sql(sqlCodeSPGetDataEfficiency);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sqlCodeDropSPGetDataEfficiency = @"
            DROP PROCEDURE [dbo].[GetDataEfficiency]";

            migrationBuilder.Sql(sqlCodeDropSPGetDataEfficiency);
        }
    }
}
