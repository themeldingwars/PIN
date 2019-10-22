using Microsoft.AspNetCore.Builder;

namespace StaticContent
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
        }
    }
}