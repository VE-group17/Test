using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;

// The spray can class that handles object passing, making/recreating strokes, and sync movements.
public class SprayCan : MonoBehaviour, IGraspable, IUseable
{
    private NetworkContext context;
    private bool owner;
    private Hand controller;
    private GameObject currentDrawing;
    private GameObject background;
    private bool painting;
    // The list that keeps tracks of the details for all the strokes
    private List<(float, float, float, float, float, float, float, float)> brushList = new List<(float, float, float, float, float, float, float, float)>();
    private Color BrushColor;
    private bool released;
    private string player1;
    private string player2;
    private int myPlayerID;

    public Camera canvasCam, sceneCamera;
    public Sprite cursorPaint;
    public GameObject brushContainer;
    public FlexibleColorPicker fcp;
    public GameObject AvatarManager;
    public string myID;
    public string ownerID;

    Color brushColor;
    // public RenderTexture canvasTexture; // Render Texture that looks at our Base Texture and the painted brushes
    // Amend message to also store current drawing state
    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isDrawing; // new
        public string ownerID;
        public string brushString;
        public string player1;
        public string player2;
        public Message(Transform transform, bool isDrawing, string ownerID, string brushString, string player1, string player2)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.isDrawing = isDrawing; // new
            this.ownerID = ownerID;
            this.brushString = brushString;
            this.player1 = player1;
            this.player2 = player2;
        }
    }

    private void Start()
    {
        // Information used to keep track of the ownership 
        player1 = "";
        player2 = "";
        owner = false;
        released = false;
        // The color picker
        fcp.onColorChange.AddListener(OnChangeColor);
        context = NetworkScene.Register(this);
        // Initialize the color to be red
        BrushColor =  Color.red;
        brushColor = Color.red;

    }
    // Used to detect the color changes
    private void OnChangeColor(Color co)
    {
        BrushColor = co;
    }
    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        myID = AvatarManager.gameObject.transform.GetChild(0).gameObject.name.Substring(12);
        var data = msg.FromJson<Message>();

        if (data.player1 != "" && data.player2 != "")
        {
            transform.position = data.position;
            transform.rotation = data.rotation;
            var remoteBrushString = data.brushString;
            var prev_ID = ownerID;
            ownerID = data.ownerID;
            if (data.isDrawing && !currentDrawing)
            {
                createBrushFromList(remoteBrushString);
            }
            if (!data.isDrawing && currentDrawing)
            {
                EndDrawing();
            }
            if (owner && (prev_ID != ownerID) && ownerID != "" && myID != ownerID && myID == prev_ID)
            {
                Release();
            }
        }
        // When I am the only user that have the spray can
        else if ((data.player2 == myID && data.player1 == "") || (data.player1 == myID && data.player2 == ""))
        {
            transform.position = data.position;
            transform.rotation = data.rotation;
            var remoteBrushString = data.brushString;
            ownerID = myID;
            if (data.isDrawing && !currentDrawing)
            {
                createBrushFromList(remoteBrushString);
            }
            if (!data.isDrawing && currentDrawing)
            {
                EndDrawing();
            }
        }
        // When I am the other user that dose not have the spray can 
        else if ((data.player2 != "" && data.player2 != myID && data.player1 == "") || (data.player1 != "" && data.player1 != myID && data.player2 == ""))
        {
            transform.position = data.position;
            transform.rotation = data.rotation;
            var remoteBrushString = data.brushString;
            ownerID = data.ownerID;
            if (data.isDrawing && !currentDrawing)
            {
                createBrushFromList(remoteBrushString);
            }
            if (!data.isDrawing && currentDrawing)
            {
                EndDrawing();
            }
        }
        // When no one owns the spraycan
        else if (player1 == "" && player2 == "")
        {
           // GetComponent<ollider>().isTrigger = false;
           // GetComponent<Rigidbody>().useGravity = false;
        }


    }

    private void FixedUpdate()
    {
        myID = AvatarManager.gameObject.transform.GetChild(0).gameObject.name.Substring(12);
        // When someone is holding the spray can or noone is holding it
        if (player1 == myID || player2 == myID || (player2 == "" && player1 == ""))
        {
            string brushString = string.Join("|", this.brushList.ConvertAll(tuple => string.Join(",", tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, tuple.Item7, tuple.Item8)));
            context.SendJson(new Message(transform, isDrawing: currentDrawing, myID, brushString, this.player1, this.player2));
            currentDrawing = null;
            this.brushList = new List<(float, float, float, float, float, float, float, float)>();
        }
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
        // Update the ownership information
        if (player1 == "")
        {
            player1 = myID;
            myPlayerID = 1;
        }
        else if (player2 == "")
        {
            player2 = myID;
            myPlayerID = 2;
        }
        // Update the controller and owner ID
        if (!released)
        {
            owner = true;
            this.controller = controller;

            ownerID = myID;
            FixedUpdate();
        }
        else
        {
            ownerID = "";
            this.controller = null;
        }
    }

    void IGraspable.Release(Hand controller)

    {
        // Update the ownership information
        if (myPlayerID == 1)
        {
            player1 = "";
        }
        else if (myPlayerID == 2)
        {
            player2 = "";
        }
        myPlayerID = 0;
        owner = false;
        this.controller = null;
        ownerID = "";
    }

    // Passively release because the other user takes it 
    void Release() 
    {
        owner = false; 
        this.controller = null;
        released = true;
    }

    void IUseable.Use(Hand controller)
    {
        painting = true;
        // Constantly drawing without blocking the main thread
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

    // Add all the informations from the created brush to the list
    void addBrushToList(GameObject brush)
    {
        // RGBA
        float a = brush.GetComponent<SpriteRenderer>().color.a;
        float r = brush.GetComponent<SpriteRenderer>().color.r;
        float g = brush.GetComponent<SpriteRenderer>().color.g;
        float b = brush.GetComponent<SpriteRenderer>().color.b;
        // Coordinates
        float x = brush.transform.localPosition[0];
        float y = brush.transform.localPosition[1];
        float z = brush.transform.localPosition[2];
        // Brushsize
        float brushSize = brush.transform.localScale[0];
        this.brushList.Add((r, g, b, a, x, y, z, brushSize));
    }

    // Reconstructed the brushes that are created by other users
    void createBrushFromList(string input)
    {
        string[] sections = input.Split('|');
        foreach (string section in sections)
        {
            string[] numbers = section.Split(',');
            Vector3 uvWorldPosition = new Vector3(float.Parse(numbers[4]), float.Parse(numbers[5]), float.Parse(numbers[6]));
            Color color = new Color(float.Parse(numbers[0]), float.Parse(numbers[1]), float.Parse(numbers[2]), float.Parse(numbers[3]));
            float brushSize = float.Parse(numbers[7]);

            GameObject tempDrawing = (GameObject)Instantiate(Resources.Load("TexturePainter-Instances/BrushEntity"));
            tempDrawing.GetComponent<SpriteRenderer>().color = color;
            tempDrawing.transform.parent = brushContainer.transform; //Add the brush to our container to be wiped later
            tempDrawing.transform.localPosition = uvWorldPosition; //The position of the brush (in the UVMap)
            tempDrawing.transform.localScale = Vector3.one * brushSize;//The size of the brush
        }
    }

    void IUseable.UnUse(Hand controller)
    {
        EndDrawing();
    }

    // The main drawing function
    private void BeginDrawing()
    {
        brushColor = BrushColor;
        Vector3 uvWorldPosition = Vector3.zero;
        Vector3 hitPoint = Vector3.zero;

        // If the ray hits the target surface, return the corresponding canvas/texture map positon
        if (HitTestUVPosition(ref uvWorldPosition, ref hitPoint))
        {
            // Create the stroke from the local file
            currentDrawing = (GameObject)Instantiate(Resources.Load("TexturePainter-Instances/BrushEntity")); //Paint a brush

        
            GameObject nozzle = GameObject.Find("Cylinder");
            // Adjust the stroke size based on the distance from the nozzle to the surface
            float brushSize = 0.001f * Vector3.Distance(hitPoint, nozzle.GetComponent<Transform>().position);

            currentDrawing.GetComponent<SpriteRenderer>().color = brushColor; //Set the brush color
            brushColor.a = 1.0f; // Brushes have alpha to have a merging effect when painted over.
            currentDrawing.transform.parent = brushContainer.transform; //Add the brush to our container to be wiped later
            currentDrawing.transform.localPosition = uvWorldPosition; //The position of the brush (in the UVMap)
            currentDrawing.transform.localScale = Vector3.one * brushSize;//The size of the brush

            addBrushToList(currentDrawing);
        }
    }

    //Returns the position on the texuremap according to a hit in the mesh collider
    bool HitTestUVPosition(ref Vector3 uvWorldPosition, ref Vector3 hitPoint)
    {
        RaycastHit hit;
        GameObject nozzle = GameObject.Find("Cylinder");
        Vector3 cursorDir = nozzle.transform.forward;
        Ray cursorRay = new Ray(nozzle.GetComponent<Transform>().position, cursorDir);
        if (Physics.Raycast(cursorRay, out hit, 3))
        {
            hitPoint = hit.point;
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider == null || meshCollider.sharedMesh == null)
                return false;
            Vector2 pixelUV = new Vector2(hit.textureCoord.x, hit.textureCoord.y);
            uvWorldPosition.x = pixelUV.x - canvasCam.orthographicSize;//To center the UV on X
            uvWorldPosition.y = pixelUV.y - canvasCam.orthographicSize;//To center the UV on Y
            uvWorldPosition.z = 0.0f;
            return true;
        }
        else
        {
            return false;
        }
    }


    private void EndDrawing()
    {
        painting = false;
    }
}