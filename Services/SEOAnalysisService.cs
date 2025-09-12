using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using WEB_CV.Data;
using WEB_CV.Models;

namespace WEB_CV.Services
{
    public class SEOAnalysisService : ISEOAnalysisService
    {
        private readonly NewsDbContext _db;

        public SEOAnalysisService(NewsDbContext db)
        {
            _db = db;
        }

        public async Task<SEOAnalysisVM> AnalyzeBaiVietAsync(int baiVietId)
        {
            var baiViet = await _db.BaiViets.FindAsync(baiVietId);
            if (baiViet == null)
                throw new ArgumentException("Bài viết không tồn tại");

            return await AnalyzeBaiVietContentAsync(baiViet.TieuDe, baiViet.TomTat, baiViet.NoiDung, baiViet.AnhTieuDeAlt);
        }

        public async Task<SEOAnalysisVM> AnalyzeBaiVietContentAsync(string tieuDe, string? tomTat, string noiDung, string? anhTieuDeAlt)
        {
            var analysis = new SEOAnalysisVM
            {
                TieuDe = tieuDe
            };

            // Phân tích tiêu đề
            analysis.TieuDeChiTiet = AnalyzeTieuDe(tieuDe);

            // Phân tích tóm tắt
            analysis.TomTatChiTiet = AnalyzeTomTat(tomTat);

            // Phân tích nội dung
            analysis.NoiDungChiTiet = AnalyzeNoiDung(noiDung);

            // Phân tích từ khóa
            analysis.TuKhoaChiTiet = AnalyzeTuKhoa(tieuDe, tomTat, noiDung);

            // Phân tích cấu trúc
            analysis.CauTrucChiTiet = AnalyzeCauTruc(noiDung);

            // Phân tích hình ảnh
            analysis.HinhAnhChiTiet = AnalyzeHinhAnh(anhTieuDeAlt);

            // Phân tích liên kết
            analysis.LienKetChiTiet = AnalyzeLienKet(noiDung);

            // Phân tích độ dài
            analysis.DoDaiChiTiet = AnalyzeDoDai(noiDung);

            // Tính điểm tổng
            analysis.TongDiem = CalculateTongDiem(analysis);
            analysis.MauSac = GetMauSac(analysis.TongDiem);
            analysis.TrangThai = GetTrangThai(analysis.TongDiem);

            // Tạo gợi ý cải thiện
            analysis.GoiY = GenerateGoiY(analysis);

            return analysis;
        }

        private SEOChiTietDiem AnalyzeTieuDe(string tieuDe)
        {
            var diem = new SEOChiTietDiem();
            var ghiChu = new List<string>();

            if (string.IsNullOrWhiteSpace(tieuDe))
            {
                diem.Diem = 0;
                diem.ThanhCong = false;
                ghiChu.Add("Tiêu đề không được để trống");
            }
            else
            {
                var doDai = tieuDe.Length;
                var diemTieuDe = 0;

                // Kiểm tra độ dài tiêu đề (50-60 ký tự là tối ưu)
                if (doDai >= 30 && doDai <= 60)
                {
                    diemTieuDe += 40;
                    ghiChu.Add("✓ Độ dài tiêu đề tối ưu (30-60 ký tự)");
                }
                else if (doDai < 30)
                {
                    diemTieuDe += 20;
                    ghiChu.Add("⚠ Tiêu đề quá ngắn, nên có ít nhất 30 ký tự");
                }
                else if (doDai > 60)
                {
                    diemTieuDe += 20;
                    ghiChu.Add("⚠ Tiêu đề quá dài, nên dưới 60 ký tự");
                }

                // Kiểm tra từ khóa trong tiêu đề
                if (tieuDe.Contains("SEO", StringComparison.OrdinalIgnoreCase) || 
                    tieuDe.Contains("tối ưu", StringComparison.OrdinalIgnoreCase) ||
                    tieuDe.Contains("marketing", StringComparison.OrdinalIgnoreCase))
                {
                    diemTieuDe += 30;
                    ghiChu.Add("✓ Tiêu đề chứa từ khóa liên quan");
                }
                else
                {
                    diemTieuDe += 10;
                    ghiChu.Add("⚠ Nên thêm từ khóa chính vào tiêu đề");
                }

                // Kiểm tra ký tự đặc biệt
                if (tieuDe.Contains("!") || tieuDe.Contains("?"))
                {
                    diemTieuDe += 10;
                    ghiChu.Add("✓ Tiêu đề có ký tự thu hút sự chú ý");
                }

                // Kiểm tra chữ hoa
                if (char.IsUpper(tieuDe[0]))
                {
                    diemTieuDe += 10;
                    ghiChu.Add("✓ Tiêu đề bắt đầu bằng chữ hoa");
                }
                else
                {
                    ghiChu.Add("⚠ Nên bắt đầu tiêu đề bằng chữ hoa");
                }

                // Kiểm tra số trong tiêu đề
                if (Regex.IsMatch(tieuDe, @"\d"))
                {
                    diemTieuDe += 10;
                    ghiChu.Add("✓ Tiêu đề chứa số (thu hút click)");
                }

                diem.Diem = Math.Min(diemTieuDe, 100);
                diem.ThanhCong = diem.Diem >= 70;
            }

            diem.MauSac = GetMauSac(diem.Diem);
            diem.TrangThai = GetTrangThai(diem.Diem);
            diem.GhiChu = ghiChu;

            return diem;
        }

