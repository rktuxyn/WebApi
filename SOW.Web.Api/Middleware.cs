//Thanks to ==> https://github.com/bryceg/Owin.WebSocket
/**
* Copyright (c) 2018, SOW (https://www.facebook.com/safeonlineworld). (https://github.com/RKTUXYN) All rights reserved.
* @author {SOW}
* Copyrights licensed under the New BSD License.
* See the accompanying LICENSE file for terms.
*/
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;
using System;

namespace SOW.Web.Api.Core {
    public class Middleware<T> : OwinMiddleware where T : ApiController {
        private readonly Regex mMatchPattern;
        private readonly object mServiceLocator;

        public Middleware( OwinMiddleware next, object locator )
            : base( next ) {
            mServiceLocator = locator;
        }

        public Middleware( OwinMiddleware next, object locator, Regex matchPattern )
            : this( next, locator ) {
            mMatchPattern = matchPattern;
        }

        public override Task Invoke( IOwinContext context ) {
            return Activator.CreateInstance<T>( ).InitiateConnection( context );
        }
    }
}