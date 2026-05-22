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

    [Header("----- Laser Audio -----")]
    [SerializeField] AudioSource laserAudioSrc;
    [SerializeField][Range(0f, 1f)] float laserVolScale = 1f;
    [SerializeField] SoundCategory laserSoundCategory = SoundCategory.Master;

    bool isDamaging;

    private void Start()
    {
        if (laserAudioSrc == null)
        {
            laserAudioSrc = GetComponent<AudioSource>();
        }

        if(laserAudioSrc != null)
        {
            laserAudioSrc.loop = true;
            laserAudioSrc.playOnAwake = false;

            laserAudioSrc.Play();
        }
        
        StartCoroutine(UpdateLaserAudioVolRoutine());
    }

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

    IEnumerator UpdateLaserAudioVolRoutine()
    {
        while(true)
        {
            if(laserAudioSrc != null)
            {
                if(SoundManager.Instance != null)
                {
                    laserAudioSrc.volume = SoundManager.Instance.GetFinalVol(laserVolScale, laserSoundCategory);
                }
                else
                {
                    laserAudioSrc.volume = laserVolScale;
                }

                if (!laserAudioSrc.isPlaying)
                {
                    laserAudioSrc.Play();
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator damageTime(IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damage);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }

    private void OnDisable()
    {
        if(laserAudioSrc != null)
        {
            laserAudioSrc.Stop();
        }
    }
}
