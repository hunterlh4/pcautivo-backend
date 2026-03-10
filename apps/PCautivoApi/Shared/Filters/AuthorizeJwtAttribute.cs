using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace PCautivoApi.Shared.Filters;

/// <summary>
/// Atributo de autorización que utiliza el esquema de autenticación JWT.
/// </summary>
public class AuthorizeJwtAttribute : AuthorizeAttribute
{
    public AuthorizeJwtAttribute() : base(JwtBearerDefaults.AuthenticationScheme)
    {
    }
}
