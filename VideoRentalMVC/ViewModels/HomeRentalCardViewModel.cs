namespace VideoRentalMVC.ViewModels;

public class HomeRentalCardViewModel
{
    public int Id { get; set; }
    public string TenBang { get; set; } = string.Empty;
    public string TenKhach { get; set; } = string.Empty;
    public DateTime HanTra { get; set; }
    public DateTime? NgayTra { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsNearDue { get; set; }
}
