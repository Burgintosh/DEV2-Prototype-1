using UnityEngine;

public class rotateObject : MonoBehaviour
{
    [SerializeField] Transform model;
    [Range(50, 500)][SerializeField] int rotateSpeed;

    //enum rotateDir { Left, Right, Up }

    // Update is called once per frame
    void Update()
    {
        //model.transform.RotateAround(model.position, Vector3.forward, Time.deltaTime * rotateSpeed);
        model.transform.Rotate(Vector3.up, Time.deltaTime * rotateSpeed, Space.Self);
    }
}
