/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.SceneManagement;

public class WaveFunctionCollapse : MonoBehaviour
{
    [SerializeField]
    public List<Tiles1> allTiles;

    public int gridSize;

    public GameObject[][] superPositionObjs;
    public GameObject[][] superPositionUIObjs;

    GameObject lastSuperObj;

    public GameObject canvas;

    bool isQuickSolved = false;

    public InputField value;

    public Slider speedValue;

    [System.Serializable]
    public class Tiles1
    {
        public Sprite tileSprite;
        public int rotation;
        public TilesSockets sockets;
        public bool asimetrik;
    }
    [System.Serializable]
    public class TilesSockets
    {
        public int[] posX;
        public int[] negX;
        public int[] posY;
        public int[] negY;
    }

    [System.Serializable]
    public class superPositionsData1 : MonoBehaviour
    {
        public Vector2 pos;
        public Tiles1 tile;
        public List<Tiles1> currentTiles;
    }

    private void Start()
    {
        int allTilesCount = allTiles.Count;
        for (int i = 0; i < allTilesCount; i++)
        {
            if(!allTiles[i].asimetrik)
            {
                for (int j = 1; j < 4; j++)
                {
                    allTiles.Add(new Tiles1());
                    Tiles1 newTile = allTiles.Last();
                    TilesSockets newSockets = new TilesSockets();
                    newSockets.posX = allTiles[i].sockets.posX;
                    newSockets.negX = allTiles[i].sockets.negX;
                    newSockets.posY = allTiles[i].sockets.posY;
                    newSockets.negY = allTiles[i].sockets.negY;
                    for (int k = 0; k < j; k++)
                    {
                        int[] tempPosX = newSockets.posX;
                        int[] tempNegX = newSockets.negX;
                        int[] tempPosY = newSockets.posY;
                        int[] tempNegY = newSockets.negY;
                        newSockets.posX = tempPosY;
                        newSockets.negX = tempNegY;
                        newSockets.posY = tempNegX;
                        newSockets.negY = tempPosX;
                    }
                    newTile.sockets = newSockets;
                    newTile.tileSprite = allTiles[i].tileSprite;
                    newTile.rotation = j;
                }
            }
        }
        CreateSuperPositions();
    }

    public void CreateSuperPositions()
    {
        superPositionObjs = new GameObject[gridSize][];
        //superPositionUIObjs = new GameObject[gridSize][];
        int NxN = CalculateNxN(allTiles.Count);
        for (int i = 0; i < gridSize; i++)
        {
            superPositionObjs[i] = new GameObject[gridSize];
            //superPositionUIObjs[i] = new GameObject[gridSize];
            for (int j = 0; j < gridSize; j++)
            {
                GameObject superPositionObj = new GameObject();
                superPositionObj.AddComponent<SpriteRenderer>();
                superPositionObj.transform.position = new Vector3(i * Camera.main.orthographicSize / gridSize * 2 + Camera.main.orthographicSize / gridSize, j * Camera.main.orthographicSize / gridSize * 2 + Camera.main.orthographicSize / gridSize, 0) +
                    new Vector3(-Camera.main.orthographicSize, -Camera.main.orthographicSize, 0) +
                    new Vector3(Camera.main.orthographicSize / 3, 0, 0);
                superPositionObj.AddComponent<superPositionsData1>();
                superPositionObj.GetComponent<superPositionsData1>().pos = new Vector2(i, j);
                superPositionObj.GetComponent<superPositionsData1>().tile = null;
                superPositionObj.GetComponent<superPositionsData1>().currentTiles = new List<Tiles1>();
                superPositionObj.tag = "Objs";
                foreach (Tiles1 tile in allTiles)
                {
                    superPositionObj.GetComponent<superPositionsData1>().currentTiles.Add(tile);
                }
                superPositionObj.name = $"[x->{i},  y->{j}]";
                superPositionObjs[i][j] = superPositionObj;
                //CreateSuperPositionsUI(superPositionObjs[i][j], NxN, superPositionObj.GetComponent<superPositionsData>().currentTiles);
            }
        }
    }

