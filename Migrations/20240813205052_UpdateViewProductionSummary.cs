using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class UpdateViewProductionSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlVmProductionSummary = @"ALTER VIEW [dbo].[ProductionSummary]
                AS
                    SELECT *
                    FROM (
                        -- Line with one flow
                        SELECT 
                            pro.Date,
                            l.Id AS LineID, 
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
                            l.Id, 
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
                            l.Id AS LineID, 
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
                            l.Id, 
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
                            l.Id AS LineID, 
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
                            l.Id,
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
                            l.Id AS LineID,
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
                            l.Id,
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
            migrationBuilder.Sql(sqlVmProductionSummary);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
