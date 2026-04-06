using System.ComponentModel.DataAnnotations;

namespace VideoRentalMVC.Models;

public class Phim
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Tên phim")]
    public string TenPhim { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Thể loại")]
    public string TheLoai { get; set; } = string.Empty;

    [Range(1900, 2100)]
    [Display(Name = "Năm sản xuất")]
    public int NamSanXuat { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Nước sản xuất")]
    public string NuocSanXuat { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Mô tả")]
    public string MoTa { get; set; } = string.Empty;

    [Display(Name = "Phim bộ/lẻ")]
    public bool PhimBoLe { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    [Display(Name = "Giá vốn")]
    public decimal GiaVon { get; set; }

    [StringLength(500)]
    [Display(Name = "Ảnh bìa URL")]
    public string? AnhBiaUrl { get; set; }

    public ICollection<Bang> Bangs { get; set; } = new List<Bang>();
}
