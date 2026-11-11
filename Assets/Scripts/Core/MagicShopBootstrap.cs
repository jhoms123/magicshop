using UnityEngine;

namespace CottageMagic
{
    public static class MagicShopBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateGame()
        {
            if (Object.FindObjectOfType<MagicShopDirector>() != null) return;

            var root = new GameObject("Cottage Keeper");
            var director = root.AddComponent<MagicShopDirector>();

            var cameraObject = new GameObject("Pixel Camera");
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(133, 166, 67, 255);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.allowMSAA = false;
            camera.allowHDR = false;
            cameraObject.AddComponent<PixelCameraRig>();

            var worldObject = new GameObject("World");
            worldObject.transform.SetParent(root.transform, false);
            var world = worldObject.AddComponent<WorldAssembler>();
            world.Build();
            director.World = world;

            var playerObject = new GameObject("Cottage Keeper");
            playerObject.transform.SetParent(root.transform, false);
            playerObject.transform.position = new Vector3(0, -0.3f, 0);
            var body = playerObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            playerObject.AddComponent<CircleCollider2D>();
            var player = playerObject.AddComponent<PlayerMotor>();
            player.Initialize(camera);
            director.Player = player;

            var uiObject = new GameObject("Pixel Art Shop UI");
            uiObject.transform.SetParent(root.transform, false);
            var ui = uiObject.AddComponent<MagicShopUI>();
            director.UI = ui;
            director.RestoreScene();
            ui.SetToast("Welcome home. Tend the garden, check the trade box, and meet the trader on market days.");
        }
    }

}
