using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class AddViewProductivityWithDowntime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlVmProductivityWithDowntime = @"CREATE VIEW [dbo].[vwProductivityWithDowntime]
                AS                
                SELECT 
                    pro.Date,
                    l.Name,
                    p.Sku AS SKU,
                    pro.ProductID,
                    pro.LineID,
                    dr.DowntimeCategoryId,
                    dc.Name AS DowntimeCategoryName,
                    dr.FlowIndex,
                    dr.Minutes,
                    pro.HourStart,
                    pro.HourEnd
                FROM 
                    dbo.Productivity pro
                INNER JOIN 
                    dbo.Line l ON l.Id = pro.LineID
                INNER JOIN 
                    dbo.Product p ON p.Id = pro.ProductID
                LEFT JOIN 
                    dbo.DowntimeReason dr ON pro.Id = dr.ProductivityID
                LEFT JOIN 
                    dbo.DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
                
                UNION ALL
                
                -- Handle line with two flows (Flow 1)
                SELECT 
                    pro.Date,
                    l.Name,
                    p.Sku AS SKU,
                    pro.ProductID,
                    pro.LineID,
                    dr.DowntimeCategoryId,
                    dc.Name AS DowntimeCategoryName,
                    dr.FlowIndex,
                    dr.Minutes,
                    pro.HourStart,
                    pro.HourEnd
                FROM 
                    dbo.Productivity pro
                INNER JOIN 
                    dbo.Line l ON l.Id = pro.LineID
                INNER JOIN 
                    dbo.Product p ON p.Id = pro.ProductID
                LEFT JOIN 
                    dbo.DowntimeReason dr ON pro.Id = dr.ProductivityID AND dr.FlowIndex = 1
                LEFT JOIN 
                    dbo.DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
                WHERE 
                    l.Flow = 2
                
                UNION ALL
                
                -- Handle line with two flows (Flow 2)
                SELECT 
                    pro.Date,
                    l.Name + ' (2)' AS Name,
                    p2.Sku AS SKU,
                    pro.ProductID2 AS ProductID,
                    pro.LineID,
                    dr.DowntimeCategoryId,
                    dc.Name AS DowntimeCategoryName,
                    dr.FlowIndex,
                    dr.Minutes,
                    pro.HourStart,
                    pro.HourEnd
                FROM 
                    dbo.Productivity pro
                INNER JOIN 
                    dbo.Line l ON l.Id = pro.LineID
                INNER JOIN 
                    dbo.Product p2 ON p2.Id = pro.ProductID2
                LEFT JOIN 
                    dbo.DowntimeReason dr ON pro.Id = dr.ProductivityID AND dr.FlowIndex = 2
                LEFT JOIN 
                    dbo.DowntimeCategory dc ON dc.Id = dr.DowntimeCategoryId
                WHERE l.Flow = 2
                ";
            migrationBuilder.Sql(sqlVmProductivityWithDowntime);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
