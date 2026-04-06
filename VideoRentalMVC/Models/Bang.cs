using System.ComponentModel.DataAnnotations;

namespace VideoRentalMVC.Models;

public class Bang
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Tên băng")]
    public string TenBang { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Tình trạng")]
    public string TinhTrang { get; set; } = "Có sẵn";

    [Display(Name = "Mã phim")]
    public int MaPhim { get; set; }

    public Phim? Phim { get; set; }

    public ICollection<Thue> Thues { get; set; } = new List<Thue>();
}
