using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace meriBOTijos.ZonaForo_Helpers.Twitter
{
    public interface ITwitter
    {
        /// <summary>
        /// tuitea un post nuevo
        /// </summary>
        /// <param name="prefixTweet">prefijo que se antepone al texto del tweet. Útil para el bot del MeriMercadillo y el foro de Música</param>
        void Twitear(string prefixTweet = "");
    }
}
