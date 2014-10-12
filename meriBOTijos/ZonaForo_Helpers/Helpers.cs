using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace meriBOTijos.ZonaForo_Helpers
{
    public static class Helpers
    {
        /// <summary>
        /// Obtiene la hora de España, ya que si usanmos "Now()" lo que devuelve es la hora del servidor donde
        /// estan alojadas nuestras páginas, que no tiene porque ser un servidor que este en España
        /// Se usa esta hora para que los regitros de tipo 'fecha' en la base de datos esten referenciados 
        /// siempre a la hora Española
        /// </summary>
        /// <returns></returns>
        public static DateTime Now_Spain()
        {
            // Get time in local time zone 
            DateTime thisTime = DateTime.Now;

            // Hora 'Española'
            TimeZoneInfo tst = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
            DateTime tstTime = TimeZoneInfo.ConvertTime(thisTime, TimeZoneInfo.Local, tst);

            return tstTime;
        }
    }
}