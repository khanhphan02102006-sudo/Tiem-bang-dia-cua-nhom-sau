namespace VideoRentalMVC.ViewModels;

public class HomeTrendingFilmViewModel
{
    public int Id { get; set; }
    public string TenPhim { get; set; } = string.Empty;
    public string AnhBiaUrl { get; set; } = string.Empty;
    public int SoLanThue { get; set; }
    public string TheLoai { get; set; } = string.Empty;
}
