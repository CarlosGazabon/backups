using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class UpdateViewStatisticalChangeOver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlVmStatisticalChangeOver = @"ALTER VIEW [dbo].[vwStatisticalChangeOver] AS
                        SELECT
                            m.Date
                        ,	s.Id AS 'SupervisorId'
                        ,	s.Description AS 'Supervisor'
                        ,   s.Name AS 'SupervisorName'
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
                        ,   s.Name AS 'SupervisorName'
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
            migrationBuilder.Sql(sqlVmStatisticalChangeOver);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
