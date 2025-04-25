using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class UpdateViewGeneralEfficiency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlVmGeneralEfficiency = @"ALTER VIEW [dbo].[vwGeneralEfficiency]
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
                    pro.HourStart,
                    pro.HourEnd,
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
                    pro.HourStart,
                    pro.HourEnd,
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
                    pro.HourStart,
                    pro.HourEnd,
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
                    pro.HourStart,
                    pro.HourEnd,
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
                    pro.HourStart,
                    pro.HourEnd,
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
                    pro.HourStart,
                    pro.HourEnd,
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
                    pro.HourStart,
                    pro.HourEnd,
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
                    pro.HourStart,
                    pro.HourEnd,
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
            migrationBuilder.Sql(sqlVmGeneralEfficiency);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
