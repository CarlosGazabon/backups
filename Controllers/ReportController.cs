using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
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
using System.Configuration;

namespace inventio.Controllers
{
   [ApiController]
   [Route("api/[controller]")]
   public class ReportController : Controller
   {
      private readonly ApplicationDBContext _context;
      private readonly IEFRepository<DTODowntimeReason> _PCLrepository;
      private readonly IConfiguration _configuration;
      private IWebHostEnvironment Environment;

      public ReportController(ApplicationDBContext context, IEFRepository<DTODowntimeReason> PCLrepository, IConfiguration configuration, IWebHostEnvironment _environment)
      {
         _context = context;
         _PCLrepository = PCLrepository;
         _configuration = configuration;
         Environment = _environment;
      }

      [HttpPost("create-shift-production")]
      public IActionResult CreatePDFShiftProduction([FromBody] ReportFilter request)
      {
         DateTime date = DateTime.Parse(request.date);
         int line = request.line;
         int shift = request.shift;
         int supervisorId = request.supervisor;
         string name = request.name;
         string cat = request.cat;

         List<Int32> lstcat = new List<int>();
         if (cat != null && cat != "")
         {
            lstcat = cat.Split(',').Select(x => int.Parse(x)).ToList();
         }
         else { cat = ""; }


         Supervisor supervisor = _context.Supervisor.First(s => s.Id == supervisorId);
         string description = supervisor.Description;

         try
         {
            string supervisorDesc = description;
            var lineSet = _context.Line.First(L => L.Id == line);
            var flow = lineSet.Flow;


            var Mresult = _PCLrepository.ExecuteStoredProcedure<DTOProductivityList>("GetReportProductivityList",
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

            _context.Database.ExecuteSqlRaw("EXEC GenerateChangeOverRecords");
            var changeOverSet = _context.ChangeOver.Where(c => c.Date == date && c.Line == lineSet.Name && c.Shift == shiftSet.Name).ToList();

            //* PDF CREATION
            if (Mresult.Count > 0)
            {

               List<DTOProductivityList> ProductivityListSorted = new List<DTOProductivityList>();
               var resultSorted = Mresult.OrderBy(obj => DateTime.Parse(obj.HourStart)).ToList();
               //  Find the index of the first element with start time equal to starthours
               int startIndex = resultSorted.FindIndex(obj => obj.HourStart == starthours);

               if (startIndex != -1)
               {
                  // Separate the list into two parts: before and after startIndex
                  var beforeStart = resultSorted.GetRange(0, startIndex);
                  var afterStart = resultSorted.GetRange(startIndex, resultSorted.Count - startIndex);

                  // Concatenate the two parts in the desired order
                  ProductivityListSorted = afterStart.Concat(beforeStart).ToList();
               }
               else
               {
                  ProductivityListSorted = resultSorted;
               }

               byte[] pdfBytes;
               using (var stream = new MemoryStream())
               using (var writer = new PdfWriter(stream))
               using (var pdf = new PdfDocument(writer))
               using (var document = new Document(pdf))
               {
                  PdfFont font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                  document.SetFont(font);
                  document.SetFontSize(7);

                  // Header
                  Paragraph header = new Paragraph("SHIFT PRODUCTION REPORT")
                     .SetTextAlignment(TextAlignment.LEFT)
                     .SetFontSize(10)
                     .SetBackgroundColor(ColorConstants.BLUE)
                     .SetFontColor(ColorConstants.WHITE)
                     .SetPaddingLeft(5);

                  // New line
                  Paragraph newline = new Paragraph(new Text("\n"));

                  document.Add(newline);
                  document.Add(header);

                  string wwwPath = this.Environment.WebRootPath;

                  //Add image LOGO and define a variable for change Over and WAREHOUSE
                  string imagePath = string.Concat(wwwPath, _configuration["Tenant:ImagePath"]) ?? throw new SettingsPropertyNotFoundException();
                  string strChangeOver = _configuration["Tenant:ChangeOverString"] ?? throw new SettingsPropertyNotFoundException();
                  string strWarehouse = _configuration["Tenant:WarehouseString"] ?? throw new SettingsPropertyNotFoundException();

                  Image img = new Image(ImageDataFactory
                     .Create(imagePath))
                     .SetTextAlignment(TextAlignment.RIGHT);

                  var x = PageSize.A4.GetWidth() - img.GetImageWidth() - 51;
                  var y = PageSize.A4.GetHeight() - img.GetImageHeight() - 102;
                  img.SetFixedPosition(x, y);

                  document.Add(img);

                  Table tableh = new Table(2, false);
                  tableh.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                  Cell cellh1 = new Cell(1, 1)
                     .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph("DATE"))
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .SetBold();
                  Cell cellh2 = new Cell(1, 1)
                     .SetTextAlignment(TextAlignment.LEFT)
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .Add(new Paragraph(ProductivityListSorted[0].Date));
                  Cell cellh3 = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("LINE").SetBold());
                  Cell cellh4 = new Cell(1, 1)
                     .SetTextAlignment(TextAlignment.LEFT)
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .Add(new Paragraph(ProductivityListSorted[0].Line.ToString()));
                  Cell cellh5 = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("SHIFT")).SetBold();
                  Cell cellh6 = new Cell(1, 1)
                     .SetTextAlignment(TextAlignment.LEFT)
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .Add(new Paragraph(ProductivityListSorted[0].Shift.ToString()));
                  Cell cellh7 = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("SUPERVISOR")).SetBold();
                  Cell cellh8 = new Cell(1, 1)
                     .SetTextAlignment(TextAlignment.LEFT)
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .Add(new Paragraph(supervisorDesc.ToString()));
                  Cell cellh9 = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.LEFT)
                   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                   .Add(new Paragraph("NAME")).SetBold();
                  Cell cellh10 = new Cell(1, 1)
                     .SetTextAlignment(TextAlignment.LEFT)
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .Add(new Paragraph(name.ToString()));

