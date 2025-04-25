using System.Data;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using inventio.Models.DTO.Report;
using inventio.Repositories.Reports.SummaryReport.Objects;
using Inventio.Models.Views;
using System.Globalization;
using System.Configuration;


namespace inventio.Repositories.Reports.SummaryReport
{
    public class SummaryReport : ISummaryReport
    {

        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;
        private IWebHostEnvironment Environment;

        public SummaryReport(ApplicationDBContext context, IConfiguration configuration, IWebHostEnvironment _environment)
        {
            _context = context; ;
            _configuration = configuration;
            Environment = _environment;
        }

        private void PrintDivider(Document document, string text)
        {
            // Header
            Paragraph header = new Paragraph(text)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(10)
               .SetBackgroundColor(ColorConstants.BLUE)
               .SetFontColor(ColorConstants.WHITE)
               .SetPaddingLeft(5);

            document.Add(header);
        }
        private void PrintLogo(Document document)
        {
            string wwwPath = this.Environment.WebRootPath;
            string imagePath = string.Concat(wwwPath, _configuration["Tenant:ImagePath"]) ?? throw new SettingsPropertyNotFoundException();

            Image img = new Image(ImageDataFactory
               .Create(imagePath))
               .SetTextAlignment(TextAlignment.RIGHT);

            var x = PageSize.A4.GetWidth() - img.GetImageWidth() - 51;
            var y = PageSize.A4.GetHeight() - img.GetImageHeight() - 80;
            img.SetFixedPosition(x, y);

            document.Add(img);
        }
        private void PrintHeader(Document document, ReportHeader reportBody)
        {

            ReportHeader reportHeaderTitles = new()
            {
                StartDate = "START DATE",
                EndDate = "END DATE",
                Line = "LINES",
            };


            Table table = new Table(2, false);
            table.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

            foreach (var prop in typeof(ReportHeader).GetProperties())
            {
                // Get the name and value of each ReportHeader property
                object propValueTitle = prop.GetValue(reportHeaderTitles) ?? "";
                object propValueBody = prop.GetValue(reportBody) ?? "";

                // Crear celdas para la clave y el valor
                Cell valueCellTitle = CreateMainCell(propValueTitle.ToString()!, TextAlignment.LEFT);
                Cell valueCellBody = CreateCell(propValueBody.ToString()!, TextAlignment.LEFT, false);

                // Add the cells to the table
                table.AddCell(valueCellTitle);
                table.AddCell(valueCellBody);
            }


            document.Add(table);
        }

        private void PrintEfficiencyPerLine(Document document, List<ProductionSummary> productionList)
        {
            var productionData = productionList
                .GroupBy(ps => new { ps.Line, ps.Flow })
                .Select(g => new
                {
                    Line = g.Key.Line,
                    Efficiency = g.Sum(ps => ps.Production) / g.Sum(ps => ps.EffDen),
                }).ToList();

            var lines = productionData.Select(pd => pd.Line).Distinct().OrderBy(line => line).ToList();

            Table efficiencyTable = new Table(lines.Count + 1, false);
            efficiencyTable.SetWidth(UnitValue.CreatePercentValue(100));

            foreach (var line in lines)
            {
                efficiencyTable.AddHeaderCell(CreateMainCell($"{line}", TextAlignment.CENTER));
            }

            foreach (var line in lines)
            {
                var efficiency = productionData.FirstOrDefault(pd => pd.Line == line)?.Efficiency ?? 0;
                string formattedEfficiency = (efficiency * 100).ToString("0.00", CultureInfo.InvariantCulture) + " %";
                efficiencyTable.AddCell(CreateCell(formattedEfficiency, TextAlignment.CENTER, true));
            }

            document.Add(efficiencyTable);
        }

