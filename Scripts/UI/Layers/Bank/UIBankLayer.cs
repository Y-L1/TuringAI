using System.Text.RegularExpressions;
using _Scripts.UI.Common;
using Data;
using DragonLi.Core;
using DragonLi.Network;
using DragonLi.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    [RequireComponent(typeof(ReceiveMessageHandler))]
    public class UIBankLayer : UILayer
    {
        #region Properties
        [Header("Settings")]
        [SerializeField] private Color colorNobody = Color.white;
        [SerializeField] private Color colorPerson = Color.green;
        [SerializeField] private Sprite iconNobody;
        [SerializeField] private Sprite iconPerson;
        

        [Header("References")] 
        [SerializeField] private TextMeshProUGUI tmpDeposit;
        [SerializeField] private TextMeshProUGUI tmpCode;
        [SerializeField] private UIInvitee[] invitees;

        [SerializeField] private GameObject objCopy;
        [SerializeField] private GameObject objOkay;
        private string InviteCode { get; set; }
        
        private FBankData Data { get; set; }

        #endregion

        #region UILayer

        protected override void OnInit()
        {
            base.OnInit();
            this["PasteBtn"].As<UIBasicButton>()?.OnClickEvent.AddListener(OnPasteClickCallback);
            this["BtnClose"].As<UIBasicButton>()?.OnClickEvent.AddListener(OnHideClickCallback);
            
            GetComponent<ReceiveMessageHandler>().OnReceiveMessageHandler += OnReceiveMessage;
        }

        protected override void OnShow()
        {
            base.OnShow();
            SetDeposit(PlayerSandbox.Instance.ChessBoardHandler.InvestCoin);
            SetCode(PlayerSandbox.Instance.ConnectionHandler.UserId);
            SetUserInfo(PlayerSandbox.Instance.ChessBoardHandler.Inviters);

            ActiveCopy();
        }

        #endregion

        #region Function

        private void ActiveCopy()
        {
            if (objCopy.activeSelf && !objCopy.activeSelf) return;
            objCopy.SetActive(true);
            objOkay.SetActive(false);
        }

        private void ActiveOkay(float autoDelayClose = -1f)
        {
            if(objOkay.activeSelf && !objOkay.activeSelf) return;
            objCopy.SetActive(false);
            objOkay.SetActive(true);

            if (autoDelayClose > 0)
            {
                CoroutineTaskManager.Instance.WaitSecondTodo(ActiveCopy, autoDelayClose);
            }
        }

        private void SetDeposit(decimal deposit)
        {
            Debug.Assert(tmpDeposit);
            tmpDeposit.text = $"{NumberUtils.GetDisplayNumberStringAsCurrency(deposit)}";
        }

        private void SetCode(string code)
        {
            Debug.Assert(tmpCode);
            InviteCode = code;
            var replaceString = $"{Regex.Replace(code, "[^a-zA-Z0-9]", "")[..10]}..";
            tmpCode.text = string.Format(this.GetLocalizedText("invite-code-fmt"), replaceString);
        }
        
        private void SetUserInfo(FBankData data)
        {
            Data = data;
            for (var i = 0; i < invitees.Length; i++)
            {
                if (i < data.GetInvites().Count)
                {
                    invitees[i].SetInvitee(data.GetInvites()[i], iconPerson);
                    invitees[i].SetColor(colorPerson);
                    continue;
                }
                
                invitees[i].SetInvitee(this.GetLocalizedText("invite"), iconNobody);
                invitees[i].SetColor(colorNobody);
            }
        }

        private bool CanWithdraw()
        {
            return Data.GetInvites().Count >= invitees.Length;
        }

        #endregion

        #region API

        public static UIBankLayer GetUIBankLayer()
        {
            var layer = UIManager.Instance.GetLayer<UIBankLayer>("UIBankLayer");
            Debug.Assert(layer);
            return layer;
        }

        public static void ShowLayer()
        {
            GetUIBankLayer()?.Show();
        }

        public static void HideLayer()
        {
            GetUIBankLayer()?.Hide();
        }

        #endregion

        #region Callback

        private void OnPasteClickCallback(UIBasicButton sender)
        {
            GUIUtility.systemCopyBuffer = InviteCode;
            ActiveOkay();
        }

        private void OnHideClickCallback(UIBasicButton sender)
        {
            Hide();
        }

        private void OnWithdrawClickCallback(UIBasicButton sender)
        {
            if (!CanWithdraw()) return;
            GameSessionAPI.ChessBoardAPI.WithdrawBank();
        }

        #endregion

        #region Callback - Socket Receiver

        private static void OnReceiveMessage(HttpResponseProtocol response, string service, string method)
        {
            if (GameSessionAPI.ChessBoardAPI.ServiceName == service && GSChessBoardAPI.MethodWithdrawBank == method)
            {
                var coin = response.GetAttachmentAsLong("coin");
                PlayerSandbox.Instance.CharacterHandler.Coin += (int)coin;
            }
        }

        #endregion
    }
}