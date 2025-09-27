using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float Speed;
    public float AlturaSalto;
    public float PotenciaSalto;
    public float Gravedad;
    public int Fase1;
    public int Fase2;
    public bool Saltando;
    public float Fallen;
    public Animator ani;
    private float YPos;
    private int sky_;

    ///////////////////Detector de piso///////////////////////

    private RaycastHit2D hit;
    public Vector3 v3;
    public float distance;
    public LayerMask layer;

    /////////////////// JumpWall ///////////////////////

    // Start is called before the first frame update



    void Start()
    {
        ani = GetComponent<Animator>();
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position + v3, Vector3.up * -1 * distance);
    }
    public bool CheckCollision
    {
        get
        {
            hit = Physics2D.Raycast(transform.position + v3, transform.up * -1, distance, layer);
            return hit.collider != null;
        }
    }

    public void Detector_Plataforma()
    {
        if (CheckCollision)///////******
        {
            ani.SetBool("Sky", false);
            sky_ = 0;

            if (!Saltando)
            {
                Gravedad = 0;
                Fase1 = 0;
                Fase2 = 0;
            }
        }
        else
        {
            ani.SetBool("Sky", true);
            if (!Saltando)
            {
                switch (Fase2)
                {
                    case 0:
                        Gravedad = 0;
                        Fase2 = 1;
                        break;
                    case 1:
                        if (Gravedad > -10)
                        {
                            Gravedad -= AlturaSalto / Fallen * Time.deltaTime;
                        }
                        break;
                }
            }
        }

        if (transform.position.y > YPos)
        {
            ani.SetFloat("Gravedad", 1);
        }
        if (transform.position.y < YPos)
        {
            ani.SetFloat("Gravedad", 0);

            switch (sky_)
            {
                case 0:
                    ani.Play("Base Layer.Sky", 0, 0);
                    sky_++;
                    break;
            }
        }

        YPos = transform.position.y;
    }
    public void Jump()
    {
        if (Input.GetKey(KeyCode.O))
        {
            switch (Fase1)
            {
                case 0:

                    if (CheckCollision)
                    {
                        Gravedad = AlturaSalto;
                        Fase1 = 1;
                        Saltando = true;
                    }

                    break;
                case 1:

                    if (Gravedad > 0)
                    {
                        Gravedad -= PotenciaSalto * Time.deltaTime;
                    }
                    else
                    {
                        Fase1 = 2;
                    }
                    Saltando = true;

                    break;
                case 2:
                    Saltando = false;
                    break;
            }
        }
        else
        {
            Saltando = false;
        }
    }
    public void Move()
    {
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Speed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, 0);
            ani.SetBool("Run", true);
        }
        else
        {
            ani.SetBool("Run", false);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.right * Speed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 180, 0);
            ani.SetBool("Run", true);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
        Jump();
    }
    void Update()
    {
        Detector_Plataforma();
        transform.Translate(Vector3.up * Gravedad * Time.deltaTime);
    }
}