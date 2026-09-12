using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>Project XP의 플레이 가능한 첫 수직 슬라이스를 런타임에 조립한다.</summary>
public sealed class ProjectXpPrototype : MonoBehaviour
{
    private PrototypeGameState state;
    private PrototypeHudView hud;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        state = new PrototypeGameState();
        BuildCamera();
        hud = BuildHud();
        BuildWorld();
        state.Changed += hud.Refresh;
        state.Changed += SaveProgress;
        state.Restore();
        hud.Refresh(state);
        hud.ShowDialogue("마을 수호대", "마왕의 기운이 가까워지고 있어요.\n[A / D] 이동, [Space] 점프, [Left Shift] 회피, [F] 공격");
    }

    private void BuildCamera()
    {
        Camera camera = Camera.main;
        camera.orthographic = true;
        camera.orthographicSize = 5.6f;
        camera.backgroundColor = new Color(0.055f, 0.12f, 0.21f);
        camera.gameObject.AddComponent<PrototypeCameraFollow>();
    }

    private void BuildWorld()
    {
        CreateSprite("SkyGlow", new Vector2(8f, 1.5f), new Vector2(28f, 8f), new Color(0.12f, 0.28f, 0.43f), -5);
        CreatePlatform("Village Ground", new Vector2(0f, -4.25f), new Vector2(30f, 1f), new Color(0.14f, 0.22f, 0.22f));
        CreatePlatform("Bridge", new Vector2(5.3f, -2.2f), new Vector2(4f, .45f), new Color(0.23f, 0.48f, 0.38f));
        CreatePlatform("Rooftop", new Vector2(10.2f, -0.65f), new Vector2(3.4f, .45f), new Color(0.23f, 0.48f, 0.38f));
        CreatePlatform("Cliff", new Vector2(15.5f, -2.7f), new Vector2(5.5f, .45f), new Color(0.23f, 0.48f, 0.38f));
        CreatePlatform("Boss Ground", new Vector2(22f, -4.25f), new Vector2(16f, 1f), new Color(0.21f, 0.14f, 0.24f));

        GameObject player = CreateSprite("Hero", new Vector2(-10f, -2.8f), new Vector2(.72f, 1.15f), new Color(0.4f, 0.86f, 1f), 2);
        player.AddComponent<BoxCollider2D>();
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 3.2f;
        body.freezeRotation = true;
        PrototypePlayer controller = player.AddComponent<PrototypePlayer>();
        controller.Initialize(state, hud);

        CreateEnemy("Shadow Scout", new Vector2(1.4f, -3f), false);
        CreateEnemy("Shadow Hunter", new Vector2(11.1f, .1f), false);
        CreateEnemy("Demon King", new Vector2(22f, -2.8f), true);
        CreatePickup("Cloak of Wind", new Vector2(6.2f, -1.35f), PrototypePickup.Kind.Cloak);
        CreatePickup("Iron Armor", new Vector2(16f, -1.8f), PrototypePickup.Kind.Armor);
        CreateGate(new Vector2(18.3f, -2.7f));

        CreateWorldLabel("NEWLAND 2019", new Vector2(-10f, 2.9f), new Color(0.75f, 0.92f, 1f));
        CreateWorldLabel("마을 외곽", new Vector2(1.5f, .2f), new Color(0.85f, 0.95f, .75f));
        CreateWorldLabel("마왕의 성채", new Vector2(21.5f, 1.3f), new Color(1f, .65f, .7f));
    }

    private void CreatePlatform(string objectName, Vector2 position, Vector2 size, Color color)
    {
        GameObject platform = CreateSprite(objectName, position, size, color, 0);
        platform.AddComponent<BoxCollider2D>();
    }

    private void CreateEnemy(string objectName, Vector2 position, bool boss)
    {
        GameObject enemy = CreateSprite(objectName, position, boss ? new Vector2(1.25f, 1.75f) : new Vector2(.8f, 1.1f), boss ? new Color(.72f, .18f, .31f) : new Color(.74f, .35f, .58f), 2);
        enemy.AddComponent<BoxCollider2D>();
        Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
        body.gravityScale = 3.2f;
        body.freezeRotation = true;
        PrototypeEnemy script = enemy.AddComponent<PrototypeEnemy>();
        script.Initialize(state, hud, boss ? 7 : 3, boss ? 2.3f : 1.25f);
    }

    private void CreatePickup(string objectName, Vector2 position, PrototypePickup.Kind kind)
    {
        GameObject pickup = CreateSprite(objectName, position, new Vector2(.52f, .52f), kind == PrototypePickup.Kind.Cloak ? new Color(.98f, .78f, .2f) : new Color(.5f, .75f, 1f), 3);
        CircleCollider2D collider = pickup.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        PrototypePickup script = pickup.AddComponent<PrototypePickup>();
        script.Initialize(kind, state, hud);
    }

    private void CreateGate(Vector2 position)
    {
        GameObject gate = CreateSprite("Stage Gate", position, new Vector2(.3f, 3.1f), new Color(.52f, .26f, .86f, .8f), 1);
        BoxCollider2D collider = gate.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        PrototypeStageGate script = gate.AddComponent<PrototypeStageGate>();
        script.Initialize(state, hud);
    }

    private static GameObject CreateSprite(string objectName, Vector2 position, Vector2 size, Color color, int order)
    {
        GameObject item = new GameObject(objectName);
        item.transform.position = position;
        item.transform.localScale = size;
        SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f));
        renderer.color = color;
        renderer.sortingOrder = order;
        return item;
    }

    private static void CreateWorldLabel(string message, Vector2 position, Color color)
    {
        GameObject item = new GameObject(message);
        item.transform.position = position;
        TextMesh text = item.AddComponent<TextMesh>();
        text.text = message;
        text.fontSize = 42;
        text.characterSize = .12f;
        text.anchor = TextAnchor.MiddleCenter;
        text.color = color;
        text.GetComponent<MeshRenderer>().sortingOrder = 4;
    }

    private PrototypeHudView BuildHud()
    {
        GameObject canvasObject = new GameObject("Prototype HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = .5f;
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        PrototypeHudView view = canvasObject.AddComponent<PrototypeHudView>();
        view.Build();
        return view;
    }

    private void SaveProgress(PrototypeGameState gameState)
    {
        PlayerPrefs.SetInt("ProjectXpCloak", gameState.HasCloak ? 1 : 0);
        PlayerPrefs.SetInt("ProjectXpArmor", gameState.HasArmor ? 1 : 0);
        PlayerPrefs.SetInt("ProjectXpStage", gameState.Stage);
        PlayerPrefs.Save();
    }
}

