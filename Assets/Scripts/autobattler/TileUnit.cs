using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamfightTactics
{
    public class TileUnit : Pickupable
    {
        public Tile Tile { get; set; }

        public TileUnitData tileUnitData;

        public Transform characterPrefabParent;

        protected Animator _animator;
        protected CharacterAnimationEventCalls _characterAnimationEventCalls;
        
        string _key;
        public string Key
        {
            get
            {
                return _key;
            }

            set
            {
                _key = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            if (!characterPrefabParent)
                Debug.LogError("Character prefab parent is not set in the inspector");
        }

        public void SpawnCharacter()
        {
            if (tileUnitData && characterPrefabParent)
            {
                GameObject spawned = Instantiate(tileUnitData.characterPrefab, characterPrefabParent, false);
                characterPrefabParent.transform.ChangeLayersRecursively(gameObject.layer);

                _animator = GetComponentInChildren<Animator>();
                _characterAnimationEventCalls = GetComponentInChildren<CharacterAnimationEventCalls>();
            }
        }

        public void DeregisterTile()
        {
            if (Tile)
            {
                Tile.Remove(this);
                Tile = null;
            }
        }

        public void RegisterTile(Tile tile)
        {
            Tile = tile;
            AttachTo(Tile._attachPoint);
            Tile.Add(this);
            transform.rotation = Tile.transform.rotation;
        }

        void AttachTo(Transform tf)
        {
            Rigidbody.transform.position = tf.position;
            Rigidbody.velocity = Vector3.zero;
        }
    }
}
