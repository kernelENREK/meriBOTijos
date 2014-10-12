using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace meriBOTijos.ZonaForo_Helpers.Twitter
{
    /// <summary>
    /// Credenciales de la aplicación dada de alta en Twitter Developers
    /// </summary>
    public class Twitter_App_Credentials
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string OAuthToken { get; set; }
        public string AccessToken { get; set; }
    }
}