        private SEOChiTietDiem AnalyzeTomTat(string? tomTat)
        {
            var diem = new SEOChiTietDiem();
            var ghiChu = new List<string>();

            if (string.IsNullOrWhiteSpace(tomTat))
            {
                diem.Diem = 0;
                diem.ThanhCong = false;
                ghiChu.Add("Tóm tắt không được để trống");
            }
            else
            {
                var doDai = tomTat.Length;
                var diemTomTat = 0;

                // Kiểm tra độ dài tóm tắt (150-160 ký tự là tối ưu)
                if (doDai >= 120 && doDai <= 160)
                {
                    diemTomTat += 50;
                    ghiChu.Add("✓ Độ dài tóm tắt tối ưu (120-160 ký tự)");
                }
                else if (doDai < 120)
                {
                    diemTomTat += 30;
                    ghiChu.Add("⚠ Tóm tắt quá ngắn, nên có ít nhất 120 ký tự");
                }
                else if (doDai > 160)
                {
                    diemTomTat += 30;
                    ghiChu.Add("⚠ Tóm tắt quá dài, nên dưới 160 ký tự");
                }

                // Kiểm tra từ khóa trong tóm tắt
                if (tomTat.Contains("SEO", StringComparison.OrdinalIgnoreCase) || 
                    tomTat.Contains("tối ưu", StringComparison.OrdinalIgnoreCase))
                {
                    diemTomTat += 30;
                    ghiChu.Add("✓ Tóm tắt chứa từ khóa chính");
                }
                else
                {
                    diemTomTat += 10;
                    ghiChu.Add("⚠ Nên thêm từ khóa chính vào tóm tắt");
                }

                // Kiểm tra câu hỏi trong tóm tắt
                if (tomTat.Contains("?"))
                {
                    diemTomTat += 10;
                    ghiChu.Add("✓ Tóm tắt chứa câu hỏi (thu hút click)");
                }

                // Kiểm tra dấu chấm câu
                if (tomTat.EndsWith(".") || tomTat.EndsWith("!"))
                {
                    diemTomTat += 10;
                    ghiChu.Add("✓ Tóm tắt kết thúc đúng dấu câu");
                }

                diem.Diem = Math.Min(diemTomTat, 100);
                diem.ThanhCong = diem.Diem >= 70;
            }

            diem.MauSac = GetMauSac(diem.Diem);
            diem.TrangThai = GetTrangThai(diem.Diem);
            diem.GhiChu = ghiChu;

            return diem;
        }

