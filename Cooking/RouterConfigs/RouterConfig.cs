using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Cooking.RouterConfigs
{
    public static class RouterConfig
    {
        // Phương thức cấu hình các endpoint của ứng dụng
        public static void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}
