using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BudgetMaster.Startup))]
namespace BudgetMaster
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