        private SEOChiTietDiem AnalyzeNoiDung(string noiDung)
        {
            var diem = new SEOChiTietDiem();
            var ghiChu = new List<string>();

            if (string.IsNullOrWhiteSpace(noiDung))
            {
                diem.Diem = 0;
                diem.ThanhCong = false;
                ghiChu.Add("Nội dung không được để trống");
            }
            else
            {
                var doDai = noiDung.Length;
                var diemNoiDung = 0;

                // Kiểm tra độ dài nội dung (300+ từ là tối ưu)
                var soTu = noiDung.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                if (soTu >= 300)
                {
                    diemNoiDung += 30;
                    ghiChu.Add($"✓ Nội dung đủ dài ({soTu} từ)");
                }
                else if (soTu >= 150)
                {
                    diemNoiDung += 20;
                    ghiChu.Add($"⚠ Nội dung hơi ngắn ({soTu} từ), nên có ít nhất 300 từ");
                }
                else
                {
                    diemNoiDung += 10;
                    ghiChu.Add($"⚠ Nội dung quá ngắn ({soTu} từ), nên có ít nhất 300 từ");
                }

                // Kiểm tra heading tags
                var h1Count = Regex.Matches(noiDung, @"<h1[^>]*>", RegexOptions.IgnoreCase).Count;
                var h2Count = Regex.Matches(noiDung, @"<h2[^>]*>", RegexOptions.IgnoreCase).Count;
                var h3Count = Regex.Matches(noiDung, @"<h3[^>]*>", RegexOptions.IgnoreCase).Count;

                if (h1Count == 1)
                {
                    diemNoiDung += 20;
                    ghiChu.Add("✓ Có đúng 1 thẻ H1");
                }
                else if (h1Count == 0)
                {
                    ghiChu.Add("⚠ Nên có 1 thẻ H1 trong nội dung");
                }
                else
                {
                    ghiChu.Add("⚠ Chỉ nên có 1 thẻ H1");
                }

                if (h2Count >= 2)
                {
                    diemNoiDung += 15;
                    ghiChu.Add($"✓ Có {h2Count} thẻ H2 (tốt cho cấu trúc)");
                }
                else if (h2Count == 1)
                {
                    diemNoiDung += 10;
                    ghiChu.Add("⚠ Nên có ít nhất 2 thẻ H2");
                }

                if (h3Count >= 1)
                {
                    diemNoiDung += 10;
                    ghiChu.Add($"✓ Có {h3Count} thẻ H3");
                }

                // Kiểm tra đoạn văn
                var pCount = Regex.Matches(noiDung, @"<p[^>]*>", RegexOptions.IgnoreCase).Count;
                if (pCount >= 3)
                {
                    diemNoiDung += 10;
                    ghiChu.Add($"✓ Có {pCount} đoạn văn");
                }
                else
                {
                    ghiChu.Add("⚠ Nên chia nội dung thành nhiều đoạn văn");
                }

                // Kiểm tra danh sách
                var ulCount = Regex.Matches(noiDung, @"<ul[^>]*>", RegexOptions.IgnoreCase).Count;
                var olCount = Regex.Matches(noiDung, @"<ol[^>]*>", RegexOptions.IgnoreCase).Count;
                if (ulCount > 0 || olCount > 0)
                {
                    diemNoiDung += 10;
                    ghiChu.Add("✓ Có sử dụng danh sách (tốt cho SEO)");
                }

                // Kiểm tra từ khóa trong nội dung
                var tuKhoaCount = Regex.Matches(noiDung, @"SEO|tối ưu|marketing", RegexOptions.IgnoreCase).Count;
                if (tuKhoaCount >= 3)
                {
                    diemNoiDung += 15;
                    ghiChu.Add($"✓ Từ khóa xuất hiện {tuKhoaCount} lần (tốt)");
                }
                else if (tuKhoaCount >= 1)
                {
                    diemNoiDung += 10;
                    ghiChu.Add($"⚠ Từ khóa chỉ xuất hiện {tuKhoaCount} lần, nên có ít nhất 3 lần");
                }
                else
                {
                    ghiChu.Add("⚠ Nên thêm từ khóa chính vào nội dung");
                }

                diem.Diem = Math.Min(diemNoiDung, 100);
                diem.ThanhCong = diem.Diem >= 70;
            }

            diem.MauSac = GetMauSac(diem.Diem);
            diem.TrangThai = GetTrangThai(diem.Diem);
            diem.GhiChu = ghiChu;

            return diem;
        }

