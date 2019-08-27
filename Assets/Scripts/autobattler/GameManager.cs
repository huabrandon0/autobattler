using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

namespace TeamfightTactics
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField]
        TileUnit tileUnitPrefab;

        List<TileUnitData> TileUnitDatas { get; set; }

        [SerializeField]
        TMP_Text _helperText;

        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;

            DontDestroyOnLoad(gameObject);

            if (!tileUnitPrefab)
                Debug.LogError("Tile unit prefab is not set in the inspector");

            Resources.LoadAll<TileUnitData>("");
            TileUnitDatas = Resources.FindObjectsOfTypeAll<TileUnitData>().ToList();
            if (TileUnitDatas.Count == 0)
                Debug.Log("Could not find any tile unit data scriptable objects");
        }

        void Start()
        {
            RestartGame();
        }

        Dictionary<string, Hand> PlayerHandMap { get; set; } = new Dictionary<string, Hand>();
        Dictionary<string, Board> PlayerBoardMap { get; set; } = new Dictionary<string, Board>();

        public string _enemyID;
        public string _playerID;

        public void RegisterHand(Hand hand)
        {
            if (PlayerHandMap.ContainsValue(hand))
                return;

            PlayerHandMap.Add(hand.Key, hand);

            OnRegisterHand(hand);
        }

        public void DeregisterHand(Hand hand)
        {
            if (!PlayerHandMap.ContainsValue(hand))
                return;

            PlayerHandMap.Remove(hand.Key);
        }

        public void RegisterBoard(Board board)
        {
            if (PlayerBoardMap.ContainsValue(board))
                return;

            PlayerBoardMap.Add(board.Key, board);
        }

        public void DeregisterBoard(Board board)
        {
            if (!PlayerBoardMap.ContainsValue(board))
                return;

            PlayerBoardMap.Remove(board.Key);
        }

        public void DestroyUnit(TileUnit tileUnit)
        {
            if (!tileUnit)
                return;

            tileUnit.DeregisterTile();
            Destroy(tileUnit.gameObject);

            // Check if a player lost
            if (PlayerBoardMap.Values.Any(x => x.OccupiedTiles.Count == 0))
                GameEnd();
        }

        void GameEnd()
        {
            Board winnerBoard = PlayerBoardMap.Values.Where(x => x.OccupiedTiles.Count != 0).FirstOrDefault();
            if (winnerBoard != null)
            {
                string winner = PlayerBoardMap.Values.Where(x => x.OccupiedTiles.Count != 0).FirstOrDefault().Key;
                _helperText.SetText(winner + " won!");
            }
            MainSceneUI.Instance.EnableRestartGameButton();
        }

        void RandomizeEnemyBoard()
        {
            Board enemyBoard = PlayerBoardMap[_enemyID];
            Hand enemyHand = PlayerHandMap[_enemyID];

            while (enemyHand.OccupiedTiles.Count != 0 && enemyBoard.EmptyTiles.Count != 0)
            {
                TileUnit randUnit = enemyHand.OccupiedTiles.PickRandom().TileUnits.FirstOrDefault();
                Tile randTile = enemyBoard.EmptyTiles.PickRandom();
                randUnit.DeregisterTile();
                randUnit.RegisterTile(randTile);
            }
        }

        void WipeAllTileUnits()
        {
            PlayerBoardMap.Values.ToList().ForEach(x => x.Tiles.ForEach(y => y.TileUnits.ToList().ForEach(z => DestroyUnit(z))));
            PlayerHandMap.Values.ToList().ForEach(x => x.Tiles.ForEach(y => y.TileUnits.ToList().ForEach(z => DestroyUnit(z))));
        }
        
        public void StartGame()
        {
            if (PlayerBoardMap[_playerID].OccupiedTiles.Count == 0)
            {
                _helperText.SetText("Place at least one unit on your side of the board before readying up.");
                return;
            }

            RandomizeEnemyBoard();
            PlayerBoardMap.Values.ToList().ForEach(x => x.Key = x.Key);
            ActiveAttackUnits.ForEach(x => x.Enable());

            PlayerBoardMap.Values.ToList().ForEach(x => x.Tiles.ForEach(y => { y.Enabled = false; }));

            _helperText.SetText("Game in progress");
            MainSceneUI.Instance.DisableStartGameButton();
            MainSceneUI.Instance.DisableRestartGameButton();
        }

        public void RestartGame()
        {
            WipeAllTileUnits();
            PlayerHandMap.Values.ToList().ForEach(x => RandomizeHandUnits(x));
            PlayerHandMap.Values.ToList().ForEach(x => x.Tiles.ForEach(y => { y.Enabled = true; }));
            PlayerBoardMap.Values.ToList().ForEach(x => x.Tiles.ForEach(y => { y.Enabled = true; }));

            _helperText.SetText("Place your units. Ready up when all are placed.");
            MainSceneUI.Instance.EnableStartGameButton();
            MainSceneUI.Instance.DisableRestartGameButton();
        }

        public List<AttackUnit> ActiveAttackUnits
        {
            get
            {
                return AttackUnitManager.Instance.AttackUnits.Where(x => PlayerBoardMap.Values.Any(y => y.Tiles.Any(z => z.TileUnits.Contains(x)))).ToList();
            }
        }

        void RandomizeHandUnits(Hand hand)
        {
            foreach (Tile tile in hand.Tiles)
            {
                if (tileUnitPrefab && TileUnitDatas.Count > 0)
                {
                    TileUnit tileUnit = Instantiate(tileUnitPrefab, transform, false) as TileUnit;
                    tileUnit.tileUnitData = TileUnitDatas.PickRandom();
                    tileUnit.RegisterTile(tile);
                    tileUnit.SpawnCharacter();
                }
            }
        }

        void OnRegisterHand(Hand hand)
        {
            RandomizeHandUnits(hand);
        }

        public bool IsMyTile(Tile tile, string key)
        {
            PlayerHandMap.TryGetValue(key, out Hand hand);
            PlayerBoardMap.TryGetValue(key, out Board board);
            return (hand != null && hand.Tiles.Contains(tile)) || (board != null && board.Tiles.Contains(tile));
        }
    }
}