public sealed class PrototypeGameState
{
    public event Action<PrototypeGameState> Changed;
    public int Hearts { get; private set; } = 5;
    public int MaxHearts { get; private set; } = 5;
    public bool HasCloak { get; private set; }
    public bool HasArmor { get; private set; }
    public int Stage { get; private set; } = 1;

    public void Restore()
    {
        HasCloak = PlayerPrefs.GetInt("ProjectXpCloak", 0) == 1;
        HasArmor = PlayerPrefs.GetInt("ProjectXpArmor", 0) == 1;
        Stage = Mathf.Max(1, PlayerPrefs.GetInt("ProjectXpStage", 1));
        if (HasArmor) MaxHearts = 6;
        Hearts = MaxHearts;
        Notify();
    }

    public void Damage(int amount)
    {
        int reduced = HasArmor ? Mathf.Max(1, amount - 1) : amount;
        Hearts = Mathf.Max(0, Hearts - reduced);
        Notify();
    }

    public void HealFull() { Hearts = MaxHearts; Notify(); }
    public void GainCloak() { HasCloak = true; Notify(); }
    public void GainArmor() { HasArmor = true; MaxHearts = 6; Hearts = MaxHearts; Notify(); }
    public void ReachStageTwo() { Stage = Mathf.Max(Stage, 2); Notify(); }
    private void Notify() => Changed?.Invoke(this);
}

public sealed class PrototypePlayer : MonoBehaviour
{
    private PrototypeGameState state;
    private PrototypeHudView hud;
    private Rigidbody2D body;
    private SpriteRenderer sprite;
    private bool grounded;
    private bool facingRight = true;
    private float dashUntil;
    private float invincibleUntil;
    private float attackUntil;

