using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    //UI canvas that holds the buttons
    public Transform buildingMenu;
    public GameObject buildingButtonPrefab;//button prefab, to be set-up via code for all available buildings
    private BuildingPlacer _buildingPlacer;
    private Dictionary<string, Button> _buildingButtons;

    //UI canvas that displays resources
    public Transform resourcesUIParent;
    public GameObject gameResourceDisplayPrefab;
    public GameObject gameResourceCostPrefab;
    private Dictionary<string, Text> _resourceTexts;

    //UI element that displays info on mouse over
    public GameObject infoPanel;
    private Text _infoPanelTitleText;
    private Text _infoPanelDescriptionText;
    private Transform _infoPanelResourcesCostParent;
    public Color invalidTextColor;

    //UI element that displays selected unit name, level, production, actions, etc...
    public Transform selectedUnitsListParent;
    public GameObject selectedUnitDisplayPrefab;
    public Transform selectionGroupsParent;
    public GameObject selectedUnitMenu;
    private RectTransform _selectedUnitContentRectTransform;
    private RectTransform _selectedUnitButtonsRectTransform;
    private Text _selectedUnitTitleText;
    private Text _selectedUnitLevelText;
    private Transform _selectedUnitResourcesProductionParent;
    private Transform _selectedUnitActionButtonsParent;
    private Unit _selectedUnit;
    public GameObject unitSkillButtonPrefab;

    //UI element that displays pause menu
    public GameObject gameSettingsPanel;

    private void Awake()
    {
        _buildingPlacer = GetComponent<BuildingPlacer>();

        // create buttons for each building type
        _buildingButtons = new Dictionary<string, Button>();
        for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
        {
            BuildingData data = Globals.BUILDING_DATA[i];
            GameObject button = Instantiate(buildingButtonPrefab, buildingMenu);
            Button b = button.GetComponent<Button>(); 
            button.name = data.unitName;
            button.transform.Find("Text").GetComponent<Text>().text = data.unitName;
            _buildingButtons[data.code] = b;
            if (!Globals.BUILDING_DATA[i].CanBuy())
            {
                b.interactable = false;
            }
            button.GetComponent<BuildingButton>().Initialize(Globals.BUILDING_DATA[i]);
            _AddBuildingButtonListener(b, i);
        }

        // create texts for each in-game resource (gold, wood, stone...)
        _resourceTexts = new Dictionary<string, Text>();
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
        {
            GameObject display = Instantiate(gameResourceDisplayPrefab, resourcesUIParent);
            display.name = pair.Key;
            display.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{display.name}");
            _resourceTexts[pair.Key] = display.transform.Find("Text").GetComponent<Text>();
            _SetResourceText(pair.Key, pair.Value.Amount);
        }

        //initializes info panel
        Transform infoPanelTransform = infoPanel.transform;
        _infoPanelTitleText = infoPanelTransform
            .Find("Content/Title").GetComponent<Text>();
        _infoPanelDescriptionText = infoPanelTransform
            .Find("Content/Description").GetComponent<Text>();
        _infoPanelResourcesCostParent = infoPanelTransform
            .Find("Content/ResourcesCost");
        ShowInfoPanel(false);

        // hide all selection group buttons
        for (int i = 1; i <= 9; i++)
            ToggleSelectionGroupButton(i, false);


        Transform selectedUnitMenuTransform = selectedUnitMenu.transform;
        _selectedUnitContentRectTransform = selectedUnitMenuTransform.Find("Content").GetComponent<RectTransform>();
        _selectedUnitButtonsRectTransform = selectedUnitMenuTransform.Find("Buttons").GetComponent<RectTransform>();
        _selectedUnitTitleText = selectedUnitMenuTransform.Find("Content/Title").GetComponent<Text>();
        _selectedUnitLevelText = selectedUnitMenuTransform.Find("Content/Level").GetComponent<Text>();
        _selectedUnitResourcesProductionParent = selectedUnitMenuTransform.Find("Content/ResourcesProduction");
        _selectedUnitActionButtonsParent = selectedUnitMenuTransform.Find("Buttons/SpecificActions");

        _ShowSelectedUnitMenu(false);
        gameSettingsPanel.SetActive(false);
    }

    public void ToggleGameSettingsPanel()
    {
        bool showGameSettingsPanel = !gameSettingsPanel.activeSelf;
        gameSettingsPanel.SetActive(showGameSettingsPanel);
        EventManager.TriggerEvent(showGameSettingsPanel ? "PauseGame" : "ResumeGame");
    }
    //activates element when control group is defined
    public void ToggleSelectionGroupButton(int groupIndex, bool on)
    {
        selectionGroupsParent.Find(groupIndex.ToString()).gameObject.SetActive(on);
    }

    private void _SetResourceText(string resource, int value)
    {
        _resourceTexts[resource].text = value.ToString();
    }

    public void UpdateResourceTexts()
    {
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
        {
            _SetResourceText(pair.Key, pair.Value.Amount);
        }
    }
    private void _AddBuildingButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _buildingPlacer.SelectPlacedBuilding(i));
    }
    public void CheckBuildingButtons()//updates button interactability based on whether the building can be built or not
    {
        foreach (BuildingData data in Globals.BUILDING_DATA)
        {
            _buildingButtons[data.code].interactable = data.CanBuy();
        }
    }
    private void OnEnable()
    {
        EventManager.AddListener("UpdateResourceTexts", _OnUpdateResourceTexts);
        EventManager.AddListener("CheckBuildingButtons", _OnCheckBuildingButtons);
        EventManager.AddListener("HoverBuildingButton", _OnHoverBuildingButton);
        EventManager.AddListener("UnhoverBuildingButton", _OnUnhoverBuildingButton);
        EventManager.AddListener("SelectUnit", _OnSelectUnit);
        EventManager.AddListener("DeselectUnit", _OnDeselectUnit);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateResourceTexts", _OnUpdateResourceTexts);
        EventManager.RemoveListener("CheckBuildingButtons", _OnCheckBuildingButtons);
        EventManager.RemoveListener("HoverBuildingButton", _OnHoverBuildingButton);
        EventManager.RemoveListener("UnhoverBuildingButton", _OnUnhoverBuildingButton);
        EventManager.RemoveListener("SelectUnit", _OnSelectUnit);
        EventManager.RemoveListener("DeselectUnit", _OnDeselectUnit);
    }

    private void _OnSelectUnit(object data)
    {
        Unit unit = (Unit)data;
        _AddSelectedUnitToUIList(unit);
        _SetSelectedUnitMenu(unit);
        _ShowSelectedUnitMenu(true);

    }

    private void _OnDeselectUnit(object data)
    {
        Unit unit = (Unit)data;
        _RemoveSelectedUnitFromUIList(unit.Code);
        if (Globals.SELECTED_UNITS.Count == 0)
            _ShowSelectedUnitMenu(false);
        else
            _SetSelectedUnitMenu(Globals.SELECTED_UNITS[Globals.SELECTED_UNITS.Count - 1].Unit);
    }

    private void _SetSelectedUnitMenu(Unit unit)
    {
        _selectedUnit = unit;
        // adapt content panel heights to match info to display
        int contentHeight = 60 + unit.Production.Count * 16;
        _selectedUnitContentRectTransform.sizeDelta = new Vector2(64, contentHeight);
        _selectedUnitButtonsRectTransform.anchoredPosition = new Vector2(0, -contentHeight - 20);
        _selectedUnitButtonsRectTransform.sizeDelta = new Vector2(70, Screen.height - contentHeight - 20);
        // update texts
        _selectedUnitTitleText.text = unit.Data.unitName;
        _selectedUnitLevelText.text = $"Level {unit.Level}";
        // clear resource production and reinstantiate new one
        foreach (Transform child in _selectedUnitResourcesProductionParent) Destroy(child.gameObject);
        foreach (Transform child in _selectedUnitActionButtonsParent) Destroy(child.gameObject);

        if (unit.SkillManagers.Count > 0)
        {
            GameObject g; Transform t; Button b;
            for (int i = 0; i < unit.SkillManagers.Count; i++)
            {
                g = GameObject.Instantiate(
                    unitSkillButtonPrefab, _selectedUnitActionButtonsParent);
                t = g.transform;
                b = g.GetComponent<Button>();
                unit.SkillManagers[i].SetButton(b);
                t.Find("Text").GetComponent<Text>().text =
                    unit.SkillManagers[i].skill.skillName;
                _AddUnitSkillButtonListener(b, i);
            }
        }
        if (unit.Production.Count > 0)
        {
            GameObject g;
            Transform t;
            foreach (ResourceValue resource in unit.Production)
            {
                g = GameObject.Instantiate(gameResourceCostPrefab, _selectedUnitResourcesProductionParent);
                t = g.transform;
                t.Find("Text").GetComponent<Text>().text = $"+{resource.amount}";
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{resource.code}");
                
            }

        }

        //--------------------------------------------------------
        _selectedUnit = unit;
        // clear skills and reinstantiate new ones
        if (_selectedUnitActionButtonsParent.childCount > 0)
        {
            foreach (Transform child in _selectedUnitActionButtonsParent)
                Destroy(child.gameObject);
        }
        if (unit.SkillManagers.Count > 0)
        {
            GameObject g;
            Transform t;
            Button b;
            for (int i = 0; i < unit.SkillManagers.Count; i++)
            {
                g = GameObject.Instantiate(unitSkillButtonPrefab, _selectedUnitActionButtonsParent);
                t = g.transform;
                b = g.GetComponent<Button>();
                unit.SkillManagers[i].SetButton(b);
                t.Find("Text").GetComponent<Text>().text = unit.SkillManagers[i].skill.skillName;
                _AddUnitSkillButtonListener(b, i);
            }
        }

    }
    private void _AddUnitSkillButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _selectedUnit.TriggerSkill(i));
    }

    private void _ShowSelectedUnitMenu(bool show)
    {
        selectedUnitMenu.SetActive(show);
        buildingMenu.gameObject.SetActive(!show);
    }

    private void _OnHoverBuildingButton(object data)
    {
        SetInfoPanel((UnitData)data);
        ShowInfoPanel(true);
    }

    private void _OnUnhoverBuildingButton()
    {
        ShowInfoPanel(false);
    }
    // ...

    private void _OnUpdateResourceTexts()
    {
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
            _SetResourceText(pair.Key, pair.Value.Amount);
    }

    private void _OnCheckBuildingButtons()
    {
        foreach (BuildingData data in Globals.BUILDING_DATA)
            _buildingButtons[data.code].interactable = data.CanBuy();
    }

    public void SetInfoPanel(UnitData data)
    {
        // update texts
        if (data.unitName != "")
            _infoPanelTitleText.text = data.unitName;
        if (data.description != "")
            _infoPanelDescriptionText.text = data.description;

        // clear resource costs and reinstantiate new ones
        foreach (Transform child in _infoPanelResourcesCostParent)
            Destroy(child.gameObject);
        
        

        if (data.cost.Count > 0)
        {
            GameObject g; Transform t;
            foreach (ResourceValue resource in data.cost)
            {
                g = GameObject.Instantiate(gameResourceCostPrefab, _infoPanelResourcesCostParent);
                t = g.transform;
                t.Find("Text").GetComponent<Text>().text = resource.amount.ToString();
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{resource.code}");
                // ...
                // check to see if resource requirement is not
                // currently met - in that case, turn the text into the "invalid"
                // color
                if (Globals.GAME_RESOURCES[resource.code].Amount < resource.amount)
                    t.Find("Text").GetComponent<Text>().color = invalidTextColor;
            }
        }
    }

    public void _AddSelectedUnitToUIList(Unit unit)
    {
        // if there is another unit of the same type already selected,
        Transform alreadyInstantiatedChild = selectedUnitsListParent.Find(unit.Code);
        if (alreadyInstantiatedChild != null)
        {
            Text t = alreadyInstantiatedChild.Find("Count").GetComponent<Text>();
            int count = int.Parse(t.text);
            t.text = (count + 1).ToString();// increase the counter
        }
        
        else
        {
            // else create a brand new counter initialized with a count of 1
            GameObject g = GameObject.Instantiate(selectedUnitDisplayPrefab, selectedUnitsListParent);
            g.name = unit.Code;
            Transform t = g.transform;
            t.Find("Count").GetComponent<Text>().text = "1";
            t.Find("Name").GetComponent<Text>().text = unit.Data.unitName;
        }
    }

    public void _RemoveSelectedUnitFromUIList(string code)
    {
        Transform listItem = selectedUnitsListParent.Find(code);
        if (listItem == null) return;

        Text t = listItem.Find("Count").GetComponent<Text>();
        int count = int.Parse(t.text);
        count -= 1;

        if (count == 0)
            DestroyImmediate(listItem.gameObject);
        else
            t.text = count.ToString();
    }

    public void ShowInfoPanel(bool show)
    {
        infoPanel.SetActive(show);
    }
}