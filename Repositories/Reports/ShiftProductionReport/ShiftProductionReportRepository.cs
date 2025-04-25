using Microsoft.Data.SqlClient;
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
using inventio.Repositories.EntityFramework;
using Inventio.Models;
using inventio.Models.DTO.Report;
using inventio.Repositories.Reports.ShiftProductionReport.Objects;
using System.Configuration;


namespace inventio.Repositories.ShiftProductionReport
{
    public class ShiftProductionReportRepository : IShiftProductionReportRepository
    {

        private readonly ApplicationDBContext _context;
        private readonly IEFRepository<DTODowntimeReason> _PCLrepository;
        private readonly IConfiguration _configuration;
        private IWebHostEnvironment Environment;

        public ShiftProductionReportRepository(ApplicationDBContext context, IEFRepository<DTODowntimeReason> PCLrepository, IConfiguration configuration, IWebHostEnvironment _environment)
        {
            _context = context;
            _PCLrepository = PCLrepository;
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
            var y = PageSize.A4.GetHeight() - img.GetImageHeight() - 90;
            img.SetFixedPosition(x, y);

            document.Add(img);
        }
        private void PrintHeader(Document document, ReportHeader reportHeader)
        {
            Table table = new Table(2, false);
            table.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

            foreach (var prop in typeof(ReportHeader).GetProperties())
            {
                // Get the name and value of each ReportHeader property
                string propName = prop.Name;
                object notNullVal = prop.GetValue(reportHeader) ?? "";
                object propValue = notNullVal;

                // Crear celdas para la clave y el valor
                Cell keyCell = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBold()
                    .Add(new Paragraph(propName + ":").SetBold());

                Cell valueCell = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(propValue?.ToString() ?? ""));

                // Add the cells to the table
                table.AddCell(keyCell);
                table.AddCell(valueCell);
            }

            document.Add(table);
        }
        private void PrintProduction(Document document, List<DTOProductivityList> dataList)
        {
            ProductionPerHour productionTableHeader = new()
            {
                HourID = "Hour Id",
                SKU = "Sku",
                Standard = "Standard",
                Flavor = "Flavor",
                Content = "Content",
                Production = "Production",
                ScrapUnits = "Scrap Units",
                ScrapPercentage = "Scrap %",
            };

            Table productionTable = new Table(8, false);
            productionTable.SetWidth(UnitValue.CreatePercentValue(100));

            foreach (var prop in typeof(ProductionPerHour).GetProperties())
            {
                object propValue = prop.GetValue(productionTableHeader)!;

                Cell cell = new Cell(1, 1)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph($"{propValue ?? ""}"));

                productionTable.AddCell(cell);
            }


            Boolean condition = true;
            for (int i = 0; i < dataList.Count; i++)
            {
                Product products = (_context.Product.Where(p => p.Sku == dataList[i].SKU).FirstOrDefault()) ?? throw new Exception($"There is no product with the SKU {dataList[i].SKU}");

                Cell schedule = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                    .Add(new Paragraph(dataList[i].Schedule));

                Cell sku = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(dataList[i].SKU.ToString()));

