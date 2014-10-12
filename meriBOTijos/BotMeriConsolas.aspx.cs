using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using meriBOTijos.ZonaForo_Helpers;
using meriBOTijos.ZonaForo_Helpers.DataAccessLayer;
using meriBOTijos.ZonaForo_Helpers.DataAccessLayer.Providers;
using meriBOTijos.ZonaForo_Helpers.Twitter;
using meriBOTijos.ZonaForo_Helpers.Twitter.Providers;

namespace meriBOTijos
{
    public partial class BotMeriConsolas : System.Web.UI.Page
    {
        #region Fields para MeriConsolas

        // comunes       
        private string URL = "http://zonaforo.meristation.com/forum/{0}/";
        private List<PostModel> _postZF;
        private BDD_Config _bddConfig;
        private IDataAccessLayer _data;
        private const string BDD_TableData = "TwitterBOTZF_DATA";
        private const string BDD_TableLastTopic = "TwitterBOTZF_LASTCHECK";
        private const string BDD_TableError = "TwitterBOTZF_ERROR";
        private Parser _parser;
        private ITwitter _twitter;

        // Especificos para el FORO
        private const bool _esMercadillo = false;
        private const string _ForumDesc = "MeriConsolas";
        private const int _ForumID = 43;
        private const string _prefixTweet = "";

        // Especificos para el Bot @Nombre_de_tu_cuenta_BOTijo_de_Twitter_para_MeriConsolas
        private string ConsumerKey = "XXXXXXXXXXXXXXXXXXXXXX";
        private string ConsumerSecret = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
        private string OAuthToken = "XXXXXXXXXX-XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
        private string AccessToken = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string logFile;
                logFile = Server.MapPath(string.Format("{0}_logData.txt", _ForumID));

                // Inicializar el objeto de SQL
                _bddConfig = new BDD_Config()
                {
                    ConnectionString = WebConfigurationManager.ConnectionStrings["conexion"].ConnectionString,
                    ForumDesc = _ForumDesc,
                    ForumID = _ForumID,
                    TableName_Data = BDD_TableData,
                    TableNameError = BDD_TableError,
                    TableNameLastTopicID = BDD_TableLastTopic
                };

                _data = new SqlServer_DAL(_bddConfig, logFile);

                // Inicializar Lista de post
                _postZF = new List<PostModel>();

                // Inicializar Parser
                _parser = new Parser(string.Format(URL, _ForumID), _postZF, _data);

                // La magia negra está aquí;
                _parser.GetForoData(_esMercadillo);

                if (_postZF.Count > 0)
                {
                    // Tuitear
                    Twitter_App_Credentials _appCredentials = new Twitter_App_Credentials()
                    {
                        AccessToken = this.AccessToken,
                        ConsumerKey = this.ConsumerKey,
                        ConsumerSecret = this.ConsumerSecret,
                        OAuthToken = this.OAuthToken
                    };

                    _twitter = new Linq2Twitter(_appCredentials, _postZF, _data);

                    _twitter.Twitear(_prefixTweet);

                    // Actualizar BDD
                    _data.ActualizarBDD(_postZF);
                }
                else
                    _data.ActualizarBDDFecha();

            }
        }
    }
}