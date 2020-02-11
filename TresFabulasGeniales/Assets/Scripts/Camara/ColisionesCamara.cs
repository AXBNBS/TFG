﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ColisionesCamara : MonoBehaviour
{
    [SerializeField] private int minimoDst, maximoDst, suavizado;
    [SerializeField] private LayerMask capas;
    private float distancia;
    private Vector3 direccion;
    private RaycastHit raycastDat;


    // Inicialización de variables.
    private void Start ()
    {
        direccion = this.transform.localPosition.normalized;
        distancia = this.transform.localPosition.magnitude;
    }


    // Si existe algún obstáculo entre la posición de la cámara y el avatar seguido, hacemos que esta se acerque al mismo.
    private void Update ()
    {
        if (Physics.Raycast (this.transform.parent.position, this.transform.position - this.transform.parent.position, out raycastDat, maximoDst, capas, QueryTriggerInteraction.Ignore) == true)
        {
            distancia = Mathf.Clamp (raycastDat.distance * 0.7f, minimoDst, maximoDst);
        }
        else 
        {
            distancia = maximoDst;
        }
        this.transform.localPosition = Vector3.Lerp (this.transform.localPosition, direccion * distancia, Time.deltaTime * suavizado);
    }
}