                Cell standardSpeed = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(products.StandardSpeed.ToString()));

                Cell flavor = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(products.Flavour));

                Cell netContent = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.CENTER)
                  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                  .Add(new Paragraph(products.NetContent));

                Cell production = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.CENTER)
                  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                  .Add(new Paragraph(dataList[i].Production.ToString("#,##0")));

                Cell scrapUnits = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(dataList[i].ScrapUnits.ToString()));

                decimal scrapPer = 0;
                if (dataList[i].Production > 0)
                {
                    if (products.UnitsPerPackage > 0 || dataList[i].ScrapUnits > 0)
                    {
                        scrapPer = (dataList[i].ScrapUnits / ((dataList[i].Production * products.UnitsPerPackage) + dataList[i].ScrapUnits)) * 100;

                        scrapPer = decimal.Round(scrapPer, 2, MidpointRounding.AwayFromZero);
                        scrapPer = Math.Round(scrapPer, 2);
                    }
                }

                Cell scrapPercentage = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.CENTER)
                  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                  .Add(new Paragraph(scrapPer + "%"));

                productionTable.AddCell(schedule);
                productionTable.AddCell(sku);
                productionTable.AddCell(standardSpeed);
                productionTable.AddCell(flavor);
                productionTable.AddCell(netContent);
                productionTable.AddCell(production);
                productionTable.AddCell(scrapUnits);
                productionTable.AddCell(scrapPercentage);

                condition = !condition;
            }


            document.Add(productionTable);
        }
        private void PrintDowntime(Document document, List<DTOProductivityList> dataList, string changeCount, List<int> lstcat)
        {
            decimal totalminutes = 0;
            decimal totalCambio = 0;
            int cambiocount = 0;
            string strChangeOver = _configuration["Tenant:ChangeOverString"] ?? throw new SettingsPropertyNotFoundException();

            Paragraph emptyLine = new(new Text("\n"));

            DowntimePerHour downtimeTableHeader = new()
            {
                HourID = "Hour Id",
                Description = "Description",
                Code = "Code",
                SubCategory2 = "Subcategory2",
                Minutes = "Minutes",

            };

            Table downtimeTable = new Table(5, false);
            downtimeTable.SetWidth(UnitValue.CreatePercentValue(100));

            foreach (var prop in typeof(DowntimePerHour).GetProperties())
            {
                object propValue = prop.GetValue(downtimeTableHeader)!;

                Cell cell = new Cell(1, 1)
                   .SetBold()
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .Add(new Paragraph($"{propValue ?? ""}"));

                downtimeTable.AddCell(cell);
            }

            Boolean condition = true;
            for (int i = 0; i < dataList.Count; i++)
            {
                var Cresult = _PCLrepository.ExecuteStoredProcedure<DTODowntimeReason>("GetProductivitybyId",
                                      new SqlParameter("@ProductivityId", dataList[i].Id),
                                      new SqlParameter("@ProductSku", dataList[i].SKU)
                                      ).ToList();
                int count = 0;
                foreach (var item in Cresult)
                {
                    count = count + 1;
                    Cell scheduleCol = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                       .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                       .Add(new Paragraph(count == 1 ? dataList[i].Schedule : ""));

                    Cell failureCol = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                       .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                       .Add(new Paragraph(item.Failure));

                    Cell codeCol = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                       .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                       .Add(new Paragraph(item.Code));

                    Cell subCategory2Col = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                       .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                       .Add(new Paragraph(item.SubCategory2));

                    Cell minutesCol = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER)
                      .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                      .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                      .Add(new Paragraph(item.Minutes.ToString()));


                    if (item.SubCategory2.ToLower().Contains(strChangeOver))
                    {
                        totalCambio = totalCambio + item.Minutes;
                        cambiocount = cambiocount + 1;
                        totalminutes = totalminutes + item.Minutes;
                    }
                    else
                    {
                        totalminutes = totalminutes + item.Minutes;
                    }

                    downtimeTable.AddCell(scheduleCol);
                    downtimeTable.AddCell(failureCol);
                    downtimeTable.AddCell(codeCol);
                    downtimeTable.AddCell(subCategory2Col);
                    downtimeTable.AddCell(minutesCol);
                }
                condition = !condition;

            }


            // Downtime Footer (Totals or summary)
            totalminutes = totalminutes - totalCambio;

            Table tableFooter = new(2, false);
            tableFooter.SetWidth(UnitValue.CreatePercentValue(100));
            Cell footerDowntimeCell = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .SetBold()
               .Add(new Paragraph("TOTAL DOWNTIME"));
            Cell footerDowntimeValue = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.RIGHT)
               .SetBold()
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .Add(new Paragraph(totalminutes.ToString()));
            Cell footerChangeOverCell = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .SetBold()
               .Add(new Paragraph("TOTAL CHANGE OVER"));
            Cell footerChangeOverValue = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.RIGHT)
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .SetBold()
               .Add(new Paragraph(totalCambio.ToString()));
            Cell footerChangesCell = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBold()
                .Add(new Paragraph("NUMBER OF CHANGE OVER"));
            Cell footerChangesValue = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.RIGHT)
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .SetBold()
               .Add(new Paragraph(changeCount));

            tableFooter.AddCell(footerDowntimeCell);
            tableFooter.AddCell(footerDowntimeValue);
            tableFooter.AddCell(footerChangeOverCell);
            tableFooter.AddCell(footerChangeOverValue);
            tableFooter.AddCell(footerChangesCell);
            tableFooter.AddCell(footerChangesValue);

            document.Add(downtimeTable);
            document.Add(emptyLine);
            document.Add(tableFooter);
        }
        private void PrintTotalsMetrics(Document document, List<DTOProductivityList> dataList, List<int> lstcat)
        {
            TotalMetrics totalMetrics = new()
            {
                Sku = "Sku",
                Standard = "Standard",
                Flavor = "Flavor",
                Content = "Content",
                Packing = "Packing",
                Production = "Production",
                Efficiency = "Efficiency",
                ScrapUnits = "Scrap Units",
                ScrapPercentage = "Scrap %"
            };

            Table tableMetrics = new Table(9, false);
            tableMetrics.SetWidth(UnitValue.CreatePercentValue(100));

            foreach (var prop in typeof(TotalMetrics).GetProperties())
            {
                object propValue = prop.GetValue(totalMetrics)!;

                Cell cell = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetBold()
               .Add(new Paragraph($"{propValue ?? ""}"));

                tableMetrics.AddCell(cell);
            }


            // Body Totals and metrics

            decimal totalProduction = 0;
            int totalScraps = 0;
            decimal totalDenomEfficiency = 0;
            decimal TotalDenomScraps = 0;
            int a = dataList.GroupBy(l => l.SKU).Count();
            List<DTOProductivityList> prodList = dataList
                                            .GroupBy(l => l.SKU)
                                                .Select(cl => new DTOProductivityList
                                                {
                                                    SKU = cl.First().SKU,
                                                    Id = cl.First().Id,
                                                    Production = cl.Sum(c => c.Production),
                                                    ScrapUnits = cl.Sum(c => c.ScrapUnits),
                                                    Shift = cl.Count().ToString(),
                                                }).ToList();

            Boolean condition2 = true;
            for (int i = 0; i < prodList.Count; i++)
            {
                // Product products = _context.Product.Where(p => p.Sku == prodList[i].SKU).FirstOrDefault() ?? throw new Exception($"There is no product with the SKU {dataList[i].SKU}");
                Product products = _context.Product.First(p => p.Sku == prodList[i].SKU);


                List<DowntimeReason> changeSum = new List<DowntimeReason>();

                decimal changeMin = 0;
                for (int k = 0; k < dataList.Count; k++)
                {
                    if (dataList[k].SKU == prodList[i].SKU)
                    {

                        var product = _PCLrepository.ExecuteStoredProcedure<DTODowntimeReason>("GetProductivitybyId",
                                 new SqlParameter("@ProductivityId", dataList[k].Id),
                                 new SqlParameter("@ProductSku", dataList[k].SKU)
                                 ).ToList();

                        product = product.Where(l => lstcat.Contains(l.DowntimeCategoryId)).ToList();
                        changeSum = product
                               .GroupBy(l => l.ProductivityID)
                                   .Select(cl => new DowntimeReason
                                   {
                                       Minutes = cl.Sum(c => c.Minutes),
                                       ProductivityID = cl.First().ProductivityID,
                                   }).ToList();

                        if (changeSum.Count > 0)
                        {
                            changeMin = changeMin + changeSum[0].Minutes;
                        }
                    }
                }

                Cell sku = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(prodList[i].SKU.ToString()));

                Cell standardSpeed = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.LEFT)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(products?.StandardSpeed.ToString()));

                Cell flavor = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(products?.Flavour));

                Cell netContent = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.CENTER)
                  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                  .Add(new Paragraph(products?.NetContent));

                Cell packing = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(products?.Packing));

                Cell production = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.RIGHT)
                  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                  .Add(new Paragraph(prodList[i].Production.ToString("#,##0")));

                totalProduction = totalProduction + prodList[i].Production;

                decimal efficiency = 0;

                int stdSpeed = 0;
                if (products is not null)
                {
                    stdSpeed = products.StandardSpeed;
                }

                if (((Convert.ToInt32(prodList[i].Shift) - (changeMin / 60)) * products?.StandardSpeed) == 0)
                    efficiency = 0;
                else
                {
                    efficiency = prodList[i].Production / ((Convert.ToInt32(prodList[i].Shift) - (changeMin / 60)) * stdSpeed) * 100;
                }
                efficiency = decimal.Round(efficiency, 2, MidpointRounding.AwayFromZero);
                efficiency = Math.Round(efficiency, 2);
                if (efficiency > 100)
                {
                    efficiency = 100;
                }
                totalDenomEfficiency = totalDenomEfficiency + ((Convert.ToInt32(prodList[i].Shift) - ((changeMin) / 60)) * stdSpeed);
                Cell efficiencyCol = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.RIGHT)
                  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                  .Add(new Paragraph(efficiency.ToString() + "%"));

                Cell scrapUnitsCol = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.RIGHT)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(prodList[i].ScrapUnits.ToString()));

                totalScraps = totalScraps + prodList[i].ScrapUnits;

                decimal scrapPer = 0;
                int unitXPackage = products!.UnitsPerPackage;
                if (prodList[i].Production > 0)
                {
                    if (unitXPackage > 0 || prodList[i].ScrapUnits > 0)
                    {
                        scrapPer = prodList[i].ScrapUnits / ((prodList[i].Production * unitXPackage) + prodList[i].ScrapUnits) * 100;

                        scrapPer = decimal.Round(scrapPer, 2, MidpointRounding.AwayFromZero);
                        scrapPer = Math.Round(scrapPer, 2);
                    }
                }

                TotalDenomScraps = TotalDenomScraps + (prodList[i].Production * unitXPackage) + prodList[i].ScrapUnits;

                Cell scrapPercentageCol = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.RIGHT)
                  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                 .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                  .Add(new Paragraph(scrapPer + "%"));

                tableMetrics.AddCell(sku);
                tableMetrics.AddCell(standardSpeed);
                tableMetrics.AddCell(flavor);
                tableMetrics.AddCell(netContent);
                tableMetrics.AddCell(packing);
                tableMetrics.AddCell(production);
                tableMetrics.AddCell(efficiencyCol);
                tableMetrics.AddCell(scrapUnitsCol);
                tableMetrics.AddCell(scrapPercentageCol);

                condition2 = !condition2;
            }

            tableMetrics.AddCell(new Cell(1, 10).SetBorder(iText.Layout.Borders.Border.NO_BORDER).Add(new Paragraph(new Text("\n"))));
            tableMetrics.AddCell(new Cell(1, 5).SetTextAlignment(TextAlignment.LEFT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold()
            .Add(new Paragraph("TOTALS")));

            tableMetrics.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold()
            .Add(new Paragraph(totalProduction.ToString("#,##0"))));

            decimal totalEfficiency;
            if (totalDenomEfficiency == 0)
                totalEfficiency = 0;
            else
                totalEfficiency = totalProduction / totalDenomEfficiency * 100;
            totalEfficiency = decimal.Round(totalEfficiency, 2, MidpointRounding.AwayFromZero);
            totalEfficiency = Math.Round(totalEfficiency, 2);

            decimal totalScrapPercentage;
            if (TotalDenomScraps == 0)
                totalScrapPercentage = 0;
            else
                totalScrapPercentage = totalScraps / TotalDenomScraps * 100;
            totalScrapPercentage = decimal.Round(totalScrapPercentage, 2, MidpointRounding.AwayFromZero);
            totalScrapPercentage = Math.Round(totalScrapPercentage, 2);

            tableMetrics.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold()
            .Add(new Paragraph(totalEfficiency.ToString() + "%")));
            tableMetrics.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold()
            .Add(new Paragraph(totalScraps.ToString())));
            tableMetrics.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold()
            .Add(new Paragraph(totalScrapPercentage.ToString() + "%")));

            document.Add(tableMetrics);
        }
        private void PrintComments(Document document, List<DTOProductivityList> dataList)
        {

            CommentPerHour commentTableHeader = new()
            {
                HourID = "Hour Id",
                Comment = "Comment"
            };

            Table commentTable = new Table(2, false);
            commentTable.SetWidth(UnitValue.CreatePercentValue(100));

            foreach (var prop in typeof(CommentPerHour).GetProperties())
            {
                object propValue = prop.GetValue(commentTableHeader)!;

                Cell cell = new Cell(1, 1)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph($"{propValue ?? ""}"));

                commentTable.AddCell(cell);
            }


            Boolean condition = true;
            Boolean commnetExists = false;

            for (int i = 0; i < dataList.Count; i++)
            {

                if (!string.IsNullOrEmpty(dataList[i].Comment))
                {

                    Cell hours = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                    .Add(new Paragraph(dataList[i].Schedule));

                    Cell comment = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                    .Add(new Paragraph(dataList[i].Comment));

                    commentTable.AddCell(hours);
                    commentTable.AddCell(comment);
                    condition = !condition;
                    commnetExists = true;
                }
            }

            if (commnetExists)
            {
                document.Add(commentTable);
            }
        }
        private void PrintReportDobleFlow(Document document, List<DTOProductivityList> dataList, List<DTOProductivityEfficiencyList> efficiencyData, string changeCount, List<int> lstcat)
        {
            string strChangeOver = _configuration["Tenant:ChangeOverString"] ?? throw new SettingsPropertyNotFoundException();
            string strWarehouse = _configuration["Tenant:WarehouseString"] ?? throw new SettingsPropertyNotFoundException();

            //* -------------------------------------------------------------------------- 
            //*                             PRODUCTION SECTION                             
            //* -------------------------------------------------------------------------- 

            ProductionPerHour productionTableHeader = new()
            {
                HourID = "Hour Id",
                SKU = "Sku",
                Standard = "Standard",
                Flavor = "Flavor",
                Content = "Content",
                Production = "Production",
                ScrapUnits = "Scrap Units",
                ScrapPercentage = "Scrap %",
            };

            Table productionTable = new Table(8, false);
            productionTable.SetWidth(UnitValue.CreatePercentValue(100));

            foreach (var prop in typeof(ProductionPerHour).GetProperties())
            {
                object propValue = prop.GetValue(productionTableHeader)!;

                Cell cell = new Cell(1, 1)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph($"{propValue ?? ""}"));

                productionTable.AddCell(cell);
            }

            Boolean condition = true;

            for (int i = 0; i < dataList.Count; i++)
            {
                Product product = _context.Product.Where(p => p.Sku == dataList[i].SKU).FirstOrDefault() ?? throw new Exception($"There is no product with the SKU {dataList[i].SKU}");

                Cell prodScheduleCell = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
               .Add(new Paragraph(dataList[i].Schedule));

                Cell prodSKUCell = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(dataList[i].SKU.ToString()));

                Cell prodStandardSpeedCell = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(product?.StandardSpeed.ToString()));

                Cell prodFlavourCell = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(product?.Flavour));

                Cell prodNetContentCell = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.CENTER)
                  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                  .Add(new Paragraph(product?.NetContent));

                Cell prodProductionCell = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.RIGHT)
                  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                  .Add(new Paragraph(dataList[i].Production.ToString("#,##0")));

                Cell prodScrapUnitsCell = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.RIGHT)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(dataList[i].ScrapUnits.ToString()));

                decimal scrapPer = 0;

                int unitXPackage = 0;
                if (product is not null)
                {
                    unitXPackage = product.UnitsPerPackage;
                }

                if (dataList[i].Production > 0)
                {
                    if (unitXPackage > 0 || dataList[i].ScrapUnits > 0)
                    {
                        scrapPer = dataList[i].ScrapUnits / ((dataList[i].Production * unitXPackage) + dataList[i].ScrapUnits) * 100;
                        scrapPer = decimal.Round(scrapPer, 2, MidpointRounding.AwayFromZero);
                        scrapPer = Math.Round(scrapPer, 2);
                    }
                }

                Cell prodScrapPerCell = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.RIGHT)
                  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                  .Add(new Paragraph(scrapPer + "%"));

                productionTable.AddCell(prodScheduleCell);
                productionTable.AddCell(prodSKUCell);
                productionTable.AddCell(prodStandardSpeedCell);
                productionTable.AddCell(prodFlavourCell);
                productionTable.AddCell(prodNetContentCell);
                productionTable.AddCell(prodProductionCell);
                productionTable.AddCell(prodScrapUnitsCell);
                productionTable.AddCell(prodScrapPerCell);

                condition = !condition;
            }
            // -----------------------------------------------------------------------------------------------------

            //* -------------------------------------------------------------------------- 
            //*                             Downtime SECTION                             
            //* -------------------------------------------------------------------------- 

            decimal totalminutes = 0;
            decimal totalCambio = 0;
            decimal totalAlcamen = 0;
            decimal excludeCat = 0;
            int cambiocount = 0;

            DowntimePerHour downtimeTableHeader = new()
            {
                HourID = "Hour Id",
                Description = "Description",
                Code = "Code",
                SubCategory2 = "Subcategory2",
                Minutes = "Minutes",

            };

            Table downtimeTable = new Table(5, false);
            downtimeTable.SetWidth(UnitValue.CreatePercentValue(100));

            foreach (var prop in typeof(DowntimePerHour).GetProperties())
            {
                object propValue = prop.GetValue(downtimeTableHeader)!;

                Cell cell = new Cell(1, 1)
                   .SetBold()
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .Add(new Paragraph($"{propValue ?? ""}"));

                downtimeTable.AddCell(cell);
            }

            var lsit = dataList.GroupBy(l => l.Id).FirstOrDefault();
            var result = dataList.GroupBy(x => x.Id).Select(x => x.First()).ToList();

            condition = true;
            for (int i = 0; i < result.Count; i++)
            {
                var Cresult = _PCLrepository.ExecuteStoredProcedure<DTODowntimeReason>("GetProductivitybyId",
                               new SqlParameter("@ProductivityId", result[i].Id),
                               new SqlParameter("@ProductSku", result[i].SKU)
                               ).ToList();
                foreach (var item in Cresult)
                {
                    Cell downtimeScheduleCell = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                       .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                       .Add(new Paragraph(result[i].Schedule));

                    Cell downtimeFailureCell = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                       .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                       .Add(new Paragraph(item.Failure));

                    Cell downtimeCodeCell = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                       .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                       .Add(new Paragraph(item.Code));

                    Cell downtimeSubCategory2Cell = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                       .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                       .Add(new Paragraph(item.SubCategory2));

                    Cell downtimeMinutesCell = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.RIGHT)
                      .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                      .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                      .Add(new Paragraph(item.Minutes.ToString()));

                    if (item.SubCategory2.ToLower().Contains(strChangeOver))
                    {
                        totalCambio = totalCambio + item.Minutes;
                        cambiocount = cambiocount + 1;
                    }

                    if (lstcat.Contains(item.DowntimeCategoryId))
                    {
                        excludeCat = excludeCat + item.Minutes;
                    }

                    if (item.SubCategory2.ToUpper().Contains(strWarehouse))
                    {
                        totalAlcamen = totalAlcamen + item.Minutes;
                    }

                    totalminutes = totalminutes + item.Minutes;

                    downtimeTable.AddCell(downtimeScheduleCell);
                    downtimeTable.AddCell(downtimeFailureCell);
                    downtimeTable.AddCell(downtimeCodeCell);
                    downtimeTable.AddCell(downtimeSubCategory2Cell);
                    downtimeTable.AddCell(downtimeMinutesCell);

                    condition = !condition;
                }
            }

            totalminutes = totalminutes - totalCambio;

            Table tableFooter = new Table(2, false);
            tableFooter.SetWidth(UnitValue.CreatePercentValue(100));
            Cell cell51 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .SetBold()
               .Add(new Paragraph("TOTAL DOWNTIME"));
            Cell cell52 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.RIGHT)
               .SetBold()
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .Add(new Paragraph(totalminutes.ToString()));
            Cell cell53 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.LEFT)
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .SetBold()
               .Add(new Paragraph("TOTAL CHANGE OVER"));
            Cell cell54 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.RIGHT)
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .SetBold()
               .Add(new Paragraph(totalCambio.ToString()));
            Cell cell55 = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBold()
                .Add(new Paragraph("NUMBER OF CHANGE OVER"));
            Cell cell56 = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.RIGHT)
               .SetBold()
               .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
               .Add(new Paragraph(changeCount));

            tableFooter.AddCell(cell51);
            tableFooter.AddCell(cell52);
            tableFooter.AddCell(cell53);
            tableFooter.AddCell(cell54);
            tableFooter.AddCell(cell55);
            tableFooter.AddCell(cell56);

            //* -------------------------------------------------------------------------- 
            //*                             TOTALS AND METRICS SECTION                             
            //* --------------------------------------------------------------------------

            TotalMetrics totalMetrics = new()
            {
                Sku = "Sku",
                Standard = "Standard",
                Flavor = "Flavor",
                Content = "Content",
                Packing = "Packing",
                Production = "Production",
                Efficiency = "Efficiency",
                ScrapUnits = "Scrap Units",
                ScrapPercentage = "Scrap %"
            };

            Table tableMetrics = new Table(9, false);
            tableMetrics.SetWidth(UnitValue.CreatePercentValue(100));

            foreach (var prop in typeof(TotalMetrics).GetProperties())
            {
                object propValue = prop.GetValue(totalMetrics)!;

                Cell cell = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetBold()
               .Add(new Paragraph($"{propValue ?? ""}"));

                tableMetrics.AddCell(cell);
            }

            decimal totalproduction = 0;
            decimal totaldenomefficiency = 0;
            int totalhours = 0;
            int estand = 0;

            int totalscraps = 0;
            decimal totaldenomscraps = 0;

            List<DTOProductivityEfficiencyList> effData = efficiencyData
                                            .GroupBy(l => l.SKU)
                                                .Select(cl => new DTOProductivityEfficiencyList
                                                {
                                                    SKU = cl.First().SKU,
                                                    Production = cl.Sum(c => c.Production),
                                                    Shift = cl.First().Shift,
                                                    EffDEN = cl.Sum(c => c.EffDEN),
                                                    EffDENSKU = cl.Sum(c => c.EffDENSKU),
                                                    NetHrs = cl.Sum(c => c.NetHrs),
                                                    Hrs = cl.Sum(c => c.Hrs),
                                                    ChangeHrs = cl.Sum(c => c.ChangeHrs),
                                                    ChangeMins = cl.Sum(c => c.ChangeMins),
                                                    Efficiency = cl.First().Efficiency,
                                                    StandardSpeed = cl.First().StandardSpeed,
                                                    Scrap = cl.Sum(c => c.Scrap),
                                                }).OrderBy(l => l.SKU).ToList();


            condition = true;
            for (int i = 0; i < effData.Count; i++)
            {
                var products = _context.Product.Where(p => p.Sku == effData[i].SKU).FirstOrDefault() ?? throw new Exception($"There is no product with the SKU {dataList[i].SKU}");

                List<DowntimeReason> cambioSum = new List<DowntimeReason>();

                Cell metricsSkuCell = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(effData[i].SKU.ToString()));

                Cell metricsSpeedCell = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(products?.StandardSpeed.ToString()));

                Cell metricsFlavourCell = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(products?.Flavour));

                Cell metricsNetContentCell = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.CENTER)
                  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                  .Add(new Paragraph(products?.NetContent));

                Cell metricsPackingCell = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                   .Add(new Paragraph(products?.Packing));

                Cell metricsProductionCell = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.RIGHT)
                  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                  .Add(new Paragraph(effData[i].Production.ToString("#,##0")));

                totalproduction = totalproduction + effData[i].Production;

                decimal effciency = 0;

                if (effData[i].EffDENSKU != 0)
                    effciency = (effData[i].Production / effData[i].EffDENSKU) * 100;

                effciency = decimal.Round(effciency, 2, MidpointRounding.AwayFromZero);
                effciency = Math.Round(effciency, 2);
                estand = products?.StandardSpeed ?? 0; ;
                totalhours = result.Count();

                totaldenomefficiency = totaldenomefficiency + effData[i].EffDEN;

                Cell metricsEffciencyCell = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                .Add(new Paragraph(effciency.ToString() + "%"));

                Cell metricsScrapCell = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                .Add(new Paragraph(effData[i].Scrap.ToString()));

                totalscraps = totalscraps + effData[i].Scrap;

                decimal scrapPer = 0;

                int stdSpeed = 0;
                int unitsXPackage = 0;

                if (products is not null)
                {
                    stdSpeed = products.StandardSpeed;
                    unitsXPackage = products.UnitsPerPackage;
                }

                if (effData[i].Production > 0)
                {
                    if (stdSpeed > 0 || effData[i].Scrap > 0)
                    {
                        scrapPer = effData[i].Scrap / ((effData[i].Production * unitsXPackage) + effData[i].Scrap) * 100;

                        scrapPer = decimal.Round(scrapPer, 2, MidpointRounding.AwayFromZero);
                        scrapPer = Math.Round(scrapPer, 2);
                    }
                }

                totaldenomscraps = totaldenomscraps + (effData[i].Production * unitsXPackage) + effData[i].Scrap;

                Cell metricsScrapPerCell = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                .Add(new Paragraph(scrapPer + "%"));

                tableMetrics.AddCell(metricsSkuCell);
                tableMetrics.AddCell(metricsSpeedCell);
                tableMetrics.AddCell(metricsFlavourCell);
                tableMetrics.AddCell(metricsNetContentCell);
                tableMetrics.AddCell(metricsPackingCell);
                tableMetrics.AddCell(metricsProductionCell);
                tableMetrics.AddCell(metricsEffciencyCell);
                tableMetrics.AddCell(metricsScrapCell);
                tableMetrics.AddCell(metricsScrapPerCell);
            }
            tableMetrics.AddCell(new Cell(1, 10).SetBorder(iText.Layout.Borders.Border.NO_BORDER).Add(new Paragraph(new Text("\n"))));
            tableMetrics.AddCell(new Cell(1, 5).SetTextAlignment(TextAlignment.LEFT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold().Add(new Paragraph("TOTALS")));
            tableMetrics.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold().Add(new Paragraph(totalproduction.ToString("#,##0"))));

            decimal totalefficiancy;
            if (totaldenomefficiency == 0)
                totalefficiancy = 0;
            else
                totalefficiancy = (totalproduction / totaldenomefficiency) * 100;

            totalefficiancy = decimal.Round(totalefficiancy, 2, MidpointRounding.AwayFromZero);
            totalefficiancy = Math.Round(totalefficiancy, 2);
            tableMetrics.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold().Add(new Paragraph(totalefficiancy.ToString() + "%")));

            decimal totalscrapPer;
            if (totaldenomscraps == 0)
                totalscrapPer = 0;
            else
                totalscrapPer = (totalscraps / totaldenomscraps) * 100;

            totalscrapPer = decimal.Round(totalscrapPer, 2, MidpointRounding.AwayFromZero);
            totalscrapPer = Math.Round(totalscrapPer, 2);
            tableMetrics.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold().Add(new Paragraph(totalscraps.ToString())));
            tableMetrics.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold().Add(new Paragraph(totalscrapPer.ToString() + "%")));


            // -----------------------------------------------------------------------------------------------------

            //* -------------------------------- PRINTING --------------------------------
            Paragraph emptyLine = new(new Text("\n"));

            PrintDivider(document, "TOTALS AND METRICS");
            document.Add(emptyLine);
            document.Add(tableMetrics);
            document.Add(emptyLine);
            PrintDivider(document, "PRODUCTION PER HOURS");
            document.Add(emptyLine);
            document.Add(productionTable);
            document.Add(emptyLine);

            PrintDivider(document, "DOWNTIME PER HOURS");
            document.Add(emptyLine);
            document.Add(downtimeTable);
            document.Add(emptyLine);
            document.Add(tableFooter);

        }

        public byte[] CreatePDFShiftProduction(ReportFilter request)
        {
            DateTime date = DateTime.Parse(request.date);
            int line = request.line;
            int shift = request.shift;
            int supervisor = request.supervisor;
            string name = request.name;
            string cat = request.cat;

            List<Int32> lstcat = new List<int>();
            if (cat != null && cat != "")
            {
                lstcat = cat.Split(',').Select(x => int.Parse(x)).ToList();
            }
            else { cat = ""; }


            try
            {
                Supervisor superv = _context.Supervisor.First(s => s.Id == supervisor);
                string supervisorDesc = superv.Description;

                Inventio.Models.Line lineSet = _context.Line.First(L => L.Id == line);
                var flow = lineSet?.Flow;


                var ProductivityList = _PCLrepository.ExecuteStoredProcedure<DTOProductivityList>("GetReportProductivityList",
                    new SqlParameter("@Date", date),
                    new SqlParameter("@Line", line),
                    new SqlParameter("@Shift", shift)
                ).ToList();

                var efficiencyData = _PCLrepository.ExecuteStoredProcedure<DTOProductivityEfficiencyList>("GetReportEfficiency",
                    new SqlParameter("@Date", date),
                    new SqlParameter("@Line", line),
                    new SqlParameter("@Shift", shift),
                    new SqlParameter("@Category", cat)
                    ).ToList();

                var shiftSet = _context.Shift.First(y => y.Id == shift);
                string starthours = shiftSet.ScheduleStarts;

                // excecution of the store procedure that generate the data for the table change over
                _context.Database.ExecuteSqlRaw("EXEC GenerateChangeOverRecords");

                string lineName = lineSet!.Name;

                var changeOverSet = _context.ChangeOver.Where(c => c.Date == date && c.Line == lineName && c.Shift == shiftSet.Name).ToList();

                // validation of whether there are records to print

                if (ProductivityList.Count < 0)
                {
                    throw new Exception("no se encontraron registros");
                }

                List<DTOProductivityList> ProductivityListSorted = new List<DTOProductivityList>();
                var ProductivityFromAMToPM = ProductivityList.OrderBy(obj => DateTime.Parse(obj.HourStart)).ToList();
                //  Find the index of the first element with start time equal to starthours
                int startIndex = ProductivityFromAMToPM.FindIndex(obj => obj.HourStart == starthours);

                if (startIndex != -1)
                {
                    // Separate the list into two parts: before and after startIndex
                    var beforeStart = ProductivityFromAMToPM.GetRange(0, startIndex);
                    var afterStart = ProductivityFromAMToPM.GetRange(startIndex, ProductivityFromAMToPM.Count - startIndex);

                    // Concatenate the two parts in the desired order
                    ProductivityListSorted = afterStart.Concat(beforeStart).ToList();
                }
                else
                {
                    ProductivityListSorted = ProductivityFromAMToPM;
                }

                //* PDF CREATION PRINTING GOES HERE
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
                    PrintDivider(document, "SHIFT PRODUCTION REPORT");
                    document.Add(emptyLine);
                    PrintLogo(document);

                    ReportHeader reportHeader = new()
                    {
                        Date = ProductivityListSorted[0].Date,
                        Line = ProductivityListSorted[0].Line,
                        Shift = ProductivityListSorted[0].Shift,
                        Supervisor = supervisorDesc,
                        Name = name
                    };

                    PrintHeader(document, reportHeader);
                    document.Add(emptyLine);

                    var changeCount = changeOverSet.Count.ToString();

                    if (flow == 2)
                    {
                        // ******** Production, Downtime and Metrics ********
                        PrintReportDobleFlow(document, ProductivityListSorted, efficiencyData, changeCount, lstcat);
                        document.Add(emptyLine);

                        // ******** COMMENTS ********
                        PrintDivider(document, "COMMENT PER HOURS");
                        document.Add(emptyLine);
                        PrintComments(document, ProductivityListSorted);
                    }
                    else
                    {

                        // ******** TOTALS AND METRICS ********
                        PrintDivider(document, "TOTALS AND METRICS");
                        PrintTotalsMetrics(document, ProductivityListSorted, lstcat);
                        document.Add(emptyLine);

                        // ******** PRODUCTION ********
                        PrintDivider(document, "PRODUCTION PER HOURS");
                        document.Add(emptyLine);
                        PrintProduction(document, ProductivityListSorted);
                        document.Add(emptyLine);

                        // ******** DOWNTIME ********
                        PrintDivider(document, "DOWNTIME PER HOURS");
                        document.Add(emptyLine);
                        PrintDowntime(document, ProductivityListSorted, changeCount, lstcat);
                        document.Add(emptyLine);

                        // ******** COMMENTS ********
                        PrintDivider(document, "COMMENT PER HOURS");
                        document.Add(emptyLine);
                        PrintComments(document, ProductivityListSorted);
                    }

                    document.Close();
                    pdfBytes = memoryStream.ToArray();
                }


                // Close document
                // document.Flush();
                return pdfBytes;

            }
            catch (System.Exception ex)
            {

                throw new Exception("Ocurrió un error al generar el PDF", ex);
            }
        }
    }
}