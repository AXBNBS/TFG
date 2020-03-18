﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Enemigo : MonoBehaviour
{
    public Transform avatarTrf;
    [HideInInspector] public int indice;

    [SerializeField] private float puntosGol, rangoAtq;
    [SerializeField] private int saltoDef, aranyazoDef, aleatoriedad, velocidadMov, velocidadRot, distanciaMaxObj;
    private LayerMask jugadorCap;
    private bool perseguir, reposicionado, objetivo1, alcanzadoObj, parado, atacado;
    private bool cercanoAvt;
    private Vector3 posicionIni, destinoRnd;
    private NavMeshAgent agente;
    private CharacterController personajeCtr;
    [SerializeField] private Transform objetivoTrf, ataqueCenTrf;
    private Vector3 offsetObj;
    private List<Transform> companyerosCer;
    private AreaEnemiga zona;
    private int indicePnt;
    private Animator animador;
    private GameObject rebotador;
    private float cooldown;

    
    // Inicialización de variables.
    private void Start ()
    {
        jugadorCap = LayerMask.GetMask (new string[] { "Violeta", "Abedul" });
        perseguir = false;
        reposicionado = false;
        alcanzadoObj = false;
        parado = false;
        atacado = false;
        posicionIni = this.transform.position;
        agente = this.GetComponent<NavMeshAgent> ();
        agente.updatePosition = false;
        agente.updateRotation = false;
        agente.destination = posicionIni;
        personajeCtr = this.GetComponent<CharacterController> ();
        objetivoTrf = null;
        ataqueCenTrf = this.transform.GetChild (1);
        companyerosCer = new List<Transform> ();
        zona = this.transform.parent.GetComponent<AreaEnemiga> ();
        animador = this.GetComponentInChildren<Animator> ();
        rebotador = this.transform.GetChild(0).gameObject;
    }


    // Si hay que perseguir a alguien y el enemigo está activo, nos dirigimos directamente al objetivo en caso de que este no esté por encima de nosotros; de lo contrario, el enemigo se mueve de forma aleatoria para tratar de evitar que el jugador
    //caiga sobre él y le haga daño, la nueva posición se definirá cada cierto tiempo.
    private void Update ()
    {
        alcanzadoObj = Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (agente.destination.x, agente.destination.z)) < distanciaMaxObj;
        cooldown -= Time.deltaTime;
        if (perseguir == true) 
        {
            StopAllCoroutines ();
            if (avatarTrf.name == "Abedul" || this.transform.position.y >= objetivoTrf.position.y) 
            {
                MoverAgenteYControlador (objetivoTrf.position + offsetObj, true, true);
                if (this.IsInvoking () == true) 
                {
                    this.CancelInvoke ("Reposicionado");
                }
                MirarSiAtaco ();
            }
            else 
            {
                MoverAgenteYControlador (destinoRnd);

                if (reposicionado == false)
                {
                    if (this.IsInvoking () == false)
                    {
                        destinoRnd = new Vector3 (avatarTrf.position.x + Random.Range (-aleatoriedad, +aleatoriedad), this.transform.position.y, avatarTrf.position.z + Random.Range (-aleatoriedad, +aleatoriedad));

                        this.Invoke ("Reposicionado", Random.Range (1f, 2f));
                    }
                }
                else
                {
                    reposicionado = false;
                }
            }
        }
        else 
        {
            if (alcanzadoObj == false && puntosGol >= 0)
            {
                StartCoroutine ("VolverAPosicionInicial");
            }
        }

        Animar ();
    }


    // Si el enemigo ha muerto, lo acercamos lentamente al suelo para que cuadre con la animación de muerte.
    private void FixedUpdate ()
    {
        if (puntosGol < 0) 
        {
            this.transform.Translate (new Vector3 (0, -0.4f, 0));
        }
    }


    // Puede servir para ver que hacer cuando no podemos llegar al punto.
    /*private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        print (this.name + ": " + hit.transform.name);
    }*/


    // Si el enemigo entra en la esfera que cubre al jugador que es su blanco, consideramos que se encuentra cerca del mismo.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true && other.transform == avatarTrf) 
        {
            cercanoAvt = true;
            cooldown = 1.25f;
        }
    }


    // Si el enemigo sale de la esfera que cubre al jugador que es us blanco, consideramos que se encuentra lejos del mismo.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true && other.transform == avatarTrf)
        {
            cercanoAvt = false;
        }
    }


    // Para marcar el objetivo de los ranoncios.
    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.red;

        if (agente != null) 
        {
            Gizmos.DrawWireSphere (agente.destination, 1);
        }
        if (ataqueCenTrf != null) 
        {
            Gizmos.DrawWireSphere (ataqueCenTrf.position, rangoAtq);
        }
    }


    // Se indica al enemigo que tiene que perseguir/atacar a alguien.
    public void AtacarA (Transform jugador, bool uno) 
    {
        int indiceArr;
        float distanciaChc;

        float distanciaMin = float.MaxValue;

        if (indicePnt != -1) 
        {
            zona.tomadosPnt[indicePnt] = false;
            indicePnt = -1;
        }
        perseguir = true;
        avatarTrf = jugador;
        objetivoTrf = jugador.GetChild (3);
        offsetObj = Vector3.zero;
        objetivo1 = uno;
        for (int p = 0; p < objetivoTrf.childCount; p += 1)
        {
            indiceArr = objetivo1 == false ? p : p + 4;
            distanciaChc = Vector3.Distance (objetivoTrf.GetChild(p).position, this.transform.position);
            if (zona.tomadosPnt[indiceArr] == false && distanciaChc < distanciaMin)
            {
                distanciaMin = distanciaChc;
                offsetObj = objetivoTrf.GetChild(p).position - objetivoTrf.position;
                indicePnt = indiceArr;
            }
        }
        if (indicePnt != -1)
        {
            zona.tomadosPnt[indicePnt] = true;
            print (objetivoTrf.parent.name);
        }
    }


    // El enemigo deja de perseguir a su objetivo y vuelve a su posición inicial, en caso de seguir activo.
    public void Parar () 
    {
        perseguir = false;
        //objetivoTrf = null;
        parado = true;
        agente.destination = posicionIni;

        this.CancelInvoke ("Reposicionando");
    }


    // Se recibe el valor del daño recibido, y si es un salto o no, para tener en cuenta el valor de defensa del enemigo según el tipo de ataque. Además desactivaremos al enemigo en caso de que su salud tras el golpe sea menor de 0.
    public void Danyar (float danyo, bool salto) 
    {
        puntosGol -= salto  == true ? danyo / saltoDef : danyo / aranyazoDef;
    }


    // Devuelve true o false dependiendo de si la salud del enemigo está por debajo de 0, y también lo desactiva si es el caso.
    public bool ChecarDerrotado () 
    {
        if (puntosGol >= 0) 
        {
            return false;
        }
        else
        {
            if (indicePnt != -1) 
            {
                zona.tomadosPnt[indicePnt] = false;
            }
            posicionIni = this.transform.position;
            perseguir = false;
            personajeCtr.enabled = false;

            rebotador.SetActive (false);
            this.Invoke ("Desaparecer", 0.7f);

            return true;
        }
    }


    // Simplemente devuelve true si el enemigo tiene su salud por debajo de 0.
    public bool Vencido () 
    {
        return (puntosGol < 0);
    }


    // Tras cierto tiempo, marcamos como verdadero que el enemigo se ha reposicionado para poder hacerlo de nuevo.
    private void Reposicionado () 
    {
        reposicionado = true;
    }


    // Si el nuevo objetivo es distinto al anteriormente asignado y no estamos cerca del jugador.
    private void MoverAgenteYControlador (Vector3 objetivo, bool mirarObj = false, bool importaCer = false)
    {
        objetivo.y = this.transform.position.y;

        if ((importaCer == false || cercanoAvt == false) && agente.destination != objetivo) 
        {
            agente.SetDestination (objetivo);
        }

        if (alcanzadoObj == false)
        {
            personajeCtr.Move (agente.desiredVelocity.normalized * Time.deltaTime * velocidadMov);

            agente.velocity = personajeCtr.velocity;
            this.transform.position = new Vector3 (this.transform.position.x, posicionIni.y, this.transform.position.z);
            if (agente.velocity != Vector3.zero)
            {
                this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler (this.transform.rotation.eulerAngles.x, Mathf.Atan2 (personajeCtr.velocity.x, personajeCtr.velocity.z) * Mathf.Rad2Deg, 
                    this.transform.rotation.eulerAngles.z), Time.deltaTime * velocidadRot);
            }
            parado = false;
        }
        else 
        {
            if (mirarObj == true) 
            {
                this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler (this.transform.rotation.eulerAngles.x, Quaternion.LookRotation(this.transform.position - avatarTrf.position).eulerAngles.y + 180, 
                    this.transform.rotation.eulerAngles.z), Time.deltaTime * velocidadRot);
            }
        }

        agente.nextPosition = this.transform.position;
    }


    // Si la salud del enemigo está por debajo de 0, hacemos que reproduzca su animación de muerte, en caso contrario, hacemos que reproduzca su animación de correr o estar parado según si se está desplazando o no.
    private void Animar () 
    {
        if (puntosGol < 0) 
        {
            animador.SetTrigger ("derrotado");
        }
        else 
        {
            animador.SetBool ("moviendose", alcanzadoObj == false && parado == false);
            if (cercanoAvt == true && cooldown < 0 && this.transform.position.y >= objetivoTrf.position.y) 
            {
                cooldown = 1.25f;

                animador.SetTrigger ("atacado");
            }
        }
    }


    // Tras completar su animación de muerte, desactivamos el objeto que representa al enemigo.
    private void Desaparecer () 
    {
        if (avatarTrf.name == "Abedul" || this.transform.position.y >= objetivoTrf.position.y)
        {
            zona.APor (avatarTrf, objetivo1);
        }
        this.gameObject.SetActive (false);

        zona.vivos -= 1;
    }


    // .
    private void MirarSiAtaco () 
    {
        if (animador.GetCurrentAnimatorStateInfo(0).IsTag ("Ataque") == true)
        {
            if (atacado == false && animador.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.4f)
            {
                Collider[] avataresCol = Physics.OverlapSphere (ataqueCenTrf.position, rangoAtq, jugadorCap, QueryTriggerInteraction.Ignore);

                foreach (Collider a in avataresCol)
                {
                    a.GetComponent<Salud>().RecibirDanyo ();
                }

                atacado = true;
            }
        }
        else 
        {
            atacado = false;
        }
    }


    // Esta corutina se llamará repetidamente cuando no haya avatares en la zona y hará que tras 2 segundos los enemigos se acerquen, frame tras frame, a su posición inicial, una vez se llegue a la misma descartaremos todas las llamadas que haya en
    //curso relativas a esta misma corutina.
    private IEnumerator VolverAPosicionInicial () 
    {
        yield return new WaitForSeconds (2);

        MoverAgenteYControlador (posicionIni);
        if (alcanzadoObj == true) 
        {
            this.StopAllCoroutines ();
        }
    }
}