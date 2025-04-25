using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class UpdateViewUtilization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlVmUtilization = @"ALTER VIEW [dbo].[vwUtilization] AS
                    SELECT * FROM (
                        -- Line with one flow
                        SELECT
                            pro.Date,
                            l.Name,
                            p.Sku SKU,
                            pro.ProductID ProductId,
                            pro.LineID as LineId,  -- Añadido LineId
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
                            pro.ProductID,
                            pro.LineID  -- Añadido LineId

                        UNION ALL

                        -- Line with two flow
                        -- Flow 1
                        SELECT
                            pro.Date,
                            l.Name,
                            p.Sku SKU,
                            pro.ProductID ProductId,
                            pro.LineID as LineId,  -- Añadido LineId
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
                            pro.ProductID,
                            pro.LineID  -- Añadido LineId

                        UNION ALL

                        -- Line with two flow
                        -- Flow 2
                        SELECT
                            pro.Date,
                            l.Name + ' (2)' Name,
                            p.Sku SKU,
                            pro.ProductID2 ProductId,
                            pro.LineID as LineId,  -- Añadido LineId
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
                            pro.ProductID2,
                            pro.LineID  -- Añadido LineId
                    ) as t
                    GO
                    ";
            migrationBuilder.Sql(sqlVmUtilization);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
