using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace meriBOTijos.ZonaForo_Helpers
{
    public class PostModel
    {
        public int Topic_ID { get; set; }

        /// <summary>
        /// Dirección del post en formato: http://zonaforo.meristation.com/topic/TOPIC_ID/
        /// </summary>
        public string Topic_Url { get; set; }

        /// <summary>
        /// Indica el título del post
        /// Tamaño máximo: 128 char
        /// </summary>
        public string TituloPost { get; set; }

        /// <summary>
        /// Indica el nombre del forero que ha creado el hilo
        /// Tamaño máximo: 32 char
        /// </summary>
        public string Forero { get; set; }

        /// <summary>
        /// indica el ID del Video de youtube. Por defecto solo se almacena el ID
        /// En la insercción se añade el string 'http://youtu.be/' en el campo [YOUTUBELINK]
        /// Si hay vídeo e imagen en el post, solo se tuitea el vídeo
        /// </summary>
        public string YoutubeID { get; set; }

        /// <summary>
        /// Link con la dirección real de la imagen en el hilo
        /// Si se consigue tweetear el hilo con la imagen el link para a ser
        /// del estilo http://t.co/XXXXXXXXXX 
        /// </summary>
        public string ImagenLink { get; set; }

        /// <summary>
        /// Texto completo del Tweet
        /// </summary>
        public string Tweet { get; set; }

        /// <summary>
        /// Indica si ese post se pudo tweetear o no
        /// </summary>
        public bool ErrorAlTweetear { get; set; }

        /// <summary>
        /// Precio. SOLO BOTs de MeriMercadillo
        /// </summary>
        public string Precio { get; set; }

        public bool EsPostMercadillo { get; set; }
    }
}