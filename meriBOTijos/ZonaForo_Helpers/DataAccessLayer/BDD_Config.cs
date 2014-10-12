using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace meriBOTijos.ZonaForo_Helpers.DataAccessLayer
{
    public class BDD_Config
    {
        /// <summary>
        /// Cadena de conexión de la base de datos
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Tabla para guardar los posibles errores
        /// </summary>
        public string TableNameError { get; set; }

        /// <summary>
        /// Tabla que contiene el ID del último post tweeteado.
        /// </summary>
        public string TableNameLastTopicID { get; set; }

        /// <summary>
        /// Tabla donde se guardan los tweets de los hilos nuevos
        /// </summary>
        public string TableName_Data { get; set; }

        /// <summary>
        /// Valor del foro que se está parseando para el campo [ForumID] de la BDD: http://zonaforo.meristation.com/forum/[Forum_ID]/
        /// </summary>
        public int ForumID { get; set; }

        /// <summary>
        /// Descripción interna del foro que se está parseando para el campo [ForumDesc] de la BDD
        /// Tamaño máximo: 50 char
        /// </summary>
        public string ForumDesc { get; set; }
    }
}