        private void PrintSummaryResults(Document document, List<ProductionSummary> productionList, List<VwDowntime> downtimeList)
        {
            var productionData = productionList
                .GroupBy(ps => new { ps.Line, ps.Flow })
                .Select(g => new
                {
                    Line = g.Key.Line,
                    Flow = g.Key.Flow,
                    Production = g.Sum(ps => ps.Production),
                    ProductionHrs = g.Sum(ps => ps.NetHrs),
                    ChangeHrs = g.Sum(ps => ps.ChangeHrs)
                }).ToList();

            var downtimeData = downtimeList
                .GroupBy(ds => new { ds.Line, ds.FlowIndex })
                .Select(g => new
                {
                    Line = g.Key.Line,
                    Flow = g.Key.FlowIndex,
                    DowntimeHrs = g.Sum(ds => ds.Hours)
                }).ToList();

            var summaryData = (from p in productionData
                               join d in downtimeData on new { p.Line, p.Flow } equals new { d.Line, d.Flow } into pd
                               from d in pd.DefaultIfEmpty()
                               select new SummaryResults
                               {
                                   Line = p.Line,
                                   Flow = p.Flow,
                                   Production = p.Production,
                                   ProductionHrs = p.ProductionHrs,
                                   ChangeOverHrs = p.ChangeHrs,
                                   DowntimeHrs = d != null ? d.DowntimeHrs : 0
                               }).ToList();


            SummaryResultsHeader productionTableHeader = new()
            {
                Line = "LINE",
                Flow = "FLOW",
                Production = "SALES UNITS",
                ProductionHrs = "PRODUCTION HRS",
                DowntimeHrs = "DOWNTIME HRS",
                ChangeOverHrs = "CHANGE OVER HRS",
            };

            Table summaryTable = new Table(6, false);
            summaryTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Create the header row in correct order
            foreach (var prop in typeof(SummaryResultsHeader).GetProperties())
            {
                object propValue = prop.GetValue(productionTableHeader)!;

                Cell cell = CreateMainCell($"{propValue ?? ""}", TextAlignment.CENTER);

                summaryTable.AddCell(cell);
            }

            bool condition = true;
            decimal totalProduction = 0, totalProductionHrs = 0, totalDowntimeHrs = 0, totalChangeOverHrs = 0;

            foreach (var summaryItem in summaryData)
            {
                totalProduction += summaryItem.Production.GetValueOrDefault(0);
                totalProductionHrs += summaryItem.ProductionHrs.GetValueOrDefault(0);
                totalDowntimeHrs += summaryItem.DowntimeHrs.GetValueOrDefault(0);
                totalChangeOverHrs += summaryItem.ChangeOverHrs.GetValueOrDefault(0);

                string formattedProduction = summaryItem.Production.GetValueOrDefault(0).ToString("N0", new CultureInfo("en-US"));

                // Add data cells in the correct order
                summaryTable.AddCell(CreateCell(summaryItem.Line, TextAlignment.CENTER, condition));
                summaryTable.AddCell(CreateCell(summaryItem.Flow.ToString(), TextAlignment.CENTER, condition));
                summaryTable.AddCell(CreateCell(formattedProduction, TextAlignment.CENTER, condition));
                summaryTable.AddCell(CreateCell(Math.Round(summaryItem.ProductionHrs.GetValueOrDefault(0), 0).ToString(), TextAlignment.CENTER, condition));
                summaryTable.AddCell(CreateCell(Math.Round(summaryItem.DowntimeHrs.GetValueOrDefault(0), 0).ToString(), TextAlignment.CENTER, condition));
                summaryTable.AddCell(CreateCell(Math.Round(summaryItem.ChangeOverHrs.GetValueOrDefault(0), 0).ToString(), TextAlignment.CENTER, condition));

                condition = !condition;
            }

            // ADD TOTALS
            summaryTable.AddCell(CreateMainCell("TOTALS", TextAlignment.CENTER));
            summaryTable.AddCell(CreateMainCell("", TextAlignment.CENTER)); // Empty cell for Flow column in totals row
            string formattedTotalProduction = totalProduction.ToString("N0", new CultureInfo("en-US"));

            summaryTable.AddCell(CreateMainCell(formattedTotalProduction, TextAlignment.CENTER));
            summaryTable.AddCell(CreateMainCell(Math.Round(totalProductionHrs, 0).ToString(), TextAlignment.CENTER));
            summaryTable.AddCell(CreateMainCell(Math.Round(totalDowntimeHrs, 0).ToString(), TextAlignment.CENTER));
            summaryTable.AddCell(CreateMainCell(Math.Round(totalChangeOverHrs, 0).ToString(), TextAlignment.CENTER));

            document.Add(summaryTable);
        }


