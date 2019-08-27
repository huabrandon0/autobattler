using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TeamfightTactics
{
    public class PlayerController : MonoBehaviour
    {
        public string _key;
        
        bool _interact = false;

        [System.Serializable]
        struct PickupData
        {
            public TileUnit tileUnit;
            public Vector3 offset;
            public Tile tile;

            public PickupData(TileUnit tileUnit, Vector3 offset, Tile tile)
            {
                this.tileUnit = tileUnit;
                this.offset = offset;
                this.tile = tile;
            }
        }
        
        PickupData? _pickedUpTileUnit = null;

        [SerializeField]
        AnimationCurve _velocityCurve;
        
        float _distance = float.MaxValue;

        [SerializeField]
        Camera _camera;

        [SerializeField]
        LayerMask _tileUnitMask;

        [SerializeField]
        LayerMask _tileMask;

        [SerializeField]
        LayerMask _selectableSurfaceMask;
        
        [SerializeField]
        Vector3 _pickupOffset = Vector3.up;

        Vector3 _offsetBase;

        [SerializeField]
        float _maxVelocityDistance = 10f;

        [SerializeField]
        float _maxVelocity = 50f;

        HashSet<Tile> _hoveredTiles = new HashSet<Tile>();
        
        void Awake()
        {
            if (_velocityCurve == null)
                Debug.LogError("Velocity curve is not set in the inspector");
        }

        void Update()
        {
            _interact = Input.GetKeyDown(KeyCode.Mouse0);

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            // Move picked up tile unit to follow mouse cursor
            if (_pickedUpTileUnit != null)
            {
                _offsetBase = CalculateOffsetBase(ray, _distance, _selectableSurfaceMask, _offsetBase);

                Vector3 desiredPos = _offsetBase + _pickedUpTileUnit.Value.offset;
                Vector3 diff = desiredPos - _pickedUpTileUnit.Value.tileUnit.transform.position;

                _pickedUpTileUnit.Value.tileUnit.Rigidbody.velocity = diff.normalized * _velocityCurve.Evaluate(diff.magnitude / _maxVelocityDistance) * _maxVelocity;
            }

            if (_pickedUpTileUnit != null)
                HandleWhilePickedUpSelectables(ray);
            else
                HandleWhileNoPickedUpSelectables(ray);
        }

        void HandleWhilePickedUpSelectables(Ray ray)
        {
            if (_pickedUpTileUnit == null)
                return;

            Tile tile = RaycastSelectable<Tile>(ray, _tileMask);

            if (tile && GameManager.Instance.IsMyTile(tile, _key) && tile.Enabled)
            {
                List<Tile> hoveredTilesToRemove = new List<Tile>();

                foreach (Tile hoveredTile in _hoveredTiles)
                {
                    if (hoveredTile != tile)
                        hoveredTilesToRemove.Add(hoveredTile);
                }

                _hoveredTiles.Where(x => hoveredTilesToRemove.Contains(x)).ToList().ForEach(x => RemoveFromHoveredTiles(x));

                if (!_hoveredTiles.Contains(tile))
                    AddToHoveredTiles(tile);

                if (_interact)
                {
                    if (tile.TileUnits.Count > 0)
                    {
                        TileUnit toPickupAfter = tile.TileUnits.FirstOrDefault();

                        _pickedUpTileUnit.Value.tileUnit.RegisterTile(tile);
                        _pickedUpTileUnit = null;

                        PickupTileUnit(toPickupAfter, ray, _distance, _selectableSurfaceMask, toPickupAfter.Rigidbody.position);
                    }
                    else
                    {
                        _pickedUpTileUnit.Value.tileUnit.RegisterTile(tile);
                        _pickedUpTileUnit = null;
                    }
                }
            }
            else
            {
                ClearHoveredTiles();
                
                if (_interact)
                {
                    if (_pickedUpTileUnit.Value.tile)
                        _pickedUpTileUnit.Value.tileUnit.RegisterTile(_pickedUpTileUnit.Value.tile);

                    _pickedUpTileUnit = null;
                }
            }
        }

        void HandleWhileNoPickedUpSelectables(Ray ray)
        {
            if (_pickedUpTileUnit != null)
                return;

            ClearHoveredTiles();

            Tile tile = RaycastSelectable<Tile>(ray, _tileMask);

            if (tile && GameManager.Instance.IsMyTile(tile, _key) && tile.Enabled)
            {
                TileUnit tileUnit = tile.TileUnits.FirstOrDefault();

                if (tileUnit)
                {
                    if (_interact)
                    {
                        PickupTileUnit(tileUnit, ray, _distance, _selectableSurfaceMask, tileUnit.Rigidbody.position);
                    }
                }
            }
        }

        void PickupTileUnit(TileUnit tileUnit, Ray ray, float distance, LayerMask selectableSurfaceMask, Vector3 backupOffsetBase)
        {
            _offsetBase = CalculateOffsetBase(ray, _distance, _selectableSurfaceMask, tileUnit.Rigidbody.position);
            Vector3 offset = tileUnit.Rigidbody.position - _offsetBase + _pickupOffset;
            _pickedUpTileUnit = new PickupData(tileUnit, offset, tileUnit.Tile);
            tileUnit.DeregisterTile();
        }

        void AddToHoveredTiles(Tile tile)
        {
            if (_hoveredTiles.Contains(tile) || !tile || !tile.Enabled)
                return;

            _hoveredTiles.Add(tile);
            tile.IsBeingHovered = true;
        }

        void RemoveFromHoveredTiles(Tile tile)
        {
            if (!_hoveredTiles.Contains(tile) || !tile)
                return;

            _hoveredTiles.Remove(tile);
            tile.IsBeingHovered = false;
        }

        void ClearHoveredTiles()
        {
            if (_hoveredTiles.Count == 0)
                return;

            foreach (Tile tile in _hoveredTiles)
                tile.IsBeingHovered = false;

            _hoveredTiles.Clear();
        }

        T RaycastSelectable<T>(Ray ray, LayerMask layerMask) where T : Selectable
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, _distance, layerMask))
            {
                T t = hitInfo.collider.GetComponentInParent<T>();
                return t;
            }

            return null;
        }

        // Calculates an offset base with the following preferences:
        //   1) Set offsetBase to the hit point of the raycast to a selectable surface
        //   2) Set offsetBase to the hit point of the raycast to a horizontal plane that backupOffsetBase lies on.
        //   3) If 1 and 2 fail, set offsetBase to backupOffsetBase. (It is the backup offset base after all.)
        Vector3 CalculateOffsetBase(Ray ray, float distance, LayerMask selectableSurfaceMask, Vector3 backupOffsetBase)
        {
            // Initially try to raycast to see if you can hit a selectable surface.
            if (Physics.Raycast(ray, out RaycastHit hitInfo, distance, selectableSurfaceMask))
            {
                return hitInfo.point;
            }
            else
            {
                // Create an "imaginary" selectable surface that is a horizontal plane going through the backup offset base.
                Plane plane = new Plane(Vector3.up, backupOffsetBase);

                if (plane.Raycast(ray, out float enter))
                    return ray.GetPoint(enter);
            }

            // Just use the backup offset base, if we can't calculate one.
            return backupOffsetBase;
        }
    }
}