    public void CreateSuperPositionsUI(GameObject superPositionObj, int NxN, List<Tiles1> tileList)
    {
        GameObject superPositionUIObj = new GameObject();
        superPositionUIObj.transform.SetParent(canvas.transform);
        superPositionUIObj.AddComponent<RectTransform>();
        superPositionUIObj.GetComponent<RectTransform>().localScale = Vector3.one;
        superPositionUIObj.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.height / gridSize, Screen.height / gridSize);
        superPositionUIObj.AddComponent<GridLayoutGroup>();
        superPositionUIObj.GetComponent<GridLayoutGroup>().spacing = Vector2.one * 5;
        Vector2 cellSize = Vector2.one * ((superPositionUIObj.GetComponent<RectTransform>().sizeDelta.x / NxN) - superPositionUIObj.GetComponent<GridLayoutGroup>().spacing.x);
        superPositionUIObj.GetComponent<GridLayoutGroup>().cellSize = cellSize;
        superPositionUIObj.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        superPositionUIObj.transform.position = superPositionObj.transform.position;
        superPositionUIObj.tag = "Objs";
        int cellCount = NxN * NxN;
        for (int i = 0; i < cellCount; i++)
        {
            if (i < tileList.Count)
            {
                GameObject tileButton = new GameObject();
                tileButton.AddComponent<CanvasRenderer>();
                tileButton.AddComponent<Image>();
                tileButton.GetComponent<Image>().sprite = allTiles[i].tileSprite;
                tileButton.AddComponent<Button>();
                Tiles1 btnTile = allTiles[i];
                tileButton.GetComponent<Button>().onClick.AddListener(() => { SuperPositionCreateWithButton(superPositionObj, btnTile); });
                tileButton.transform.SetParent(superPositionUIObj.transform);
                tileButton.GetComponent<RectTransform>().localScale = Vector3.one;
            }
        }
        superPositionUIObjs[(int)superPositionObj.GetComponent<superPositionsData1>().pos.x][(int)superPositionObj.GetComponent<superPositionsData1>().pos.y] = superPositionUIObj;
    }

    public void RearrangeSuperPositionUI(GameObject superPositionUIObj, GameObject superPositionObj, int NxN, List<Tiles1> tileList)
    {
        foreach (Transform btn in superPositionUIObj.transform)
        {
            Destroy(btn.gameObject);
        }
        int cellCount = Mathf.Clamp(NxN * NxN, 0, tileList.Count);
        for (int i = 0; i < cellCount; i++)
        {
            if (i < tileList.Count)
            {
                Tiles1 newTile = tileList[i];
                GameObject tileButton = new GameObject();
                tileButton.AddComponent<CanvasRenderer>();
                tileButton.AddComponent<Image>();
                tileButton.GetComponent<Image>().sprite = newTile.tileSprite;
                tileButton.AddComponent<Button>();
                tileButton.GetComponent<Button>().onClick.AddListener(() => { SuperPositionCreateWithButton(superPositionObj, newTile); });
                tileButton.transform.SetParent(superPositionUIObj.transform);
                tileButton.GetComponent<RectTransform>().localScale = Vector3.one;
            }
        }
    }

    public void SuperPositionCreateWithButton(GameObject superPositionObj, Tiles1 buttonTile)
    {
        superPositionObj.GetComponent<superPositionsData1>().tile = buttonTile;
        CheckSuperPositionNeighbours(superPositionObj);
    }

    void CheckSuperPositionNeighbours(GameObject superPositionObj)
    {
        superPositionsData1 data = superPositionObj.GetComponent<superPositionsData1>();
        superPositionObj.GetComponent<SpriteRenderer>().sprite = superPositionObj.GetComponent<superPositionsData1>().tile.tileSprite;
        superPositionObj.transform.localScale = 17.85f * Vector2.one / gridSize;
        superPositionObj.transform.Rotate(new Vector3(0, 0, -90 * superPositionObj.GetComponent<superPositionsData1>().tile.rotation));
        if (data.pos.x - 1 >= 0)
        {
            if(superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().tile == null)
            {
                int tileCount = superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles.Count;
                List<Tiles1> equalTiles = new List<Tiles1>();
                for (int i = 0; i < tileCount; i++)
                {
                    bool arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles[i].sockets.posX, data.tile.sockets.negX);
                    if (arraysEqual)
                    {
                        equalTiles.Add(superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles[i]);
                    }
                }
                //int NxN = CalculateNxN(equalTiles.Count);
                    superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles = equalTiles;
                //RearrangeSuperPositionUI(superPositionUIObjs[(int)data.pos.x - 1][(int)data.pos.y],superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y], NxN, equalTiles);
            }
        }
        if (data.pos.x + 1 < superPositionObjs.Length && superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().tile == null)
        {
            int tileCount = superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles.Count;
            List<Tiles1> equalTiles = new List<Tiles1>();
            for (int i = 0; i < tileCount; i++)
            {
                bool arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles[i].sockets.negX, data.tile.sockets.posX);
                if (arraysEqual)
                {
                    equalTiles.Add(superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles[i]);
                }
            }
            //int NxN = CalculateNxN(equalTiles.Count);
                superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles = equalTiles;
            //RearrangeSuperPositionUI(superPositionUIObjs[(int)data.pos.x + 1][(int)data.pos.y], superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y], NxN, equalTiles);
        }
        if (data.pos.y - 1 >= 0 && superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().tile == null)
        {
            int tileCount = superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().currentTiles.Count;
            List<Tiles1> equalTiles = new List<Tiles1>();
            for (int i = 0; i < tileCount; i++)
            {
                bool arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().currentTiles[i].sockets.posY, data.tile.sockets.negY);
                if (arraysEqual)
                {
                    equalTiles.Add(superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().currentTiles[i]);
                }
            }
            //int NxN = CalculateNxN(equalTiles.Count);
                superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().currentTiles = equalTiles;
            //RearrangeSuperPositionUI(superPositionUIObjs[(int)data.pos.x][(int)data.pos.y-1], superPositionObjs[(int)data.pos.x][(int)data.pos.y-1], NxN, equalTiles);
        }
        if (data.pos.y + 1 < superPositionObjs.Length && superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().tile == null)
        {
            int tileCount = superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().currentTiles.Count;
            List<Tiles1> equalTiles = new List<Tiles1>();
            for (int i = 0; i < tileCount; i++)
            {
                bool arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().currentTiles[i].sockets.negY, data.tile.sockets.posY);
                if (arraysEqual)
                {
                    equalTiles.Add(superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().currentTiles[i]);
                }
            }
            //int NxN = CalculateNxN(equalTiles.Count);
                superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().currentTiles = equalTiles;
            //RearrangeSuperPositionUI(superPositionUIObjs[(int)data.pos.x][(int)data.pos.y + 1], superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1], NxN, equalTiles);
        }
        //superPositionUIObjs[(int)data.pos.x][(int)data.pos.y].SetActive(false);
    }
    public int CalculateNxN(int length)
    {
        int NxN = 0;
        while (NxN * NxN < length)
        {
            NxN++;
        }
        return NxN;
    }

    public static bool ArrayEquals<T>(T[] a, T[] b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        for (int i = 0; i < a.Length; i++)
        {
            if (!a[i].Equals(b[i]))
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerator QuickSolve()
    {
        if (isQuickSolved)
        {
            SetGridSize();
        }
        List<GameObject> openSuperPositionObjs = new List<GameObject>();
        int lowestEntropy = allTiles.Count + 1;
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                openSuperPositionObjs.Add(superPositionObjs[i][j]);
            }
        }

        while (openSuperPositionObjs.Count != 0)
        {
            lowestEntropy = allTiles.Count + 1;
            GameObject currentSuperObj = null;
            foreach (GameObject superPositionObjInOpen in openSuperPositionObjs)
            {
                if (superPositionObjInOpen.GetComponent<superPositionsData1>().currentTiles.Count < lowestEntropy)
                {
                    currentSuperObj = superPositionObjInOpen;
                    lowestEntropy = superPositionObjInOpen.GetComponent<superPositionsData1>().currentTiles.Count;
                }
            }
            superPositionsData1 data = currentSuperObj.GetComponent<superPositionsData1>();
            data.tile = data.currentTiles[Random.Range(0, data.currentTiles.Count)];
            data.currentTiles.Clear();
            data.currentTiles.Add(data.tile);
            CheckSuperPositionNeighbours(currentSuperObj);
            openSuperPositionObjs.Remove(currentSuperObj);
            yield return new WaitForSeconds(1 / Mathf.Clamp(speedValue.value, 0.01f, 1f) / 100);
        }
        if (!isQuickSolved)
        {
            isQuickSolved = true;
        }
    }

    public void QuickSolveButton()
    {
        StartCoroutine(QuickSolve());
    }

    public void SetGridSize()
    {
        if (value.text != null)
        {
            gridSize = int.Parse(value.text);
            GameObject[] objs = GameObject.FindGameObjectsWithTag("Objs");
            foreach (GameObject obj in objs)
            {
                Destroy(obj);
            }
            isQuickSolved = false;
            CreateSuperPositions();
        }
    }
}
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.SceneManagement;