        private SEOChiTietDiem AnalyzeTuKhoa(string tieuDe, string? tomTat, string noiDung)
        {
            var diem = new SEOChiTietDiem();
            var ghiChu = new List<string>();

            var tuKhoaChinh = "SEO"; // Có thể mở rộng để tự động phát hiện từ khóa
            var diemTuKhoa = 0;

            // Kiểm tra từ khóa trong tiêu đề
            if (tieuDe.Contains(tuKhoaChinh, StringComparison.OrdinalIgnoreCase))
            {
                diemTuKhoa += 30;
                ghiChu.Add("✓ Từ khóa có trong tiêu đề");
            }
            else
            {
                ghiChu.Add("⚠ Từ khóa chưa có trong tiêu đề");
            }

            // Kiểm tra từ khóa trong tóm tắt
            if (!string.IsNullOrEmpty(tomTat) && tomTat.Contains(tuKhoaChinh, StringComparison.OrdinalIgnoreCase))
            {
                diemTuKhoa += 20;
                ghiChu.Add("✓ Từ khóa có trong tóm tắt");
            }
            else
            {
                ghiChu.Add("⚠ Từ khóa chưa có trong tóm tắt");
            }

            // Kiểm tra mật độ từ khóa trong nội dung
            var noiDungKhongHTML = Regex.Replace(noiDung, @"<[^>]+>", " ");
            var soTu = noiDungKhongHTML.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var soTuKhoa = Regex.Matches(noiDungKhongHTML, tuKhoaChinh, RegexOptions.IgnoreCase).Count;
            var matDoTuKhoa = soTu > 0 ? (double)soTuKhoa / soTu * 100 : 0;

            if (matDoTuKhoa >= 1 && matDoTuKhoa <= 3)
            {
                diemTuKhoa += 30;
                ghiChu.Add($"✓ Mật độ từ khóa tối ưu ({matDoTuKhoa:F1}%)");
            }
            else if (matDoTuKhoa < 1)
            {
                diemTuKhoa += 10;
                ghiChu.Add($"⚠ Mật độ từ khóa thấp ({matDoTuKhoa:F1}%), nên tăng lên 1-3%");
            }
            else
            {
                diemTuKhoa += 10;
                ghiChu.Add($"⚠ Mật độ từ khóa cao ({matDoTuKhoa:F1}%), nên giảm xuống 1-3%");
            }

            // Kiểm tra từ khóa LSI (từ khóa liên quan)
            var tuKhoaLienQuan = new[] { "tối ưu", "marketing", "tìm kiếm", "google", "website" };
            var soTuKhoaLienQuan = tuKhoaLienQuan.Count(tk => noiDungKhongHTML.Contains(tk, StringComparison.OrdinalIgnoreCase));
            
            if (soTuKhoaLienQuan >= 3)
            {
                diemTuKhoa += 20;
                ghiChu.Add($"✓ Có {soTuKhoaLienQuan} từ khóa liên quan");
            }
            else
            {
                ghiChu.Add("⚠ Nên thêm từ khóa liên quan (tối ưu, marketing, tìm kiếm...)");
            }

            diem.Diem = Math.Min(diemTuKhoa, 100);
            diem.ThanhCong = diem.Diem >= 70;
            diem.MauSac = GetMauSac(diem.Diem);
            diem.TrangThai = GetTrangThai(diem.Diem);
            diem.GhiChu = ghiChu;

            return diem;
        }

        private SEOChiTietDiem AnalyzeCauTruc(string noiDung)
        {
            var diem = new SEOChiTietDiem();
            var ghiChu = new List<string>();
            var diemCauTruc = 0;

            // Kiểm tra thẻ heading
            var h1Count = Regex.Matches(noiDung, @"<h1[^>]*>", RegexOptions.IgnoreCase).Count;
            var h2Count = Regex.Matches(noiDung, @"<h2[^>]*>", RegexOptions.IgnoreCase).Count;
            var h3Count = Regex.Matches(noiDung, @"<h3[^>]*>", RegexOptions.IgnoreCase).Count;

            if (h1Count == 1)
            {
                diemCauTruc += 25;
                ghiChu.Add("✓ Có đúng 1 thẻ H1");
            }
            else if (h1Count == 0)
            {
                ghiChu.Add("⚠ Thiếu thẻ H1");
            }
            else
            {
                ghiChu.Add("⚠ Có quá nhiều thẻ H1");
            }

            if (h2Count >= 2)
            {
                diemCauTruc += 25;
                ghiChu.Add($"✓ Có {h2Count} thẻ H2");
            }
            else if (h2Count == 1)
            {
                diemCauTruc += 15;
                ghiChu.Add("⚠ Nên có ít nhất 2 thẻ H2");
            }
            else
            {
                ghiChu.Add("⚠ Thiếu thẻ H2");
            }

            if (h3Count >= 1)
            {
                diemCauTruc += 15;
                ghiChu.Add($"✓ Có {h3Count} thẻ H3");
            }

            // Kiểm tra đoạn văn
            var pCount = Regex.Matches(noiDung, @"<p[^>]*>", RegexOptions.IgnoreCase).Count;
            if (pCount >= 3)
            {
                diemCauTruc += 20;
                ghiChu.Add($"✓ Có {pCount} đoạn văn");
            }
            else
            {
                ghiChu.Add("⚠ Nên chia thành nhiều đoạn văn");
            }

            // Kiểm tra danh sách
            var listCount = Regex.Matches(noiDung, @"<(ul|ol)[^>]*>", RegexOptions.IgnoreCase).Count;
            if (listCount > 0)
            {
                diemCauTruc += 15;
                ghiChu.Add("✓ Có sử dụng danh sách");
            }

            diem.Diem = Math.Min(diemCauTruc, 100);
            diem.ThanhCong = diem.Diem >= 70;
            diem.MauSac = GetMauSac(diem.Diem);
            diem.TrangThai = GetTrangThai(diem.Diem);
            diem.GhiChu = ghiChu;

            return diem;
        }

