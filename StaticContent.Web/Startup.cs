using Microsoft.AspNetCore.Builder;

namespace StaticContent.Web
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
        }
    }
}