public class WaveFunctionCollapse : MonoBehaviour
{
    [SerializeField]
    public List<Tiles1> allTiles;

    public int gridSize;

    public GameObject[][] superPositionObjs;
    public GameObject[][] superPositionUIObjs;

    GameObject lastSuperObj;

    public GameObject canvas;

    bool isQuickSolved = false;

    public InputField value;

    public Slider speedValue;

    [System.Serializable]
    public class Tiles1
    {
        public Sprite tileSprite;
        public int rotation;
        public TilesSockets sockets;
        public bool asimetrik;
    }
    [System.Serializable]
    public class TilesSockets
    {
        public int[] posX;
        public int[] negX;
        public int[] posY;
        public int[] negY;
    }

    [System.Serializable]
    public class superPositionsData1 : MonoBehaviour
    {
        public Vector2 pos;
        public Tiles1 tile;
        public List<Tiles1> currentTiles;
    }

    private void Start()
    {
        int allTilesCount = allTiles.Count;
        for (int i = 0; i < allTilesCount; i++)
        {
            if (!allTiles[i].asimetrik)
            {
                for (int j = 1; j < 4; j++)
                {
                    allTiles.Add(new Tiles1());
                    Tiles1 newTile = allTiles.Last();
                    TilesSockets newSockets = new TilesSockets();
                    newSockets.posX = allTiles[i].sockets.posX;
                    newSockets.negX = allTiles[i].sockets.negX;
                    newSockets.posY = allTiles[i].sockets.posY;
                    newSockets.negY = allTiles[i].sockets.negY;
                    for (int k = 0; k < j; k++)
                    {
                        int[] tempPosX = newSockets.posX;
                        int[] tempNegX = newSockets.negX;
                        int[] tempPosY = newSockets.posY;
                        int[] tempNegY = newSockets.negY;
                        newSockets.posX = tempPosY;
                        newSockets.negX = tempNegY;
                        newSockets.posY = tempNegX;
                        newSockets.negY = tempPosX;
                    }
                    newTile.sockets = newSockets;
                    newTile.tileSprite = allTiles[i].tileSprite;
                    newTile.rotation = j;
                }
            }
        }
        CreateSuperPositions();
    }

