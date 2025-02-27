using Microsoft.IdentityModel.Tokens;

namespace JwtTokenProject.Middleware
{
    public class CustomJwtMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomJwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // JWT doğrulama işlemi burada yapılır
                await _next(context); // İstek işleme sırasına devam et
            }
            catch (SecurityTokenExpiredException)
            {
                // Token süresi dolmuşsa, kullanıcıya yönlendirme işlemi yapılabilir
                context.Response.StatusCode = 401; // Unauthorized
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"message\":\"Token süresi dolmuş. Lütfen tekrar giriş yapınız.\"}");
            }
            catch (SecurityTokenValidationException)
            {
                // Geçersiz token hatası
                context.Response.StatusCode = 401; // Unauthorized
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"message\":\"Geçersiz token. Lütfen tekrar giriş yapınız.\"}");
            }
        }
    }
}