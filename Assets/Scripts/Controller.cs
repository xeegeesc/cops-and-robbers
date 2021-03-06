using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
    private int[,] matriz = new int[Constants.NumTiles, Constants.NumTiles];
    private List<int> casillasPolis = new List<int>();

    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }

    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();
            }
        }

        cops[0].GetComponent<CopMove>().currentTile = Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile = Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile = Constants.InitialRobber;
    }


    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia

        Debug.Log("longitud" + matriz.GetLength(0));

        //TODO: Inicializar matriz a 0's

        for (int i = 0; i < matriz.GetLength(0); i++)
        {
            for (int j = 0; j < matriz.GetLength(1); j++)
            {
                matriz[i, j] = 0;
            }
        }

        //TODO: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha) 

        for (int i = 0; i < 64; i++)
        {
            if (i > 7) matriz[i, i - 8] = 1;//Tiene casilla abajo

            if (i < 56) matriz[i, i + 8] = 1;//Tiene casilla arriba

            if (i % 8 != 0) matriz[i, i - 1] = 1;//Tiene casilla a la izquierda

            if ((i + 1) % 8 != 0) matriz[i, i + 1] = 1;//Tiene casilla a la derecha

        }


        //TODO: Rellenar la lista "adyacencia" de cada casilla con los índices de sus casillas adyacentes
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++) //Menor que 64
            {
                if (matriz[i, j] == 1)
                {
                    tiles[i].adjacency.Add(j);

                }
            }
        }
    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        /*TODO: Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        - Movemos al caco a esa casilla
        - Actualizamos la variable currentTile del caco a la nueva casilla
        */
        robber.GetComponent<RobberMove>().MoveToTile(tiles[robber.GetComponent<RobberMove>().currentTile]);
    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {

        int idexCurrent;
        int otherCop = 99;

        if (cop == true)
        {
            idexCurrent = cops[clickedCop].GetComponent<CopMove>().currentTile;
        }
        else
        {
            idexCurrent = robber.GetComponent<RobberMove>().currentTile;
        }

        casillasPolis.Add(cops[0].GetComponent<CopMove>().currentTile);
        casillasPolis.Add(cops[1].GetComponent<CopMove>().currentTile);


        //La ponemos rosa porque acabamos de hacer un reset
        tiles[idexCurrent].current = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendrás que cambiar este código por el BFS

        foreach (int i in tiles[idexCurrent].adjacency)
        {
            tiles[i].selectable = true;
            if (tiles[i].numTile == casillasPolis[0] || tiles[i].numTile == casillasPolis[1])
            {
                otherCop = tiles[i].numTile;
            }
            foreach (int j in tiles[i].adjacency)
            {
                tiles[j].selectable = true;
            }
        }

        foreach (int tile in casillasPolis)
        {
            if (tiles[tile].selectable) tiles[tile].selectable = false;
        }

        if (otherCop != 99)
        {
            //Esta arriba
            if (otherCop == idexCurrent + 8) tiles[otherCop + 8].selectable = false;

            //Esta abajo
            if (otherCop == idexCurrent - 8) tiles[otherCop - 8].selectable = false;

            //Esta derecha
            if (otherCop == idexCurrent + 1) tiles[otherCop + 1].selectable = false;

            //Esta izquierda
            if (otherCop == idexCurrent - 1) tiles[otherCop - 1].selectable = false;

        }

        casillasPolis.Clear();
    }









}
