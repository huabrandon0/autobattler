using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TeamfightTactics
{
    public class Tile : Selectable
    {
        public HashSet<TileUnit> TileUnits { get; set; } = new HashSet<TileUnit>();

        public Transform _attachPoint;

        [SerializeField]
        Animator _animator;

        [SerializeField]
        ParticleSystem _pushParticleSystem;

        [SerializeField]
        TileHoverAnimator _tileHoverAnimator;

        bool _isBeingHovered;
        public bool IsBeingHovered
        {
            get
            {
                return _isBeingHovered;
            }
            set
            {
                _isBeingHovered = value;

                if (_isBeingHovered)
                    AnimateHover();
                else
                    StopAnimateHover();
            }
        }

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
                TileUnits.ToList().ForEach(x => x.Key = _key);
            }
        }

        public bool Enabled { get; set; } = true;

        void Awake()
        {
            if (!_attachPoint)
                Debug.LogError("Attach point is not set in the inspector");

            if (!_animator)
                Debug.LogWarning("Animator is not set in the inspector");

            if (!_pushParticleSystem)
                Debug.LogWarning("Push particle system is not set in the inspector");

            if (!_tileHoverAnimator)
                Debug.LogWarning("Tile hover animator is not set in the inspector");
        }
        
        public void Add(TileUnit tileUnit)
        {
            TileUnits.Add(tileUnit);
            AnimateAdd();
        }

        public bool Remove(TileUnit tileUnit)
        {
            return TileUnits.Remove(tileUnit);
        }

        void AnimateHover()
        {
            if (_tileHoverAnimator)
                _tileHoverAnimator.Play();
        }

        void StopAnimateHover()
        {
            if (_tileHoverAnimator)
                _tileHoverAnimator.Stop();
        }

        void AnimateAdd()
        {
            if (_animator)
                _animator.Play("TilePush", -1, 0f);
        }

        void OnPushParticleEffects()
        {
            if (_pushParticleSystem)
            {
                if (_pushParticleSystem.isPlaying)
                    _pushParticleSystem.Stop();

                _pushParticleSystem.Play();
            }
        }
    }
}
