using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : GrabThrow
{
    public GameObject collectible;
    public int numCollectibles;

    private GameObject[] collectibles;

    private void Awake()
    {
        collectibles = new GameObject[numCollectibles];

        for (int i = 0; i < numCollectibles; i++)
        {
            collectibles[i] = Instantiate(collectible, transform.position, collectible.transform.rotation);
            collectibles[i].GetComponent<OdinPart>().InstantiateOdinPart();
            collectibles[i].gameObject.SetActive(false);

        }
    }




    private void OnCollisionEnter(Collision collision)
    {
        if (grabbed)
        {
            GetComponent<BoxCollider>().enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
            StartCoroutine(DestroyObjects(0.125f, 0.75f));
            StartCoroutine(ReleaseCollectibles(0.085f));
        }

        if (!grabbed && collision.gameObject.layer == 9 && collision.gameObject.GetComponent<GrabThrow>().grabbed)
        {
            GetComponent<BoxCollider>().enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
            StartCoroutine(DestroyObjects(0.125f, 0.75f));
            StartCoroutine(ReleaseCollectibles(0.085f));
        }
    }

    public void GetHitByAxe()
    {
        GetComponent<BoxCollider>().enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(true);
        StartCoroutine(DestroyObjects(0.125f, 0.75f));
        StartCoroutine(ReleaseCollectibles(0.085f));
    }

    IEnumerator ReleaseCollectibles(float timeBetween)
    {
        for (int i = 0; i < numCollectibles; i++)
        {
            collectibles[i].transform.position = transform.position;
            collectibles[i].transform.parent = null;
            collectibles[i].GetComponent<OdinPart>().SetMoveToOdin(true);
        }

        for (int i = 0; i < numCollectibles; i++)
        {
            yield return new WaitForSeconds(timeBetween);
            
            collectibles[i].gameObject.SetActive(true);
        }
    }

    IEnumerator DestroyObjects(float time, float time01)
    {
        yield return new WaitForSeconds(time);

        transform.GetChild(0).gameObject.SetActive(false);

        yield return new WaitForSeconds(time01);

        for (int i = 0; i < numCollectibles; i++)
        {
            collectibles[i].gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}