        private SEOChiTietDiem AnalyzeHinhAnh(string? anhTieuDeAlt)
        {
            var diem = new SEOChiTietDiem();
            var ghiChu = new List<string>();
            var diemHinhAnh = 0;

            if (string.IsNullOrWhiteSpace(anhTieuDeAlt))
            {
                diem.Diem = 0;
                diem.ThanhCong = false;
                ghiChu.Add("⚠ Thiếu mô tả ảnh (alt text)");
            }
            else
            {
                diemHinhAnh += 50;
                ghiChu.Add("✓ Có mô tả ảnh");

                if (anhTieuDeAlt.Length >= 10 && anhTieuDeAlt.Length <= 125)
                {
                    diemHinhAnh += 30;
                    ghiChu.Add("✓ Độ dài mô tả ảnh tối ưu");
                }
                else
                {
                    ghiChu.Add("⚠ Mô tả ảnh nên có 10-125 ký tự");
                }

                if (anhTieuDeAlt.Contains("SEO", StringComparison.OrdinalIgnoreCase) || 
                    anhTieuDeAlt.Contains("tối ưu", StringComparison.OrdinalIgnoreCase))
                {
                    diemHinhAnh += 20;
                    ghiChu.Add("✓ Mô tả ảnh chứa từ khóa");
                }
            }

            diem.Diem = Math.Min(diemHinhAnh, 100);
            diem.ThanhCong = diem.Diem >= 70;
            diem.MauSac = GetMauSac(diem.Diem);
            diem.TrangThai = GetTrangThai(diem.Diem);
            diem.GhiChu = ghiChu;

            return diem;
        }

        private SEOChiTietDiem AnalyzeLienKet(string noiDung)
        {
            var diem = new SEOChiTietDiem();
            var ghiChu = new List<string>();
            var diemLienKet = 0;

            // Kiểm tra liên kết nội bộ
            var internalLinks = Regex.Matches(noiDung, @"<a[^>]*href=[""'](?!https?://)[^""']*[""'][^>]*>", RegexOptions.IgnoreCase).Count;
            if (internalLinks >= 2)
            {
                diemLienKet += 40;
                ghiChu.Add($"✓ Có {internalLinks} liên kết nội bộ");
            }
            else if (internalLinks == 1)
            {
                diemLienKet += 20;
                ghiChu.Add("⚠ Nên có ít nhất 2 liên kết nội bộ");
            }
            else
            {
                ghiChu.Add("⚠ Thiếu liên kết nội bộ");
            }

            // Kiểm tra liên kết ngoài
            var externalLinks = Regex.Matches(noiDung, @"<a[^>]*href=[""']https?://[^""']*[""'][^>]*>", RegexOptions.IgnoreCase).Count;
            if (externalLinks >= 1)
            {
                diemLienKet += 30;
                ghiChu.Add($"✓ Có {externalLinks} liên kết ngoài");
            }
            else
            {
                ghiChu.Add("⚠ Nên có ít nhất 1 liên kết ngoài");
            }

            // Kiểm tra title attribute của liên kết
            var linksWithTitle = Regex.Matches(noiDung, @"<a[^>]*title=[""'][^""']*[""'][^>]*>", RegexOptions.IgnoreCase).Count;
            if (linksWithTitle > 0)
            {
                diemLienKet += 20;
                ghiChu.Add($"✓ Có {linksWithTitle} liên kết có title");
            }

            // Kiểm tra liên kết có từ khóa
            var keywordLinks = Regex.Matches(noiDung, @"<a[^>]*>.*?(SEO|tối ưu|marketing).*?</a>", RegexOptions.IgnoreCase).Count;
            if (keywordLinks > 0)
            {
                diemLienKet += 10;
                ghiChu.Add($"✓ Có {keywordLinks} liên kết chứa từ khóa");
            }

            diem.Diem = Math.Min(diemLienKet, 100);
            diem.ThanhCong = diem.Diem >= 70;
            diem.MauSac = GetMauSac(diem.Diem);
            diem.TrangThai = GetTrangThai(diem.Diem);
            diem.GhiChu = ghiChu;

            return diem;
        }

