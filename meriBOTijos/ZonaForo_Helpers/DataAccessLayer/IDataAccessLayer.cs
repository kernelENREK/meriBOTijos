using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace meriBOTijos.ZonaForo_Helpers.DataAccessLayer
{
    public interface IDataAccessLayer
    {
        /// <summary>
        /// Guarda un log en un fichero de texto. A priori solo se utilizará en una exception de InsertarErrorBdd
        /// </summary>
        /// <param name="datos"></param>
        void SaveToLogFile(string datos);

        /// <summary>
        /// Inserta los posibles errores en la BDD
        /// </summary>
        /// <param name="errorMsg"></param>
        void InsertarErrorBdd(string errorMsg);

        /// <summary>
        /// Devuelve el último ID del hilo 'nuevo' en el anterior ciclo
        /// De esta forma se puede saber rápidamente si hay nuevos hilos (si el topic actual > anterior ciclo ----> post nuevo
        /// </summary>
        /// <returns></returns>
        int ObtenerUltimoTopicID_Tweeteado();

        /// <summary>
        /// Actualiza la base de datos con los nuevos hilos        
        /// </summary>
        /// <param name="hilosNuevos"></param>
        void ActualizarBDD(List<PostModel> hilosNuevos);

        /// <summary>
        /// Actualiza la fecha del ultimo chequeo. Util para ver que el 'CRON' del bot está funcionando
        /// </summary>
        void ActualizarBDDFecha();

    }
}
