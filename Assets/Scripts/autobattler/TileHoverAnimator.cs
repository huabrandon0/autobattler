using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TeamfightTactics
{
    public class TileHoverAnimator : MonoBehaviour
    {
        public List<ParticleSystem> particleSystems;

        public void Play()
        {
            particleSystems.ForEach(x =>
            {
                if (x.isPlaying)
                    x.Stop();

                x.Play();
            });
        }

        public void Stop()
        {
            particleSystems.ForEach(x =>
            {
                if (x.isPlaying)
                    x.Stop();
            });
        }
    }
}
