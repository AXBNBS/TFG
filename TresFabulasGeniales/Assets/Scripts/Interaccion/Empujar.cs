﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Empujar : MonoBehaviour
{
    public bool input, cercano;

    private bool agarrado, ejeX;
    private LayerMask movilCap;
    private CharacterController characterCtr;
    private float longitudRay, offsetY;
    private MovimientoHistoria2 movimientoScr;
    private ObjetoMovil empujado;
    private RaycastHit rayoDat;
    private Animator animador;
    private Collider objetoMovTrg;


    // Inicialización de variables.
    private void Start ()
    {
        cercano = false;
        agarrado = false;
        movilCap = LayerMask.GetMask ("Movil");
        characterCtr = this.GetComponent<CharacterController> ();
        longitudRay = this.transform.localScale.x * characterCtr.radius * 3;
        offsetY = characterCtr.height / 2;
        movimientoScr = this.GetComponent<MovimientoHistoria2> ();
        animador = this.transform.GetChild(6).GetComponent<Animator> ();
    }


    // Si no hay un objeto agarrado, comprobamos si se cumplen las condiciones que lo permiten; en caso contrario, miramos si hemos soltado el botón de la interacción o el objeto empieza a caer para soltarlo. Animamos también al personaje en 
    //función de si está cogiendo un objeto o no.
    private void Update ()
    {
        if (agarrado == false)
        {
            EmpujePermitido ();
        }
        else
        {
            if (empujado.caer == true || empujado.bloqueado == true || Input.GetButtonUp ("Interacción") == true)
            {
                agarrado = false;

                movimientoScr.PararEmpuje ();
            }
        }
        Animar ();
    }


    // Nos aseguramos de sólo permitir que se empuje al estar cerca de un objeto con su respectivo trigger que lo active.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Empujable") == true)
        {
            cercano = true;
            objetoMovTrg = other;
            empujado = other.GetComponent<ObjetoMovil> ();
        }
    }


    // Al salir del trigger desactivamos el booleano que permite que cojamos el objeto.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Empujable") == true)
        {
            cercano = false;
            empujado = null;
        }
    }


    // Pal debug.
    /*private void OnDrawGizmos ()
    {
        Gizmos.DrawLine (this.transform.position, this.transform.position - this.transform.right * longitudRay);
    }*/


    // Si el imput está permitido, estamos cerca del objeto, nuestro personaje está en el suelo, se está pulsando el botón de interacción y el objeto está delante del personaje a poca distancia, rotamos al personaje para que esté perfectamente alineado
    //con él y a una distancia correcta para evitar que el personaje atraviese el objeto o se quede demasiado lejos del mismo, también ofrecemos los datos necesarios sobre el objeto al personaje para realizar el empuje.
    private void EmpujePermitido ()
    {
        Vector3 puntoIni = new Vector3 (this.transform.position.x, this.transform.position.y + offsetY, this.transform.position.z);

        if (input == true && cercano == true && movimientoScr.sueleado == true && empujado.bloqueado == false && Input.GetButton ("Interacción") == true && Physics.Raycast (puntoIni, -this.transform.right, out rayoDat, longitudRay, movilCap, 
            QueryTriggerInteraction.Ignore) == true)
        {
            Vector3 diferencia;

            ejeX = empujado.EjeDeTrigger (objetoMovTrg);
            diferencia = rayoDat.point - puntoIni;
            diferencia.y = 0;
            print ("Jugador: " + puntoIni + ". Punto del rayo: " + rayoDat.point + ". Diferencia: " + diferencia + ".");
            if (ejeX == true)
            {
                this.transform.rotation = Quaternion.Euler (0, Vector3.Angle (diferencia.x > 0 ? this.transform.right : -this.transform.right, diferencia), 0);
            }
            else 
            {
                this.transform.rotation = Quaternion.Euler (0, diferencia.z > 0 ? Vector3.Angle (-this.transform.right, diferencia) + 90 : Vector3.Angle (-this.transform.right, diferencia) - 90, 0);
            }
            agarrado = true;

            if (CambioDePersonajesYAgrupacion.instancia.juntos == true) 
            {
                CambioDePersonajesYAgrupacion.instancia.Separar ();
            }
            movimientoScr.ComenzarEmpuje (ejeX, empujado);
        }
    }


    // Se anima al personaje en función de si está empujando un objeto o no.
    private void Animar ()
    {
        animador.SetBool ("cogiendo", agarrado);
    }
}