    public void CreateSuperPositions()
    {
        superPositionObjs = new GameObject[gridSize][];
        //superPositionUIObjs = new GameObject[gridSize][];
        int NxN = CalculateNxN(allTiles.Count);
        for (int i = 0; i < gridSize; i++)
        {
            superPositionObjs[i] = new GameObject[gridSize];
            //superPositionUIObjs[i] = new GameObject[gridSize];
            for (int j = 0; j < gridSize; j++)
            {
                GameObject superPositionObj = new GameObject();
                superPositionObj.AddComponent<SpriteRenderer>();
                superPositionObj.transform.position = new Vector3(i * Camera.main.orthographicSize / gridSize * 2 + Camera.main.orthographicSize / gridSize, j * Camera.main.orthographicSize / gridSize * 2 + Camera.main.orthographicSize / gridSize, 0) +
                    new Vector3(-Camera.main.orthographicSize, -Camera.main.orthographicSize, 0) +
                    new Vector3(Camera.main.orthographicSize / 3, 0, 0);
                superPositionObj.AddComponent<superPositionsData1>();
                superPositionObj.GetComponent<superPositionsData1>().pos = new Vector2(i, j);
                superPositionObj.GetComponent<superPositionsData1>().tile = null;
                superPositionObj.GetComponent<superPositionsData1>().currentTiles = new List<Tiles1>();
                superPositionObj.tag = "Objs";
                foreach (Tiles1 tile in allTiles)
                {
                    superPositionObj.GetComponent<superPositionsData1>().currentTiles.Add(tile);
                }
                superPositionObj.name = $"[x->{i},  y->{j}]";
                superPositionObjs[i][j] = superPositionObj;
                //CreateSuperPositionsUI(superPositionObjs[i][j], NxN, superPositionObj.GetComponent<superPositionsData>().currentTiles);
            }
        }
    }

