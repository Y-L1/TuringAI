using System;
using System.Collections;
using Data;
using DG.Tweening;
using DragonLi.Core;
using DragonLi.Frame;
using DragonLi.Network;
using DragonLi.UI;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(WorldObjectRegister))]
    [RequireComponent(typeof(ReceiveMessageHandler))]
    [RequireComponent(typeof(DiceRecoverComponent))]
    [RequireComponent(typeof(RollDiceComponent))]
    [RequireComponent(typeof(PlayerCameraController))]
    public class ChessGameMode : GameMode, IMessageReceiver
    {
        public new static readonly string WorldObjectRegisterKey = "ChessGameMode";

        #region Debug

        [Space]
        
        [Header("Debug")]
        [SerializeField] public bool debugMode;
        
        [Header("Debug - ChanceEvent")]
        [SerializeField][Range(1f, 13f)] public int chanceEventId = 1;

        #endregion
        
        #region Properties

        private ChessGameBoard ChessGameBoard { get; set; }
        private DiceController DiceControllerRef { get; set; }
        
        public DragonLiCamera CameraRef { get; private set; }
        
        public RollDiceComponent RollDiceRef { get; private set; }
        
        public PlayerCameraController PlayerCameraControllerRef { get; private set; }
        
        public ChessGameCharacter CharacterRef { get; private set; }

        #endregion

        #region Unity
        
        private void Awake()
        {
            AudioManager.Instance.StopSound(1, 2f);
            AudioManager.Instance.PlaySound(0, AudioInstance.Instance.Settings.chessboard, SystemSandbox.Instance.VolumeHandler.Volume, 2.0f);
            RollDiceRef = GetComponent<RollDiceComponent>();
            GetComponent<ReceiveMessageHandler>().OnReceiveMessageHandler += OnReceiveMessage;
        }

        private void OnDestroy()
        {
            DisableCamera(null);
            if (!CharacterRef) return;
            CharacterRef.OnCharacterMoveStart -= DisableCamera;
            CharacterRef.OnCharacterMoveEnd -= EnableCamera;
            CharacterRef.OnCharacterJumpStart -= DisableCamera;
            CharacterRef.OnCharacterJumpEnd -= EnableCamera;
        }

        private IEnumerator Start()
        {
            while (!World.GetRegisteredObject<ChessGameMode>(WorldObjectRegisterKey))
            {
                yield return null;
            }
            
            while (!(ChessGameBoard = World.GetRegisteredObject<ChessGameBoard>(ChessGameBoard.WorldObjectRegisterKey)))
            {
                yield return null;
            }
            
            while (!(DiceControllerRef = World.GetRegisteredObject<DiceController>(DiceController.WorldObjectRegisterKey)))
            {
                yield return null;
            }

            while (!(CameraRef = World.GetMainCamera()?.GetComponent<DragonLiCamera>()))
            {
                yield return null;
            }
            
            PlayerCameraControllerRef = GetComponent<PlayerCameraController>();

            this.LogEditorOnly("检测与服务端的连接是否完成...");
            while (!GameSessionConnection.Instance.IsConnected())
            {
                yield return null;
            }

            yield return CoroutineTaskManager.Waits.QuarterSecond;
            
            this.LogEditorOnly("检测与服务端的连接完成");
            ChessGameBoard.InitializeChessBoard(PlayerSandbox.Instance.ChessBoardHandler.StandIndex);

            while (!(CharacterRef = World.GetPlayer<ChessGameCharacter>()))
            {
                yield return null;
            }
            
            if (PlayerSandbox.Instance.ChessBoardHandler.StandIndex == 0)
            {
                UIManager.Instance.GetLayer("UIBlackScreen").Hide();
                CameraRef.enabled = false;
                var cameraStart = World.GetRegisteredObject("Camera-Start");
                Debug.Assert(cameraStart != null, "cameraStart != null");
                CameraRef.transform.DOMove(cameraStart.transform.position, 5.0f).SetEase(Ease.InOutQuad);
                yield return new WaitForSeconds(5.0f);
                CameraRef.enabled = true;
            }
            else
            {
                yield return CoroutineTaskManager.Waits.QuarterSecond;
                UIManager.Instance.GetLayer("UIBlackScreen").Hide();
            }
            
            PlayerCameraControllerRef.SetupController();

            CharacterRef.OnCharacterMoveStart += DisableCamera;
            CharacterRef.OnCharacterMoveEnd += EnableCamera;
            CharacterRef.OnCharacterJumpStart += DisableCamera;
            CharacterRef.OnCharacterJumpEnd += EnableCamera;
            
            UIStaticsLayer.ShowUIStaticsLayer();
            UIActivityLayer.ShowUIActivityLayer();
            UIChessboardLayer.ShowLayer();
        }
        
        #endregion

        #region Functions

        private void StartRollingDice(int diceA, int diceB)
        {
            DiceControllerRef.SpawnDices(diceA, diceB);
            var step = diceA + diceB;
            Go(step);
        }

        private void EnableCamera(GameCharacter sender)
        {
            PlayerCameraControllerRef.SetControllerEnable(true);
        }

        private void DisableCamera(GameCharacter sender)
        {
            PlayerCameraControllerRef.SetControllerEnable(false);
            PlayerCameraControllerRef.SetOverrideTarget(null);
            PlayerCameraControllerRef.SetupCamera();
        }

        #endregion

        #region API
        

        private void Go(int step)
        {
            if (ChessGameBoard.IsProcessing())
            {
                return;
            }

            var tile = ChessGameBoard.GetTileByIndex(PlayerSandbox.Instance.ChessBoardHandler.StandIndex);
            ChessGameBoard.MoveCharacterForwards(step, tile.GetArriveAnimationType());
        }

        public void Jump(int toIndex)
        {
            if (ChessGameBoard.IsMoving())
            {
                return;
            }

            PlayerSandbox.Instance.ChessBoardHandler.StandIndex = toIndex;
            ChessGameBoard.TeleportCharacter(toIndex, ChessTile.EArriveAnimationType.Teleport);
        }

        private void Stay()
        {
            ChessGameBoard.Stay();
        }

        public void ModifyCoin(int coin, Vector3 snapLocation, Transform snapTo = null)
        {
            var layer = UIManager.Instance.GetLayer<UIWorldElementLayer>("UIWorldElementLayer");
            var wsCoin = layer.SpawnWorldElement<UIWSCoinNumber>(EffectInstance.Instance.Settings.uiEffectCoinNumber, snapLocation + Vector3.up * 3.5f, snapTo);
            wsCoin.SetCoinNumber(coin);
        }

        #endregion

        #region Callbacks

        public void OnReceiveMessage(HttpResponseProtocol response, string service, string method)
        {
            if (service != "chess" || method != "move") return;
            
            var diceA = response.GetAttachmentAsInt("a");
            var diceB = response.GetAttachmentAsInt("b");
            var finalTileIndex = response.GetAttachmentAsInt("stand");
            if (finalTileIndex == PlayerSandbox.Instance.ChessBoardHandler.StandIndex)
            {
                Stay();
            }
            else
            {
                StartRollingDice(diceA, diceB);
                PlayerSandbox.Instance.ChessBoardHandler.StandIndex = finalTileIndex;
            }
            
            PlayerSandbox.Instance.CharacterHandler.Dice--;
            PlayerSandbox.Instance.ObjectiveHandler.Daily.AddProgressDailyById("roll-dice", 1);
        }


        #endregion
    }
}


