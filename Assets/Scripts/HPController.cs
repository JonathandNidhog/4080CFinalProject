using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class HPController : MonoBehaviour
{
    // Start is called before the first frame update


    public float health = 5;
    public float maxhealth = 5;
    public Slider Barfill;


    void Start()
    {
        health = maxhealth;
        if(Barfill)
        {
            Barfill.value = health / maxhealth;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        //BarFill();
    }


    //private void BarFill()
   // {
    //    Bar.fillAmount = health/maxhealth;  
   // }

    private void AddHealth()
    {
        health += 10;


    }
    private void DamageHealth() 
    { health -= 10;}


    public void ShowHp(int hp) {

        if (Barfill)
        {
            Barfill.value = hp / maxhealth;
        }

    }




}
