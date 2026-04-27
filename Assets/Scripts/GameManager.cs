using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Estado del Jugador")]
    public int vidas = 3;
    public int coleccionablesRecogidos = 0;
    public int coleccionablesGanar = 5;

    [Header("Gestión de Audio")]
    public AudioSource musicaFondo;
    public AudioSource efectosSource;
    public bool musicaPermitida = true; 
    public bool efectosPermitidos = true; 

    [Header("Pantallas de Fin de Juego")]
    public TextMeshProUGUI textoResumenVidas;
    public TextMeshProUGUI textoResumenColeccionables;
    
    [Header("Referencias UI")]
    public GameObject panelGameOver;

    private bool juegoTerminado = false;
private void OnEnable()
{
    SceneManager.sceneLoaded += AlCargarEscena;
}

private void OnDisable()
{
    SceneManager.sceneLoaded -= AlCargarEscena;
}

private void AlCargarEscena(Scene escena, LoadSceneMode modo)
{
    // Si volvemos al Menú Principal, buscamos los botones y los conectamos
    if (escena.name == "MenuPrincipal")
    {
        VincularBotonesMenu();
    }
} 
private void VincularBotonesMenu()
{
    Button btnJugar = GameObject.Find("Comenzar")?.GetComponent<Button>();
    Button btnAjustes = GameObject.Find("Ajustes")?.GetComponent<Button>();
    Button btnCreditos = GameObject.Find("Creditos")?.GetComponent<Button>();
    Button btnSalir = GameObject.Find("Salir")?.GetComponent<Button>();

 
    if (btnJugar != null) { btnJugar.onClick.RemoveAllListeners(); btnJugar.onClick.AddListener(IniciarJuego); }
    if (btnAjustes != null) { btnAjustes.onClick.RemoveAllListeners(); btnAjustes.onClick.AddListener(cargarAjustes); }
    if (btnCreditos != null) { btnCreditos.onClick.RemoveAllListeners(); btnCreditos.onClick.AddListener(cargarCreditos); }
    if (btnSalir != null) { btnSalir.onClick.RemoveAllListeners(); btnSalir.onClick.AddListener(SalirJuego); } // <-- LÍNEA NUEVA
    
    Debug.Log("✅ Botones del Menú vinculados automáticamente");
}
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 

            if (musicaFondo == null)
            {
                GameObject objetoMusica = GameObject.Find("Audio Source Menú principal");
                if (objetoMusica != null)
                {
                    musicaFondo = objetoMusica.GetComponent<AudioSource>();
                    objetoMusica.transform.SetParent(this.transform);
                }
            }
        }
        else
        {
            AudioSource[] todosLosAudios = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude);
            foreach (AudioSource audio in todosLosAudios)
            {
                if (audio.gameObject.name == "Audio Source Menú principal" && audio.transform.parent == null)
                {
                    Destroy(audio.gameObject);
                }
            }
            
            Destroy(gameObject); 
        }
    }

    // --- MÉTODOS DE GESTIÓN GLOBAL ---
    public void cargarNivel() { SceneManager.LoadScene("Juego"); }
    public void cargarCreditos() { SceneManager.LoadScene("Creditos"); }
    public void cargarAjustes() { SceneManager.LoadScene("Ajustes"); }
    public void VolverAlMenuPrincipal() { SceneManager.LoadScene("MenuPrincipal"); }
    public void SalirJuego() 
    { 
        Debug.Log("🛑 Saliendo del juego...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        // Si estamos jugando en el móvil o PC ya exportado...
        #else
            Application.Quit();
        #endif
    }

    public void IniciarJuego()
    {
        Time.timeScale = 1f; 
        juegoTerminado = false;
        vidas = 3; 
        coleccionablesRecogidos = 0;
        SceneManager.LoadScene("Nivel"); 
    }

    public void ReintentarNivel()
    {
        Time.timeScale = 1f; 
        juegoTerminado = false;        
        vidas = 3; 
        coleccionablesRecogidos = 0; 
        SceneManager.LoadScene("Nivel"); 
    }

    // --- MÉTODOS DE GESTIÓN DE MÚSICA Y EFECTOS ---
    public void ReproducirMusica()
    {
        if (this != Instance) { Instance.ReproducirMusica(); return; }
        musicaPermitida = true; 
        if (musicaFondo != null)
        {
            musicaFondo.mute = false; 
            if (!musicaFondo.isPlaying) musicaFondo.Play();
        }
   Debug.Log("🔇 Música ACTIVADA");
    }

    public void DetenerMusica()
    {
        if (this != Instance) { Instance.DetenerMusica(); return; 
    }
        musicaPermitida = false; 
        if (musicaFondo != null) musicaFondo.mute = true; 
   Debug.Log("🔇 Música DESACTIVADA");
    }

    public void CambiarMusica(AudioClip nuevaPista)
    {
        if (musicaFondo != null)
        {
            musicaFondo.Stop();
            musicaFondo.clip = nuevaPista;
            if (musicaPermitida) 
            {
                musicaFondo.volume = 1;
                musicaFondo.Play();
            }
        }
    }

    public void ComprobarEstadoMusica()
    {
        if (musicaFondo == null)
        {
            GameObject objetoMusica = GameObject.Find("Audio Source Menú principal");
            if (objetoMusica != null) musicaFondo = objetoMusica.GetComponent<AudioSource>();
        }

        if (musicaFondo != null)
        {
            if (musicaPermitida && !musicaFondo.isPlaying)
            {
                musicaFondo.volume = 1;
                musicaFondo.Play();
            }
            else if (!musicaPermitida) 
            {
                musicaFondo.volume = 0;
                musicaFondo.Stop();
            }
        }
    }

    public void ActivarEfectos()
    {
        efectosPermitidos = true;
        Debug.Log("🔊 Efectos ACTIVADOS");
    }

    public void DesactivarEfectos()
    {
        efectosPermitidos = false;
        if (efectosSource != null) efectosSource.Stop(); 
        Debug.Log("🔇 Efectos DESACTIVADOS");
    }

    public void ReproducirEfecto(AudioClip clipSonido)
    {
        if (clipSonido != null && efectosSource != null && efectosPermitidos)
        {
            efectosSource.PlayOneShot(clipSonido);
        }
    }

    // --- MÉTODOS DE JUEGO ---
    public void RestarVida()
    {
        vidas--;
        if (vidas <= 0) ActivarGameOver();
    }

    public void AgregarColeccionable()
    {
        coleccionablesRecogidos++;
        if (coleccionablesRecogidos >= coleccionablesGanar) ActivarVictoria();
    }

    public void ActivarVictoria()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("PantallaVictoria"); 
    }

    public void ActivarGameOver()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;
        Time.timeScale = 1f; 
        Invoke("CargarEscenaDerrota", 0.6f); 
    }

    private void CargarEscenaDerrota()
    {
        SceneManager.LoadScene("PantallaGameOver"); 
    }
}