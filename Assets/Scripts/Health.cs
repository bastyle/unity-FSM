using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Health : MonoBehaviour
{
    private int health = 100;
    public bool IsAlive()
    {
        return health > 0;
    }
    public void TakeDamage(int damage)
    {
        if (health > 0)
        {
            health -= damage;
            health = health < 0 ? 0 : health; //clamp to 0
            if (health == 0)
            {
                this.gameObject.SetActive(false);
                print("PLAYER DIED!");
                //Time.timeScale = 0f;
                Destroy(this.gameObject);
            }
        }

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}