using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace meriBOTijos.ZonaForo_Helpers.DataAccessLayer.Providers
{
    /// <summary>
    /// Implementación de la capa de acceso a datos para SQL SERVER
    /// </summary>
    public class SqlServer_DAL :IDataAccessLayer
    {
        #region Fields

        /// <summary>
        /// Nombre del fichero de log
        /// </summary>
        private string LogFileName { get; set; }

        /// <summary>
        /// Configuración de la BDD inyectada en el constructor
        /// </summary>
        private BDD_Config _bddConfig;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicia el acceso a los datos
        /// </summary>
        /// <param name="bddConfig">Parámetros de configuración que usará la BDD</param>
        /// <param name="logFile">Nombre del fichero de texto de log. Será necesario pasar el nombre como Server.MapPath(nombre)</param>
        public SqlServer_DAL(BDD_Config bddConfig, string logFile)
        {
            this._bddConfig = bddConfig;
            this.LogFileName = logFile;
        }

        #endregion

        #region Miembros de IDataAccessLayer

        public void SaveToLogFile(string datos)
        {
            try
            {
                //using (StreamWriter w = new StreamWriter(LogFileName, true, System.Text.Encoding.UTF8))
                //{
                //    w.WriteLine(Helpers.Now_Spain().ToString("dd/MM/yyyy HH:mm:ss") + ":" + datos);
                //}
            }
            catch (Exception)
            {
            }
        }

        public void InsertarErrorBdd(string errorMsg)
        {
            // Los errores de no poder conectarse a Meri no se guardan en el log
            if (errorMsg.ToLower().Contains("Unable to connect to the remote server".ToLower()) ||
                errorMsg.ToLower().Contains("The remote server returned an error: (500)".ToLower()) ||
                errorMsg.ToLower().Contains("The remote server returned an error: (503)".ToLower()))
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(_bddConfig.ConnectionString))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();

                    cmd.CommandText = "insert into " + _bddConfig.TableNameError + " (" +
                        " [ForumID]" +
                        ",[ForumDesc]" +
                        ",[Date_Spain]" +
                        ",[ErrorDesc]" +
                        ") VALUES (" +
                        " @forumid" +
                        ",@forumdesc" +
                        ",@fecha" +
                        ",@error" +
                        ")";

                    cmd.Parameters.Add("@forumid", SqlDbType.Int).Value = _bddConfig.ForumID;
                    cmd.Parameters.Add("@forumdesc", SqlDbType.NVarChar, 50).Value = _bddConfig.ForumDesc;
                    cmd.Parameters.Add("@fecha", SqlDbType.DateTime).Value = Helpers.Now_Spain();

                    if (errorMsg.Length > 256)
                        errorMsg = errorMsg.Substring(0, 256);

                    cmd.Parameters.Add("@error", SqlDbType.NVarChar, 256).Value = errorMsg;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                SaveToLogFile("InsertarErrorBdd(): " + ex.Message);
            }
        }

        public int ObtenerUltimoTopicID_Tweeteado()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_bddConfig.ConnectionString))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = "select BOT_LastPostID_Tweet from " + _bddConfig.TableNameLastTopicID + " where ForumID=" + _bddConfig.ForumID;

                    var tmp = cmd.ExecuteScalar();

                    int lRet;
                    if (tmp == null)
                    {
                        lRet = 2261070; //No hay ningun registro. Esto debería ocurrir solo la primera vez                    

                        cmd.Parameters.Clear();
                        cmd.CommandText = "insert into " + _bddConfig.TableNameLastTopicID + " (" +
                                        " [ForumID]" +
                                        ",[ForumDesc]" +
                                        ",[BOT_LastPostID_Tweet]" +
                                        ",[BOT_LastDate_CHECK]" +
                                        ") VALUES (" +
                                        " @forumid" +
                                        ",@forumdesc" +
                                        ",@topic" +
                                        ",@fecha" +
                                        ")";

                        cmd.Parameters.Add("@forumid", SqlDbType.Int).Value = _bddConfig.ForumID;
                        cmd.Parameters.Add("@forumdesc", SqlDbType.NVarChar, 50).Value = _bddConfig.ForumDesc;
                        cmd.Parameters.Add("@topic", SqlDbType.Int).Value = 1;
                        cmd.Parameters.Add("@fecha", SqlDbType.DateTime).Value = Helpers.Now_Spain();

                        cmd.ExecuteNonQuery();
                    }
                    else
                        lRet = Convert.ToInt32(tmp);

                    return lRet;
                }
            }
            catch (Exception ex)
            {
                InsertarErrorBdd("ObtenerUltimoTopicID_Tweeteado(): " + ex.Message);
                return -1;
            }
        }

        public void ActualizarBDD(List<PostModel> hilosNuevos)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_bddConfig.ConnectionString))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();


                    foreach (PostModel hilo in hilosNuevos)
                    {
                        cmd.Parameters.Clear();

                        cmd.CommandText = "insert into " + _bddConfig.TableName_Data + " (" +
                                        " [ForumID]" +
                                        ",[ForumDesc]" +
                                        ",[Topic_URL]" +
                                        ",[Titulo]" +
                                        ",[Forero]" +
                                        ",[Fecha]" +
                                        ",[TweetERROR]" +
                                        ",[Tweet]" +
                                        ",[YoutubeLINK]" +
                                        ",[ImagenLINK]" +
                                        ") VALUES (" +
                                        " @forumid" +
                                        ",@forumdesc" +
                                        ",@topicurl" +
                                        ",@titulo" +
                                        ",@forero" +
                                        ",@fecha" +
                                        ",@tweeterror" +
                                        ",@tweet" +
                                        ",@youtubelink" +
                                        ",@imagenlink" +
                                        ")";

                        cmd.Parameters.Add("@forumid", SqlDbType.Int).Value = _bddConfig.ForumID;
                        cmd.Parameters.Add("@forumdesc", SqlDbType.NVarChar, 50).Value = _bddConfig.ForumDesc;

                        cmd.Parameters.Add("@topicurl", SqlDbType.VarChar, 64).Value = hilo.Topic_Url;
                        cmd.Parameters.Add("@titulo", SqlDbType.NVarChar, 128).Value = hilo.TituloPost;
                        cmd.Parameters.Add("@forero", SqlDbType.NVarChar, 32).Value = hilo.Forero;
                        cmd.Parameters.Add("@fecha", SqlDbType.DateTime).Value = Helpers.Now_Spain();
                        cmd.Parameters.Add("@tweeterror", SqlDbType.Bit).Value = hilo.ErrorAlTweetear;
                        cmd.Parameters.Add("@tweet", SqlDbType.NVarChar, 150).Value = hilo.Tweet;

                        if (hilo.YoutubeID != null)
                            cmd.Parameters.Add("@youtubelink", SqlDbType.VarChar, 32).Value = "http://youtu.be/" +
                                                                                              hilo.YoutubeID;
                        else
                            cmd.Parameters.Add("@youtubelink", SqlDbType.VarChar, 32).Value = DBNull.Value;

                        if (hilo.ImagenLink != null)
                            cmd.Parameters.Add("@imagenlink", SqlDbType.VarChar, 128).Value = hilo.ImagenLink;
                        else
                            cmd.Parameters.Add("@imagenlink", SqlDbType.VarChar, 128).Value = DBNull.Value;

                        cmd.ExecuteNonQuery();
                    }

                    cmd.Parameters.Clear();

                    cmd.CommandText = "update " + _bddConfig.TableNameLastTopicID +
                        " set [BOT_LastPostID_Tweet] = @topic" +
                        ",[BOT_LastDate_CHECK] = @fecha" +
                        " where [ForumID] = @id";

                    cmd.Parameters.Add("@topic", SqlDbType.Int).Value = hilosNuevos[hilosNuevos.Count - 1].Topic_ID;
                    cmd.Parameters.Add("@fecha", SqlDbType.DateTime).Value = Helpers.Now_Spain();
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = _bddConfig.ForumID;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                InsertarErrorBdd("ActualizarBDD(): " + ex.Message);
            }
        }

        public void ActualizarBDDFecha()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_bddConfig.ConnectionString))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();

                    //Guardar Fecha del chequeo:
                    cmd.CommandText = "update " + _bddConfig.TableNameLastTopicID +
                        " set [BOT_LastDate_CHECK] = @fecha" +
                        " where [ForumID] = @forumid";

                    cmd.Parameters.Add("@fecha", SqlDbType.DateTime).Value = Helpers.Now_Spain();
                    cmd.Parameters.Add("@forumid", SqlDbType.Int).Value = _bddConfig.ForumID;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                InsertarErrorBdd("ActualizarBDDFecha(): " + ex.Message);
            }
        }

        #endregion
    }
}