                  tableh.AddCell(cellh1); tableh.AddCell(cellh2);
                  //tableh.AddCell(cellhI);
                  tableh.AddCell(cellh3);
                  tableh.AddCell(cellh4); tableh.AddCell(cellh5); tableh.AddCell(cellh6);
                  tableh.AddCell(cellh7); tableh.AddCell(cellh8); tableh.AddCell(cellh9);
                  tableh.AddCell(cellh10);

                  document.Add(newline);
                  document.Add(tableh);

                  Paragraph header1 = new Paragraph("PRODUCTION PER HOUR")
                     .SetTextAlignment(TextAlignment.LEFT)
                     .SetFontSize(10)
                     .SetBackgroundColor(ColorConstants.BLUE)
                     .SetPaddingLeft(5)
                     .SetFontColor(ColorConstants.WHITE);

                  Paragraph newline1 = new Paragraph(new Text("\n"));

                  document.Add(newline1);
                  document.Add(header1);

                  // Table Production
                  //Table table = new Table(10, false);
                  Table table = new Table(8, false);
                  table.SetWidth(UnitValue.CreatePercentValue(100));
                  Cell cell11 = new Cell(1, 1)
                     .SetBold()
                     .SetTextAlignment(TextAlignment.CENTER)
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .Add(new Paragraph("HOUR ID"));
                  Cell cell12 = new Cell(1, 1)
                     .SetBold()
                     .SetTextAlignment(TextAlignment.CENTER)
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .Add(new Paragraph("SKU"));
                  Cell cell13 = new Cell(1, 1)
                     .SetBold()
                     .SetTextAlignment(TextAlignment.CENTER)
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .Add(new Paragraph("STANDARD"));
                  Cell cell14 = new Cell(1, 1)
                     .SetBold()
                     .SetTextAlignment(TextAlignment.CENTER)
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .Add(new Paragraph("FLAVOURD"));
                  Cell cell15 = new Cell(1, 1)
                     .SetBold()
                     .SetTextAlignment(TextAlignment.CENTER)
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .Add(new Paragraph("CONTENT"));
                  //Cell cell16 = new Cell(1, 1)
                  //   .SetBold()
                  //   .SetTextAlignment(TextAlignment.CENTER)
                  //   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  //   .Add(new Paragraph("EMPAQUE"));
                  Cell cell17 = new Cell(1, 1)
                     .SetBold()
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("PRODUCTION"));
                  //Cell cell18 = new Cell(1, 1)
                  //  .SetBold()
                  //  .SetTextAlignment(TextAlignment.CENTER)
                  //  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                  //  .Add(new Paragraph("Unidades Empaque"));
                  Cell cell19 = new Cell(1, 1)
                     .SetBold()
                     .SetTextAlignment(TextAlignment.CENTER)
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .Add(new Paragraph("SCRAP UNITS"));
                  Cell cell20 = new Cell(1, 1)
                     .SetBold()
                     .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("SCRAP %"));
                  table.AddCell(cell11);
                  table.AddCell(cell12);
                  table.AddCell(cell13);
                  table.AddCell(cell14);
                  table.AddCell(cell15);
                  //table.AddCell(cell16);
                  table.AddCell(cell17);
                  //table.AddCell(cell18);
                  table.AddCell(cell19);
                  table.AddCell(cell20);
                  Boolean condition = true;
                  if (flow == 2)
                  {

                     for (int i = 0; i < ProductivityListSorted.Count; i++)
                     {
                        // Product product = _context.Product.Where(p => p.Sku == ProductivityListSorted[i].SKU).FirstOrDefault() ?? new Product();
                        Product product = _context.Product.First(p => p.Sku == ProductivityListSorted[i].SKU);

                        if (product != null)
                        {
                           Cell cell21 = new Cell(1, 1)
                          .SetTextAlignment(TextAlignment.CENTER)
                          .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                          .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                          .Add(new Paragraph(ProductivityListSorted[i].Schedule));

                           Cell cell22 = new Cell(1, 1)
                              .SetTextAlignment(TextAlignment.CENTER)
                              .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                              .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                              .Add(new Paragraph(ProductivityListSorted[i].SKU.ToString()));

                           Cell cell23 = new Cell(1, 1)
                              .SetTextAlignment(TextAlignment.CENTER)
                              .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                              .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                              .Add(new Paragraph(product?.StandardSpeed.ToString()));

                           Cell cell24 = new Cell(1, 1)
                              .SetTextAlignment(TextAlignment.CENTER)
                              .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                              .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                              .Add(new Paragraph(product?.Flavour));

                           Cell cell25 = new Cell(1, 1)
                             .SetTextAlignment(TextAlignment.CENTER)
                             .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                             .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                             .Add(new Paragraph(product?.NetContent));

                           //Cell cell26 = new Cell(1, 1)
                           //   .SetTextAlignment(TextAlignment.CENTER)
                           //   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           //   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                           //   .Add(new Paragraph(productos.Empaque));

                           Cell cell27 = new Cell(1, 1)
                             .SetTextAlignment(TextAlignment.RIGHT)
                             .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                             .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                             .Add(new Paragraph(ProductivityListSorted[i].Production.ToString("#,##0")));

                           //Cell cell28 = new Cell(1, 1)
                           //  .SetTextAlignment(TextAlignment.RIGHT)
                           //  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           //  .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                           //  .Add(new Paragraph(productos.Unidades_por_Empaque.ToString()));

                           Cell cell29 = new Cell(1, 1)
                              .SetTextAlignment(TextAlignment.RIGHT)
                              .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                              .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                              .Add(new Paragraph(ProductivityListSorted[i].ScrapUnits.ToString()));

                           // = IF(L13 > 0, N13 / ((M13 * L13) + N13), 0)
                           decimal scrapPer = 0;

                           var unitsPerPackage = product!.UnitsPerPackage;

                           if (ProductivityListSorted[i].Production > 0)
                           {
                              if (unitsPerPackage > 0 || ProductivityListSorted[i].ScrapUnits > 0)
                              {
                                 scrapPer = (ProductivityListSorted[i].ScrapUnits / ((ProductivityListSorted[i].Production * product.UnitsPerPackage) + ProductivityListSorted[i].ScrapUnits)) * 100;
                                 scrapPer = decimal.Round(scrapPer, 2, MidpointRounding.AwayFromZero);
                                 scrapPer = Math.Round(scrapPer, 2);
                              }
                           }

                           Cell cell30 = new Cell(1, 1)
                             .SetTextAlignment(TextAlignment.RIGHT)
                             .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                             .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                             .Add(new Paragraph(scrapPer + "%"));

                           table.AddCell(cell21);
                           table.AddCell(cell22);
                           table.AddCell(cell23);
                           table.AddCell(cell24);
                           table.AddCell(cell25);
                           //table.AddCell(cell26);
                           table.AddCell(cell27);
                           //table.AddCell(cell28);
                           table.AddCell(cell29);
                           table.AddCell(cell30);

                           condition = !condition;
                        }
                        else
                        {
                           return Json(new { success = false, msg = $"There is no product with the SKU {ProductivityListSorted[i].SKU}" });
                        }
                     }
                     // -----------------------------------------------------------------------------------------------------
                     decimal totalminutes = 0;
                     decimal totalCambio = 0;
                     decimal totalAlcamen = 0;
                     decimal excludeCat = 0;
                     int cambiocount = 0;
                     Paragraph newline2 = new Paragraph(new Text("\n"));
                     document.Add(newline2);
                     document.Add(table);

                     Paragraph header3 = new Paragraph("DOWNTIME PER HOUR")
                       .SetTextAlignment(TextAlignment.LEFT)
                       .SetFontSize(10)
                       .SetBackgroundColor(ColorConstants.BLUE)
                       .SetPaddingLeft(5)
                       .SetFontColor(ColorConstants.WHITE);

                     Paragraph newline3 = new Paragraph(new Text("\n"));
                     document.Add(newline3);
                     document.Add(header3);

                     // Table child
                     Table table4 = new Table(5, false);
                     table4.SetWidth(UnitValue.CreatePercentValue(100));

                     Cell cellp1 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("HOUR ID"));
                     Cell cellp2 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("FAILURE"));
                     Cell cellp3 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("CODE"));
                     Cell cellp4 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("SUBCATEGORY 2"));
                     Cell cellp5 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("MINUTES"));

                     table4.AddCell(cellp1);
                     table4.AddCell(cellp2);
                     table4.AddCell(cellp3);
                     table4.AddCell(cellp4);
                     table4.AddCell(cellp5);

                     var lsit = ProductivityListSorted.GroupBy(l => l.Id).FirstOrDefault();
                     var result = ProductivityListSorted.GroupBy(x => x.Id).Select(x => x.First()).ToList();

                     Boolean condition1 = true;
                     for (int i = 0; i < result.Count; i++)
                     {
                        var Cresult = _PCLrepository.ExecuteStoredProcedure<DTODowntimeReason>("GetProductivitybyId",
                                       new SqlParameter("@ProductivityId", result[i].Id),
                                       new SqlParameter("@ProductSku", result[i].SKU)
                                       ).ToList();
                        //int count = 0;
                        foreach (var item in Cresult)
                        {
                           //count = count + 1;
                           Cell cell21 = new Cell(1, 1)
                              .SetTextAlignment(TextAlignment.CENTER)
                              .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                              .SetBackgroundColor(condition1 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                              .Add(new Paragraph(result[i].Schedule));

                           Cell cell22 = new Cell(1, 1)
                              .SetTextAlignment(TextAlignment.CENTER)
                              .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                              .SetBackgroundColor(condition1 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                              .Add(new Paragraph(item.Failure));

                           Cell cell23 = new Cell(1, 1)
                              .SetTextAlignment(TextAlignment.CENTER)
                              .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                              .SetBackgroundColor(condition1 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                              .Add(new Paragraph(item.Code));

                           Cell cell24 = new Cell(1, 1)
                              .SetTextAlignment(TextAlignment.CENTER)
                              .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                              .SetBackgroundColor(condition1 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                              .Add(new Paragraph(item.SubCategory2));

                           Cell cell25 = new Cell(1, 1)
                             .SetTextAlignment(TextAlignment.RIGHT)
                             .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                             .SetBackgroundColor(condition1 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
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

                           table4.AddCell(cell21);
                           table4.AddCell(cell22);
                           table4.AddCell(cell23);
                           table4.AddCell(cell24);
                           table4.AddCell(cell25);

                           condition1 = !condition1;
                        }
                     }

                     Paragraph newline4 = new Paragraph(new Text("\n"));
                     document.Add(newline4);
                     document.Add(table4);

                     totalminutes = totalminutes - totalCambio;

                     Table table5 = new Table(2, false);
                     table5.SetWidth(UnitValue.CreatePercentValue(100));
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
                        .Add(new Paragraph(changeOverSet.Count().ToString()));

                     table5.AddCell(cell51);
                     table5.AddCell(cell52);
                     table5.AddCell(cell53);
                     table5.AddCell(cell54);
                     table5.AddCell(cell55);
                     table5.AddCell(cell56);

                     Paragraph newline5 = new Paragraph(new Text("\n"));
                     document.Add(newline5);
                     document.Add(table5);

                     // table for calculation
                     Paragraph header6 = new Paragraph("TOTALS & METRICS")
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetFontSize(10)
                        .SetBackgroundColor(ColorConstants.BLUE)
                        .SetPaddingLeft(5)
                        .SetFontColor(ColorConstants.WHITE);

                     Paragraph newline6 = new Paragraph(new Text("\n"));

                     document.Add(newline6);
                     document.Add(header6);

                     // Table Production
                     //Table table6 = new Table(10, false);
                     Table table6 = new Table(9, false);
                     table6.SetWidth(UnitValue.CreatePercentValue(100));
                     Cell cell61 = new Cell(1, 1)
                         .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("SKU"));
                     Cell cell62 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("STANDARD"));
                     Cell cell63 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("FLAVOUR"));
                     Cell cell64 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("CONTENT"));
                     Cell cell65 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("PACKING"));
                     Cell cell66 = new Cell(1, 1)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Paragraph("PRODUCTION"));
                     Cell cell67 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("EFFICIENCY"));
                     //Cell cell68 = new Cell(1, 1)
                     // .SetBold()
                     // .SetTextAlignment(TextAlignment.CENTER)
                     // .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     // .Add(new Paragraph("Unidades Empaque"));
                     Cell cell69 = new Cell(1, 1)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("SCRAP UNITS"));
                     Cell cell70 = new Cell(1, 1)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Paragraph("SCRAP %"));
                     table6.AddCell(cell61);
                     table6.AddCell(cell62);
                     table6.AddCell(cell63);
                     table6.AddCell(cell64);
                     table6.AddCell(cell65);
                     table6.AddCell(cell66);
                     table6.AddCell(cell67);
                     //table6.AddCell(cell68);
                     table6.AddCell(cell69);
                     table6.AddCell(cell70);

                     decimal totalproduction = 0;
                     decimal totaldenomefficiency = 0;
                     int totalhours = 0;
                     int estand = 0;

                     int totalscraps = 0;
                     decimal totaldenomscraps = 0;

                     //int a = Mresult.GroupBy(l => l.SKU).Count();

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

                     //List<ProductividadList> liresult = Mresult
                     //                                .GroupBy(l => l.SKU)
                     //                                    .Select(cl => new ProductividadList
                     //                                    {
                     //                                        SKU = cl.First().SKU,
                     //                                        Id = cl.First().Id,
                     //                                        Produccion = cl.Sum(c => c.Produccion),
                     //                                        Unidades_de_Scrap = cl.Sum(c => c.Unidades_de_Scrap),
                     //                                        Turno = cl.Count().ToString(),
                     //                                        Flujo = cl.First().Flujo,
                     //                                    }).OrderBy(l => l.SKU).ToList();

                     //Boolean condition2 = true;
                     //for (int i = 0; i < liresult.Count; i++)
                     for (int i = 0; i < effData.Count; i++)
                     {
                        var products = _context.Product.Where(p => p.Sku == effData[i].SKU).FirstOrDefault() ?? new Product();

                        if (products == null)
                        {
                           return Json(new { success = false, msg = $"There is no product with the SKU {ProductivityListSorted[i].SKU}" });
                        }

                        //var Mresultdeta = _context.Productividad_Master.Where(o => o.Id == liresult[i].Id).FirstOrDefault();
                        List<DowntimeReason> cambioSum = new List<DowntimeReason>();
                        //for (int k = 0; k < Mresult.Count; k++)
                        //{
                        //    if (Mresult[k].SKU == liresult[i].SKU)
                        //    {
                        //        SqlParameter PId2 = new SqlParameter("PId", (object)Mresult[k].Id ?? DBNull.Value);
                        //        SqlParameter SKU2 = new SqlParameter("SKU", (object)Mresult[k].SKU ?? DBNull.Value);
                        //        var dresult = _PCLrepository.ExecuteStoredProcedure<ProductividadChildById>("GetProductividadbyIdSKU", PId2, SKU2).ToList();
                        //        dresult = dresult.Where(l => lstcat.Contains(l.Categoria)).ToList();
                        //        cambioSum = dresult
                        //               .GroupBy(l => l.PMID)
                        //                   .Select(cl => new Productividad_Child
                        //                   {
                        //                       Minutos = cl.Sum(c => c.Minutos),
                        //                       PMID = cl.First().PMID,
                        //                   }).ToList();

                        //        if (cambioSum.Count > 0)
                        //        {
                        //            cambioMin = cambioMin + cambioSum[0].Minutos;
                        //        }
                        //    }
                        //}

                        Cell cell21 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(ColorConstants.WHITE)
                           .Add(new Paragraph(effData[i].SKU.ToString()));

                        Cell cell22 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(ColorConstants.WHITE)
                           .Add(new Paragraph(products?.StandardSpeed.ToString()));

                        Cell cell23 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(ColorConstants.WHITE)
                           .Add(new Paragraph(products?.Flavour));

                        Cell cell24 = new Cell(1, 1)
                          .SetTextAlignment(TextAlignment.CENTER)
                          .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                          .SetBackgroundColor(ColorConstants.WHITE)
                          .Add(new Paragraph(products?.NetContent));

                        Cell cell25 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(ColorConstants.WHITE)
                           .Add(new Paragraph(products?.Packing));

                        Cell cell26 = new Cell(1, 1)
                          .SetTextAlignment(TextAlignment.RIGHT)
                          .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                          .SetBackgroundColor(ColorConstants.WHITE)
                          .Add(new Paragraph(effData[i].Production.ToString("#,##0")));

                        totalproduction = totalproduction + effData[i].Production;

                        decimal effciency = 0;
                        //if (((Convert.ToInt32(liresult[i].Turno) - (cambioMin / 60)) * productos.Velocidad_Estandar) == 0)
                        //if (((Convert.ToInt32(liresult[i].Turno) - (cambioMin / 60)) * (productos.Velocidad_Estandar/liresult[i].Flujo)) == 0)
                        //    effciency = 0;
                        //else
                        //    //effcincy = (liresult[i].Produccion / ((Convert.ToInt32(liresult[i].Turno) - (cambioMin / 60)) * productos.Velocidad_Estandar)) * 100;
                        //    effciency = (liresult[i].Produccion / ((Convert.ToInt32(liresult[i].Turno) - (cambioMin / 60)) * (productos.Velocidad_Estandar/liresult[i].Flujo))) * 100;

                        if (effData[i].EffDENSKU != 0)
                           effciency = (effData[i].Production / effData[i].EffDENSKU) * 100;

                        effciency = decimal.Round(effciency, 2, MidpointRounding.AwayFromZero);
                        effciency = Math.Round(effciency, 2);
                        estand = products?.StandardSpeed ?? 0; ;
                        totalhours = result.Count();
                        //decimal denominator = ((Convert.ToInt32(liresult[i].Turno) - (cambioMin / 60)) * productos.Velocidad_Estandar);
                        //totaldenomefficiency = totaldenomefficiency + ((Convert.ToInt32(liresult[i].Turno) - (cambioMin / 60)) * productos.Velocidad_Estandar);
                        //totaldenomefficiency = totaldenomefficiency + (((Convert.ToInt32(liresult[i].Turno) / Convert.ToDecimal(liresult[i].Flujo)) - (cambioMin / 60)) * productos.Velocidad_Estandar);
                        totaldenomefficiency = totaldenomefficiency + effData[i].EffDEN;
                        Cell cell27 = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.RIGHT)
                      .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                      .SetBackgroundColor(ColorConstants.WHITE)
                      .Add(new Paragraph(effciency.ToString() + "%"));

                        //Cell cell28 = new Cell(1, 1)
                        //   .SetTextAlignment(TextAlignment.RIGHT)
                        //   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        //  .SetBackgroundColor(ColorConstants.WHITE)
                        //   .Add(new Paragraph(productos.Unidades_por_Empaque.ToString()));

                        Cell cell29 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.RIGHT)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(ColorConstants.WHITE)
                           .Add(new Paragraph(effData[i].Scrap.ToString()));

                        totalscraps = totalscraps + effData[i].Scrap;

                        decimal scrapPer = 0;
                        if (effData[i].Production > 0)
                        {
                           if (products!.StandardSpeed > 0 || effData[i].Scrap > 0)
                           {
                              scrapPer = (effData[i].Scrap / ((effData[i].Production * products.UnitsPerPackage) + effData[i].Scrap)) * 100;

                              scrapPer = decimal.Round(scrapPer, 2, MidpointRounding.AwayFromZero);
                              scrapPer = Math.Round(scrapPer, 2);
                           }
                        }

                        totaldenomscraps = totaldenomscraps + ((effData[i].Production * products!.UnitsPerPackage) + effData[i].Scrap);
                        Cell cell30 = new Cell(1, 1)
                          .SetTextAlignment(TextAlignment.RIGHT)
                          .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                         .SetBackgroundColor(ColorConstants.WHITE)
                          .Add(new Paragraph(scrapPer + "%"));

                        table6.AddCell(cell21);
                        table6.AddCell(cell22);
                        table6.AddCell(cell23);
                        table6.AddCell(cell24);
                        table6.AddCell(cell25);
                        table6.AddCell(cell26);
                        table6.AddCell(cell27);
                        //table6.AddCell(cell28);
                        table6.AddCell(cell29);
                        table6.AddCell(cell30);
                     }
                     table6.AddCell(new Cell(1, 10).SetBorder(iText.Layout.Borders.Border.NO_BORDER).Add(new Paragraph(new Text("\n"))));
                     table6.AddCell(new Cell(1, 5).SetTextAlignment(TextAlignment.LEFT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold().Add(new Paragraph("TOTALS")));
                     table6.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold().Add(new Paragraph(totalproduction.ToString("#,##0"))));

                     decimal totalefficiancy;
                     if (totaldenomefficiency == 0)
                        totalefficiancy = 0;
                     else
                        totalefficiancy = (totalproduction / totaldenomefficiency) * 100;

                     //decimal totalefficiancy = 0;
                     //if (((totalhours - (excludeCat / 60)) * estand) == 0)
                     //    totalefficiancy = 0;
                     //else
                     //    totalefficiancy = (totalproduction / ((totalhours - (excludeCat / 60)) * estand)) * 100;

                     totalefficiancy = decimal.Round(totalefficiancy, 2, MidpointRounding.AwayFromZero);
                     totalefficiancy = Math.Round(totalefficiancy, 2);
                     table6.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold().Add(new Paragraph(totalefficiancy.ToString() + "%")));

                     decimal totalscrapPer;
                     if (totaldenomscraps == 0)
                        totalscrapPer = 0;
                     else
                        totalscrapPer = (totalscraps / totaldenomscraps) * 100;

                     totalscrapPer = decimal.Round(totalscrapPer, 2, MidpointRounding.AwayFromZero);
                     totalscrapPer = Math.Round(totalscrapPer, 2);
                     table6.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold().Add(new Paragraph(totalscraps.ToString())));
                     table6.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold().Add(new Paragraph(totalscrapPer.ToString() + "%")));

                     //decimal totalalcamenefficiancy = (totalproduction / ((totalhours - ((totalCambio + excludeCat) / 60) - (totalAlcamen / 60)) * estand)) * 100;

                     decimal totalalcamenefficiancy;
                     if ((totalhours - (excludeCat / 60)) == 0)
                        totalalcamenefficiancy = 0;
                     else
                        totalalcamenefficiancy = (totalproduction / ((totalhours - (excludeCat / 60)) * estand)) * 100;

                     totalalcamenefficiancy = decimal.Round(totalalcamenefficiancy, 2, MidpointRounding.AwayFromZero);
                     totalalcamenefficiancy = Math.Round(totalalcamenefficiancy, 2);
                     table6.AddCell(new Cell(1, 10).SetBorder(iText.Layout.Borders.Border.NO_BORDER).Add(new Paragraph(new Text("\n"))));
                     table6.AddCell(new Cell(1, 6).SetBorder(iText.Layout.Borders.Border.NO_BORDER).Add(new Paragraph(new Text(" "))));
                     table6.AddCell(new Cell(1, 1).SetBackgroundColor(ColorConstants.BLUE).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetFontColor(ColorConstants.WHITE).SetBold().Add(new Paragraph(totalefficiancy.ToString() + "%")));
                     table6.AddCell(new Cell(1, 3).SetBorder(iText.Layout.Borders.Border.NO_BORDER).Add(new Paragraph(new Text(" "))));

                     // -----------------------------------------------------------------------------------------------------
                     Paragraph newline7 = new Paragraph(new Text("\n"));
                     document.Add(newline7);
                     document.Add(table6);

                     // New line
                     Paragraph newline15 = new Paragraph(new Text("\n"));

                     document.Add(newline15);

                     /*                   Image img7 = new Image(ImageDataFactory
                                         .Create(@".\wwwroot\assets\media\Formula.png"))
                                         .SetTextAlignment(TextAlignment.CENTER);
                                       document.Add(img7); */
                  }
                  else
                  {
                     for (int i = 0; i < ProductivityListSorted.Count; i++)
                     {
                        Product products = _context.Product.Where(p => p.Sku == ProductivityListSorted[i].SKU).FirstOrDefault() ?? new Product();

                        if (products == null)
                        {
                           return Json(new { success = false, msg = $"There is no product with the SKU {ProductivityListSorted[i].SKU}" });
                        }

                        /*                         
                        if (i > 0)
                        {
                           if (Mresult[i].SKU != Mresult[i - 1].SKU)
                           {
                              condition = !condition;
                           }
                        } */

                        //var Mresultdeta = _context.Productividad_Master.Where(o => o.Id == Mresult[i].Id).FirstOrDefault();

                        Cell cell21 = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER)
                       .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                       .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                       .Add(new Paragraph(ProductivityListSorted[i].Schedule));

                        Cell cell22 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                           .Add(new Paragraph(ProductivityListSorted[i].SKU.ToString()));

                        Cell cell23 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                           .Add(new Paragraph(products.StandardSpeed.ToString()));

                        Cell cell24 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                           .Add(new Paragraph(products.Flavour));

                        Cell cell25 = new Cell(1, 1)
                          .SetTextAlignment(TextAlignment.CENTER)
                          .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                          .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                          .Add(new Paragraph(products.NetContent));

                        //Cell cell26 = new Cell(1, 1)
                        //   .SetTextAlignment(TextAlignment.CENTER)
                        //   .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        //   .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                        //   .Add(new Paragraph(productos.Empaque));

                        Cell cell27 = new Cell(1, 1)
                          .SetTextAlignment(TextAlignment.RIGHT)
                          .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                          .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                          .Add(new Paragraph(ProductivityListSorted[i].Production.ToString("#,##0")));

                        //Cell cell28 = new Cell(1, 1)
                        //  .SetTextAlignment(TextAlignment.RIGHT)
                        //  .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        //  .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                        //  .Add(new Paragraph(productos.Unidades_por_Empaque.ToString()));

                        Cell cell29 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.RIGHT)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                           .Add(new Paragraph(ProductivityListSorted[i].ScrapUnits.ToString()));

                        decimal scrapPer = 0;
                        if (ProductivityListSorted[i].Production > 0)
                        {
                           if (products.UnitsPerPackage > 0 || ProductivityListSorted[i].ScrapUnits > 0)
                           {
                              scrapPer = (ProductivityListSorted[i].ScrapUnits / ((ProductivityListSorted[i].Production * products.UnitsPerPackage) + ProductivityListSorted[i].ScrapUnits)) * 100;

                              scrapPer = decimal.Round(scrapPer, 2, MidpointRounding.AwayFromZero);
                              scrapPer = Math.Round(scrapPer, 2);
                           }
                        }

                        Cell cell30 = new Cell(1, 1)
                          .SetTextAlignment(TextAlignment.RIGHT)
                          .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                          .SetBackgroundColor(condition ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                          .Add(new Paragraph(scrapPer + "%"));

                        table.AddCell(cell21);
                        table.AddCell(cell22);
                        table.AddCell(cell23);
                        table.AddCell(cell24);
                        table.AddCell(cell25);
                        //table.AddCell(cell26);
                        table.AddCell(cell27);
                        //table.AddCell(cell28);
                        table.AddCell(cell29);
                        table.AddCell(cell30);

                        condition = !condition;
                     }
                     // -----------------------------------------------------------------------------------------------------
                     decimal totalminutes = 0;
                     decimal totalCambio = 0;
                     int cambiocount = 0;
                     Paragraph newline2 = new Paragraph(new Text("\n"));
                     document.Add(newline2);
                     document.Add(table);

                     Paragraph header3 = new Paragraph("DOWNTIME PER HOUR")
                       .SetTextAlignment(TextAlignment.LEFT)
                       .SetFontSize(10)
                       .SetBackgroundColor(ColorConstants.BLUE)
                       .SetPaddingLeft(5)
                       .SetFontColor(ColorConstants.WHITE);

                     Paragraph newline3 = new Paragraph(new Text("\n"));
                     document.Add(newline3);
                     document.Add(header3);

                     // Table child
                     Table table4 = new Table(5, false);
                     table4.SetWidth(UnitValue.CreatePercentValue(100));

                     Cell cellp1 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("HOUR ID"));
                     Cell cellp2 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("DESCRIPTION"));
                     Cell cellp3 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("CODE"));
                     Cell cellp4 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("SUBCATEGORY 2"));
                     Cell cellp5 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("MINUTES"));

                     table4.AddCell(cellp1);
                     table4.AddCell(cellp2);
                     table4.AddCell(cellp3);
                     table4.AddCell(cellp4);
                     table4.AddCell(cellp5);

                     Boolean condition1 = true;
                     for (int i = 0; i < ProductivityListSorted.Count; i++)
                     {
                        var Cresult = _PCLrepository.ExecuteStoredProcedure<DTODowntimeReason>("GetProductivitybyId",
                                       new SqlParameter("@ProductivityId", ProductivityListSorted[i].Id),
                                       new SqlParameter("@ProductSku", ProductivityListSorted[i].SKU)
                                       ).ToList();
                        int count = 0;
                        foreach (var item in Cresult)
                        {
                           count = count + 1;
                           Cell cell21 = new Cell(1, 1)
                              .SetTextAlignment(TextAlignment.CENTER)
                              .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                              .SetBackgroundColor(condition1 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                              .Add(new Paragraph(count == 1 ? ProductivityListSorted[i].Schedule : ""));

                           Cell cell22 = new Cell(1, 1)
                              .SetTextAlignment(TextAlignment.CENTER)
                              .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                              .SetBackgroundColor(condition1 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                              .Add(new Paragraph(item.Failure));

                           Cell cell23 = new Cell(1, 1)
                              .SetTextAlignment(TextAlignment.CENTER)
                              .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                              .SetBackgroundColor(condition1 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                              .Add(new Paragraph(item.Code));

                           Cell cell24 = new Cell(1, 1)
                              .SetTextAlignment(TextAlignment.CENTER)
                              .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                              .SetBackgroundColor(condition1 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                              .Add(new Paragraph(item.SubCategory2));

                           Cell cell25 = new Cell(1, 1)
                             .SetTextAlignment(TextAlignment.RIGHT)
                             .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                             .SetBackgroundColor(condition1 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
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

                           table4.AddCell(cell21);
                           table4.AddCell(cell22);
                           table4.AddCell(cell23);
                           table4.AddCell(cell24);
                           table4.AddCell(cell25);
                        }
                        condition1 = !condition1;
                     }

                     Paragraph newline4 = new Paragraph(new Text("\n"));
                     document.Add(newline4);
                     document.Add(table4);

                     totalminutes = totalminutes - totalCambio;

                     Table table5 = new Table(2, false);
                     table5.SetWidth(UnitValue.CreatePercentValue(100));
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
                        .Add(new Paragraph("TOTAL CHANGE"));
                     Cell cell54 = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetBold()
                        .Add(new Paragraph(totalCambio.ToString()));
                     Cell cell55 = new Cell(1, 1)
                         .SetTextAlignment(TextAlignment.LEFT)
                         .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                         .SetBold()
                         .Add(new Paragraph("NUMBER OF CHANGE"));
                     Cell cell56 = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph(changeOverSet.Count().ToString()));
                     table5.AddCell(cell51);
                     table5.AddCell(cell52);
                     table5.AddCell(cell53);
                     table5.AddCell(cell54);
                     table5.AddCell(cell55);
                     table5.AddCell(cell56);

                     Paragraph newline5 = new Paragraph(new Text("\n"));
                     document.Add(newline5);
                     document.Add(table5);

                     // table for calculation
                     Paragraph header6 = new Paragraph("TOTALS AND METRICS")
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetFontSize(10)
                        .SetBackgroundColor(ColorConstants.BLUE)
                        .SetPaddingLeft(5)
                        .SetFontColor(ColorConstants.WHITE);

                     Paragraph newline6 = new Paragraph(new Text("\n"));

                     document.Add(newline6);
                     document.Add(header6);

                     // Table Production
                     //Table table6 = new Table(10, false);
                     Table table6 = new Table(9, false);
                     table6.SetWidth(UnitValue.CreatePercentValue(100));
                     Cell cell61 = new Cell(1, 1)
                         .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("SKU"));
                     Cell cell62 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("STANDARD"));
                     Cell cell63 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("FLAVOUR"));
                     Cell cell64 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("CONTENT"));
                     Cell cell65 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("PACKING"));
                     Cell cell66 = new Cell(1, 1)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Paragraph("PRODUCTION"));
                     Cell cell67 = new Cell(1, 1)
                        .SetBold()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .Add(new Paragraph("EFFICIENCY"));

                     //Cell cell68 = new Cell(1, 1)
                     //.SetBold()
                     //.SetTextAlignment(TextAlignment.CENTER)
                     //.SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                     //.Add(new Paragraph("Unidades Empaque"));
                     Cell cell69 = new Cell(1, 1)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph("SCRAP UNITS"));
                     Cell cell70 = new Cell(1, 1)
                        .SetBold()
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Paragraph("SCRAP %"));

                     table6.AddCell(cell61);
                     table6.AddCell(cell62);
                     table6.AddCell(cell63);
                     table6.AddCell(cell64);
                     table6.AddCell(cell65);
                     table6.AddCell(cell66);
                     table6.AddCell(cell67);
                     //table6.AddCell(cell68);
                     table6.AddCell(cell69);
                     table6.AddCell(cell70);
                     decimal totalproduction = 0;
                     int totalscraps = 0;
                     decimal totaldenomefficiency = 0;
                     decimal totaldenomscraps = 0;
                     int a = ProductivityListSorted.GroupBy(l => l.SKU).Count();
                     List<DTOProductivityList> liresult = ProductivityListSorted
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
                     for (int i = 0; i < liresult.Count; i++)
                     {
                        Product products = _context.Product.Where(p => p.Sku == liresult[i].SKU).FirstOrDefault() ?? new Product();

                        if (products == null)
                        {
                           return Json(new { success = false, msg = $"There is no product with the SKU {ProductivityListSorted[i].SKU}" });
                        }

                        //var Mresultdeta = _context.Productividad_Master.Where(o => o.Id == liresult[i].Id).FirstOrDefault();
                        List<DowntimeReason> cambioSum = new List<DowntimeReason>();
                        //List<Productividad_Child> excludecat = new List<Productividad_Child>();
                        decimal cambioMin = 0;
                        for (int k = 0; k < ProductivityListSorted.Count; k++)
                        {
                           if (ProductivityListSorted[k].SKU == liresult[i].SKU)
                           {

                              var dresult = _PCLrepository.ExecuteStoredProcedure<DTODowntimeReason>("GetProductivitybyId",
                                       new SqlParameter("@ProductivityId", ProductivityListSorted[k].Id),
                                       new SqlParameter("@ProductSku", ProductivityListSorted[k].SKU)
                                       ).ToList();

                              dresult = dresult.Where(l => lstcat.Contains(l.DowntimeCategoryId)).ToList();
                              //dresult = dresult.Where(l => Regex.IsMatch(l.Categoria, cat.ToString())).ToList();
                              cambioSum = dresult
                                     .GroupBy(l => l.ProductivityID)
                                         .Select(cl => new DowntimeReason
                                         {
                                            Minutes = cl.Sum(c => c.Minutes),
                                            ProductivityID = cl.First().ProductivityID,
                                         }).ToList();

                              if (cambioSum.Count > 0)
                              {
                                 cambioMin = cambioMin + cambioSum[0].Minutes;
                              }
                           }
                        }

                        Cell cell21 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                           .Add(new Paragraph(liresult[i].SKU.ToString()));

                        Cell cell22 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.LEFT)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                           .Add(new Paragraph(products?.StandardSpeed.ToString()));

                        Cell cell23 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                           .Add(new Paragraph(products?.Flavour));

                        Cell cell24 = new Cell(1, 1)
                          .SetTextAlignment(TextAlignment.CENTER)
                          .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                          .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                          .Add(new Paragraph(products?.NetContent));

                        Cell cell25 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                           .Add(new Paragraph(products?.Packing));

                        Cell cell26 = new Cell(1, 1)
                          .SetTextAlignment(TextAlignment.RIGHT)
                          .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                          .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                          .Add(new Paragraph(liresult[i].Production.ToString("#,##0")));

                        totalproduction = totalproduction + liresult[i].Production;

                        decimal effcincy = 0;
                        if (((Convert.ToInt32(liresult[i].Shift) - (cambioMin / 60)) * products?.StandardSpeed) == 0)
                           effcincy = 0;
                        else
                        {
                           effcincy = liresult[i].Production / ((Convert.ToInt32(liresult[i].Shift) - (cambioMin / 60)) * products!.StandardSpeed) * 100;
                        }
                        effcincy = decimal.Round(effcincy, 2, MidpointRounding.AwayFromZero);
                        effcincy = Math.Round(effcincy, 2);
                        if (effcincy > 100)
                        {
                           effcincy = 100;
                        }
                        totaldenomefficiency = totaldenomefficiency + ((Convert.ToInt32(liresult[i].Shift) - ((cambioMin) / 60)) * products!.StandardSpeed);
                        Cell cell27 = new Cell(1, 1)
                          .SetTextAlignment(TextAlignment.RIGHT)
                          .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                          .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                          .Add(new Paragraph(effcincy.ToString() + "%"));

                        //Cell cell28 = new Cell(1, 1)
                        //    .SetTextAlignment(TextAlignment.RIGHT)
                        //    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        //   .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                        //    .Add(new Paragraph(productos.Unidades_por_Empaque.ToString()));

                        Cell cell29 = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.RIGHT)
                           .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                           .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                           .Add(new Paragraph(liresult[i].ScrapUnits.ToString()));

                        totalscraps = totalscraps + liresult[i].ScrapUnits;

                        decimal scrapPer = 0;
                        if (liresult[i].Production > 0)
                        {
                           if (products.UnitsPerPackage > 0 || liresult[i].ScrapUnits > 0)
                           {
                              scrapPer = liresult[i].ScrapUnits / ((liresult[i].Production * products.UnitsPerPackage) + liresult[i].ScrapUnits) * 100;

                              scrapPer = decimal.Round(scrapPer, 2, MidpointRounding.AwayFromZero);
                              scrapPer = Math.Round(scrapPer, 2);
                           }
                        }

                        totaldenomscraps = totaldenomscraps + (liresult[i].Production * products.UnitsPerPackage) + liresult[i].ScrapUnits;

                        Cell cell30 = new Cell(1, 1)
                          .SetTextAlignment(TextAlignment.RIGHT)
                          .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                         .SetBackgroundColor(condition2 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE)
                          .Add(new Paragraph(scrapPer + "%"));

                        table6.AddCell(cell21);
                        table6.AddCell(cell22);
                        table6.AddCell(cell23);
                        table6.AddCell(cell24);
                        table6.AddCell(cell25);
                        table6.AddCell(cell26);
                        table6.AddCell(cell27);
                        //table6.AddCell(cell28);
                        table6.AddCell(cell29);
                        table6.AddCell(cell30);

                        condition2 = !condition2;
                     }
                     table6.AddCell(new Cell(1, 10).SetBorder(iText.Layout.Borders.Border.NO_BORDER).Add(new Paragraph(new Text("\n"))));
                     table6.AddCell(new Cell(1, 5).SetTextAlignment(TextAlignment.LEFT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold()
                     .Add(new Paragraph("TOTALS")));

                     table6.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold()
                     .Add(new Paragraph(totalproduction.ToString("#,##0"))));

                     decimal totalefficiancy;
                     if (totaldenomefficiency == 0)
                        totalefficiancy = 0;
                     else
                        totalefficiancy = (totalproduction / totaldenomefficiency) * 100;
                     totalefficiancy = decimal.Round(totalefficiancy, 2, MidpointRounding.AwayFromZero);
                     totalefficiancy = Math.Round(totalefficiancy, 2);

                     decimal totalscrapPer;
                     if (totaldenomscraps == 0)
                        totalscrapPer = 0;
                     else
                        totalscrapPer = (totalscraps / totaldenomscraps) * 100;
                     totalscrapPer = decimal.Round(totalscrapPer, 2, MidpointRounding.AwayFromZero);
                     totalscrapPer = Math.Round(totalscrapPer, 2);

                     table6.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold()
                     .Add(new Paragraph(totalefficiancy.ToString() + "%")));
                     table6.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold()
                     .Add(new Paragraph(totalscraps.ToString())));
                     table6.AddCell(new Cell(1, 1).SetTextAlignment(TextAlignment.RIGHT).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetBold()
                     .Add(new Paragraph(totalscrapPer.ToString() + "%")));
                     // -----------------------------------------------------------------------------------------------------
                     Paragraph newline7 = new Paragraph(new Text("\n"));
                     document.Add(newline7);
                     document.Add(table6);

                     // New line
                     Paragraph newline15 = new Paragraph(new Text("\n"));

                     document.Add(newline15);

                     //IMG FORMULA
                     /* string imageFormulaPath = System.IO.Path.GetFullPath(@"./Image/Formula.png");
                     Image img5 = new Image(ImageDataFactory
                       .Create(imageFormulaPath))
                       .SetTextAlignment(TextAlignment.CENTER);
                     document.Add(img5); */
                  }
                  // footer
                  Paragraph headerLast = new Paragraph("footer")
                     .SetTextAlignment(TextAlignment.LEFT)
                     .SetFontSize(10)
                     .SetBackgroundColor(ColorConstants.GRAY)
                     .SetFontColor(ColorConstants.GRAY)
                     .SetPaddingLeft(5);

                  // New line
                  Paragraph newline10 = new Paragraph(new Text("\n"));

                  document.Add(newline10);
                  document.Add(headerLast);

                  // Close document
                  document.Close();
                  document.Flush();
                  pdfBytes = stream.ToArray();
               }
               /*string base64 = Convert.ToBase64String(pdfBytes, 0, pdfBytes.Length);
                  return Content(base64, "application/pdf"); */
               return File(pdfBytes, "application/pdf");
            }
            else { return Json(new { success = false, msg = "No se encontraron registros" }); }
         }
         catch (Exception ex)
         {
            Console.WriteLine("Error general: " + ex.Message);
            return Json(new { success = false, error = "Error general: " + ex.Message, });
         }

      }
   }
}