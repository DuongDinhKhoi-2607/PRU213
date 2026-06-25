using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("Inventory Settings")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.I;
    [SerializeField] private KeyCode _toggleKeyAlternative = KeyCode.B;

    private GameObject _inventoryPanel;
    private GameObject _bagButton;

    private TextMeshProUGUI _copperText;
    private TextMeshProUGUI _tinText;
    private TextMeshProUGUI _turtleShellText;
    private TextMeshProUGUI _bronzeIngotText;
    private TextMeshProUGUI _weaponText;

    private PlayerStats _stats;
    private PlayerMovement _playerMovement;
    private bool _isOpen = false;

    // Tự động khởi tạo ngay sau khi Scene được load mà không cần người dùng kéo thả script thủ công
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (FindAnyObjectByType<InventoryUI>() == null)
        {
            GameObject obj = new GameObject("Runtime_InventoryUI", typeof(InventoryUI));
            DontDestroyOnLoad(obj);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetupUI();
    }

    private void Update()
    {
        // Hỗ trợ cả phím I và phím B để mở/đóng túi đồ
        if (Input.GetKeyDown(_toggleKey) || Input.GetKeyDown(_toggleKeyAlternative))
        {
            ToggleInventory();
        }
    }

    public void SetupUI()
    {
        // 1. Tìm các đối tượng quản lý trong Scene
        UIManager uiManager = UIManager.Instance;
        if (uiManager == null) uiManager = FindAnyObjectByType<UIManager>();
        
        if (uiManager != null)
        {
            _stats = uiManager.playerStats;
            if (uiManager.playerCombat != null)
            {
                _playerMovement = uiManager.playerCombat.GetComponent<PlayerMovement>();
            }
        }

        if (_stats == null)
        {
            PlayerController pc = FindAnyObjectByType<PlayerController>();
            if (pc != null)
            {
                _stats = pc.stats;
                _playerMovement = pc.GetComponent<PlayerMovement>();
            }
            else
            {
                PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
                if (pm != null)
                {
                    _playerMovement = pm;
                    PlayerController controller = pm.GetComponent<PlayerController>();
                    if (controller != null) _stats = controller.stats;
                }
            }
        }

        Canvas canvas = null;
        if (uiManager != null) canvas = uiManager.mainCanvas;
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("[Túi Đồ] Không tìm thấy Canvas trong Scene hiện tại. Bỏ qua khởi tạo UI.");
            return;
        }

        GameObject hudPanel = null;
        if (uiManager != null) hudPanel = uiManager.HUDPanel;
        if (hudPanel == null) hudPanel = canvas.transform.Find("HUDPanel")?.gameObject;
        if (hudPanel == null) hudPanel = canvas.gameObject;

        // 2. Tạo Nút chiếc túi (Bag Button) trên HUD
        CreateBagButton(hudPanel);

        // 3. Tạo Bảng túi đồ (Inventory Panel)
        CreateInventoryPanel(canvas.gameObject);

        // Ẩn bảng túi đồ khi bắt đầu
        if (_inventoryPanel != null) _inventoryPanel.SetActive(false);
    }

    private void CreateBagButton(GameObject parent)
    {
        Transform existingButton = parent.transform.Find("BagButton");
        if (existingButton != null)
        {
            _bagButton = existingButton.gameObject;
            Button btn = _bagButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ToggleInventory);
            }
            return;
        }

        _bagButton = new GameObject("BagButton", typeof(RectTransform));
        _bagButton.transform.SetParent(parent.transform, false);

        RectTransform rect = _bagButton.GetComponent<RectTransform>();
        // Neo ở góc dưới bên phải màn hình
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-150f, 25f); // Cách lề phải 150px, lề dưới 25px
        rect.sizeDelta = new Vector2(65f, 65f);

        // Nền nút thiết kế kính mờ sang trọng
        Image img = _bagButton.AddComponent<Image>();
        img.color = new Color(0.12f, 0.12f, 0.16f, 0.9f);
        
        // Bo viền vàng hoàng kim
        Outline outline = _bagButton.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.84f, 0f, 0.8f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        // Icon hiển thị trên nút
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(_bagButton.transform, false);
        TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = "<size=22>🎒</size>\nTÚI ĐỒ";
        txt.fontSize = 11f;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        Button button = _bagButton.AddComponent<Button>();
        button.onClick.AddListener(ToggleInventory);
        
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.12f, 0.12f, 0.16f, 0.9f);
        colors.highlightedColor = new Color(0.22f, 0.22f, 0.3f, 0.95f);
        colors.pressedColor = new Color(0.08f, 0.08f, 0.1f, 1f);
        button.colors = colors;
    }

    private void CreateInventoryPanel(GameObject canvasObj)
    {
        Transform existingPanel = canvasObj.transform.Find("CustomInventoryPanel");
        if (existingPanel != null)
        {
            _inventoryPanel = existingPanel.gameObject;
            AssignPanelTexts();
            return;
        }

        // Tạo Panel chính
        _inventoryPanel = new GameObject("CustomInventoryPanel", typeof(RectTransform));
        _inventoryPanel.transform.SetParent(canvasObj.transform, false);

        RectTransform rect = _inventoryPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(380f, 300f);

        // Nền tối mờ cao cấp
        Image bgImg = _inventoryPanel.AddComponent<Image>();
        bgImg.color = new Color(0.07f, 0.07f, 0.09f, 0.95f);

        Outline panelOutline = _inventoryPanel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(1f, 0.84f, 0f, 0.8f);
        panelOutline.effectDistance = new Vector2(2f, -2f);

        // Tiêu đề
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
        titleObj.transform.SetParent(_inventoryPanel.transform, false);
        TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "🎒 HÀNH TRANG LOA THÀNH";
        titleTxt.fontSize = 18f;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = new Color(1f, 0.84f, 0f, 1f);

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -15f);
        titleRect.sizeDelta = new Vector2(0f, 35f);

        // Khung nội dung
        GameObject contentObj = new GameObject("Content", typeof(RectTransform));
        contentObj.transform.SetParent(_inventoryPanel.transform, false);
        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = new Vector2(0f, -15f);
        contentRect.sizeDelta = new Vector2(-40f, -80f);

        VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        // Tạo các dòng hiển thị tài nguyên
        _copperText = CreateInventoryRow(contentObj, "🔸 Đồng (Copper):", "0");
        _tinText = CreateInventoryRow(contentObj, "⚪ Thiếc (Tin):", "0");
        _turtleShellText = CreateInventoryRow(contentObj, "🐢 Mai Linh Quy (Shell):", "0");
        _bronzeIngotText = CreateInventoryRow(contentObj, "⭐ Thỏi Đồng Thau (Bronze):", "0");
        _weaponText = CreateInventoryRow(contentObj, "⚔️ Vũ khí hiện tại:", "Không có");

        // Nút Đóng
        GameObject closeBtnObj = new GameObject("CloseButton", typeof(RectTransform));
        closeBtnObj.transform.SetParent(_inventoryPanel.transform, false);
        RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0f, 15f);
        closeRect.sizeDelta = new Vector2(110f, 32f);

        Image closeImg = closeBtnObj.AddComponent<Image>();
        closeImg.color = new Color(0.65f, 0.15f, 0.15f, 0.9f);
        Outline closeOutline = closeBtnObj.AddComponent<Outline>();
        closeOutline.effectColor = Color.white;
        closeOutline.effectDistance = new Vector2(1f, -1f);

        GameObject closeTextObj = new GameObject("Text", typeof(RectTransform));
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        TextMeshProUGUI closeTxt = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeTxt.text = "ĐÓNG";
        closeTxt.fontSize = 12f;
        closeTxt.fontStyle = FontStyles.Bold;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color = Color.white;

        RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;

        Button closeBtn = closeBtnObj.AddComponent<Button>();
        closeBtn.onClick.AddListener(ToggleInventory);

        ColorBlock closeBtnColors = closeBtn.colors;
        closeBtnColors.normalColor = new Color(0.65f, 0.15f, 0.15f, 0.9f);
        closeBtnColors.highlightedColor = new Color(0.8f, 0.2f, 0.2f, 0.95f);
        closeBtnColors.pressedColor = new Color(0.45f, 0.1f, 0.1f, 1f);
        closeBtn.colors = closeBtnColors;

        // Tự động liên kết panel này vào UIManager
        UIManager uiManager = UIManager.Instance;
        if (uiManager == null) uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null && uiManager.inventoryPanel == null)
        {
            uiManager.inventoryPanel = _inventoryPanel;
        }
    }

    private TextMeshProUGUI CreateInventoryRow(GameObject parent, string label, string defaultValue)
    {
        GameObject row = new GameObject("Row_" + label.Replace(":", "").Trim(), typeof(RectTransform));
        row.transform.SetParent(parent.transform, false);

        HorizontalLayoutGroup hor = row.AddComponent<HorizontalLayoutGroup>();
        hor.childControlWidth = true;
        hor.childControlHeight = true;
        hor.childForceExpandWidth = false;
        hor.childForceExpandHeight = false;

        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(row.transform, false);
        TextMeshProUGUI labelTxt = labelObj.AddComponent<TextMeshProUGUI>();
        labelTxt.text = label;
        labelTxt.fontSize = 14f;
        labelTxt.alignment = TextAlignmentOptions.Left;
        labelTxt.color = new Color(0.8f, 0.8f, 0.85f, 1f);

        GameObject valObj = new GameObject("Value", typeof(RectTransform));
        valObj.transform.SetParent(row.transform, false);
        TextMeshProUGUI valTxt = valObj.AddComponent<TextMeshProUGUI>();
        valTxt.text = defaultValue;
        valTxt.fontSize = 14f;
        valTxt.fontStyle = FontStyles.Bold;
        valTxt.alignment = TextAlignmentOptions.Right;
        valTxt.color = Color.white;

        return valTxt;
    }

    private void AssignPanelTexts()
    {
        Transform content = _inventoryPanel.transform.Find("Content");
        if (content != null)
        {
            _copperText = content.Find("Row_Row_🔸 Đồng (Copper)")?.Find("Value")?.GetComponent<TextMeshProUGUI>();
            _tinText = content.Find("Row_Row_⚪ Thiếc (Tin)")?.Find("Value")?.GetComponent<TextMeshProUGUI>();
            _turtleShellText = content.Find("Row_Row_🐢 Mai Linh Quy (Shell)")?.Find("Value")?.GetComponent<TextMeshProUGUI>();
            _bronzeIngotText = content.Find("Row_Row_⭐ Thỏi Đồng Thau (Bronze)")?.Find("Value")?.GetComponent<TextMeshProUGUI>();
            _weaponText = content.Find("Row_Row_⚔️ Vũ khí hiện tại")?.Find("Value")?.GetComponent<TextMeshProUGUI>();
        }
    }

    public void ToggleInventory()
    {
        _isOpen = !_isOpen;
        if (_inventoryPanel != null)
        {
            _inventoryPanel.SetActive(_isOpen);
            if (_isOpen)
            {
                UpdateInventoryDisplay();
                // Hiện chuột khi mở túi đồ
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // Khóa chuột lại khi đóng túi đồ để tiếp tục chơi
                if (UIManager.Instance == null || UIManager.Instance.pausePanel == null || !UIManager.Instance.pausePanel.activeSelf)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    }

    public void UpdateInventoryDisplay()
    {
        if (_stats != null)
        {
            if (_copperText != null) _copperText.text = _stats.copperCount.ToString();
            if (_tinText != null) _tinText.text = _stats.tinCount.ToString();
            if (_turtleShellText != null) _turtleShellText.text = _stats.turtleShell.ToString();
            if (_bronzeIngotText != null) _bronzeIngotText.text = _stats.bronzeIngot.ToString();
        }

        if (_playerMovement != null)
        {
            if (_weaponText != null)
            {
                _weaponText.text = _playerMovement.HasSword ? $"<color=#FFD700>{_playerMovement.EquippedSwordName}</color>" : "<color=#aaaaaa>Tay không (Chưa có)</color>";
            }
        }
        else
        {
            PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
            if (pm != null)
            {
                _playerMovement = pm;
                if (_weaponText != null)
                {
                    _weaponText.text = _playerMovement.HasSword ? $"<color=#FFD700>{_playerMovement.EquippedSwordName}</color>" : "<color=#aaaaaa>Tay không (Chưa có)</color>";
                }
            }
        }
    }
}
