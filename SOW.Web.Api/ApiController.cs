/**
* Copyright (c) 2018, SOW (https://www.facebook.com/safeonlineworld). (https://github.com/RKTUXYN) All rights reserved.
* @author {SOW}
* Copyrights licensed under the New BSD License.
* See the accompanying LICENSE file for terms.
*/
using Microsoft.Owin;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Web.Script.Serialization;
using System.Linq;
namespace SOW.Web.Api.Core {
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class ApiController {
        /// <summary>
        /// Owin context for the web Context
        /// </summary>
        public IOwinContext Context { get; private set; }
        public IOwinRequest Request { get { return Context.Request; } }
        public IOwinResponse Response { get { return Context.Response; } }
        public IPrincipal User { get { return Context.Request.User; } }
        public IIdentity Identity { get { return Context.Request.User.Identity; } }
        public bool IsAuthenticated { get; private set; }
        public string UserName { get; private set; }
        public JavaScriptSerializer _jss { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Task<byte[]></returns>
        public async Task<byte[]> ReadBody( ) {
            //8:14 PM 8/15/2018
            using ( var stream = new MemoryStream( ) ) {
                byte[] buffer = new byte[2048]; // read in chunks of 2KB
                int bytesRead;
                while ( ( bytesRead = await Request.Body.ReadAsync( buffer, 0, buffer.Length ) ) > 0 ) {
                    stream.Write( buffer, 0, bytesRead );
                }
                return stream.ToArray( );
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Task<Dictionary<string, string>></returns>
        public async Task<Dictionary<string, string>> GetFormData( ) {
            Dictionary<string, string> dct = new Dictionary<string, string>( StringComparer.CurrentCultureIgnoreCase );
            if ( Request.ContentType == "application/json" ) {
                //Read JSON Data From Request Stream
                string content = Encoding.UTF8.GetString( await this.ReadBody( ) );
                var dict = _jss.Deserialize<Dictionary<string, object>>( content );
                foreach ( var pair in dict ) {
                    string value = ( pair.Value is string ) ? Convert.ToString( pair.Value ) : _jss.Serialize( pair.Value );
                    dct.Add( pair.Key, value );
                }
                return dct;
            }
            if ( Request.ContentType.IndexOf( "multipart/form-data" ) > -1 ) {
                //Read Multipart Form Data From Request Stream
                //8:14 PM 8/15/2018
                string content = Encoding.UTF8.GetString( await this.ReadBody( ) );
                if ( content.IndexOf( "\r\n" ) <= -1 ) {
                    return dct;
                }
                string ct = Context.Request.Headers["Content-Type"];
                if ( ct.IndexOf( "WebKitFormBoundary" ) <= -1 ) {
                    return dct;
                }
                string[] cta = ct.Split( new string[] { "----" }, StringSplitOptions.RemoveEmptyEntries );
                if ( cta == null || cta.Length < 1 ) return dct;
                string boundary = cta[1]; cta = null;
                if ( content.IndexOf( boundary ) <= -1 || boundary.IndexOf( "WebKitFormBoundary" ) <= -1 ) {
                    return dct;
                }
                boundary = "----" + boundary;
                string[] contentArray = content.Split( new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries );
                content = null;
                string prop = string.Empty;
                contentArray.Select( a => {
                    if ( a == boundary ) return a;
                    if ( a.IndexOf( boundary ) > -1 ) return a;
                    if ( a.IndexOf( "Content-Disposition: form-data;" ) > -1 ) {
                        string[] arr = a.Split( new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries );
                        if ( arr == null || arr.Length < 1 ) return a;
                        prop = arr[1];
                        prop = prop.Replace( "\\", "" ).Replace( "\"", "" );
                        return a;
                    }
                    dct.Add( prop, a );
                    return a;
                } ).ToList( );
                contentArray = null;
                return dct;
            }
            //Try to Read Request Payload Data From Request Stream
            IFormCollection formData = await Context.Request.ReadFormAsync( );
            if ( formData == null ) return dct;
            formData.Select( a => {
                if ( a.Value != null ) {
                    dct.Add( a.Key, string.Join( ",", a.Value ) );
                }
                return a;
            } );
            return dct;
        }
        /// <summary>
        /// Write System.String to Request Context
        /// </summary>
        /// <param name="data"></param>
        /// <param name="intercept"></param>
        /// <returns>Task</returns>
        public Task ResponseWriteAsync( string data, bool intercept = false ) {
            return Util.WriteAsync( data, Context, intercept );
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Task</returns>
        public Task InitiateConnection( IOwinContext context ) {
            try {
                string reqPath = context.Request.Path.Value;
                MethodInfo[] methodInfo = this.GetType( ).GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly );
                _jss = new JavaScriptSerializer( );
                if ( methodInfo == null ) {
                    return context.Response.WriteAsync( _jss.Serialize( new {
                        s_code = -404,
                        s_msg = string.Format( "Not Found ==>{0}!!!", reqPath )
                    } ) );
                }
                //Task task = null; bool hasMember = false;
                var method = methodInfo.FirstOrDefault( a => a.GetCustomAttribute<RouteAttribute>().Route == reqPath );
                if ( method == null ) {
                    return context.Response.WriteAsync( _jss.Serialize( new {
                        s_code = -404,
                        s_msg = string.Format( "Not Found ==>{0}!!!", reqPath )
                    } ) );
                }
                this.IsAuthenticated = false;
                Context = context;
                if ( context.Request.User != null ) {
                    if ( context.Request.User.Identity != null ) {
                        this.IsAuthenticated = context.Request.User.Identity.IsAuthenticated;
                        this.UserName = context.Request.User.Identity.Name;
                    }
                }
               
                var authAttribute = method.GetCustomAttribute<AuthorizeAttribute>( );
                if ( authAttribute != null ) {
                    if ( authAttribute.IsDefaultAttribute( ) ) {
                        if ( !this.IsAuthenticated ) {
                            return Response.WriteAsync( _jss.Serialize( new {
                                s_code = -401,
                                s_msg = string.Format( "Authorization required to invoke this path ==>{0}!!!", reqPath )
                            } ) );
                        }
                    } else {
                        if ( !authAttribute.IsInRole( this.Context.Request ) ) {
                            return Response.WriteAsync( _jss.Serialize( new {
                                s_code = -403,
                                s_msg = string.Format( "Forbidden response path ==>{0}!!!", reqPath )
                            } ) );
                        }
                        if ( !authAttribute.IsInUsers( this.Context.Request ) ) {
                            return Response.WriteAsync( _jss.Serialize( new {
                                s_code = -403,
                                s_msg = string.Format( "Forbidden response path ==>{0}!!!", reqPath )
                            } ) );
                        }
                    }
                }
                _jss.MaxJsonLength = Int32.MaxValue;
               
                return ( Task )method.Invoke( this, new object[] { } );
            } catch ( Exception e ) {
                return context.Response.WriteAsync( new JavaScriptSerializer( ).Serialize( new {
                    s_code = 505,
                    s_msg = e.Message
                } ) );
            }
        }

        public Task Ok( ) {
            return Task.FromResult( 0 );
        }
    }
}