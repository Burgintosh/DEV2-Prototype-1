using UnityEngine;
using System.Collections;

public class laser : MonoBehaviour
{
    [SerializeField] LineRenderer laserLine;

    [SerializeField] GameObject hitEffect;
    [SerializeField] Transform laserStartPos;

    [SerializeField] int laserMaxDist;
    [SerializeField] int damage;
    [SerializeField] float damageRate;

    bool isDamaging;

    // Update is called once per frame
    void Update()
    {
        createLaser();
    }

    void createLaser()
    {
        RaycastHit hit;
        if (Physics.Raycast(laserStartPos.position, laserStartPos.forward, out hit, laserMaxDist))
        {
            laserLine.SetPosition(0, laserStartPos.position);
            laserLine.SetPosition(1, hit.point); // Can use multiple indexes to have laser look like lightning (zigzagging)

            hitEffect.SetActive(true);
            hitEffect.transform.position = hit.point;

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null && isDamaging == false)
            {
                StartCoroutine(damageTime(dmg));
            }
        }
        else
        {
            laserLine.SetPosition(0, laserStartPos.position);
            laserLine.SetPosition(1, laserStartPos.position + laserStartPos.forward * laserMaxDist);

            hitEffect.SetActive(false);
        }
    }

    IEnumerator damageTime(IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damage);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }
}