    public void Initialize(PrototypeGameState gameState, PrototypeHudView hudView) { state = gameState; hud = hudView; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        float move = (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? -1f : 0f) + (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f);
        float speed = state.HasCloak ? 7.5f : 5.5f;
        if (Time.time < dashUntil) speed *= 1.8f;
        body.linearVelocity = new Vector2(move * speed, body.linearVelocity.y);
        if (move != 0) { facingRight = move > 0; sprite.flipX = !facingRight; }
        if (grounded && keyboard.spaceKey.wasPressedThisFrame) body.linearVelocity = new Vector2(body.linearVelocity.x, 10.3f);
        if (keyboard.leftShiftKey.wasPressedThisFrame && Time.time >= dashUntil)
        {
            dashUntil = Time.time + .18f;
            invincibleUntil = Time.time + .28f;
            body.linearVelocity = new Vector2((facingRight ? 1f : -1f) * speed * 1.8f, body.linearVelocity.y);
            hud.ShowToast("회피! 짧은 무적 상태입니다.");
        }
        if (keyboard.fKey.wasPressedThisFrame && Time.time >= attackUntil)
        {
            attackUntil = Time.time + .35f;
            PrototypeEnemy[] enemies = FindObjectsByType<PrototypeEnemy>(FindObjectsSortMode.None);
            foreach (PrototypeEnemy enemy in enemies)
            {
                Vector2 delta = enemy.transform.position - transform.position;
                if (Mathf.Abs(delta.x) < 1.55f && Mathf.Abs(delta.y) < 1.35f && Mathf.Sign(delta.x) == (facingRight ? 1 : -1)) enemy.Hit();
            }
        }
        if (keyboard.enterKey.wasPressedThisFrame) hud.AdvanceDialogue();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
            if (contact.normal.y > .45f) grounded = true;
    }
    private void OnCollisionExit2D(Collision2D collision) => grounded = false;

    public void TakeDamage()
    {
        if (Time.time < invincibleUntil) return;
        invincibleUntil = Time.time + 1f;
        state.Damage(1);
        hud.ShowToast(state.Hearts > 0 ? "피격! 하트가 감소했습니다." : "쓰러졌습니다. 시작 지점으로 돌아갑니다.");
        if (state.Hearts == 0) { state.HealFull(); transform.position = new Vector2(-10f, -2.8f); }
    }
}

public sealed class PrototypeEnemy : MonoBehaviour
{
    private PrototypeGameState state;
    private PrototypeHudView hud;
    private int health;
    private float speed;
    private Rigidbody2D body;
    private float nextHit;

    public void Initialize(PrototypeGameState gameState, PrototypeHudView hudView, int startingHealth, float moveSpeed)
    { state = gameState; hud = hudView; health = startingHealth; speed = moveSpeed; }
    private void Awake() => body = GetComponent<Rigidbody2D>();
    private void FixedUpdate()
    {
        PrototypePlayer player = FindFirstObjectByType<PrototypePlayer>();
        if (player == null) return;
        float distance = player.transform.position.x - transform.position.x;
        if (Mathf.Abs(distance) < 5.5f) body.linearVelocity = new Vector2(Mathf.Sign(distance) * speed, body.linearVelocity.y);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        PrototypePlayer player = collision.gameObject.GetComponent<PrototypePlayer>();
        if (player != null && Time.time >= nextHit) { nextHit = Time.time + 1.05f; player.TakeDamage(); }
    }
    public void Hit()
    {
        health--;
        if (health <= 0) { hud.ShowToast(gameObject.name == "Demon King" ? "마왕을 물리쳤습니다! 마을에 평화가 찾아왔습니다." : "적을 처치했습니다."); Destroy(gameObject); }
        else hud.ShowToast($"{gameObject.name}에게 피해를 입혔습니다. ({health})");
    }
}

public sealed class PrototypePickup : MonoBehaviour
{
    public enum Kind { Cloak, Armor }
    private Kind kind;
    private PrototypeGameState state;
    private PrototypeHudView hud;
    public void Initialize(Kind pickupKind, PrototypeGameState gameState, PrototypeHudView hudView) { kind = pickupKind; state = gameState; hud = hudView; }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PrototypePlayer>() == null) return;
        if (kind == Kind.Cloak) { state.GainCloak(); hud.ShowDialogue("바람의 망토", "민첩성이 상승했습니다. 이동과 회피가 빨라집니다."); }
        else { state.GainArmor(); hud.ShowDialogue("철의 갑옷", "최대 하트가 증가하고 받는 피해가 감소합니다."); }
        Destroy(gameObject);
    }
}

