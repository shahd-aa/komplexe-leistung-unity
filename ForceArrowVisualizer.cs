using UnityEngine;

public class ForceArrowVisualizer : MonoBehaviour
{
    [Header("Settings")]
    public float scaleMultiplier = 0.5f;  // wie stark der pfeil wächst
    public float minLength = 0.2f;        // minimale länge
    public float maxLength = 5f;          // maximale länge
    
    [Header("Scaling Axis")]
    public bool scaleX = false;  // pfeil wächst in x-richtung?
    public bool scaleY = false;  // pfeil wächst in y-richtung?
    public bool scaleZ = true;   // pfeil wächst in z-richtung? (meistens ja)
    
    private Vector3 originalScale;
    private float currentForce = 0f;
    
    void Start()
    {
        // speichere die original-größe
        originalScale = transform.localScale;
    }
    
    // diese methode rufst du auf um die kraft zu setzen
    public void SetForce(float force)
    {
        currentForce = force;
        UpdateArrowScale();
    }
    
    // diese methode rufst du auf um die kraft zu setzen MIT richtung
    public void SetForce(Vector3 forceVector)
    {
        currentForce = forceVector.magnitude;
        
        // pfeil in richtung der kraft drehen
        if (forceVector != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(forceVector);
        }
        
        UpdateArrowScale();
    }
    
    void UpdateArrowScale()
    {
        // berechne die länge basierend auf der kraft
        float targetLength = currentForce * scaleMultiplier;
        targetLength = Mathf.Clamp(targetLength, minLength, maxLength);
        
        // erstelle neue scale
        Vector3 newScale = originalScale;
        
        // skaliere nur die achse(n) die du willst
        if (scaleX) newScale.x = targetLength;
        if (scaleY) newScale.y = targetLength;
        if (scaleZ) newScale.z = targetLength;
        
        transform.localScale = newScale;
    }
    
    // optional: zeige den pfeil nur wenn kraft > 0
    public void Show(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
