using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public float m_MaxGameTime = 240f; // 4 minutos en segundos
    public TextMeshProUGUI m_TimerText;


    private float m_ElapsedTime; // Tiempo total transcurrido desde el inicio de la partida
    private bool m_GameEnded = false;

    public int m_NumRoundsToWin = 5; //Número de rondas que un jugador debe ganar para ganar el juego
    public float m_StartDelay = 3f; //Delay entre las fases de RoundStarting yRoundPlaying
    public float m_EndDelay = 3f; //Delay entre las fases de RoundPlaying y RoundEnding
    public CameraControl m_CameraControl; //Referencia al sccript de CameraControl
    public TextMeshProUGUI m_MessageText; //Referencia al texto para mostrar mensajes
    private bool[] m_PreviousTankStates;

    public GameObject m_TankPrefab; //Referencia al Prefab del Tanque
    public TankManager[] m_Tanks; //Array de TankManagers para controlar cadatanque
    private int m_RoundNumber; //Número de ronda
    private WaitForSeconds m_StartWait; //Delay hasta que la ronda empieza
    private WaitForSeconds m_EndWait; //Delay hasta que la ronda acaba
    private TankManager m_RoundWinner; //Referencia al ganador de la ronda para anunciar quién ha ganado
    private TankManager m_GameWinner; //Referencia al ganador del juego para anunciar quién ha ganado
    private void Start()
    {
        //Creamos los delays para que solo se apliquen una vez
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);
        SpawnAllTanks(); //Generar tanques
        m_PreviousTankStates = new bool[m_Tanks.Length];
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_PreviousTankStates[i] = m_Tanks[i].m_Instance.activeSelf;
        }


        SetCameraTargets(); //Ajustar cámara
        StartCoroutine(GameLoop()); //Iniciar juego
    }
    private void SpawnAllTanks()
    {
        //Recorro los tanques...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            //...los creo, ajusto el número de jugador y ls referencias necesarias para controlarlo
            m_Tanks[i].m_Instance =
            Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }
    private void SetCameraTargets()
    {
        //Creo un array de Transforms del mismo tamaño que el número de tanques
        Transform[] targets = new Transform[m_Tanks.Length];
        //Recorro los Transforms...
        for (int i = 0; i < targets.Length; i++)
        {
            //...lo ajusto al transform del tanque apropiado
            targets[i] = m_Tanks[i].m_Instance.transform;
        }
        //Estos son los targets que la cámara debe seguir
        m_CameraControl.m_Targets = targets;
    }
    //llamado al principio y en cada fase del juego después de otra
    private IEnumerator GameLoop()
    {
        //Empiezo con la corutina RoundStarting y no retorno hasta que finalice
        yield return StartCoroutine(RoundStarting());
        //Cuando finalice RoundStarting, empiezo con RoundPlaying y no retornohasta que finalice
        yield return StartCoroutine(RoundPlaying());
        //Cuando finalice RoundPlaying, empiezo con RoundEnding y no retorno hasta que finalice
        yield return StartCoroutine(RoundEnding());
        if (m_ElapsedTime >= m_MaxGameTime)
        {
            DisableTankControl(); // Detener movimiento y disparo

            m_MessageText.text = "¡TIEMPO AGOTADO!\nAMBOS JUGADORES PIERDEN.";

            // Mostrar por unos segundos
            yield return m_EndWait;
            m_GameEnded = true;
            // Detener completamente la partida (no volver a GameLoop)
            yield break;
    
        }


        //Si aún no ha ganado ninguno
        if (m_GameWinner != null)
        {
            //Si hay un ganador, reinicio el nivel

        }
        else
        {
            //Si no, reinicio lsa corutinas para que continúe el bucle
            //En este caso sin yiend, de modo que esta versión del GameLoop finalizará siempre
            StartCoroutine(GameLoop());
        }
    }
    private IEnumerator RoundStarting()
    {
        // Cuando empiece la ronda reseteo los tanques e impido que se muevan.
        ResetAllTanks();
        DisableTankControl();
        // Ajusto la cámara a los tanques resteteados.
        m_CameraControl.SetStartPositionAndSize();
        // Incremento la ronda y muestro el texto informativo.
        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;
        // Espero a que pase el tiempo de espera antes de volver al bucle.
        yield return m_StartWait;
    }
    private IEnumerator RoundPlaying()
    {

        // Cuando empiece la ronda dejo que los tanques se muevan.
        EnableTankControl();
        // Borro el texto de la pantalla.
        m_MessageText.text = string.Empty;
        // Mientras haya más de un tanque...
        while (!OneTankLeft())
        {
            // Revisamos cambios de estado para detectar muertes
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                bool isAlive = m_Tanks[i].m_Instance.activeSelf;

                // Si antes estaba vivo y ahora no, entonces murió
                if (m_PreviousTankStates[i] && !isAlive)
                {
                    m_Tanks[i].m_Deaths++;
                    Debug.Log($"Tanque {m_Tanks[i].m_PlayerNumber} ha muerto. Total: {m_Tanks[i].m_Deaths}");
                }

                // Actualizamos el estado para la siguiente vuelta
                m_PreviousTankStates[i] = isAlive;
            }
            // ... vuelvo al frame siguiente.
            yield return null;
        }
        
        
    }
    private IEnumerator RoundEnding()
    {
        // Deshabilito el movimiento de los tanques.
        DisableTankControl();
        // Borro al ganador de la ronda anterior.
        m_RoundWinner = null;
        // Miro si hay un ganador de la ronda.
        m_RoundWinner = GetRoundWinner();
        // Si lo hay, incremento su puntuación.
        if (m_RoundWinner != null)
            m_RoundWinner.m_Wins++;
        // Compruebo si alguien ha ganado el juego.
        m_GameWinner = GetGameWinner();
        // Genero el mensaje según si hay un gaandor del juego o no.
        string message = EndMessage();
        m_MessageText.text = message;
        // Espero a que pase el tiempo de espera antes de volver al bucle.
        yield return m_EndWait;
    }
    // Usado para comprobar si queda más de un tanque.
    private bool OneTankLeft()
    {
        // Contador de tanques.
        int numTanksLeft = 0;
        // recorro los tanques...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... si está activo, incremento el contador.
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }
        // Devuelvo true si queda 1 o menos, false si queda más de uno.
        return numTanksLeft <= 1;
    }
    // Comprueba si algún tanque ha ganado la ronda (si queda un tanque o menos).
    private TankManager GetRoundWinner()
    {
        // Recorro los tanques...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... si solo queda uno, es el ganador y lo devuelvo.
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }
        // SI no hay ninguno activo es un empate, así que devuelvo null.
        return null;
    }
    // Comprueba si hay algún ganador del juegoe.
    private TankManager GetGameWinner()
    {
        // Recorro los tanques...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... si alguno tiene las rondas necesarias, ha ganado y lo devuelvo.
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }
        // Si no, devuelvo null.
        return null;
    }
    // Deveulve el texto del mensaje a mostrar al final de cada ronda.
    private string EndMessage()
    {
        // Pordefecto no hya ganadores, así que es empate.
        string message = "EMPATE!";
        // Si hay un ganador de ronda cambio el mensaje.
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";
        // Retornos de carro.
        message += "\n\n\n\n";
        // Recorro los tanques y añado sus puntuaciones.
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " GANA\n";
        }
        // Si hay un ganador del juego, cambio el mensaje entero para reflejarlo.
        if (m_GameWinner != null)
        {
            m_GameEnded = true;
            int minutes = Mathf.FloorToInt(m_ElapsedTime / 60f);
            int seconds = Mathf.FloorToInt(m_ElapsedTime % 60f);
            message = $"{m_GameWinner.m_ColoredPlayerText} GANA EL JUEGO!\n\n" +
                      $"PUNTUACIÓN: {m_GameWinner.m_Wins}\n" +
                      $"TIEMPO TOTAL: {minutes:00}:{seconds:00}";
        }

        return message;
    }
    // Para resetear los tanques (propiedaes, posiciones, etc.).
    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }
    //Habilita el control del tanque
    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }
    //Deshabilita el control del tanque
    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
    private void Update()
    {
        if (m_GameEnded)
        {
            return; // No seguir si el juego terminó
        }

        m_ElapsedTime += Time.deltaTime;

        float timeLeft = Mathf.Max(m_MaxGameTime - m_ElapsedTime, 0f);
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        m_TimerText.text = $"TIEMPO: {minutes:00}:{seconds:00}";

        // Definimos colores intermedios
        Color green = Color.green;
        Color yellow = Color.yellow;
        Color orange = new Color(1f, 0.5f, 0f);
        Color red = Color.red;

        float t = m_ElapsedTime / m_MaxGameTime;

        // 3 etapas de cambio de color
        if (t < 0.33f)
        {
            // Verde a amarillo
            float lerpValue = t / 0.33f;
            m_TimerText.color = Color.Lerp(green, yellow, lerpValue);
        }
        else if (t < 0.66f)
        {
            // Amarillo a naranja
            float lerpValue = (t - 0.33f) / 0.33f;
            m_TimerText.color = Color.Lerp(yellow, orange, lerpValue);
        }
        else
        {
            // Naranja a rojo
            float lerpValue = (t - 0.66f) / 0.34f; // Lo que queda hasta 1.0
            m_TimerText.color = Color.Lerp(orange, red, lerpValue);
        }
    
    }

}
