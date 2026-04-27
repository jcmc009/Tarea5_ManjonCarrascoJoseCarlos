using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 5f;

    [Header("Efectos de Sonido")]
    public AudioClip coinSoundEffect;  
    public AudioClip jumpSoundEffect; 
    public AudioClip hurtSoundEffect; 
    public AudioClip winSoundEffect;
    public AudioClip overSoundEffect;
    private AudioSource audioSource; 
    
    private bool estaMuerto = false;
    private Rigidbody2D rb;
    private float movimientoHorizontal = 0f;
    private bool enSuelo = true; 
    private SpriteRenderer spriteRenderer;
    private Animator animator;
     
    [Header("Interfaz (HUD)")]
    [SerializeField] private TextMeshProUGUI cherryCountText;
    [SerializeField] private TextMeshProUGUI liveCountText;

    private int cerezasAnteriores = -1;
    private int vidasAnteriores = -1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        ActualizarHUD();

        if (estaMuerto) return;

        ManejarMovimientoYAnimaciones();
    }

    void FixedUpdate()
    {
        if (!estaMuerto) 
        {
            rb.linearVelocity = new Vector2(movimientoHorizontal * velocidad, rb.linearVelocity.y);
        }
    }

    // --- MÉTODOS DE ACTUALIZACIÓN ---

    private void ActualizarHUD()
    {
        if (GameManager.Instance != null)
        {
            if (cherryCountText != null && GameManager.Instance.coleccionablesRecogidos != cerezasAnteriores) 
            {
                cerezasAnteriores = GameManager.Instance.coleccionablesRecogidos;
                cherryCountText.text = cerezasAnteriores.ToString();
            }
            if (liveCountText != null && GameManager.Instance.vidas != vidasAnteriores) 
            {
                vidasAnteriores = GameManager.Instance.vidas;
                liveCountText.text = vidasAnteriores.ToString();
            }
        }
    }

    private void ManejarMovimientoYAnimaciones()
    {
        float nuevoMovimiento = 0f; 
        bool quiereSaltar = false;

        animator.SetBool("isRunning", rb.linearVelocityX != 0); 
        animator.SetBool("isJumping", !enSuelo && rb.linearVelocity.y > 0.1f);
        animator.SetBool("isFalling", !enSuelo && rb.linearVelocity.y < -0.1f);

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch toque = Input.GetTouch(i);
                
                // Mover izquierda/derecha
                if (toque.position.x < Screen.width / 2f)
                {
                    nuevoMovimiento = -1f; 
                    spriteRenderer.flipX = true;
                }
                else if (toque.position.x > Screen.width / 2f) 
                {
                    nuevoMovimiento = 1f;  
                    spriteRenderer.flipX = false;
                }

                // Saltar con deslizamiento hacia arriba
                if (toque.phase == TouchPhase.Moved && toque.deltaPosition.y > 10f)
                {
                    quiereSaltar = true;
                }
            }
        }

        movimientoHorizontal = nuevoMovimiento;

        if (quiereSaltar && enSuelo) Saltar();
    }

    void Saltar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
        enSuelo = false;
        
        ReproducirSonido(jumpSoundEffect);
    }

    // --- COLISIONES ---

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Suelo")) enSuelo = true;

        if (collision.gameObject.CompareTag("Enemigo") && !estaMuerto)
        {
            ProcesarGolpeEnemigo(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Suelo")) enSuelo = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Suelo")) enSuelo = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("collectible"))
        {
            ReproducirSonido(coinSoundEffect);
            if (GameManager.Instance != null) GameManager.Instance.AgregarColeccionable(); 
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Enemigo") && !estaMuerto)
        {
            ProcesarGolpeEnemigo(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Meta"))
        {
            ReproducirSonido(winSoundEffect);
            if (GameManager.Instance != null) GameManager.Instance.ActivarVictoria();
        }
    }

    private void ProcesarGolpeEnemigo(GameObject enemigo)
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.vidas <= 1)
            {
                estaMuerto = true; 
                ReproducirSonido(overSoundEffect);
            }
            else 
            {
                ReproducirSonido(hurtSoundEffect);
            }
            GameManager.Instance.RestarVida();
        }
        Destroy(enemigo); 
    }

    // --- FUNCIÓN DE AYUDA PARA SONIDOS---
    private void ReproducirSonido(AudioClip clip)
    {
        // Esta función hace la comprobación pesada una sola vez para todos
        if (clip != null && audioSource != null && GameManager.Instance != null && GameManager.Instance.efectosPermitidos)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}