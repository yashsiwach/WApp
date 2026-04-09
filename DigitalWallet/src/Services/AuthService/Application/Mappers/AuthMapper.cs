using AuthService.Application.DTOs;
using AuthService.Domain.Entities;

namespace AuthService.Application.Mappers;

/// <summary>Single responsibility: maps AuthService domain objects to DTOs.</summary>
public static class AuthMapper
{
    public static UserDto ToDto(User user) => new()
    {
        Id        = user.Id,
        Email     = user.Email,
        Phone     = user.Phone,
        Role      = user.Role,
        IsActive  = user.IsActive,
        CreatedAt = user.CreatedAt
    };

    public static KYCStatusResponse ToDto(KYCDocument doc) => new()
    {
        DocumentId  = doc.Id,
        DocType     = doc.DocType,
        Status      = doc.Status,
        ReviewNotes = doc.ReviewNotes,
        SubmittedAt = doc.SubmittedAt
    };

    public static AuthResponse ToAuthResponse(string accessToken, string refreshToken, DateTime expiresAt, User user) => new()
    {
        AccessToken  = accessToken,
        RefreshToken = refreshToken,
        ExpiresAt    = expiresAt,
        User         = ToDto(user)
    };
}
