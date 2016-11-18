using UnityEngine;
using System.Collections;

public class MakeDamage : MonoBehaviour {

    private double hp;
    private double maxHP = 1500;
    private bool isAlive = true;
    private GameObject parent;
    public GameObject hpBar;

	// Use this for initialization
	void Start () {
        hp = maxHP;
        parent = GameObject.Find("dragon");
	}
	
	// Update is called once per frame
	void Update () {
	    if(hp <= 0 && isAlive == true)
        {
            PlayDead();
            isAlive = false;
        }
	}

    void OnCollisionStay(Collision col)
    {
        if(col.gameObject.tag == "Projectile")
        {
            hp -= 10;

        }
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Projectile")
        {
            hp -= 10;
            hpBar.GetComponent<GUIBarScript>().SetNewValue(hp/maxHP);
            Debug.Log("Enter Collided with Projectile");
        }
    }

    private void PlayDead()
    {
        parent.GetComponent<Animator>().Play("dead");
        gameObject.GetComponent<AudioSource>().Play();
        StartCoroutine("DestroyUponDeath", 5);

    }

    private IEnumerator DestroyUponDeath(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(parent);
    }

}
