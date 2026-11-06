using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Profiles;

namespace JobPortal.Application.Interfaces.Service;

public interface ICandidateProfileService
{
    Task<CandidateProfileDto> GetMyAsync(Guid userId, CancellationToken ct);
    Task<CandidateProfileDto> UpdateAsync(Guid userId, UpdateCandidateProfileDto dto, CancellationToken ct);

    Task<CandidateProfileDto> UploadCvAsync(Guid userId, FileUploadDto file, CancellationToken ct);
    Task DeleteCvAsync(Guid userId, CancellationToken ct);
    Task<FileDownloadDto> DownloadMyCvAsync(Guid userId, CancellationToken ct);
}