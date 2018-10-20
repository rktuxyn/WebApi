/**
* Copyright (c) 2018, SOW (https://www.facebook.com/safeonlineworld). (https://github.com/RKTUXYN) All rights reserved.
* @author {SOW}
* Copyrights licensed under the New BSD License.
* See the accompanying LICENSE file for terms.
*/
//Thanks to ==> https://github.com/bryceg/Owin.WebSocket
namespace SOW.Web.Api.Core.Extensions {
    using System.Collections.Generic;
    using Owin;
    public static class OwinExtension {
        /// <summary>
        /// Maps a static URI to a web socket consumer
        /// </summary>
        /// <typeparam name="T">Type of WebApiHubConnection</typeparam>
        /// <param name="app">Owin App</param>
        /// <param name="route">Static URI to map to the hub</param>
        /// <param name="serviceLocator">Service locator to use for getting instances of T</param>
        public static void MapWebApiRoute<T>( this IAppBuilder app, string route, object serviceLocator = null )
            where T : ApiController {
            app.Map( route, config => config.Use<Middleware<T>>( serviceLocator ) );
        }
        internal static T Get<T>( this IDictionary<string, object> dictionary, string key ) {
            object item;
            if ( dictionary.TryGetValue( key, out item ) ) {
                return ( T )item;
            }
            return default( T );
        }
    }
}
