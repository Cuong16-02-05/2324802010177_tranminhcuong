using ASC.Business;
using ASC.Model;
using ASC.Web.Areas.Configuration.Models;
using ASC.Web.Controllers;
using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Web.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = Constants.Roles.Admin)]
    public class MasterDataController : BaseController
    {
        private readonly IMasterDataOperations _masterDataOps;
        private readonly IMapper _mapper;

        public MasterDataController(IMasterDataOperations masterDataOps, IMapper mapper)
        {
            _masterDataOps = masterDataOps;
            _mapper = mapper;
        }

        // ── Lab 6: MASTER KEYS ────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            var keys = await _masterDataOps.GetAllMasterKeysAsync();
            var keyVMs = _mapper.Map<List<MasterDataKeyViewModel>>(
                keys.OrderBy(k => k.Name).ToList());
            return View(new MasterKeysViewModel
            {
                MasterDataKeys = keyVMs,
                MasterDataKey = new MasterDataKeyViewModel { IsActive = true }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.MasterDataKey.Name))
            {
                TempData["Error"] = "Tên Key không được để trống.";
                return RedirectToAction(nameof(MasterKeys));
            }

            if (string.IsNullOrEmpty(model.MasterDataKey.UniqueId))
                await _masterDataOps.CreateMasterKeyAsync(
                    model.MasterDataKey.Name.Trim(), User.Identity?.Name ?? "system");
            else
                await _masterDataOps.UpdateMasterKeyAsync(
                    model.MasterDataKey.UniqueId, model.MasterDataKey.Name.Trim(),
                    model.MasterDataKey.IsActive, User.Identity?.Name ?? "system");

            TempData["Success"] = $"Đã lưu Key: {model.MasterDataKey.Name}";
            return RedirectToAction(nameof(MasterKeys));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleMasterKey(string id, string name, bool isActive)
        {
            await _masterDataOps.UpdateMasterKeyAsync(
                id, name, !isActive, User.Identity?.Name ?? "system");
            TempData["Success"] = $"Đã cập nhật trạng thái Key: {name}";
            return RedirectToAction(nameof(MasterKeys));
        }

        // ── Lab 6: MASTER VALUES ──────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> MasterValues(string keyId, string keyName)
        {
            var values = await _masterDataOps.GetMasterValuesByKeyAsync(keyId);
            var valueVMs = _mapper.Map<List<MasterDataValueViewModel>>(
                values.OrderBy(v => v.Name).ToList());
            return View(new MasterValuesViewModel
            {
                MasterDataValues = valueVMs,
                MasterDataValue = new MasterDataValueViewModel
                    { MasterDataKeyId = keyId, IsActive = true },
                MasterDataKeyId = keyId,
                MasterDataKeyName = keyName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(MasterValuesViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.MasterDataValue.Name))
            {
                TempData["Error"] = "Tên Value không được để trống.";
                return RedirectToAction(nameof(MasterValues),
                    new { keyId = model.MasterDataKeyId, keyName = model.MasterDataKeyName });
            }

            if (string.IsNullOrEmpty(model.MasterDataValue.UniqueId))
                await _masterDataOps.CreateMasterValueAsync(
                    model.MasterDataKeyId!, model.MasterDataValue.Name.Trim(),
                    User.Identity?.Name ?? "system", model.MasterDataValue.Price);
            else
                await _masterDataOps.UpdateMasterValueAsync(
                    model.MasterDataValue.UniqueId, model.MasterDataValue.Name.Trim(),
                    model.MasterDataValue.IsActive, User.Identity?.Name ?? "system",
                    model.MasterDataValue.Price);

            TempData["Success"] = $"Đã lưu Value: {model.MasterDataValue.Name}";
            return RedirectToAction(nameof(MasterValues),
                new { keyId = model.MasterDataKeyId, keyName = model.MasterDataKeyName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleMasterValue(
            string id, string name, bool isActive, string keyId, string keyName)
        {
            // Đọc giá hiện tại từ DB để không bị ghi đè null
            var existing = await _masterDataOps.GetMasterValuesByKeyAsync(keyId);
            var currentPrice = existing.FirstOrDefault(v => v.UniqueId == id)?.Price;

            await _masterDataOps.UpdateMasterValueAsync(
                id, name, !isActive, User.Identity?.Name ?? "system", currentPrice);
            TempData["Success"] = $"Đã cập nhật trạng thái Value: {name}";
            return RedirectToAction(nameof(MasterValues), new { keyId, keyName });
        }

        // ── Lab 6: UPLOAD EXCEL ───────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcel(
            IFormFile excelFile, string keyId, string keyName)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel (.xlsx)";
                return RedirectToAction(nameof(MasterValues), new { keyId, keyName });
            }

            if (!excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Chỉ hỗ trợ file .xlsx";
                return RedirectToAction(nameof(MasterValues), new { keyId, keyName });
            }

            try
            {
                int added = 0, skipped = 0;
                using var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed()?.RowsUsed().Skip(1); // bỏ header

                if (rows != null)
                {
                    var existingValues = (await _masterDataOps.GetMasterValuesByKeyAsync(keyId))
                        .Select(v => v.Name?.ToLower())
                        .ToHashSet();

                    foreach (var row in rows)
                    {
                        var valueName = row.Cell(1).GetString().Trim();
                        if (string.IsNullOrEmpty(valueName)) { skipped++; continue; }

                        if (existingValues.Contains(valueName.ToLower()))
                        {
                            skipped++;
                            continue;
                        }

                        // Cột C (index 3) = Price, tùy chọn
                        decimal? price = null;
                        var priceCell = row.Cell(3).GetString().Trim();
                        if (!string.IsNullOrEmpty(priceCell) && decimal.TryParse(priceCell, out var parsedPrice))
                            price = parsedPrice;

                        await _masterDataOps.CreateMasterValueAsync(
                            keyId, valueName, User.Identity?.Name ?? "system", price);
                        existingValues.Add(valueName.ToLower());
                        added++;
                    }
                }

                TempData["Success"] = $"Upload thành công: {added} values thêm mới, {skipped} bỏ qua.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi đọc file Excel: {ex.Message}";
            }

            return RedirectToAction(nameof(MasterValues), new { keyId, keyName });
        }

        // ── Lab 6: DOWNLOAD TEMPLATE ──────────────────────────────────────

        [HttpGet]
        public IActionResult DownloadTemplate(string keyName)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("MasterValues");

            // Header
            ws.Cell(1, 1).Value = "ValueName";
            ws.Cell(1, 2).Value = "IsActive";
            ws.Cell(1, 3).Value = "Price (VND)";
            ws.Row(1).Style.Font.Bold = true;
            ws.Row(1).Style.Fill.BackgroundColor = XLColor.Brown;
            ws.Row(1).Style.Font.FontColor = XLColor.White;

            // Sample rows
            ws.Cell(2, 1).Value = "Engine Repair";
            ws.Cell(2, 2).Value = "TRUE";
            ws.Cell(2, 3).Value = 500000;
            ws.Cell(3, 1).Value = "AC Service";
            ws.Cell(3, 2).Value = "TRUE";
            ws.Cell(3, 3).Value = 350000;
            ws.Cell(4, 1).Value = "Oil Change";
            ws.Cell(4, 2).Value = "TRUE";
            ws.Cell(4, 3).Value = 150000;
            ws.Cell(5, 1).Value = "Tire Replacement";
            ws.Cell(5, 2).Value = "TRUE";
            ws.Cell(5, 3).Value = 200000;

            ws.Columns().AdjustToContents();

            using var mem = new MemoryStream();
            workbook.SaveAs(mem);
            return File(mem.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Template_{keyName ?? "MasterValues"}.xlsx");
        }
    }
}