        private void DowntimePerCategory(Document document, List<VwDowntime> downtimeList)
        {
            var downtimeData = downtimeList
            .GroupBy(ds => new { ds.Line, ds.Category })
            .Select(g => new
            {
                Line = g.Key.Line,
                Category = g.Key.Category,
                DowntimeHrs = g.Sum(ds => ds.Hours)
            });

            // retrieve all the distinct categories
            var categories = downtimeData.Select(d => d.Category).Distinct().OrderBy(c => c).ToList();

            // retrieve all the distinct lines to loop them
            var lines = downtimeData.Select(d => d.Line).Distinct().OrderBy(l => l);

            //* -------------------------------------------------------------------------- 
            //*                                    Print Table                                 
            //* -------------------------------------------------------------------------- 

            Table summaryTable = new Table(categories.Count + 2, false);
            summaryTable.SetWidth(UnitValue.CreatePercentValue(100));

            Boolean condition = true;

            //* --------------------------------- HEADERS -------------------------------- 

            Cell lineHeader = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBold()
                .Add(new Paragraph("LINE"));

            summaryTable.AddCell(lineHeader);

            foreach (var category in categories)
            {
                Cell categoryHeader = CreateCell(category, TextAlignment.CENTER, false).SetBold();
                summaryTable.AddCell(categoryHeader);
            }

            Cell totalsHeader = CreateCell("TOTALS", TextAlignment.CENTER, false).SetBold();


            summaryTable.AddCell(totalsHeader);

            //* --------------------------------- BODY -------------------------------- 

            // save the totals per category
            decimal[] categoryTotals = new decimal[categories.Count];

            foreach (var line in lines)
            {

                summaryTable.AddCell(CreateCell(line, TextAlignment.CENTER, condition));
                decimal lineTotal = 0;

                for (int i = 0; i < categories.Count; i++)
                {
                    var category = categories[i];
                    var hours = downtimeData.FirstOrDefault(d => d.Line == line && d.Category == category)?.DowntimeHrs ?? 0;
                    string hoursString = hours == 0 ? "" : Math.Round(hours, 2).ToString();
                    summaryTable.AddCell(CreateCell(hoursString, TextAlignment.CENTER, condition));
                    lineTotal += hours;
                    categoryTotals[i] += hours;
                }

                summaryTable.AddCell(CreateCell(Math.Round(lineTotal, 2).ToString(), TextAlignment.CENTER, condition));

                condition = !condition;
            }

            //* ------------------------- ADD THE LAST ROW OF TOTALS ------------------------- */
            summaryTable.AddCell(CreateMainCell("TOTALS", TextAlignment.CENTER));

            foreach (var total in categoryTotals)
            {
                summaryTable.AddCell(CreateMainCell(Math.Round(total, 2).ToString(), TextAlignment.CENTER));
            }

            // ADD THE TOTALS OF TOTALS
            var grandTotal = categoryTotals.Sum();
            summaryTable.AddCell(CreateMainCell(Math.Round(grandTotal, 2).ToString(), TextAlignment.CENTER));

            document.Add(summaryTable);

        }
        private void PrintProductionPerPkg(Document document, List<ProductionSummary> productionList)
        {
            var productionData = productionList
                .GroupBy(pd => new { pd.Line, pd.Flavour, pd.Sku, pd.Packing })
                .Select(g => new
                {
                    Line = g.Key.Line,
                    Flavour = g.Key.Flavour,
                    SKU = g.Key.Sku,
                    Packing = g.Key.Packing,
                    Production = g.Sum(ds => ds.Production)
                }).ToList();

            // filter all distinct packings and lines
            var packings = productionData.Select(pd => pd.Packing).Distinct().OrderBy(p => p).ToList();
            var lines = productionData.Select(pd => pd.Line).Distinct().OrderBy(l => l).ToList();



            //* -------------------------------------------------------------------------- 
            //*                                    Print Table                                 
            //* -------------------------------------------------------------------------- 

            // Create table
            Table productionTable = new Table(packings.Count + 4, false);
            productionTable.SetWidth(UnitValue.CreatePercentValue(100));


            //* --------------------------------- HEADERS -------------------------------- 

            // ADD FIXED HEADERS
            Cell lineHeader = CreateMainCell("LINE", TextAlignment.CENTER);
            productionTable.AddCell(lineHeader);

            Cell flavourHeader = CreateMainCell("FLAVOUR", TextAlignment.CENTER);
            productionTable.AddCell(flavourHeader);

            Cell skuHeader = CreateMainCell("SKU", TextAlignment.CENTER);
            productionTable.AddCell(skuHeader);

            // Add dynamic headers
            foreach (var packing in packings)
            {

                Cell packingHeader = CreateMainCell(packing, TextAlignment.CENTER);
                productionTable.AddCell(packingHeader);
            }

            // Add "Totals" header
            Cell totalsHeader = CreateMainCell("TOTALS", TextAlignment.CENTER);
            productionTable.AddCell(totalsHeader);


            //* --------------------------------- BODY -------------------------------- 
            // add body rows
            Boolean condition = true;
            // List to store the totals for each packing
            var packingTotals = new Dictionary<string, decimal>();
            decimal totalOfTotals = 0;

            foreach (var line in lines)
            {
                var lineData = productionData.Where(pd => pd.Line == line).ToList();


                foreach (var data in lineData)
                {
                    productionTable.AddCell(CreateCell(line, TextAlignment.CENTER, condition));
                    productionTable.AddCell(CreateCell(data.Flavour, TextAlignment.CENTER, condition));
                    productionTable.AddCell(CreateCell(data.SKU, TextAlignment.CENTER, condition));

                    decimal lineTotal = 0;

                    foreach (var packing in packings)
                    {
                        var production = lineData.FirstOrDefault(d => d.Packing == packing && d.SKU == data.SKU)?.Production ?? 0;
                        string formattedProduction = production.ToString("N0", new CultureInfo("en-US"));
                        if (data.Packing == packing)
                        {
                            productionTable.AddCell(CreateCell(formattedProduction, TextAlignment.CENTER, condition));
                            lineTotal += production;
                        }
                        else
                        {
                            productionTable.AddCell(CreateCell("", TextAlignment.CENTER, condition));
                        }

                        if (!packingTotals.ContainsKey(packing))
                            packingTotals[packing] = production;
                        else
                            packingTotals[packing] += production;
                    }

                    // Add cell with total production for this line
                    string formattedLineTotal = lineTotal.ToString("N0", new CultureInfo("en-US"));
                    productionTable.AddCell(CreateCell(formattedLineTotal, TextAlignment.CENTER, condition));
                    totalOfTotals += lineTotal;
                    condition = !condition;
                }
            }

            //* --------------------------------- TOTALS --------------------------------- 
            // Add row for packing totals
            productionTable.AddCell(new Cell(1, 3)
            .SetTextAlignment(TextAlignment.LEFT)
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
            .SetBold()
            .Add(new Paragraph("TOTALS")));

            foreach (var packing in packings)
            {
                var total = packingTotals.ContainsKey(packing) ? packingTotals[packing] : 0;
                string formattedTotal = total.ToString("N0", new CultureInfo("en-US"));

                productionTable.AddCell(CreateMainCell(formattedTotal, TextAlignment.CENTER));
            }

            // Calculate and add grand total
            string formattedTotalOfTotals = totalOfTotals.ToString("N0", new CultureInfo("en-US"));
            productionTable.AddCell(CreateMainCell(formattedTotalOfTotals, TextAlignment.CENTER));

            document.Add(productionTable);
        }

