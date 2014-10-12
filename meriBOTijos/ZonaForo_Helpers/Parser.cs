using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using meriBOTijos.ZonaForo_Helpers.DataAccessLayer;

namespace meriBOTijos.ZonaForo_Helpers
{
    public class Parser
    {

        #region Fieds

        // se usará la capa de acceso a datos para guardar algún posible error en la base de datos
        private IDataAccessLayer _dal;
        
        // Dirección del foro que se está parseando (OT, Mericonsolas, Deportes, etc)
        private string _urlForo;

        // Colección de los nuevos post
        private List<PostModel> _postCollection;

        /// <summary>
        /// Valor mínimo de mensajes que tiene que tener un forero para poder tuitear imagenes
        /// Esta restricción no se aplica al Mercadillo
        /// </summary>
        private const int MINIMO_MENSAJES_IMAGENES = 300;

        // Dirección del hilo que se está parseando
        private const string URL_ZONAFORO_TOPIC = "http://zonaforo.meristation.com/topic/{0}/";

        // Objeto WebClient para 'bajarse' el código html
        private WebClient wclient;

        // ¿El foro que se está parseando es una sección del mercadillo?
        private bool _parsingMercadillo;

        /// <summary>
        /// Estructura de los posibles hilos nuevos
        /// </summary>
        public class HilosCandidatos
        {
            /// <summary>
            /// Indica el numero del topic del hilo para ser usado con URL_ZONAFORO_TOPIC 
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Título del hilo que se ha evaluado desde la pantalla principal de la sección del foro (OT, Mericonsolas. Deportes, etc)
            /// </summary>
            public string Titulo { get; set; }

