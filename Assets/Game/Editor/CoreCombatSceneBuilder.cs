using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BAA.Editor
{
    public static class CoreCombatSceneBuilder
    {
        private const string PlayerConfigPath = "Assets/Game/Data/Player/PlayerConfig.asset";
        private const string ArrowConfigPath = "Assets/Game/Data/Projectiles/ArrowConfig.asset";
        private const string ArrowPrefabPath = "Assets/Game/Prefabs/Projectiles/Arrow.prefab";
        private const string DummyPrefabPath = "Assets/Game/Prefabs/Enemies/TargetDummy.prefab";
        private const string JoystickPrefabPath = "Assets/Game/Prefabs/UI/VirtualJoystick.prefab";
        private const string PlayerPrefabPath = "Assets/Game/Prefabs/Characters/Player.prefab";
        private const string ScenePath = "Assets/Game/Scenes/CoreCombat.unity";
        private const string EnemyLayerName = "Enemy";
        private const string ProjectileLayerName = "PlayerProjectile";

        [MenuItem("BAA/Build Core Combat Scene")]
        public static void Build()
        {
            EnsureOutputFolders();

            var enemyLayer = EnsureLayer(EnemyLayerName);
            var projectileLayer = EnsureLayer(ProjectileLayerName);
            ConfigureProjectileCollisions(projectileLayer, enemyLayer);

            var playerConfig = CreateOrUpdatePlayerConfig();
            var arrowConfig = CreateOrUpdateArrowConfig();
            var arrowPrefab = CreateArrowPrefab(projectileLayer);
            var dummyPrefab = CreateTargetDummyPrefab(enemyLayer);
            var joystickPrefab = CreateVirtualJoystickPrefab();
            var playerPrefab = CreatePlayerPrefab(
                playerConfig,
                arrowConfig,
                arrowPrefab,
                enemyLayer);

            CreateCoreCombatScene(playerPrefab, dummyPrefab, joystickPrefab, enemyLayer);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("phase 1 complete — playable single-room core combat vertical slice");
        }

        private static void EnsureOutputFolders()
        {
            EnsureFolder("Assets/Game/Data/Player");
            EnsureFolder("Assets/Game/Data/Projectiles");
            EnsureFolder("Assets/Game/Prefabs/Characters");
            EnsureFolder("Assets/Game/Prefabs/Enemies");
            EnsureFolder("Assets/Game/Prefabs/Projectiles");
            EnsureFolder("Assets/Game/Prefabs/UI");
            EnsureFolder("Assets/Game/Scenes");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var folderName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
            {
                throw new InvalidOperationException($"Invalid asset folder path: {path}");
            }

            EnsureFolder(parent);
            if (string.IsNullOrEmpty(AssetDatabase.CreateFolder(parent, folderName)))
            {
                throw new InvalidOperationException($"Could not create asset folder: {path}");
            }
        }

        private static int EnsureLayer(string layerName)
        {
            var tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagManagerAssets.Length == 0)
            {
                throw new InvalidOperationException("Could not load ProjectSettings/TagManager.asset.");
            }

            var tagManager = new SerializedObject(tagManagerAssets[0]);
            var layers = RequireProperty(tagManager, "layers");
            for (var i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    return i;
                }
            }

            for (var i = 8; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(layer.stringValue))
                {
                    continue;
                }

                layer.stringValue = layerName;
                tagManager.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                return i;
            }

            throw new InvalidOperationException($"No free user layer is available for '{layerName}'.");
        }

        private static void ConfigureProjectileCollisions(int projectileLayer, int enemyLayer)
        {
            for (var layer = 0; layer < 32; layer++)
            {
                Physics.IgnoreLayerCollision(projectileLayer, layer, layer != enemyLayer);
            }
        }

        private static PlayerConfig CreateOrUpdatePlayerConfig()
        {
            var config = LoadOrCreateAsset<PlayerConfig>(PlayerConfigPath);
            config.MaxHealth = 100f;
            config.MoveSpeed = 5f;
            config.Attack = 10f;
            config.AttackInterval = 0.6f;
            config.TargetRange = 10f;
            config.InputDeadZone = 0.1f;
            config.SettleDuration = 0.1f;
            EditorUtility.SetDirty(config);
            return config;
        }

        private static ProjectileConfig CreateOrUpdateArrowConfig()
        {
            var config = LoadOrCreateAsset<ProjectileConfig>(ArrowConfigPath);
            config.Speed = 16f;
            config.Lifetime = 3f;
            EditorUtility.SetDirty(config);
            return config;
        }

        private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static GameObject CreateArrowPrefab(int projectileLayer)
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                root.name = "Arrow";
                SetLayerRecursively(root, projectileLayer);
                root.transform.localScale = new Vector3(0.16f, 0.16f, 0.7f);
                root.GetComponent<BoxCollider>().isTrigger = true;

                var body = root.AddComponent<Rigidbody>();
                body.useGravity = false;
                body.isKinematic = true;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                root.AddComponent<Projectile>();

                return SavePrefab(root, ArrowPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateTargetDummyPrefab(int enemyLayer)
        {
            var root = new GameObject("TargetDummy");
            try
            {
                SetLayerRecursively(root, enemyLayer);
                var health = root.AddComponent<CharacterHealth>();
                var healthData = new SerializedObject(health);
                RequireProperty(healthData, "initialMaxHealth").floatValue = 50f;
                healthData.ApplyModifiedPropertiesWithoutUndo();

                var model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                model.name = "Model";
                model.transform.SetParent(root.transform, false);
                model.transform.localPosition = Vector3.up;
                SetLayerRecursively(model, enemyLayer);
                model.GetComponent<CapsuleCollider>().isTrigger = false;

                return SavePrefab(root, DummyPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateVirtualJoystickPrefab()
        {
            var root = new GameObject(
                "JoystickBackground",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(VirtualJoystick));
            try
            {
                var background = root.GetComponent<RectTransform>();
                background.sizeDelta = new Vector2(220f, 220f);
                var backgroundImage = root.GetComponent<Image>();
                backgroundImage.color = new Color(0.08f, 0.08f, 0.08f, 0.45f);
                backgroundImage.raycastTarget = true;

                var handleObject = new GameObject(
                    "Handle",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                var handle = handleObject.GetComponent<RectTransform>();
                handle.SetParent(background, false);
                handle.anchorMin = new Vector2(0.5f, 0.5f);
                handle.anchorMax = new Vector2(0.5f, 0.5f);
                handle.pivot = new Vector2(0.5f, 0.5f);
                handle.anchoredPosition = Vector2.zero;
                handle.sizeDelta = new Vector2(90f, 90f);
                var handleImage = handleObject.GetComponent<Image>();
                handleImage.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
                handleImage.raycastTarget = false;

                var joystick = root.GetComponent<VirtualJoystick>();
                var joystickData = new SerializedObject(joystick);
                RequireProperty(joystickData, "background").objectReferenceValue = background;
                RequireProperty(joystickData, "handle").objectReferenceValue = handle;
                RequireProperty(joystickData, "deadZone").floatValue = 0.1f;
                joystickData.ApplyModifiedPropertiesWithoutUndo();

                return SavePrefab(root, JoystickPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreatePlayerPrefab(
            PlayerConfig playerConfig,
            ProjectileConfig arrowConfig,
            GameObject arrowPrefab,
            int enemyLayer)
        {
            var root = new GameObject("Player");
            try
            {
                var controller = root.AddComponent<CharacterController>();
                controller.center = Vector3.up;
                controller.radius = 0.45f;
                controller.height = 2f;
                controller.stepOffset = 0.3f;

                var model = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                model.name = "Model";
                model.transform.SetParent(root.transform, false);
                model.transform.localPosition = Vector3.up;
                UnityEngine.Object.DestroyImmediate(model.GetComponent<Collider>());

                var firePointObject = new GameObject("FirePoint");
                firePointObject.transform.SetParent(root.transform, false);
                firePointObject.transform.localPosition = new Vector3(0f, 1f, 0.8f);

                var input = root.AddComponent<PlayerInputReader>();
                var movement = root.AddComponent<PlayerMovement>();
                var targetFinder = root.AddComponent<TargetFinder>();
                var weapon = root.AddComponent<WeaponController>();
                var pool = root.AddComponent<ProjectilePool>();
                var health = root.AddComponent<CharacterHealth>();
                var combat = root.AddComponent<PlayerCombat>();

                var inputData = new SerializedObject(input);
                RequireProperty(inputData, "deadZone").floatValue = 0.1f;
                inputData.ApplyModifiedPropertiesWithoutUndo();

                var movementData = new SerializedObject(movement);
                RequireProperty(movementData, "controller").objectReferenceValue = controller;
                RequireProperty(movementData, "input").objectReferenceValue = input;
                RequireProperty(movementData, "rotationSpeed").floatValue = 720f;
                movementData.ApplyModifiedPropertiesWithoutUndo();

                var targetData = new SerializedObject(targetFinder);
                RequireProperty(targetData, "targetMask").intValue = 1 << enemyLayer;
                RequireProperty(targetData, "range").floatValue = 10f;
                targetData.ApplyModifiedPropertiesWithoutUndo();

                var poolData = new SerializedObject(pool);
                RequireProperty(poolData, "prefab").objectReferenceValue =
                    arrowPrefab.GetComponent<Projectile>();
                poolData.ApplyModifiedPropertiesWithoutUndo();

                var weaponData = new SerializedObject(weapon);
                RequireProperty(weaponData, "firePoint").objectReferenceValue = firePointObject.transform;
                RequireProperty(weaponData, "pool").objectReferenceValue = pool;
                RequireProperty(weaponData, "projectileConfig").objectReferenceValue = arrowConfig;
                weaponData.ApplyModifiedPropertiesWithoutUndo();

                var healthData = new SerializedObject(health);
                RequireProperty(healthData, "initialMaxHealth").floatValue = 100f;
                healthData.ApplyModifiedPropertiesWithoutUndo();

                var combatData = new SerializedObject(combat);
                RequireProperty(combatData, "config").objectReferenceValue = playerConfig;
                RequireProperty(combatData, "input").objectReferenceValue = input;
                RequireProperty(combatData, "movement").objectReferenceValue = movement;
                RequireProperty(combatData, "targetFinder").objectReferenceValue = targetFinder;
                RequireProperty(combatData, "weapon").objectReferenceValue = weapon;
                RequireProperty(combatData, "health").objectReferenceValue = health;
                RequireProperty(combatData, "rotationSpeed").floatValue = 720f;
                combatData.ApplyModifiedPropertiesWithoutUndo();

                return SavePrefab(root, PlayerPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void CreateCoreCombatScene(
            GameObject playerPrefab,
            GameObject dummyPrefab,
            GameObject joystickPrefab,
            int enemyLayer)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateRoom();
            CreateLighting();
            CreateCamera();

            var player = InstantiatePrefab(playerPrefab, "Player", new Vector3(0f, 0f, -4f));
            var dummyPositions = new[]
            {
                new Vector3(-4f, 0f, 3f),
                new Vector3(0f, 0f, 5f),
                new Vector3(4f, 0f, 3f)
            };
            for (var i = 0; i < dummyPositions.Length; i++)
            {
                var dummy = InstantiatePrefab(
                    dummyPrefab,
                    $"TargetDummy_{i + 1}",
                    dummyPositions[i]);
                SetLayerRecursively(dummy, enemyLayer);
            }

            var joystick = CreateUi(joystickPrefab);
            var inputData = new SerializedObject(player.GetComponent<PlayerInputReader>());
            RequireProperty(inputData, "virtualJoystick").objectReferenceValue = joystick;
            inputData.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Could not save scene: {ScenePath}");
            }
        }

        private static void CreateRoom()
        {
            CreateCube("Floor", new Vector3(0f, -0.1f, 0f), new Vector3(16f, 0.2f, 24f));
            CreateCube("Wall_Left", new Vector3(-8.25f, 1.5f, 0f), new Vector3(0.5f, 3f, 24.5f));
            CreateCube("Wall_Right", new Vector3(8.25f, 1.5f, 0f), new Vector3(0.5f, 3f, 24.5f));
            CreateCube("Wall_Back", new Vector3(0f, 1.5f, -12.25f), new Vector3(17f, 3f, 0.5f));
            CreateCube("Wall_Front", new Vector3(0f, 1.5f, 12.25f), new Vector3(17f, 3f, 0.5f));
        }

        private static void CreateLighting()
        {
            var lightObject = new GameObject("Directional Light");
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(
                new Vector3(0f, 18f, -12.6f),
                Quaternion.Euler(55f, 0f, 0f));
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 16f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.15f, 0.18f);
            cameraObject.AddComponent<AudioListener>();
        }

        private static VirtualJoystick CreateUi(GameObject joystickPrefab)
        {
            var canvasObject = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var joystickObject = PrefabUtility.InstantiatePrefab(
                joystickPrefab,
                canvasObject.transform) as GameObject;
            if (joystickObject == null)
            {
                throw new InvalidOperationException("Could not instantiate VirtualJoystick prefab.");
            }

            joystickObject.name = "VirtualJoystick";
            var joystickRect = joystickObject.GetComponent<RectTransform>();
            joystickRect.anchorMin = Vector2.zero;
            joystickRect.anchorMax = Vector2.zero;
            joystickRect.pivot = new Vector2(0.5f, 0.5f);
            joystickRect.anchoredPosition = new Vector2(170f, 170f);
            joystickRect.sizeDelta = new Vector2(220f, 220f);

            new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            return joystickObject.GetComponent<VirtualJoystick>();
        }

        private static GameObject CreateCube(string name, Vector3 position, Vector3 scale)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = position;
            cube.transform.localScale = scale;
            return cube;
        }

        private static GameObject InstantiatePrefab(GameObject prefab, string name, Vector3 position)
        {
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"Could not instantiate prefab: {prefab.name}");
            }

            instance.name = name;
            instance.transform.SetPositionAndRotation(position, Quaternion.identity);
            return instance;
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path, out var success);
            if (!success || prefab == null)
            {
                throw new InvalidOperationException($"Could not save prefab: {path}");
            }

            return prefab;
        }

        private static SerializedProperty RequireProperty(SerializedObject owner, string name)
        {
            var property = owner.FindProperty(name);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Serialized field '{name}' was not found on {owner.targetObject.name}.");
            }

            return property;
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
