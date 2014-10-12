using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LinqToTwitter;
using System.Net;
using meriBOTijos.ZonaForo_Helpers.Acortadores_Links;
using meriBOTijos.ZonaForo_Helpers.Acortadores_Links.Providers;
using meriBOTijos.ZonaForo_Helpers.DataAccessLayer;

namespace meriBOTijos.ZonaForo_Helpers.Twitter.Providers
{
    /// <summary>
    /// Clase para tuitear los post de ZonaForo mediante LinqToTwitter.
    /// Para usar esta clase hay que hacer una referencia a la librería LinqToTwitter
    /// Esta librería se puede referenciar de dos formas:
    /// 1) usando el adminstrador de paquetes NuGet:   PM> Install-Package linqtotwitter -Version 2.1.11
    /// 2) descargando la libreria directamente desde la web del desarrollador: https://linqtotwitter.codeplex.com/
    /// 
    /// En esta solución se ha optado por la opción 1) ya que solo hay que abrir la consola del NuGet en Visual Studio
    /// y hacer un 'Install-Package linqtotwitter -Version 2.1.11' (sin las ') con lo que ya tendremos la referencia a la dll en nuestro proyecto
    /// Fácil, rápido y sencillo :)
    /// 
    /// Nota sobre la versión de LinqToTwitter:
    /// Se está usando la versión 2.x en lugar de la 3.0.x (que ha fecha de hoy es la última) por sencilla razón de que la versión
    /// 3.0 'solo' tiene soporte para .NET Framework 4.5x
    /// La vesión 2.1.11 tiene soporte para .NET Framework 3.5, 4.0 y 4.5. Como este proyecto es un proyecto de .NET 3.5 se usa
    /// la versión 2.1.11 en lugar de la nueva
    /// </summary>
    public class Linq2Twitter : ITwitter
    {
        #region Fields

        // se usará la capa de acceso a datos para guardar algún posible error en la base de datos
        private IDataAccessLayer _dal;

        // Lista de post que se van a tuitear
        private List<PostModel> _postCollection;

        // Credenciales para la cuenta de Twwitter
        private Twitter_App_Credentials _twitterAppCredentials;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa el objeto
        /// </summary>
        /// <param name="twitterAppCredentials">Credenciales para la cuenta de Twitter. Se obtienen al registrar la aplicación en Twitter Developers</param>
        /// <param name="postCollection">Lista de hilos nuevos que se van a tuitear</param>
        /// <param name="datalayer">Acceso a los datos para guardar posibles excepciones</param>
        public Linq2Twitter(Twitter_App_Credentials twitterAppCredentials, List<PostModel> postCollection, IDataAccessLayer datalayer)
        {
            this._twitterAppCredentials = twitterAppCredentials;
            this._postCollection = postCollection;
            this._dal = datalayer;
        }

        #endregion

        #region Miembros de ITwitter

        #region Métodos privados

        private List<Media> GetMedia(string picURL)
        {
            try
            {
                byte[] byteArr = null;
                WebClient c = new WebClient();
                c.Proxy = null;

                byteArr = c.DownloadData(picURL);

                // debug ----------------------------------------
                //var media =
                //    new List<Media>
                //    {
                //        new Media
                //        {
                //            ContentType = MediaContentType.Png,
                //            Data = byteArr,
                //            FileName = System.IO.Path.GetFileName(@"c:\temp\xxxxx.png")
                //        }
                //    };
                // ----------------------------------------------

                MediaContentType mc = MediaContentType.Jpeg;
                if (picURL.ToLower().EndsWith(".png"))
                    mc = MediaContentType.Png;
                else if (picURL.ToLower().EndsWith(".gif"))
                    mc = MediaContentType.Gif;

                var media =
                    new List<Media>
                {
                    new Media
                    {
                        ContentType = mc,
                        Data = byteArr
                    }
                };

                return media;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        public void Twitear(string prefixTweet = "")
        {
            var auth = new SingleUserAuthorizer
            {
                Credentials = new InMemoryCredentials
                {
                    ConsumerKey = _twitterAppCredentials.ConsumerKey,
                    ConsumerSecret = _twitterAppCredentials.ConsumerSecret,
                    OAuthToken = _twitterAppCredentials.OAuthToken,
                    AccessToken = _twitterAppCredentials.AccessToken
                }
            };

            foreach (var post in _postCollection)
            {
                try
                {
                    post.Tweet = prefixTweet + post.TituloPost;
                    if (post.EsPostMercadillo)
                    {
                        if (post.Precio != null)
                            post.Tweet += string.Format(" [{0} €]", post.Precio);
                    }

                    int longitudMaximaTituloTweet = 115;

                    if (post.ImagenLink != null || post.YoutubeID != null)
                        longitudMaximaTituloTweet = 90;

                    if (post.Tweet.Length > longitudMaximaTituloTweet)
                        post.Tweet = post.Tweet.Substring(0, longitudMaximaTituloTweet);

                    // Usar servicio de Bitly como acortador de links
                    IAcortadorLink acortadorLink = new Bitly(_dal); 
                    string linkCorto = null;
                    linkCorto = acortadorLink.GetShortLink(post.Topic_Url);

                    post.Tweet += linkCorto == null ? " " + post.Topic_Url : " " + linkCorto;

                    if (post.YoutubeID != null)
                    {
                        post.Tweet += " ► youtu.be/" + post.YoutubeID;

                        // si el post tiene vídeo de youtube automaticamente la posible imagen que pudiera tener se pone a null
                        // con lo que prevalece el video a la foto
                        post.ImagenLink = null;
                    }

                    bool tImagenOK = false;

                    using (var twitterCtx = new TwitterContext(auth))
                    {
                        // Tweet con imagen
                        if (post.ImagenLink != null)
                        {
                            try
                            {
                                var media = GetMedia(post.ImagenLink);
                                var tweetM = twitterCtx.TweetWithMedia(post.Tweet, true, media);
                                tImagenOK = true;

                                int p = tweetM.Text.LastIndexOf("http");
                                post.ImagenLink = tweetM.Text.Substring(p).Trim();
                            }
                            catch (Exception)
                            {
                                tImagenOK = false; // prueba de tuitear sin la foto
                                post.ImagenLink = "(*)" + post.ImagenLink;
                                if (post.ImagenLink.Length > 128)
                                    post.ImagenLink = post.ImagenLink.Substring(0, 128);
                            }
                        }

                        if (tImagenOK == false)
                        {
                            var tweet = twitterCtx.UpdateStatus(post.Tweet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    post.ErrorAlTweetear = true;
                    _dal.InsertarErrorBdd("Error al Twittear (tweet=" + post.Tweet + ")  [ex: " + ex.Message + "]");
                }
            }
        }

        #endregion
    }
}