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

        var order = await _context.Orders
            .FirstOrDefaultAsync(x =>
                x.OrderNumber == request.OrderNumber &&
                x.UserId == userId);

        if (order == null || order.Status != "Delivered")
        {
            return null;
        }

        var existingRequest = await _context.ReturnRequests
            .AnyAsync(x =>
                x.OrderId == order.Id &&
                x.UserId == userId &&
                (x.Status == "Pending" ||
                 x.Status == "Approved"));

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
            Id = returnRequest.Id,
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
            Id = x.Id,
            OrderNumber = x.Order?.OrderNumber ?? string.Empty,
            Reason = x.Reason,
            Status = x.Status,
            AdminNote = x.AdminNote,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    // Admin bütün müştərilərin qaytarılma müraciətlərini görür.
    public async Task<List<ReturnRequestDto>> GetAllRequestsAsync()
    {
        var requests = await _context.ReturnRequests
            .AsNoTracking()
            .Include(x => x.Order)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return requests.Select(x => new ReturnRequestDto
        {
            Id = x.Id,
            OrderNumber = x.Order?.OrderNumber ?? string.Empty,
            Reason = x.Reason,
            Status = x.Status,
            AdminNote = x.AdminNote,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    // Admin müraciəti təsdiqləyir və ya rədd edir.
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
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == returnRequestId);

        if (request == null ||
            request.Status != "Pending" ||
            request.Order == null)
        {
            return false;
        }

        // Təsdiqlənən qaytarılma zamanı məhsulları stoka qaytarırıq.
        if (status == "Approved")
        {
            var orderItems = await _context.OrderItems
                .Where(x => x.OrderId == request.OrderId)
                .ToListAsync();

            if (orderItems.Count == 0)
            {
                return false;
            }

            foreach (var item in orderItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(x => x.Id == item.ProductId);

                if (product != null)
                {
                    product.Stock += item.Quantity;
                }
            }
        }

        request.Status = status;
        request.AdminNote = adminNote?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync();

        return true;
    }
}