        private SEOChiTietDiem AnalyzeDoDai(string noiDung)
        {
            var diem = new SEOChiTietDiem();
            var ghiChu = new List<string>();
            var diemDoDai = 0;

            var noiDungKhongHTML = Regex.Replace(noiDung, @"<[^>]+>", " ");
            var soTu = noiDungKhongHTML.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            if (soTu >= 300)
            {
                diemDoDai += 50;
                ghiChu.Add($"✓ Nội dung đủ dài ({soTu} từ)");
            }
            else if (soTu >= 150)
            {
                diemDoDai += 30;
                ghiChu.Add($"⚠ Nội dung hơi ngắn ({soTu} từ), nên có ít nhất 300 từ");
            }
            else
            {
                diemDoDai += 10;
                ghiChu.Add($"⚠ Nội dung quá ngắn ({soTu} từ), nên có ít nhất 300 từ");
            }

            // Kiểm tra độ dài câu
            var cau = noiDungKhongHTML.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var cauDai = cau.Count(c => c.Trim().Split(' ').Length > 20);
            var cauNgan = cau.Count(c => c.Trim().Split(' ').Length <= 10);

            if (cauDai <= cau.Length * 0.3)
            {
                diemDoDai += 25;
                ghiChu.Add("✓ Tỷ lệ câu dài hợp lý");
            }
            else
            {
                ghiChu.Add("⚠ Có quá nhiều câu dài, nên chia nhỏ");
            }

            if (cauNgan >= cau.Length * 0.2)
            {
                diemDoDai += 25;
                ghiChu.Add("✓ Có đủ câu ngắn để dễ đọc");
            }
            else
            {
                ghiChu.Add("⚠ Nên có thêm câu ngắn để dễ đọc");
            }

            diem.Diem = Math.Min(diemDoDai, 100);
            diem.ThanhCong = diem.Diem >= 70;
            diem.MauSac = GetMauSac(diem.Diem);
            diem.TrangThai = GetTrangThai(diem.Diem);
            diem.GhiChu = ghiChu;

            return diem;
        }

        private int CalculateTongDiem(SEOAnalysisVM analysis)
        {
            var tongDiem = (analysis.TieuDeChiTiet.Diem + 
                           analysis.TomTatChiTiet.Diem + 
                           analysis.NoiDungChiTiet.Diem + 
                           analysis.TuKhoaChiTiet.Diem + 
                           analysis.CauTrucChiTiet.Diem + 
                           analysis.HinhAnhChiTiet.Diem + 
                           analysis.LienKetChiTiet.Diem + 
                           analysis.DoDaiChiTiet.Diem) / 8;

            return Math.Min(tongDiem, 100);
        }

        private string GetMauSac(int diem)
        {
            return diem switch
            {
                >= 80 => "green",
                >= 60 => "yellow",
                >= 40 => "orange",
                _ => "red"
            };
        }

        private string GetTrangThai(int diem)
        {
            return diem switch
            {
                >= 90 => "Xuất sắc",
                >= 80 => "Tốt",
                >= 60 => "Trung bình",
                >= 40 => "Kém",
                _ => "Rất kém"
            };
        }

