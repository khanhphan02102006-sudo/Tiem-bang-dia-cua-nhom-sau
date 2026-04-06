using System.ComponentModel.DataAnnotations;

namespace VideoRentalMVC.Models;

public class Khach
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Tên khách")]
    public string TenKhach { get; set; } = string.Empty;

    [StringLength(250)]
    [Display(Name = "Địa chỉ")]
    public string DiaChi { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Phone]
    [Display(Name = "Điện thoại")]
    public string DienThoai { get; set; } = string.Empty;

    public string? IdentityUserId { get; set; }

    public ICollection<Thue> Thues { get; set; } = new List<Thue>();
}
