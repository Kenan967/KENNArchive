using KENNArchive.Application.DTOs.Return;
using KENNArchive.Application.Interfaces;
using KENNArchive.Domain.Entities;
using KENNArchive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KENNArchive.Infrastructure.Services;

// Məhsulun qaytarılması ilə bağlı əməliyyatları yerinə yetirir.
public class ReturnService : IReturnService
{
    private readonly KennArchiveDbContext _context;

    public ReturnService(KennArchiveDbContext context)
    {
        _context = context;
    }

    // Müştəri sifarişi üçün qaytarılma müraciəti yaradır.
    public async Task<ReturnRequestDto?> CreateAsync(
        int userId,
        CreateReturnRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OrderNumber) ||
            string.IsNullOrWhiteSpace(request.Reason))
        {
            return null;
        }

        // Sifarişin həmin müştəriyə aid olduğunu yoxlayırıq.
        var order = await _context.Orders
            .FirstOrDefaultAsync(x =>
                x.OrderNumber == request.OrderNumber &&
                x.UserId == userId);

        if (order == null)
        {
            return null;
        }

        // Yalnız çatdırılmış sifariş üçün qaytarılma müraciəti qəbul edirik.
        if (order.Status != "Delivered")
        {
            return null;
        }

        // Eyni sifariş üçün gözləyən və ya təsdiqlənmiş müraciət varsa,
        // yeni müraciət yaratmırıq.
        var existingRequest = await _context.ReturnRequests
            .AnyAsync(x =>
                x.OrderId == order.Id &&
                x.UserId == userId &&
                (x.Status == "Pending" || x.Status == "Approved"));

        if (existingRequest)
        {
            return null;
        }

        var returnRequest = new ReturnRequest
        {
            OrderId = order.Id,
            UserId = userId,
            Reason = request.Reason.Trim(),
            Status = "Pending",
            AdminNote = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _context.ReturnRequests.Add(returnRequest);

        await _context.SaveChangesAsync();

        return new ReturnRequestDto
        {
            OrderNumber = order.OrderNumber,
            Reason = returnRequest.Reason,
            Status = returnRequest.Status,
            AdminNote = returnRequest.AdminNote,
            CreatedAt = returnRequest.CreatedAt
        };
    }

    // Müştərinin öz qaytarılma müraciətlərini göstərir.
    public async Task<List<ReturnRequestDto>> GetMyRequestsAsync(
        int userId)
    {
        var requests = await _context.ReturnRequests
            .AsNoTracking()
            .Include(x => x.Order)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return requests.Select(x => new ReturnRequestDto
        {
            OrderNumber = x.Order?.OrderNumber ?? string.Empty,
            Reason = x.Reason,
            Status = x.Status,
            AdminNote = x.AdminNote,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    // Admin qaytarılma müraciətini təsdiqləyir və ya rədd edir.
    public async Task<bool> UpdateStatusAsync(
        int returnRequestId,
        string status,
        string adminNote)
    {
        if (status != "Approved" && status != "Rejected")
        {
            return false;
        }

        var request = await _context.ReturnRequests
            .FirstOrDefaultAsync(x => x.Id == returnRequestId);

        if (request == null || request.Status != "Pending")
        {
            return false;
        }

        request.Status = status;
        request.AdminNote = adminNote?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync();

        return true;
    }
}