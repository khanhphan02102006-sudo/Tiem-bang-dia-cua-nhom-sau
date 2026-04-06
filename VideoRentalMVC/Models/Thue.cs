using System.ComponentModel.DataAnnotations;

namespace VideoRentalMVC.Models;

public class Thue
{
    public int Id { get; set; }

    [Display(Name = "Ngày thuê")]
    [DataType(DataType.Date)]
    public DateTime NgayThue { get; set; }

    [Display(Name = "Hạn trả")]
    [DataType(DataType.Date)]
    public DateTime HanTra { get; set; }

    [Display(Name = "Ngày trả")]
    [DataType(DataType.Date)]
    public DateTime? NgayTra { get; set; }

    [Display(Name = "Đã trả tiền thuê")]
    public bool DaTraTienThue { get; set; }

    [Display(Name = "Phí trễ hạn")]
    public decimal PhiTreHan { get; set; }

    [Display(Name = "Tổng tiền")]
    public decimal TongTien { get; set; }

    [Display(Name = "Mã băng")]
    public int MaBang { get; set; }
    public Bang? MaBangNavigation { get; set; }

    [Display(Name = "Mã khách")]
    public int MaKhach { get; set; }
    public Khach? MaKhachNavigation { get; set; }
}
