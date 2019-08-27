using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TeamfightTactics
{
    public class Board : TileGroup
    {
        void OnEnable()
        {
            GameManager.Instance.RegisterBoard(this);
        }

        void OnDisable()
        {
            GameManager.Instance.DeregisterBoard(this);
        }
    }
    
    public class TileGroup : MonoBehaviour
    {
        public List<Tile> Tiles { get; set; }

        public List<Tile> EmptyTiles
        {
            get
            {
                return Tiles.Where(x => x.TileUnits.Count == 0).ToList();
            }
        }

        public List<Tile> OccupiedTiles
        {
            get
            {
                return Tiles.Where(x => x.TileUnits.Count != 0).ToList();
            }
        }
        
        [SerializeField]
        protected string _key;
        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
                Tiles.ForEach(x => x.Key = _key);
            }
        }

        void Awake()
        {
            ResolveTileReferences();
        }

        public void ResolveTileReferences()
        {
            Tiles = GetComponentsInChildren<Tile>().ToList();
            Tiles.ForEach(x => x.Key = _key);
        }
    }
}