        private void PrintProductionPerShift(Document document, List<ProductionSummary> productionList)
        {
            var productionData = productionList
                .GroupBy(pd => new { pd.Line, pd.Shift })
                .Select(g => new
                {
                    Line = g.Key.Line,
                    Shift = g.Key.Shift,
                    Production = g.Sum(ds => ds.Production)
                }).ToList();

            // Retrieve all the distinct shifts
            var shifts = productionData.Select(p => p.Shift).Distinct().OrderBy(s => s).ToList();

            // Retrieve all the distinct lines to loop them
            var lines = productionData.Select(p => p.Line).Distinct().OrderBy(l => l);

            //* --------------------------------------------------------------------------
            //*                                    Print Table
            //* --------------------------------------------------------------------------

            Table summaryTable = new Table(shifts.Count + 2, false);
            summaryTable.SetWidth(UnitValue.CreatePercentValue(100));

            Boolean condition = true;


            //* --------------------------------- HEADERS --------------------------------

            Cell lineHeader = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBold()
                .Add(new Paragraph("LINE"));

            summaryTable.AddCell(lineHeader);

            foreach (var shift in shifts)
            {
                Cell shiftHeader = CreateCell(shift, TextAlignment.CENTER, false).SetBold();
                summaryTable.AddCell(shiftHeader);
            }

            Cell totalsHeader = CreateCell("TOTALS", TextAlignment.CENTER, false).SetBold();

            summaryTable.AddCell(totalsHeader);

            //* --------------------------------- BODY --------------------------------

            // Save the totals per shift
            decimal[] shiftTotals = new decimal[shifts.Count];

            foreach (var line in lines)
            {
                summaryTable.AddCell(CreateCell(line, TextAlignment.CENTER, condition));
                decimal lineTotal = 0;

                for (int i = 0; i < shifts.Count; i++)
                {
                    var shift = shifts[i];
                    var production = productionData.FirstOrDefault(p => p.Line == line && p.Shift == shift)?.Production ?? 0;
                    string productionString = production == 0 ? "" : Math.Round(production, 2).ToString();
                    summaryTable.AddCell(CreateCell(productionString, TextAlignment.CENTER, condition));
                    lineTotal += production;
                    shiftTotals[i] += production;
                }

                summaryTable.AddCell(CreateCell(Math.Round(lineTotal, 2).ToString(), TextAlignment.CENTER, condition));

                condition = !condition;
            }

            //* ------------------------- ADD THE LAST ROW OF TOTALS -------------------------

            summaryTable.AddCell(CreateMainCell("TOTALS", TextAlignment.CENTER));

            foreach (var total in shiftTotals)
            {
                summaryTable.AddCell(CreateMainCell(Math.Round(total, 2).ToString(), TextAlignment.CENTER));
            }

            // ADD THE TOTALS OF TOTALS
            var grandTotal = shiftTotals.Sum();
            summaryTable.AddCell(CreateMainCell(Math.Round(grandTotal, 2).ToString(), TextAlignment.CENTER));

            document.Add(summaryTable);
        }

