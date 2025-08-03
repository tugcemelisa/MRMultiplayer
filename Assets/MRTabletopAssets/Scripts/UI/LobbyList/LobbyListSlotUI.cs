using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

namespace UnityEngine.XR.Templates.MRTTabletopAssets
{
    public class LobbyListSlotUI : MonoBehaviour
    {
        [SerializeField] TMP_Text m_RoomNameText;
        [SerializeField] TMP_Text m_PlayerCountText;
        [SerializeField] Button m_JoinButton;
        [SerializeField] GameObject m_FullImage;
        [SerializeField] TMP_Text m_StatusText;
        [SerializeField] GameObject m_JoinImage;

        LobbyUI m_LobbyListUI;
        Lobby m_Lobby;

        bool m_NonJoinable = false;

        void OnDisable()
        {
            ToggleHover(false);
        }

        public void CreateLobbyUI(Lobby lobby, LobbyUI lobbyListUI)
        {
            m_NonJoinable = false;
            m_Lobby = lobby;
            m_LobbyListUI = lobbyListUI;
            m_JoinButton.onClick.AddListener(JoinRoom);
            m_RoomNameText.text = lobby.Name;
            m_PlayerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
            m_JoinButton.interactable = m_Lobby.AvailableSlots <= 0;
            m_FullImage.SetActive(false);
            m_JoinImage.SetActive(false);
        }

        public void CreateNonJoinableLobbyUI(Lobby lobby, LobbyUI lobbyListUI, string statusText)
        {
            m_NonJoinable = true;
            m_JoinButton.interactable = false;
            m_Lobby = lobby;
            m_LobbyListUI = lobbyListUI;
            m_RoomNameText.text = lobby.Name;
            m_StatusText.text = statusText;
            m_FullImage.SetActive(true);
            m_JoinImage.SetActive(false);
        }

        public void ToggleHover(bool toggle)
        {
            if (m_NonJoinable) return;
            if (toggle)
            {
                if (m_Lobby != null && m_Lobby.AvailableSlots > 0)
                {
                    SetLobbyAvailable(true);
                }
                else
                {
                    SetLobbyAvailable(false);
                }
            }
            else
            {
                m_FullImage.SetActive(false);
                m_JoinImage.SetActive(false);
            }
        }

        void SetLobbyAvailable(bool available)
        {
            m_JoinImage.SetActive(available);
            m_FullImage.SetActive(!available);

            if (m_Lobby != null)
                m_JoinButton.interactable = available;
        }

        private void OnDestroy()
        {
            m_JoinButton.onClick.RemoveListener(JoinRoom);
        }

        void JoinRoom()
        {
            m_LobbyListUI.JoinLobby(m_Lobby);
        }
    }
}
