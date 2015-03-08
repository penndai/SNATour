using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SNAtours.Startup))]
namespace SNAtours
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