            /// <summary>
            /// Nombre del forero que ha abierto el hilo
            /// </summary>
            public string Forero { get; set; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa el objeto
        /// </summary>
        /// <param name="urlForo">Dirección del indice del foro que se quiere parsear</param>
        /// <param name="postCollection">objeto donde se guardarán los posibles hilos nuevos</param>
        /// <param name="datalayer">Acceso a la BDD para guardar posibles excepciones</param>
        public Parser(string urlForo, List<PostModel> postCollection, IDataAccessLayer datalayer)
        {
            this._urlForo = urlForo;
            this._postCollection = postCollection;
            this._dal = datalayer;

            this.wclient = new WebClient();
        }

        #endregion

        #region PARSING ZonaForo

        #region Parsear el indice del foro especificado (OT, Mericonsolas, Deportes, etc)

        /// <summary>
        /// Este método parsea la primera página del foro especificado.
        /// Los 'candidatos' a nuevos hilos se guardan en la lista 'Hilos'
        /// El proceso es el siguiente:
        /// 1) Se consulta a la BDD cual es el último topic ID que se ha tuiteado con anterioridad
        /// 2) Se parsea el código HTML que se obtiene con wclient, obteniendo el ID, el titulo y el forero que ha iniciado el hilo
        ///    2.1) Si el TopicID del hilo que se ha parseado es menor que el ultimo tuit se añade a 'Hilos'
        ///    2.2) Se ordena 'Hilos' de menor a mayor TopicID
        ///    2.3) Si hay mas de 5 'Hilos' nos quedamos con los 5 más antiguos. Esto se hace para no tuitear 23 hilos de golpe
        ///         ya que si se tuitean muchos tweets de golpe Twitter puede 'banear' temporalmente la aplicación que hemos dado
        ///         de alta en la página de desarrollo de Twitter
        ///    2.4) para cada uno de los candidatos en 'Hilos' se parsea para ver si tiene imagenes o vídeos de Youtube. Esto lo realiza
        ///         el método GetPostData()
        /// </summary>
        /// <param name="esMercadillo">Indica si el foro pertenece al Mercadillo. Se usa principalmente para quitar la restricción de las imágenes</param>
        public void GetForoData(bool esMercadillo = false)
        {
            this._parsingMercadillo = esMercadillo;

            int START_LINE_PARSING = 600;
            const string PARSE_topicID = "<a itemprop=\"url\" id=\"tid-link-";      // sample: <a itemprop="url" id="tid-link-1890334" href="http://zonaforobeta.meristation.com/topic/1890334/"
            const string PARSE_titulo = "<span itemprop=\"name\">";                 // sample: <span itemprop="name">[Hilo oficial] Quiero estudiar videojuegos</span>
            const string PARSE_forero = "title='Mostrar perfil'><span itemprop=\"name\">";  // sample: title='Mostrar perfil'><span itemprop="name">Ellolo17</span></a>

            const string PARSE_FinPagina = "</table>";

            wclient.Proxy = null;

            int lastTopicBdd = -1;
            lastTopicBdd = _dal.ObtenerUltimoTopicID_Tweeteado();

            // debug: -------------
            // lastTopicBdd = 2225250;
            // --------------------

            if (lastTopicBdd == -1) //Algo salio mal al leer de la base de datos! OMG :(
                return;

            List<HilosCandidatos> Hilos = new List<HilosCandidatos>();

            string topicID = string.Empty;
            string topicTitle = string.Empty;
            string topicForero = string.Empty;
            try
            {
                var data = wclient.DownloadData(_urlForo); // Nos 'bajamos' el HTML
                string str = System.Text.Encoding.UTF8.GetString(data);
                string[] sd = str.Split((char)10);

                // debug: -----------------------------------------------
                // string[] sd = System.IO.File.ReadAllLines("19836.txt");
                // ------------------------------------------------------

                for (int i = START_LINE_PARSING; i < sd.Length; i++)
                {
                    if (sd[i].IndexOf(PARSE_FinPagina) != -1)
                        break;

                    if (sd[i].IndexOf(PARSE_topicID) != -1)
                    {
                        string tmpLine = sd[i].Trim();
                        int start = tmpLine.IndexOf(PARSE_topicID);
                        int end = tmpLine.IndexOf("\" href=");

                        topicID = tmpLine.Substring(start + PARSE_topicID.Length, (end - start) - PARSE_topicID.Length).Trim();
                    }

                    if (sd[i].IndexOf(PARSE_titulo) != -1 && topicTitle == string.Empty)
                    {
                        string tmpLine = sd[i].Trim();
                        int start = tmpLine.IndexOf(PARSE_titulo);
                        int end = tmpLine.IndexOf("</span>");

                        topicTitle = tmpLine.Substring(start + PARSE_titulo.Length, (end - start) - PARSE_titulo.Length).Trim();

                        // convierte los caracteres HTML (&#xx) en su equivalente de texto (http://www.ascii.cl/htmlcodes.htm)
                        topicTitle = HttpUtility.HtmlDecode(topicTitle);
                    }

                    if (sd[i].IndexOf(PARSE_forero) != -1)
                    {
                        string tmpLine = sd[i].Trim();
                        int start = tmpLine.IndexOf(PARSE_forero);
                        int end = tmpLine.IndexOf("</span>");
                        topicForero = tmpLine.Substring(start + PARSE_forero.Length, (end - start) - PARSE_forero.Length).Trim();

                        int n;
                        if (int.TryParse(topicID, out n))
                        {
                            HilosCandidatos candidato = new HilosCandidatos();
                            candidato.ID = n;
                            candidato.Titulo = topicTitle;
                            candidato.Forero = topicForero;

                            if (candidato.Titulo.Length > 128)
                                candidato.Titulo = candidato.Titulo.Substring(0, 128);

                            if (candidato.Forero.Length > 32)
                                candidato.Forero = candidato.Forero.Substring(0, 32);

                            if (candidato.ID > lastTopicBdd)
                            {
                                if (topicTitle != string.Empty && topicForero != string.Empty) // Esto no debería ocurrir nunca
                                    Hilos.Add(candidato);
                            }
                        }

                        topicID = string.Empty;
                        topicTitle = string.Empty;
                        topicForero = string.Empty;
                    }
                } // fin for evaluacion FORO

                // ordenar los candidatos de menor a mayor topicID
                Hilos = Hilos.OrderBy(x => x.ID).ToList();

                // si hay más de 5 candidatos quedarse con los 5 primeros (5 más antiguos)
                if (Hilos.Count > 5)
                    Hilos.RemoveRange(5, Hilos.Count - 5);

                foreach (var h in Hilos)
                {
                    string urlPost = string.Format(URL_ZONAFORO_TOPIC, h.ID);
                    GetPostData(urlPost, h);
                }
            }
            catch (Exception ex)
            {
                _dal.InsertarErrorBdd("GetForoData(" + _urlForo + ") " + ex.Message);
            }
        }

        #endregion

        #region Parsear un hilo

        /// <summary>
        /// Este método parsea un hilo en particular, sea del foro que sea
        /// Nótese que es un método privado solo accesible dentro de esta clase.
        /// Este método va rellenando la _postCollection que se ha inyectado en el constructor.
        /// </summary>
        /// <param name="topicUrl">Dirección COMPLETA del hilo correctamente formateada</param>
        /// <param name="candidatoInfo">Información del candidato para añadirlo a la _postCollection y así no tener que volver a parsear dentro del propio hilo</param>
        private void GetPostData(string topicUrl, HilosCandidatos candidatoInfo)
        {
            try
            {
                int START_LINE_PARSING = 600;
                const string PARSE_totalMensajes = "<li class='post_count desc lighter'>";
                //el numero de mensajes siempre va en la siguiente linea de PARSE_toalMensajes. Ejemplo:
                //		<li class='post_count desc lighter'>
                //          Mensajes: 5.129
                //      </li>
                //const string PARSE_inicioPost = "<div itemprop=\"commentText\""; // sample: <div itemprop="commentText" class='post entry-content '>
                const string PARSE_image = "<img class='bbc_img' src=\""; // sample: <img class='bbc_img' src="http://www.danasoft.com/vipersig.jpg" alt="vipersig.jpg">
                const string PARSE_image2 = "<img class='bbc_img' src='"; // <img class='bbc_img' src='http://i.imgur.com/XBzgA8H.jpg' alt='Imagen Enviada'  />
                const string PARSE_youtube = "class=\"EmbeddedVideo\" type=\"text/html\" width=\"640\" height=\"390\" src=\""; // sample: class="EmbeddedVideo" type="text/html" width="640" height="390" src="http://youtube.com/embed/np0solnL1XY?html5=1&fs=1" frameborder="0"
                const string PARSE_firma = "<div class=\"signature\" data-memberid=\""; // sample: <div class="signature" data-memberid="517023">
                const string PARSE_finPost = "class='post_controls clear clearfix'"; // sample: <ul id='postControlsNormal_41096912' class='post_controls clear clearfix' >
                const string PARSE_precio = "<div style=\"display:block\">Precio: "; //sample: <div style="display:block">Precio: 500 </div>

                var data = wclient.DownloadData(topicUrl); // Nos 'bajamos' el HTML
                string str = System.Text.Encoding.UTF8.GetString(data);
                string[] sd = str.Split((char)10);

                // debug ------------------------------------------------------
                // string[] sd = System.IO.File.ReadAllLines("post2227814.txt");
                // ------------------------------------------------------------

                PostModel post = new PostModel();
                post.Forero = candidatoInfo.Forero;
                post.TituloPost = candidatoInfo.Titulo;
                post.Topic_ID = candidatoInfo.ID;
                post.Topic_Url = topicUrl;
                post.EsPostMercadillo = this._parsingMercadillo;

                int mensajesForero = 0;

                for (int i = START_LINE_PARSING; i < sd.Length; i++)
                {
                    if (sd[i].IndexOf(PARSE_firma) != -1 || sd[i].IndexOf(PARSE_finPost) != -1)
                    {
                        this._postCollection.Add(post);
                        break;
                    }

                    if (sd[i].IndexOf(PARSE_totalMensajes) != -1) // Mensajes del forero
                    {
                        string tmpLine = sd[i + 1].Trim();
                        tmpLine = tmpLine.Replace("Mensajes: ", "");
                        tmpLine = tmpLine.Replace(".", "");
                        int n;
                        if (int.TryParse(tmpLine, out n))
                            mensajesForero = n;

                        if (this._parsingMercadillo)
                            mensajesForero = MINIMO_MENSAJES_IMAGENES;
                    }

                    // Imagenes
                    if (sd[i].IndexOf(PARSE_image) != -1 && mensajesForero >= MINIMO_MENSAJES_IMAGENES &&
                        post.ImagenLink == null)
                    {
                        string tmpLine = sd[i].Trim();
                        int start = tmpLine.IndexOf(PARSE_image);
                        string tmpData = string.Empty;
                        for (int j = start + PARSE_image.Length; j < tmpLine.Length - 1; j++)
                        {
                            char x = tmpLine[j];
                            if (x != '"')
                                tmpData += x;
                            else
                                break;
                        }

                        if (tmpData.ToLower().EndsWith(".jpg") || tmpData.ToLower().EndsWith(".png") || tmpData.ToLower().EndsWith(".gif"))
                            post.ImagenLink = tmpData;
                    }

                    // Imagenes (2) [Visto en los mensajes del Mercadillo]
                    if (sd[i].IndexOf(PARSE_image2) != -1 && mensajesForero >= MINIMO_MENSAJES_IMAGENES &&
                        post.ImagenLink == null)
                    {
                        string tmpLine = sd[i].Trim();
                        int start = tmpLine.IndexOf(PARSE_image2);
                        string tmpData = string.Empty;
                        for (int j = start + PARSE_image2.Length; j < tmpLine.Length - 1; j++)
                        {
                            char x = tmpLine[j];
                            if (x != '\'')
                                tmpData += x;
                            else
                                break;
                        }

                        if (tmpData.ToLower().EndsWith(".jpg") || tmpData.ToLower().EndsWith(".png") || tmpData.ToLower().EndsWith(".gif"))
                            post.ImagenLink = tmpData;
                    }

                    // Video Youtube
                    if (sd[i].IndexOf(PARSE_youtube) != -1 && post.YoutubeID == null)
                    {
                        string tmpLine = sd[i].Trim();
                        int start = tmpLine.IndexOf(PARSE_youtube);
                        string tmpData = string.Empty;
                        for (int j = start + PARSE_youtube.Length; j < tmpLine.Length - 1; j++)
                        {
                            char x = tmpLine[j];
                            if (x != '?')
                                tmpData += x;
                            else
                            {
                                // en este punto tmpData vale: http://youtube.com/embed/XXXXXXXXXXX
                                // y lo que nosotros queremos es solamente el "id" : XXXXXXXXXXX
                                int lastSlash = -1;
                                lastSlash = tmpData.LastIndexOf("/");
                                if (lastSlash != -1)
                                    tmpData = tmpData.Substring(lastSlash + 1);
                                else
                                    tmpData = string.Empty;

                                break;
                            }
                        }

                        if (tmpData != string.Empty)
                            post.YoutubeID = tmpData;
                    }

                    // Precio: (a priori solo para BOTS de Merimercadillo)
                    if (sd[i].IndexOf(PARSE_precio) != -1)
                    {
                        string tmpLine = sd[i].Trim();
                        int start = tmpLine.IndexOf(PARSE_precio);
                        string tmpPrecio = string.Empty;
                        for (int j = start + PARSE_precio.Length; j < tmpLine.Length - 1; j++)
                        {
                            char x = tmpLine[j];
                            if (x != ' ' && x != '<')
                                tmpPrecio += x;
                            else
                            {
                                tmpPrecio = tmpPrecio.Replace("€", "").Trim();
                                break;
                            }
                        }

                        if (tmpPrecio != string.Empty)
                            post.Precio = tmpPrecio;
                    }
                }
            }
            catch (Exception ex)
            {
                _dal.InsertarErrorBdd("GetPostData(" + topicUrl + ") " + ex.Message);
            }
        }

        #endregion

        #endregion
    }
}