public sealed class PrototypeStageGate : MonoBehaviour
{
    private PrototypeGameState state;
    private PrototypeHudView hud;
    public void Initialize(PrototypeGameState gameState, PrototypeHudView hudView) { state = gameState; hud = hudView; }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PrototypePlayer>() == null || state.Stage >= 2) return;
        state.ReachStageTwo();
        hud.ShowDialogue("성채의 관문", "스테이지 2가 연결되었습니다. 마왕이 당신을 기다립니다.");
    }
}

public sealed class PrototypeCameraFollow : MonoBehaviour
{
    private void LateUpdate()
    {
        PrototypePlayer player = FindFirstObjectByType<PrototypePlayer>();
        if (player == null) return;
        Vector3 target = transform.position;
        target.x = Mathf.Clamp(player.transform.position.x, -5f, 18f);
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 5f);
    }
}

public sealed class PrototypeHudView : MonoBehaviour
{
    private Text heartsText;
    private Text equipmentText;
    private Text stageText;
    private Text toastText;
    private Text speakerText;
    private Text dialogueText;
    private GameObject dialoguePanel;
    private float toastUntil;
    private bool dialogueVisible;

    public void Build()
    {
        heartsText = CreateText("Hearts", transform, new Vector2(.03f, .95f), new Vector2(.35f, .07f), 35, TextAnchor.MiddleLeft, Color.white);
        equipmentText = CreateText("Equipment", transform, new Vector2(.03f, .885f), new Vector2(.52f, .06f), 23, TextAnchor.MiddleLeft, new Color(.73f, .88f, 1f));
        stageText = CreateText("Stage", transform, new Vector2(.97f, .95f), new Vector2(.28f, .07f), 30, TextAnchor.MiddleRight, new Color(1f, .87f, .53f));
        toastText = CreateText("Toast", transform, new Vector2(.5f, .77f), new Vector2(.7f, .08f), 26, TextAnchor.MiddleCenter, Color.white);
        dialoguePanel = CreatePanel("DialoguePanel", transform, new Vector2(.5f, .17f), new Vector2(.84f, .24f), new Color(.035f, .055f, .10f, .93f));
        speakerText = CreateText("Speaker", dialoguePanel.transform, new Vector2(.06f, .78f), new Vector2(.82f, .23f), 28, TextAnchor.MiddleLeft, new Color(1f, .8f, .35f));
        dialogueText = CreateText("Dialogue", dialoguePanel.transform, new Vector2(.06f, .42f), new Vector2(.82f, .52f), 27, TextAnchor.MiddleLeft, Color.white);
    }

    public void Refresh(PrototypeGameState state)
    {
        heartsText.text = $"♥ {new string('♥', Mathf.Max(0, state.Hearts - 1))}  {state.Hearts}/{state.MaxHearts}";
        equipmentText.text = $"장비  망토: {(state.HasCloak ? "바람의 망토" : "없음")}  |  갑옷: {(state.HasArmor ? "철의 갑옷" : "없음")}";
        stageText.text = $"STAGE {state.Stage}  ·  Project XP";
    }

    public void ShowToast(string message) { toastText.text = message; toastUntil = Time.time + 2.4f; }
    public void ShowDialogue(string speaker, string message)
    {
        speakerText.text = speaker;
        dialogueText.text = message + "\n\n<color=#9BB8D8>[Enter] 계속</color>";
        dialoguePanel.SetActive(true);
        dialogueVisible = true;
    }
    public void AdvanceDialogue() { if (!dialogueVisible) return; dialogueVisible = false; dialoguePanel.SetActive(false); }
    private void Update() { if (Time.time > toastUntil) toastText.text = string.Empty; }

    private static GameObject CreatePanel(string objectName, Transform parent, Vector2 anchor, Vector2 size, Color color)
    {
        GameObject item = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        item.transform.SetParent(parent, false);
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.sizeDelta = new Vector2(size.x * 1920f, size.y * 1080f);
        item.GetComponent<Image>().color = color;
        return item;
    }

    private static Text CreateText(string objectName, Transform parent, Vector2 anchor, Vector2 size, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject item = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        item.transform.SetParent(parent, false);
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
        rect.sizeDelta = new Vector2(size.x * 1920f, size.y * 1080f);
        Text text = item.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }
}