        private List<string> GenerateGoiY(SEOAnalysisVM analysis)
        {
            var goiY = new List<string>();

            if (analysis.TieuDeChiTiet.Diem < 70)
                goiY.Add("Cải thiện tiêu đề: đảm bảo độ dài 30-60 ký tự và chứa từ khóa chính");

            if (analysis.TomTatChiTiet.Diem < 70)
                goiY.Add("Cải thiện tóm tắt: đảm bảo độ dài 120-160 ký tự và chứa từ khóa");

            if (analysis.NoiDungChiTiet.Diem < 70)
                goiY.Add("Cải thiện nội dung: thêm thẻ heading, chia đoạn văn, sử dụng danh sách");

            if (analysis.TuKhoaChiTiet.Diem < 70)
                goiY.Add("Cải thiện từ khóa: tăng mật độ từ khóa lên 1-3% và thêm từ khóa liên quan");

            if (analysis.CauTrucChiTiet.Diem < 70)
                goiY.Add("Cải thiện cấu trúc: sử dụng đúng thẻ H1, H2, H3 và chia đoạn văn");

            if (analysis.HinhAnhChiTiet.Diem < 70)
                goiY.Add("Cải thiện hình ảnh: thêm alt text mô tả ảnh");

            if (analysis.LienKetChiTiet.Diem < 70)
                goiY.Add("Cải thiện liên kết: thêm liên kết nội bộ và ngoài");

            if (analysis.DoDaiChiTiet.Diem < 70)
                goiY.Add("Cải thiện độ dài: viết ít nhất 300 từ với câu văn đa dạng");

            return goiY;
        }

        public async Task<SEOAnalysis?> GetLatestAnalysisAsync(int baiVietId)
        {
            return await _db.SEOAnalyses
                .Where(x => x.BaiVietId == baiVietId)
                .OrderByDescending(x => x.NgayPhanTich)
                .FirstOrDefaultAsync();
        }

        public async Task SaveAnalysisAsync(SEOAnalysis analysis)
        {
            var existing = await _db.SEOAnalyses
                .FirstOrDefaultAsync(x => x.BaiVietId == analysis.BaiVietId);

            if (existing != null)
            {
                existing.TongDiem = analysis.TongDiem;
                existing.DiemTieuDe = analysis.DiemTieuDe;
                existing.DiemTomTat = analysis.DiemTomTat;
                existing.DiemNoiDung = analysis.DiemNoiDung;
                existing.DiemTuKhoa = analysis.DiemTuKhoa;
                existing.DiemCauTruc = analysis.DiemCauTruc;
                existing.DiemHinhAnh = analysis.DiemHinhAnh;
                existing.DiemLienKet = analysis.DiemLienKet;
                existing.DiemDoDai = analysis.DiemDoDai;
                existing.TieuDePhanTich = analysis.TieuDePhanTich;
                existing.TomTatPhanTich = analysis.TomTatPhanTich;
                existing.NoiDungPhanTich = analysis.NoiDungPhanTich;
                existing.TuKhoaPhanTich = analysis.TuKhoaPhanTich;
                existing.CauTrucPhanTich = analysis.CauTrucPhanTich;
                existing.HinhAnhPhanTich = analysis.HinhAnhPhanTich;
                existing.LienKetPhanTich = analysis.LienKetPhanTich;
                existing.DoDaiPhanTich = analysis.DoDaiPhanTich;
                existing.GoiYCaiThien = analysis.GoiYCaiThien;
                existing.NgayPhanTich = DateTime.UtcNow;
                existing.DaXuLy = analysis.DaXuLy;
            }
            else
            {
                _db.SEOAnalyses.Add(analysis);
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<string>> GetGoiYCaiThienAsync(int baiVietId)
        {
            var analysis = await GetLatestAnalysisAsync(baiVietId);
            if (analysis == null) return new List<string>();

            var goiY = new List<string>();
            
            if (analysis.DiemTieuDe < 70)
                goiY.Add("Cải thiện tiêu đề");
            if (analysis.DiemTomTat < 70)
                goiY.Add("Cải thiện tóm tắt");
            if (analysis.DiemNoiDung < 70)
                goiY.Add("Cải thiện nội dung");
            if (analysis.DiemTuKhoa < 70)
                goiY.Add("Cải thiện từ khóa");
            if (analysis.DiemCauTruc < 70)
                goiY.Add("Cải thiện cấu trúc");
            if (analysis.DiemHinhAnh < 70)
                goiY.Add("Cải thiện hình ảnh");
            if (analysis.DiemLienKet < 70)
                goiY.Add("Cải thiện liên kết");
            if (analysis.DiemDoDai < 70)
                goiY.Add("Cải thiện độ dài");

            return goiY;
        }
    }
}
