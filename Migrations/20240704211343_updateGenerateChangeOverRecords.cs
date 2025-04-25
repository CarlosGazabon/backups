using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventio.Migrations
{
    /// <inheritdoc />
    public partial class updateGenerateChangeOverRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sqlCodeGenerateChangeOverRecords = @"
            ALTER PROCEDURE [dbo].[GenerateChangeOverRecords]
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
    DECLARE @codeC AS VARCHAR(MAX)
    DECLARE @codeN AS VARCHAR(MAX)
    DECLARE @codeP AS VARCHAR(MAX)
    DECLARE @lineC AS VARCHAR(MAX)
    DECLARE @shiftC AS VARCHAR(MAX)
    DECLARE @supervisorC AS VARCHAR(MAX)
    DECLARE @category2C AS VARCHAR(MAX)
    DECLARE @failureC AS VARCHAR(MAX)
    DECLARE @hour_sortC AS INT
    DECLARE @hour_sortN AS INT
    DECLARE @hour_sortP AS INT
    DECLARE @objectiveC AS INT
	DECLARE @lastLineID AS INT
	DECLARE @firstLineID AS INT
    DECLARE @currentLine AS VARCHAR(MAX)


    IF OBJECT_ID('tempdb..#changerecords') IS NOT NULL DROP TABLE #changerecords

    CREATE TABLE #changerecords (
        Id INT IDENTITY(1,1)
    ,  Fecha DATETIME
    ,  Linea VARCHAR(MAX)
    ,  Turno VARCHAR(MAX)
    ,  Supervisor VARCHAR(MAX)
    ,  Subcategoria2 VARCHAR(MAX)
    ,  SKU VARCHAR(MAX)
    ,  Minutos DECIMAL(18,2)
    ,  Codigo VARCHAR(MAX)
    ,  Falla VARCHAR(MAX)
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
";

            migrationBuilder.Sql(sqlCodeGenerateChangeOverRecords);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sqlCodeGenerateChangeOverRecords = @"
ALTER PROCEDURE [dbo].[GenerateChangeOverRecords]
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
";

            migrationBuilder.Sql(sqlCodeGenerateChangeOverRecords);
        }
    }
}