    public void CreateSuperPositionsUI(GameObject superPositionObj, int NxN, List<Tiles1> tileList)
    {
        GameObject superPositionUIObj = new GameObject();
        superPositionUIObj.transform.SetParent(canvas.transform);
        superPositionUIObj.AddComponent<RectTransform>();
        superPositionUIObj.GetComponent<RectTransform>().localScale = Vector3.one;
        superPositionUIObj.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.height / gridSize, Screen.height / gridSize);
        superPositionUIObj.AddComponent<GridLayoutGroup>();
        superPositionUIObj.GetComponent<GridLayoutGroup>().spacing = Vector2.one * 5;
        Vector2 cellSize = Vector2.one * ((superPositionUIObj.GetComponent<RectTransform>().sizeDelta.x / NxN) - superPositionUIObj.GetComponent<GridLayoutGroup>().spacing.x);
        superPositionUIObj.GetComponent<GridLayoutGroup>().cellSize = cellSize;
        superPositionUIObj.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        superPositionUIObj.transform.position = superPositionObj.transform.position;
        superPositionUIObj.tag = "Objs";
        int cellCount = NxN * NxN;
        for (int i = 0; i < cellCount; i++)
        {
            if (i < tileList.Count)
            {
                GameObject tileButton = new GameObject();
                tileButton.AddComponent<CanvasRenderer>();
                tileButton.AddComponent<Image>();
                tileButton.GetComponent<Image>().sprite = allTiles[i].tileSprite;
                tileButton.AddComponent<Button>();
                Tiles1 btnTile = allTiles[i];
                tileButton.GetComponent<Button>().onClick.AddListener(() => { SuperPositionCreateWithButton(superPositionObj, btnTile); });
                tileButton.transform.SetParent(superPositionUIObj.transform);
                tileButton.GetComponent<RectTransform>().localScale = Vector3.one;
            }
        }
        superPositionUIObjs[(int)superPositionObj.GetComponent<superPositionsData1>().pos.x][(int)superPositionObj.GetComponent<superPositionsData1>().pos.y] = superPositionUIObj;
    }

    public void RearrangeSuperPositionUI(GameObject superPositionUIObj, GameObject superPositionObj, int NxN, List<Tiles1> tileList)
    {
        foreach (Transform btn in superPositionUIObj.transform)
        {
            Destroy(btn.gameObject);
        }
        int cellCount = Mathf.Clamp(NxN * NxN, 0, tileList.Count);
        for (int i = 0; i < cellCount; i++)
        {
            if (i < tileList.Count)
            {
                Tiles1 newTile = tileList[i];
                GameObject tileButton = new GameObject();
                tileButton.AddComponent<CanvasRenderer>();
                tileButton.AddComponent<Image>();
                tileButton.GetComponent<Image>().sprite = newTile.tileSprite;
                tileButton.AddComponent<Button>();
                tileButton.GetComponent<Button>().onClick.AddListener(() => { SuperPositionCreateWithButton(superPositionObj, newTile); });
                tileButton.transform.SetParent(superPositionUIObj.transform);
                tileButton.GetComponent<RectTransform>().localScale = Vector3.one;
            }
        }
    }

    public void SuperPositionCreateWithButton(GameObject superPositionObj, Tiles1 buttonTile)
    {
        superPositionObj.GetComponent<superPositionsData1>().tile = buttonTile;
        CheckSuperPositionNeighbours(superPositionObj);
    }

    void CheckSuperPositionNeighbours(GameObject superPositionObj)
    {
        superPositionsData1 data = superPositionObj.GetComponent<superPositionsData1>();
        if (data.pos.x - 1 >= 0)
        {
            if (superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().tile == null)
            {
                int tileCount = superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles.Count;
                List<Tiles1> equalTiles = new List<Tiles1>();
                for (int i = 0; i < tileCount; i++)
                {
                    bool arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles[i].sockets.posX, data.tile.sockets.negX);
                    if (arraysEqual)
                    {
                        equalTiles.Add(superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles[i]);
                    }
                }
                superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles = equalTiles;
            }
        }
        if (data.pos.x + 1 < superPositionObjs.Length)
        {
            if(superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().tile == null)
            {
                int tileCount = superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles.Count;
                List<Tiles1> equalTiles = new List<Tiles1>();
                for (int i = 0; i < tileCount; i++)
                {
                    bool arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles[i].sockets.negX, data.tile.sockets.posX);
                    if (arraysEqual)
                    {
                        equalTiles.Add(superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles[i]);
                    }
                }
                superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles = equalTiles;
            }
        }
        if (data.pos.y - 1 >= 0)
        {
            if(superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().tile == null)
            {
                int tileCount = superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().currentTiles.Count;
                List<Tiles1> equalTiles = new List<Tiles1>();
                for (int i = 0; i < tileCount; i++)
                {
                    bool arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().currentTiles[i].sockets.posY, data.tile.sockets.negY);
                    if (arraysEqual)
                    {
                        equalTiles.Add(superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().currentTiles[i]);
                    }
                }
                superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().currentTiles = equalTiles;
            }
        }
        if (data.pos.y + 1 < superPositionObjs.Length)
        {
            if (superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().tile == null)
            {
                int tileCount = superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().currentTiles.Count;
                List<Tiles1> equalTiles = new List<Tiles1>();
                for (int i = 0; i < tileCount; i++)
                {
                    bool arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().currentTiles[i].sockets.negY, data.tile.sockets.posY);
                    if (arraysEqual)
                    {
                        equalTiles.Add(superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().currentTiles[i]);
                    }
                }
                superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().currentTiles = equalTiles;
            }
        }

        superPositionObj.GetComponent<SpriteRenderer>().sprite = superPositionObj.GetComponent<superPositionsData1>().tile.tileSprite;
        //superPositionObj.transform.localScale =  Vector2.one * 64 / gridSize;
        superPositionObj.transform.localScale = 17.87f * Vector2.one / gridSize;
        superPositionObj.transform.Rotate(new Vector3(0, 0, -90 * superPositionObj.GetComponent<superPositionsData1>().tile.rotation));
    }
    public int CalculateNxN(int length)
    {
        int NxN = 0;
        while (NxN * NxN < length)
        {
            NxN++;
        }
        return NxN;
    }

    public static bool ArrayEquals<T>(T[] a, T[] b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        for (int i = 0; i < a.Length; i++)
        {
            if (!a[Mathf.Abs(i-2)].Equals(b[i]))
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerator QuickSolve()
    {
        if (isQuickSolved)
        {
            SetGridSize();
        }
        List<GameObject> openSuperPositionObjs = new List<GameObject>();
        List<GameObject> startList = new List<GameObject>();
        List<GameObject> closedSuperPositionObjs = new List<GameObject>();
        Vector2Int randomSuperObjValue = new Vector2Int(Random.Range(0,superPositionObjs[0].Length) ,Random.Range(0, superPositionObjs[0].Length));
        int lowestEntropy = allTiles.Count + 1;
        openSuperPositionObjs.Add(superPositionObjs[randomSuperObjValue.x][randomSuperObjValue.y]);
        while (openSuperPositionObjs.Count != 0)
        {
            lowestEntropy = allTiles.Count + 1;
            GameObject currentSuperObj = null;
            List<GameObject> equalEntropies = new List<GameObject>();
            foreach (GameObject superPositionObjInOpen in openSuperPositionObjs)
            {
                if (superPositionObjInOpen.GetComponent<superPositionsData1>().currentTiles.Count < lowestEntropy)
                {
                    currentSuperObj = superPositionObjInOpen;
                    lowestEntropy = superPositionObjInOpen.GetComponent<superPositionsData1>().currentTiles.Count;
                    equalEntropies.Clear();
                }
            }
            if (equalEntropies.Count != 0)
            {
                currentSuperObj = equalEntropies[Random.Range(0, equalEntropies.Count)];
            }
            if(currentSuperObj.GetComponent<superPositionsData1>().currentTiles.Count != 0)
            {
                currentSuperObj.GetComponent<superPositionsData1>().tile = currentSuperObj.GetComponent<superPositionsData1>().currentTiles[Random.Range(0, currentSuperObj.GetComponent<superPositionsData1>().currentTiles.Count)];
            }
            else
            {
                lastSuperObj.GetComponent<superPositionsData1>().currentTiles.Remove(lastSuperObj.GetComponent<superPositionsData1>().tile);
                lastSuperObj.GetComponent<superPositionsData1>().tile = null;
                continue;
            }
            superPositionsData1 data = currentSuperObj.GetComponent<superPositionsData1>();
            /*while (true)
            {
                if (data.currentTiles.Count == 0)
                {
                    lastSuperObj.GetComponent<superPositionsData1>().currentTiles.Remove(lastSuperObj.GetComponent<superPositionsData1>().tile);
                }
                data.tile = data.currentTiles[Random.Range(0, data.currentTiles.Count)];
                if (data.pos.x - 1 >= 0)
                {
                    if (superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().tile != null)
                    {
                        bool arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().tile.sockets.posX, data.tile.sockets.negX);
                        if (!arraysEqual)
                        {
                            data.currentTiles.Remove(data.tile);
                            data.tile = data.currentTiles[Random.Range(0, data.currentTiles.Count)];
                            continue;
                        }
                    }
                    else
                    {
                        bool arraysEqual = false;
                        for (int k = 0; k < superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles.Count; k++)
                        {
                            arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles[k].sockets.posX, data.tile.sockets.negX);
                            if (arraysEqual)
                            {
                                Debug.Log("sa");
                                break;
                            }
                        }
                        if(!arraysEqual)
                        {
                                data.currentTiles.Remove(data.tile);
                            if(data.currentTiles.Count != 0)
                            {
                                data.tile = data.currentTiles[Random.Range(0, data.currentTiles.Count)];
                            }
                            continue;
                        }
                    }
                }
                if (data.pos.x + 1 < superPositionObjs.Length)
                {
                    if (superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().tile != null)
                    {
                        bool arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().tile.sockets.negX, data.tile.sockets.posX);
                        if (!arraysEqual)
                        {
                            data.currentTiles.Remove(data.tile);
                            data.tile = data.currentTiles[Random.Range(0, data.currentTiles.Count)];
                            continue;
                        }
                    }
                    else
                    {
                        bool arraysEqual = false;
                        for (int k = 0; k < superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles.Count; k++)
                        {
                            arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().currentTiles[k].sockets.negX, data.tile.sockets.posX);
                            if (arraysEqual)
                            {
                                break;
                            }
                        }
                        if(!arraysEqual)
                        {
                            data.currentTiles.Remove(data.tile);
                            if (data.currentTiles.Count != 0)
                            {
                                data.tile = data.currentTiles[Random.Range(0, data.currentTiles.Count)];
                            }
                            continue;
                        }
                    }
                }
                if (data.pos.y - 1 >= 0)
                {
                    if (superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().tile != null)
                    {
                        bool arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().tile.sockets.posY, data.tile.sockets.negY);
                        if (!arraysEqual)
                        {
                            data.currentTiles.Remove(data.tile);
                            data.tile = data.currentTiles[Random.Range(0, data.currentTiles.Count)];
                            continue;
                        }
                    }
                    else
                    {
                        bool arraysEqual = false;
                        for (int k = 0; k < superPositionObjs[(int)data.pos.x][(int)data.pos.y-1].GetComponent<superPositionsData1>().currentTiles.Count; k++)
                        {
                            arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x][(int)data.pos.y-1].GetComponent<superPositionsData1>().currentTiles[k].sockets.posY, data.tile.sockets.negY);
                            if (arraysEqual)
                            {
                                break;
                            }
                        }
                        if(!arraysEqual)
                        {
                                data.currentTiles.Remove(data.tile);
                            if(data.currentTiles.Count != 0)
                            {
                                data.tile = data.currentTiles[Random.Range(0, data.currentTiles.Count)];
                            }
                            continue;
                        }
                    }
                }
                if (data.pos.y + 1 < superPositionObjs.Length)
                {
                    if (superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().tile != null)
                    {
                        bool arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().tile.sockets.negY, data.tile.sockets.posY);
                        if (!arraysEqual)
                        {
                            data.currentTiles.Remove(data.tile);
                            data.tile = data.currentTiles[Random.Range(0, data.currentTiles.Count)];
                            continue;
                        }
                    }
                    else
                    {
                        bool arraysEqual = false;
                        for (int k = 0; k < superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().currentTiles.Count; k++)
                        {
                            arraysEqual = ArrayEquals<int>(superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().currentTiles[k].sockets.negY, data.tile.sockets.posY);
                            if (arraysEqual)
                            {
                                break;
                            }
                        }
                        if(!arraysEqual)
                        {
                            data.currentTiles.Remove(data.tile);
                            if (data.currentTiles.Count != 0)
                            {
                                data.tile = data.currentTiles[Random.Range(0, data.currentTiles.Count)];
                            }
                            continue;
                        }
                    }
                }
                break;
            }*/

            CheckSuperPositionNeighbours(currentSuperObj);
            closedSuperPositionObjs.Add(currentSuperObj);
            openSuperPositionObjs.Remove(currentSuperObj);
            if (data.pos.x - 1 >= 0 && superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData1>().tile == null && !closedSuperPositionObjs.Contains(superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y]))
            {
                if (!openSuperPositionObjs.Contains(superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y]))
                {
                    openSuperPositionObjs.Add(superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y]);
                }
            }
            if (data.pos.x + 1 < superPositionObjs.Length && superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData1>().tile == null && !closedSuperPositionObjs.Contains(superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y]))
            {
                if (!openSuperPositionObjs.Contains(superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y]))
                {
                    openSuperPositionObjs.Add(superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y]);
                }
            }
            if (data.pos.y - 1 >= 0 && superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData1>().tile == null && !closedSuperPositionObjs.Contains(superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1]))
            {
                if (!openSuperPositionObjs.Contains(superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1]))
                {
                    openSuperPositionObjs.Add(superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1]);
                }
            }
            if (data.pos.y + 1 < superPositionObjs.Length && superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData1>().tile == null && !closedSuperPositionObjs.Contains(superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1]))
            {
                if (!openSuperPositionObjs.Contains(superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1]))
                {
                    openSuperPositionObjs.Add(superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1]);
                }
            }
            lastSuperObj = currentSuperObj;
            yield return new WaitForSeconds(1 / Mathf.Clamp(speedValue.value, 0.001f, 1f) / 1000);
        }
        if (!isQuickSolved)
        {
            isQuickSolved = true;
        }
    }

    public void QuickSolveButton()
    {
        StartCoroutine(QuickSolve());
    }

    public void SetGridSize()
    {
        if (value.text != null)
        {
            gridSize = int.Parse(value.text);
            GameObject[] objs = GameObject.FindGameObjectsWithTag("Objs");
            foreach (GameObject obj in objs)
            {
                Destroy(obj);
            }
            isQuickSolved = false;
            CreateSuperPositions();
        }
    }
}
