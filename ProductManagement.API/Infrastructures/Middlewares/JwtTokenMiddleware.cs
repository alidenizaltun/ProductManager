namespace ProductManagement.API.Infrastructures.Middlewares
{
    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtTokenMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<JwtTokenMiddleware> logger)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            await _next(context);
        }
    }
}
