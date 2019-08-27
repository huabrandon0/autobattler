using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TeamfightTactics
{
    public class MainSceneUI : MonoBehaviour
    {
        public static MainSceneUI Instance { get; private set; }

        [SerializeField]
        Button _startGameButton;
        
        [SerializeField]
        Button _restartGameButton;

        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        public void StartGame()
        {
            GameManager.Instance.StartGame();
        }

        public void RestartGame()
        {
            GameManager.Instance.RestartGame();
        }

        public void DisableStartGameButton()
        {
            _startGameButton.gameObject.SetActive(false);
        }

        public void EnableStartGameButton()
        {
            _startGameButton.gameObject.SetActive(true);
        }

        public void DisableRestartGameButton()
        {
            _restartGameButton.gameObject.SetActive(false);
        }
        
        public void EnableRestartGameButton()
        {
            _restartGameButton.gameObject.SetActive(true);
        }
    }
}
