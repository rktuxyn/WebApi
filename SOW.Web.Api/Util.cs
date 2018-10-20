/**
* Copyright (c) 2018, SOW (https://www.facebook.com/safeonlineworld). (https://github.com/RKTUXYN) All rights reserved.
* @author {SOW}
* Copyrights licensed under the New BSD License.
* See the accompanying LICENSE file for terms.
*/
using Microsoft.Owin;
using SOW.Web.Api.Core.Extensions;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
namespace SOW.Web.Api.Core {
    class Util {
        private static readonly TaskQueue mSendQueue = new TaskQueue( );
        /// <summary>
        /// Is request support gzip
        /// </summary>
        /// <param name="request"></param>
        /// <returns>bool</returns>
        private static bool CanGZip( IOwinRequest request ) {
            string acceptEncoding = request.Headers["Accept-Encoding"];
            if ( !string.IsNullOrEmpty( acceptEncoding ) &&
            ( acceptEncoding.Contains( "gzip" ) || acceptEncoding.Contains( "deflate" ) ) )
                return true;
            return false;
        }
        /// <summary>
        /// Write System.String data to gzip stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destinationStream"></param>
        /// <param name="canGZip"></param>
        public static void WriteGzipStream( string data, Stream destinationStream, bool canGZip = true ) {
            UTF8Encoding encoding = new UTF8Encoding( false );
            using ( MemoryStream memoryStream = new MemoryStream( data.Length ) ) {
                using ( Stream writer = canGZip ?
                    ( new GZipStream( memoryStream, CompressionMode.Compress ) ) as Stream : memoryStream
                ) {
                    byte[] dataByte = Encoding.UTF8.GetBytes( data );
                    writer.Write( dataByte, 0, dataByte.Length ); dataByte = null; data = null;
                }
                memoryStream.Seek( 0, SeekOrigin.Begin );
                byte[] buffer = new byte[1024 * 10];//10KB
                int read = 0;
                while ( ( read = memoryStream.Read( buffer, 0, buffer.Length ) ) > 0 ) {
                    destinationStream.Write( buffer, 0, read );
                }
            }
            return;
        }
        /// <summary>
        /// Get gzip byte[] from System.String
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>byte[]</returns>
        public static byte[] GetZipByte( string Data ) {
            byte[] responseBytes; UTF8Encoding encoding = new UTF8Encoding( false );
            using ( MemoryStream memoryStream = new MemoryStream( 5000 ) ) {
                using ( Stream writer = true ?
                    ( new GZipStream( memoryStream, CompressionMode.Compress ) ) as Stream : memoryStream
                ) {
                    byte[] dataByte;
                    dataByte = encoding.GetBytes( Data );
                    writer.Write( dataByte, 0, dataByte.Length ); dataByte = null; Data = "";
                }
                responseBytes = memoryStream.ToArray( );
            }
            return responseBytes;
        }
        /// <summary>
        /// Write System.String to Request Context
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        /// <param name="intercept"></param>
        /// <returns>Task</returns>
        public static Task WriteAsync( string data, IOwinContext context, bool intercept = false ) {
            return mSendQueue.Enqueue(
               s => {
                   try {
                       if (!CanGZip( context.Request )) {
                           return context.Response.WriteAsync( data );
                       }
                       context.Response.Headers.Add( "Content-Encoding", new string[] { "gzip" } );
                       if (!intercept) {
                           context.Response.Headers.Add( "X-Powered-By", new string[] { "https://www.safeonlineworld.com" } );
                       }
                       byte[] responseBytes = GetZipByte( data ); data = null;
                       context.Response.Write( responseBytes, 0, responseBytes.Length );
                       responseBytes = null;
                       context.Response.Body.Flush( );
                   } catch (Exception) {
                       
                   }
                   return Task.FromResult( 0 );
               }, context );
        }
    }
}
