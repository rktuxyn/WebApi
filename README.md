# WebApi
Web Api C#
<b>1) Map the route for the web api </b>
```c#
using Owin;
using Microsoft.Owin;
using System.Collections.Generic;
[assembly: OwinStartup( typeof( SOW.Web.Api.View.Startup ) )]

namespace SOW.Web.Api.View {
    using SOW.Web.Api.Core.Extensions;
    using SOW.Web.Api.Core;
    public class Startup {
        public void Configuration( IAppBuilder app ) {
            app.MapWebApiRoute<ApiCnotrollers>( "/api" );
        }
    }
}
```
<b>2) Inherit Api Cnotrollers from SOW.Web.Api.Core </b>
```c#
using System.Threading.Tasks;
namespace SOW.Web.Api.View {
    using Microsoft.Owin;
    using SOW.Web.Api.Core;
    public class ApiCnotrollers : SOW.Web.Api.Core.ApiController {
        [Authorize]
        [Route( "/getdata/" )]
        public async Task<Task> GetData( ) {
          base.Response.ContentType = "application/json";
          return base.ResponseWriteAsync( _jss.Serialize( new {
                msg="Success"
            } ) );
         }
     }
 }
```
<p> THANKS to Bryce Godfrey (https://github.com/bryceg/) for Owin.WebSocket</p>