        private Cell CreateCell(string content, TextAlignment alignment, bool isLightGray)
        {
            Color backgroundColor = isLightGray ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE;

            return new Cell(1, 1)
                .SetTextAlignment(alignment)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBackgroundColor(backgroundColor)
                .Add(new Paragraph(content));
        }
        private Cell CreateMainCell(string content, TextAlignment alignment)
        {
            return new Cell(1, 1)
                    .SetTextAlignment(alignment)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBold()
                    .Add(new Paragraph(content));
        }

        public async Task<byte[]> CreatePDFSummaryProduction(SummaryReportFilter request)
        {
            /* Coment update git */

            DateTime startDate = DateTime.Parse(request.StartDate);
            DateTime endDate = DateTime.Parse(request.EndDate);
            try
            {
                var productionData = await _context.ProductionSummary
                .Where(w => w.Date >= startDate && w.Date <= endDate)
                .ToListAsync();

                var downtimeData = await _context.VwDowntime
                .Where(w => w.Date >= startDate && w.Date <= endDate)
                .ToListAsync();

                byte[] pdfBytes;
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    var writer = new PdfWriter(memoryStream);
                    var pdf = new PdfDocument(writer);
                    var document = new Document(pdf);

                    Paragraph emptyLine = new(new Text("\n"));

                    PdfFont font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                    document.SetFont(font);
                    document.SetFontSize(7);

                    // ******** HEADER ********
                    PrintDivider(document, "Summary Production Report");
                    document.Add(emptyLine);
                    PrintLogo(document);

                    ReportHeader reportHeader = new()
                    {
                        StartDate = startDate.ToString("MM/dd/yyyy"),
                        EndDate = endDate.ToString("MM/dd/yyyy"),
                        Line = "ALL",
                    };

                    PrintHeader(document, reportHeader);
                    document.Add(emptyLine);

                    PrintDivider(document, "Efficiency per Line");
                    document.Add(emptyLine);
                    PrintEfficiencyPerLine(document, productionData);
                    document.Add(emptyLine);

                    PrintDivider(document, "Summary Results");
                    document.Add(emptyLine);
                    PrintSummaryResults(document, productionData, downtimeData);
                    document.Add(emptyLine);

                    PrintDivider(document, "Downtime per Category per Line in Hours");
                    document.Add(emptyLine);
                    DowntimePerCategory(document, downtimeData);
                    document.Add(emptyLine);

                    PrintDivider(document, "Production per Line and Shift");
                    document.Add(emptyLine);
                    PrintProductionPerShift(document, productionData);
                    document.Add(emptyLine);

                    PrintDivider(document, "Production per Package and SKU");
                    document.Add(emptyLine);
                    PrintProductionPerPkg(document, productionData);
                    document.Add(emptyLine);

                    document.Close();
                    pdfBytes = memoryStream.ToArray();
                }

                return pdfBytes;
            }
            catch (System.Exception ex)
            {

                throw new Exception("Ocurrió un error al generar el PDF", ex);
            }
        }
    }
}