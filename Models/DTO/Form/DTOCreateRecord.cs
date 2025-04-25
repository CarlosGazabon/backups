namespace Inventio.Models.DTO.Form
{
  public class DTOProduct
  {
    public int Id { get; set; }
    public string Sku { get; set; } = default!;
    public string NetContent { get; set; } = default!;
    public string Packing { get; set; } = default!;
    public string Flavour { get; set; } = default!;
    public bool Inactive { get; set; }
    public int UnitsPerPackage { get; set; }
    public int StandardSpeed { get; set; }
    public int LineID { get; set; }
  }
  public class DTOProductFlow
  {
    public required DTOProduct Product { get; set; }
    public int FlowIndex { get; set; }
    public int Production { get; set; }
    public int BottleScrap { get; set; }
    public int CanScrap { get; set; }
    public int PouchScrap { get; set; }
    public int PreformScrap { get; set; }
    public int TotalScrap { get; set; }
  }
  public class DTOCreateRecord
  {
    public DateTime Date { get; set; }
    public int LineID { get; set; }
    public int Flow { get; set; }
    public int ShiftID { get; set; }
    public int HourID { get; set; }
    public int SupervisorID { get; set; }
    public string Comment { get; set; } = default!;
    public required List<DTOProductFlow> ProductFlows { get; set; }
  }
}