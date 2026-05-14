using UnityEngine;
using UnityEngine.UI;


public class MouseHover : MonoBehaviour
{
    
    Image buttonImage;
    Color colorOrig;
    [SerializeField] Color colorHover;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        buttonImage = GetComponent<Image>();
    }
    void Start()
    {
        //renderer.material.color = Color.black;
        colorOrig = buttonImage.color;
        
    }

    private void OnMouseEnter()
    {
        buttonImage.color = colorHover;
    }
    private void OnMouseExit()
    {
        buttonImage.color = colorOrig;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
