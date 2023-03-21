using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    ParticleSystem myParticleSystem;
    public Color color;
    private Renderer parent_renderer;
    // Start is called before the first frame update
    void Start()
    {
        myParticleSystem = GetComponent<ParticleSystem>();
        // Create a new cube primitive to set the color on
        // GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Get the Renderer component from the new cube
        var Renderer = myParticleSystem.GetComponent<Renderer>();
        parent_renderer = GetComponentInParent<Renderer>();
        Color ParentColor = parent_renderer.material.color;
        // Call SetColor using the shader property name "_Color" and setting the color to red
        Renderer.material.SetColor("_Color", color);
    }


}
