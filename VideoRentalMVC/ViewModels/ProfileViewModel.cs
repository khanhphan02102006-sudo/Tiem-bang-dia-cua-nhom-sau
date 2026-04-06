using System.ComponentModel.DataAnnotations;

namespace VideoRentalMVC.ViewModels;

public class ProfileViewModel
{
    [Display(Name = "Tên đăng nhập")]
    public string UserName { get; set; } = string.Empty;

    [Display(Name = "Tên khách")]
    [StringLength(120)]
    public string? TenKhach { get; set; }

    [Display(Name = "Địa chỉ")]
    [StringLength(250)]
    public string? DiaChi { get; set; }

    [Display(Name = "Email")]
    [EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "Số điện thoại")]
    [Phone]
    public string? PhoneNumber { get; set; }
}
