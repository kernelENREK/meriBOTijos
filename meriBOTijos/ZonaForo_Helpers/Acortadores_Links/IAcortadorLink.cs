using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace meriBOTijos.ZonaForo_Helpers.Acortadores_Links
{
    public interface IAcortadorLink
    {
        /// <summary>
        /// Obtiene el 'link corto' usando un servicio de acortadores de links
        /// </summary>
        /// <param name="longLink">dirección completa del link que se quiere acortar</param>
        /// <returns>devuelve el link corto o null en caso de error</returns>
        string GetShortLink(string longLink);

    }
}
