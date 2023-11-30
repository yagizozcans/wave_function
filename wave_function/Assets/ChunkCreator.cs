using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChunkCreator : MonoBehaviour
{
    public Tiles[] allTiles;

    public int gridSize;

    public GameObject[][] superPositionObjs;
    public GameObject[][] superPositionUIObjs;

    public GameObject canvas;

    [System.Serializable]
    public class Tiles
    {
        public string tileName;
        public Sprite tileSprite;
        public Sockets sockets;
    }
    [System.Serializable]
    public class Sockets
    {
        public int posX;
        public int negX;
        public int posY;
        public int negY;
    }

    [System.Serializable]
    public class superPositionsData : MonoBehaviour
    {
        public Vector2 pos;
        public Tiles tile;
        public List<Tiles> currentTiles;
    }

    private void Start()
    {
        CreateSuperPositions();
    }

    public void CreateSuperPositions()
    {
        superPositionObjs = new GameObject[gridSize][];
        superPositionUIObjs = new GameObject[gridSize][];
        int NxN = CalculateNxN(allTiles.Length);
        for(int i = 0; i < gridSize; i++)
        {
            superPositionObjs[i] = new GameObject[gridSize];
            superPositionUIObjs[i] = new GameObject[gridSize];
            for (int j = 0; j < gridSize; j++)
            {
                GameObject superPositionObj = new GameObject();
                superPositionObj.AddComponent<SpriteRenderer>();
                superPositionObj.transform.position = new Vector3(i * Camera.main.orthographicSize / gridSize*2 + Camera.main.orthographicSize / gridSize, j * Camera.main.orthographicSize/gridSize*2 + Camera.main.orthographicSize / gridSize, 0)+
                    new Vector3(-Camera.main.orthographicSize,-Camera.main.orthographicSize,0);
                superPositionObj.AddComponent<superPositionsData>();
                superPositionObj.GetComponent<superPositionsData>().pos = new Vector2(i,j);
                superPositionObj.GetComponent<superPositionsData>().tile = null;
                superPositionObj.GetComponent<superPositionsData>().currentTiles = new List<Tiles>();
                foreach(Tiles tile in allTiles)
                {
                    superPositionObj.GetComponent<superPositionsData>().currentTiles.Add(tile);
                }
                superPositionObj.name = $"[x->{i},  y->{j}]";
                superPositionObjs[i][j] = superPositionObj;
                CreateSuperPositionsUI(superPositionObjs[i][j], NxN, superPositionObj.GetComponent<superPositionsData>().currentTiles);
            }
        }
    }

    public void CreateSuperPositionsUI(GameObject superPositionObj,int NxN,List<Tiles> tileList)
    {
        GameObject superPositionUIObj = new GameObject();
        superPositionUIObj.transform.SetParent(canvas.transform);
        superPositionUIObj.AddComponent<RectTransform>();
        superPositionUIObj.GetComponent<RectTransform>().localScale = Vector3.one;
        superPositionUIObj.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / gridSize, Screen.height / gridSize);
        superPositionUIObj.AddComponent<GridLayoutGroup>();
        superPositionUIObj.GetComponent<GridLayoutGroup>().spacing = Vector2.one * 5;
        Vector2 cellSize = Vector2.one * ((superPositionUIObj.GetComponent<RectTransform>().sizeDelta.x / NxN) - superPositionUIObj.GetComponent<GridLayoutGroup>().spacing.x);
        superPositionUIObj.GetComponent<GridLayoutGroup>().cellSize = cellSize;
        superPositionUIObj.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        superPositionUIObj.transform.position = superPositionObj.transform.position;
        int cellCount = NxN * NxN;
        for(int i = 0; i < cellCount; i++)
        {
            if (i < tileList.Count)
            {
                GameObject tileButton = new GameObject();
                tileButton.AddComponent<CanvasRenderer>();
                tileButton.AddComponent<Image>();
                tileButton.GetComponent<Image>().sprite = allTiles[i].tileSprite;
                tileButton.AddComponent<Button>();
                Tiles btnTile = allTiles[i];
                tileButton.GetComponent<Button>().onClick.AddListener(() => { SuperPositionCreateWithButton(superPositionObj, btnTile); });
                tileButton.transform.SetParent(superPositionUIObj.transform);
                tileButton.GetComponent<RectTransform>().localScale = Vector3.one;
            }
        }
        superPositionUIObjs[(int)superPositionObj.GetComponent<superPositionsData>().pos.x][(int)superPositionObj.GetComponent<superPositionsData>().pos.y] = superPositionUIObj;
    }

    public void RearrangeSuperPositionUI(GameObject superPositionUIObj,GameObject superPositionObj,int NxN,List<Tiles> tileList)
    {
        foreach(Transform btn in superPositionUIObj.transform)
        {
            Destroy(btn.gameObject);
        }
        int cellCount = Mathf.Clamp(NxN * NxN,0,tileList.Count);
        for (int i = 0; i < cellCount; i++)
        {
            if (i < tileList.Count)
            {
                Tiles newTile = tileList[i];
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
        superPositionObj.GetComponent<superPositionsData>().currentTiles = tileList;
    }
    
    public void SuperPositionCreateWithButton(GameObject superPositionObj,Tiles buttonTile)
    {
        superPositionObj.GetComponent<superPositionsData>().tile = buttonTile;
        CheckSuperPositionNeighbours(superPositionObj);
    }

    void CheckSuperPositionNeighbours(GameObject superPositionObj)
    {
        superPositionsData data = superPositionObj.GetComponent<superPositionsData>();
        if (data.pos.x - 1 >= 0 && superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData>().tile == null)
        {
            int tileCount = superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData>().currentTiles.Count;
            List<Tiles> equalTiles = new List<Tiles>();
            for (int i = 0; i < tileCount; i++)
            {
                if (superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData>().currentTiles[i].sockets.posX == data.tile.sockets.negX)
                {
                    equalTiles.Add(superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y].GetComponent<superPositionsData>().currentTiles[i]);
                }
            }
            int NxN = CalculateNxN(equalTiles.Count);
            RearrangeSuperPositionUI(superPositionUIObjs[(int)data.pos.x - 1][(int)data.pos.y],superPositionObjs[(int)data.pos.x - 1][(int)data.pos.y], NxN, equalTiles);
        }
        if (data.pos.x + 1 < superPositionObjs.Length && superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData>().tile == null)
        {
            int tileCount = superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData>().currentTiles.Count;
            List<Tiles> equalTiles = new List<Tiles>();
            for (int i = 0; i < tileCount; i++)
            {
                if (superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData>().currentTiles[i].sockets.negX == data.tile.sockets.posX)
                {
                    equalTiles.Add(superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y].GetComponent<superPositionsData>().currentTiles[i]);
                }
            }
            int NxN = CalculateNxN(equalTiles.Count);
            RearrangeSuperPositionUI(superPositionUIObjs[(int)data.pos.x + 1][(int)data.pos.y], superPositionObjs[(int)data.pos.x + 1][(int)data.pos.y], NxN, equalTiles);
        }
        if (data.pos.y - 1 >= 0 && superPositionObjs[(int)data.pos.x][(int)data.pos.y-1].GetComponent<superPositionsData>().tile == null)
        {
            int tileCount = superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData>().currentTiles.Count;
            List<Tiles> equalTiles = new List<Tiles>();
            for (int i = 0; i < tileCount; i++)
            {
                if (superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData>().currentTiles[i].sockets.posY == data.tile.sockets.negY)
                {
                    equalTiles.Add(superPositionObjs[(int)data.pos.x][(int)data.pos.y - 1].GetComponent<superPositionsData>().currentTiles[i]);
                }
            }
            int NxN = CalculateNxN(equalTiles.Count);
            RearrangeSuperPositionUI(superPositionUIObjs[(int)data.pos.x][(int)data.pos.y-1], superPositionObjs[(int)data.pos.x][(int)data.pos.y-1], NxN, equalTiles);
        }
        if (data.pos.y + 1 < superPositionObjs.Length && superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData>().tile == null)
        {
            int tileCount = superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData>().currentTiles.Count;
            List<Tiles> equalTiles = new List<Tiles>();
            for (int i = 0; i < tileCount; i++)
            {
                if (superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData>().currentTiles[i].sockets.negY == data.tile.sockets.posY)
                {
                    equalTiles.Add(superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData>().currentTiles[i]);
                }
            }
            int NxN = CalculateNxN(equalTiles.Count);
            RearrangeSuperPositionUI(superPositionUIObjs[(int)data.pos.x][(int)data.pos.y + 1], superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1], NxN, equalTiles);
        }
        /*if (data.pos.y + 1 < superPositionObjs.Length && superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData>().tile == null)
        {
            for (int i = 0; i < superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData>().currentTiles.Count; i++)
            {
                if (superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData>().currentTiles[i].sockets.negY != data.tile.sockets.posY)
                {
                    superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData>().currentTiles.Remove(superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData>().currentTiles[i]);
                }
            }
            int NxN = CalculateNxN(superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData>().currentTiles.Count);
            RearrangeSuperPositionUI(superPositionUIObjs[(int)data.pos.x][(int)data.pos.y + 1], superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1], NxN, superPositionObjs[(int)data.pos.x][(int)data.pos.y + 1].GetComponent<superPositionsData>().currentTiles);
        }*/
        superPositionObj.GetComponent<SpriteRenderer>().sprite = EventSystem.current.currentSelectedGameObject.GetComponent<Image>().sprite;
        superPositionObj.transform.localScale = 16*Vector2.one;
        superPositionUIObjs[(int)data.pos.x][(int)data.pos.y].SetActive(false);
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
}