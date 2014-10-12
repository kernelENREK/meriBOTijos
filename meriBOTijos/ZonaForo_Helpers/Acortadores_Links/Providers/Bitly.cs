using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BitlyDotNET.Interfaces;
using BitlyDotNET.Implementations;
using meriBOTijos.ZonaForo_Helpers;
using meriBOTijos.ZonaForo_Helpers.DataAccessLayer;

namespace meriBOTijos.ZonaForo_Helpers.Acortadores_Links.Providers
{
    /// <summary>
    /// Clase para recuperar el 'link corto' de una dirección mediante el servicio de Bit.ly
    /// Para usar esta clase hay que hacer una referencia a la librería BitlyDotNET
    /// Esta librería se puede referenciar de dos formas:
    /// 1) usando el adminstrador de paquetes NuGet:   PM> Install-Package Bitly.Net
    /// 2) descargando la libreria directamente desde la web del desarrollador: https://code.google.com/p/bitly-dot-net/
    /// 
    /// En esta solución se ha optado por la opción 1) ya que solo hay que abrir la consola del NuGet en Visual Studio
    /// y hacer un 'Install-Package Bitly.Net' (sin las ') con lo que ya tendremos la referencia a la dll en nuestro proyecto
    /// Fácil, rápido y sencillo :)
    /// </summary>
    public class Bitly : IAcortadorLink
    {
        #region Fields

        // se usará la capa de acceso a datos para guardar algún posible error en la base de datos
        private IDataAccessLayer _dal;

        // Para conseguir una APIKEY de Bitly puedes registrarte aquí: http://bit.ly/account/
        private const string BITLY_LOGIN = "PON_AQUI_TU_LOGIN";
        private const string BITLY_APIKEY = "PON_AQUI_TU_APIKEY";

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa el objeto
        /// </summary>
        /// <param name="datalayer">Acceso a la BDD para guardar posibles excepciones</param>
        public Bitly(IDataAccessLayer datalayer)
        {
            this._dal = datalayer;
        }
        
        #endregion

        #region Miembros de IAcortadorLink

        public string GetShortLink(string longLink)
        {
            try
            {
                string lRet = null;
                IBitlyService s = new BitlyService(BITLY_LOGIN, BITLY_APIKEY);

                string shortened;
                StatusCode status;

                status = s.Shorten(longLink, out shortened);

                if (status == StatusCode.OK)
                    lRet = shortened;
                else
                    _dal.InsertarErrorBdd("LinkCortoBitLy(" + longLink + ") Error. StatusCode = " + (int)status + " (" + status.ToString() + ")");

                return lRet;
            }
            catch (Exception ex)
            {
                _dal.InsertarErrorBdd(string.Format("GetShortLink({0}): {1}", longLink, ex.Message));
                return null;
            }
        }

        #endregion
    }
}