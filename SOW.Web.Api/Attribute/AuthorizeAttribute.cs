/**
* Copyright (c) 2018, SOW (https://www.facebook.com/safeonlineworld). (https://github.com/RKTUXYN) All rights reserved.
* @author {SOW}
* Copyrights licensed under the New BSD License.
* See the accompanying LICENSE file for terms.
*/
namespace SOW.Web.Api.Core {
    using Microsoft.Owin;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true )]
    public class AuthorizeAttribute : Attribute, IAuthorizeAttribute {
        public AuthorizeAttribute( ) { }
        public bool RequireOutgoing { get; set; }
        public string Roles { get; set; }
        public string Users { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual bool IsAuthorized( IOwinRequest request ) {
            if ( request.User == null ) return false;
            return request.User.Identity.IsAuthenticated;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsDefaultAttribute( ) {
            if ( string.IsNullOrEmpty( Users ) && string.IsNullOrEmpty( Roles ) )
                return true;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual bool IsInRole( IOwinRequest request ) {
            if ( !IsAuthorized( request ) ) return false;
            if ( string.IsNullOrEmpty( Roles ) ) return true;
            var arr = Roles.Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
            int len = arr.Length;
            foreach ( var r in arr ) {
                if ( r == null ) return false;
                var role = r?.Trim( );
                if ( request.User.IsInRole( role ) ) {
                    return true;
                }
            }
            return len > 0 ? false : true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual bool IsInUsers( IOwinRequest request ) {
            if ( !IsAuthorized( request ) ) return false;
            if ( string.IsNullOrEmpty( Users ) ) return true;
            var userName = request.User.Identity.Name;
            var arr = Users.Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
            var resp = arr.FirstOrDefault( a => a?.Trim( ) == userName );
            return string.IsNullOrEmpty( resp ) ? false : true;
        }
    }
    public static class CustomAttribute {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="inherit"></param>
        /// <returns>IEnumerable<TAttribute></returns>
        public static IEnumerable<T> GetAttributes<T>( this ICustomAttributeProvider provider, bool inherit = false )
            where T : Attribute {
            return provider
                .GetCustomAttributes( typeof( T ), inherit )
                .Cast<T>( );
        }
    }
    public interface IAuthorizeAttribute {
        bool RequireOutgoing { get; set; }
        string Users { get; set; }
        string Roles { get; set; }
        bool IsAuthorized( IOwinRequest request );
        bool IsInUsers( IOwinRequest request );
        bool IsInRole( IOwinRequest request );
    }
}
