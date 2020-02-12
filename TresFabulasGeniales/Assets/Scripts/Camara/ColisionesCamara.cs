﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ColisionesCamara : MonoBehaviour
{
    [SerializeField] private int minimoDst, maximoDst, suavizado;
    [SerializeField] private LayerMask capas, capasSinAvt;
    private float distancia;
    private Vector3 direccion, posicionUltFrm;
    private RaycastHit raycastDat;


    // Inicialización de variables.
    private void Start ()
    {
        direccion = this.transform.localPosition.normalized;
        distancia = this.transform.localPosition.magnitude;
        posicionUltFrm = this.transform.position;
        //movidoUltFrm = false;
    }


    // Si existe algún obstáculo entre la posición de la cámara y el avatar seguido, hacemos que esta se acerque al mismo.
    private void Update ()
    {
        if (this.transform.position != posicionUltFrm) 
        {
            if (Physics.Raycast (this.transform.parent.position, this.transform.position - this.transform.parent.position, out raycastDat, maximoDst, CambioDePersonajesYAgrupacion.instancia.juntos == false ? capas : capasSinAvt,
                QueryTriggerInteraction.Ignore) == true)
            {
                distancia = Mathf.Clamp (raycastDat.distance * 0.7f, minimoDst, maximoDst);
            }
            else
            {
                distancia = maximoDst;
            }

            /*Vector3 posicionObj = distancia * (direccion - extra);

            if (Vector3.Distance (posicionObj, this.transform.parent.position) < minimoDst) 
            {
                posicionObj = minimoDst * direccion;
            }*/
            this.transform.localPosition = Vector3.Lerp (this.transform.localPosition, distancia * direccion, Time.deltaTime * suavizado);
        }
        posicionUltFrm = this.transform.position;
    }


    // Pal debug.
    /*private void OnDrawGizmos ()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere (this.transform.position, 0.5f);
    }*/
}