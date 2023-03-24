using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
// Adds simple networking to the 3d pen. The approach used is to draw locally
// when a remote user tells us they are drawing, and stop drawing locally when
// a remote user tells us they are not.
public class SprayCan : MonoBehaviour, IGraspable, IUseable
{
    private NetworkContext context;
    private bool owner;
    private Hand controller;
    private Material drawingMaterial;
    private GameObject currentDrawing;
    private bool painting;

    public Camera canvasCam, sceneCamera;
    public Sprite cursorPaint;
    public GameObject brushContainer;

    private Collider my_collider;

    Color brushColor;
    // public RenderTexture canvasTexture; // Render Texture that looks at our Base Texture and the painted brushes
    // Amend message to also store current drawing state
    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isDrawing; // new
        
        public Message(Transform transform, bool isDrawing)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.isDrawing = isDrawing; // new
        }
    }

    private void Start()
    {
        context = NetworkScene.Register(this);
        var shader = Shader.Find("Particles/Standard Unlit");
        drawingMaterial = new Material(shader);
        brushColor = Color.blue;

        my_collider = GetComponent<Collider>();
        
    }

    public void ProcessMessage (ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;


        
		// if (Input.GetMouseButton(0)) {
		// 	BeginDrawing();
		// }
        // new
        // Also start drawing locally when a remote user starts
        if (data.isDrawing && !currentDrawing)
        {
            BeginDrawing();
        }
        if (!data.isDrawing && currentDrawing)
        {
            
            EndDrawing();
        }
    }

    private void FixedUpdate()
    {
        if (owner)
        {
            // new
            context.SendJson(new Message(transform,isDrawing:currentDrawing));
            
            my_collider.isTrigger = true;
        }
        else{my_collider.isTrigger = false;}
    }

    private void LateUpdate()
    {
        if (controller)
        {
            transform.position = controller.transform.position;
            transform.rotation = controller.transform.rotation;
        }
    }

    void IGraspable.Grasp(Hand controller)
    {
        owner = true;
        this.controller = controller;
        GetComponent<Rigidbody>().useGravity = false;
    }

    void IGraspable.Release(Hand controller)
    {
        owner = false;
        this.controller = null;
        GetComponent<Rigidbody>().useGravity = true;
    }

    void IUseable.Use(Hand controller)
    {
        painting = true;
        StartCoroutine(waiter_drawing());
    }
    
    IEnumerator waiter_drawing()
    {
        while (painting)
        {
            BeginDrawing();
            yield return new WaitForSeconds(0.001f);
        }
    }

    void IUseable.UnUse(Hand controller)
    {
        print("?????????????");
        EndDrawing();
    }

    private void BeginDrawing()
    {
        brushColor = ColorSelector.GetColor ();
        Vector3 uvWorldPosition = Vector3.zero;	
        Vector3 hitPoint = Vector3.zero;	
        
        // Debug.Log("begin drawing outside");	
		if(HitTestUVPosition(ref uvWorldPosition, ref hitPoint)){
			// GameObject brushObj;
            // Debug.Log("begin drawing");
            currentDrawing=(GameObject)Instantiate(Resources.Load("TexturePainter-Instances/BrushEntity")); //Paint a brush
            
            float randomZ = Random.Range(0f, 360f);
            // Create a Quaternion representing the random rotation around the z-axis
            // Quaternion randomRotation = Quaternion.Euler(0, 0, randomZ);
            // Apply the random rotation to the GameObject
            // currentDrawing.GetComponent<RectTransform>().transform.Rotate(0f, 0f,randomZ);
            GameObject nozzle = GameObject.Find("Cylinder");
            float brushSize = 0.001f * Vector3.Distance(hitPoint, nozzle.GetComponent<Transform>().position);

            currentDrawing.GetComponent<SpriteRenderer>().color=brushColor; //Set the brush color
			brushColor.a=1.0f; // Brushes have alpha to have a merging effect when painted over.
			currentDrawing.transform.parent=brushContainer.transform; //Add the brush to our container to be wiped later
			currentDrawing.transform.localPosition=uvWorldPosition; //The position of the brush (in the UVMap)
            currentDrawing.transform.localScale=Vector3.one*brushSize;//The size of the brush
		}
    }
    
    //Returns the position on the texuremap according to a hit in the mesh collider
    bool HitTestUVPosition(ref Vector3 uvWorldPosition, ref Vector3 hitPoint){
		RaycastHit hit;
        GameObject nozzle = GameObject.Find("Cylinder");
		Vector3 cursorDir = nozzle.transform.forward;
		// Ray cursorRay=sceneCamera.ScreenPointToRay (cursorPos);
		Ray cursorRay= new Ray(nozzle.GetComponent<Transform>().position, cursorDir);
		if (Physics.Raycast(cursorRay,out hit,3)){
            hitPoint = hit.point;
            // Debug.Log("Inside hitTest");
			MeshCollider meshCollider = hit.collider as MeshCollider;
			if (meshCollider == null || meshCollider.sharedMesh == null)
				return false;			
			Vector2 pixelUV  = new Vector2(hit.textureCoord.x,hit.textureCoord.y);
			uvWorldPosition.x=pixelUV.x-canvasCam.orthographicSize;//To center the UV on X
			uvWorldPosition.y=pixelUV.y-canvasCam.orthographicSize;//To center the UV on Y
			uvWorldPosition.z=0.0f;
			return true;
		}
		else{		
			return false;
		}
	}


    private void EndDrawing()
    {
        // Debug.Log(currentDrawing);
        currentDrawing.transform.parent = null;
        // currentDrawing.GetComponent<TrailRenderer>().emitting = false;
        currentDrawing = null;
        painting = false;
    }
}