$path = Join-Path $PSScriptRoot 'Views\Home\Index.cshtml'
$content = Get-Content -Path $path -Raw
$old = @'
</section>

<section class="home-activity mb-3">
'@
$new = @'
</section>

<section class="home-trending mb-3">
    <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-2">
        <h6 class="mb-0 fw-bold text-primary">Phim được thuê nhiều</h6>
        <small class="text-muted">Xem nhanh những phim đang được quan tâm</small>
    </div>
    <div class="home-trending-grid">
        @if (topFilms != null && topFilms.Any())
        {
            foreach (var film in topFilms)
            {
                if (film.Id > 0)
                {
                    <a class="home-trending-card text-decoration-none" asp-controller="Phims" asp-action="Details" asp-route-id="@film.Id">
                        <img src="@film.AnhBiaUrl" alt="@film.TenPhim" loading="lazy" decoding="async" />
                        <div class="home-trending-card-body">
                            <div class="d-flex justify-content-between align-items-start gap-2">
                                <strong>@film.TenPhim</strong>
                                <span class="soft-badge success">@film.SoLanThue</span>
                            </div>
                            <small>@film.TheLoai</small>
                        </div>
                    </a>
                }
                else
                {
                    <div class="home-trending-card is-placeholder">
                        <img src="@film.AnhBiaUrl" alt="@film.TenPhim" loading="lazy" decoding="async" />
                        <div class="home-trending-card-body">
                            <div class="d-flex justify-content-between align-items-start gap-2">
                                <strong>@film.TenPhim</strong>
                                <span class="soft-badge neutral">@film.SoLanThue</span>
                            </div>
                            <small>@film.TheLoai</small>
                        </div>
                    </div>
                }
            }
        }
    </div>
</section>

<section class="home-genre-strip mb-3">
    <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-2">
        <h6 class="mb-0 fw-bold text-primary">Thể loại đang nổi bật</h6>
        <small class="text-muted">Nhìn nhanh các thể loại có nhiều phim hơn</small>
    </div>
    <div class="home-genre-grid">
        @if (topGenres != null && topGenres.Any())
        {
            var genreIndex = 0;
            foreach (var genre in topGenres)
            {
                genreIndex++;
                <div class="home-genre-card genre-@genreIndex">
                    <div class="home-genre-card-top">
                        <strong>@genre.TheLoai</strong>
                        <span class="soft-badge info">@genre.SoLuong</span>
                    </div>
                    <div class="home-genre-bar"><span style="width:@Math.Min(100, genre.SoLuong * 10)%"></span></div>
                </div>
            }
        }
    </div>
</section>

<section class="home-activity mb-3">
'@
if (-not $content.Contains($old)) { throw 'anchor not found' }
$content = $content.Replace($old, $new)
$content = $content.Replace('`r`n', "`r`n")
Set-Content -Path $path -Value $content -Encoding utf8 -Force