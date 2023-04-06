using HarmonyLib;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;

namespace InvertLookControls
{
    public class InvertLookControls : ModBehaviour
    {
        public static InvertLookControls Instance;

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Instance = this;
        }

    }
    
    [HarmonyPatch]
    public class UpdateInversionPatch
    {
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BaseInputManager), nameof(BaseInputManager.UpdateInversion))]
        public static bool BaseInputManager_UpdateInversion_Prefix()
        {
            InputLibrary.pitch.InversionFactor = PlayerData.GetShipLookInversionFactor();
            InputLibrary.yaw.InversionFactor = PlayerData.GetShipLookInversionFactor();
            InputLibrary.look.InversionFactor = PlayerData.GetShipLookInversionFactor();
            InputLibrary.freeLook.InversionFactor = PlayerData.GetShipLookInversionFactor();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.UpdateTurning))]
        private static bool PlayerCharacterController_UpdateTurning_Prefix(PlayerCharacterController __instance)
        {
            float num = 1f;
            num *= __instance._playerCam.fieldOfView / __instance._initFOV;
            float num2 = OWInput.GetAxisValue(InputLibrary.look, InputMode.Character | InputMode.ScopeZoom | InputMode.NomaiRemoteCam).x * num * -1;
            __instance._lastTurnInput = num2;
            bool flag = Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer();
            float num3 = ((__instance._signalscopeZoom || flag) ? (PlayerCameraController.LOOK_RATE * PlayerCameraController.ZOOM_SCALAR) : PlayerCameraController.LOOK_RATE);
            Quaternion quaternion = Quaternion.AngleAxis(num2 * num3 * Time.fixedDeltaTime / Time.timeScale, __instance._transform.up);
            if (__instance._isGrounded && __instance._groundBody != null)
            {
                Vector3 vector = ((__instance._movingPlatform != null) ? __instance._movingPlatform.GetAngularVelocity() : __instance._groundBody.GetAngularVelocity());
                int num4 = (int)Mathf.Sign(Vector3.Dot(vector, __instance._transform.up));
                __instance._baseAngularVelocity = Vector3.Project(vector, __instance._transform.up).magnitude * (float)num4;
            }
            else
            {
                __instance._baseAngularVelocity *= 0.995f;
            }
            Quaternion quaternion2 = Quaternion.AngleAxis(__instance._baseAngularVelocity * 180f / 3.1415927f * Time.fixedDeltaTime, __instance._transform.up);
            __instance._transform.rotation = quaternion * quaternion2 * __instance._transform.rotation;

            return false;
        }
    }
    
}