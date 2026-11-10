using UnityEngine;

namespace CottageMagic
{
    public static class MagicShopBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateGame()
        {
            if (Object.FindObjectOfType<MagicShopDirector>() != null) return;

            var root = new GameObject("Magic Shop · Runtime");
            var director = root.AddComponent<MagicShopDirector>();

            var cameraObject = new GameObject("Pixel Camera"); cameraObject.transform.SetParent(root.transform, false); cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 6.45f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.23f, 0.28f, 0.22f);
            camera.nearClipPlane = 0.1f; camera.farClipPlane = 100f;

            var worldObject = new GameObject("World"); worldObject.transform.SetParent(root.transform, false);
            var world = worldObject.AddComponent<WorldAssembler>(); world.Build(); director.World = world;

            var playerObject = new GameObject("Witch Apprentice"); playerObject.transform.SetParent(root.transform, false);
            playerObject.transform.position = new Vector3(0, -0.3f, 0);
            var body = playerObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            playerObject.AddComponent<CircleCollider2D>();
            playerObject.AddComponent<SpriteRenderer>();
            var player = playerObject.AddComponent<PlayerMotor>(); player.Initialize(camera); director.Player = player;

            var uiObject = new GameObject("Shop UI"); uiObject.transform.SetParent(root.transform, false);
            var ui = uiObject.AddComponent<MagicShopUI>(); director.UI = ui;
            director.RestoreScene();
            ui.SetToast("Welcome to your little corner of the woods. Press E near anything to use it.");
        }
    }
}
