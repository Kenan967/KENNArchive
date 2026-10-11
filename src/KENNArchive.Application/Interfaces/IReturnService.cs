using System.Collections.Generic;
using System.Threading.Tasks;
using KENNArchive.Application.DTOs.Return;

namespace KENNArchive.Application.Interfaces;

// Məhsulun qaytarılması ilə bağlı əməliyyatlar.
public interface IReturnService
{
    // Müştəri qaytarılma müraciəti yaradır.
    Task<ReturnRequestDto?> CreateAsync(
        int userId,
        CreateReturnRequest request);

    // Müştərinin öz müraciətlərini qaytarır.
    Task<List<ReturnRequestDto>> GetMyRequestsAsync(
        int userId);

    // Admin bütün müştərilərin müraciətlərini görür.
    Task<List<ReturnRequestDto>> GetAllRequestsAsync();

    // Admin müraciəti təsdiqləyir və ya rədd edir.
    Task<bool> UpdateStatusAsync(
        int returnRequestId,
        string status,
        string adminNote);
}
