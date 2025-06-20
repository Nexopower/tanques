using System;
using UnityEngine;
[Serializable] //Hace que los atributos aparezcan en el inspector (si no los escondemos)
public class TankManager
{
    //Esta clase gestiona la configuración del tanque junto con el GameManager
    //gestiona el comportamiento de los tanques y si los jugadores tienen control sobre el tanque
    //en los distintos momentos del juego
    public Color m_PlayerColor; //Color para el tanque
    public Transform m_SpawnPoint; //Posición y direción en la que se generaráel tanque
    [HideInInspector] public int m_PlayerNumber; //Especifica con qué jugadorestá actuando el Game Manager
    [HideInInspector] public string m_ColoredPlayerText; //String que reprsenta el color del tanque
    [HideInInspector] public GameObject m_Instance; //Refernecia a la instancia del tanque cuando se crea
    [HideInInspector] public int m_Wins; //Número de victorias del jugador

    private TankMovement m_Movement; //Referencia al script de movimiento deltanque. Utilizado para deshabilitar y habilitar el control
    private TankShooting m_Shooting; //Referencia al script de disparo del tanque. Utilizado para deshabilitar y habilitar el control
    private GameObject m_CanvasGameObject; //Utilizado para deshabilitar el UIdel mundo durante als fases de inicio y fin de cada ronda
public void Setup()
{
    // Obtener referencias de los componentes
    m_Movement = m_Instance.GetComponent<TankMovement>();
    m_Shooting = m_Instance.GetComponent<TankShooting>();
    m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

    // Asignar número de jugador
    m_Movement.m_PlayerNumber = m_PlayerNumber;
    m_Shooting.m_PlayerNumber = m_PlayerNumber;

    // Crear texto coloreado (ej: "<color=#FF0000>PLAYER 1</color>")
    m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

    // Cambiar solo el material "TankColor" en todas las piezas
    MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();
    foreach (MeshRenderer renderer in renderers)
    {
        // Buscar el material "TankColor" en los materiales asignados a este renderer
        Material[] materials = renderer.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            // Comparar sin "(Instance)" que Unity añade
            if (materials[i].name.StartsWith("TankColor"))
            {
                // Clonar el material para evitar afectar otros objetos
                Material newMaterial = new Material(materials[i]);
                newMaterial.color = m_PlayerColor;
                materials[i] = newMaterial;
                break; // Salir del bucle tras encontrar el material
            }
        }
        renderer.materials = materials; // Aplicar cambios
    }
}
    //Usado durante la fases del juego en las que el jugador no debe poder controlar el tanque
    public void DisableControl()
    {
        m_Movement.enabled = false;
        m_Shooting.enabled = false;
        m_CanvasGameObject.SetActive(false);
    }
    //Usado durante la fases del juego en las que el jugador debe poder controlar el tanque
    public void EnableControl()
    {
        m_Movement.enabled = true;
        m_Shooting.enabled = true;
        m_CanvasGameObject.SetActive(true);
    }
    //Usado al inicio de cada ronda para poner el tanque en su estado inicial
    public void Reset()
    {
        m_Instance.transform.position = m_SpawnPoint.position;
        m_Instance.transform.rotation = m_SpawnPoint.rotation;
        m_Instance.SetActive(false);
        m_Instance.SetActive(true);
    }
}
