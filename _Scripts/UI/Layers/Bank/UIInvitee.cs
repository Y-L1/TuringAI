using DragonLi.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace Game
{
    public class UIInvitee : MonoBehaviour
    {
        #region Properties

        [Header("References")]
        [SerializeField] private Image imgAvatar;
        [SerializeField] private TextMeshProUGUI tmpInviteeName;
        [SerializeField] private Button btnInvitee;

        private string InviteeName { get; set; } = null;
        
        #endregion

        #region Unity

        private void Awake()
        {
            btnInvitee.onClick.AddListener(OnClickInviteeButton);
        }

        #endregion

        #region API

        public void SetInvitee(string inviteeName, Sprite inviteeAvatar)
        {
            imgAvatar.sprite = inviteeAvatar;
            tmpInviteeName.text = inviteeName;
        }

        public void SetColor(Color color)
        {
            foreach (var img in GetComponentsInChildren<Image>())
            {
                img.color = color;
            }
            tmpInviteeName.color = color;
        }

        #endregion

        #region Callback

        private void OnClickInviteeButton()
        {
            if (InviteeName.IsNullOrEmpty())
            {
                UITipLayer.DisplayTip(this.GetLocalizedText("bank"), this.GetLocalizedText("invite-code-copy-tip-des"));
            }
        }

        #endregion
    }
}